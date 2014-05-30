//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Example for using Steam Controller
//
//=============================================================================

using UnityEngine;
using System.Runtime.InteropServices;

public class SteamVR_TestController : MonoBehaviour
{
	[DllImport("steam_api", EntryPoint = "SteamAPI_InitSafe")]
	public static extern bool InitSteamAPI();

	[DllImport("steam_api", EntryPoint = "SteamAPI_Shutdown")]
	public static extern void ShutdownSteamAPI();

	[DllImport("steam_api", EntryPoint = "SteamAPI_RunCallbacks")]
	public static extern void RunCallbacks();

	[DllImport("steam_api", EntryPoint = "SteamController_Init")]
	public static extern bool InitController(string pchAbsolutePathToControllerConfigVDF);

	[DllImport("steam_api", EntryPoint = "SteamController_Shutdown")]
	public static extern void ShutdownController();

	public enum ESteamControllerPad
	{
		k_ESteamControllerPad_Left,
		k_ESteamControllerPad_Right,
	}

	public const ulong STEAM_RIGHT_TRIGGER_MASK           = 0x0000000000000001UL;
	public const ulong STEAM_LEFT_TRIGGER_MASK            = 0x0000000000000002UL;
	public const ulong STEAM_RIGHT_BUMPER_MASK            = 0x0000000000000004UL;
	public const ulong STEAM_LEFT_BUMPER_MASK             = 0x0000000000000008UL;
	public const ulong STEAM_BUTTON_0_MASK                = 0x0000000000000010UL;
	public const ulong STEAM_BUTTON_1_MASK                = 0x0000000000000020UL;
	public const ulong STEAM_BUTTON_2_MASK                = 0x0000000000000040UL;
	public const ulong STEAM_BUTTON_3_MASK                = 0x0000000000000080UL;
	public const ulong STEAM_TOUCH_0_MASK                 = 0x0000000000000100UL;
	public const ulong STEAM_TOUCH_1_MASK                 = 0x0000000000000200UL;
	public const ulong STEAM_TOUCH_2_MASK                 = 0x0000000000000400UL;
	public const ulong STEAM_TOUCH_3_MASK                 = 0x0000000000000800UL;
	public const ulong STEAM_BUTTON_MENU_MASK             = 0x0000000000001000UL;
	public const ulong STEAM_BUTTON_STEAM_MASK            = 0x0000000000002000UL;
	public const ulong STEAM_BUTTON_ESCAPE_MASK           = 0x0000000000004000UL;
	public const ulong STEAM_BUTTON_BACK_LEFT_MASK        = 0x0000000000008000UL;
	public const ulong STEAM_BUTTON_BACK_RIGHT_MASK       = 0x0000000000010000UL;
	public const ulong STEAM_BUTTON_LEFTPAD_CLICKED_MASK  = 0x0000000000020000UL;
	public const ulong STEAM_BUTTON_RIGHTPAD_CLICKED_MASK = 0x0000000000040000UL;
	public const ulong STEAM_LEFTPAD_FINGERDOWN_MASK      = 0x0000000000080000UL;
	public const ulong STEAM_RIGHTPAD_FINGERDOWN_MASK     = 0x0000000000100000UL;

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SteamControllerState_t
	{
		/// uint32->unsigned int
		public uint unPacketNum;

		/// uint64->unsigned __int64
		public ulong ulButtons;

		/// short
		public short sLeftPadX;

		/// short
		public short sLeftPadY;

		/// short
		public short sRightPadX;

		/// short
		public short sRightPadY;
	}

