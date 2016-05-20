using System;
using System.Linq;
using UIKit;
using SpotifySDK;
using CoreFoundation;
using Foundation;
using System.Collections.Generic;
using AVFoundation;
using System.Threading.Tasks;
using MediaPlayer;
using CoreGraphics;
using MonoTouch.Dialog;
using CoreAnimation;

namespace Spookify
{
	public partial class PlayerViewController : UIViewController
	{
		public bool IsSessionValid { get { return CurrentPlayer.Current.IsSessionValid; } }
		public bool CanRenewSession { get { return CurrentPlayer.Current.CanRenewSession; } }
		public bool IsPlayerCreated { get {	return CurrentPlayer.Current.IsPlayerCreated; } }
		public SPTAudioStreamingController Player { get { return CurrentPlayer.Current.Player; } }
		public void PlayCurrentAudioBook() { CurrentPlayer.Current.PlayCurrentAudioBook (); }
		bool SavePosition(bool store = true) { return CurrentPlayer.Current.SavePosition(store); }

		public MiniPlayerView MiniPlayer { get; set; }
		public bool HasCloseButton { get { return MiniPlayer != null; } }

		public PlayerViewController (IntPtr handle) : base (handle)
		{
		}
					
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();	

			var volumeView = new MPVolumeView (this.Airplay.Frame);
			volumeView.ShowsRouteButton = true;
			volumeView.ShowsVolumeSlider = false;
			volumeView.SizeToFit();
			volumeView.TranslatesAutoresizingMaskIntoConstraints = false;
			volumeView.TintColor = UIColor.White;
			foreach (var wnd in volumeView.Subviews) {
				if (wnd is UIButton) {
					var img = (wnd as UIButton).CurrentImage.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
					volumeView.SetRouteButtonImage (img, UIControlState.Normal);
				}
			}
			this.Airplay.AddSubview (volumeView);
			this.Airplay.AddConstraint (NSLayoutConstraint.Create (volumeView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.Airplay, NSLayoutAttribute.Width, 1f, 0));
			this.Airplay.AddConstraint (NSLayoutConstraint.Create (volumeView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.Airplay, NSLayoutAttribute.Height, 1f, 0));
			this.Airplay.AddConstraint (NSLayoutConstraint.Create (volumeView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this.Airplay, NSLayoutAttribute.CenterX, 1f, 0));
			this.Airplay.AddConstraint (NSLayoutConstraint.Create (volumeView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this.Airplay, NSLayoutAttribute.CenterY, 1f, 0));

			foreach (var wnd in this.View.Subviews) {
				if (wnd is UIButton) {
					var img = (wnd as UIButton).CurrentImage.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
					wnd.TintColor = UIColor.White;
					(wnd as UIButton).SetImage(img, UIControlState.Normal);
				}
			}
			
			UIBarButtonItem rightButton = new UIBarButtonItem ();
			rightButton.Image = UIImage.FromBundle ("Kaset");
			rightButton.Clicked += PlaySettingsClicked;

			this.NavigationController.NavigationBar.BarStyle = UIBarStyle.BlackTranslucent;
			this.NavigationController.NavigationBar.BarTintColor = ConfigSpookify.BackgroundColor;
			this.NavigationController.NavigationBar.Translucent = false;
			this.NavigationController.NavigationBar.TintColor = ConfigSpookify.BartTintColor;

			this.NavigationItem.SetRightBarButtonItem (rightButton, false);

			if (HasCloseButton) {
				UIBarButtonItem leftButton = new UIBarButtonItem ();
				leftButton.Image = UIImage.FromBundle ("Close");
				leftButton.Clicked += DismissHandler;
				this.NavigationItem.SetLeftBarButtonItem (leftButton, false);
			}
				

			CurrentPlayer.Current.CreateSPTAudioStreamingDelegate (this);


			UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer (HandleTouchInImage);
			tapRecognizer.NumberOfTapsRequired = 1;
			this.AlbumImage.UserInteractionEnabled = true;
			this.AlbumImage.AddGestureRecognizer (tapRecognizer);
		}

		void DismissHandler (object sender, EventArgs e)
		{
			this.DismissViewController (true, null);
		}
	
		void HandleTouchInImage()
		{
			UIView.BeginAnimations (null);
			UIView.SetAnimationRepeatCount (1);
		
			UIView.SetAnimationDuration (0.5);
			this.AlbumImage.Layer.Transform = CATransform3D.MakeRotation(3.141f, 1.0f, 0.0f,0.0f);
			UIView.SetAnimationDuration (0.5);
			this.AlbumImage.Layer.Transform = CATransform3D.MakeRotation(0f, 0f, 0.0f,0.0f);

			UIView.CommitAnimations ();

			this.OnPlay(null);
		}

		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			SavePosition ();
		}
			
		void SkipTrack(int step)
		{
			if (SavePosition (false)) {
				if (CurrentState.Current.CurrentAudioBook.SkipTrack (step)) {
					CurrentState.Current.StoreCurrent ();
					PlayCurrentAudioBook ();
				}
			}
		}
		void SkipTime(double seconds)
		{
			if (SavePosition (false)) {
				if (CurrentState.Current.CurrentAudioBook.AdjustTime (seconds)) {
					CurrentState.Current.StoreCurrent ();
					PlayCurrentAudioBook ();
				}
			}
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (CurrentPlayer.Current.NeedToRenewSession)
				CurrentPlayer.Current.RenewSession ();
			
			DisplayAlbum ();
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		AudioBook displayedAudioBook = null; 




		void ShowNoConnection() {
			this.AlbumLabel.Text = "Keine Internetverbindung\nStreaming nicht möglich";
			this.PlayButton.SetImage (UIImage.FromBundle ("NoConnection").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.PlayButton.Hidden = true;
			this.AlbumImage.Image = UIImage.FromBundle ("NoConnectionTitle");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
		}
		void ShowPendingRenewSession() {
			this.AlbumLabel.Text = "Sitzung wird aufgebaut";
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.PlayButton.Hidden = true;
			this.AlbumImage.Image = null;
			this.AlbumImage.Hidden = true;
			this.ActivityIndicatorBackgroundView.Hidden = false;
			this.ActivityIndicatorBackgroundView.Layer.CornerRadius = 15f;
			this.ActivityIndicatorView.StartAnimating();
		}
		void ShowPendingLogin() {
			this.AlbumLabel.Text = "Anmeldung wird durchgeführt";
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.PlayButton.Hidden = true;
			this.AlbumImage.Image = null;
			this.AlbumImage.Hidden = true;
			this.ActivityIndicatorBackgroundView.Hidden = false;
			this.ActivityIndicatorBackgroundView.Layer.CornerRadius = 15f;
			this.ActivityIndicatorView.StartAnimating();
		}
		void ShowLoginToSpotify() {
			this.AlbumLabel.Text = "Bitte melde dich mit deinem\nPremium Spotify Account an";
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.PlayButton.Hidden = false;
			this.AlbumImage.Image = UIImage.FromBundle ("Spotify");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
		}
		void ShowSelectAudiobook() {
			this.AlbumLabel.Text = "Wähle ein Hörbuch aus einer\nder Playlisten oder Suche\nnach einem Titel oder Autor";
			this.AuthorLabel.Text = "";
			this.PlayButton.SetImage (UIImage.FromBundle ("Suche").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.AlbumImage.Image = UIImage.FromBundle ("Books");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
		}
		void DisplayCurrentAudiobook(AudioBook ab) {
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
			this.PlayButton.SetImage (this.Player.CurrentPlayButtonImage(), UIControlState.Normal);
			this.PlayButton.Hidden = false;

			if (ab != null &&
				ab.CurrentPosition != null &&
				ab.Tracks.Count () > ab.CurrentPosition.TrackIndex) 
			{
				var track = ab.Tracks.ElementAt (ab.CurrentPosition.TrackIndex);
				this.AlbumLabel.Text = ab.ToAlbumName ();

				// this.TrackLabel.Text = track.Name;
				this.ProgressBar.Hidden = track.Duration == 0.0;
				if (track.Duration != 0.0)
					this.ProgressBar.Progress = (float)(ab.CurrentPosition.PlaybackPosition / track.Duration);

				var gesamtBisEnde = ab.Tracks.Skip(ab.CurrentPosition.TrackIndex).Sum (t => t.Duration) - ab.CurrentPosition.PlaybackPosition;
				var tsBisEnde = TimeSpan.FromSeconds (gesamtBisEnde);
				var gesamtSeitAnfang = ab.Tracks.Take (ab.CurrentPosition.TrackIndex - 1).Sum (t => t.Duration) + ab.CurrentPosition.PlaybackPosition;
				var tsSeitAnfang = TimeSpan.FromSeconds (gesamtSeitAnfang);
				this.bisEndeBuchLabel.Text = tsBisEnde.ToLongTimeText () + " verbleiben";
				this.kapitelLabel.Text = string.Format ("Kapitel {0} von {1}", ab.CurrentPosition.TrackIndex+1, ab.Tracks.Count);
				var ts = TimeSpan.FromSeconds (ab.CurrentPosition.PlaybackPosition);
				this.seitStartKapitelLabel.Text = ts.ToMinutesText ();
				var tsToEnd = TimeSpan.FromSeconds (track.Duration).Subtract (ts);
				this.bisEndeKapitelLabel.Text = tsToEnd.ToMinutesText ();
			} else {
				this.bisEndeKapitelLabel.Text = "";
				this.bisEndeBuchLabel.Text = "";
				this.kapitelLabel.Text = "";
				this.seitStartKapitelLabel.Text = "";
			}
			if (ab != displayedAudioBook) { 
				this.AuthorLabel.Text = ab.ToAuthorName ();
				ab.SetLargeImage (this.AlbumImage);
				displayedAudioBook = ab;
			} 
		}
		public void DisplayAlbum()
		{
			if (!this.IsViewLoaded || this.View.Superview == null)
				return;
			
			var ab = CurrentState.Current.CurrentAudioBook;

			bool hasConnection = Reachability.RemoteHostStatus () != NetworkStatus.NotReachable;
			bool pendingLogin = CurrentPlayer.Current.TriggerWaitingForLogin;
			bool pendingRenewSession = CurrentPlayer.Current.TriggerWaitingForSessionRenew;
				
			bool hideControls = ab == null || !hasConnection || !this.IsPlayerCreated;
			this.SkipForward.Hidden = hideControls;
			this.SkipBackward.Hidden = hideControls;
			this.PrevTrack.Hidden = hideControls;
			this.NextTrack.Hidden = hideControls;
			this.Airplay.Hidden = hideControls;
			this.LesezeichenButton.Hidden = hideControls;

			if (!hasConnection || !this.IsPlayerCreated || ab == null || pendingLogin || pendingRenewSession) {
				this.AuthorLabel.Text = "";
				this.ProgressBar.Hidden = true;
				this.bisEndeKapitelLabel.Text = "";
				this.bisEndeBuchLabel.Text = "";
				this.kapitelLabel.Text = "";
				this.seitStartKapitelLabel.Text = "";
				displayedAudioBook = null;
				if (!hasConnection) {
					ShowNoConnection();
				}
				else if (pendingRenewSession) {
					ShowPendingRenewSession ();
				}
				else if (pendingLogin) {
					ShowPendingLogin ();
				}
				else if (!this.IsPlayerCreated) {
					if (this.IsSessionValid) {
						ShowPendingLogin ();
					} else if (this.CanRenewSession) {
						ShowPendingRenewSession ();
					} else {
						ShowLoginToSpotify();
					}
				} else {
					ShowSelectAudiobook();
				}
			} 
			else 
			{
				DisplayCurrentAudiobook (ab);
			}
		}
		public override bool CanBecomeFirstResponder {
			get {
				return true;
			}
		}
		public override void RemoteControlReceived (UIEvent theEvent)
		{
			UIEventSubtype rc = theEvent.Subtype;
			/*
			if (rc == UIEventSubtype.RemoteControlTogglePlayPause) {
				this.TogglePlaying ();
			} else if (rc == UIEventSubtype.RemoteControlPlay) {
				if (this.IsPlayerCreated)	
					this.Player.SetIsPlaying (true, (error) => {
				});
			} else if (rc == UIEventSubtype.RemoteControlPause) {
				if (this.IsPlayerCreated)
					this.Player.SetIsPlaying (false, (error) => {
				});
			}
			*/
		}

		partial void OnAddBookmark (UIKit.UIButton sender)
		{
			if (this.IsPlayerCreated) {
				var ab = CurrentState.Current.CurrentAudioBook;
				if (ab != null && ab.CurrentPosition != null) {
					var alertView = new UIAlertView("Lesezeichen","Lesezeichen hinzufügen?", null, "Abbrechen", "Ok");
					alertView.Show();
					alertView.Clicked += (object sender1, UIButtonEventArgs e) => {
						if (e.ButtonIndex == 1) {;	
							ab.AddBookmark(new AudioBookBookmark(ab.CurrentPosition));
							CurrentState.Current.StoreCurrent();
						}
					};
				}
			}
		}
		void PlaySettingsClicked(object sender, EventArgs args) 
		{
			new PlayerViewSettings(this, this.MiniPlayer).PlaySettingsClicked(sender, args);
		}
		partial void OnPlay (UIKit.UIButton sender)
		{
			bool hasConnection = Reachability.RemoteHostStatus () != NetworkStatus.NotReachable;
			if (hasConnection) {
				if (!this.IsPlayerCreated) {
					if (this.IsSessionValid) {
						var dummy = this.Player;
					}
					else if (this.CanRenewSession) {
						CurrentPlayer.Current.RenewSession();
					}
					else {
						OpenLoginPage();
					}
				}
				if (this.IsPlayerCreated)
				{
					if (CurrentState.Current.Audiobooks.Count == 0)
						SwitchTab(2);
					else if (CurrentState.Current.CurrentAudioBook == null)
						SwitchTab(0);
					else  {
						if (Reachability.RemoteHostStatus () != NetworkStatus.NotReachable) {
							if ((!this.Player.IsPlaying ||
								(this.Player.CurrentTrackURI != null && this.Player.CurrentTrackURI.AbsoluteString != CurrentState.Current.CurrentTrackURI))) 
								PlayCurrentAudioBook ();
							else 
								TogglePlaying();
						}
					}
				}
			}
		}

		public void SwitchTab(int tab)
		{
			this.TabBarController.SwitchToTab (tab);
			if (this.HasCloseButton)
				DismissHandler (this, EventArgs.Empty);
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		void OpenLoginPage()
		{
			this.AlbumLabel.Text = @"Logging in...";
			CurrentPlayer.Current.OpenLoginPage (this);
		}

		void TogglePlaying()
		{
			if (this.IsPlayerCreated)
				this.Player.SetIsPlaying(!this.Player.IsPlaying, (error) => { });
		}	

		partial void OnPrevTrack (UIKit.UIButton sender)
		{
			SkipTrack(-1);
		}
		partial void OnNextTrack (UIKit.UIButton sender)
		{
			SkipTrack(1);
		}
		partial void OnBackTime (UIKit.UIButton sender)
		{
			SkipTime(-30);
		}
		partial void OnForwardTime (UIKit.UIButton sender)
		{
			SkipTime(30);
		}

	}
}

