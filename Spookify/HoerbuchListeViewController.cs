using System;
using System.Linq;
using UIKit;
using SpotifySDK;
using Foundation;
using CoreFoundation;
using System.Collections.Generic;
using CoreGraphics;

namespace Spookify
{
	public partial class HoerbuchListeViewController : UIViewController
	{
		ResultsTableViewController resultsTableController;
		UISearchController searchController;
		bool searchControllerWasActive;
		bool searchControllerSearchFieldWasFirstResponder;

		public UserPlaylist ThisAudioBookPlaylist { get; set; }

		public HoerbuchListeViewController (IntPtr handle) : base (handle)
		{
		}
		public HoerbuchListeViewController () : base ("HoerbuchListeViewController", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.HoerbuchListeTableView.SeparatorColor = UIColor.FromRGB (50, 50, 50);
			this.HoerbuchListeTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
			this.HoerbuchListeTableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);

			this.AutomaticallyAdjustsScrollViewInsets = false;

			var ds = new HoerbuchListeDataSource () { hoerbuchListeViewController = this };
			this.HoerbuchListeTableView.Source = ds;
			ds.Changed += (object sender, EventArgs e) => {
				this.HoerbuchListeTableView.ReloadData();
				this.UpdateSearchResults();
			};
			ds.Selected += RowSelected;

			HoerbuchListeTableView.TableFooterView = new UIView (CGRect.Empty);
				
			resultsTableController = new ResultsTableViewController() {
				HoerbuchTableView = this.HoerbuchListeTableView,
				FilteredPlaylist = new List<PlaylistBook> ()
			};
			resultsTableController.TableView.RowHeight = 88;

			searchController = new UISearchController (resultsTableController) {
				WeakDelegate = this,
				DimsBackgroundDuringPresentation = false,
				WeakSearchResultsUpdater = this
			};

			searchController.SearchBar.SizeToFit ();

			HoerbuchListeTableView.TableHeaderView = searchController.SearchBar;
			HoerbuchListeTableView.ScrollToRow(NSIndexPath.FromRowSection(0,0), UITableViewScrollPosition.Top, false);

			searchController.ObscuresBackgroundDuringPresentation = true;
			searchController.SearchBar.BackgroundColor = UIColor.FromRGB (25, 25, 25);
			searchController.SearchBar.BarTintColor = UIColor.FromRGB (25, 25, 25);
			searchController.SearchBar.TintColor = UIColor.White;

			resultsTableController.TableView.WeakDelegate = this;
			searchController.SearchBar.WeakDelegate = this;

			foreach (UIView subview in this.HoerbuchListeTableView.Subviews) {
				subview.BackgroundColor = UIColor.FromRGB (25, 25, 25);
			}

			DefinesPresentationContext = true;

