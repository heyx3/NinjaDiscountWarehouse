//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Helper to display various hmd stats via GUIText
//
//=============================================================================

using UnityEngine;

public class SteamVR_Stats : MonoBehaviour
{
	public SteamVR_Menu menu;
	public GUIText text;

	void Awake()
	{
		if (menu == null)
		{
			menu = Object.FindObjectOfType<SteamVR_Menu>();
		}

		if (text == null)
		{
			text = GetComponent<GUIText>();
		}
	}

	float lastUpdate = 0.0f;

	void Update()
	{
		if (text != null)
		{
			if (Input.GetKeyDown(KeyCode.I))
			{
				text.enabled = !text.enabled;
			}

			if (text.enabled)
			{
				var framerate = (lastUpdate > 0.0f) ? 1.0f / (Time.realtimeSinceStartup - lastUpdate) : 0.0f;
				lastUpdate = Time.realtimeSinceStartup;
				text.text = string.Format("framerate: {0:N0}", framerate);
				if (menu != null)
				{
					text.text += string.Format("\nscale: {0:N2}", menu.scale);
				}
			}
		}
	}
}

