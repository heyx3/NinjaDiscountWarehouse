//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Direct C# wrapper for native SteamVR interface
//
//=============================================================================

using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

public class SteamVR
{
	// right-handed system
	// +y is up
	// +x is to the right
	// -z is going away from you
	// Distance unit is  meters
	[StructLayout(LayoutKind.Sequential)]
	public struct HmdMatrix34_t
	{
		/// float[12]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12, ArraySubType = UnmanagedType.R4)]
		public float[] m;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HmdMatrix44_t
	{
		/// float[16]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.R4)]
		public float[] m;
	}

	/** Used to return the post-distortion UVs for each color channel. 
	* UVs range from 0 to 1 with 0,0 in the upper left corner of the 
	* source render target. The 0,0 to 1,1 range covers a single eye. */
	[StructLayout(LayoutKind.Sequential)]
	public struct DistortionCoordinates_t
	{
		/// float[2]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.R4)]
		public float[] rfRed;

		/// float[2]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.R4)]
		public float[] rfGreen;

		/// float[2]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.R4)]
		public float[] rfBlue;
	}

	public enum Hmd_Eye
	{
		Eye_Left = 0,
		Eye_Right = 1,
	}

	public enum GraphicsAPIConvention
	{
		API_DirectX = 0, // Normalized Z goes from 0 at the viewer to 1 at the far clip plane
		API_OpenGL = 1,  // Normalized Z goes from 1 at the viewer to -1 at the far clip plane
	}

	public enum HmdTrackingResult
	{
		TrackingResult_Uninitialized			= 1,

		TrackingResult_Calibrating_InProgress	= 100,
		TrackingResult_Calibrating_OutOfRange	= 101,

		TrackingResult_Running_OK				= 200,
		TrackingResult_Running_OutOfRange		= 201,
	}

	/// Return Type: void
	///pHmd: IHmd->void*
	///pnX: int32_t*
	///pnY: int32_t*
	///pnWidth: uint32_t*
	///pnHeight: uint32_t*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetWindowBounds")]
	public static extern void GetWindowBounds(System.IntPtr pHmd, ref int pnX, ref int pnY, ref uint pnWidth, ref uint pnHeight);

	/// Return Type: void
	///pHmd: IHmd->void*
	///pnWidth: uint32_t*
	///pnHeight: uint32_t*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetRecommendedRenderTargetSize")]
	public static extern void GetRecommendedRenderTargetSize(System.IntPtr pHmd, ref uint pnWidth, ref uint pnHeight);

	/// Return Type: void
	///pHmd: IHmd->void*
	///eEye: Hmd_Eye
	///pnX: uint32_t*
	///pnY: uint32_t*
	///pnWidth: uint32_t*
	///pnHeight: uint32_t*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetEyeOutputViewport")]
	public static extern void GetEyeOutputViewport(System.IntPtr pHmd, Hmd_Eye eEye, ref uint pnX, ref uint pnY, ref uint pnWidth, ref uint pnHeight);

	/// Return Type: HmdMatrix44_t
	///pHmd: IHmd->void*
	///eEye: Hmd_Eye
	///fNearZ: float
	///fFarZ: float
	///eProjType: GraphicsAPIConvention
	[DllImport("steam_api", EntryPoint = "SteamVR_GetProjectionMatrix")]
	public static extern HmdMatrix44_t GetProjectionMatrix(System.IntPtr pHmd, Hmd_Eye eEye, float fNearZ, float fFarZ, GraphicsAPIConvention eProjType);

	/// Return Type: void
	///pHmd: IHmd->void*
	///eEye: Hmd_Eye
	///pfLeft: float*
	///pfRight: float*
	///pfTop: float*
	///pfBottom: float*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetProjectionRaw")]
	public static extern void GetProjectionRaw(System.IntPtr pHmd, Hmd_Eye eEye, ref float pfLeft, ref float pfRight, ref float pfTop, ref float pfBottom);

	/// Return Type: DistortionCoordinates_t
	///pHmd: IHmd->void*
	///eEye: Hmd_Eye
	///fU: float
	///fV: float
	[DllImport("steam_api", EntryPoint = "SteamVR_ComputeDistortion")]
	public static extern DistortionCoordinates_t ComputeDistortion(System.IntPtr pHmd, Hmd_Eye eEye, float fU, float fV);