			if (searchControllerWasActive) {
				searchController.Active = searchControllerWasActive;
				searchControllerWasActive = false;

				if (searchControllerSearchFieldWasFirstResponder) {
					searchController.SearchBar.BecomeFirstResponder ();
					searchControllerSearchFieldWasFirstResponder = false;
				}
			}
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (!CurrentPlayer.Current.IsSessionValid && CurrentPlayer.Current.CanRenewSession) {
				CurrentPlayer.Current.RenewSession ();
			}
		}
		public void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			tableView.DeselectRow(indexPath, true);
		}
		public void UpdateSearchResults()
		{
			UpdateSearchResultsForSearchController (searchController);
		}

		[Export ("searchBarSearchButtonClicked:")]
		public virtual void SearchButtonClicked (UISearchBar searchBar)
		{
			searchBar.ResignFirstResponder ();
		}
		[Export ("updateSearchResultsForSearchController:")]
		public virtual void UpdateSearchResultsForSearchController (UISearchController searchController)
		{
			var tableController = (ResultsTableViewController)searchController.SearchResultsController;
			tableController.HoerbuchListeViewController = this;
			var newResult = PerformSearch (searchController.SearchBar.Text);
			if (!newResult.SequenceEqual (tableController.FilteredPlaylist)) {
				tableController.FilteredPlaylist = newResult;
				tableController.TableView.ReloadData ();
			} 
			if (this.ThisAudioBookPlaylist.TrackCount > (nuint)this.ThisAudioBookPlaylist.Books.Count) {
				var ds = this.HoerbuchListeTableView.DataSource as HoerbuchListeDataSource;
				if (ds != null)
					ds.LoadMore ();
			}
			else if (tableController.LoadMoreCell) 
			{
				tableController.TableView.ReloadData ();
			}
		}
		List<PlaylistBook> PerformSearch(string searchString)
		{
			searchString = searchString.Trim ();
			string[] searchItems = string.IsNullOrEmpty (searchString)
				? new string[0]
				: searchString.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			var filteredBooks = new List<PlaylistBook> ();

			IEnumerable<PlaylistBook> query =
				ThisAudioBookPlaylist.Books.Where (p => 
					searchItems.All(item =>
						p.Album.Name.IndexOf (item, StringComparison.OrdinalIgnoreCase) >= 0 || 
						p.Artists.Any(a => a.IndexOf (item, StringComparison.OrdinalIgnoreCase) >= 0)
					))
					.OrderBy (p => p.Album.Name);

			filteredBooks.AddRange (query);
			var list = filteredBooks.Distinct ().ToList ();
			return list;
		}
		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			var cell = sender as HoerbuchTableViewCell;
			if (cell != null) {
				var destinationViewController = segue.DestinationViewController as HoerbuchViewController;
				try {
					destinationViewController.Book = cell.CurrentPlaylistBook;
				} catch {

				}
			} else {
				NSIndexPath indexPath = this.HoerbuchListeTableView.IndexPathForSelectedRow;
				var destinationViewController = segue.DestinationViewController as HoerbuchViewController;

				if (ThisAudioBookPlaylist.Books.Count > indexPath.Row) {
					try {
						destinationViewController.Book = ThisAudioBookPlaylist.Books [indexPath.Row];
					} catch {
					
					}
				}
			}
		}
	}
	public class HoerbuchListeDataSource : UITableViewSource
	{
		public HoerbuchListeViewController hoerbuchListeViewController { get; set; }
		public delegate void RowSelectedEventHandler (UITableView tableView, NSIndexPath indexPath);

		public event RowSelectedEventHandler Selected;
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			OnRowSelected (tableView, indexPath);
		}
		private void OnRowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			if (Selected != null)
				Selected (tableView, indexPath);
		}

		public override UIView GetViewForHeader (UITableView tableView, nint section)
		{
			var view = new UIView (new CGRect (0, 0, tableView.Frame.Width, tableView.SectionHeaderHeight));
			var label = new UILabel ();
			view.BackgroundColor = label.BackgroundColor = UIColor.FromRGB (25, 25, 25);
			label.TextColor = UIColor.LightGray;
			var prettyString = new NSMutableAttributedString (string.Format ("{0} ({1})", hoerbuchListeViewController.ThisAudioBookPlaylist.Name, hoerbuchListeViewController.ThisAudioBookPlaylist.TrackCount),
				UIFont.FromName ("HelveticaNeue-Light", 15f));
			label.AttributedText = prettyString;

			label.TranslatesAutoresizingMaskIntoConstraints = false;
			view.AddSubview (label);
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, view, NSLayoutAttribute.Leading, 1f, 10f));
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1f, 10f));
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, view, NSLayoutAttribute.CenterY, 1f, 0));
			return view;
		}
	
		public event EventHandler Changed;
		private void OnChanged() {
			DispatchQueue.MainQueue.DispatchAsync(() => {
				if (Changed != null) 
					Changed (this, EventArgs.Empty);
			});
		}

		public void LoadMore() 
		{
			var audiobooks = this.hoerbuchListeViewController.ThisAudioBookPlaylist;
			if (audiobooks != null) {
				// if (hoerbuchListeViewController.PlayList.TrackCount > (nuint) hoerbuchListeViewController.ThisAudioBookPlaylist.Books.Count)
				//	LoadNextPageAsync (audiobooks.CurrentPage);
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("HoerbuchCell") as HoerbuchTableViewCell;
			var audiobooks = this.hoerbuchListeViewController.ThisAudioBookPlaylist;
			if (audiobooks != null) {
				if (indexPath.Row >= audiobooks.Books.Count ()) {
					// LoadNextPageAsync (audiobooks.CurrentPage);
					cell.AlbumLabel.Text = "";
					cell.AuthorLabel.Text = "";
					cell.AlbumImage.Image = null;
				} else {
					var currentBook = audiobooks.Books [indexPath.Row];
					ConfigureCell (cell, currentBook);
				}
			}
			return cell;
		}
		public static void ConfigureCell(HoerbuchTableViewCell cell, PlaylistBook currentBook)
		{
			cell.CurrentPlaylistBook = currentBook;
			cell.AlbumLabel.Text = currentBook.Album.Name;
			cell.AuthorLabel.Text = currentBook.Artists.FirstOrDefault ();
			var imageView = cell.AlbumImage;
			if (imageView != null) {
				imageView.Image = null;
				var nd = CurrentLRUCache.Current.CoverCache.GetItem(currentBook.SmallestCoverURL);
				if (nd != null) {
					imageView.Image = nd.Data.ToImage();
				}
				else {
					var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
					gloalQueue.DispatchAsync (() => {
						NSError err = null;
						UIImage image = null;
						NSData data = NSData.FromUrl( new NSUrl(currentBook.SmallestCoverURL), 0, out err);
						if (data != null) {
							CurrentLRUCache.Current.CoverCache.Insert(currentBook.SmallestCoverURL,data.ToByteArray());
							image = UIImage.LoadFromData (data);

							DispatchQueue.MainQueue.DispatchAsync (() => {
								imageView.Image = image;
								if (image == null) {
									System.Diagnostics.Debug.WriteLine ("Could not load image with error: {0}", err);
									return;
								}
							});
						}
					});
				}
			}
		}
		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}
		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return string.Format(" {0} ({1})",hoerbuchListeViewController.ThisAudioBookPlaylist.Name, hoerbuchListeViewController.ThisAudioBookPlaylist.TrackCount);
		}
		public override nint RowsInSection (UITableView tableView, nint section)
		{
			if (section == 0) 
				return (nint)hoerbuchListeViewController.ThisAudioBookPlaylist.TrackCount;
			else
				return 0;
		}
	}

	public class ResultsTableViewController : UITableViewController
	{
		public HoerbuchListeViewController HoerbuchListeViewController { get; set; }
		public UITableView HoerbuchTableView { get; set; }
		public List<PlaylistBook> FilteredPlaylist { get; set; }

		public bool LoadMoreCell;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.TableView.SeparatorColor = UIColor.FromRGB (50, 50, 50);
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
			this.TableView.SeparatorInset = new UIEdgeInsets (0, 0, 0, 0);

			this.TableView.BackgroundColor = UIColor.FromRGB (25, 25, 25);
			this.TableView.TableFooterView = new UIView (CGRect.Empty);
		}
		public override nint RowsInSection (UITableView tableview, nint section)
		{
			if (HoerbuchListeViewController == null)
				return 0;
			if (HoerbuchListeViewController.ThisAudioBookPlaylist.TrackCount > (nuint)HoerbuchListeViewController.ThisAudioBookPlaylist.Books.Count) {
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
					var ds = HoerbuchTableView.Source as HoerbuchListeDataSource;
					if (ds != null)
						ds.LoadMore ();
				}
				var cell = HoerbuchTableView.DequeueReusableCell ("HoerbuchLoadMoreCell");
				return cell;
			}
		}
	}
}


