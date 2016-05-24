using System;
using UIKit;
using CoreGraphics;
using System.Linq;
using SpotifySDK;
using MediaPlayer;
using AVFoundation;


namespace Spookify
{
	public class MiniPlayerView : UIView, ISleepTimerController
	{
		public bool IsSessionValid { get { return CurrentPlayer.Current.IsSessionValid; } }
		public bool CanRenewSession { get { return CurrentPlayer.Current.CanRenewSession; } }
		public bool IsPlayerCreated { get {	return CurrentPlayer.Current.IsPlayerCreated; } }
		public SPTAudioStreamingController Player { get { return CurrentPlayer.Current.Player; } }
		bool SavePosition(bool store = true) { return CurrentPlayer.Current.SavePosition(store); }
		public void PlayCurrentAudioBook() { CurrentPlayer.Current.PlayCurrentAudioBook (); }

		UIView touchView,touchPlayView;
		UIImageView playImage;
		UIImageView showImage;
		UITabBarController _tabbarController;
		UILabel albumNameLabel, authorNameLabel, centerLabel;
		UIActivityIndicatorView activityIndicatorView;

		PlayerViewSleepTimer _playerViewSleepTimer;
		public DateTime SleepTimerStartTime { get; set; } = DateTime.MinValue;
		public int SleepTimerOpion { get; set; } = 0;

		#region ISleepTimerController implementation

		int ISleepTimerController.SleepTimerOpion {
			get {
				return this.SleepTimerOpion;
			}
			set {
				this.SleepTimerOpion = value;
			}
		}

		void ISleepTimerController.DisplayAlbum ()
		{
			UpdateCurrentAudiobook ();
		}

		DateTime ISleepTimerController.SleepTimerStartTime {
			get {
				return this.SleepTimerStartTime;
			}
			set {
				this.SleepTimerStartTime = value;
			}
		}

		UIView ISleepTimerController.View {
			get {
				return this;
			}
		}

		#endregion

		public MiniPlayerView (UIView superView, UIViewController viewController)
		{
			_tabbarController = viewController.TabBarController;
			SetupView (superView, viewController);
			SetupRemoteControl ();
			SetupSleepTimer ();
		}

		void SetupSleepTimer ()
		{
			if (_playerViewSleepTimer == null)
				_playerViewSleepTimer = new PlayerViewSleepTimer (this);
			_playerViewSleepTimer.StartTimer ();
		}

