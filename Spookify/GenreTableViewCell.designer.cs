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
		public UIKit.UIImageView GenereImage { get; set; }

		[Outlet]
		public UIKit.UILabel GenereLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (GenereImage != null) {
				GenereImage.Dispose ();
				GenereImage = null;
			}
			if (GenereLabel != null) {
				GenereLabel.Dispose ();
				GenereLabel = null;
			}
		}
	}
}
