using System;
using SpotifySDK;
using Foundation;

namespace Spookify
{
	public class MySPTAudioStreamingPlaybackDelegate : SPTAudioStreamingPlaybackDelegate {
		PlayerViewController viewController = null;

		public MySPTAudioStreamingPlaybackDelegate(PlayerViewController vc) {
			this.viewController = vc;
		}
		void LogState(string txt)
		{
			#if DEBUG
			Console.WriteLine(txt);
			var p = this.viewController.Player;
			if (p != null) {
				Console.WriteLine ("Player.CurrentTrackIndex="+p.CurrentTrackIndex+", Player.CurrentPlaybackPosition: "+p.CurrentPlaybackPosition+") CurrentStartTrack="+CurrentPlayer.Current.CurrentStartTrack);
			}
			#endif
		}

		public override void AudioStreaming (SPTAudioStreamingController audioStreaming, bool isPlaying)
		{
			LogState ("AudioStreaming (isPlaying = " + isPlaying + ")");
			viewController.DisplayAlbum();
		}
		public override void AudioStreaming (SPTAudioStreamingController audioStreaming, double offset)
		{
			LogState ("AudioStreaming (double offset = " + offset + ")");
			viewController.DisplayAlbum();
		}
		public override void AudioStreaming (SPTAudioStreamingController audioStreaming, NSDictionary trackMetadata)
		{
			LogState ("AudioStreaming (trackMetadata)");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidFailToPlayTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri)
		{
			LogState ("AudioStreamingDidFailToPlayTrack (NSUrl trackUri)");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidLosePermissionForPlayback (SPTAudioStreamingController audioStreaming)
		{
			LogState ("AudioStreamingDidLosePermissionForPlayback ()");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidSkipToNextTrack (SPTAudioStreamingController audioStreaming)
		{
			LogState ("AudioStreamingDidSkipToNextTrack ()");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidSkipToPreviousTrack (SPTAudioStreamingController audioStreaming)
		{
			LogState ("AudioStreamingDidSkipToPreviousTrack ()");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidStartPlayingTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri)
		{
			LogState ("AudioStreamingDidStartPlayingTrack (trackUri="+trackUri.AbsoluteString+")");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidStopPlayingTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri)
		{
			LogState ("AudioStreamingDidStopPlayingTrack (trackUri="+trackUri.AbsoluteString+")");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidBecomeInactivePlaybackDevice (SPTAudioStreamingController audioStreaming)
		{
			LogState ("AudioStreamingDidBecomeInactivePlaybackDevice");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidBecomeActivePlaybackDevice (SPTAudioStreamingController audioStreaming)
		{
			LogState ("AudioStreamingDidBecomeActivePlaybackDevice");
			viewController.DisplayAlbum();
		}
		public override void AudioStreamingDidPopQueue (SPTAudioStreamingController audioStreaming)
		{
			LogState ("AudioStreamingDidPopQueue");
			viewController.DisplayAlbum();
		}
	}
}

