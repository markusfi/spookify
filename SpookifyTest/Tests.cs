using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Spookify;

namespace Spookify.UITests
{
	[TestFixture]
	public class Tests
	{

		[SetUp]
		public void BeforeEachTest ()
		{
		}


		[Test]
		public void TestSkipAudioBook()
		{
			var ab = SetupAudioBook ();

			var trackIndex = ab.CurrentPosition.TrackIndex;
			ab.SkipTrack (1);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex+1);

			ab.SkipTrack (-1);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex);

			ab.SkipTrack (2);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex+2);

			ab.SkipTrack (-2);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex);
		}
		[Test]
		public void TestForwardTimeAudioBook()
		{
			var ab = SetupAudioBook ();

			var trackIndex = ab.CurrentPosition.TrackIndex;
			var playbackPosition = ab.CurrentPosition.PlaybackPosition;
			ab.AdjustTime (1);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, playbackPosition + 1);

			ab.AdjustTime (-1);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, playbackPosition);

			ab.AdjustTime (30);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, playbackPosition + 30);

			ab.AdjustTime (-30);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, playbackPosition);

			ab.AdjustTime (60);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex + 1);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, 5);

			ab.AdjustTime (-60);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, playbackPosition);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex);

			ab.AdjustTime (123);
			ab.AdjustTime (-123);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, playbackPosition);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex);

			ab.AdjustTime (54 * 4);
			ab.AdjustTime (-54 * 4);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, playbackPosition);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex);
		}
		[Test]
		public void TestForwardTimeAudioBookGrenze()
		{
			var ab = SetupAudioBook ();
			var trackIndex = ab.CurrentPosition.TrackIndex;
			var playbackPosition = ab.CurrentPosition.PlaybackPosition;

			ab.AdjustTime (-123);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, playbackPosition);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, trackIndex);

			ab.AdjustTime (1000);
			Assert.AreEqual (ab.CurrentPosition.PlaybackPosition, 0);
			Assert.AreEqual (ab.CurrentPosition.TrackIndex, ab.Tracks.Count-1);

		}


		AudioBook SetupAudioBook()
		{
			var ab = new AudioBook ();
			ab.Tracks = new [] {
				new AudioBookTrack () { Index = 0, Duration = 55 },
				new AudioBookTrack () { Index = 1, Duration = 55 },
				new AudioBookTrack () { Index = 2, Duration = 55 },
				new AudioBookTrack () { Index = 3, Duration = 55 },
				new AudioBookTrack () { Index = 4, Duration = 55 },
				new AudioBookTrack () { Index = 5, Duration = 55 },
			}.ToList ();
			ab.CurrentPosition = new AudioBookBookmark () { PlaybackPosition = 0, TrackIndex = 0 };
			return ab;
		}
	}
}


