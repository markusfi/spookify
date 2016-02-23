using System;
using ObjCRuntime;

namespace SpotifySDK
{
[Native]
public enum SPTSearchQueryType : long
{
	Track = 0,
	Artist,
	Album,
	Playlist
}

[Native]
public enum SPTAlbumType : long
{
	Album,
	Single,
	Compilation,
	AppearsOn
}

[Native]
public enum SPTProduct : long
{
	Free,
	Unlimited,
	Premium,
	Unknown
}

[Native]
public enum SPTBitrate : long
{
	Low = 0,
	Normal = 1,
	High = 2
}
}