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

		UserPlaylist _thisAudioBookPlaylist = null;
		public UserPlaylist ThisAudioBookPlaylist { 
			get { return _thisAudioBookPlaylist; }
			set {
				_thisAudioBookPlaylist = value.Clone ();
				_thisAudioBookPlaylist.Books = value.Books.ToList();
				_thisAudioBookPlaylist.Books.Sort(PlaylistBook.CompareName);
			}
		}

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
				
			SetupSearch ();
		}
		void SetupSearch()
		{
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

			var tag = this.NavigationController.TabBarItem.Tag;
			if (tag != 3) 
				HoerbuchListeTableView.ScrollToRow(NSIndexPath.FromRowSection(0,0), UITableViewScrollPosition.Top, false);

			searchController.ObscuresBackgroundDuringPresentation = true;
			searchController.SearchBar.BackgroundColor = ConfigSpookify.BackgroundColor;
			searchController.SearchBar.BarTintColor = ConfigSpookify.BackgroundColor;
			searchController.SearchBar.TintColor = ConfigSpookify.BartTintColor;

			resultsTableController.TableView.WeakDelegate = this;
			searchController.SearchBar.WeakDelegate = this;

			foreach (UIView subview in this.HoerbuchListeTableView.Subviews) {
				subview.BackgroundColor = ConfigSpookify.BackgroundColor;
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
			if (!this.ThisAudioBookPlaylist.IsComplete) {
				// nothing to do...
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
					.GroupBy(p => p.Uri)
					.Select(g => g.First())
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
			view.BackgroundColor = label.BackgroundColor = ConfigSpookify.BackgroundColor;
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
			cell.AlbumImage.LoadImage (currentBook.MediumCoverUrl);
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


}


