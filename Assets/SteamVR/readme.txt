SteamVR plugin for Unity
Copyright 2014, Valve Corporation, All rights reserved.

This plugin is part of the Steamworks SDK, and can be downloaded from https://partner.steamgames.com/


Quickstart:

To use, simply add the SteamVR_Camera script to your Main Camera object.  Everything else gets set up at
runtime.

If you haven't already, you will need to install SteamVR via Steam.  It can be found under Tools.


Requirements:

* The included reference implementation uses Unity features which require a Unity Pro license.

* A head-mounted display (hmd) e.g. an Oculus Rift.

* On OSX and Linux, Steam must be running in the background for SteamVR to operate.


Files:

Assets/SteamVR/Scripts/SteamVR.cs - This direct wrapper for Steamworks SDK's VR support mirrors SteamVR.h and  
is the only script required.  It exposes all functionality provided by SteamVR.  It is not recommended you make  
changes to this file.  It should be kept in sync with the associated Steamworks dlls.

The remaining files found in Assets/SteamVR/Scripts are provided as a reference implementation, and to get you  
up and running quickly and easily.  You are encouraged to modify these to suit your project's unique needs,  
and provide feedback at http://steamcommunity.com/app/250820

Assets/SteamVR/Scenes/example.unity - A sample scene demonstrating the functionality provided by this plugin.   
This also shows you how to set up a separate camera for rendering gui elements, and handle events to display  
hmd status.


Details:

Assets/SteamVR/Scripts/SteamVR_Camera.cs - Adds VR support to your existing camera object.

To combat stretching incurred by distortion correction, we render scenes at a higher resolution off-screen.
Since all camera's in Unity are rendered sequentially, we share a single static render texture across each
eye camera.  SteamVR provides a recommended render target size as a minimum to account for distortion,
however, rendering to a higher resolution provides additional multisampling benefits at the associated
expense.  By default, SteamVR_Camera rounds up to the next power-of-two dimensions in order to take advantage
of Unity's render texture anti-aliasing support.  For the Oculus Rift DK1 this end up at 1024x1024.
SteamVR_Camera.cs defines HI_QUALITY at the top to control the rounding up functionality.   Comment this out to
use the recommended size instead, or just change the code to whatever works best for your project.  This
setting is not per-camera since the texture gets shared.

SteamVR_Camera expects an existing Camera component on its gameObject.  It uses this as a template to create  
the left and right eye child objects.  If there are additional components you would like copied to each of the  
sub-cameras, these should be added to the 'Render Components' array.  This can be performed in the Inspector  
by simply dragging the title of each existing component onto SteamVR_Camera's 'Render Components'.  Any  
components that override OnRenderImage (e.g. Image Effects) are automatically copied over and do not need to
be added to the list.

Note: GUILayer is not compatible with SteamVR_Camera since it renders in screen space rather than world space,  
and will be automatically removed for you.  Instead, to render 2D content, an optional Overlay texture can be  
specified.  This content is composited into the scene on a virtual curved surface using a special render path  
for increased fidelity.  See [Status]._Stats in the example scene for how to set this up.

SteamVR_Camera also creates an "offset" object.  The head tracking transform is applied to this object  
allowing the Main Camera to be moved programmatically by other systems.  This object corresponds to the point  
midway between the user's eyes.  The camera's AudioListener is automatically moved to this object to ensure  
positional audio works properly.  Add other objects to the camera's "FollowHead" array to auto-attach them to
this offset object as well.  Similarly a "FollowEyes" array is provided to attach objects to each eye (and are
therefore duplicated).  For performing ray-casts in code based on which way the user is looking, you'll want to
use the Transform GetComponent<SteamVR_Camera>().offset or SteamVR_Camera.GetRay().

Assets/SteamVR/Scripts/SteamVR_CameraEye.cs - This script is automatically added to the two eye camera's that  
SteamVR_Camera creates on startup.  It stores per-eye values, and handles actual rendering of the scene.  The  
scene is first rendered to an off-screen texture, and then copied to the appropriate half of the viewport using  
a tessellated mesh with three sets of UVs which compensate for lens distortion and chromatic aberration.

Assets/SteamVR/Scripts/SteamVR_Utils.cs - Various bits for working with the SteamVR API in Unity including a  
simple event system, a RigidTransform class for working with vector/quaternion pairs, matrix conversions, and  
other useful functions.  Logic for calculating distortion mesh and properly rendering skyboxes lives here as  
well.

