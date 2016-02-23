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
	[Register ("FirstViewController")]
	partial class FirstViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel statusLabel { get; set; }

		[Action ("OnButtonPressed:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void OnButtonPressed (UIButton sender);

		[Action ("OnClearCookies:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void OnClearCookies (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (statusLabel != null) {
				statusLabel.Dispose ();
				statusLabel = null;
			}
		}
	}
}
