using System;
using LRUCache.Implementation;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Spookify
{
	[Serializable]
	public class CurrentLRUCache : CurrentBase<CurrentLRUCache>
	{
	    public override string Filename() { return "MyCovers"; } 

		LRUCache<string,byte[]> _coverCache = null;
		public LRUCache<string,byte[]> CoverCache {
			get {
				if (_coverCache == null)
					_coverCache = new LRUCache<string, byte[]>(100);
				return _coverCache;
			}
			set {
				_coverCache = value;			
			}
		}
	}
}

