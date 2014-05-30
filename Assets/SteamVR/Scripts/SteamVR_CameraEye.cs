//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Child objects for SteamVR_Camera to handle rendering of each eye
//
//=============================================================================

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SteamVR_CameraEye : MonoBehaviour
{
	public SteamVR_Camera tracker;
	public SteamVR.Hmd_Eye eye;

	public CameraClearFlags clearFlags;

	struct OverlaySettings
	{
		public Matrix4x4 invProj;
		public Vector3[] kernel;
	}

	OverlaySettings overlaySettings;

	static Mesh[] distortMesh = new Mesh[2];

	public void Init(SteamVR_Camera tracker, SteamVR.Hmd_Eye eye, float depth)
	{
		this.tracker = tracker;
		this.eye = eye;

		var hmd = SteamVR.IHmd.instance;
		var i = (int)eye;

		uint x = 0, y = 0, w = 0, h = 0;
		hmd.GetEyeOutputViewport(eye, ref x, ref y, ref w, ref h);

		overlaySettings.kernel = new Vector3[] { // AA sub-pixel sampling (2x2 RGSS)
			new Vector3( 0.125f / w, -0.375f / h, 0),
			new Vector3( 0.375f / w,  0.125f / h, 0),
			new Vector3(-0.125f / w,  0.375f / h, 0),
			new Vector3(-0.375f / w, -0.125f / h, 0)};

		float left = 0.0f, right = 0.0f, top = 0.0f, bottom = 0.0f;
		hmd.GetProjectionRaw(eye, ref left, ref right, ref top, ref bottom);

		var camera = GetComponent<Camera>();
		float zFar = camera.farClipPlane;
		float zNear = camera.nearClipPlane;

		var m = Matrix4x4.identity;

		float idx = 1.0f / (right - left);
		float idy = 1.0f / (bottom - top);
		float idz = 1.0f / (zFar - zNear);
		float sx = right + left;
		float sy = bottom + top;

		m[0, 0] = 2*idx; m[0, 1] = 0;     m[0, 2] = sx*idx;    m[0, 3] = 0;
		m[1, 0] = 0;     m[1, 1] = 2*idy; m[1, 2] = sy*idy;    m[1, 3] = 0;
		m[2, 0] = 0;     m[2, 1] = 0;     m[2, 2] = -zFar*idz; m[2, 3] = -zFar*zNear*idz;
		m[3, 0] = 0;     m[3, 1] = 0;     m[3, 2] = -1.0f;     m[3, 3] = 0;

		camera.projectionMatrix	= m;
		camera.depth			= depth;		// enforce rendering order
		camera.eventMask		= 0;			// disable mouse events
		camera.orthographic		= false;		// force perspective
		camera.aspect			= (float)w / h;

		// Use fov of projection matrix (which is grown to account for non-linear undistort)
		camera.fieldOfView = (Mathf.Atan(bottom) - Mathf.Atan(top)) * Mathf.Rad2Deg;

		// Copy and clear clearFlags to avoid duplicate work.
		if ((int)clearFlags == 0)
		{
			clearFlags = camera.clearFlags;
			camera.clearFlags = CameraClearFlags.Nothing;
		}

		camera.targetTexture = SteamVR_Camera.sceneTexture;

		overlaySettings.invProj = m.inverse;

		transform.parent = tracker.offset;
		transform.localScale = Vector3.one;

		var t = new SteamVR_Utils.RigidTransform(hmd.GetHeadFromEyePose(eye));
		transform.localPosition = t.pos;
		transform.localRotation = t.rot;

		if (distortMesh[i] == null)
		{
			var viewportWidth = SteamVR_Camera.viewportTexture.width;
			var viewportHeight = SteamVR_Camera.viewportTexture.height;

			var eyeViewport = new Rect(
				2.0f * x / viewportWidth - 1.0f,
				2.0f * y / viewportHeight - 1.0f,
				2.0f * w / viewportWidth,
				2.0f * h / viewportHeight);

			distortMesh[i] = SteamVR_Utils.CreateDistortMesh(hmd, eye, 32, 32, eyeViewport);
		}
	}

	void OnPreCull()
	{
		if (eye == SteamVR.Hmd_Eye.Eye_Left)
			tracker.UpdateTracking();
	}

	static bool pendingBlit; // for chaining across cameras (since Unity doesn't handle this properly)

	void OnPreRender()
	{
		if (pendingBlit)
			Graphics.Blit(SteamVR_Camera.sceneTexture, RenderTexture.active);

		if (tracker.wireframe)
			GL.wireframe = true;

		if (clearFlags == CameraClearFlags.Skybox)
		{
			var material = GetSkyboxMaterial();
			if (material != null)
			{
				SteamVR_Utils.DrawSkybox(material, tracker.offset.rotation, camera.projectionMatrix);
			}
			else
			{
				GL.Clear(true, true, Color.black);
				GL.PushMatrix(); GL.PopMatrix(); // Necessary when setting clearFlags to Nothing for some reason.
			}
		}
		else
		{
			var clearColor = clearFlags == CameraClearFlags.Color;
			var clearDepth = clearFlags != CameraClearFlags.Nothing;
			GL.Clear(clearDepth, clearColor, camera.backgroundColor);
			GL.PushMatrix(); GL.PopMatrix(); // Necessary when setting clearFlags to Nothing for some reason.
		}
	}

	public Material GetSkyboxMaterial()
	{
		foreach (var childSkybox in transform.GetComponentsInChildren<Skybox>())
			if (childSkybox.enabled)
				return childSkybox.material;

		var trackerSkybox = tracker.GetComponent<Skybox>();
		if (trackerSkybox && trackerSkybox.enabled)
			return trackerSkybox.material;

		return RenderSettings.skybox;
	}

	void OnPostRender()
	{
		if (tracker.wireframe)
			GL.wireframe = false;
	}

	// This component is added after copying over the rest so it should always get called last.
	// This makes it safe to apply our distortion pass to the tracker's viewport texture.
	// Waiting for the tracker's camera to apply distortion would prevent us from sharing
	// the larger offscreen render target across eye cameras.  Passing null as the dest
	// to blit directly to the backbuffer, would avoid the need for a separate viewportTexture,
	// however, Unity's lighting does not play well with that technique, unfortunately.
	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (tracker.applyDistortion)
		{
			ApplyDistortionCorrection(src, SteamVR_Camera.viewportTexture);
			pendingBlit = false;
		}
		else
		{
			Graphics.Blit(src, SteamVR_Camera.sceneTexture);
			pendingBlit = true;
		}
	}

	protected void ApplyDistortionCorrection(RenderTexture src, RenderTexture dest)
	{
		int pass = 4;
		var mesh = distortMesh[(int)eye];
		var material = SteamVR_Camera.distortMaterial;
		material.mainTexture = src;

		Graphics.SetRenderTarget(dest);
		if (tracker.overlaySettings.texture != null)
		{
			pass = tracker.overlaySettings.curved ? 0 : 2;

			var r = tracker.overlaySettings.radius;
			var cam = SteamVR_Utils.RigidTransform.FromLocal(tracker.offset) * transform.localPosition;
			var P0 = new Vector4(cam.x, cam.y, cam.z, r * r);

			// Pull back taking variable radius into account so we are always ZDist away from projection surface.
			P0.z += r - tracker.overlaySettings.distance;

			material.SetTexture("_Overlay", tracker.overlaySettings.texture);
			material.SetMatrix("invProj", overlaySettings.invProj);
			material.SetVector("P0", P0);
		}

		if (tracker.antialiasing)
		{
			float totalWeight = 0.0f;
			foreach (var offset in overlaySettings.kernel)
			{
				totalWeight += 1.0f;
				material.SetFloat("weight", 1.0f / totalWeight);
				material.SetPass(pass);
				Graphics.DrawMeshNow(mesh, offset, Quaternion.identity);
			}
		}
		else
		{
			material.SetFloat("weight", 1.0f);
			material.SetPass(pass + 1);
			Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
		}
	}
}