Assets/SteamVR/Shaders/SteamVR_Distort.shader - This shader handles correcting for lens distortion and  
chromatic aberration for both the scene blit and the overlay raytracing.  There are four separate passes to  
handle each combination of overlay usage and antialiasing.  Only one is ever used at a time.


GUILayer, GUIText, and GUITexture:

The recommended way for drawing 2D content is through SteamVR_Camera's overlay texture.  There is an example  
of how to set this up in the example scene.  GUIText and GUITexture use their Transform to determine where  
they are drawn, so these objects will need to live near the origin.  You will need to set up a separate camera  
using a Target Texture.  To keep it from rendering other elements of your scene, you should create a unique  
layer used by all of your gui elements, and set the camera's Culling Mask to only draw those items.  Set its  
depth to -1 to ensure it gets updated before composited into the final view.


OnGUI:

Assets/SteamVR/Scripts/SteamVR_Menu.cs demonstrates use of OnGUI with SteamVR_Camera's overlay texture.  The  
key is to set RenderTexture.active and restore it afterward.  Beware when also using a camera to render to the  
same texture as it may clear your content.


Camera layering:

One powerful feature of Unity is its ability to layer cameras to render scenes (e.g. drawing a skybox scene
with one camera, the rest of the environment with a second, and maybe a third for a 3D hud).  This is performed
by setting the latter cameras to only clear the depth buffer, and leveraging the cameras' cullingMask to control
which items get rendered per-camera, and depth to control order.  The only extra bit to worry about when in VR
is setting the ApplyDistortion flag *only* on the final camera.  This is also the camera that will be used to
render the overlay texture if you are using that.  ApplyDistortion can be toggled dynamically to support scene
transitions (e.g. switching from a cockpit view to an overview).


Camera scale:

Setting SteamVR_Camera's gameObject scale will result in the world appearing (inversely) larger or smaller.
This can be used to powerful effect, and is useful for allowing you to build skybox geometry at a sane scale
while still making it feel far away.  Similarly, it allows you to build geometry at scales the physics engine
and nav mesh generation prefers, while visually feeling much smaller or larger.  Of course, if you are building
geometry to real-world scale you should leave this at its default of 1,1,1.


Events:

SteamVR_Camera fires off several events.  These can be handled by registering for them through  
SteamVR_Utils.Event.Listen.  Be sure to remove your handler when no longer needed.  The best pattern is to  
Listen and Remove in OnEnable and OnDisable respectively.

"calibrating" - This event is sent when starting or stopping calibration with the new state.

"absolute_tracking" - This event is sent when losing or reacquiring absolute positional tracking.  This will  
never fire for the Rift DK1 since it does not have positional tracking.  For camera based trackers, this  
happens when the hmd exits and enters the camera's view.

"zerotracker" - This event is sent after the user hits Z to re-zero the tracker.  This can be useful for reacting
to changes in the zero tracker pose.

Feel free to leverage this system to fire off events of your own.  SteamVR_Utils.Event.Send takes any number  
of parameters, and passes them on to all registered callbacks.

A helper class has been included called SteamVR_Status which leverages these events to display hmd status to  
the user.  Examples of this can be found in the example scene.  SteamVR_StatusText specifically, leverages this
functionality to wrap up GUIText display, overriding SetAlpha.


Keybindings:

Escape/Start - toggle menu
PageUp/PageDown - adjust scale
Home - reset scale
Z - rezero the hmd tracker

These can be found in SteamVR_Camera nd SteamVR_Menu.  Feel free to strip them out if they conflict with your
existing configuration.


Antialiasing:

Rasterization is the process of taking geometric shapes and converting them to a 2D array of pixels which can  
then be displayed on a monitor.  This creates jaggies along edges not aligned to the grid.  Antialiasing  
encompasses a set of techniques for making these edges appear smoother.  Since we first render our scene to an  
off-screen texture which subsequently gets distorted and rerasterized to the viewport, there are multiple  
points in the pipeline that antialiasing can take place.

1) Textures with straight lines are helped by higher resolutions, better sampling (trilinear rather than  
bilinear or point), and anisotropic filtering (which requires mipmaps).

2) Antialiasing when rendering the scene into the off-screen render target.

3) Antialiasing when blitting the distorted scene texture onto the viewport.