	/// Return Type: HmdMatrix34_t
	///pHmd: IHmd->void*
	///eEye: Hmd_Eye
	[DllImport("steam_api", EntryPoint = "SteamVR_GetHeadFromEyePose")]
	public static extern HmdMatrix34_t GetHeadFromEyePose(System.IntPtr pHmd, Hmd_Eye eEye);

	/// Return Type: boolean
	///pHmd: IHmd->void*
	///fSecondsFromNow: float
	///pMatLeftView: HmdMatrix44_t*
	///pMatRightView: HmdMatrix44_t*
	///peResult: HmdTrackingResult*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetViewMatrix")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool GetViewMatrix(System.IntPtr pHmd, float fSecondsFromNow, ref HmdMatrix44_t pMatLeftView, ref HmdMatrix44_t pMatRightView, ref HmdTrackingResult peResult);

	/// Return Type: int32_t->int
	///pHmd: IHmd->void*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetD3D9AdapterIndex")]
	public static extern int GetD3D9AdapterIndex(System.IntPtr pHmd);

	/// Return Type: boolean
	///pHmd: IHmd->void*
	///fPredictedSecondsFromNow: float
	///pmPose: HmdMatrix34_t*
	///peResult: HmdTrackingResult*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetTrackerFromHeadPose")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool GetTrackerFromHeadPose(System.IntPtr pHmd, float fPredictedSecondsFromNow, ref HmdMatrix34_t pmPose, ref HmdTrackingResult peResult);

	/// Return Type: boolean
	///pHmd: IHmd->void*
	///pmPose: HmdMatrix34_t*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetLastTrackerFromHeadPose")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool GetLastTrackerFromHeadPose(System.IntPtr pHmd, ref HmdMatrix34_t pmPose);

