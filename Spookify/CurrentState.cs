using System;
using System.Linq;
using UIKit;
using SpotifySDK;
using CoreFoundation;
using Foundation;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
					try {
						BinaryFormatter formatter = new BinaryFormatter();
						if (File.Exists (CurrentState.StateFilename)) {
							using (var fs = new FileStream (CurrentState.StateFilename, FileMode.Open, FileAccess.Read)) {
								_currentState = formatter.Deserialize (fs) as CurrentState;
							}
						}
						if (_currentState == null)
							_currentState = new CurrentState ();
					}
					catch {
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
		public void StoreCurrentState()
		{
			BinaryFormatter formatter = new BinaryFormatter();
			if (File.Exists (CurrentState.StateFilename))
				File.Delete (CurrentState.StateFilename);
			using (var fs = new FileStream (CurrentState.StateFilename, FileMode.CreateNew, FileAccess.ReadWrite)) {
				fs.Seek (0, SeekOrigin.Begin);
				fs.SetLength (0);
				formatter.Serialize (fs, this);
			}
		}
			

	}

	[Serializable]
	public class AudioBookAlbum
	{
		public string Name { get; set; }
	}

	[Serializable]
	public class AudioBookTrack
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public double Duration { get; set; }
		public NSUrl NSUrl { get { return new NSUrl (this.Url); } }
		public int Index { get; set; }
	}

	[Serializable]
	public class AudioBookBookmark
	{
		public AudioBookBookmark() {}
		public AudioBookBookmark(AudioBookBookmark orig) {
			this.PlaybackPosition = orig.PlaybackPosition;
			this.TrackIndex = orig.TrackIndex;
			this.Timestamp = DateTime.Now;
		}
		public double PlaybackPosition { get; set; }
		public int TrackIndex { get; set; }
		public DateTime Timestamp { get; set; }
	}

	[Serializable]
	public class AudioBook
	{
		public AudioBookAlbum Album { get; set; }
		public List<AudioBookTrack> Tracks { get; set; }
		public List<AudioBookBookmark> Bookmarks { get; set; }
		public List<string> Artists { get; set; } 
		public string SmallestCoverURL  { get; set; }
		public string LargestCoverURL { get; set; }

		public AudioBookBookmark CurrentPosition { get; set; }
	}

	[Serializable]
	public class PlaylistBook
	{
		public AudioBookAlbum Album { get; set; }
		public List<AudioBookTrack> Tracks { get; set; }
		public List<string> Artists { get; set; } 
		public string SmallestCoverURL { get; set; }
		public string LargestCoverURL { get; set; }
		public string Uri { get; set; }
	}

	[Serializable]
	public class AudioBookPlaylist
	{
		List<PlaylistBook> _books;
		public List<PlaylistBook> Books 
		{
			get {
				if (_books == null)
					_books = new List<PlaylistBook> ();
				return _books;
			}
		}
		public string NextPageURL  { get; set; }
		public SPTListPage CurrentPage { get; set; }
	} 



}

