using System;
using UIKit;

namespace Spookify
{
	
	[Foundation.Register ("ResizeTabBarController")]
	public class ResizeTabBarController : UITabBarController
	{
		public ResizeTabBarController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.View.Hidden = true;
			this.View.BackgroundColor = ConfigSpookify.BackgroundColor;

			var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
			if (appDelegate != null)
				appDelegate.TabBarController = this;

		}
		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();
			var tabFrame = this.TabBar.Frame;
			// this.TabBar.Frame = new CoreGraphics.CGRect (tabFrame.Left, tabFrame.Top, tabFrame.Width, tabFrame.Height);

			var transitionView = this.View.Subviews [0];
			var refercentView = this.View;
			transitionView.Frame = new CoreGraphics.CGRect (refercentView.Frame.Left, refercentView.Frame.Top, refercentView.Frame.Width, refercentView.Frame.Height - 50);
		}
	}
}

