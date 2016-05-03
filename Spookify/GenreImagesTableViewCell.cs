using System;

using Foundation;
using UIKit;

namespace Spookify
{
	public partial class GenreImagesTableViewCell : UITableViewCell
	{
		public GenreViewController GenreViewController { get; set; }
		public UserPlaylist UserPlaylist { get; set; }

		public GenreImagesTableViewCell (IntPtr handle) : base (handle)
		{
		}
		protected override void Dispose (bool disposing)
		{
			if (this.MoreButton != null) {
				this.MoreButton.TouchUpInside -= HandleClickOnMore;			
				this.MoreButton.Dispose ();
				this.MoreButton = null;
			}
			if (this.TitleButton != null) {
				this.TitleButton.TouchUpInside -= HandleClickOnMore;			
				this.TitleButton.Dispose ();
				this.TitleButton = null;
			}
			base.Dispose (disposing);
		}
		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
			if (this.MoreButton != null)
				this.MoreButton.TouchUpInside += HandleClickOnMore;
			if (this.TitleButton != null)
				this.TitleButton.TouchUpInside += HandleClickOnMore;
		}
		public override void PrepareForReuse ()
		{
			base.PrepareForReuse ();		
		}
		void HandleClickOnMore(object sender, EventArgs e)
		{
			if (this.UserPlaylist != null && this.GenreViewController != null)
				this.GenreViewController.PerformSegue("HB", this);
		}
		public override UIEdgeInsets LayoutMargins {
			get {
				return UIEdgeInsets.Zero;
			}
			set {
				base.LayoutMargins = value;
			}
		}	
	}
}
