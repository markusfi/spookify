using System;
using MonoTouch.Dialog;
using System.Linq;
using UIKit;
using Foundation;

namespace Spookify
{
	public class PlayerViewSettings
	{
		UIViewController _parentViewController;
		ISleepTimerController _sleepTimerController;

		public PlayerViewSettings (UIViewController parentViewController, ISleepTimerController sleepTimerController)
		{
			_parentViewController = parentViewController;
			_sleepTimerController = sleepTimerController;
		}

		public void PlaySettingsClicked(object sender, EventArgs args) 
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
							CurrentPlayer.Current.PlayCurrentAudioBook();
							_parentViewController.NavigationController.PopToRootViewController(true);
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
									CurrentPlayer.Current.PlayCurrentAudioBook();
									_parentViewController.NavigationController.PopToRootViewController(true);
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
						CurrentPlayer.Current.PlayCurrentAudioBook();
						_parentViewController.DismissViewController(true, delegate {});
					}; 
					lesezeichenSection.Add(currentPos);
				};
			}
			var sleepTimerSection = new Section("");
			sleepTimerSection.Add(new CLickableRadioElement(_sleepTimerController, 0, "Aus",TimeSpan.FromSeconds(0)));
			sleepTimerSection.Add(new CLickableRadioElement(_sleepTimerController, 1, "8 Minuten",TimeSpan.FromMinutes(8)));
			sleepTimerSection.Add(new CLickableRadioElement(_sleepTimerController, 2, "15 Minuten",TimeSpan.FromMinutes(15)));
			sleepTimerSection.Add(new CLickableRadioElement(_sleepTimerController, 3, "30 Minuten",TimeSpan.FromMinutes(30)));
			sleepTimerSection.Add(new CLickableRadioElement(_sleepTimerController, 4, "45 Minuten",TimeSpan.FromMinutes(45)));
			sleepTimerSection.Add(new CLickableRadioElement(_sleepTimerController, 5, "60 Minuten",TimeSpan.FromMinutes(60)));
			sleepTimerSection.Add(new CLickableRadioElement(_sleepTimerController, 6, "Ende des Kapitels",TimeSpan.FromDays(1)));
			var root = new RootElement ("Einstellungen") {
				new Section () {
					new RootElement("Kapitel",0,ab?.CurrentPosition != null ? ab.CurrentPosition.TrackIndex : 0) {
						kapitelSection	
					},
					new RootElement("Lesezeichen",0,lesezeichenSection.Count-1) {
						lesezeichenSection
					},
					new RootElement("Schlafmodus", new RadioGroup("Schlafmodus",_sleepTimerController.SleepTimerOpion)) {
						sleepTimerSection
					}
				}
			};
			var dvc = new DialogViewController(UITableViewStyle.Plain, root, true);
			_parentViewController.NavigationController.PushViewController(dvc,true);
		}

		public class CLickableRadioElement : RadioElement {
			TimeSpan _time;
			ISleepTimerController _sleepTimerController;
			int _option;

			public CLickableRadioElement (ISleepTimerController sleepTimerController, int option, string s, TimeSpan time) : base (s) {
				_time = time;
				_sleepTimerController = sleepTimerController;
				_option = option;
			}

			public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
			{
				base.Selected (dvc, tableView, path);
				_sleepTimerController.SleepTimerOpion = _option;
				if (_time.TotalSeconds == 0)
					_sleepTimerController.SleepTimerStartTime = DateTime.MinValue;
				else if (_time.TotalDays == 1)
					_sleepTimerController.SleepTimerStartTime = DateTime.MaxValue;
				else 
					_sleepTimerController.SleepTimerStartTime = DateTime.Now.Add(_time);
				dvc.NavigationController.PopToRootViewController(true);
			}
		}

	}
}

