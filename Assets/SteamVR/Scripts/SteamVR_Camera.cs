//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Adds SteamVR render support to existing camera objects
//
//=============================================================================
#define HI_QUALITY

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SteamVR_Camera : MonoBehaviour
{
	SteamVR_CameraEye[] eyes;

	public Transform offset { get; private set; }

	public bool applyDistortion = true;
	public bool antialiasing = true;
	public bool positionGameWindow = false;
	public bool disableTracking = false;
	public bool wireframe = false;

	static bool calibrating, trackingOutOfRange; // status
	static int renderedFrameCount = -1;

	[System.Serializable]
	public class OverlaySettings
	{
		public Texture texture;
		public bool curved = true;
		public float scale = 3.0f;			// size of overlay view
		public float distance = 1.25f;		// distance from surface
		public float alpha = 1.0f;			// opacity 0..1

		public Vector4 uvOffset = new Vector4(0, 0, 1, 1);

		[HideInInspector] public float radius;
	}

	public OverlaySettings overlaySettings;

	public Component[] renderComponents;
	public Transform[] followHead; // (child) objects to attach to offset
	public Transform[] followEyes; // (child) objects to attach to each eye

	static public RenderTexture sceneTexture, viewportTexture;
	static public Material distortMaterial, blitMaterial;

	public void SetViewPlanes(float fNearZ, float fFarZ)
	{
		var hmd = SteamVR.IHmd.instance;
		if (hmd == null)
			return;

		var convention = SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") ?
			SteamVR.GraphicsAPIConvention.API_OpenGL : SteamVR.GraphicsAPIConvention.API_DirectX;

		foreach (var eye in eyes)
		{
			var proj = hmd.GetProjectionMatrix(eye.eye, fNearZ, fFarZ, convention);
			var m = new Matrix4x4();
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
					m[i, j] = proj.m[i * 4 + j];
			eye.camera.projectionMatrix = m;
		}
	}

	public Ray GetRay()
	{
		return new Ray(offset.position, offset.forward);
	}

	private void SetCalibrating(params object[] args)
	{
		calibrating = (bool)args[0];
	}

	private void SetTrackingOutOfRange(params object[] args)
	{
		trackingOutOfRange = !(bool)args[0];
	}

	static bool DestroyOnEnable = false;

	void OnEnable()
	{
		// Static guard to prevent infinite recursion when duplicating
		if (DestroyOnEnable)
		{
			Object.DestroyImmediate(this);
			return;
		}

		SteamVR_Utils.Event.Listen("calibrating", SetCalibrating);
		SteamVR_Utils.Event.Listen("absolute_tracking", SetTrackingOutOfRange);

		// First make sure an hmd is connected.
		var hmd = SteamVR.IHmd.instance;
		if (hmd == null)
		{
			enabled = false;
			return;
		}

		// Create an offset to use for positional tracking
		if (offset == null)
		{
			offset = new GameObject("offset").transform;
			offset.position = transform.position;
			offset.rotation = transform.rotation;
		}

		// Transfer AudioListener if necessary
		var listener = GetComponent<AudioListener>();
		if (listener != null)
		{
			Object.DestroyImmediate(listener);
			offset.gameObject.AddComponent<AudioListener>();
		}

		// Strip unsupported components
		var guiLayer = GetComponent<GUILayer>();
		if (guiLayer != null)
			Object.DestroyImmediate(guiLayer);
		var flareLayer = GetComponent("FlareLayer");
		if (flareLayer != null)
			Object.DestroyImmediate(flareLayer);

		int x = 0, y = 0;
		uint viewportWidth = 0, viewportHeight = 0;
		hmd.GetWindowBounds(ref x, ref y, ref viewportWidth, ref viewportHeight);

#if UNITY_EDITOR
		if (positionGameWindow && (x | y) != 0)
		{
			var type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			var method = type.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			var view = method.Invoke(null, null);
			if (view != null)
			{
				int osBorderWidth = (Application.platform == RuntimePlatform.OSXEditor) ? 0 : 5;

				// Size of the toolbar above the game view, excluding the OS border.
				const int tabHeight = 22;

				var size = new Vector2(viewportWidth, viewportHeight + tabHeight - osBorderWidth);
				var pos = new Rect(x, y - tabHeight, size.x, size.y);

				// TODO: Force "Free Aspect" setting
				type.GetProperty("minSize").SetValue(view, size, null);
				type.GetProperty("maxSize").SetValue(view, size, null);
				type.GetProperty("position").SetValue(view, pos, null);
				type.GetMethod("ShowPopup").Invoke(view, null);
			}
		}
#endif
		// Create offscreen render target shared by eyes cams
		if (sceneTexture == null)
		{
			uint w = 0, h = 0;
			hmd.GetRecommendedRenderTargetSize(ref w, ref h);
#if HI_QUALITY
			// MSAA only works with power-of-two render texture sizes.
			sceneTexture = new RenderTexture(Mathf.NextPowerOfTwo((int)w - 1), Mathf.NextPowerOfTwo((int)h - 1), 0);
			if (!SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
				sceneTexture.antiAliasing = 8; // 8x sampling
#else
			sceneTexture = new RenderTexture((int)w, (int)h, 0);
#endif
		}

		if (viewportTexture == null)
		{
			viewportTexture = new RenderTexture((int)viewportWidth, (int)viewportHeight, 0);
		}

		if (distortMaterial == null)
		{
			distortMaterial = new Material(Shader.Find("Custom/SteamVR_Distort"));
		}

		Screen.showCursor = false;

		// Gather or create our two eyes used for rendering
		eyes = transform.GetComponentsInChildren<SteamVR_CameraEye>();
		if (eyes == null || eyes.Length != 2)
		{
			var children = new System.Collections.Generic.List<Transform>();
			foreach (Transform child in transform)
			{
				bool followEyes = false;
				foreach (var t in this.followEyes)
				{
					if (t == child)
					{
						followEyes = true;
						break;
					}
				}

				if (followEyes)
					continue;

				child.parent = null;
				children.Add(child);
			}

			foreach (var t in this.followEyes)
				t.parent = transform;

			DestroyOnEnable = true;
			var go = Object.Instantiate(gameObject) as GameObject;
			DestroyOnEnable = false;

			foreach (Transform child in children)
			{
				child.parent = transform;
			}

			foreach (var t in this.followEyes)
			{
				Object.DestroyImmediate(t.gameObject);
			}

			this.followEyes = null;

			// Strip all components that are not listed as needed for rendering
			var components = go.GetComponents<Behaviour>();
			for (int i = components.Length - 1; i >= 0; --i)
				if (!IsRenderComponent(components[i]))
					Object.DestroyImmediate(components[i]);

			// Strip all render specific components now that they've been copied off
			components = GetComponents<Behaviour>();
			for (int i = components.Length - 1; i >= 0; --i)
			{
				if (components[i] == this || components[i] is Camera)
					continue;
				if (IsRenderComponent(components[i]))
					Object.DestroyImmediate(components[i]);
			}

			// Clear out now invalid references.
			renderComponents = null;

			eyes = new SteamVR_CameraEye[] { go.AddComponent<SteamVR_CameraEye>(),
				(Object.Instantiate(go) as GameObject).GetComponent<SteamVR_CameraEye>() };

			eyes[0].name = "eyeL";
			eyes[1].name = "eyeR";
		}

		offset.parent = transform;
		offset.localScale = Vector3.one;

		foreach (var child in followHead)
			child.parent = offset;

		var depth = eyes[0].camera.depth;

		eyes[0].Init(this, SteamVR.Hmd_Eye.Eye_Left, depth);
		eyes[1].Init(this, SteamVR.Hmd_Eye.Eye_Right, depth + 1000);

		// Continue using existing camera for OnPostRender callback below.
		camera.depth = depth + 2000;
		camera.clearFlags = CameraClearFlags.Nothing;
		camera.cullingMask = 0;
		camera.eventMask = 0;
		camera.orthographic = true;
		camera.orthographicSize = 1;
		camera.nearClipPlane = 0;
		camera.farClipPlane = 1;
		camera.useOcclusionCulling = false;
	}

	bool IsRenderComponent(Component test)
	{
		// always for cameras and image effects
		var type = test.GetType();
		if (type == typeof(Camera) || type.GetMethod("OnRenderImage",
			System.Reflection.BindingFlags.Public |
			System.Reflection.BindingFlags.NonPublic |
			System.Reflection.BindingFlags.Instance) != null)
			return true;

		// check if requires camera component
		var required = System.Attribute.GetCustomAttribute(type, typeof(RequireComponent)) as RequireComponent;
		if (required != null && (
			required.m_Type0 == typeof(Camera) ||
			required.m_Type1 == typeof(Camera) ||
			required.m_Type2 == typeof(Camera)))
			return true;

		// check explicit list
		foreach (var component in renderComponents)
			if (type == component.GetType())
				return true;

		// otherwise not a render component
		return false;
	}

	void OnDisable()
	{
		SteamVR_Utils.Event.Remove("calibrating", SetCalibrating);
		SteamVR_Utils.Event.Remove("absolute_tracking", SetTrackingOutOfRange);
	}

	public void UpdateTracking()
	{
		var head = SteamVR_Utils.RigidTransform.identity;
		if (!disableTracking)
		{
			var hmd = SteamVR.IHmd.instance;
			var pose = new SteamVR.HmdMatrix34_t();
			if (Time.renderedFrameCount != renderedFrameCount)
			{
				renderedFrameCount = Time.renderedFrameCount;

				var result = SteamVR.HmdTrackingResult.TrackingResult_Uninitialized;
				if (hmd.GetTrackerFromHeadPose(0.0f, ref pose, ref result))
					head = new SteamVR_Utils.RigidTransform(pose);

				var calibrating =
					result == SteamVR.HmdTrackingResult.TrackingResult_Calibrating_InProgress ||
					result == SteamVR.HmdTrackingResult.TrackingResult_Calibrating_OutOfRange;
				if (calibrating != SteamVR_Camera.calibrating)
				{
					SteamVR_Utils.Event.Send("calibrating", calibrating);
				}

				var trackingOutOfRange =
					result == SteamVR.HmdTrackingResult.TrackingResult_Running_OutOfRange ||
					result == SteamVR.HmdTrackingResult.TrackingResult_Calibrating_OutOfRange;
				if (trackingOutOfRange != SteamVR_Camera.trackingOutOfRange)
				{
					SteamVR_Utils.Event.Send("absolute_tracking", !trackingOutOfRange);
				}
			}
			else
			{
				if (hmd.GetLastTrackerFromHeadPose(ref pose))
					head = new SteamVR_Utils.RigidTransform(pose);
			}
		}

		offset.localPosition = head.pos;
		offset.localRotation = head.rot;

		// Update shared optional overlay shader variables
		if (overlaySettings.texture != null && applyDistortion)
		{
			distortMaterial.SetFloat("alpha", Mathf.Clamp01(overlaySettings.alpha));
			distortMaterial.SetVector("uvOffset", overlaySettings.uvOffset);

			var rot = Matrix4x4.identity;
			rot.SetColumn(0, head.rot * Vector3.right);
			rot.SetColumn(1, head.rot * Vector3.up);
			rot.SetColumn(2, head.rot * Vector3.forward);
			distortMaterial.SetMatrix("rot", rot);

			float aspect = (float)overlaySettings.texture.width / overlaySettings.texture.height;
			if (overlaySettings.curved)
			{
				var range = new Vector2(0.1f, 1.0f); // probably should make this tweakable
				var theta = Mathf.Lerp(60.0f, 5.0f, Mathf.SmoothStep(range.x, range.y, -offset.localPosition.z));
				var coef = new Vector4(2.0f * theta * Mathf.Deg2Rad, aspect / overlaySettings.scale, aspect, 2.0f * overlaySettings.distance);
				distortMaterial.SetVector("coef", coef);
				overlaySettings.radius = overlaySettings.scale / coef.x;
			}
			else
			{
				var coef = new Vector4(1.0f / overlaySettings.scale, aspect / overlaySettings.scale, 0, 0);
				distortMaterial.SetVector("coef", coef);
				overlaySettings.radius = 0.0f;
			}
		}
	}

	void Update()
	{
		// Ensure various settings to minimize latency.
		Application.targetFrameRate = Screen.currentResolution.refreshRate;
		Time.fixedDeltaTime = 1.0f / Application.targetFrameRate;
		QualitySettings.maxQueuedFrames = 0;
		QualitySettings.vSyncCount = 1;

		if (Input.GetKeyDown(KeyCode.Z))
		{
			var hmd = SteamVR.IHmd.instance;
			if (hmd != null)
			{
				hmd.ZeroTracker();
				SteamVR_Utils.Event.Send("zerotracker", true);
			}
		}

		camera.enabled = applyDistortion;
	}

	void OnPostRender()
	{
		if (!blitMaterial)
		{
			blitMaterial = new Material("Shader \"SteamVR_Viewport\" {" + "\n" +
				"Properties { _MainTex (\"Base (RGB)\", 2D) = \"white\" {} }" + "\n" +
				"SubShader { Pass {" + "\n" +
				"	ZTest Always Cull Off ZWrite Off Fog { Mode Off }" + "\n" +
				"	BindChannels { Bind \"vertex\", vertex Bind \"texcoord\", texcoord0 }" + "\n" +
				"	SetTexture [_MainTex] { combine texture }" + "\n" +
				"} } }" );
			blitMaterial.hideFlags = HideFlags.HideAndDontSave;
			blitMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			blitMaterial.mainTexture = viewportTexture;
		}

		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(GL.QUADS);
		blitMaterial.SetPass(0);
		GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0, 1, 0);
		GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1, 1, 0);
		GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1, 0, 0);
		GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0, 0, 0);
		GL.End();
		GL.PopMatrix();
	}
}

