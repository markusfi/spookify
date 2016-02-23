using System;

using UIKit;
using SpotifySDK;
using Foundation;
// using SystemConfiguration;
// using AudioToolbox;

namespace Simple
{
	public partial class ViewController : UIViewController
	{
		bool firstLoad = false;
		SPTAuthViewController authViewController = null;
		MySPTAuthViewDelegate mySPTAuthViewDelegate = null;

		public ViewController(IntPtr handle) : base(handle)
		{
		}

		/*
		public static bool IsHostReachable(string host)
		{
			if (string.IsNullOrEmpty(host))
				return false;

			using (var r = new NetworkReachability(host))
			{
				NetworkReachabilityFlags flags;

				if (r.TryGetFlags(out flags)) // Hangs here
				{
					return IsReachableWithoutRequiringConnection(flags);
				}
			}
			var path = new NSBundle("com.apple.UIKit").PathForResource("Tock","aiff");
			var ss = new SystemSound(new NSUrl(path));
			ss.PlaySystemSound();
			return false;
		}
		static bool  IsReachableWithoutRequiringConnection(NetworkReachabilityFlags flags)
		{
			return (flags & NetworkReachabilityFlags.Reachable) != 0;
		}
*/
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.mySPTAuthViewDelegate = new MySPTAuthViewDelegate (this);
			this.statusLabel.Text = "";
			this.firstLoad = true;

			NSNotificationCenter.DefaultCenter.AddObserver (new NSString("sessionUpdated"), (notification) => { 
				this.statusLabel.Text = @"";
				if(this.NavigationController.TopViewController == this) {
					SPTAuth auth = SPTAuth.DefaultInstance;
					if (auth.Session != null && auth.Session.IsValid) {
						this.PerformSegue("ShowPlayer",null);
					}
				}
			} ); 
			{
				SPTAuth auth = SPTAuth.DefaultInstance;
				if (auth.Session != null && auth.Session.IsValid) {
					this.PerformSegue ("ShowPlayer", null);
				}
			}
		}
		public void ShowPlayer() {
			firstLoad = false;
			this.PerformSegue("ShowPlayer",null);

		}
		public class MySPTAuthViewDelegate : SPTAuthViewDelegate {
			ViewController viewController = null;
			public MySPTAuthViewDelegate(ViewController vc)  {
				this.viewController = vc;
			}

			#region implemented abstract members of SPTAuthViewDelegate

			public override void AuthenticationViewControllerDidCancelLogin (SPTAuthViewController authenticationViewController)
			{
				this.viewController.statusLabel.Text = "Login abgebrochen";
			}

			public override void AuthenticationViewControllerFail (SPTAuthViewController authenticationViewController, NSError error)
			{
				this.viewController.statusLabel.Text = "Login Fehler";
			}

			public override void AuthenticationViewControllerLogin (SPTAuthViewController authenticationViewController, SPTSession session)
			{
				this.viewController.statusLabel.Text = "";
				this.viewController.ShowPlayer ();
			}

			#endregion
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		partial void OnButtonPressed (UIButton sender)
		{
			OpenLoginPage();
		}

		void OpenLoginPage()
		{
			this.statusLabel.Text = @"Logging in...";

			this.authViewController = SPTAuthViewController.AuthenticationViewController;
			this.authViewController.Delegate = this.mySPTAuthViewDelegate;
			this.authViewController.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
			this.authViewController.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;

			this.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
			this.DefinesPresentationContext = true;

			this.PresentViewController (this.authViewController, false, null);
		}

		partial void OnClearCookies (UIButton sender)
		{
			this.authViewController = SPTAuthViewController.AuthenticationViewController;
			this.authViewController.ClearCookies(() => {});
			this.statusLabel.Text  = @"Cookies cleared.";
		}
	}
}

