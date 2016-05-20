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
