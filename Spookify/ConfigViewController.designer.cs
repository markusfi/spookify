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
	[Register ("ConfigViewController")]
	partial class ConfigViewController
	{
		[Outlet]
		UIKit.UILabel _StatusRefreshLabel { get; set; }

		[Outlet]
		UIKit.UILabel _StatusSwapLabel { get; set; }

		[Outlet]
		UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		UIKit.UIButton RefreshButton { get; set; }

		[Outlet]
		UIKit.UISwitch RefreshURLSwitch { get; set; }

		[Outlet]
		UIKit.UILabel RefreshwitchLabel { get; set; }

		[Outlet]
		UIKit.UILabel SwapSwitchLabel { get; set; }

		[Outlet]
		UIKit.UISwitch SwapURLSwitch { get; set; }

		[Outlet]
		UIKit.UILabel VersionLabel { get; set; }

		[Action ("ValueChanged:")]
		partial void ValueChanged (Foundation.NSObject sender);

		[Action ("LoginClicked:")]
		partial void LoginClicked (UIKit.UIButton sender);

		[Action ("RenewSessionClicked:")]
		partial void RenewSessionClicked (UIKit.UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (_StatusRefreshLabel != null) {
				_StatusRefreshLabel.Dispose ();
				_StatusRefreshLabel = null;
			}
			if (_StatusSwapLabel != null) {
				_StatusSwapLabel.Dispose ();
				_StatusSwapLabel = null;
			}
			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}
			if (RefreshButton != null) {
				RefreshButton.Dispose ();
				RefreshButton = null;
			}
			if (RefreshURLSwitch != null) {
				RefreshURLSwitch.Dispose ();
				RefreshURLSwitch = null;
			}
			if (RefreshwitchLabel != null) {
				RefreshwitchLabel.Dispose ();
				RefreshwitchLabel = null;
			}
			if (SwapSwitchLabel != null) {
				SwapSwitchLabel.Dispose ();
				SwapSwitchLabel = null;
			}
			if (SwapURLSwitch != null) {
				SwapURLSwitch.Dispose ();
				SwapURLSwitch = null;
			}
			if (VersionLabel != null) {
				VersionLabel.Dispose ();
				VersionLabel = null;
			}
		}
	}
}
