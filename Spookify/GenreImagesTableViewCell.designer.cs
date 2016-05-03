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
	[Register ("GenreImagesTableViewCell")]
	partial class GenreImagesTableViewCell
	{
		[Outlet]
		public UIKit.UICollectionView CollectionView { get; private set; }

		[Outlet]
		public UIKit.UIButton MoreButton { get; set; }

		[Outlet]
		public UIKit.UIButton TitleButton { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CollectionView != null) {
				CollectionView.Dispose ();
				CollectionView = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (MoreButton != null) {
				MoreButton.Dispose ();
				MoreButton = null;
			}

			if (TitleButton != null) {
				TitleButton.Dispose ();
				TitleButton = null;
			}
		}
	}
}
