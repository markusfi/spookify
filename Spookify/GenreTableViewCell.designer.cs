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

namespace Spookify
{
	[Register ("GenreTableViewCell")]
	partial class GenreTableViewCell
	{
		[Outlet]
		public UIKit.UIImageView GenereImage { get; private set; }

		[Outlet]
		public UIKit.UIImageView GenereImage2 { get; set; }

		[Outlet]
		public UIKit.UILabel GenereLabel { get; private set; }

		[Action ("onImage2Clicked:")]
		partial void onImage2Clicked (Foundation.NSObject sender);

		[Action ("onImageClicked:")]
		partial void onImageClicked (Foundation.NSObject sender);

		void ReleaseDesignerOutlets ()
		{
			if (GenereImage != null) {
				GenereImage.Dispose ();
				GenereImage = null;
			}
			if (GenereImage2 != null) {
				GenereImage2.Dispose ();
				GenereImage2 = null;
			}
			if (GenereLabel != null) {
				GenereLabel.Dispose ();
				GenereLabel = null;
			}
		}
	}
}
