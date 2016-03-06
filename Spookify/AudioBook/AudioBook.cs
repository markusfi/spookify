using System;
using SpotifySDK;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreFoundation;

namespace Spookify
{
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

		[NonSerializedAttribute]
		NSData SmallestCoverData;
		[NonSerializedAttribute]
		NSData LargestCoverData;
	
		public AudioBookBookmark CurrentPosition { get; set; }

		public void SetSmallImage(UIImageView imageView)
		{
			SetUIImage (imageView, this.SmallestCoverURL, SmallestCoverData, (val) => SmallestCoverData = val);
		}
		public void SetLargeImage(UIImageView imageView)
		{
			SetUIImage (imageView, this.LargestCoverURL, LargestCoverData, (val) => LargestCoverData = val);
		}

		void SetUIImage(UIImageView imageView, string converUrl, NSData coverData, Action<NSData> coverDataSetter)
		{
			if (imageView != null) {
				if (coverData != null)
					imageView.Image = UIImage.LoadFromData (coverData);
				else if (converUrl != null) {
					imageView.Image = null;
					var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
					gloalQueue.DispatchAsync (() => {
						NSError err = null;
						UIImage image = null;
						coverData = NSData.FromUrl (new NSUrl (converUrl), 0, out err);
						coverDataSetter(coverData);
						if (coverData != null)
							image = UIImage.LoadFromData (coverData);

						DispatchQueue.MainQueue.DispatchAsync (() => {
							imageView.Image = image;
							if (image == null) {
								System.Diagnostics.Debug.WriteLine ("Could not load image with error: {0}", err);
								return;
							}
						});
					});
				}	
			}
		}
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

