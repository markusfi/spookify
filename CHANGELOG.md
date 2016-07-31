Spotify iOS SDK Beta 1
=============

**What's New**

* Initial release.

**Known Issues**

* No cover art APIs. ([#1](https://github.com/spotify/ios-sdk/issues/1))

* Cannot remove items from playlists. ([#2](https://github.com/spotify/ios-sdk/issues/2))

* Sessions will expire after one day, even if persisted to disk. At this point,
you'll need to re-authenticate the user using `SPAuth`. ([#3](https://github.com/spotify/ios-sdk/issues/3))
