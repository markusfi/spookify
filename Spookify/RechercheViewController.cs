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
			this.MyWebView.Hidden = true;
			this.MyWebView.Alpha = 0.01f;
			this.NothingFoundLabel.Hidden = false;
			this.NothingFoundLabel.Text = "ich google..."; 
			this.Spinner.Hidden = false;
			this.Spinner.StartAnimating ();
			this.View.BackgroundColor = UIColor.Black;

			try {
				var url = new NSUrl(Url);
				this.MyWebView.LoadRequest(new NSUrlRequest(url));
				this.MyWebView.LoadFinished += MyWebView_LoadFinished;
			} catch {
			}
		}

		void MyWebView_LoadFinished1 (object sender, EventArgs e)
		{
			this.MyWebView.LoadFinished -= MyWebView_LoadFinished1;
			this.MyWebView.Hidden = false;
			this.MyWebView.Alpha = 1;

			this.NothingFoundLabel.Hidden = true;
			this.Spinner.Hidden = true;
			this.Spinner.StopAnimating ();
		}

		void MyWebView_LoadFinished (object sender, EventArgs e)
		{
			this.MyWebView.LoadFinished -= MyWebView_LoadFinished;
			string html = this.MyWebView.EvaluateJavascript (@"s=''; for (i=0;i<document.getElementsByTagName('a').length;i++) (s += document.getElementsByTagName('a')[i].href + ' '); s");
			var arr = html.Split (' ');
			foreach (var a in arr) { 
				if (a.StartsWith ("https://www.amazon.de") || 
					a.StartsWith ("http://www.amazon.de") ||
					a.StartsWith ("http://www.buechertreff.de") ||
					a.StartsWith ("http://www.buechertreff.de")) {
					Console.WriteLine (a);
					var url = new NSUrl (a);
					this.MyWebView.LoadRequest (new NSUrlRequest (url));
					this.MyWebView.LoadFinished += MyWebView_LoadFinished1;
					this.NothingFoundLabel.Text = "ich lade...";
					return;
				}
			}
			this.NothingFoundLabel.Text = "Zu diesem Buch finde ich nichts..."; 
			this.NothingFoundLabel.Hidden = false;
			this.Spinner.Hidden = true;
			this.Spinner.StopAnimating ();
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}


