using System;
using System.Linq;
using UIKit;
using SpotifySDK;
using Foundation;
using System.Collections.Generic;
using CoreFoundation;
using CoreGraphics;
using ObjCRuntime;

namespace Spookify
{
	public partial class GenreViewController : UIViewController 
	{
		ResultsTableViewController resultsTableController;
		UISearchController searchController;
		bool searchControllerWasActive;
		bool searchControllerSearchFieldWasFirstResponder;
		public bool doNotDisplayList = false;

		public GenreViewController (IntPtr handle) : base (handle)
		{
		}
		public GenreViewController () : base ("HoerbuecherViewController", null)
		{
		}
		public override UIStatusBarStyle PreferredStatusBarStyle ()
		{
			return UIStatusBarStyle.LightContent;
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.NavigationController.NavigationBar.TopItem.Title = this.NavigationController.TabBarItem.Title;

			this.NavigationController.NavigationBar.BarStyle = UIBarStyle.BlackTranslucent;
			this.NavigationController.NavigationBar.BarTintColor = ConfigSpookify.BackgroundColor;
			this.NavigationController.NavigationBar.Translucent = false;
			this.NavigationController.NavigationBar.TintColor = ConfigSpookify.BartTintColor;

			// Perform any additional setup after loading the view, typically from a nib.
			var ds = new HoerbuecherSource (this);
			this.HoerbuchTableView.Source = ds;
			ds.Changed += DataSourceChanged;
			ds.Selected += RowSelected;
			this.HoerbuchTableView.EstimatedRowHeight = (125+155)/2;
			this.HoerbuchTableView.RowHeight = UITableView.AutomaticDimension;
			this.AutomaticallyAdjustsScrollViewInsets = false;

			HoerbuchTableView.TableFooterView = new UIView (CGRect.Empty);

			SetupSearch ();
			var tag = this.NavigationController.TabBarItem.Tag;
			if (tag == 3) {
				this.doNotDisplayList = true;
				this.HoerbuchTableView.Subviews [1].BackgroundColor = UIColor.Clear;
				this.HoerbuchTableView.BackgroundColor = UIColor.Clear;
				var bv = SearchBackgroundView.Create ();
				bv.Frame = this.HoerbuchTableView.Bounds;
				bv.BackgroundColor = ConfigSpookify.BackgroundColor;
				this.HoerbuchTableView.BackgroundView = bv;
				bv.ShowGengres += (object sender, EventArgs e) => {
					this.doNotDisplayList = false;
					this.HoerbuchTableView.ReloadData();
				};
			}
		}

