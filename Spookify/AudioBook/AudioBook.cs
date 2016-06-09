using System;
using System.Linq;
using SpotifySDK;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreFoundation;
using System.Collections;
using Newtonsoft.Json;
using System.Globalization;

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
	public class Volume
	{
		[JsonProperty("items")]
		public IList<Item> Items { get; set; }
	}
	[Serializable]
	public class Item
	{
		[JsonProperty("volumeInfo")]
		public VolumeInfo VolumeInfo { get; set;}
	}

	[Serializable]
	public class VolumeInfo 
	{
		public static VolumeInfo Empty { get; } = new VolumeInfo();

		[JsonProperty("title")]
		public string Title { get; set; }
		[JsonProperty("averageRating")]
		public float AverageRating { get; set; }
		[JsonProperty("ratingsCount")]
		public float RatingsCount { get; set; }
		[JsonProperty("publishedDate")]
		public string _publishedDateInternal { set; private get; }
		[JsonProperty("description")]
		public string Description { get; set; }

		public DateTime? PublishedDate {
			get { 
				if (!string.IsNullOrWhiteSpace (_publishedDateInternal)) {
					DateTime date;
					if (DateTime.TryParseExact (_publishedDateInternal, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
						return date;
					}
				}
				return null;
			}
		}
		[JsonProperty("publisher")]
		public string Publisher { get; set; }
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
		public string[] ImageUrls { get; set; }
		public string Uri { get; set; }
		public VolumeInfo VolumeInfo { get; set; }

		public string MediumCoverUrl { 
			get { 
				var medium = ImageUrls?.FirstOrDefault (i => i != LargestCoverURL && i != SmallestCoverURL);
				if (medium == null)
					medium = SmallestCoverURL;
				return medium;
			}
		}

		public double GesamtSeit(AudioBookBookmark bookmark) {
			return this.Tracks == null ? 0.0 : this.Tracks.Take (bookmark?.TrackIndex ?? 0).Sum (t => t.Duration) + (bookmark?.PlaybackPosition ?? 0.0);
		}
		public double GesamtSeitAnfang { get { return GesamtSeit (this.CurrentPosition);  } }
		public double GesamtBisEnde { get { return this.Tracks == null ? 0.0 : this.Tracks.Sum (t => t.Duration); } }

		[NonSerializedAttribute]
		NSData SmallestCoverData;
		[NonSerializedAttribute]
		NSData LargestCoverData;
		[NonSerializedAttribute]
		NSData MediumCoverData;
	
		public AudioBookBookmark CurrentPosition { get; set; }

		public bool Started { get; set; }
		public bool Finished { get; set; }

		public void SetSmallImage(UIImageView imageView)
		{
			imageView.SetUIImage(this.SmallestCoverURL, SmallestCoverData, (val) => SmallestCoverData = val);
		}
		public void SetLargeImage(UIImageView imageView)
		{
			imageView.SetUIImage( this.LargestCoverURL, LargestCoverData, (val) => LargestCoverData = val);
		}
		public void SetMediumImage(UIImageView imageView)
		{
			imageView.SetUIImage(this.MediumCoverUrl, MediumCoverData, (val) => MediumCoverData = val);
		}
		public void AddBookmark(AudioBookBookmark b)
		{
			if (Bookmarks == null)
				Bookmarks = new List<AudioBookBookmark> ();
			Bookmarks.Add (b);
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
		public string MediumCoverUrl { 
			get { 
				var medium = ImageUrls.FirstOrDefault (i => i != LargestCoverURL && i != SmallestCoverURL);
				if (medium == null)
					medium = SmallestCoverURL;
				return medium;
			}
		}
		public string[] ImageUrls { get; set; }
		public string Uri { get; set; }
		public VolumeInfo VolumeInfo { get; set; }

		int IComparer<PlaylistBook>.Compare (PlaylistBook x, PlaylistBook y) {
			return PlaylistBook.CompareUri (x, y);
		}
		public static int CompareUri (PlaylistBook x, PlaylistBook y)
		{			
			if ((x == null && y == null) || ((x != null && x.Uri == null) && (y != null && y.Uri == null)))
				return 0;			
			if (x == null || x.Uri == null)
				return -1;
			if (y == null || y.Uri == null)
				return 1;
			return string.Compare(x.Uri, y.Uri);
		}

		public static int CompareName (PlaylistBook x, PlaylistBook y)
		{			
			if (x?.Album?.Name == null && y?.Album?.Name == null)
				return 0;			
			if (x?.Album?.Name == null)
				return -1;
			if (y?.Album?.Name == null)
				return 1;
			return AlphanumComparator.AlphanumComparator.CompareStatic(x.Album.Name, y.Album.Name);
		}

		public static explicit operator PlaylistBook(AudioBook ab)
		{
			return new PlaylistBook() {
				Album = new AudioBookAlbum() { Name = ab.Album == null ? null : ab.Album.Name },
				Tracks = ab.Tracks == null ? null : ab.Tracks.Select(t => new AudioBookTrack() { Name = t.Name, Index = t.Index, Url = t.Url, Duration = t.Duration }).ToList(),
				Authors = ab.Authors == null ? null : ab.Authors.Select(a => new Author() { Name = a.Name, URI = a.URI }).ToList(),
				SmallestCoverURL = ab.SmallestCoverURL,
				LargestCoverURL = ab.LargestCoverURL,
				ImageUrls = ab.ImageUrls,
				Uri = ab.Uri,
				VolumeInfo = ab.VolumeInfo
			};
		}

		public static void CreatePlaylistFromUriAsync(NSUrl playlistUri, Action<IEnumerable<PlaylistBook>, bool, SPTListPage> addBooksCompletionHandler, Action finished)
		{
			if (playlistUri == null || addBooksCompletionHandler == null) {
				Console.WriteLine ("CreateFromFullAsync: Abbruch weil SPTPartialPlaylist oder action ungültig");
				if (finished != null)
					finished ();
				return;
			}
	
			SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
			NSError errorOut;
			NSUrlRequest playlistReq = SPTPlaylistSnapshot.CreateRequestForPlaylistWithURI (playlistUri, auth.Session.AccessToken, out errorOut);
			SPTRequestHandlerProtocol_Extensions.Callback (SPTRequest.SPTRequestHandlerProtocol, playlistReq, (er, resp, dat) => {
				if (er != null) {
					Console.WriteLine ("CreateFromFullAsync: Abbruch weil Error: "+er.LocalizedDescription);
					if (finished != null)
						finished ();
					return;
				}
				try {
					var page = SPTPlaylistSnapshot.PlaylistSnapshotFromData (dat, resp, out errorOut);
					if (page != null)
						AddListPage(auth, page.FirstTrackPage, addBooksCompletionHandler, finished);
					else {
						Console.WriteLine ("CreateFromFullAsync: Abbruch weil PlaylistSnapshotFromData() == null");
						if (finished != null)
							finished ();
					}
				} catch(Exception ex) {
					Console.WriteLine ("CreateFromFullAsync: Abbruch weil Exception int PlaylistSnapshotFromData()");
					if (finished != null)
						finished();
				}
			});
		}
		static void AddListPage(SPTAuth auth, SPTListPage firstTrackPage,  Action<IEnumerable<PlaylistBook>, bool, SPTListPage> addBooksCompletionHandler, Action finished)
		{
			if (firstTrackPage != null) {
				if (firstTrackPage.Items != null && firstTrackPage.Items.Any ()) {
					var books = CreatePlaylistBooks (firstTrackPage);
					addBooksCompletionHandler (books, !firstTrackPage.HasNextPage, firstTrackPage);
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
					track.Album != null &&
					track.Album.Name != null &&
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
						SmallestCoverURL = track.Album.SmallestCover?.ImageURL?.AbsoluteString,
						LargestCoverURL = track.Album.LargestCover?.ImageURL?.AbsoluteString,
						ImageUrls = track.Album.Covers?.Select(c => c.ImageURL?.AbsoluteString).ToArray(), 
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

