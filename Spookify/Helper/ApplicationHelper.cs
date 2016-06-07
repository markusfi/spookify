using System;
using UIKit;
using CoreFoundation;
using CoreGraphics;
using System.Linq;
using SpotifySDK;
using Foundation;

namespace Spookify
{
	public static class ApplicationHelper
	{
		public static void SwitchToTab(this UITabBarController tabBarController, int page = 1)
		{
			if (tabBarController == null)
				return;
			if (page < 0 || page > tabBarController.ViewControllers.Length)
				return;
			
			// Switch tab.

			UIView  fromView = tabBarController.SelectedViewController.View;
			UIView  toView = tabBarController.ViewControllers[page].View;

			if (fromView == null) {
				tabBarController.SelectedIndex = page;
			} else {
				if (fromView != toView) {
					UIView.Transition (fromView, toView, 0.5, UIViewAnimationOptions.CurveEaseInOut, () => {
						tabBarController.SelectedIndex = page;
					});
				}
			}
		}
		public static void AsyncLoadWhenSession()
		{
			var gloalQueue = DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default);
			gloalQueue.DispatchAsync (() => {
				if (CurrentPlayer.Current.IsSessionValid) {
					var dummy = CurrentAudiobooks.Current.User.Playlists;
				}
			});
		}

		public static string ToMinutesText(this TimeSpan ts)
		{
			return string.Format ("{0:00}:{1:00}", Math.Truncate(ts.TotalMinutes), ts.Seconds);
		}
		public static string ToLongTimeText(this TimeSpan ts)
		{
			return ToTimeText(ts," Stunden"," Stunde"," Minuten"," Minute"," Sekunden"," Sekunde");
		}
		public static string ToTimeText(this TimeSpan ts)
		{
			return ToTimeText(ts," Std."," Std."," Min."," Min."," Sek."," Sek.");
		}
		public static string ToShortTimeText(this TimeSpan ts)
		{
			return ToTimeText(ts,"h","h","min","min","s","s");
		}
		static string ToTimeText(this TimeSpan ts, string stunden, string stunde, string minuten, string minute, string sekunden, string sekunde)
		{
			if (Math.Truncate (ts.TotalHours) > 0.0)
				return string.Format ("{0}{2} {1:00}{3}", Math.Truncate (ts.TotalHours), ts.Minutes, ts.Hours > 1 ? stunden : stunde, ts.Minutes > 1 ? minuten : minute);
			else if (Math.Truncate (ts.TotalMinutes) > 0.0)
				return string.Format ("{0}{2} {1:00}{3}", Math.Truncate (ts.TotalMinutes), ts.Seconds, ts.Minutes > 1 ? minuten : minute, ts.Seconds > 1 ? sekunden : sekunde);
			else
				return string.Format ("{0:00}{1}", ts.Seconds, ts.Seconds > 1 ? sekunden : sekunde);
		}

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
		public static string RemoveDelimiter(this string s)
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
		public static string ToAlbumName(this AudioBook ab)
		{
			if (ab == null || ab.Album == null)
				return "";
			if (ab.CurrentPosition == null || ab.Tracks == null || ab.Tracks.Count <= ab.CurrentPosition.TrackIndex)
				return ab.Album.Name;
			
			var track = ab.Tracks.ElementAt (ab.CurrentPosition.TrackIndex);
			var prefix = CommonPrefix (new [] { track.Name, ab.Album.Name} );
			if (prefix != null && prefix.Length == ab.Album.Name.Length) {
				return track.Name;
			} else if (prefix != null && prefix.Length > 5 && track.Name.Length > (prefix.Length + 1)) {
				return ab.Album.Name + " - " + RemoveDelimiter(track.Name.Substring (prefix.Length));
			} else
				return ab.Album.Name + " - " + track.Name;
		}
		public static string ToAuthorName(this AudioBook ab)
		{
			return ab?.Artists?.Aggregate ("", (s, t) => string.IsNullOrEmpty (s) ? t : t + ", " + s) ?? "";
		}
		public static string TimeToEnd(this AudioBook ab)
		{
			if (ab == null || ab.Tracks == null)
				return null;			
			var gesamtBisEnde = ab.GesamtBisEnde - ab.GesamtSeitAnfang;
			var tsBisEnde = TimeSpan.FromSeconds (gesamtBisEnde);
			return tsBisEnde.ToTimeText ();
		}
		public static UIImage CurrentPlayButtonImage(this SPTAudioStreamingController player)
		{
			return UIImage.FromBundle (player != null && player.IsPlaying ? "Pause" : "Play").ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
		}

		public static DateTime NSDateToDateTime(this Foundation.NSDate date)
		{
			DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0);
			DateTime currentDate = reference.AddSeconds(date.SecondsSinceReferenceDate);
			DateTime localDate = currentDate.ToLocalTime ();
			return localDate;
		}
	}
}

