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
			this.View.BackgroundColor = ConfigSpookify.BackgroundColor;
		}
		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();
			var tabFrame = this.TabBar.Frame;
			// this.TabBar.Frame = new CoreGraphics.CGRect (tabFrame.Left, tabFrame.Top, tabFrame.Width, tabFrame.Height);

			var transitionView = this.View.Subviews [0];
			transitionView.Frame = new CoreGraphics.CGRect (transitionView.Frame.Left, transitionView.Frame.Top, transitionView.Frame.Width, transitionView.Frame.Height - 50);
		}
	}
}

