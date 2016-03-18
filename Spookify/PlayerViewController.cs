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
		PlayerViewSleepTimer _playerViewSleepTimer;
		public bool IsSessionValid { get { return CurrentPlayer.Current.IsSessionValid; } }
		public bool CanRenewSession { get { return CurrentPlayer.Current.CanRenewSession; } }
		public bool IsPlayerCreated { get {	return CurrentPlayer.Current.IsPlayerCreated; } }
		public SPTAudioStreamingController Player { get { return CurrentPlayer.Current.Player; } }
		public void PlayCurrentAudioBook() { CurrentPlayer.Current.PlayCurrentAudioBook (); }
		bool SavePosition(bool store = true) { return CurrentPlayer.Current.SavePosition(store); }

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
			volumeView.TintColor = UIColor.DarkGray;
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

			// if (this.IsPlayerCreated) 							
			//	PlayCurrentAudioBook ();
			
			SetupRemoteControl ();


			UIBarButtonItem rightButton = new UIBarButtonItem ();
			rightButton.Image = UIImage.FromBundle ("Kaset");
			rightButton.Clicked += PlaySettingsClicked;

			this.NavigationController.NavigationBar.TintColor = UIColor.DarkGray;

			this.NavigationItem.SetRightBarButtonItem (rightButton, false);

			CurrentPlayer.Current.CreateSPTAudioStreamingDelegate (this);


			UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer (HandleTouchInImage);
			tapRecognizer.NumberOfTapsRequired = 1;
			this.AlbumImage.UserInteractionEnabled = true;
			this.AlbumImage.AddGestureRecognizer (tapRecognizer);


			if (_playerViewSleepTimer == null)
				_playerViewSleepTimer = new PlayerViewSleepTimer (this);
			_playerViewSleepTimer.StartTimer ();
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
			_playerViewSleepTimer.StopTimer ();
		}
			
		void SkipTrack(int step)
		{
			if (SavePosition (false)) {
				if (CurrentState.Current.CurrentAudioBook.SkipTrack (step)) {
					CurrentState.Current.StoreCurrentState ();
					PlayCurrentAudioBook ();
				}
			}
		}
		void SkipTime(double seconds)
		{
			if (SavePosition (false)) {
				if (CurrentState.Current.CurrentAudioBook.AdjustTime (seconds)) {
					CurrentState.Current.StoreCurrentState ();
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


		private static string CommonPrefix(string[] ss)
		{
			if (ss.Length == 0)
				return "";
			if (ss.Length == 1)
				return ss[0];

			int prefixLength = 0;

			foreach (char c in ss[0]) {
				foreach (string s in ss) {
					if (s.Length <= prefixLength || s[prefixLength] != c) {
						return ss[0].Substring(0, prefixLength);
					}
				}
				prefixLength++;
			}
			return ss[0]; // all strings identical
		}
		private static string RemoveDelimiter(string s)
		{
			if (string.IsNullOrEmpty (s))
				return s;
			int delimiterLength = 0; 
			while (delimiterLength < s.Length &&
			       ((char.IsPunctuation (s [delimiterLength]) || char.IsWhiteSpace (s [delimiterLength])))) {
				delimiterLength++;
			}
			return delimiterLength > 0 ? s.Substring (delimiterLength) : s;
		}

		void ShowNoConnection() {
			this.AlbumLabel.Text = "Keine Internetverbindung\nStreaming nicht möglich";
			this.PlayButton.SetImage (UIImage.FromBundle ("NoConnection"), UIControlState.Normal);
			this.AlbumImage.Image = UIImage.FromBundle ("NoConnectionTitle");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
		}
		void ShowPendingRenewSession() {
			this.AlbumLabel.Text = "Sitzung wird aufgebaut";
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn"), UIControlState.Normal);
			this.AlbumImage.Image = null;
			this.AlbumImage.Hidden = true;
			this.ActivityIndicatorBackgroundView.Hidden = false;
			this.ActivityIndicatorBackgroundView.Layer.CornerRadius = 15f;
			this.ActivityIndicatorView.StartAnimating();
		}
		void ShowPendingLogin() {
			this.AlbumLabel.Text = "Anmeldung wird durchgeführt";
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn"), UIControlState.Normal);
			this.AlbumImage.Image = null;
			this.AlbumImage.Hidden = true;
			this.ActivityIndicatorBackgroundView.Hidden = false;
			this.ActivityIndicatorBackgroundView.Layer.CornerRadius = 15f;
			this.ActivityIndicatorView.StartAnimating();
		}
		void ShowLoginToSpotify() {
			this.AlbumLabel.Text = "Bitte melde dich mit deinem\nPremium Spotify Account an";
			this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn"), UIControlState.Normal);
			this.AlbumImage.Image = UIImage.FromBundle ("Spotify");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
		}
		void ShowSelectAudiobook() {
			this.AlbumLabel.Text = "Wähle ein Hörbuch aus einer\nder Playlisten oder Suche\nnach einem Titel oder Autor";
			this.AuthorLabel.Text = "";
			this.PlayButton.SetImage (UIImage.FromBundle ("Suche"), UIControlState.Normal);
			this.AlbumImage.Image = UIImage.FromBundle ("Books");
			this.AlbumImage.Hidden = false;
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
		}
		void DisplayCurrentAudiobook(AudioBook ab) {
			this.ActivityIndicatorBackgroundView.Hidden = true;
			this.ActivityIndicatorView.StopAnimating ();
			this.PlayButton.SetImage (UIImage.FromBundle (this.Player.IsPlaying ? "Pause" : "Play"), UIControlState.Normal);

			var c = CurrentState.Current;
			if (c.CurrentAudioBook != null &&
				c.CurrentAudioBook.CurrentPosition != null &&
				ab.Tracks.Count () > c.CurrentAudioBook.CurrentPosition.TrackIndex) 
			{
				var track = ab.Tracks.ElementAt (c.CurrentAudioBook.CurrentPosition.TrackIndex);

				string displayString = ab.Album.Name;
				var prefix = CommonPrefix (new [] { track.Name, ab.Album.Name} );
				if (prefix != null && prefix.Length == ab.Album.Name.Length) {
					displayString = track.Name;
				} else if (prefix != null && prefix.Length > 5 && track.Name.Length > (prefix.Length + 1)) {
					displayString = ab.Album.Name + " - " + RemoveDelimiter(track.Name.Substring (prefix.Length));
				} else
					displayString = ab.Album.Name + " - " + track.Name;
				this.AlbumLabel.Text = displayString;

				// this.TrackLabel.Text = track.Name;
				this.ProgressBar.Hidden = track.Duration == 0.0;
				if (track.Duration != 0.0)
					this.ProgressBar.Progress = (float)(c.CurrentAudioBook.CurrentPosition.PlaybackPosition / track.Duration);

				var gesamtBisEnde = ab.Tracks.Skip(c.CurrentAudioBook.CurrentPosition.TrackIndex).Sum (t => t.Duration) - c.CurrentAudioBook.CurrentPosition.PlaybackPosition;
				var tsBisEnde = TimeSpan.FromSeconds (gesamtBisEnde);
				var gesamtSeitAnfang = ab.Tracks.Take (c.CurrentAudioBook.CurrentPosition.TrackIndex - 1).Sum (t => t.Duration) + c.CurrentAudioBook.CurrentPosition.PlaybackPosition;
				var tsSeitAnfang = TimeSpan.FromSeconds (gesamtSeitAnfang);

				if (Math.Truncate(tsBisEnde.TotalHours) > 1.0)
					this.bisEndeBuchLabel.Text = string.Format ("{0} Stunden {1:00} Minuten verbleiben", Math.Truncate (tsBisEnde.TotalHours), tsBisEnde.Minutes);
				else if (Math.Truncate(tsBisEnde.TotalHours) > 0.0)
					this.bisEndeBuchLabel.Text = string.Format ("{0} Stunde {1:00} Minuten verbleiben", Math.Truncate (tsBisEnde.TotalHours), tsBisEnde.Minutes);
				else 
					this.bisEndeBuchLabel.Text = string.Format ("{0} Minuten {1:00} Sekunden verbleiben",  Math.Truncate(tsBisEnde.TotalMinutes), tsBisEnde.Seconds);
				this.kapitelLabel.Text = string.Format ("Kapitel {0} von {1}", c.CurrentAudioBook.CurrentPosition.TrackIndex+1, ab.Tracks.Count);
				var ts = TimeSpan.FromSeconds (c.CurrentAudioBook.CurrentPosition.PlaybackPosition);
				this.seitStartKapitelLabel.Text = string.Format ("{0:00}:{1:00}", Math.Truncate(ts.TotalMinutes), ts.Seconds);
				var tsToEnd = TimeSpan.FromSeconds (track.Duration).Subtract (ts);
				this.bisEndeKapitelLabel.Text = string.Format ("{0:00}:{1:00}", Math.Truncate(tsToEnd.TotalMinutes), tsToEnd.Seconds);
			} else {
				this.bisEndeKapitelLabel.Text = "";
				this.bisEndeBuchLabel.Text = "";
				this.kapitelLabel.Text = "";
				this.seitStartKapitelLabel.Text = "";
			}
			if (ab != displayedAudioBook) { 
				this.AuthorLabel.Text = ab.Artists.FirstOrDefault ();
				ab.SetLargeImage (this.AlbumImage);
				displayedAudioBook = ab;
			} 
		}
		public void DisplayAlbum()
		{
			SavePosition ();

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
			} 
			else 
			{
				DisplayCurrentAudiobook (ab);
				SetupNowPlaying ();
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
		}
		void SetupRemoteControl()
		{
			AVAudioSession sharedInstance = AVAudioSession.SharedInstance();
			sharedInstance.SetCategory (AVAudioSessionCategory.Playback);
			sharedInstance.SetActive (true);
			this.BecomeFirstResponder ();
			UIApplication.SharedApplication.BeginReceivingRemoteControlEvents ();
			MPRemoteCommandCenter commandCenter = MPRemoteCommandCenter.Shared;

			MPSkipIntervalCommand skipBackwardIntervalCommand = commandCenter.SkipBackwardCommand;
			skipBackwardIntervalCommand.Enabled = true;
			skipBackwardIntervalCommand.PreferredIntervals = new [] { 30.0 };
			skipBackwardIntervalCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.IsPlayerCreated) {
						this.OnBackTime(null);
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			MPSkipIntervalCommand skipForwardIntervalCommand = commandCenter.SkipForwardCommand;
			skipForwardIntervalCommand.Enabled = true;
			skipForwardIntervalCommand.PreferredIntervals = new [] { 30.0 };
			skipForwardIntervalCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.IsPlayerCreated) {
						this.OnForwardTime(null);
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.TogglePlayPauseCommand.Enabled = true;
			commandCenter.TogglePlayPauseCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.IsPlayerCreated) {
						this.TogglePlaying();
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.PlayCommand.Enabled = true;
			commandCenter.PlayCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.IsPlayerCreated) {
						this.Player.SetIsPlaying(true, (error) => { });
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.StopCommand.Enabled = true;
			commandCenter.StopCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.IsPlayerCreated) {
						this.Player.SetIsPlaying(false, (error) => { });
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.PauseCommand.Enabled = true;
			commandCenter.PauseCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.IsPlayerCreated) {
						this.Player.SetIsPlaying(false, (error) => { });
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.SkipForwardCommand.Enabled = true;
			commandCenter.SkipForwardCommand.AddTarget( (remoteCommand) => 
			{ 
				if (this.IsPlayerCreated) {
					this.OnForwardTime(null);
					return MPRemoteCommandHandlerStatus.Success;
				} else 
					return MPRemoteCommandHandlerStatus.CommandFailed;
			});			
			commandCenter.SkipBackwardCommand.Enabled = true;
			commandCenter.SkipBackwardCommand.AddTarget( (remoteCommand) => 
			{ 
				if (this.IsPlayerCreated) {
					this.OnBackTime(null);
					return MPRemoteCommandHandlerStatus.Success;
				} else 
					return MPRemoteCommandHandlerStatus.CommandFailed;
			});
			commandCenter.NextTrackCommand.Enabled = true;
			commandCenter.NextTrackCommand.AddTarget( (remoteCommand) => 
			{ 
				if (this.IsPlayerCreated) {
					this.OnNextTrack(null);
					return MPRemoteCommandHandlerStatus.Success;
				} else 
					return MPRemoteCommandHandlerStatus.CommandFailed;
			});			
			commandCenter.PreviousTrackCommand.Enabled = true;
			commandCenter.PreviousTrackCommand.AddTarget( (remoteCommand) => 
			{ 
				if (this.IsPlayerCreated) {
					this.OnPrevTrack(null);
					return MPRemoteCommandHandlerStatus.Success;
				} else 
					return MPRemoteCommandHandlerStatus.CommandFailed;
			});
		}

		void SetupNowPlaying()
		{
			var ab = CurrentState.Current.CurrentAudioBook;
			if (ab != null) {
				MPNowPlayingInfoCenter info = MPNowPlayingInfoCenter.DefaultCenter;

				var nowPlaying = new MPNowPlayingInfo ();
				if (nowPlaying != null) {
					nowPlaying.DiscCount = 1;
					nowPlaying.DiscNumber = 0;
					nowPlaying.PlaybackRate = 1.0;
					nowPlaying.AlbumTitle = ab.Album.Name;
					nowPlaying.Artist = ab.Artists.FirstOrDefault ();

					nowPlaying.AlbumTrackNumber = 0;
					nowPlaying.ElapsedPlaybackTime = 0;

					if (ab.Tracks != null) {
						nowPlaying.ChapterCount = ab.Tracks.Count;
						nowPlaying.AlbumTrackCount = ab.Tracks.Count;
						if (ab.CurrentPosition != null && 
							ab.Tracks != null && 
							ab.CurrentPosition.TrackIndex < ab.Tracks.Count) {
							var track = ab.Tracks.ElementAt (ab.CurrentPosition.TrackIndex);
							nowPlaying.Title = track.Name;
							nowPlaying.AlbumTrackNumber = ab.CurrentPosition.TrackIndex;
							nowPlaying.ElapsedPlaybackTime = ab.CurrentPosition.PlaybackPosition;
							nowPlaying.PlaybackDuration = track.Duration;
							nowPlaying.ChapterNumber = ab.CurrentPosition.TrackIndex;
						}
					}

					if (this.AlbumImage.Image != null) 
						nowPlaying.Artwork = new MPMediaItemArtwork (this.AlbumImage.Image);
					info.NowPlaying = nowPlaying;
				}
			}
		}
			
		public DateTime SleepTimerStartTime { get; set; } = DateTime.MinValue;
		public int SleepTimerOpion = 0;

		partial void OnAddBookmark (UIKit.UIButton sender)
		{
			if (this.IsPlayerCreated) {
				var ab = CurrentState.Current.CurrentAudioBook;
				if (ab != null && ab.CurrentPosition != null) {
					var alertView = new UIAlertView("Lesezeichen","Lesezeichen hinzufügen?", null, "Abbrechen", "Ok");
					alertView.Show();
					alertView.Clicked += (object sender1, UIButtonEventArgs e) => {
						if (e.ButtonIndex == 1) {;	
							if (ab.Bookmarks == null)
								ab.Bookmarks = new List<AudioBookBookmark>();
							ab.Bookmarks.Add(new AudioBookBookmark(ab.CurrentPosition));
							CurrentState.Current.StoreCurrentState();
						}
					};
				}
			}
		}
		void PlaySettingsClicked(object sender, EventArgs args) 
		{
			new PlayerViewSettings(this).PlaySettingsClicked(sender, args);
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
			var tabBarController = this.TabBarController;

			UIView fromView = tabBarController.SelectedViewController.View;
			UIView toView = tabBarController.ViewControllers [tab].View;

			if (fromView != toView) {
				UIView.Transition (fromView, toView, 0.5, UIViewAnimationOptions.CurveEaseInOut, () => {
					tabBarController.SelectedIndex = tab;
				});
			}
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

