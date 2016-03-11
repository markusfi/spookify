using System;
using LRUCache.Implementation;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Spookify
{
	[Serializable]
	public class CurrentLRUCache
	{
		static CurrentLRUCache _currentLRUCache;
		public static CurrentLRUCache Current
		{ 
			get {
				if (_currentLRUCache == null) {
					try {
						BinaryFormatter formatter = new BinaryFormatter();
						if (File.Exists (CurrentLRUCache.StateFilename)) {
							using (var fs = new FileStream (CurrentLRUCache.StateFilename, FileMode.Open, FileAccess.Read)) {
								_currentLRUCache = formatter.Deserialize (fs) as CurrentLRUCache;
							}
						}
						if (_currentLRUCache == null)
							_currentLRUCache = new CurrentLRUCache ();
					}
					catch {
						_currentLRUCache = new CurrentLRUCache ();
					}
				}
				return _currentLRUCache;			
			} 
		}
		public static string StateFilename {
			get {
				var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				var filename = Path.Combine (documents, "MyCovers");
				return filename;
			}
		}
		LRUCache<string,byte[]> _coverCache = null;
		public LRUCache<string,byte[]> CoverCache {
			get {
				if (_coverCache == null)
					_coverCache = new LRUCache<string, byte[]>(100);
				return _coverCache;
			}
		}
		public void StoreCurrentState()
		{
			BinaryFormatter formatter = new BinaryFormatter();
			if (File.Exists (CurrentLRUCache.StateFilename))
				File.Delete (CurrentLRUCache.StateFilename);
			using (var fs = new FileStream (CurrentLRUCache.StateFilename, FileMode.CreateNew, FileAccess.ReadWrite)) {
				fs.Seek (0, SeekOrigin.Begin);
				fs.SetLength (0);
				formatter.Serialize (fs, this);
			}
		}
	}
}