		void SetupSearch()
		{
			var tag = this.NavigationController.TabBarItem.Tag;
			if (tag >= 1) {

				resultsTableController = new ResultsTableViewController () {
					HoerbuchTableView = this.HoerbuchTableView,
					FilteredPlaylist = new List<PlaylistBook> ()
				};
				resultsTableController.TableView.RowHeight = 88;

				searchController = new UISearchController (resultsTableController) {
					WeakDelegate = this,
					DimsBackgroundDuringPresentation = false,
					WeakSearchResultsUpdater = this
				};

				searchController.SearchBar.SizeToFit ();

				HoerbuchTableView.TableHeaderView = searchController.SearchBar;
				if (CurrentAudiobooks.Current.HasPlaylists) {
					if (tag < 3) {
						if (HoerbuchTableView.NumberOfRowsInSection (0) > 0)
							HoerbuchTableView.ScrollToRow (NSIndexPath.FromRowSection (0, 0), UITableViewScrollPosition.Top, false);
						else
							Console.WriteLine ("Problem");
					}
				}
				searchController.ObscuresBackgroundDuringPresentation = true;
				searchController.SearchBar.BackgroundColor = ConfigSpookify.BackgroundColor;
				searchController.SearchBar.BarTintColor = ConfigSpookify.BackgroundColor;
				searchController.SearchBar.TintColor = ConfigSpookify.BartTintColor;

				resultsTableController.TableView.WeakDelegate = this;
				searchController.SearchBar.WeakDelegate = this;

				DefinesPresentationContext = true;

				foreach (UIView subview in this.HoerbuchTableView.Subviews) {
					subview.BackgroundColor = ConfigSpookify.BackgroundColor ;
				}

				if (searchControllerWasActive) {
					searchController.Active = searchControllerWasActive;
					searchControllerWasActive = false;

					if (searchControllerSearchFieldWasFirstResponder) {
						searchController.SearchBar.BecomeFirstResponder ();
						searchControllerSearchFieldWasFirstResponder = false;
					}
				}
			}
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
			var tag = this.NavigationController.TabBarItem.Tag;
			if (tag == 3 && !this.doNotDisplayList) {
				this.doNotDisplayList = true;
				this.HoerbuchTableView.ReloadData ();
			}

			var resultTableViewController = (ResultsTableViewController)searchController.SearchResultsController;
			resultTableViewController.HoerbuchListeViewController = null;
			resultTableViewController.GenreViewController = this;
			var newResult = PerformSearch (searchController.SearchBar.Text);
			if (!newResult.SequenceEqual (resultTableViewController.FilteredPlaylist)) {
				resultTableViewController.FilteredPlaylist = newResult;
				resultTableViewController.TableView.ReloadData ();
			} 
		}
		List<PlaylistBook> PerformSearch(string searchString)
		{
			searchString = searchString.Trim ();
			string[] searchItems = string.IsNullOrEmpty (searchString)
				? new string[0]
				: searchString.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			var query =
				CurrentAudiobooks.Current.User.Playlists.SelectMany (pl => pl.Books).Where (p => 
					searchItems.All (item =>
						p.Album.Name.IndexOf (item, StringComparison.OrdinalIgnoreCase) >= 0 ||
				p.Artists.Any (a => a.IndexOf (item, StringComparison.OrdinalIgnoreCase) >= 0)
				))
				.GroupBy (p => p.Uri)
				.Select (g => g.First ())
				.OrderBy(p => p.Album.Name)
				.ToList();

			if (query.Count<100)
				query.Sort (PlaylistBook.CompareName);
			return query;
		}

		public void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			this.PerformSegue("HB",this);
			tableView.DeselectRow(indexPath, true);

		}
		void DataSourceChanged (object sender, EventArgs args)
		{
			this.HoerbuchTableView.ReloadData ();
		}
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (!CurrentPlayer.Current.IsSessionValid && CurrentPlayer.Current.CanRenewSession) {
				CurrentPlayer.Current.RenewSession (() => {
					if (CurrentPlayer.Current.IsSessionValid) {
						BeginInvokeOnMainThread (LoadGenereData);
					}
				});
			} else
				LoadGenereData ();
		}

		void LoadGenereData()
		{
			if (CurrentPlayer.Current.IsSessionValid) {
				var ds = this.HoerbuchTableView.Source as HoerbuecherSource;
				if (ds == null || ds.ObjectsNeedInitialisation ()) {
					ApplicationHelper.AsyncLoadWhenSession ();
					this.HoerbuchTableView.ReloadData ();
				}
			}
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			// if (segue.Identifier.Equals(""))
			if (sender is GenreImagesTableViewCell) {
				var genreImagesTableViewCell = sender as GenreImagesTableViewCell;
				var destinationViewController = segue.DestinationViewController as HoerbuchListeViewController;
				if (destinationViewController != null && genreImagesTableViewCell != null) {
					destinationViewController.ThisAudioBookPlaylist = genreImagesTableViewCell.UserPlaylist;
				}
			} else if (sender is GenreCollectionViewCell) {
				var genreCollectionViewCell = sender as GenreCollectionViewCell;
				var hoerbuchViewController = segue.DestinationViewController as HoerbuchViewController;
				if (hoerbuchViewController != null && genreCollectionViewCell != null) {
					hoerbuchViewController.InitialWithCurrentBook (genreCollectionViewCell.Book);
				}
			} else if ((sender is HoerbuchTableViewCell) &&
				(segue.DestinationViewController is HoerbuchViewController) &&
				((sender as HoerbuchTableViewCell).CurrentPlaylistBook != null)) {
				var cell = sender as HoerbuchTableViewCell;
				var hoerbuchViewController = segue.DestinationViewController as HoerbuchViewController;
				hoerbuchViewController.InitialWithCurrentBook (cell.CurrentPlaylistBook);				
			} else {
				NSIndexPath indexPath = this.HoerbuchTableView.IndexPathForSelectedRow;
				if (indexPath != null) {
					var p = (this.HoerbuchTableView.Source as HoerbuecherSource).Getplaylist (indexPath);
					var destinationViewController = segue.DestinationViewController as HoerbuchListeViewController;
					if (destinationViewController != null) {
						destinationViewController.ThisAudioBookPlaylist = p;
					}
				} else {
					var hoerbuchViewController = segue.DestinationViewController as HoerbuchViewController;
					if (hoerbuchViewController != null) {
						var p = (this.HoerbuchTableView.Source as HoerbuecherSource).Getplaylist (NSIndexPath.FromItemSection (segue.Identifier == "HB1" ? 0 : 1, 0));
						hoerbuchViewController.InitialWithCurrentBook (p.Books.FirstOrDefault ());
					}
				}
			}
		}
	}

	public class HoerbuecherSource : UITableViewSource
	{
		GenreViewController genreViewController;

		public HoerbuecherSource(GenreViewController genreViewController)
		{
			this.genreViewController = genreViewController; 
			CurrentAudiobooks.Current.Changed += (object sender, PlaylistChangedEventArgs e) => {
				var visibleRows = this.genreViewController.HoerbuchTableView.IndexPathsForVisibleRows;
				var sections = this.genreViewController.HoerbuchTableView.NumberOfSections();

				for (nint i=0;i<sections;i++) {
					if (this.genreViewController.HoerbuchTableView.NumberOfRowsInSection(i) != 
						this.RowsInSection(this.genreViewController.HoerbuchTableView, i) 
						|| string.IsNullOrEmpty(e.Name)
						|| e.Name.Contains("Geheimtipps")
					) {
						OnChanged();

					}
				}
			};
		}
			
		public delegate void RowSelectedEventHandler (UITableView tableView, NSIndexPath indexPath);

		public event RowSelectedEventHandler Selected;
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			var tag = this.genreViewController?.NavigationController?.TabBarItem?.Tag;
			if (tag != 2 || indexPath.Row != 0 || indexPath.Section != 0)
				OnRowSelected (tableView, indexPath);
		}
		private void OnRowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			if (Selected != null)
				Selected (tableView, indexPath);
		}
		public override void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
