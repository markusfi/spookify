using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

namespace Spookify
{
	[Serializable]
	public class CurrentSettings
	{
		public CurrentSettings ()
		{
			kTokenSwapService = true;
			kTokenRefreshService = true;
		}

		public bool kTokenSwapService { get; set; }
		public bool kTokenRefreshService { get; set; }

		static CurrentSettings _currentState;
		public static CurrentSettings Current
		{ 
			get {
				if (_currentState == null) {
					try {
						BinaryFormatter formatter = new BinaryFormatter();
						if (File.Exists (CurrentSettings.StateFilename)) {
							using (var fs = new FileStream (CurrentSettings.StateFilename, FileMode.Open, FileAccess.Read)) {
								_currentState = formatter.Deserialize (fs) as CurrentSettings;
							}
						}
						if (_currentState == null)
							_currentState = new CurrentSettings ();
					}
					catch {
						_currentState = new CurrentSettings ();
					}
				}
				return _currentState;			
			} 
		}
		public static string StateFilename {
			get {
				var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				var filename = Path.Combine (documents, "MySettings");
				return filename;
			}
		}
		public void StoreCurrentSettings()
		{
			BinaryFormatter formatter = new BinaryFormatter();
			if (File.Exists (CurrentSettings.StateFilename))
				File.Delete (CurrentSettings.StateFilename);
			using (var fs = new FileStream (CurrentSettings.StateFilename, FileMode.CreateNew, FileAccess.ReadWrite)) {
				fs.Seek (0, SeekOrigin.Begin);
				fs.SetLength (0);
				formatter.Serialize (fs, this);
			}
		}
	}
}

