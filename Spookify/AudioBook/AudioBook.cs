using System;
using System.Linq;
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
		public IEnumerable<string> Artists { get { return Authors != null ? Authors.Select (a => a.Name) : new string[0]; } }
		public List<Author> Authors { get; set; } 
		public string SmallestCoverURL  { get; set; }
		public string LargestCoverURL { get; set; }
		public string Uri { get; set; }

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
				if (coverData != null) {
					imageView.Image = UIImage.LoadFromData (coverData);
					if (imageView.Image != null)
						imageView.Hidden = false;
				} else if (converUrl != null) {
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
							if (image != null)
								imageView.Hidden = false;
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
	public class Author
	{
		public string Name { get; set; }
		public string URI { get; set; }
	}

	[Serializable]
	public class PlaylistBook 
	{		
		public AudioBookAlbum Album { get; set; }
		public List<AudioBookTrack> Tracks { get; set; }
		public IEnumerable<string> Artists { get { return Authors != null ? Authors.Select (a => a.Name) : new string[0]; } }
		public List<Author> Authors { get; set; } 
		public string SmallestCoverURL { get; set; }
		public string LargestCoverURL { get; set; }
		public string Uri { get; set; }

		public static explicit operator PlaylistBook(AudioBook ab)
		{
			return new PlaylistBook() {
				Album = new AudioBookAlbum() { Name = ab.Album == null ? null : ab.Album.Name },
				Tracks = ab.Tracks == null ? null : ab.Tracks.Select(t => new AudioBookTrack() { Name = t.Name, Index = t.Index, Url = t.Url, Duration = t.Duration }).ToList(),
				Authors = ab.Authors == null ? null : ab.Authors.Select(a => new Author() { Name = a.Name, URI = a.URI }).ToList(),
				SmallestCoverURL = ab.SmallestCoverURL,
				LargestCoverURL = ab.LargestCoverURL,
				Uri = ab.Uri
			};
		}

		public static void CreateFromAsync(SPTPartialPlaylist p, Action<IEnumerable<PlaylistBook>> action)
		{
			if (p == null)
				return;
			if (action == null)
				return;

			var list = CurrentPlaylistsCache.Current.ListCache.GetItem (p.Uri.AbsoluteString);
			if (list != null) {
				action (list.Data);
			} else {				
				SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
				NSError errorOut;
				NSUrlRequest playlistReq = SPTPlaylistSnapshot.CreateRequestForPlaylistWithURI (p.Uri, auth.Session.AccessToken, out errorOut);
				SPTRequestHandlerProtocol_Extensions.Callback (SPTRequest.SPTRequestHandlerProtocol, playlistReq, (er, resp, dat) => {
					if (er != null) {
						return;
					}
					var page = SPTPlaylistSnapshot.PlaylistSnapshotFromData (dat, resp, out errorOut);
					if (page != null && page.FirstTrackPage != null && page.FirstTrackPage.Items != null && page.FirstTrackPage.Items.Any ()) {
						var books = CreatePlaylistBooks (page.FirstTrackPage);
						CurrentPlaylistsCache.Current.ListCache.Insert(p.Uri.AbsoluteString, books.ToList());
						action (books);
					}
				});
			}
		}
		public static void CreateFromFullAsync(SPTPartialPlaylist p, Action<IEnumerable<PlaylistBook>, bool> action, Action finished)
		{
			if (p == null || action == null) {
				Console.WriteLine ("CreateFromFullAsync: Abbruch weil SPTPartialPlaylist oder action ungültig");
				if (finished != null)
					finished ();
				return;
			}
	
			SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
			NSError errorOut;
			NSUrlRequest playlistReq = SPTPlaylistSnapshot.CreateRequestForPlaylistWithURI (p.Uri, auth.Session.AccessToken, out errorOut);
			SPTRequestHandlerProtocol_Extensions.Callback (SPTRequest.SPTRequestHandlerProtocol, playlistReq, (er, resp, dat) => {
				if (er != null) {
					Console.WriteLine ("CreateFromFullAsync: Abbruch weil Error: "+er.LocalizedDescription);
					if (finished != null)
						finished ();
				}
				var page = SPTPlaylistSnapshot.PlaylistSnapshotFromData (dat, resp, out errorOut);
				if (page != null)
					AddListPage(auth, page.FirstTrackPage, action, finished);
				else {
					Console.WriteLine ("CreateFromFullAsync: Abbruch weil PlaylistSnapshotFromData() == null");
					if (finished != null)
						finished ();
				}
			});
		}
		static void AddListPage(SPTAuth auth, SPTListPage firstTrackPage,  Action<IEnumerable<PlaylistBook>, bool> action, Action finished)
		{
			if (firstTrackPage != null) {
				if (firstTrackPage.Items != null && firstTrackPage.Items.Any ()) {
					var books = CreatePlaylistBooks (firstTrackPage);
					action (books, !firstTrackPage.HasNextPage);
				}
				if (firstTrackPage.HasNextPage) {
					// next page of this playlists books
					NSError errorOut;
					var nsUrlRequest = firstTrackPage.CreateRequestForNextPageWithAccessToken (auth.Session.AccessToken, out errorOut);
					if (errorOut != null) {
						Console.WriteLine ("AddListPage: Abbruch weil firstTrackPage.CreateRequestForNextPageWithAccessToken() Error: "+errorOut.LocalizedDescription);
						if (finished != null)
							finished ();
						return;
					}
					if (nsUrlRequest != null) {
						SPTRequestHandlerProtocol_Extensions.Callback (SPTRequest.SPTRequestHandlerProtocol, nsUrlRequest, (er, resp1, jsonData1) => {
							if (er != null) {
								Console.WriteLine ("AddListPage: Abbruch weil Callback() Error: "+er.LocalizedDescription);
								if (finished != null)
									finished ();
								return;
							}
							var nextpage = SPTListPage.ListPageFromData (jsonData1, resp1, true, "", out errorOut);
							if (errorOut != null) {
								Console.WriteLine ("AddListPage: Abbruch weil SPTListPage.ListPageFromData() Error: "+errorOut.LocalizedDescription);
								if (finished != null)
									finished ();
								return;
							}
							if (nextpage != null)
								AddListPage (auth, nextpage, action, finished);
						});
					}
				} else {
					Console.WriteLine ("AddListPage: Abbruch weil firstTrackPage.HasNextPage == false");
					if (finished != null)
						finished ();
				}
			} else {
				Console.WriteLine ("AddListPage: Abbruch weil firstTrackPage == null");
				if (finished != null)
					finished ();
			}
		}


		public static IEnumerable<PlaylistBook>CreatePlaylistBooks(SPTListPage firstTrackPage)
		{
			foreach (SPTPlaylistTrack track in firstTrackPage.Items) {
				if (track != null && 
					!track.Album.Name.StartsWith("Anleitung zum Profil Hörbücher und den Playlists") &&
					!track.Album.Name.StartsWith("Full Length Audiobooks: Here's how it works!")) {

					yield return new PlaylistBook () {
						Album = new AudioBookAlbum () {
							Name = track.Album.Name   
						},
						Authors = track.Artists.Cast<SPTPartialArtist> ().Select (a => new Author () {
							Name = a.Name,
							URI = a.Uri.AbsoluteString
						}).ToList (),
						SmallestCoverURL = track.Album.SmallestCover.ImageURL.AbsoluteString,
						LargestCoverURL = track.Album.LargestCover.ImageURL.AbsoluteString,
						Uri = track.Album.GetUri ().AbsoluteString
					};
				}
			}
		}

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

