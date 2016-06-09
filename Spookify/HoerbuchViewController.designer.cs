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
		UIKit.UIImageView BewertungImageview { get; set; }

		[Outlet]
		UIKit.UILabel BewertungLabel { get; set; }

		[Outlet]
		UIKit.UIButton BuchPlayButton { get; set; }

		[Outlet]
		UIKit.UIButton BuchSelektiertButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ContainerBottomLayoutConstraint { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ContainerHeightLayoutConstraint { get; set; }

		[Outlet]
		UIKit.UIView ContainerView { get; set; }

		[Outlet]
		UIKit.UITextView DescriptionTextView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint DescriptionTextViewHeightLayoutConstraint { get; set; }

		[Outlet]
		UIKit.UILabel LengthLabel { get; set; }

		[Outlet]
		UIKit.UIButton MehrVomAutorButton { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		UIKit.UIButton SucheAmazonButton { get; set; }

		[Outlet]
		UIKit.UIButton SucheBuechertreffButton { get; set; }

		[Outlet]
		UIKit.UIButton SuchehoebudeButton { get; set; }

		[Action ("OnBuchAnhoeren:")]
		partial void OnBuchAnhoeren (UIKit.UIButton sender);

		[Action ("OnBuchAufMerkliste:")]
		partial void OnBuchAufMerkliste (UIKit.UIButton sender);

		[Action ("OnBuchSelektiert:")]
		partial void OnBuchSelektiert (UIKit.UIButton sender);

		[Action ("OnSucheAmazon:")]
		partial void OnSucheAmazon (UIKit.UIButton sender);

		[Action ("OnSucheBuechertreff:")]
		partial void OnSucheBuechertreff (UIKit.UIButton sender);
		
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

			if (BewertungImageview != null) {
				BewertungImageview.Dispose ();
				BewertungImageview = null;
			}

			if (BewertungLabel != null) {
				BewertungLabel.Dispose ();
				BewertungLabel = null;
			}

			if (BuchPlayButton != null) {
				BuchPlayButton.Dispose ();
				BuchPlayButton = null;
			}

			if (BuchSelektiertButton != null) {
				BuchSelektiertButton.Dispose ();
				BuchSelektiertButton = null;
			}

			if (ContainerBottomLayoutConstraint != null) {
				ContainerBottomLayoutConstraint.Dispose ();
				ContainerBottomLayoutConstraint = null;
			}

			if (ContainerHeightLayoutConstraint != null) {
				ContainerHeightLayoutConstraint.Dispose ();
				ContainerHeightLayoutConstraint = null;
			}

			if (ContainerView != null) {
				ContainerView.Dispose ();
				ContainerView = null;
			}

			if (DescriptionTextView != null) {
				DescriptionTextView.Dispose ();
				DescriptionTextView = null;
			}

			if (DescriptionTextViewHeightLayoutConstraint != null) {
				DescriptionTextViewHeightLayoutConstraint.Dispose ();
				DescriptionTextViewHeightLayoutConstraint = null;
			}

			if (LengthLabel != null) {
				LengthLabel.Dispose ();
				LengthLabel = null;
			}

			if (MehrVomAutorButton != null) {
				MehrVomAutorButton.Dispose ();
				MehrVomAutorButton = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}

			if (SucheAmazonButton != null) {
				SucheAmazonButton.Dispose ();
				SucheAmazonButton = null;
			}

			if (SucheBuechertreffButton != null) {
				SucheBuechertreffButton.Dispose ();
				SucheBuechertreffButton = null;
			}

			if (SuchehoebudeButton != null) {
				SuchehoebudeButton.Dispose ();
				SuchehoebudeButton = null;
			}
		}
	}
}
