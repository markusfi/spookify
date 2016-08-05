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
					
		void ConfigureViews(UIView view, int level)
		{
			foreach (var wnd in view.Subviews) {
				Console.WriteLine ("UIView("+level+"): " + wnd.GetType().Name + " " + ((wnd is UIButton) ? wnd.Tag.ToString() : ((wnd is UILabel) ? (wnd as UILabel).Text : "" )));
				if (wnd is UIButton) {
					var image = (wnd as UIButton).CurrentImage;
					if (image != null)
					{
						var img = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
						wnd.TintColor = UIColor.White;
						(wnd as UIButton).SetImage(img, UIControlState.Normal);
					}
				}
				if (wnd is UILabel)
					(wnd as UILabel).Text = "";
				if (level == 1 && wnd is UILabel)
					wnd.Hidden = true;
				if (wnd.Subviews.Any())
					ConfigureViews (wnd, level+1);
			}
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

			UIBarButtonItem rightButton = new UIBarButtonItem ();
			rightButton.Image = UIImage.FromBundle ("Kaset");
			rightButton.Clicked += PlaySettingsClicked;

			if (this.NavigationController != null && this.NavigationController.NavigationItem != null) {
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
			}

			this.PlayButton.Layer.BorderColor = UIColor.White.CGColor;
			this.PlayButton.Layer.BorderWidth = 1f;
			this.PlayButton.Layer.CornerRadius = 45f/2.0f;

			CurrentPlayer.Current.CreateSPTAudioStreamingDelegate (this);

			UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer (HandleTouchInImage);
			tapRecognizer.NumberOfTapsRequired = 1;
			this.AlbumImage.UserInteractionEnabled = true;
			this.AlbumImage.AddGestureRecognizer (tapRecognizer);

			this.SleepTimerLabel.UserInteractionEnabled = true;
			this.SleepTimerLabel.AddGestureRecognizer (new UITapGestureRecognizer (() => OnSleeptimer(null)));
		}
		bool subViewsConfigured = false;
		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();
			// call this FIRST here, so the View is already ready for display without any updates...
			if (!subViewsConfigured) {
				subViewsConfigured = true;
				ConfigureViews (this.View, 1);
			}
		}
		partial void OnCloseButtonClicked (UIKit.UIButton sender) {
			this.DismissViewController (true, null);
		}
		void DismissHandler (object sender, EventArgs e)
		{
			this.DismissViewController (true, null);
		}
	
		void HandleTouchInImage(UITapGestureRecognizer tapRecognizer)
		{
			CGPoint touchPoint = tapRecognizer.LocationInView (this.View);
			if (touchPoint.Y < this.View.Frame.Height / 3) {
				if (touchPoint.X < this.View.Frame.Width / 3) {
					this.DismissViewController (true, null);
					return;
				}
				if (touchPoint.X > this.View.Frame.Width * 2 / 3) {
					return;
				}
			}			
			UIView.BeginAnimations (null);
			UIView.SetAnimationRepeatCount (1);
		
			UIView.SetAnimationDuration (0.5);
			this.AlbumImage.Layer.Transform = CATransform3D.MakeRotation (3.141f, 1.0f, 0.0f, 0.0f);
			UIView.SetAnimationDuration (0.5);
			this.AlbumImage.Layer.Transform = CATransform3D.MakeRotation (0f, 0f, 0.0f, 0.0f);

			UIView.CommitAnimations ();

			this.OnPlay (null);
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
		public void UpdateGUI()
		{
			if (CurrentPlayer.Current.NeedToRenewSession)
				CurrentPlayer.Current.RenewSession ();

			DisplayAlbum ();
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			UpdateGUI ();
		}
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}
		public override bool PrefersStatusBarHidden()
		{
			return true;
		}
		public override void ViewWillDisappear (bool animated)
		{
			this.PresentingViewController.View.Hidden = false;
			base.ViewWillDisappear (animated);
		}
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			this.MiniPlayer.DisposeBigPlayer ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			CurrentPlayer.Current.RemoveSPTAudioStreamingDelegate ();
		}

		void ShowNoConnection() {
			this.AlbumLabel.Text = "Keine Internetverbindung\nStreaming nicht möglich";
			this.AlbumLabel.Hidden = false;
			this.PlayButton.SetImage (UIImage.FromBundle ("NoConnection").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.PlayButton.Hidden = true;
			this.AlbumImage.Image = UIImage.FromBundle ("NoConnectionTitle");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
			ShowOptionButtons (false);
			ShowProgress (false);
		}
		void ShowPendingRenewSession() {
			this.AlbumLabel.Text = "Sitzung wird aufgebaut";
			this.AlbumLabel.Hidden = false;
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.PlayButton.Hidden = true;
			this.AlbumImage.Image = null;
			this.AlbumImage.Hidden = true;
			this.ActivityIndicatorBackgroundView.Hidden = false;
			this.ActivityIndicatorBackgroundView.Layer.CornerRadius = 15f;
			this.ActivityIndicatorView.StartAnimating();
			ShowOptionButtons (false);
			ShowProgress (false);
		}
		void ShowPendingLogin() {
			this.AlbumLabel.Text = "Anmeldung wird durchgeführt";
			this.AlbumLabel.Hidden = false;
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.PlayButton.Hidden = true;
			this.AlbumImage.Image = null;
			this.AlbumImage.Hidden = true;
			this.ActivityIndicatorBackgroundView.Hidden = false;
			this.ActivityIndicatorBackgroundView.Layer.CornerRadius = 15f;
			this.ActivityIndicatorView.StartAnimating();
			ShowOptionButtons (false);
			ShowProgress (false);
		}
		void ShowLoginToSpotify() {
			this.AlbumLabel.Text = "Bitte melde dich mit deinem\nPremium Spotify Account an.";
			this.AlbumLabel.Hidden = false;
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.PlayButton.Hidden = false;
			this.AlbumImage.Image = UIImage.FromBundle ("Spotify");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();

			ShowOptionButtons (false);
			ShowProgress (false);
		}
		void ShowSelectAudiobook() {
			this.AlbumLabel.Text = "Wähle ein Hörbuch aus einer\nder Playlisten oder Suche\nnach einem Titel oder Autor";
			this.AlbumLabel.Hidden = false;
			this.PlayButton.SetImage (UIImage.FromBundle ("Suche").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
			this.AlbumImage.Image = UIImage.FromBundle ("Books");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
			ShowOptionButtons (false);
			ShowProgress (false);
		}
		void ShowProgress(bool show)
		{
			this.AuthorLabel.Hidden = 
			this.ProgressBar.Hidden =
			this.bisEndeKapitelLabel.Hidden = 
			this.bisEndeBuchLabel.Hidden = 
			this.kapitelLabel.Hidden = 
			this.seitStartKapitelLabel.Hidden = !show;
		}
		void ShowOptionButtons(bool show)
		{
			this.SendenButton.Hidden = 
			this.MoreButton.Hidden =
			this.SleepButton.Hidden = !show;
		}
			
		void DisplayCurrentAudiobook(AudioBook ab) {
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
			this.PlayButton.SetImage (this.Player.CurrentPlayButtonImage(), UIControlState.Normal);
			this.PlayButton.ImageEdgeInsets = this.Player.CurrentPlayButtonInset ();
			this.PlayButton.Hidden = false;

			if (this.MiniPlayer != null && this.MiniPlayer.SleepTimerOpion != 0) {
				this.SleepTimerLabel.Hidden = false;
				this.SleepTimerLabel.Text = string.Format (" {0:mm\\:ss}", this.MiniPlayer.SleepTimerStartTime.Subtract (DateTime.Now));
			}
			else 
				this.SleepTimerLabel.Hidden = true;

			if (ab != null &&
				ab.CurrentPosition != null &&
				ab.Tracks.Count () > ab.CurrentPosition.TrackIndex) 
			{
				ShowOptionButtons (true);
				ShowProgress (true);
				var track = ab.Tracks.ElementAt (ab.CurrentPosition.TrackIndex);
				this.AlbumLabel.Text = ab.ToAlbumName ();
				this.AlbumLabel.Hidden = false;
				this.AuthorLabel.Text = ab.ToAuthorName ();
				this.AuthorLabel.Hidden = false;

				// this.TrackLabel.Text = track.Name;
				this.ProgressBar.Hidden = track.Duration == 0.0;
				if (track.Duration != 0.0)
					this.ProgressBar.Progress = (float)(ab.CurrentPosition.PlaybackPosition / track.Duration);

				var tsBisEnde = TimeSpan.FromSeconds (ab.GesamtBisEnde - ab.GesamtSeitAnfang);
				this.bisEndeBuchLabel.Text = tsBisEnde.ToLongTimeText () + " verbleiben";
				this.kapitelLabel.Text = string.Format ("Kapitel {0} von {1}", ab.CurrentPosition.TrackIndex+1, ab.Tracks.Count);
				var ts = TimeSpan.FromSeconds (ab.CurrentPosition.PlaybackPosition);
				this.seitStartKapitelLabel.Text = ts.ToMinutesText ();
				var tsToEnd = TimeSpan.FromSeconds (track.Duration).Subtract (ts);
				this.bisEndeKapitelLabel.Text = tsToEnd.ToMinutesText ();

			} else {
				ShowOptionButtons (true);
				ShowProgress (false);
				this.bisEndeKapitelLabel.Text = "";
				this.bisEndeBuchLabel.Text = "";
				this.kapitelLabel.Text = "";
				this.seitStartKapitelLabel.Text = "";
			}
			if (ab != displayedAudioBook) { 
				ab.SetLargeImage (this.AlbumImage);
				displayedAudioBook = ab;
			} 
			if (displayImage != this.AlbumImage.Image && this.AlbumImage.Image != null) {
				displayImage = this.AlbumImage.Image;
				this.CloseButton.Layer.CornerRadius = this.CloseButton.Frame.Width / 2.0f;
				var backgroundColor = this.AlbumImage.Image.AverageColorDarkerAsRef(ConfigSpookify.BackgroundColorLight);
				this.CloseButton.BackgroundColor = backgroundColor; // ConfigSpookify.BackgroundColor;

				if (gradient != null)
					gradient.RemoveFromSuperLayer();
				gradient = new CAGradientLayer()
				{
					Frame = this.View.Frame,
					Colors = new[] { backgroundColor.CGColor, ConfigSpookify.BackgroundColor.CGColor },
					StartPoint = new CGPoint(0.0f, 0.0f),
					EndPoint = new CGPoint(0.0f, 1.0f)
				};
				SetGradientFrame();
				this.View.Layer.InsertSublayer(gradient, 0);
			}
		}
		void SetGradientFrame()
		{
			if (gradient != null)
			{
				if (this.AlbumImage != null)
					gradient.Frame = new CGRect(this.View.Frame.Left, this.View.Frame.Top + this.AlbumImage.Frame.Height, this.View.Frame.Width, this.View.Frame.Height - +this.AlbumImage.Frame.Height);
				else
					gradient.Frame = new CGRect(this.View.Frame.Left, this.View.Frame.Top + this.AlbumImage.Frame.Height, this.View.Frame.Width, this.View.Frame.Height - +this.AlbumImage.Frame.Height);
			}
		}
		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();
			SetGradientFrame();
		}
		CAGradientLayer gradient = null;
		AudioBook displayedAudioBook = null; 
		UIImage displayImage = null;

		public void DisplayAlbum()
		{
			if (!this.IsViewLoaded || this.View == null)
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
						if (e.ButtonIndex == 1) {
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

		partial void OnSendenButtonClicked (UIKit.UIButton sender)
		{
			this.SendAudiobook(CurrentState.Current.CurrentAudioBook, this.AlbumImage.Image);
		}

		partial void OnSleeptimer (UIKit.UIButton sender)
		{
			// Actionsheet mit Sleeptimer Optionen
			PlayerViewSleepTimer.ShowSleepTimerConfiguration(this, this.MiniPlayer, null);
		}

		partial void OnMoreButtonClicked (UIKit.UIButton sender)
		{
			ShowBookmarks(this, CurrentPlayer.Current.PlayCurrentAudioBook);
		}
		public static void ShowBookmarks(UIViewController presentingViewController, Action okHandler = null, Action cancelHandler = null)
		{
			UIAlertController ac = UIAlertController.Create("Lesezeichen","Springe direkt zu einem Lesezeichen",UIAlertControllerStyle.ActionSheet);
			var ab = CurrentState.Current.CurrentAudioBook;

			if (ab != null) {
				ac.AddAction(UIAlertAction.Create ("An den Anfang", UIAlertActionStyle.Default, (alertAction) => {
					ac.Dispose ();
					ab.CurrentPosition = new AudioBookBookmark ();
					if (okHandler != null)
						okHandler ();
				}));
			}
			if (ab != null && ab.Bookmarks != null && ab.Bookmarks.Count > 0) {
				for (int i = 0; i < ab.Bookmarks.Count; i++) {
					ac.AddAction (CreateBookmarkOption (i, ac, ab.Bookmarks.ElementAt (i), okHandler));
				}
			}
			ac.AddAction(UIAlertAction.Create ("Abbrechen", UIAlertActionStyle.Cancel, (alertAction) =>  {
				ac.Dispose ();
				if (cancelHandler != null)
					cancelHandler();
			}));
			presentingViewController.PresentViewController(ac, true, null);			
		}
		static UIAlertAction CreateBookmarkOption(int i, UIAlertController ac, AudioBookBookmark bookmark, Action okHandler)
		{
			var time = CurrentState.Current.CurrentAudioBook.GesamtSeit (bookmark);
			var ts = TimeSpan.FromSeconds (time);
			string actiontext = 
				string.Format ("Kapitel {0} - {1}",
					bookmark.TrackIndex + 1,
					ts.ToShortTimeText());
			var alertOption = UIAlertAction.Create(
				actiontext,
				UIAlertActionStyle.Default, (alertAction) => { 
					CurrentState.Current.CurrentAudioBook.CurrentPosition = bookmark;
					ac.Dispose();
					if (okHandler != null)
						okHandler();
				});
			return alertOption;
		}

		public void CompleteAuthentication ()
		{
			this.DisplayAlbum ();

			if (CurrentState.Current.Audiobooks.Count == 0) {
				this.SwitchTab (2); // liste der Bücher in Playlists
			} else if (CurrentState.Current.CurrentAudioBook == null) {
				this.SwitchTab (0); // Liste der gewählten Bücher
			} else {
				this.SwitchTab (1);
				this.PlayCurrentAudioBook ();
			}
		}
	}
}

