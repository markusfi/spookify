using System;

using UIKit;
using Foundation;
using System.Text.RegularExpressions;

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
				this.MyWebView.LoadFinished += MyWebView_LoadFinished;
			} catch {
			}
		}

		void MyWebView_LoadFinished (object sender, EventArgs e)
		{
			string html = this.MyWebView.EvaluateJavascript(@"document.body.innerHTML");	
			var match = Regex.Matches(html, "href=\"http://wwww.amazon.de/(.*?)\"", RegexOptions.Singleline); //spelling error
			foreach (Match m in match) {
				var url = m.Groups [1].Value;
			}
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}


