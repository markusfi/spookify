using System;
using SpotifySDK;
using UIKit;
using Foundation;

namespace Spookify
{

	public class MySPTAuthViewDelegate : SPTAuthViewDelegate {
		UIViewController viewController = null;
		public MySPTAuthViewDelegate(UIViewController vc)  {
			this.viewController = vc;
		}

		public PlayerViewController PlayerViewController { get { return this.viewController as PlayerViewController; } }

		public override void AuthenticationViewControllerDidCancelLogin (SPTAuthViewController authenticationViewController)
		{
			BeginInvokeOnMainThread (() => {
				new UIAlertView("Anmeldung","Die Anmeldung wurde abgerochen.",null,"OK").Show();
			});
		}

		public override void AuthenticationViewControllerFail (SPTAuthViewController authenticationViewController, NSError error)
		{
			BeginInvokeOnMainThread (() => {
				new UIAlertView("Fehler",error.LocalizedDescription + error.Description,null,"OK").Show();
			});
		}

		public override void AuthenticationViewControllerLogin (SPTAuthViewController authenticationViewController, SPTSession session)
		{
			if (this.viewController != null) {
				if (this.viewController is PlayerViewController) {					
					var playerViewController = this.PlayerViewController;

					// wir erlauben prinzipiell wieder das Anmelden am Player...
					CurrentPlayer.Current.SessionDisabled = false;

					playerViewController.CompleteAuthentication ();

				} else {
					new UIAlertView("Spookify","Login erfolgreich",null,"OK").Show();
				}
			}
		}
	}
}

