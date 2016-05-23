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
	[Register ("HoerbuchTableViewCell")]
	partial class HoerbuchTableViewCell
	{
		[Outlet]
		public UIKit.UIImageView AlbumImage { get; private set; }

		[Outlet]
		public UIKit.UILabel AlbumLabel { get; private set; }

		[Outlet]
		public UIKit.UILabel AuthorLabel { get; private set; }

		[Outlet]
		public UIKit.UILabel DauerLabel { get; set; }

		[Outlet]
		public UIKit.UIProgressView progressBar { get; set; }

		[Outlet]
		public UIKit.UILabel ZeitLabel { get; private set; }
		
		void ReleaseDesignerOutlets ()
		{
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

			if (ZeitLabel != null) {
				ZeitLabel.Dispose ();
				ZeitLabel = null;
			}

			if (DauerLabel != null) {
				DauerLabel.Dispose ();
				DauerLabel = null;
			}

			if (progressBar != null) {
				progressBar.Dispose ();
				progressBar = null;
			}
		}
	}
}
