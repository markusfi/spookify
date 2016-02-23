using System;
using System.Linq;
using UIKit;
using SpotifySDK;
using CoreFoundation;
using Foundation;
using System.Collections.Generic;

namespace Spookify
{
	public partial class HoerbuchViewController : UIViewController
	{
		public PlaylistBook Book { get; set; }

		public HoerbuchViewController (IntPtr handle) : base (handle)
		{
		}
		public HoerbuchViewController () : base ("HoerbuchViewController", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.AlbumLabel.Text = Book.Album.Name;
			this.AuthorLabel.Text = Book.Artists.FirstOrDefault();
			var imageView = this.AlbumImage;
			if (imageView != null) {
				imageView.Image = null;
				var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
				gloalQueue.DispatchAsync (() => {
					NSError err = null;
					UIImage image = null;
					NSData imageData = NSData.FromUrl (new NSUrl(Book.LargestCoverURL), 0, out err);
					if (imageData != null)
						image = UIImage.LoadFromData (imageData);

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

		string MakeSearchKey(string name)
		{
			name = name.Replace("Ungekürzte Fassung","");
			name = name.Replace("Ungekürzt","");
			name = name.Replace("Gekürzte Fassung","");
			name = name.Replace("Gekürzt","");
			foreach (var source in name.ToCharArray().Where(c => char.IsPunctuation(c) || char.IsWhiteSpace(c))) {
				name = name.Replace(source,'+');
			}
			while (name.Contains("++")) {
				name = name.Replace("++","+");
			}
			if (name.EndsWith("+"))
				name = name.Substring(0,name.Length-1);
			return System.Web.HttpUtility.UrlEncode(name);
		}
		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			try {
				var rechercheViewController = segue.DestinationViewController as RechercheViewController;
				var name = MakeSearchKey(Book.Album.Name);
				var author = MakeSearchKey(Book.Artists.FirstOrDefault());
				rechercheViewController.Url = string.Format(@"https://www.google.de/search?q='{0}'{1}'{2}'&as_sitesearch=amazon.de",name,System.Web.HttpUtility.UrlEncode(" "),author);
			}
			catch(System.Exception ex) {
				System.Diagnostics.Debug.WriteLine ("Bug: " + ex.ToString ());
			}	
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		partial void OnBuchSelektiert (UIKit.UIButton sender)
		{
			var url = new NSUrl(Book.Uri);
			SPTAuth auth = SPTAuth.DefaultInstance;
			var p = SPTRequest.SPTRequestHandlerProtocol;
			NSError errorOut;

			var nsUrlRequest = SPTAlbum.CreateRequestForAlbum(url, auth.Session.AccessToken, "DE", out errorOut);
			SPTRequestHandlerProtocol_Extensions.Callback(p, nsUrlRequest, (er,resp,jsonData) => {
				if (er != null) {
					return;
				}
				NSError nsError;
				var album = SPTAlbum.AlbumFromData(jsonData, resp, out nsError);
				if (album!= null) {
					int kapitelNummer = 1;
					var newbook = new AudioBook() {
						Album = new AudioBookAlbum() { Name = album.Name } , 
						Tracks = album.FirstTrackPage.Items.Cast<SPTPartialTrack>().Select(pt => new AudioBookTrack() { Url = pt.GetUri().AbsoluteString, Name = pt.Name, Duration = pt.Duration, Index = kapitelNummer++  } ).ToList(),
						Artists = album.Artists.Cast<SPTPartialArtist>().Select(a => a.Name).ToList(),
						LargestCoverURL = album.LargestCover.ImageURL.AbsoluteString,
						SmallestCoverURL = album.SmallestCover.ImageURL.AbsoluteString
					};
					LoadNextPageAsync(newbook, album.FirstTrackPage, auth, p);
					var thisBook = CurrentState.Current.Audiobooks.FirstOrDefault(a => a.Album.Name == album.Name);
					if (thisBook != null) {
						CurrentState.Current.Audiobooks.Remove(thisBook);
						newbook.CurrentPosition = thisBook.CurrentPosition;
					}					
					CurrentState.Current.Audiobooks.Add(newbook);
					CurrentState.Current.CurrentAudioBook = newbook;
					CurrentState.Current.StoreCurrentState();

					var tabBarController = this.TabBarController;

					UIView  fromView = tabBarController.SelectedViewController.View;
					UIView  toView = tabBarController.ViewControllers[1].View;

					UIView.Transition(fromView,toView,0.5,UIViewAnimationOptions.CurveEaseInOut,() => { tabBarController.SelectedIndex = 1; });
				} 
			});
		}

		void LoadNextPageAsync(AudioBook newbook, SPTListPage page, SPTAuth auth, SPTRequestHandlerProtocol p)
		{
			NSError errorOut, nsError;
			if (page != null && page.HasNextPage) {
				var nsUrlRequest = page.CreateRequestForNextPageWithAccessToken(auth.Session.AccessToken, out errorOut);
			    SPTRequestHandlerProtocol_Extensions.Callback(p, nsUrlRequest, (er1,resp1,jsonData1) => {
					var nextpage = SPTListPage.ListPageFromData(jsonData1, resp1, true, "", out nsError);
					if (nextpage != null) {
						int kapitelNummer = newbook.Tracks.Max(t => t.Index) + 1;
						newbook.Tracks.AddRange(nextpage.Items.Cast<SPTPartialTrack>().Select(pt => new AudioBookTrack() { Url = pt.GetUri().AbsoluteString, Name = pt.Name, Duration = pt.Duration, Index = kapitelNummer++ } ));
						LoadNextPageAsync(newbook, nextpage, auth, p);
					}
				});
			}
		}
	}
}


