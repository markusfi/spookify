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
	[Register ("AutorViewController")]
	partial class AutorViewController
	{
		[Outlet]
		UIKit.UITableView HoerbuchTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (HoerbuchTableView != null) {
				HoerbuchTableView.Dispose ();
				HoerbuchTableView = null;
			}
		}
	}
}
