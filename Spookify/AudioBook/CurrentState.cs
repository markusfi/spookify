using System;
using System.Linq;
using UIKit;
using SpotifySDK;
using CoreFoundation;
using Foundation;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using LRUCache.Implementation;

namespace Spookify
{	
	[Serializable]
	public class CurrentState
	{
		public CurrentState ()
		{
		}

		static CurrentState _currentState;
		public static CurrentState Current
		{ 
			get {
				if (_currentState == null) {
					lock (typeof(CurrentState)) {
						BinaryFormatter formatter = new BinaryFormatter ();
						try {
							if (File.Exists (CurrentState.StateFilename)) {
								using (var fs = new FileStream (CurrentState.StateFilename, FileMode.Open, FileAccess.Read)) {
									_currentState = formatter.Deserialize (fs) as CurrentState;
								}
							} else if (File.Exists (CurrentState.StateFilenameBackup)) {
								using (var fs = new FileStream (CurrentState.StateFilenameBackup, FileMode.Open, FileAccess.Read)) {
									_currentState = formatter.Deserialize (fs) as CurrentState;
								}
							}
						} catch {
							if (File.Exists (CurrentState.StateFilenameBackup)) {
								// Try the Backup file  
								try {
									using (var fs = new FileStream (CurrentState.StateFilenameBackup, FileMode.Open, FileAccess.Read)) {
										_currentState = formatter.Deserialize (fs) as CurrentState;
									}
								} catch {
									// Backup did not work either...
								}
							}
						}
						if (_currentState == null)
							_currentState = new CurrentState ();
					}
				}
				return _currentState;			
			} 
		}
		List<AudioBook> _audiobooks;
		public List<AudioBook> Audiobooks {
			get {
				if (_audiobooks == null)
					_audiobooks = new List<AudioBook> ();
				return _audiobooks;
			}
		}

		public string CurrentTrackURI
		{
			get {
				if (CurrentAudioBook != null &&
				    CurrentAudioBook.Tracks != null &&
				    CurrentAudioBook.CurrentPosition != null &&
				    CurrentAudioBook.Tracks.Count > CurrentAudioBook.CurrentPosition.TrackIndex) 
				{
					return CurrentAudioBook.Tracks [CurrentAudioBook.CurrentPosition.TrackIndex].Url;
				}
				return null;
			}
		}
		public AudioBook CurrentAudioBook;

		public static string StateFilename {
			get {
				var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
				var filename = Path.Combine (documents, "MyAudioBooks");
				return filename;
			}
		}
		public static string StateFilenameTemp {
			get {
				return StateFilename + ".tmp";
			}
		}
		public static string StateFilenameBackup {
			get {
				return StateFilename + ".backup";
			}
		}
		public void StoreCurrentState()
		{
			lock (typeof(CurrentState)) {
				if (File.Exists (CurrentState.StateFilenameTemp))
					File.Delete (CurrentState.StateFilenameTemp);

				BinaryFormatter formatter = new BinaryFormatter ();
				using (var fs = new FileStream (CurrentState.StateFilenameTemp, FileMode.CreateNew, FileAccess.ReadWrite)) {
					fs.Seek (0, SeekOrigin.Begin);
					fs.SetLength (0);
					formatter.Serialize (fs, this);
				}
				if (File.Exists (CurrentState.StateFilenameBackup))
					File.Delete (CurrentState.StateFilenameBackup);
				File.Replace (CurrentState.StateFilenameTemp, CurrentState.StateFilename, CurrentState.StateFilenameBackup);
			}
		}
	}
}

