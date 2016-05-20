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
	[Register ("ZuletztViewController")]
	partial class ZuletztViewController
	{
		[Outlet]
		UIKit.NSLayoutConstraint bottomConstraint { get; set; }

		[Outlet]
		UIKit.UITableView HoerbuchListeTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (bottomConstraint != null) {
				bottomConstraint.Dispose ();
				bottomConstraint = null;
			}
			if (HoerbuchListeTableView != null) {
				HoerbuchListeTableView.Dispose ();
				HoerbuchListeTableView = null;
			}
		}
	}
}
