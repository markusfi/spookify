using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using CoreGraphics;

namespace Spookify
{
	public class ResultsTableViewController : UITableViewController
	{
		public GenreViewController GenreViewController { get; set; }
		public HoerbuchListeViewController HoerbuchListeViewController { get; set; }
		public UITableView HoerbuchTableView { get; set; }
		public List<PlaylistBook> FilteredPlaylist { get; set; }

		public bool LoadMoreCell;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.TableView.SeparatorColor = ConfigSpookify.TableSeparatorColor;
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
			this.TableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);

			this.TableView.BackgroundColor = ConfigSpookify.BackgroundColor;
			this.TableView.TableFooterView = new UIView (CGRect.Empty);
		}
		public override nint RowsInSection (UITableView tableview, nint section)
		{
			if (HoerbuchListeViewController == null && GenreViewController == null)
				return 0;
			if ((HoerbuchListeViewController != null && !HoerbuchListeViewController.ThisAudioBookPlaylist.IsComplete) ||
				(GenreViewController != null && !CurrentAudiobooks.Current.IsComplete))
			{
				LoadMoreCell = true;
				return FilteredPlaylist.Count + 1;
			} else {
				LoadMoreCell = false;
				return FilteredPlaylist.Count;
			}
		}
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Row < FilteredPlaylist.Count) {
				PlaylistBook book = FilteredPlaylist [indexPath.Row];
				var cell = HoerbuchTableView.DequeueReusableCell ("HoerbuchCell") as HoerbuchTableViewCell;
				HoerbuchListeDataSource.ConfigureCell (cell, book);
				return cell;
			} else {
				if (HoerbuchTableView != null) {
					
				}
				var cell = HoerbuchTableView.DequeueReusableCell ("HoerbuchLoadMoreCell");
				return cell;
			}
		}
	}
}