/*			if (tableView.RespondsToSelector (new Selector ("setSeparatorInset:")))
				tableView.SeparatorInset = UIEdgeInsets.Zero;
			if (tableView.RespondsToSelector (new Selector ("setLayoutMargins:")))
				tableView.LayoutMargins = UIEdgeInsets.Zero;
			if (cell.RespondsToSelector (new Selector ("setSeparatorInset:")))
				cell.SeparatorInset = UIEdgeInsets.Zero;
			*/		
		}

		public event EventHandler Changed;
		private void OnChanged() {
			DispatchQueue.MainQueue.DispatchAsync(() => {
				if (Changed != null) 
					Changed (this, EventArgs.Empty);
			});
		}
		public bool ObjectsNeedInitialisation() {
			return !CurrentAudiobooks.Current.HasPlaylists;
		}

		List<UserPlaylist> PlaylistLists {
			get {
				return CurrentAudiobooks.Current.User.Playlists; 
			}
		}

		string[] keyList = "Geheimtipps,Bestseller,Neuheiten,Charts Top,All Audiobooks".Split(',');
		string[] startList = "Alle Hörbücher,Alle Hörspiele,Mit Star bewertet".Split(',');
		string keyTippDerWoche = "tipp der Woche";
		string keyHoerbuecher = "Hörbücher";
		string[] sectionList = "Tipps der Woche,Hörbücher,Hörspiele,Genres".Split(',');

		public string[] GetBarSectionList()
		{
			var tag = this.genreViewController.NavigationController.TabBarItem.Tag;
			if (tag == 1)
				return sectionList.Skip (3).Take (1).ToArray ();
			else if (tag == 2)
				return sectionList.Take (3).ToArray ();
			else
				return new string[0];
		}
			
		public IEnumerable<UserPlaylist>GetBarSublist(nint section)
		{
			var tag = this.genreViewController.NavigationController.TabBarItem.Tag;
			if (tag == 1)
				return GetSublist(section == 0 ? 3 : -1);
			else if (tag == 2)
				return GetSublist  (section < 3 ? section : -1);
			else if (tag == 3)
				return GetSublist(4);
			else 
				return GetSublist (-1);
		}
		public int GetBarNumberOfSection()
		{
			var tag = this.genreViewController.NavigationController.TabBarItem.Tag;
			if (tag == 1)
				return 1;
			else if (tag == 2)
				return 3;
			else if (tag == 3)
				return 1;
			else
				return 0;
		}

		public IEnumerable<UserPlaylist>GetSublist(nint section)
		{
			return GetSublistInternal (section).Where (b => b.Books != null && b.Books.Any ());
		}
		public IEnumerable<UserPlaylist>GetSublistInternal(nint section)
		{
			if (this.genreViewController.doNotDisplayList)
				return new UserPlaylist[0];
			if (this.PlaylistLists != null) {
				if (section == 0) {
					return this.PlaylistLists.Where (p => p.Name.Contains (keyTippDerWoche));
				} else if (section == 1) {
					return this.PlaylistLists.Where (p => p.Name.Contains (keyHoerbuecher) && (startList.Any(k => p.Name.StartsWith(k)) || keyList.Any (k => p.Name.Contains (k))));
				} else if (section == 2) {
					return this.PlaylistLists.Where (p => !p.Name.Contains (keyHoerbuecher) && (startList.Any(k => p.Name.StartsWith(k)) || keyList.Any (k => p.Name.Contains (k))));
				} else if (section == 3) {
					return this.PlaylistLists.Where (p => !p.Name.Contains (keyTippDerWoche) && !(startList.Any(k => p.Name.StartsWith(k)) || keyList.Any (k => p.Name.Contains (k))));
				} else if (section == 4) {
					return this.PlaylistLists.Where (p => !p.Name.Contains (keyTippDerWoche));
				}
			}
			return new UserPlaylist[0];
		}
			
		public int GetplaylistCount {		
			get {
				return this.PlaylistLists != null ? this.PlaylistLists.Count : 0;
			}
		}
		public UserPlaylist Getplaylist(Foundation.NSIndexPath indexPath) {	

			if (indexPath.Section >= 0 && indexPath.Section < 4) {
				var sublist = this.GetBarSublist (indexPath.LongSection);
				if (indexPath.Row >= 0 && indexPath.Row < sublist.Count())
					return sublist.ElementAt (indexPath.Row);
			}
			return null;
		}

		UITableViewCell ConfigureCollectionViewCell(UITableView tableView, UserPlaylist playlist, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (indexPath.Row % 2 == 1 ? "ImagesCell" : "ImagesCellTiny") as GenreImagesTableViewCell;
			if (cell != null && playlist != null) {
				cell.CollectionView.Source = new CollectionViewDataSource(this.genreViewController, playlist);
				cell.TitleButton.SetTitle(playlist.Name, UIControlState.Normal);
				cell.MoreButton.SetTitle ("mehr...", UIControlState.Normal);
				cell.GenreViewController = this.genreViewController; 
				cell.UserPlaylist = playlist;
			}
			return cell;
		}
		public class CollectionViewDataSource :  UICollectionViewSource
		{
			GenreViewController _genreViewController;
			UserPlaylist _playlist;
			public CollectionViewDataSource(GenreViewController genreViewController, UserPlaylist playlist) {
				_playlist = playlist;
				_genreViewController = genreViewController;
			}
			public override System.nint NumberOfSections (UIKit.UICollectionView collectionView)
			{
				return 1;
			}
			public override nint GetItemsCount (UICollectionView collectionView, nint section)
			{
				return _playlist.Books.Count ();
			}
			public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
			{
				var collectionViewCell = collectionView.DequeueReusableCell ("CollectionViewImageCell", indexPath) as GenreCollectionViewCell;
				if (collectionViewCell != null) {
					collectionViewCell.Book = _playlist.Books.ElementAt((int)indexPath.Item);
					collectionViewCell.GenreViewController = _genreViewController;
					collectionViewCell.ImageView.Hidden = true;
					collectionViewCell.ImageView.BackgroundColor = UIColor.Red;
					collectionViewCell.ImageView.LoadImage(_playlist.Books.ElementAt((int)indexPath.Item));
				}
				return collectionViewCell;
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var tag = this.genreViewController.NavigationController.TabBarItem.Tag;
			GenreTableViewCell cell = null;
			try {
				var p = Getplaylist(indexPath);

				bool tippCell = tag == 2 && p != null && p.Name != null && p.Name.Contains(keyTippDerWoche);

				if ((tag == 1 || tag == 2) && !tippCell)
					return ConfigureCollectionViewCell(tableView, p, indexPath);
				
				cell = tableView.DequeueReusableCell (tippCell ? "TippCell" : "GenereCell") as GenreTableViewCell;
				if (cell != null) {
					if (!tippCell) {
						cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
						if (p != null && p.Name != null)
							cell.GenereLabel.Text = p.Name; // p.Name.StartsWith(prefixHoerbuecher) ? p.Name.Substring(prefixHoerbuecher.Length) : p.Name.StartsWith(prefixHoerspiele) ? p.Name.Substring(prefixHoerspiele.Length) : p.Name;					
					}

					cell.GenereImage.LoadImage(p, tippCell);
					if (tippCell) {						
						cell.SelectionStyle = UITableViewCellSelectionStyle.None;
						cell.GenereImage.AddGestureRecognizer(new UITapGestureRecognizer(() => { this.genreViewController.PerformSegue("HB1",cell.GenereImage); }));
						cell.GenereImage2.AddGestureRecognizer(new UITapGestureRecognizer(() => { this.genreViewController.PerformSegue("HB2",cell.GenereImage); }));
						var p2 = Getplaylist(NSIndexPath.FromItemSection(indexPath.Item+1,indexPath.Section));
						cell.GenereImage2.LoadImage(p2, tippCell);
					}
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (ex.ToString ());
			}
			if (cell != null)
				cell.BackgroundColor = ConfigSpookify.BackgroundColor;
			return cell;
		}
		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var tag = this.genreViewController.NavigationController.TabBarItem.Tag;
			if (tag == 1 || tag == 2) {
				var p = Getplaylist (indexPath);
				if (p != null) {
					bool tippCell = p.Name.Contains (keyTippDerWoche);
					return tippCell ? (tableView.Frame.Width/2+15+21) : (indexPath.Row % 2 == 1 ? 125 : 155); 
				}
			}
			return 88;
		}

		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return null; //GetBarSectionList()[section];
		}
		public override nint NumberOfSections (UITableView tableView)
		{
			return CurrentAudiobooks.Current.HasPlaylists ? GetBarNumberOfSection()
				: 0;
		}
		public override nint RowsInSection (UITableView tableView, nint section)
		{			
			if (!CurrentAudiobooks.Current.HasPlaylists)
				return 0;
			if (section == 0) {
				var tag = this.genreViewController.NavigationController.TabBarItem.Tag;
				if (tag == 2) {
					var p = Getplaylist(NSIndexPath.FromRowSection(0,section));
					bool tippCell = tag == 2 && p != null && p.Name != null && p.Name.Contains(keyTippDerWoche);
					return tippCell ? 1 : 0;
				}
			}
			return this.GetBarSublist(section).Count();
		}
		/*
		public override UIView GetViewForHeader (UITableView tableView, nint section)
		{
			var view = new UIView (new CGRect (0, 0, tableView.Frame.Width, tableView.SectionHeaderHeight));
			var label = new UILabel ();
			view.BackgroundColor = label.BackgroundColor = ConfigSpookify.BackgroundColor;
			label.TextColor = UIColor.LightGray;
			var trackCount = this.GetBarSublist (section).Sum (x => (int)x.TrackCount);
			var prettyString = new NSMutableAttributedString (section == 0 
				? GetBarSectionList() [section]
				: string.Format ("{0} ({1})", GetBarSectionList() [section], trackCount),
				UIFont.FromName ("HelveticaNeue-Light", 15f));
			label.AttributedText = prettyString;

			label.TranslatesAutoresizingMaskIntoConstraints = false;
			view.AddSubview (label);
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, view, NSLayoutAttribute.Leading, 1f, 10f));
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1f, 10f));
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, view, NSLayoutAttribute.CenterY, 1f, 0));
			return view;
		}*/
	}
}


