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
	public class CurrentState : CurrentBase<CurrentState>
	{
	 	public override string Filename()  { return "MyAudioBooks"; } 

		public CurrentState ()
		{
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
				return CurrentTrack?.Url;
			}
		}
		public AudioBookTrack CurrentTrack
		{
			get {
				if (CurrentAudioBook != null &&
					CurrentAudioBook.Tracks != null &&
					CurrentAudioBook.CurrentPosition != null &&
					CurrentAudioBook.Tracks.Count > CurrentAudioBook.CurrentPosition.TrackIndex) 
				{
					return CurrentAudioBook.Tracks [CurrentAudioBook.CurrentPosition.TrackIndex];
				}
				return null;
			}
		}
		public AudioBook CurrentAudioBook;
	}
}

