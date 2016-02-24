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
		SPTAuthViewController authViewController = null;
		MySPTAuthViewDelegate mySPTAuthViewDelegate = null;

		public bool IsSessionValid {
			get {
				SPTAuth auth = SPTAuth.DefaultInstance;
				return !(auth.Session == null || !auth.Session.IsValid);
			}
		}

		public bool IsPlayerCreated { 
			get { 
				return this._player != null && this._player.LoggedIn; 
			}
		}
		SPTAudioStreamingController _player;
		public SPTAudioStreamingController Player
		{
			get 
			{ 
				if (this.IsSessionValid) {
					if (_player == null) {
						SPTAuth auth = SPTAuth.DefaultInstance;
						var ca = new SPTCoreAudioController ();
						_player = new SPTAudioStreamingController (auth.ClientID, ca);
						_player.PlaybackDelegate = new MySPTAudioStreamingDelegate (this);

						// this.player.DiskCache = new SPTDiskCache(1024 * 1024 * 64).;
					} 
					if (!_player.LoggedIn) {
						SPTAuth auth = SPTAuth.DefaultInstance;
						_player.LoginWithSession (auth.Session, error => {

							// login failed.
							Console.WriteLine(error);
						});
					}
				} else {
					_player = null;
				}
				return _player;
			}
		}

		public void PlayCurrentAudioBook()
		{			
			if (this.Player != null) {
				if (CurrentState.Current.CurrentAudioBook != null) {
					if (CurrentState.Current.CurrentAudioBook.CurrentPosition != null) {

						SPTPlayOptions options = new SPTPlayOptions ();
						options.TrackIndex = CurrentState.Current.CurrentAudioBook.CurrentPosition.TrackIndex;
						options.StartTime = CurrentState.Current.CurrentAudioBook.CurrentPosition.PlaybackPosition;
						this.Player.PlayURIs (CurrentState.Current.CurrentAudioBook.Tracks.Select (t => t.NSUrl).ToArray (), options, 
							(playURIError) => {
							});
					} else {
						this.Player.PlayURIs (CurrentState.Current.CurrentAudioBook.Tracks.Select (t => t.NSUrl).ToArray (), 
							0, 
							(playURIError) => {
							});
					}
				}
			}
		}
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
		}

		UILabel SleepTimerLabel { get; set; }

		bool timerRunning;
		async void StartTimer ()
		{
			timerRunning = true;
			while (timerRunning) {
				await Task.Delay (1000);
				InvokeOnMainThread ( async () => {
					DisplayAlbum ();
					if (SleepTimerStartTime == DateTime.MinValue) {
						if (SleepTimerLabel != null) {
							SleepTimerLabel.RemoveFromSuperview();
							SleepTimerLabel.Dispose();
							SleepTimerLabel = null;
						}
						
					} else if (SleepTimerStartTime == DateTime.MaxValue) {
						
					} else {
						if (SleepTimerStartTime > DateTime.MinValue) {
							if (SleepTimerStartTime > DateTime.Now) {
								if (SleepTimerLabel == null) {
									SleepTimerLabel = new UILabel();
									SleepTimerLabel.TranslatesAutoresizingMaskIntoConstraints = false;
									SleepTimerLabel.TextAlignment = UITextAlignment.Center;
									SleepTimerLabel.Layer.CornerRadius = 5;
									SleepTimerLabel.Layer.MasksToBounds = true;
									SleepTimerLabel.Layer.BorderColor = UIColor.DarkGray.CGColor;
									SleepTimerLabel.Alpha = 0.8f;
									SleepTimerLabel.BackgroundColor = UIColor.Red;
									SleepTimerLabel.TintColor = UIColor.Black;
									SleepTimerLabel.TextColor = UIColor.Black;

									this.View.AddSubview (SleepTimerLabel);
									this.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Width, 0.5f, 0));
									this.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Height, 0f, 24f));
									this.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.CenterX, 1f, 0));
									this.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.CenterY, 1f, 90f));

								}
								SleepTimerLabel.Text = string.Format("Sleep Timer {0:mm\\:ss}",SleepTimerStartTime.Subtract(DateTime.Now));
							}
							if (SleepTimerStartTime < DateTime.Now) {
								SleepTimerStartTime = DateTime.MinValue;
								SavePosition();
								if (this.Player != null) {
									var origVol = this.Player.Volume;
									var vol = origVol;
									var step = vol/20;
									while (vol >= 0) {
										this.Player.SetVolume(vol-=step, (error) => { });
										await Task.Delay (500);
									}
									this.Player.SetIsPlaying(false, (error) => { });
									this.Player.SetVolume(origVol, (error) => { });
								}
							}
						}
					}
				});
			}
		}
		void StopTimer()
		{
			timerRunning = false;
		}

		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			SavePosition ();
		}

		void SavePosition()
		{
			if (this.Player != null) {
				if (this.Player.CurrentPlaybackPosition != 0 ||
				   this.Player.CurrentTrackIndex != 0) {
					if (CurrentState.Current.CurrentAudioBook != null) {				
						if (CurrentState.Current.CurrentAudioBook.CurrentPosition == null ||
						    (CurrentState.Current.CurrentAudioBook.CurrentPosition.TrackIndex < this.Player.CurrentTrackIndex ||
						    (CurrentState.Current.CurrentAudioBook.CurrentPosition.TrackIndex == this.Player.CurrentTrackIndex &&
						    CurrentState.Current.CurrentAudioBook.CurrentPosition.PlaybackPosition < this.Player.CurrentPlaybackPosition)))
						{
							CurrentState.Current.CurrentAudioBook.CurrentPosition = new AudioBookBookmark () {
								PlaybackPosition = this.Player.CurrentPlaybackPosition,
								TrackIndex = this.Player.CurrentTrackIndex
							};
							CurrentState.Current.StoreCurrentState ();
						}
					}
				}
			}
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			UpdateUI ();
			StartTimer ();
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			StopTimer ();
		}

		AudioBook displayedAudioBook = null; 

		void DisplayAlbum()
		{
			SavePosition ();

			var ab = CurrentState.Current.CurrentAudioBook;
			bool reload = (ab != displayedAudioBook);
			displayedAudioBook = ab;

			bool hasConnection = Reachability.RemoteHostStatus () != NetworkStatus.NotReachable;
				
			bool hideControls = ab == null || !hasConnection;
			this.SkipForward.Hidden = hideControls;
			this.SkipBackward.Hidden = hideControls;
			this.PrevTrack.Hidden = hideControls;
			this.NextTrack.Hidden = hideControls;
			this.Airplay.Hidden = hideControls;

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
					var imageView = this.AlbumImage;
					if (imageView != null && ab.LargestCoverURL != null) {
						imageView.Image = null;
						var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
						gloalQueue.DispatchAsync (() => {
							NSError err = null;
							UIImage image = null;
							NSData imageData = NSData.FromUrl (new NSUrl (ab.LargestCoverURL), 0, out err);
							if (imageData != null)
								image = UIImage.LoadFromData (imageData);

							DispatchQueue.MainQueue.DispatchAsync (() => {
								imageView.Image = image;
								if (image == null) {
									System.Diagnostics.Debug.WriteLine ("Could not load image with error: {0}", err);
									return;
								}
								SetupNowPlaying();
							});
						});
					}
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
					this.OnNextTrack(null);
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

		void UpdateUI()
		{
			DisplayAlbum ();
		}

		DateTime SleepTimerStartTime { get; set; } = DateTime.MinValue;
		int SleepTimerOpion = 0;

		partial void OnSleeptimer (UIKit.UIButton sender)
		{
		}

		partial void OnAddBookmark (UIKit.UIButton sender)
		{
			if (this.Player != null) {
				var ab = CurrentState.Current.CurrentAudioBook;
				if (ab != null && ab.CurrentPosition != null) {
					var alertView = new UIAlertView("Bookmark","Bookmark hinzufügen?", null, "Abbrechen", "Ok");
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
			var ab = CurrentState.Current.CurrentAudioBook;
			var kapitelSection = new Section("Kapitel","Springe direkt zu einem Kapitel");
			if (ab != null && ab.Tracks != null)
				kapitelSection.AddAll(ab.Tracks.Select(t => { 
					var kapitelStart = TimeSpan.FromSeconds(ab.Tracks.TakeWhile(tw => tw != t).Sum(ts => ts.Duration));
					var element = new StringElement(
						string.Format("Kapitel {0}",t.Index),
						string.Format((kapitelStart.TotalHours > 1.0 ? "{0:hh\\:mm\\:ss}" : "{0:mm\\:ss}"), kapitelStart));
					element.Tapped += delegate {
						if (element.IndexPath.Row < ab.Tracks.Count) {
							ab.CurrentPosition = new AudioBookBookmark() { PlaybackPosition = 0, TrackIndex = element.IndexPath.Row };
							PlayCurrentAudioBook();
							this.NavigationController.PopToRootViewController(true);
						}
					}; 
					return element;
				}));

			var lesezeichenSection = new Section("Lesezeichen","Du kannst die Wiedergabe direkt an einem Lesezeichen fortsetzen");
			if (ab != null) {
				if (ab.Bookmarks != null) {
				lesezeichenSection.AddAll(
					ab.Bookmarks.Select(b => { 
					var element = new StringElement(
							string.Format("Kapitel {0}",b.TrackIndex+1),
							string.Format("{0:mm\\:ss}",TimeSpan.FromSeconds(b.PlaybackPosition)));
					element.Tapped += delegate {
						if (element.IndexPath.Row < ab.Bookmarks.Count) {
							ab.CurrentPosition = new AudioBookBookmark(ab.Bookmarks[element.IndexPath.Row]);
							PlayCurrentAudioBook();
							this.NavigationController.PopToRootViewController(true);
						}
					}; 
					return element;
					}));
				}
				if (ab.CurrentPosition != null) {
					var currentPos = new StringElement(
						"Aktuelle Position",
						string.Format("Kapitel {0} {1:mm\\:ss}",
							ab.CurrentPosition.TrackIndex,
							TimeSpan.FromSeconds(ab.CurrentPosition.PlaybackPosition)));
					currentPos.Tapped += delegate {
						ab.CurrentPosition = ab.CurrentPosition;
						PlayCurrentAudioBook();
						DismissViewController(true, delegate {});
					}; 
					lesezeichenSection.Add(currentPos);
				};
			}
			var sleepTimerSection = new Section("");
			sleepTimerSection.Add(new CLickableRadioElement(this, 0, "Aus",TimeSpan.FromSeconds(0)));
			sleepTimerSection.Add(new CLickableRadioElement(this, 1, "1 Minuten",TimeSpan.FromMinutes(1)));
			sleepTimerSection.Add(new CLickableRadioElement(this, 2, "15 Minuten",TimeSpan.FromMinutes(15)));
			sleepTimerSection.Add(new CLickableRadioElement(this, 3, "30 Minuten",TimeSpan.FromMinutes(30)));
			sleepTimerSection.Add(new CLickableRadioElement(this, 4, "45 Minuten",TimeSpan.FromMinutes(45)));
			sleepTimerSection.Add(new CLickableRadioElement(this, 5, "60 Minuten",TimeSpan.FromMinutes(60)));
			sleepTimerSection.Add(new CLickableRadioElement(this, 6, "Ende des Kapitels",TimeSpan.FromDays(1)));
			var root = new RootElement ("Einstellungen") {
				new Section () {
					new RootElement("Kapitel",0,ab.CurrentPosition != null ? ab.CurrentPosition.TrackIndex : 0) {
						kapitelSection	
					},
					new RootElement("Lesezeichen",0,lesezeichenSection.Count-1) {
						lesezeichenSection
					},
					new RootElement("Schlafmodus", new RadioGroup("Schlafmodus",SleepTimerOpion)) {
						sleepTimerSection
					}
				}
			};
			var dvc = new DialogViewController(UITableViewStyle.Plain, root, true);
			this.NavigationController.PushViewController(dvc,true);
		}

		public class CLickableRadioElement : RadioElement {
			TimeSpan _time;
			PlayerViewController _vc;
			int _option;

			public CLickableRadioElement (PlayerViewController vc, int option, string s, TimeSpan time) : base (s) {
				_time = time;
				_vc = vc;
				_option = option;
			}

			public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
			{
				base.Selected (dvc, tableView, path);
				_vc.SleepTimerOpion = _option;
				if (_time.TotalSeconds == 0)
					_vc.SleepTimerStartTime = DateTime.MinValue;
				else if (_time.TotalDays == 1)
					_vc.SleepTimerStartTime = DateTime.MaxValue;
				else 
					_vc.SleepTimerStartTime = DateTime.Now.Add(_time);
				dvc.NavigationController.PopToRootViewController(true);
			}
		}
		partial void OnNextTrack (UIKit.UIButton sender)
		{
			if (this.Player != null)
				this.Player.SkipNext((error) => {});
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

		public class MySPTAuthViewDelegate : SPTAuthViewDelegate {
			PlayerViewController viewController = null;
			public MySPTAuthViewDelegate(PlayerViewController vc)  {
				this.viewController = vc;
			}

			#region implemented abstract members of SPTAuthViewDelegate

			public override void AuthenticationViewControllerDidCancelLogin (SPTAuthViewController authenticationViewController)
			{
				this.viewController.AlbumLabel.Text = "Login abgebrochen";
			}

			public override void AuthenticationViewControllerFail (SPTAuthViewController authenticationViewController, NSError error)
			{
				this.viewController.AlbumLabel.Text = "Login Fehler";
			}

			public override void AuthenticationViewControllerLogin (SPTAuthViewController authenticationViewController, SPTSession session)
			{
				this.viewController.UpdateUI ();

				if (CurrentState.Current.Audiobooks.Count == 0)
					viewController.SwitchTab (2); // liste der Bücher in Playlists
				else if (CurrentState.Current.CurrentAudioBook == null)
					viewController.SwitchTab (0); // Liste der gewählten Bücher
				else
					viewController.SwitchTab (1);
			}

			#endregion
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		void OpenLoginPage()
		{
			this.AlbumLabel.Text = @"Logging in...";

			this.authViewController = SPTAuthViewController.AuthenticationViewController;
			this.authViewController.HideSignup = false;
			this.authViewController.Delegate = new MySPTAuthViewDelegate (this);
			this.authViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
			this.authViewController.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;

			this.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
			this.DefinesPresentationContext = true;

			this.PresentViewController (this.authViewController, false, null);
		}
		void TogglePlaying()
		{
			if (this.Player != null)
				this.Player.SetIsPlaying(!this.Player.IsPlaying, (error) => { });
		}	

		partial void OnPrevTrack (UIKit.UIButton sender)
		{
			if (this.Player != null)
				this.Player.SkipPrevious((error) => {});
		}

		partial void OnBackTime (UIKit.UIButton sender)
		{
			if (this.Player != null)
				this.Player.SeekToOffset(Math.Max(0,this.Player.CurrentPlaybackPosition - 30.0), (error) => { });
		}

		partial void OnForwardTime (UIKit.UIButton sender)
		{
			if (this.Player != null)
				this.Player.SeekToOffset(Math.Min(this.Player.CurrentTrackDuration,this.Player.CurrentPlaybackPosition + 30.0), (error) => { });
		}

		public class MySPTAudioStreamingDelegate : SPTAudioStreamingPlaybackDelegate {
			PlayerViewController viewController = null;
			public MySPTAudioStreamingDelegate(PlayerViewController vc) {
				this.viewController = vc;
			}
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, bool isPlaying)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, double offset)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, NSDictionary trackMetadata)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreamingDidFailToPlayTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreamingDidLosePermissionForPlayback (SPTAudioStreamingController audioStreaming)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreamingDidSkipToNextTrack (SPTAudioStreamingController audioStreaming)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreamingDidSkipToPreviousTrack (SPTAudioStreamingController audioStreaming)
			{
				viewController.UpdateUI();
			}
		}
	}
}

