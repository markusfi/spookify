using System;
using UIKit;
using System.Threading.Tasks;

namespace Spookify
{
	public class PlayerViewSleepTimer
	{
		PlayerViewController _playerViewController;

		public PlayerViewSleepTimer (PlayerViewController playerViewController)
		{
			_playerViewController = playerViewController;
		}

		UILabel SleepTimerLabel { get; set; }

		bool timerRunning;

		public void StopTimer()
		{
			timerRunning = false;
		}
		public async void StartTimer ()
		{
			if (!timerRunning) {
				timerRunning = true;
				while (timerRunning) {
					await Task.Delay (1000);

					if (CurrentPlayer.Current.NeedToRenewSession)
						CurrentPlayer.Current.RenewSession ();

					_playerViewController.InvokeOnMainThread (async () => {
						_playerViewController.DisplayAlbum ();
						if (_playerViewController.SleepTimerStartTime == DateTime.MinValue) {
							if (SleepTimerLabel != null) {
								SleepTimerLabel.RemoveFromSuperview ();
								SleepTimerLabel.Dispose ();
								SleepTimerLabel = null;
							}

						} else if (_playerViewController.SleepTimerStartTime == DateTime.MaxValue) {

						} else {
							if (_playerViewController.SleepTimerStartTime > DateTime.MinValue) {
								if (_playerViewController.SleepTimerStartTime > DateTime.Now) {
									if (SleepTimerLabel == null) {
										SleepTimerLabel = new UILabel ();
										SleepTimerLabel.TranslatesAutoresizingMaskIntoConstraints = false;
										SleepTimerLabel.TextAlignment = UITextAlignment.Center;
										SleepTimerLabel.Layer.CornerRadius = 5;
										SleepTimerLabel.Layer.MasksToBounds = true;
										SleepTimerLabel.Layer.BorderColor = UIColor.DarkGray.CGColor;
										SleepTimerLabel.Alpha = 0.8f;
										SleepTimerLabel.BackgroundColor = UIColor.LightGray;
										SleepTimerLabel.TintColor = UIColor.Black;
										SleepTimerLabel.TextColor = UIColor.Black;

										_playerViewController.View.AddSubview (SleepTimerLabel);
										_playerViewController.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _playerViewController.View, NSLayoutAttribute.Width, 0.5f, 0));
										_playerViewController.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Height, 0f, 28f));
										_playerViewController.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _playerViewController.View, NSLayoutAttribute.CenterX, 1f, 0));
										_playerViewController.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _playerViewController.View, NSLayoutAttribute.CenterY, 1f, 90f));

									}
									SleepTimerLabel.Text = string.Format ("Sleep Timer {0:mm\\:ss}", _playerViewController.SleepTimerStartTime.Subtract (DateTime.Now));
								}
								if (_playerViewController.SleepTimerStartTime < DateTime.Now) {
									_playerViewController.SleepTimerStartTime = DateTime.MinValue;
									CurrentPlayer.Current.SavePosition(true);
									if (CurrentPlayer.Current.Player != null) {
										var origVol = CurrentPlayer.Current.Player.Volume;
										var vol = origVol;
										var step = vol / 20;
										while (vol >= 0) {
											CurrentPlayer.Current.Player.SetVolume (vol -= step, (error) => {});
											await Task.Delay (500);
										}
										CurrentPlayer.Current.Player.SetIsPlaying (false, (error) => {});
										CurrentPlayer.Current.Player.SetVolume (origVol, (error) => {});
									}
								}
							}
						}
					});
				}
			}
		}
	}
}

