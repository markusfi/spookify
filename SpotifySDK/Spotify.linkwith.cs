using ObjCRuntime;

[assembly: LinkWith ("Spotify.a", SmartLink = true, ForceLoad = true, Frameworks="AVFoundation QuartzCore AudioToolbox SystemConfiguration")]
