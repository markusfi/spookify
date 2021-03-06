// WARNING
//
// This file has been generated automatically by Xamarin Studio Professional to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Spookify
{
	[Register ("PlayerViewController")]
	partial class PlayerViewController
	{
		[Outlet]
		UIKit.UIView ActivityIndicatorBackgroundView { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView ActivityIndicatorView { get; set; }

		[Outlet]
		UIKit.UIView Airplay { get; set; }

		[Outlet]
		public UIKit.UIImageView AlbumImage { get; private set; }

		[Outlet]
		UIKit.UILabel AlbumLabel { get; set; }

		[Outlet]
		UIKit.UILabel AuthorLabel { get; set; }

		[Outlet]
		UIKit.UILabel bisEndeBuchLabel { get; set; }

		[Outlet]
		UIKit.UILabel bisEndeKapitelLabel { get; set; }

		[Outlet]
		UIKit.UIButton CloseButton { get; set; }

		[Outlet]
		UIKit.UILabel kapitelLabel { get; set; }

		[Outlet]
		UIKit.UIButton LesezeichenButton { get; set; }

		[Outlet]
		UIKit.UIButton MoreButton { get; set; }

		[Outlet]
		UIKit.UIButton NextTrack { get; set; }

		[Outlet]
		UIKit.UIButton PlayButton { get; set; }

		[Outlet]
		UIKit.UIButton PrevTrack { get; set; }

		[Outlet]
		UIKit.UIProgressView ProgressBar { get; set; }

		[Outlet]
		UIKit.UILabel seitStartKapitelLabel { get; set; }

		[Outlet]
		UIKit.UIButton SendenButton { get; set; }

		[Outlet]
		UIKit.UIButton SkipBackward { get; set; }

		[Outlet]
		UIKit.UIButton SkipForward { get; set; }

		[Outlet]
		UIKit.UIButton SleepButton { get; set; }

		[Outlet]
		UIKit.UILabel SleepTimerLabel { get; set; }

		[Outlet]
		UIKit.UILabel TrackLabel { get; set; }

		[Action ("OnAddBookmark:")]
		partial void OnAddBookmark (UIKit.UIButton sender);

		[Action ("OnBackTime:")]
		partial void OnBackTime (UIKit.UIButton sender);

		[Action ("OnCloseButtonClicked:")]
		partial void OnCloseButtonClicked (UIKit.UIButton sender);

		[Action ("OnForwardTime:")]
		partial void OnForwardTime (UIKit.UIButton sender);

		[Action ("OnMoreButtonClicked:")]
		partial void OnMoreButtonClicked (UIKit.UIButton sender);

		[Action ("OnNextTrack:")]
		partial void OnNextTrack (UIKit.UIButton sender);

		[Action ("OnPlay:")]
		partial void OnPlay (UIKit.UIButton sender);

		[Action ("OnPrevTrack:")]
		partial void OnPrevTrack (UIKit.UIButton sender);

		[Action ("OnSendenButtonClicked:")]
		partial void OnSendenButtonClicked (UIKit.UIButton sender);

		[Action ("OnSleeptimer:")]
		partial void OnSleeptimer (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (ActivityIndicatorBackgroundView != null) {
				ActivityIndicatorBackgroundView.Dispose ();
				ActivityIndicatorBackgroundView = null;
			}

			if (ActivityIndicatorView != null) {
				ActivityIndicatorView.Dispose ();
				ActivityIndicatorView = null;
			}

			if (Airplay != null) {
				Airplay.Dispose ();
				Airplay = null;
			}

			if (AlbumImage != null) {
				AlbumImage.Dispose ();
				AlbumImage = null;
			}

			if (AlbumLabel != null) {
				AlbumLabel.Dispose ();
				AlbumLabel = null;
			}

			if (AuthorLabel != null) {
				AuthorLabel.Dispose ();
				AuthorLabel = null;
			}

			if (bisEndeBuchLabel != null) {
				bisEndeBuchLabel.Dispose ();
				bisEndeBuchLabel = null;
			}

			if (bisEndeKapitelLabel != null) {
				bisEndeKapitelLabel.Dispose ();
				bisEndeKapitelLabel = null;
			}

			if (CloseButton != null) {
				CloseButton.Dispose ();
				CloseButton = null;
			}

			if (kapitelLabel != null) {
				kapitelLabel.Dispose ();
				kapitelLabel = null;
			}

			if (LesezeichenButton != null) {
				LesezeichenButton.Dispose ();
				LesezeichenButton = null;
			}

			if (MoreButton != null) {
				MoreButton.Dispose ();
				MoreButton = null;
			}

			if (NextTrack != null) {
				NextTrack.Dispose ();
				NextTrack = null;
			}

			if (PlayButton != null) {
				PlayButton.Dispose ();
				PlayButton = null;
			}

			if (PrevTrack != null) {
				PrevTrack.Dispose ();
				PrevTrack = null;
			}

			if (ProgressBar != null) {
				ProgressBar.Dispose ();
				ProgressBar = null;
			}

			if (seitStartKapitelLabel != null) {
				seitStartKapitelLabel.Dispose ();
				seitStartKapitelLabel = null;
			}

			if (SendenButton != null) {
				SendenButton.Dispose ();
				SendenButton = null;
			}

			if (SkipBackward != null) {
				SkipBackward.Dispose ();
				SkipBackward = null;
			}

			if (SkipForward != null) {
				SkipForward.Dispose ();
				SkipForward = null;
			}

			if (SleepButton != null) {
				SleepButton.Dispose ();
				SleepButton = null;
			}

			if (SleepTimerLabel != null) {
				SleepTimerLabel.Dispose ();
				SleepTimerLabel = null;
			}

			if (TrackLabel != null) {
				TrackLabel.Dispose ();
				TrackLabel = null;
			}
		}
	}
}