SteamVR_Camera's antialiasing bool controls this third stage only.  It is most noticeable on the edges of the  
overlay texture.  If your application does not use the overlay texture, or the edges of that texture are  
alpha'd out, you may wish to disable this, and rely instead on the multisampling that happens by using a  
higher resolution off-screen render target, and proper sampling / filtering.


Framerate:

It is imperative in VR to make framerate to avoid judder.  Judder is the effect of seeing double due to the  
same frame being displayed twice without having updated the view transformation.  There are three factors that  
must be met to avoid this:

1) Vsync - Make sure vsync is enabled in all of your quality settings.  We do this already in SteamVR_Camera's  
Update.

2) Fullscreen exclusive mode - When running windowed, you are at the mercy of the desktop compositor which may  
introduce latency by queuing frames, and tearing due to multiple conflicting refresh rates of your attached  
monitors.

3) Update rate - if you try to render too much, or spend too much CPU time in your gameplay logic, then you'll  
miss the next screen refresh.

By default, Unity will launch non-windowed apps on your primary monitor.  To override this, use the 
commandline option "-adapter N" where N is the index of your Rift monitor (usually 1).  Also be sure to select  
the proper resolution from the dropdown (1280x800 for the Rift DK1 and 1920x1080 for the Rift DK2).  Make sure
you enable all aspect ratios in your Player settings (under Resolution and Presentation, supported Aspect
Ratios).

On Windows, you can make a shortcut to the executable, and append the command line option to the target field.

Note: The "-adapter N" command line option only works when using DirectX9.


Previewing in Editor:

SteamVR_Camera will automatically position the editor's game view in the correct place for viewing on your  
hmd.  To disable this functionality, set 'Position Game Window' to false.  Use's Unity's Layout functionality  
to restore the game window's position.  Previewing in the editor will not meet the framerate requirements 
outlined above, so you will still need to build and run in standalone in order to experience things fully.
Also, make sure you have 'Free Aspect' selected on the Game window so its view isn't artifically constrained.


Deploying on Steam:

If you are releasing your game on Steam (i.e. have a Steam ID and are calling Steam_Init through the  
Steamworks SDK), then you may want to check ISteamUtils::IsSteamRunningInVRMode() in order to determine if you  
should automatically launch into VR mode or not.


Known Issues:

* The first time SteamVR tries to move the game viewport in the editor to the hmd screen it fails when initially
docked.  If you leave it undocked and toggle Play again, it should fix itself on the second attempt.

* Unity does not support antialiasing on render textures in OpenGL (renders all black).  

* Unity's render texture antialiasing currently introduces some odd artifacts that manifest as ghosting of  
occluded parts of an object with shadows enabled, and speckling along edges on high contrast, high detailed  
objects.

* When overriding OnRenderImage on a camera using a target render texture, Unity uses its own internal temp  
render texture instead, ignoring useMipMaps, mipMapBias, filterMode and anisoLevel which could all be used to  
increase the fidelity of the final output.

* While a 64-bit version of this plugin is provided for Linux, SteamVR's runtime itself does not yet support  
that configuration.

* If you are using another plug-in (e.g. Ludocity) for Steamworks integration, it also includes steam_api.dll.
To run side-by-side with this plug-in, ensure it is built against the same version, then delete one of the
duplicate dlls.

* If Unity finds an Assets\Plugins\x86 folder, it will ignore all files in Assets\Plugins.  You will need to
either move steam_api.dll into the x86 subfolder, or move the dlls in the x86 folder up a level and delete
the x86 folder.


Troubleshooting:

* HmdError_Init_VRClientDLLNotFound - SteamVR not installed, or cannot be found.  On OSX or Linux, ensure
Steam is running.  Try exiting and relaunching Steam.  Try uninstalling and reinstalling SteamVR.

* HmdError_Init_HmdNotFound - SteamVR cannot detect your VR headset, ensure the USB cable is plugged in.
If that doesn't work, try deleting your Steam/config/steamvr.cfg.

* HmdError_Init_InterfaceNotFound - Make sure your SteamVR installation is up to date.

* HmdError_IPC_ConnectFailed - SteamVR launches a separate process called vrserver.exe which directly talks
to the hardware.  Games communicate to vrserver through vrclient.dll over IPC.  This error is usually due
to the communication pipe between the two having closed.  Ensure you are only calling SteamVR_Init and
SteamVR_Shutdown once.  Also use task manager to verify there are no rogue apps that got stuck trying to
shut down.

