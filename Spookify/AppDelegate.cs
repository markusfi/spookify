using Foundation;
using UIKit;
using SpotifySDK;
using CoreFoundation;

namespace Spookify
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window {
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method

			// Code to start the Xamarin Test Cloud Agent
			#if ENABLE_TEST_CLOUD
			// Xamarin.Calabash.Start();
			#endif

			// Set up shared authentication information
			SPTAuth auth = SPTAuth.GetDefaultInstance();

			auth.ClientID = ConfigSpotify.kClientId;
			auth.RequestedScopes = new[]{ Constants.SPTAuthUserLibraryReadScope };
			auth.RedirectURL = new NSUrl(ConfigSpotify.kCallbackURL);

			if (!string.IsNullOrEmpty(ConfigSpotify.kTokenSwapServiceURL))
				auth.TokenSwapURL = new NSUrl(ConfigSpotify.kTokenSwapServiceURL);

			if (!string.IsNullOrEmpty(ConfigSpotify.kTokenRefreshServiceURL))
				auth.TokenRefreshURL = new NSUrl(ConfigSpotify.kTokenRefreshServiceURL);
			auth.SessionUserDefaultsKey = ConfigSpotify.kSessionUserDefaultsKey;

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
			SPTAuth auth = SPTAuth.DefaultInstance;

			SPTAuthCallback authCallback = (NSError error, SPTSession session) => {

				if (error != null) {
					System.Diagnostics.Debug.WriteLine("*** Auth error: {0}", error);
					return;
				}
				auth.Session = session;
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

			return false;
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


