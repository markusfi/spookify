<<<<<<< HEAD
# spookify

iOS App that allows to listen to German Spotify Audiobooks.

Using Spotify iOS SDK.
Written in C# and build with Xamarin Studio.

You need a Xamarin iOS account and Xamarin Studio to build this app.
You need a Spotify Premium Account to use this App.

Not finished, work in progress.

Done so far:
- Created warpper project to use Spotify iOS SDK from Xamarin/C#
- Working App as a Testflight version awaing for approval
- You can actually look up a German audiobook, select it and listen to it
- You can find the audiobook through integrated google search on amazon.de

Next Steps:
- Create Spotify Refresh Service in C' for IIS
- Integrate with worldcat.org to lookup background information for audiobooks
- Integrate with goodreads.com to lookup reviews and background to audiobooks
- Integrate non german playlists for Spotify Audiobooks
 
If you want to test the app send me a message to markus[at]spookify.de





=======
**WARNING: This is a beta release of the Spotify iOS SDK, and can stop working
at any time. This SDK release is not suitable for publicly released applications.**


Spotify iOS SDK Readme
=======

Welcome to Spotify iOS SDK! This ReadMe is for people who wish to develop iOS
applications containing Spotify-related functionality, such as audio streaming,
playlist manipulation, searching and more.


Beta Release Information
=======

We're releasing this SDK early to gain feedback from the developer community
about the future of our iOS SDKs. Please file feedback about missing issues or
bugs over at our [issue tracker](https://github.com/spotify/ios-sdk/issues),
making sure you search for existing issues and adding your voice to those
rather than duplicating.

For known issues and release notes, see the
[CHANGELOG.md](https://github.com/spotify/ios-sdk/blob/master/CHANGELOG.md)
file.

**IMPORTANT:** This SDK is pre-release software and is not supported, and must
not be shipped to end users. It *will* stop working in the future.


OAuth/SPTAuth Credentials
=======

For the beta release, please use the following OAuth credentials:

* Client ID: `spotify-ios-sdk-beta`
* Client Callback URL: `spotify-ios-sdk-beta://callback`,
  `spotify-ios-sdk-beta-alternate://callback` or
  `spotify-ios-sdk-beta-alternate-2://callback`
* Client Secret: `ba95c775e4b39b8d60b27bcfced57ba473c10046`

These credentials will be invalidated when the beta period is over. At this
point, you'll be able to request your own personal credentials for future use.


Getting Started
=======

Getting the Spotify iOS SDK into your applcation is easy:

1. Add the `Spotify.framework` library to your Xcode project.
2. Add the `-ObjC` flag to your project's `Other Linker Flags` build setting.
3. Add `AVFoundation.framework` to the "Link Binary With Libraries" build phase
   of your project.
4. `#import <Spotify/Spotify.h>` into your source files and away you go!

The library's headers are extensively documented, and it comes with an Xcode
documentation set which can be indexed by Xcode itself and applications like
Dash. This, along with the included demo projects, should give you everything
you need to get going. The classes that'll get you started are:

* `SPTAuth` contains methods of authenticating users. See the "Basic Auth" demo
  project for a working example of this.

  **Note:** To perform audio playback, you must request the `login` scope when
  using `SPTAuth`. To do so, pass an array containing the string `@"login"` to
  `-loginURLForClientId:declaredRedirectURL:scopes:`. The supplied demo
  projects already do this.

* `SPTRequest` contains methods for searching, getting playlists and doing
  metadata lookup. Most metadata classes (`SPTTrack`, `SPTArtist`, `SPTAlbum` and
  so on) contain convenience methods too.

* `SPTTrackPlayer` is a class for playing track providers (currently `SPTAlbum`
  and `SPTPlaylist`) with basic playback controls. `SPTAudioStreamingController`
  gives you more direct access to audio streaming if you need it.


Migrating from CocoaLibSpotify
=======

CocoaLibSpotify is based on the libspotify library, which contains a lot of
legacy and is a very complex library. While this provided a great deal of
functionality, it could also eat up a large amount of RAM and CPU resources,
which isn't ideal for mobile platforms.

The Spotify iOS SDK is based on a completely new technology stack that aims to
avoid these problems while still providing a rich set of functionality. Due to
this new architecture, we took the decision to start from scratch with the
Spotify iOS SDK's API rather than trying to squeeze the new technology into
CocoaLibSpotify's API. This has resulted in a library that's much easier to use
and has a vastly smaller CPU and RAM footprint compared to CocoaLibSpotify.

The Spotify iOS API does *not* have 1:1 feature parity with CocoaLibSpotify.
It contains functionality that CocoaLibSpotify does not, and CocoaLibSpotify
has features that the Spotify iOS SDK does not. We're working to close that
gap, and if there's a feature missing from the Spotify iOS SDK that's
particularly important to you, please get in touch so we can prioritise
correctly.

Due to the API and feature differences between CocoaLibSpotify and the Spotify
iOS SDK, we understand that migration may be difficult. Due to this,
CocoaLibSpotify will remain available for a reasonable amount of time after
this SDK exits beta status.
>>>>>>> cd114b2... Spotify iOS SDK Beta 1 Release
