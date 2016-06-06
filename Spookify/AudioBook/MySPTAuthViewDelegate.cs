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
					var playerViewController = this.viewController as PlayerViewController;

					// wir erlauben prinzipiell wieder das Anmelden am Player...
					CurrentPlayer.Current.SessionDisabled = false;

					playerViewController.DisplayAlbum ();

					if (CurrentState.Current.Audiobooks.Count == 0) {
						playerViewController.SwitchTab (2); // liste der Bücher in Playlists
					} else if (CurrentState.Current.CurrentAudioBook == null) {
						playerViewController.SwitchTab (0); // Liste der gewählten Bücher
					} else {
						playerViewController.SwitchTab (1);
						playerViewController.PlayCurrentAudioBook ();
					}
				} else {
					new UIAlertView("Spookify","Login erfolgreich",null,"OK").Show();
				}
			}
		}
	}
}