		void SetupView (UIView superView, UIViewController viewController)
		{
			var tabBar = viewController.TabBarController.TabBar;
			var tabview = viewController.TabBarController.View;
			var tabFrame = tabBar.Frame;
			var frame = superView.Frame;
			Frame = new CGRect (frame.Left, frame.Top + frame.Height, frame.Width, 50);
			BackgroundColor = ConfigSpookify.BackgroundColorLight;
			this.AddSubview (albumNameLabel = new UILabel () {
				TextColor = UIColor.White,
				Font = UIFont.FromName ("HelveticaNeue", 12f),
				TranslatesAutoresizingMaskIntoConstraints = false,
				Lines = 0,
			});
			albumNameLabel.SetContentCompressionResistancePriority (750,UILayoutConstraintAxis.Vertical);
			this.AddSubview (authorNameLabel = new UILabel () {
				TextColor = UIColor.LightGray,
				Font = UIFont.FromName ("HelveticaNeue-Light", 12f),
				TranslatesAutoresizingMaskIntoConstraints = false,
			});
			this.AddSubview (centerLabel = new UILabel (new CGRect (55, 25, frame.Width - 110, 20)) {
				TextColor = UIColor.LightGray,
				Font = UIFont.FromName ("HelveticaNeue-Light", 12f),
				TranslatesAutoresizingMaskIntoConstraints = false,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap
			});
			authorNameLabel.SetContentCompressionResistancePriority (751,UILayoutConstraintAxis.Vertical);
			this.AddSubview (showImage = new UIImageView (new CGRect (15, 15, 20, 20)) {
				Image = UIImage.FromBundle ("Up").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate),
				TintColor = UIColor.White,
				UserInteractionEnabled = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			});
			this.AddSubview (playImage = new UIImageView (new CGRect (frame.Width - 15 - 20, 15, 20, 20)) {
				TintColor = UIColor.White,
				UserInteractionEnabled = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			});
			this.AddSubview (activityIndicatorView = new UIActivityIndicatorView (new CGRect (frame.Width - 15 - 20, 15, 20, 20)) {
				TintColor = UIColor.White,
				UserInteractionEnabled = true,
				HidesWhenStopped = true,
				Hidden = true
			});
			this.AddSubview (touchView = new UIView (new CGRect (0, 0, Frame.Width - 100, Frame.Height)) {
				UserInteractionEnabled = true,
			});
			this.AddSubview (touchPlayView = new UIView (new CGRect (Frame.Width - 50, 0, 50, Frame.Height)) {
				UserInteractionEnabled = true,
			});
			UpdateCurrentAudiobook ();
			showImage.AddGestureRecognizer (new UITapGestureRecognizer (ShowBigPlayer));
			playImage.AddGestureRecognizer (new UITapGestureRecognizer (TogglePlaying));
			touchPlayView.AddGestureRecognizer (new UITapGestureRecognizer (TogglePlaying));
			touchView.AddGestureRecognizer (new UITapGestureRecognizer (ShowBigPlayer));
			this.TranslatesAutoresizingMaskIntoConstraints = false;
			tabview.AddSubview (this);

