// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Spookify
{
	[Register ("HoerbuchViewController")]
	partial class HoerbuchViewController
	{
		[Outlet]
		public UIKit.UIImageView AlbumImage { get; private set; }

		[Outlet]
		public UIKit.UILabel AlbumLabel { get; private set; }

		[Outlet]
		public UIKit.UILabel AuthorLabel { get; private set; }

		[Outlet]
		UIKit.UIButton BuchSelektiertButton { get; set; }

		[Outlet]
		UIKit.UIView ContainerView { get; set; }

		[Outlet]
		UIKit.UITextView DescriptionTextView { get; set; }

		[Outlet]
		UIKit.UILabel LengthLabel { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }

		[Action ("OnBuchSelektiert:")]
		partial void OnBuchSelektiert (UIKit.UIButton sender);
		
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

			if (BuchSelektiertButton != null) {
				BuchSelektiertButton.Dispose ();
				BuchSelektiertButton = null;
			}

			if (DescriptionTextView != null) {
				DescriptionTextView.Dispose ();
				DescriptionTextView = null;
			}

			if (LengthLabel != null) {
				LengthLabel.Dispose ();
				LengthLabel = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}

			if (ContainerView != null) {
				ContainerView.Dispose ();
				ContainerView = null;
			}
		}
	}
}
