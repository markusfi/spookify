// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Simple
{
	[Register ("PlayerViewController")]
	partial class PlayerViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel artistLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView coverView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton PlayButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel titleLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel trackLabel { get; set; }

		[Action ("OnPressBack:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void OnPressBack (UIButton sender);

		[Action ("OnPressForward:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void OnPressForward (UIButton sender);

		[Action ("OnPressLogout:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void OnPressLogout (UIButton sender);

		[Action ("OnPressPlay:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void OnPressPlay (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (artistLabel != null) {
				artistLabel.Dispose ();
				artistLabel = null;
			}
			if (coverView != null) {
				coverView.Dispose ();
				coverView = null;
			}
			if (PlayButton != null) {
				PlayButton.Dispose ();
				PlayButton = null;
			}
			if (titleLabel != null) {
				titleLabel.Dispose ();
				titleLabel = null;
			}
			if (trackLabel != null) {
				trackLabel.Dispose ();
				trackLabel = null;
			}
		}
	}
}
