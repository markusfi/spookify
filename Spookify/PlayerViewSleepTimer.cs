using System;
using UIKit;
using System.Threading.Tasks;
using Foundation;

namespace Spookify
{
	public class PlayerViewSleepTimer : NSObject
	{
		ISleepTimerController _sleepTimerController;

		public PlayerViewSleepTimer (ISleepTimerController sleepTimerController)
		{
			_sleepTimerController = sleepTimerController;
		}

		static int[] SleepTimerOptions = new int[] { 0, 10, 15, 30, 45, 60, 24*60 };
		public static void ShowSleepTimerConfiguration(UIViewController presentingViewController, ISleepTimerController sleepTimerController, Action okHandler = null, Action cancelHandler = null)
		{
			UIAlertController ac = UIAlertController.Create("Schlafmodus","Das Hörbuch wird nach Ablauf der eingestellten Zeit automatisch beendet",UIAlertControllerStyle.ActionSheet);
			for (int i=0;i<SleepTimerOptions.Length;i++) {
				ac.AddAction(CreateAlertOption(i, ac, sleepTimerController, okHandler));
			}
			var action = UIAlertAction.Create ("Abbrechen", UIAlertActionStyle.Cancel, (alertAction) => 
				{
					ac.Dispose ();
					if (cancelHandler != null)
						cancelHandler();
				});
			if (action != null)
				ac.AddAction(action);
			presentingViewController.PresentViewController(ac, true, null);			
		}
		static UIAlertAction CreateAlertOption(int i, UIAlertController ac, ISleepTimerController sleepTimerController, Action okHandler)
		{
			double time = SleepTimerOptions[i];
			bool firstOption = i == 0;
			bool lastOption = (i == SleepTimerOptions.Length - 1);
			if (lastOption && !(CurrentState.Current.CurrentAudioBook != null && CurrentState.Current.CurrentAudioBook.CurrentPosition != null))
				return null; // kein aktuelles Hörbuch --> keine Ende des Kapitels!
			string actiontext = firstOption ? "Aus" : lastOption ? "Ende des Kapitels" : string.Format("{0} Minuten",time);
			var alertOption = UIAlertAction.Create(
				actiontext,
				UIAlertActionStyle.Default, (alertAction) => { 
					if (lastOption && CurrentState.Current.CurrentAudioBook != null && CurrentState.Current.CurrentAudioBook.CurrentPosition != null)
						time = (CurrentState.Current.CurrentTrack.Duration - CurrentState.Current.CurrentAudioBook.CurrentPosition.PlaybackPosition) / 60.0;
					sleepTimerController.SleepTimerOpion = i;	
					sleepTimerController.SleepTimerStartTime = firstOption ? DateTime.MinValue : DateTime.Now.AddMinutes(time);
					ac.Dispose();
					if (okHandler != null)
						okHandler();
				});
			if (sleepTimerController.SleepTimerOpion == i) {
				var img = UIImage.FromBundle("Checkmark");
				if (img != null)
					alertOption.SetValueForKey(img,(NSString)"image");
			}
			return alertOption;
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

					this.InvokeOnMainThread (async () => {
						_sleepTimerController.DisplayAlbum ();
						await HandleSleepTimer();
					});
				}
			}
		}
		async Task HandleSleepTimer()
		{
			if (_sleepTimerController.SleepTimerStartTime == DateTime.MinValue) {
				if (SleepTimerLabel != null) {
					SleepTimerLabel.RemoveFromSuperview ();
					SleepTimerLabel.Dispose ();
					SleepTimerLabel = null;
				}

			} else if (_sleepTimerController.SleepTimerStartTime == DateTime.MaxValue) {

			} else {
				if (_sleepTimerController.SleepTimerStartTime > DateTime.MinValue) {
					if (_sleepTimerController.SleepTimerStartTime > DateTime.Now) {
						if (SleepTimerLabel == null) {
							SleepTimerLabel = new UILabel ();
							SleepTimerLabel.Tag = 100;
							SleepTimerLabel.TranslatesAutoresizingMaskIntoConstraints = false;
							SleepTimerLabel.TextAlignment = UITextAlignment.Center;
							SleepTimerLabel.Layer.CornerRadius = 5;
							SleepTimerLabel.Layer.MasksToBounds = true;
							SleepTimerLabel.Layer.BorderColor = UIColor.DarkGray.CGColor;
							SleepTimerLabel.Alpha = 0.8f;
							SleepTimerLabel.BackgroundColor = UIColor.LightGray;
							SleepTimerLabel.TintColor = UIColor.Black;
							SleepTimerLabel.TextColor = UIColor.Black;
							SleepTimerLabel.Font = UIFont.FromName ("HelveticaNeue-Light", 20f);
							SleepTimerLabel.UserInteractionEnabled = true;
							SleepTimerLabel.AddGestureRecognizer(new UITapGestureRecognizer(() => { 
								ShowSleepTimerConfiguration(_sleepTimerController.CurrentViewController, _sleepTimerController);
							}) { NumberOfTapsRequired = 1 });

							_sleepTimerController.View.AddSubview (SleepTimerLabel);
							_sleepTimerController.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _sleepTimerController.View, NSLayoutAttribute.Width, 0.6f, 0));
							_sleepTimerController.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Height, 0f, 45f));
							_sleepTimerController.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _sleepTimerController.View, NSLayoutAttribute.CenterX, 1f, 0));
							_sleepTimerController.View.AddConstraint (NSLayoutConstraint.Create (SleepTimerLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _sleepTimerController.View, NSLayoutAttribute.CenterY, 1f, -90f));

						}
						SleepTimerLabel.Text = string.Format ("Sleep Timer {0:mm\\:ss}", _sleepTimerController.SleepTimerStartTime.Subtract (DateTime.Now));
					}
					if (_sleepTimerController.SleepTimerStartTime < DateTime.Now) {
						_sleepTimerController.SleepTimerStartTime = DateTime.MinValue;
						_sleepTimerController.SleepTimerOpion = 0;
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
		}
	}
}

