using System;
using System.Linq;
using SpotifySDK;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreFoundation;
using System.Collections;

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
	public class AudioBook : IComparer
	{
		[NonSerializedAttribute]
		public static AudioBook Empty = new AudioBook();

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
			imageView.SetUIImage(this.SmallestCoverURL, SmallestCoverData, (val) => SmallestCoverData = val);
		}
		public void SetLargeImage(UIImageView imageView)
		{
			imageView.SetUIImage( this.LargestCoverURL, LargestCoverData, (val) => LargestCoverData = val);
		}

		public int Compare (object x, object y)
		{
			var xb = x as AudioBook;
			var yb = y as AudioBook;
			if (xb == null && yb == null)
				return 0;			
			if (xb == null)
				return -1;
			if (yb == null)
				return 1;
			return string.Compare(xb.Uri, yb.Uri);
		}
	}

	[Serializable]
	public class Author
	{
		public string Name { get; set; }
		public string URI { get; set; }
	}

	[Serializable]
	public class PlaylistBook : IComparer<PlaylistBook>
	{		
		[NonSerializedAttribute]
		public static PlaylistBook Empty = new PlaylistBook();

		public AudioBookAlbum Album { get; set; }
		public List<AudioBookTrack> Tracks { get; set; }
		public IEnumerable<string> Artists { get { return Authors != null ? Authors.Select (a => a.Name) : new string[0]; } }
		public List<Author> Authors { get; set; } 
		public string SmallestCoverURL { get; set; }
		public string LargestCoverURL { get; set; }
		public string Uri { get; set; }

		int IComparer<PlaylistBook>.Compare (PlaylistBook x, PlaylistBook y)
		{			
			if ((x == null && y == null) || ((x != null && x.Uri == null) && (y != null && y.Uri == null)))
				return 0;			
			if (x == null || x.Uri == null)
				return -1;
			if (y == null || y.Uri == null)
				return 1;
			return string.Compare(x.Uri, y.Uri);
		}

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
		public static void CreateFromFullAsync(SPTPartialPlaylist p, Action<IEnumerable<PlaylistBook>, bool, SPTListPage> action, Action finished)
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
		static void AddListPage(SPTAuth auth, SPTListPage firstTrackPage,  Action<IEnumerable<PlaylistBook>, bool, SPTListPage> action, Action finished)
		{
			if (firstTrackPage != null) {
				if (firstTrackPage.Items != null && firstTrackPage.Items.Any ()) {
					var books = CreatePlaylistBooks (firstTrackPage);
					action (books, !firstTrackPage.HasNextPage, firstTrackPage);
				}
				// AddNextPage (auth, firstTrackPage, action, finished);
				Console.WriteLine ("AddListPage: Abbruch weil Continue in Breath");
				if (finished != null)
					finished ();
			} else {
				Console.WriteLine ("AddListPage: Abbruch weil firstTrackPage == null");
				if (finished != null)
					finished ();
			}
		}
		public static void CreateRequestNextPage(SPTListPage firstTrackPage, Action<IEnumerable<PlaylistBook>, bool, SPTListPage> action, Action finished)
		{
			if (firstTrackPage == null || action == null) {
				Console.WriteLine ("CreateFromFullAsync: Abbruch weil firstTrackPage oder action ungültig");
				if (finished != null)
					finished ();
				return;
			}

			SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
			AddNextPage (auth, firstTrackPage, action, finished);
		}
		static void AddNextPage(SPTAuth auth, SPTListPage firstTrackPage,  Action<IEnumerable<PlaylistBook>, bool, SPTListPage> action, Action finished)
		{
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

