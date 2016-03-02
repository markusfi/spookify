using System;

namespace Spookify
{
	public static class AudioBookManipulation
	{
		public static bool SkipTrack(this AudioBook ab, int step)
		{
			if (ab != null &&
				ab.CurrentPosition != null) {

				if (ab.CurrentPosition.TrackIndex + step < ab.Tracks.Count &&
					ab.CurrentPosition.TrackIndex + step >= 0) {
					ab.CurrentPosition.TrackIndex += step;
					ab.CurrentPosition.PlaybackPosition = 0;
					return true;
				}
			}
			return false;
		}
		public static bool AdjustTime(this AudioBook ab, double seconds)
		{
			if (ab != null &&
				ab.CurrentPosition != null) 
			{
				if (seconds > 0) {
					while (seconds > 0) {
						var track = ab.Tracks [ab.CurrentPosition.TrackIndex];
						if (track.Duration > ab.CurrentPosition.PlaybackPosition + seconds) {
							ab.CurrentPosition.PlaybackPosition += seconds;
							return true;
						} else {
							seconds = (seconds - (track.Duration - ab.CurrentPosition.PlaybackPosition));
							ab.CurrentPosition.PlaybackPosition = 0;
							if (ab.CurrentPosition.TrackIndex < ab.Tracks.Count-1)
								ab.CurrentPosition.TrackIndex += 1;
							else
								return true;
						}
					}
				} else {
					while (seconds < 0) {
						var track = ab.Tracks [ab.CurrentPosition.TrackIndex];
						if (ab.CurrentPosition.PlaybackPosition >= Math.Abs(seconds)) {
							ab.CurrentPosition.PlaybackPosition += seconds;
							return true;
						} else { 
							seconds = (seconds + ab.CurrentPosition.PlaybackPosition);
							if (ab.CurrentPosition.TrackIndex > 0) {
								ab.CurrentPosition.TrackIndex -= 1;
								ab.CurrentPosition.PlaybackPosition = ab.Tracks [ab.CurrentPosition.TrackIndex].Duration;
							} else
								return true;
						}
					}
				}
			}
			return false;
		}
	}
}

