using System;
using UIKit;

namespace Spookify
{
	public partial class GenreCollectionViewCell : UICollectionViewCell
	{
		public PlaylistBook Book { get; set; } 
		public GenreViewController GenreViewController { get; set; }

		public GenreCollectionViewCell (IntPtr handle) : base (handle)
		{
		}
		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
			ImageView.UserInteractionEnabled = true;
			ImageView.AddGestureRecognizer(new UITapGestureRecognizer(() => { this.GenreViewController.PerformSegue("HB1", this); }));
		}
		public override void PrepareForReuse ()
		{
			base.PrepareForReuse ();
			this.ImageView.Image = null;
		}
	}
}

