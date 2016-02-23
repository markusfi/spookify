using System;

using UIKit;
using Foundation;

namespace Spookify
{
	public partial class RechercheViewController : UIViewController
	{
		public string Url { get; set; }
		public RechercheViewController (IntPtr handle) : base (handle)
		{
		}
		public RechercheViewController () : base ("RechercheViewController", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			try {
				var url = new NSUrl(Url);
				this.MyWebView.LoadRequest(new NSUrlRequest(url));
			} catch {
			}
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}