			var c = new NSLayoutConstraint[] {
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, tabBar, NSLayoutAttribute.Leading, 1, 0),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, tabBar, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, tabBar, NSLayoutAttribute.Trailing, 1, 0),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 50),

				NSLayoutConstraint.Create(centerLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 55),
				NSLayoutConstraint.Create(centerLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create(centerLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1, -55),

				NSLayoutConstraint.Create(albumNameLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 55),
				NSLayoutConstraint.Create(albumNameLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 8),
				NSLayoutConstraint.Create(albumNameLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1, -55),

				NSLayoutConstraint.Create(authorNameLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, albumNameLabel, NSLayoutAttribute.Leading, 1, 0),
				NSLayoutConstraint.Create(authorNameLabel, NSLayoutAttribute.Top, NSLayoutRelation.GreaterThanOrEqual, albumNameLabel, NSLayoutAttribute.Bottom, 1, 2),
				NSLayoutConstraint.Create(authorNameLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, albumNameLabel, NSLayoutAttribute.Trailing, 1, 0),
				NSLayoutConstraint.Create(authorNameLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, -8),

				NSLayoutConstraint.Create(showImage, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 15),
				NSLayoutConstraint.Create(showImage, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create(showImage, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 20),
				NSLayoutConstraint.Create(showImage, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 20),

				NSLayoutConstraint.Create(playImage, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1, -15),
				NSLayoutConstraint.Create(playImage, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create(playImage, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 20),
				NSLayoutConstraint.Create(playImage, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 20),
			};

			tabview.AddConstraints (c);
		}

		void ShowNoConnection() {
			centerLabel.Text = "Keine Internetverbindung\nStreaming nicht möglich";
			playImage.Image = UIImage.FromBundle ("NoConnection").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
			playImage.Hidden = true;
			activityIndicatorView.StopAnimating ();
		}
		void ShowPendingRenewSession() {
			centerLabel.Text = "Sitzung wird aufgebaut";
			playImage.Image = UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
			playImage.Hidden = true;
			activityIndicatorView.StartAnimating();
		}
		void ShowPendingLogin() {
			centerLabel.Text = "Anmeldung wird durchgeführt";
			playImage.Image = UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
			playImage.Hidden = true;
			activityIndicatorView.StartAnimating();
		}
		void ShowLoginToSpotify() {
			centerLabel.Text = "Bitte melde dich mit deinem\nPremium Spotify Account an";
			playImage.Image = UIImage.FromBundle ("NotLoggedIn").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
			playImage.Hidden = false;
			activityIndicatorView.StopAnimating ();
		}
		void ShowSelectAudiobook() {
			centerLabel.Text = "Wähle ein Hörbuch aus einer\nder Playlisten oder Suche\nnach einem Titel oder Autor";
			playImage.Image = UIImage.FromBundle ("Suche").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
			activityIndicatorView.StopAnimating ();
		}
		void UpdateCurrentAudiobook()
		{
			var ab = CurrentState.Current.CurrentAudioBook;

			bool hasConnection = Reachability.RemoteHostStatus () != NetworkStatus.NotReachable;
			bool pendingLogin = CurrentPlayer.Current.TriggerWaitingForLogin;
			bool pendingRenewSession = CurrentPlayer.Current.TriggerWaitingForSessionRenew;

			if (!hasConnection || !this.IsPlayerCreated || ab == null || pendingLogin || pendingRenewSession) {
				albumNameLabel.Text = "";
				authorNameLabel.Text = "";
				centerLabel.Text = "";
				centerLabel.Hidden = false;
				playImage.Image = null;
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
						CurrentPlayer.Current.ResetPlayer ();
						var dummy = this.Player;
					} else if (this.CanRenewSession) {
						ShowPendingRenewSession ();
						CurrentPlayer.Current.RenewSession ();
					} else {
						ShowLoginToSpotify();
					}
				} else {
					ShowSelectAudiobook();
				}
			} else {
				SavePosition ();
				centerLabel.Hidden = true;
				centerLabel.Text = "";
				activityIndicatorView.StopAnimating ();
				playImage.Hidden = false;
				playImage.Image = Player.CurrentPlayButtonImage ();
				albumNameLabel.Text = ab?.ToAlbumName ();
				if (ab != null) {
					var gesamtSeitAnfang = ab.Tracks.Sum (t => t.Duration);
					var tsSeitAnfang = TimeSpan.FromSeconds (gesamtSeitAnfang);
					if (!ab.Started && !ab.Finished) {
						authorNameLabel.Text = ab?.ToAuthorName () + " - " + ab?.TimeToEnd ();
					} else if (ab.Finished) {
						authorNameLabel.Text = ab?.ToAuthorName () + " - Beendet";
					} else if (ab.CurrentPosition != null) {
						authorNameLabel.Text = ab?.ToAuthorName () + " - " + ab?.TimeToEnd () + " verbleiben";
					} else {
						authorNameLabel.Text = ab?.ToAuthorName () + " - " + ab?.TimeToEnd ();
					}
				}

				SetupNowPlaying ();
			}
			if (BigPlayer != null) {
				BigPlayer?.DisplayAlbum ();
			}
		}

		void SetupNowPlaying()
		{
			var ab = CurrentState.Current.CurrentAudioBook;
			if (ab != null) {
				MPNowPlayingInfoCenter info = MPNowPlayingInfoCenter.DefaultCenter;

				var nowPlaying = new MPNowPlayingInfo ();
				if (nowPlaying != null) {
					// nowPlaying.DiscCount = 1;
					// nowPlaying.DiscNumber = 0;
					nowPlaying.PlaybackRate = 1.0;
					nowPlaying.AlbumTitle = ab.ToAlbumName ();
					nowPlaying.Artist = ab.ToAuthorName ();

					nowPlaying.AlbumTrackNumber = 0;
					nowPlaying.ElapsedPlaybackTime = 0;
					nowPlaying.PlaybackDuration = 0;

					if (ab.Tracks != null) {

						// var gesamtBisEnde = ab.Tracks.Skip(ab.CurrentPosition?.TrackIndex ?? 0).Sum (t => t.Duration) - (ab.CurrentPosition?.PlaybackPosition ?? 0.0);
						var gesamtSeitAnfang = ab.Tracks.Take ((ab.CurrentPosition?.TrackIndex ?? 1) - 1).Sum (t => t.Duration) + (ab.CurrentPosition?.PlaybackPosition ?? 0.0);
						var gesamtBisEnde = ab.Tracks.Sum (t => t.Duration) - gesamtSeitAnfang;

						nowPlaying.AlbumTrackCount = 1; // ab.Tracks.Count;
						if (ab.CurrentPosition != null && 
							ab.Tracks != null && 
							ab.CurrentPosition.TrackIndex < ab.Tracks.Count) 
						{
							var track = ab.Tracks.ElementAt (ab.CurrentPosition.TrackIndex);
							nowPlaying.Title = track.Name;
							nowPlaying.AlbumTrackNumber = 1; // ab.CurrentPosition.TrackIndex+1;
							nowPlaying.ElapsedPlaybackTime = gesamtSeitAnfang; //ab.CurrentPosition.PlaybackPosition;
							nowPlaying.PlaybackDuration = gesamtBisEnde; // track.Duration;
						}
					}

					if (BigPlayer?.AlbumImage?.Image != null) 
						nowPlaying.Artwork = new MPMediaItemArtwork (BigPlayer.AlbumImage.Image);
					info.NowPlaying = nowPlaying;
				}
			}
		}

		void SetupRemoteControl()
		{
			AVAudioSession sharedInstance = AVAudioSession.SharedInstance();
			sharedInstance.SetCategory (AVAudioSessionCategory.Playback);
			sharedInstance.SetActive (true);
			this.BecomeFirstResponder ();
			UIApplication.SharedApplication.BeginReceivingRemoteControlEvents ();
			MPRemoteCommandCenter commandCenter = MPRemoteCommandCenter.Shared;

			var skipBackwardIntervalCommand = commandCenter.SkipBackwardCommand;
			skipBackwardIntervalCommand.Enabled = true;
			skipBackwardIntervalCommand.PreferredIntervals = new [] { 30.0, 60.0 };
			skipBackwardIntervalCommand.AddTarget( (remoteCommand) =>  { 
				var cc = remoteCommand as MPSkipIntervalCommandEvent;
				return this.SkipTime(-(cc?.Interval ?? 30.0)) ? MPRemoteCommandHandlerStatus.Success : MPRemoteCommandHandlerStatus.CommandFailed;
			});
			MPSkipIntervalCommand skipForwardIntervalCommand = commandCenter.SkipForwardCommand;
			skipForwardIntervalCommand.Enabled = true;
			skipForwardIntervalCommand.PreferredIntervals = new [] { 30.0, 60.0 };
			skipForwardIntervalCommand.AddTarget( (remoteCommand) => { 
				var cc = remoteCommand as MPSkipIntervalCommandEvent;
				return this.SkipTime(cc?.Interval ?? 30.0) ? MPRemoteCommandHandlerStatus.Success : MPRemoteCommandHandlerStatus.CommandFailed;
			});
			commandCenter.TogglePlayPauseCommand.Enabled = true;
			commandCenter.TogglePlayPauseCommand.AddTarget( (remoteCommand) => { 
				return this.TogglePlaying(null) ? MPRemoteCommandHandlerStatus.Success : MPRemoteCommandHandlerStatus.CommandFailed;
			});
			commandCenter.PlayCommand.Enabled = true;
			commandCenter.PlayCommand.AddTarget( (remoteCommand) => { 
				return this.TogglePlaying(true) ? MPRemoteCommandHandlerStatus.Success : MPRemoteCommandHandlerStatus.CommandFailed;
			});
			commandCenter.StopCommand.Enabled = true;
			commandCenter.StopCommand.AddTarget( (remoteCommand) => { 
				return this.TogglePlaying(false) ? MPRemoteCommandHandlerStatus.Success : MPRemoteCommandHandlerStatus.CommandFailed;
			});
			commandCenter.PauseCommand.Enabled = true;
			commandCenter.PauseCommand.AddTarget( (remoteCommand) =>  { 
				return this.TogglePlaying(false) ? MPRemoteCommandHandlerStatus.Success : MPRemoteCommandHandlerStatus.CommandFailed;
			});
			commandCenter.NextTrackCommand.Enabled = true;
			commandCenter.NextTrackCommand.AddTarget( (remoteCommand) => { 
				return this.SkipTrack(1) ? MPRemoteCommandHandlerStatus.Success : MPRemoteCommandHandlerStatus.CommandFailed;
			});
			commandCenter.PreviousTrackCommand.Enabled = true;
			commandCenter.PreviousTrackCommand.AddTarget( (remoteCommand) =>  { 
				return this.SkipTrack(-1) ? MPRemoteCommandHandlerStatus.Success : MPRemoteCommandHandlerStatus.CommandFailed;
			});
			/* Bookmark Command: should be enabled only when configured in settings.
			var bc = commandCenter.BookmarkCommand;
			bc.Enabled = true;
			bc.AddTarget ((remoteCommand) => {				
				var ab = CurrentState.Current.CurrentAudioBook;
				if (ab == null || ab.CurrentPosition == null)
					return MPRemoteCommandHandlerStatus.CommandFailed;
				ab.AddBookmark(new AudioBookBookmark(ab.CurrentPosition));
				return MPRemoteCommandHandlerStatus.Success;
			});
			*/
		}

		UINavigationController _navCon = null;
		PlayerViewController BigPlayer {
			get {
				if (_navCon == null)
					_navCon = _tabbarController.SelectedViewController.Storyboard.InstantiateViewController("Player") as UINavigationController;
				if (_navCon != null) 
					return _navCon.TopViewController as PlayerViewController;
				return null;
			}
		}
		void ShowBigPlayer()
		{
			{
				var vc = BigPlayer;
				if (vc != null) {
					vc.MiniPlayer = this;
					_tabbarController.SelectedViewController.PresentViewController (_navCon, true, () => {
						
					});
				}
			}		
		}

		bool EnsurePlayer()
		{
			bool hasConnection = Reachability.RemoteHostStatus () != NetworkStatus.NotReachable;
			if (hasConnection) {
				if (!this.IsPlayerCreated) {
					if (this.IsSessionValid) {
						var dummy = this.Player;
					} else if (this.CanRenewSession) {
						CurrentPlayer.Current.RenewSession ();
					} else {
						CurrentPlayer.Current.OpenLoginPage (_tabbarController.SelectedViewController);
					}
				}
				return this.IsPlayerCreated;
			}
			return false;
		}

		public void TogglePlaying() {
			TogglePlaying(null);
		}
		public bool TogglePlaying(bool? play)
		{
			if (EnsurePlayer ()) {
				bool shouldPlay = play.HasValue ? play.Value : !Player.IsPlaying;
				bool wrongTrack = Player?.CurrentTrackURI?.AbsoluteString != CurrentState.Current.CurrentTrackURI;
				if ((this.Player.TrackListSize == 0 || wrongTrack) && shouldPlay) {
					CurrentPlayer.Current.PlayCurrentAudioBook ();
				} else {
					Player.SetIsPlaying (shouldPlay, (err) => {
						InvokeOnMainThread (UpdateCurrentAudiobook);
					});
				}
				return true;
			}
			return false;
		}

		public bool SkipTrack(int step)
		{
			if (EnsurePlayer()) {
				if (SavePosition (false)) {
					if (CurrentState.Current.CurrentAudioBook.SkipTrack (step)) {
						CurrentState.Current.StoreCurrent ();
						PlayCurrentAudioBook ();
					}
				}
				return true;
			}
			return false;
		}
		public bool SkipTime(double seconds)
		{
			if (EnsurePlayer()) {
				if (SavePosition (false)) {
					if (CurrentState.Current.CurrentAudioBook.AdjustTime (seconds)) {
						CurrentState.Current.StoreCurrent ();
						PlayCurrentAudioBook ();
					}
				}
				return true;
			}
			return false;
		}
	}
}

