using Foundation;
using UIKit;
using SpotifySDK;
using CoreFoundation;
using System.Linq;
using System;

namespace Spookify
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }
		public ResizeTabBarController TabBarController { get; set; }

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes() {
				TextColor = UIColor.White,
				TextShadowColor = UIColor.Clear,
				Font = UIFont.FromName("HelveticaNeue-Light",18)
			}); 

			UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
			UINavigationBar.Appearance.ShadowImage = new UIImage();
			UINavigationBar.Appearance.SetBackgroundImage (new UIImage (), UIBarMetrics.Default);

			ApplicationHelper.AsyncLoadWhenSession ();
			return true;
		}

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			SPTAuth auth = CurrentPlayer.Current.AuthPlayer;

			SPTAuthCallback authCallback = (NSError error, SPTSession session) => {

				if (error != null) {
					var alert = new UIAlertView("Fehler","Beim Anmelden ist ein Fehler aufgetreten: "+error.LocalizedDescription,null,"Ok",null);
					alert.Show();
					alert.Dismissed += (object sender, UIButtonEventArgs e) => {
						alert.Dispose();
					};
					CurrentPlayer.Current.DismissAuthenticationViewControllerWithAuth();
					return;
				}
				auth.Session = session;
				CurrentPlayer.Current.SessionDisabled = false;
				CurrentPlayer.Current.DismissAuthenticationViewControllerWithAuth();
				NSNotificationCenter.DefaultCenter.PostNotificationName("sessionUpdated", this);
			};

			/*
    			 Handle the callback from the authentication service. -[SPAuth -canHandleURL:]
     			 helps us filter out URLs that aren't authentication URLs (i.e., URLs you use elsewhere in your application).
    		 */

			if (auth.CanHandleURL(url)) {
				auth.HandleAuthCallbackWithTriggeredAuthURL (url, authCallback);
				return true;
			}
			// link to a book was sent...open this book.
			if (url.AbsoluteString.StartsWith (ConfigSpookify.UriPlayBook)) {

				if (url.AbsoluteString.Length > ConfigSpookify.UriPlayBook.Length + 3) {
					string book = url.AbsoluteString.Substring (ConfigSpookify.UriPlayBook.Length + 3);
					var	thisBook = CurrentState.Current.Audiobooks.FirstOrDefault (a => string.Equals(a.Uri,book));
					if (thisBook != null) {
						CurrentState.Current.Audiobooks.Remove (thisBook);
						SelectBook (thisBook);
					} else {
						var playlistBook = CurrentAudiobooks.Current.User.Playlists.SelectMany (b => b.Books).FirstOrDefault (b => string.Equals (b.Uri, book));
						if (playlistBook != null)
							LoadBookTracksAsync(playlistBook.Uri, SelectBook);
						else
							LoadBookTracksAsync(book, SelectBook);
					}

				}
				return true;
			}
			return false;
		}
		void SelectBook(AudioBook thisBook)
		{
			if (thisBook != null) {
				CurrentState.Current.Audiobooks.Insert (0, thisBook);
				CurrentState.Current.CurrentAudioBook = thisBook;
				CurrentState.Current.StoreCurrent ();
				CurrentPlayer.Current.PlayCurrentAudioBook ();
				if (TabBarController != null) {
					TabBarController.SwitchToTab (0);
					var zuletztViewController = TabBarController.ViewControllers
						.Select (vc => (vc as UINavigationController)?.ViewControllers?.FirstOrDefault () as ZuletztViewController)
						.FirstOrDefault (vc => vc != null && vc is ZuletztViewController);
					if (zuletztViewController != null && zuletztViewController.IsViewLoaded && zuletztViewController.HoerbuchListeTableView != null) {
						zuletztViewController.HoerbuchListeTableView.ReloadData ();
					}
				}
			}
		}
		void LoadBookTracksAsync(string uri, Action<AudioBook> completionHandler)
		{
			if (uri != null) {
				var url = new NSUrl (uri);
				SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
				if (auth == null || auth.Session == null || !auth.Session.IsValid)
					return;
				var p = SPTRequest.SPTRequestHandlerProtocol;
				NSError errorOut;

				var nsUrlRequest = SPTAlbum.CreateRequestForAlbum (url, auth.Session.AccessToken, "DE", out errorOut);
				SPTRequestHandlerProtocol_Extensions.Callback (p, nsUrlRequest, (er, resp, jsonData) => {
					if (er != null) {
						return;
					}
					NSError nsError;
					var album = SPTAlbum.AlbumFromData (jsonData, resp, out nsError);
					if (album != null) {
						int kapitelNummer = 1;
						var newBook = new AudioBook () {
							Uri = album.Uri.AbsoluteString,
							Album = new AudioBookAlbum () { Name = album.Name }, 
							Tracks = album.FirstTrackPage.Items
								.Cast<SPTPartialTrack> ()
								.Where (pt => pt.IsPlayable)
								.Select (pt => new AudioBookTrack () { 
									Url = pt.GetUri ().AbsoluteString, 
									Name = pt.Name, 
									Duration = pt.Duration, 
									Index = kapitelNummer++
								})
								.ToList (),
							Authors = album.Artists.Cast<SPTPartialArtist> ().Select (a => new Author () {
								Name = a.Name,
								URI = a.Uri.AbsoluteString
							}).ToList (),
							LargestCoverURL = album.LargestCover.ImageURL.AbsoluteString,
							SmallestCoverURL = album.SmallestCover.ImageURL.AbsoluteString
						};
						LoadNextPageAsync (newBook, album.FirstTrackPage, auth, p, completionHandler);
					}
				});
			}
		}

		void LoadNextPageAsync(AudioBook newbook, SPTListPage page, SPTAuth auth, SPTRequestHandlerProtocol p, Action<AudioBook> completionHandler)
		{
			NSError errorOut, nsError;
			if (auth == null || auth.Session == null || !auth.Session.IsValid) {
				completionHandler (newbook);
				return;
			}
			
			if (page != null && page.HasNextPage) {
				var nsUrlRequest = page.CreateRequestForNextPageWithAccessToken (auth.Session.AccessToken, out errorOut);
				SPTRequestHandlerProtocol_Extensions.Callback (p, nsUrlRequest, (er1, resp1, jsonData1) => {
					var nextpage = SPTListPage.ListPageFromData (jsonData1, resp1, true, "", out nsError);
					if (nextpage != null) {
						int kapitelNummer = newbook.Tracks.Any () ? newbook.Tracks.Max (t => t.Index) + 1 : 0;
						newbook.Tracks.AddRange (nextpage.Items
							.Cast<SPTPartialTrack> ()
							.Where (pt => pt.IsPlayable)
							.Select (pt => new AudioBookTrack () { 
							Url = pt.GetUri ().AbsoluteString, 
							Name = pt.Name, 
							Duration = pt.Duration, 
							Index = kapitelNummer++
						})
							.ToList ());
						LoadNextPageAsync (newbook, nextpage, auth, p, completionHandler);
					}
					else
						completionHandler (newbook);
				});
			} else
				completionHandler (newbook);
		}

		public override void OnResignActivation (UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground (UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
			CurrentPlaylistsCache.Current.StoreCurrent();
			CurrentLRUCache.Current.StoreCurrent();

			if (CurrentAudiobooks.Current.IsComplete && CurrentAudiobooks.Current.HasPlaylists && CurrentAudiobooks.Current.User.Playlists.Count > 10)
				CurrentAudiobooks.Current.StoreCurrent ();
		}

		public override void WillEnterForeground (UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated (UIApplication application)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		}

		public override void WillTerminate (UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}
	}
}


