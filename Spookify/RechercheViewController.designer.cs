// WARNING
//
// This file has been generated automatically by Xamarin Studio Community to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Spookify
{
	[Register ("RechercheViewController")]
	partial class RechercheViewController
	{
		[Outlet]
		public UIKit.UIWebView MyWebView { get; private set; }

		[Outlet]
		UIKit.UILabel NothingFoundLabel { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView Spinner { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MyWebView != null) {
				MyWebView.Dispose ();
				MyWebView = null;
			}

			if (Spinner != null) {
				Spinner.Dispose ();
				Spinner = null;
			}

			if (NothingFoundLabel != null) {
				NothingFoundLabel.Dispose ();
				NothingFoundLabel = null;
			}
		}
	}
}