	/// Return Type: boolean
	///pHmd: IHmd->void*
	[DllImport("steam_api", EntryPoint = "SteamVR_WillDriftInYaw")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool WillDriftInYaw(System.IntPtr pHmd);

	/// Return Type: void
	///pHmd: IHmd->void*
	[DllImport("steam_api", EntryPoint = "SteamVR_ZeroTracker")]
	public static extern void ZeroTracker(System.IntPtr pHmd);

	/// Return Type: HmdMatrix34_t*
	///pHmd: IHmd->void*
	[DllImport("steam_api", EntryPoint = "SteamVR_GetTrackerZeroPose")]
	public static extern HmdMatrix34_t GetTrackerZeroPose(System.IntPtr pHmd);

	/// Return Type: uint32_t->unsigned int
	///pHmd: IHmd->void*
	///pchBuffer: char*
	///unBufferLen: uint32_t->unsigned int
	[DllImport("steam_api", EntryPoint = "SteamVR_GetDriverId")]
	public static extern uint GetDriverId(System.IntPtr pHmd, System.Text.StringBuilder pchBuffer, uint unBufferLen);

	/// Return Type: uint32_t->unsigned int
	///pHmd: IHmd->void*
	///pchBuffer: char*
	///unBufferLen: uint32_t->unsigned int
	[DllImport("steam_api", EntryPoint = "SteamVR_GetDisplayId")]
	public static extern uint GetDisplayId(System.IntPtr pHmd, System.Text.StringBuilder pchBuffer, uint unBufferLen);

	public class IHmd
	{
		private static IHmd _instance;
		public static IHmd instance
		{
			get
			{
				if (_instance == null)
				{
					var error = SteamVR.HmdError.HmdError_None;
					var pNativeObject = SteamVR.Init(ref error);
					if (error != SteamVR.HmdError.HmdError_None)
						Debug.Log(error);
					if (pNativeObject != System.IntPtr.Zero)
						_instance = new IHmd(pNativeObject);
				}
				return _instance;
			}
		}

		~IHmd()
		{
			if (m_pNativeObject != System.IntPtr.Zero)
			{
				m_pNativeObject = System.IntPtr.Zero;
				SteamVR.Shutdown();
			}
		}

		// Variable to hold the C++ class's this pointer
		private System.IntPtr m_pNativeObject = System.IntPtr.Zero;

		private IHmd(System.IntPtr pNativeObject)
		{
			m_pNativeObject = pNativeObject;
		}

		// ------------------------------------
		// Display Methods
		// ------------------------------------

		/** Size and position that the window needs to be on the VR display. */
		public void GetWindowBounds(ref int pnX, ref int pnY, ref uint pnWidth, ref uint pnHeight)
		{
			SteamVR.GetWindowBounds(m_pNativeObject, ref pnX, ref pnY, ref pnWidth, ref pnHeight);
		}

		/** Suggested size for the intermediate render target that the distortion pulls from. */
		public void GetRecommendedRenderTargetSize(ref uint pnWidth, ref uint pnHeight)
		{
			SteamVR.GetRecommendedRenderTargetSize(m_pNativeObject, ref pnWidth, ref pnHeight);
		}

		/** Gets the viewport in the frame buffer to draw the output of the distortion into */
		public void GetEyeOutputViewport(Hmd_Eye eEye, ref uint pnX, ref uint pnY, ref uint pnWidth, ref uint pnHeight)
		{
			SteamVR.GetEyeOutputViewport(m_pNativeObject, eEye, ref pnX, ref pnY, ref pnWidth, ref pnHeight);
		}

		/** The projection matrix for the specified eye */
		public HmdMatrix44_t GetProjectionMatrix(Hmd_Eye eEye, float fNearZ, float fFarZ, GraphicsAPIConvention eProjType)
		{
			return SteamVR.GetProjectionMatrix(m_pNativeObject, eEye, fNearZ, fFarZ, eProjType);
		}

		/** The components necessary to build your own projection matrix in case your
		* application is doing something fancy like infinite Z */
		public void GetProjectionRaw(Hmd_Eye eEye, ref float pfLeft, ref float pfRight, ref float pfTop, ref float pfBottom)
		{
			SteamVR.GetProjectionRaw(m_pNativeObject, eEye, ref pfLeft, ref pfRight, ref pfTop, ref pfBottom);
		}

		/** Returns the result of the distortion function for the specified eye and input UVs. UVs go from 0,0 in 
		* the upper left of that eye's viewport and 1,1 in the lower right of that eye's viewport. */
		public DistortionCoordinates_t ComputeDistortion(Hmd_Eye eEye, float fU, float fV)
		{
			return SteamVR.ComputeDistortion(m_pNativeObject, eEye, fU, fV);
		}

		/** Returns the transform from eye space to the head space. Eye space is the per-eye flavor of head
		* space that provides stereo disparity. Instead of Model * View * Projection the sequence is Model * View * Eye^-1 * Projection. 
		* Normally View and Eye^-1 will be multiplied together and treated as View in your application. 
		*/
		public HmdMatrix34_t GetHeadFromEyePose(Hmd_Eye eEye)
		{
			return SteamVR.GetHeadFromEyePose(m_pNativeObject, eEye);
		}

		/** For use in simple VR apps, this method returns the concatenation of the 
		* tracking pose and the eye matrix to get a full view matrix for each eye.
		* This is ( GetHeadFromEyePose() ) * (GetTrackerFromHeadPose() ^ -1 )  */
		public bool GetViewMatrix(float fSecondsFromNow, ref HmdMatrix44_t pMatLeftView, ref HmdMatrix44_t pMatRightView, ref HmdTrackingResult peResult)
		{
			return SteamVR.GetViewMatrix(m_pNativeObject, fSecondsFromNow, ref pMatLeftView, ref pMatRightView, ref peResult);
		}

		/** [D3D9 Only]
		* Returns the adapter index that the user should pass into CreateDevice to set up D3D9 in such
		* a way that it can go full screen exclusive on the HMD. Returns -1 if there was an error.
		*/
		public int GetD3D9AdapterIndex()
		{
			return SteamVR.GetD3D9AdapterIndex(m_pNativeObject);
		}

		// ------------------------------------
		// Tracking Methods
		// ------------------------------------

		/** The pose that the tracker thinks that the HMD will be in at the specified
		* number of seconds into the future. Pass 0 to get the current state. 
		*
		* This is roughly analogous to the inverse of the view matrix in most applications, though 
		* many games will need to do some additional rotation or translation on top of the rotation
		* and translation provided by the head pose.
		*
		* If this function returns true the pose has been populated with a pose that can be used by the application.
		* Check peResult for details about the pose, including messages that should be displayed to the user.
		*/
		public bool GetTrackerFromHeadPose(float fPredictedSecondsFromNow, ref HmdMatrix34_t pmPose, ref HmdTrackingResult peResult)
		{
			return SteamVR.GetTrackerFromHeadPose(m_pNativeObject, fPredictedSecondsFromNow, ref pmPose, ref peResult);
		}

		/** Passes back the pose matrix from the last successful call to GetTrackerFromHeadPose(). Returns true if that matrix is 
		* valid (because there has been a previous successful pose.) */
		public bool GetLastTrackerFromHeadPose(ref HmdMatrix34_t pmPose)
		{
			return SteamVR.GetLastTrackerFromHeadPose(m_pNativeObject, ref pmPose);
		}

		/** Returns true if the tracker for this HMD will drift the Yaw component of its pose over time regardless of
		* actual head motion. This is true for gyro-based trackers with no ground truth. */
		public bool WillDriftInYaw()
		{
			return SteamVR.WillDriftInYaw(m_pNativeObject);
		}

		/** Sets the zero pose for the tracker coordinate system. After this call all WorldFromHead poses will be relative 
		* to the pose whenever this was called. The new zero coordinate system will not change the fact that the Y axis is
		* up in the real world, so the next pose returned from GetWorldFromHeadPose after a call to ZeroTracker may not be
		* exactly an identity matrix. */
		public void ZeroTracker()
		{
			SteamVR.ZeroTracker(m_pNativeObject);
		}

		/** Returns the zero pose for the tracker coordinate system. If the tracker has never had a valid pose, this
		* will be an identity matrix. */
		public HmdMatrix34_t GetTrackerZeroPose()
		{
			return SteamVR.GetTrackerZeroPose(m_pNativeObject);
		}

		// ------------------------------------
		// Administrative methods
		// ------------------------------------

		/** The ID of the driver this HMD uses as a UTF-8 string. Returns the length of the ID in bytes. If 
		* the buffer is not large enough to fit the ID an empty string will be returned. In general, 128 bytes
		* will be enough to fit any ID. */
		public string GetDriverId()
		{
			var capacity = SteamVR.GetDriverId(m_pNativeObject, null, 0);
			var result = new System.Text.StringBuilder((int)capacity);
			SteamVR.GetDriverId(m_pNativeObject, result, capacity);
			return result.ToString();
		}

		/** The ID of this display within its driver this HMD uses as a UTF-8 string. Returns the length of the ID in bytes. If 
		* the buffer is not large enough to fit the ID an empty string will be returned. In general, 128 bytes
		* will be enough to fit any ID. */
		public string GetDisplayId()
		{
			var capacity = SteamVR.GetDisplayId(m_pNativeObject, null, 0);
			var result = new System.Text.StringBuilder((int)capacity);
			SteamVR.GetDisplayId(m_pNativeObject, result, capacity);
			return result.ToString();
		}
	}

	[DllImport("steam_api")]
	private static extern System.IntPtr SteamVR_IHmd_Version();
	public static string IHmd_Version()
		{ return Marshal.PtrToStringAnsi(SteamVR_IHmd_Version()); }

	/** error codes returned by Vr_Init */
	public enum HmdError
	{
		HmdError_None = 0,

		HmdError_Init_InstallationNotFound = 100,
		HmdError_Init_InstallationCorrupt = 101,
		HmdError_Init_VRClientDLLNotFound = 102,
		HmdError_Init_FileNotFound = 103,
		HmdError_Init_FactoryNotFound = 104,
		HmdError_Init_InterfaceNotFound = 105,
		HmdError_Init_InvalidInterface = 106,
		HmdError_Init_UserConfigDirectoryInvalid = 107,
		HmdError_Init_HmdNotFound = 108,
		HmdError_Init_NotInitialized = 109,

		HmdError_Driver_Failed = 200,

		HmdError_IPC_ServerInitFailed = 300,
		HmdError_IPC_ConnectFailed = 301,
		HmdError_IPC_SharedStateInitFailed = 302,
	}

	/// Return Type: IHmd*
	///peError: HmdError*
	[DllImport("steam_api", EntryPoint = "VR_Init")]
	public static extern System.IntPtr Init(ref HmdError peError);

	/// Return Type: void
	[DllImport("steam_api", EntryPoint = "VR_Shutdown")]
	public static extern void Shutdown();
}

