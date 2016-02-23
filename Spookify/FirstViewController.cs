using System;

using UIKit;
using Foundation;
using SpotifySDK;

namespace Spookify
{
	public partial class FirstViewController : UIViewController
	{
		public FirstViewController (IntPtr handle) : base (handle)
		{
		}

		SPTAuthViewController authViewController = null;
		MySPTAuthViewDelegate mySPTAuthViewDelegate = null;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.mySPTAuthViewDelegate = new MySPTAuthViewDelegate (this);
			this.statusLabel.Text = "";

			NSNotificationCenter.DefaultCenter.AddObserver (new NSString("sessionUpdated"), (notification) => { 
				this.statusLabel.Text = @"";
				if(this.NavigationController.TopViewController == this) {
					SPTAuth auth = SPTAuth.DefaultInstance;
					if (auth.Session != null && auth.Session.IsValid) {
						//this.PerformSegue("ShowPlayer",null);
					}
				}
			} ); 
			{
				SPTAuth auth = SPTAuth.DefaultInstance;
				if (auth.Session != null && auth.Session.IsValid) {
					//this.PerformSegue ("ShowPlayer", null);
				}
			}
		}
		public void ShowPlayer() {
			var tabBarController = this.TabBarController;

			UIView fromView = tabBarController.SelectedViewController.View;
			UIView toView = tabBarController.ViewControllers [1].View;

			UIView.Transition (fromView, toView, 0.5, UIViewAnimationOptions.CurveEaseInOut, () => {
				tabBarController.SelectedIndex = 1;
			});
		}
		public class MySPTAuthViewDelegate : SPTAuthViewDelegate {
			FirstViewController viewController = null;
			public MySPTAuthViewDelegate(FirstViewController vc)  {
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

		public bool IsSessionValid {
			get {
				SPTAuth auth = SPTAuth.DefaultInstance;
				return !(auth.Session == null || !auth.Session.IsValid);
			}
		}

		void OpenLoginPage()
		{
			this.statusLabel.Text = @"Logging in...";

			this.authViewController = SPTAuthViewController.AuthenticationViewController;
			this.authViewController.HideSignup = false;
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

