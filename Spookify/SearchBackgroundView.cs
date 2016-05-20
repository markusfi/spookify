using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ObjCRuntime;

namespace Spookify
{
	partial class SearchBackgroundView : UIView
	{
		public event EventHandler ShowGengres;

		public SearchBackgroundView (IntPtr handle) : base (handle)
		{
		}
		public static SearchBackgroundView Create()
		{

			var arr = NSBundle.MainBundle.LoadNib ("SearchBackgroundview", null, null);
			var v = Runtime.GetNSObject<SearchBackgroundView> (arr.ValueAt(0));
			if (v != null) {
				v.LupeImageView.Image = UIImage.FromBundle ("Lupe")?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				v.LupeImageView.TintColor = UIColor.LightGray;
			}
			return v;
		}

		partial void OnShowGenres (UIButton sender)
		{
			if (ShowGengres != null)
				ShowGengres(sender, EventArgs.Empty);
		}
	}
}
