using System;
using LRUCache.Implementation;
using System.Collections.Generic;

namespace Spookify
{
	[Serializable]
	public class CurrentPlaylistsCache: CurrentBase<CurrentPlaylistsCache>
	{
		public override string Filename()  { return "MyPlaylists"; } 

		LRUCache<string,List<PlaylistBook>> _listCache = null;
		public LRUCache<string,List<PlaylistBook>> ListCache {
			get {
				if (_listCache == null)
					_listCache = new LRUCache<string, List<PlaylistBook>>(100);
				return _listCache;
			}
			set { _listCache = value; }
		}
	}
}