	/// Return Type: boolean
	///unControllerIndex: uint32->unsigned int
	///pState: SteamControllerState_t*
	[DllImport("steam_api", EntryPoint = "SteamController_GetControllerState")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool GetControllerState(uint unControllerIndex, ref SteamControllerState_t pState);

	/// Return Type: void
	///unControllerIndex: uint32->unsigned int
	///eTargetPad: ESteamControllerPad
	///usDurationMicroSec: unsigned short
	[DllImport("steam_api", EntryPoint = "SteamController_TriggerHapticPulse")]
	public static extern void TriggerHapticPulse(uint unControllerIndex, ESteamControllerPad eTargetPad, ushort usDurationMicroSec);

	/// Return Type: void
	///pchMode: char*
	[DllImport("steam_api", EntryPoint = "SteamController_SetOverrideMode")]
	public static extern void SetOverrideMode([In] [MarshalAs(UnmanagedType.LPStr)] string pchMode);

	bool SteamAPI_Initialized = false;
	bool Controller_Initialized = false;

	void OnEnable()
	{
		SteamAPI_Initialized = InitSteamAPI();
		if (SteamAPI_Initialized)
			Controller_Initialized = InitController(Application.dataPath + "/controller.vdf");

		if (!SteamAPI_Initialized)
			Debug.LogError("Failed to initialize SteamAPI!");
		else if (!Controller_Initialized)
			Debug.LogError("Failed to initialize SteamController!");
	}

	void OnDisable()
	{
		if (Controller_Initialized)
			ShutdownController();
		if (SteamAPI_Initialized)
			ShutdownSteamAPI();
	}

	SteamControllerState_t state = new SteamControllerState_t();

	void Update()
	{
		if (Controller_Initialized)
		{
			RunCallbacks();

			if (GetControllerState(0, ref state))
			{
				Debug.Log(string.Format("PacketNum: {0} Buttons: {1} LeftPad: {2},{3} RightPad: {4},{5}",
					state.unPacketNum, state.ulButtons, state.sLeftPadX, state.sLeftPadY, state.sRightPadX, state.sRightPadY));

				if ((state.ulButtons & STEAM_RIGHT_TRIGGER_MASK          ) != 0) Debug.Log("RIGHT_TRIGGER");
				if ((state.ulButtons & STEAM_RIGHT_TRIGGER_MASK          ) != 0) Debug.Log("RIGHT_TRIGGER");
				if ((state.ulButtons & STEAM_LEFT_TRIGGER_MASK           ) != 0) Debug.Log("LEFT_TRIGGER");
				if ((state.ulButtons & STEAM_RIGHT_BUMPER_MASK           ) != 0) Debug.Log("RIGHT_BUMPER");
				if ((state.ulButtons & STEAM_LEFT_BUMPER_MASK            ) != 0) Debug.Log("LEFT_BUMPER");
				if ((state.ulButtons & STEAM_BUTTON_0_MASK               ) != 0) Debug.Log("BUTTON_0");
				if ((state.ulButtons & STEAM_BUTTON_1_MASK               ) != 0) Debug.Log("BUTTON_1");
				if ((state.ulButtons & STEAM_BUTTON_2_MASK               ) != 0) Debug.Log("BUTTON_2");
				if ((state.ulButtons & STEAM_BUTTON_3_MASK               ) != 0) Debug.Log("BUTTON_3");
				if ((state.ulButtons & STEAM_TOUCH_0_MASK                ) != 0) Debug.Log("TOUCH_0");
				if ((state.ulButtons & STEAM_TOUCH_1_MASK                ) != 0) Debug.Log("TOUCH_1");
				if ((state.ulButtons & STEAM_TOUCH_2_MASK                ) != 0) Debug.Log("TOUCH_2");
				if ((state.ulButtons & STEAM_TOUCH_3_MASK                ) != 0) Debug.Log("TOUCH_3");
				if ((state.ulButtons & STEAM_BUTTON_MENU_MASK            ) != 0) Debug.Log("BUTTON_MENU");
				if ((state.ulButtons & STEAM_BUTTON_STEAM_MASK           ) != 0) Debug.Log("BUTTON_STEAM");
				if ((state.ulButtons & STEAM_BUTTON_ESCAPE_MASK          ) != 0) Debug.Log("BUTTON_ESCAPE");
				if ((state.ulButtons & STEAM_BUTTON_BACK_LEFT_MASK       ) != 0) Debug.Log("BUTTON_BACK_LEFT");
				if ((state.ulButtons & STEAM_BUTTON_BACK_RIGHT_MASK      ) != 0) Debug.Log("BUTTON_BACK_RIGHT");
				if ((state.ulButtons & STEAM_BUTTON_LEFTPAD_CLICKED_MASK ) != 0) Debug.Log("BUTTON_LEFTPAD_CLICKED");
				if ((state.ulButtons & STEAM_BUTTON_RIGHTPAD_CLICKED_MASK) != 0) Debug.Log("BUTTON_RIGHTPAD_CLICKED");
				if ((state.ulButtons & STEAM_LEFTPAD_FINGERDOWN_MASK     ) != 0) Debug.Log("LEFTPAD_FINGERDOWN");
				if ((state.ulButtons & STEAM_RIGHTPAD_FINGERDOWN_MASK    ) != 0) Debug.Log("RIGHTPAD_FINGERDOWN");

				if ((state.ulButtons & STEAM_LEFTPAD_FINGERDOWN_MASK) != 0)
					TriggerHapticPulse(0, ESteamControllerPad.k_ESteamControllerPad_Left, 100);
				if ((state.ulButtons & STEAM_RIGHTPAD_FINGERDOWN_MASK) != 0)
					TriggerHapticPulse(0, ESteamControllerPad.k_ESteamControllerPad_Right, 100);

			}
		}
	}
}

