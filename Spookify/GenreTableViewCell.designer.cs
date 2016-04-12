// WARNING
//
// This file has been generated automatically by Xamarin Studio Indie to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

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
