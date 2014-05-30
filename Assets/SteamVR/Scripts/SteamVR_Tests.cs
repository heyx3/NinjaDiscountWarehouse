//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Simple set of tests to exercise each interface
//
//=============================================================================

using UnityEngine;

public class SteamVR_Tests : MonoBehaviour
{
	void Start()
	{
		Debug.Log("Testing SteamVR...");

		var hmd = SteamVR.IHmd.instance;
		if (hmd == null)
			return;

		int x = 0, y = 0;
		uint w = 0, h = 0;

		hmd.GetWindowBounds(ref x, ref y, ref w, ref h);
		Debug.Log(string.Format("GetWindowBounds: {0} {1} {2} {3}", x, y, w, h));

		hmd.GetRecommendedRenderTargetSize(ref w, ref h);
		Debug.Log(string.Format("GetRecommendedRenderTargetSize: {0} {1}", w, h));

		uint X = 0, Y = 0;
		hmd.GetEyeOutputViewport(SteamVR.Hmd_Eye.Eye_Left, ref X, ref Y, ref w, ref h);
		Debug.Log(string.Format("GetEyeOutputViewport:L {0} {1} {2} {3}", X, Y, w, h));

		hmd.GetEyeOutputViewport(SteamVR.Hmd_Eye.Eye_Right, ref X, ref Y, ref w, ref h);
		Debug.Log(string.Format("GetEyeOutputViewport:R {0} {1} {2} {3}", X, Y, w, h));

		var m = hmd.GetProjectionMatrix(SteamVR.Hmd_Eye.Eye_Left, 1.0f, 1000.0f, SteamVR.GraphicsAPIConvention.API_DirectX);
		Debug.Log(string.Format("GetProjectionMatrix: {0}", m));

		float left = 0.0f, right = 0.0f, top = 0.0f, bottom = 0.0f;
		hmd.GetProjectionRaw(SteamVR.Hmd_Eye.Eye_Left, ref left, ref right, ref top, ref bottom);
		Debug.Log(string.Format("GetProjectionRaw: {0} {1} {2} {3}", left, right, top, bottom));

		var coords = hmd.ComputeDistortion(SteamVR.Hmd_Eye.Eye_Left, 0.5f, 0.5f);
		Debug.Log(string.Format("ComputeDistortion: {0}", coords));

		var eye = hmd.GetHeadFromEyePose(SteamVR.Hmd_Eye.Eye_Left);
		Debug.Log(string.Format("GetHeadFromEyePose: {0}", eye));

		var result = SteamVR.HmdTrackingResult.TrackingResult_Uninitialized;
		SteamVR.HmdMatrix44_t mL = new SteamVR.HmdMatrix44_t(), mR = new SteamVR.HmdMatrix44_t();
		if (hmd.GetViewMatrix(0.0f, ref mL, ref mR, ref result))
			Debug.Log(string.Format("GetViewMatrix: {0} {1}", mL, mR));
		else
			Debug.Log(string.Format("GetViewMatrix: {0}", result));

		var adapter = hmd.GetD3D9AdapterIndex();
		Debug.Log("GetD3D9AdapterIndex: " + adapter);

		var pose = new SteamVR.HmdMatrix34_t();
		if (hmd.GetTrackerFromHeadPose(0.0f, ref pose, ref result))
			Debug.Log("GetTrackerFromHeadPose: " + pose);
		else
			Debug.Log("GetTrackerFromHeadPose: " + result);

		if (hmd.WillDriftInYaw())
			Debug.Log("WillDriftInYaw:yes");
		else
			Debug.Log("WillDriftInYaw:no");

		hmd.ZeroTracker();

		var zero = hmd.GetTrackerZeroPose();
		Debug.Log("GetTrackerZeroPose: " + zero);

		var driverId = hmd.GetDriverId();
		Debug.Log("DriverId: " + driverId);

		var displayId = hmd.GetDisplayId();
		Debug.Log("DisplayId: " + displayId);

		var version = SteamVR.IHmd_Version();
		Debug.Log("IHmd_Version: " + version);
	}
}

