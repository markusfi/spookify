using System;
using SpotifySDK;
using UIKit;
using System.Runtime.InteropServices;

namespace Spookify
{
	public class MySPTAudioStreamingDelegate : SPTAudioStreamingDelegate
	{
		int error = 0;
		void CancelAlert()
		{
			if (error > 0 && av != null)
				av.DismissWithClickedButtonIndex (1, true);
			error = 0;
		}
			
		void LogState(SPTAudioStreamingController audioStreaming, string txt)
		{
			#if DEBUG
			Console.WriteLine(txt);
			var p = audioStreaming;
			if (p != null) {
				Console.WriteLine ("Player.CurrentTrackIndex="+p.CurrentTrackIndex+", Player.CurrentPlaybackPosition: "+p.CurrentPlaybackPosition+") CurrentStartTrack="+CurrentPlayer.Current.CurrentStartTrack);
			}
			#endif
		}
		UIAlertView av = null;
		public override void AudioStreaming (SPTAudioStreamingController audioStreaming, Foundation.NSError error)
		{
			LogState (audioStreaming, "AudioStreaming (error = " + error.LocalizedDescription + ")");
			CancelAlert ();
			if (av == null) {
				av = new UIAlertView ("Achtung", error.LocalizedDescription, null, "Ok");
				av.Show ();
				av.WillDismiss += (object sender, UIButtonEventArgs e) => {
					av.Dispose ();
					av = null;
				};
			}
		}
		public override void AudioStreaming (SPTAudioStreamingController audioStreaming, string message)
		{
			LogState (audioStreaming, "AudioStreaming (message = " + message + ")");
			CancelAlert ();
			if (av == null) {
				av = new UIAlertView ("Spotify", message, null, "ok");
				av.Show ();
				av.WillDismiss += (object sender, UIButtonEventArgs e) => {
					av.Dispose ();
					av = null;
				};
			}
		}
		public override void AudioStreamingDidDisconnect (SPTAudioStreamingController audioStreaming)
		{
			LogState (audioStreaming, "AudioStreamingDidDisconnect ()");
			CancelAlert ();
		}

		[DllImport("__Internal", EntryPoint = "exit")]
		public static extern void exit(int status);

		public override void AudioStreamingDidEncounterTemporaryConnectionError (SPTAudioStreamingController audioStreaming)
		{
			LogState (audioStreaming, "AudioStreamingDidEncounterTemporaryConnectionError ()");
			error++;
			if (error > 10 && av == null) {
				av = new UIAlertView ("Fehler", "Ein Verbindungsfehler ist aufgetreten.\nApp Beenden?", null, "Wiederholen","Beenden");
				av.Show ();
				av.WillDismiss += (object sender, UIButtonEventArgs e) => {
					if (e.ButtonIndex==1 && error>0)
						exit(0);
					av.Dispose ();
					av = null;
				};
			}
		}

		public override void AudioStreamingDidLogin (SPTAudioStreamingController audioStreaming)
		{
			LogState (audioStreaming, "AudioStreamingDidLogin ()");
			CancelAlert ();
		}
		public override void AudioStreamingDidLogout (SPTAudioStreamingController audioStreaming)
		{
			LogState (audioStreaming, "AudioStreamingDidLogout ()");
			CancelAlert ();
		}
		public override void AudioStreamingDidReconnect (SPTAudioStreamingController audioStreaming)
		{
			LogState (audioStreaming, "AudioStreamingDidReconnect ()");
			CancelAlert ();
		}
	}
}

