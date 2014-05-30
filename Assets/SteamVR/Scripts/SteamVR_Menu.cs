//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Example menu using OnGUI with SteamVR_Camera's overlay support
//
//=============================================================================

using UnityEngine;

public class SteamVR_Menu : MonoBehaviour
{
	public Texture cursor, background, logo;
	public float logoHeight, menuOffset;

	public Vector2 scaleLimits = new Vector2(0.1f, 5.0f);
	public float scaleRate = 0.5f;

	SteamVR_Camera tracker;
	Camera overlayCam;
	Vector4 uvOffset;
	float distance;

	public RenderTexture texture { get; private set; }
	public float scale { get; private set; }

	string scaleLimitX, scaleLimitY, scaleRateText;

	void Awake()
	{
		FindTracker();

		scaleLimitX = string.Format("{0:N1}", scaleLimits.x);
		scaleLimitY = string.Format("{0:N1}", scaleLimits.y);
		scaleRateText = string.Format("{0:N1}", scaleRate);

		if (tracker != null)
		{
			uvOffset = tracker.overlaySettings.uvOffset;
			distance = tracker.overlaySettings.distance;
			scale = tracker.transform.localScale.x;
		}
		else
		{
			scale = 1.0f;
		}
	}

	void OnGUI()
	{
		if (texture == null)
			return;

		var prevActive = RenderTexture.active;
		RenderTexture.active = texture;

		if (Event.current.type == EventType.Repaint)
			GL.Clear(false, true, Color.clear);

		var area = new Rect(0, 0, texture.width, texture.height);

		// Account for screen smaller than texture (since mouse position gets clamped)
		if (Screen.width < texture.width)
		{
			area.width = Screen.width;
			tracker.overlaySettings.uvOffset.x = -(float)(texture.width - Screen.width) / (2 * texture.width);
		}
		if (Screen.height < texture.height)
		{
			area.height = Screen.height;
			tracker.overlaySettings.uvOffset.y = (float)(texture.height - Screen.height) / (2 * texture.height);
		}

		// Pull screen closer for Rift
		if (Screen.width <= 1280)
			tracker.overlaySettings.distance = 0.8f;

		GUILayout.BeginArea(area);

		if (background != null)
		{
			GUI.DrawTexture(new Rect(
				(area.width - background.width) / 2,
				(area.height - background.height) / 2,
				background.width, background.height), background);
		}

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();

		if (logo != null)
		{
			GUILayout.Space(area.height / 2 - logoHeight);
			GUILayout.Box(logo);
		}

		GUILayout.Space(menuOffset);

		if (GUILayout.Button("[Esc] - Close menu"))
			HideMenu();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("Scale: {0:N4}", scale));
		{
			var result = GUILayout.HorizontalSlider(scale, scaleLimits.x, scaleLimits.y);
			if (result != scale)
			{
				scale = result;
				tracker.transform.localScale = new Vector3(scale, scale, scale);
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("Scale limits:"));
		{
			var result = GUILayout.TextField(scaleLimitX);
			if (result != scaleLimitX)
			{
				if (float.TryParse(result, out scaleLimits.x))
					scaleLimitX = result;
			}
		}
		{
			var result = GUILayout.TextField(scaleLimitY);
			if (result != scaleLimitY)
			{
				if (float.TryParse(result, out scaleLimits.y))
					scaleLimitY = result;
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("Scale rate:"));
		{
			var result = GUILayout.TextField(scaleRateText);
			if (result != scaleRateText)
			{
				if (float.TryParse(result, out scaleRate))
					scaleRateText = result;
			}
		}
		GUILayout.EndHorizontal();

		tracker.overlaySettings.curved = GUILayout.Toggle(tracker.overlaySettings.curved, "Curved overlay");
		tracker.antialiasing = GUILayout.Toggle(tracker.antialiasing, "Antialiasing");
		tracker.wireframe = GUILayout.Toggle(tracker.wireframe, "Wireframe");

		var eyes = tracker.GetComponentsInChildren<SteamVR_CameraEye>();
		if (eyes != null && eyes.Length == 2)
		{
			bool skybox = eyes[0].clearFlags == CameraClearFlags.Skybox;
			bool result = GUILayout.Toggle(skybox, "Skybox");
			if (result != skybox)
			{
				foreach (var eye in eyes)
					eye.clearFlags = result ? CameraClearFlags.Skybox : CameraClearFlags.Color;
			}
		}

		if (GUILayout.Button("[Z]ero Tracker"))
		{
			var hmd = SteamVR.IHmd.instance;
			if (hmd != null)
				hmd.ZeroTracker();
		}

#if !UNITY_EDITOR
		if (GUILayout.Button("Exit"))
			Application.Quit();
#endif
		GUILayout.Space(menuOffset);

		var env = System.Environment.GetEnvironmentVariable("VR_OVERRIDE");
		if (env != null)
		{
			GUILayout.Label("VR_OVERRIDE=" + env);
		}

		GUILayout.Label("Graphics device: " + SystemInfo.graphicsDeviceVersion);

		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.EndArea();

		if (cursor != null)
		{
			float x = Input.mousePosition.x, y = Screen.height - Input.mousePosition.y;
			float w = cursor.width, h = cursor.height;
			GUI.DrawTexture(new Rect(x, y, w, h), cursor);
		}

		RenderTexture.active = prevActive;
	}

	void FindTracker()
	{
		foreach (var cam in Object.FindObjectsOfType(typeof(SteamVR_Camera)) as SteamVR_Camera[])
		{
			if (cam.applyDistortion)
			{
				tracker = cam;
				break;
			}
		}
	}

	public void ShowMenu()
	{
		FindTracker();

		if (tracker == null)
			return;

		texture = tracker.overlaySettings.texture as RenderTexture;
		if (texture == null)
		{
			Debug.LogError("Menu requires hmd have overlay render texture.");
			return;
		}

		uvOffset = tracker.overlaySettings.uvOffset;
		distance = tracker.overlaySettings.distance;

		// If an existing camera is rendering into the overlay texture, we need
		// to temporarily disable it to keep it from clearing the texture on us.
		var cameras = Object.FindObjectsOfType(typeof(Camera)) as Camera[];
		foreach (var cam in cameras)
		{
			if (cam.enabled && cam.targetTexture == texture)
			{
				overlayCam = cam;
				overlayCam.enabled = false;
				break;
			}
		}
	}

	public void HideMenu()
	{
		texture = null;
		if (overlayCam != null)
			overlayCam.enabled = true;
		if (tracker != null)
		{
			tracker.overlaySettings.uvOffset = uvOffset;
			tracker.overlaySettings.distance = distance;
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7))
		{
			if (texture == null)
			{
				ShowMenu();
			}
			else
			{
				HideMenu();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Home))
		{
			scale = 1.0f;
			tracker.transform.localScale = new Vector3(scale, scale, scale);
		}
		else if (Input.GetKey(KeyCode.PageUp))
		{
			scale = Mathf.Clamp(scale + scaleRate * Time.deltaTime, scaleLimits.x, scaleLimits.y);
			tracker.transform.localScale = new Vector3(scale, scale, scale);
		}
		else if (Input.GetKey(KeyCode.PageDown))
		{
			scale = Mathf.Clamp(scale - scaleRate * Time.deltaTime, scaleLimits.x, scaleLimits.y);
			tracker.transform.localScale = new Vector3(scale, scale, scale);
		}
	}
}

