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

namespace Spookify
{
	public partial class PlayerViewController : UIViewController
	{
		PlayerViewSleepTimer _playerViewSleepTimer;
		public bool IsSessionValid { get { return CurrentPlayer.Current.IsSessionValid; } }
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

			// if (this.Player != null) 							
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

			UpdateUI ();
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		AudioBook displayedAudioBook = null; 

		public void DisplayAlbum()
		{
			SavePosition ();

			var ab = CurrentState.Current.CurrentAudioBook;
			bool reload = (ab != displayedAudioBook);
			displayedAudioBook = ab;

			bool hasConnection = Reachability.RemoteHostStatus () != NetworkStatus.NotReachable;
				
			bool hideControls = ab == null || !hasConnection || this.Player == null;
			this.SkipForward.Hidden = hideControls;
			this.SkipBackward.Hidden = hideControls;
			this.PrevTrack.Hidden = hideControls;
			this.NextTrack.Hidden = hideControls;
			this.Airplay.Hidden = hideControls;
			this.LesezeichenButton.Hidden = hideControls;

			if (!hasConnection || this.Player == null || ab == null) {
				this.AuthorLabel.Text = "";
				this.ProgressBar.Hidden = true;
				this.bisEndeKapitelLabel.Text = "";
				this.bisEndeBuchLabel.Text = "";
				this.kapitelLabel.Text = "";
				this.seitStartKapitelLabel.Text = "";
				displayedAudioBook = null;
				if (!hasConnection) {
					this.AlbumLabel.Text = "Keine Internetverbindung";
					this.TrackLabel.Text = "Streaming nicht möglich";
					this.PlayButton.SetImage (UIImage.FromBundle ("NoConnection"), UIControlState.Normal);
					this.AlbumImage.Image = UIImage.FromBundle ("NoConnectionTitle");
				} else if (this.Player == null) {
					this.AlbumLabel.Text = "Bitte melde dich mit deinem";
					this.TrackLabel.Text = "Premium Spotify Account an";
					this.AlbumImage.Image = UIImage.FromBundle ("Spotify");
					this.PlayButton.SetImage (UIImage.FromBundle ("NotLoggedIn"), UIControlState.Normal);
				} else {
					this.AlbumLabel.Text = "Wähle ein Hörbuch aus einer";
					this.TrackLabel.Text = "der Playlisten oder Suche";
					this.AuthorLabel.Text = "nach einem Titel oder Autor";
					this.AlbumImage.Image = UIImage.FromBundle ("Books");
					this.PlayButton.SetImage (UIImage.FromBundle ("Suche"), UIControlState.Normal);
				}
			} 
			else 
			{
				this.PlayButton.SetImage (UIImage.FromBundle (this.Player.IsPlaying ? "Pause" : "Play"), UIControlState.Normal);

				var c = CurrentState.Current;
				if (c.CurrentAudioBook != null &&
					c.CurrentAudioBook.CurrentPosition != null &&
					ab.Tracks.Count () > c.CurrentAudioBook.CurrentPosition.TrackIndex) 
				{
					var track = ab.Tracks.ElementAt (c.CurrentAudioBook.CurrentPosition.TrackIndex);
					this.TrackLabel.Text = track.Name;
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
				if (reload) { 
					this.AlbumLabel.Text = ab.Album.Name;
					this.AuthorLabel.Text = ab.Artists.FirstOrDefault ();
					ab.SetLargeImage (this.AlbumImage);
				} 
			}
			SetupNowPlaying ();
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
				if (this.Player != null)	
					this.Player.SetIsPlaying (true, (error) => {
				});
			} else if (rc == UIEventSubtype.RemoteControlPause) {
				if (this.Player != null)
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
					if (this.Player != null) {
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
					if (this.Player != null) {
						this.OnForwardTime(null);
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.TogglePlayPauseCommand.Enabled = true;
			commandCenter.TogglePlayPauseCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.Player != null) {
						this.TogglePlaying();
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.PlayCommand.Enabled = true;
			commandCenter.PlayCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.Player != null) {
						this.Player.SetIsPlaying(true, (error) => { });
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.StopCommand.Enabled = true;
			commandCenter.StopCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.Player != null) {
						this.Player.SetIsPlaying(false, (error) => { });
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.PauseCommand.Enabled = true;
			commandCenter.PauseCommand.AddTarget( (remoteCommand) => 
				{ 
					if (this.Player != null) {
						this.Player.SetIsPlaying(false, (error) => { });
						return MPRemoteCommandHandlerStatus.Success;
					} else 
						return MPRemoteCommandHandlerStatus.CommandFailed;
				});
			commandCenter.SkipForwardCommand.Enabled = true;
			commandCenter.SkipForwardCommand.AddTarget( (remoteCommand) => 
			{ 
				if (this.Player != null) {
					this.OnForwardTime(null);
					return MPRemoteCommandHandlerStatus.Success;
				} else 
					return MPRemoteCommandHandlerStatus.CommandFailed;
			});			
			commandCenter.SkipBackwardCommand.Enabled = true;
			commandCenter.SkipBackwardCommand.AddTarget( (remoteCommand) => 
			{ 
				if (this.Player != null) {
					this.OnBackTime(null);
					return MPRemoteCommandHandlerStatus.Success;
				} else 
					return MPRemoteCommandHandlerStatus.CommandFailed;
			});
			commandCenter.NextTrackCommand.Enabled = true;
			commandCenter.NextTrackCommand.AddTarget( (remoteCommand) => 
			{ 
				if (this.Player != null) {
					this.OnNextTrack(null);
					return MPRemoteCommandHandlerStatus.Success;
				} else 
					return MPRemoteCommandHandlerStatus.CommandFailed;
			});			
			commandCenter.PreviousTrackCommand.Enabled = true;
			commandCenter.PreviousTrackCommand.AddTarget( (remoteCommand) => 
			{ 
				if (this.Player != null) {
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

		public void UpdateUI()
		{
			DisplayAlbum ();
		}

		public DateTime SleepTimerStartTime { get; set; } = DateTime.MinValue;
		public int SleepTimerOpion = 0;

		partial void OnAddBookmark (UIKit.UIButton sender)
		{
			if (this.Player != null) {
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
				if (this.Player != null)
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
				else {
					OpenLoginPage();
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
			if (this.Player != null)
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

