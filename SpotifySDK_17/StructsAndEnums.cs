using System;
using ObjCRuntime;

namespace SpotifySDK
{
	[Native]
	public enum SPTSearchQueryType : ulong
	{
		Track = 0,
		Artist,
		Album,
		Playlist
	}

	[Native]
	public enum SPTAlbumType : ulong
	{
		Album,
		Single,
		Compilation,
		AppearsOn
	}

	[Native]
	public enum SPTProduct : ulong
	{
		Free,
		Unlimited,
		Premium,
		Unknown
	}

	[Native]
	public enum SPTBitrate : ulong
	{
		Low = 0,
		Normal = 1,
		High = 2
	}
}
