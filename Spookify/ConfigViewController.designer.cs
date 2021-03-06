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
	[Register ("ConfigViewController")]
	partial class ConfigViewController
	{
		[Outlet]
		UIKit.UILabel _StatusRefreshLabel { get; set; }

		[Outlet]
		UIKit.UILabel _StatusSwapLabel { get; set; }

		[Outlet]
		UIKit.UIView InfoTextBottomTransaprentView { get; set; }

		[Outlet]
		UIKit.UIScrollView InfoTextScrollView { get; set; }

		[Outlet]
		UIKit.UIView InfoTextTopTransaprentView { get; set; }

		[Outlet]
		UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		UIKit.UIButton LogoutButton { get; set; }

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
		UIKit.UILabel TextLabel { get; set; }

		[Outlet]
		UIKit.UILabel VersionLabel { get; set; }

		[Action ("LoginClicked:")]
		partial void LoginClicked (UIKit.UIButton sender);

		[Action ("LogoutClicked:")]
		partial void LogoutClicked (UIKit.UIButton sender);

		[Action ("RenewSessionClicked:")]
		partial void RenewSessionClicked (UIKit.UIButton sender);

		[Action ("ValueChanged:")]
		partial void ValueChanged (Foundation.NSObject sender);
		
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

			if (InfoTextScrollView != null) {
				InfoTextScrollView.Dispose ();
				InfoTextScrollView = null;
			}

			if (InfoTextTopTransaprentView != null) {
				InfoTextTopTransaprentView.Dispose ();
				InfoTextTopTransaprentView = null;
			}

			if (InfoTextBottomTransaprentView != null) {
				InfoTextBottomTransaprentView.Dispose ();
				InfoTextBottomTransaprentView = null;
			}

			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}

			if (LogoutButton != null) {
				LogoutButton.Dispose ();
				LogoutButton = null;
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

			if (TextLabel != null) {
				TextLabel.Dispose ();
				TextLabel = null;
			}
		}
	}
}
