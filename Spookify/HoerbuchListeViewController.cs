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
		ResultsTableController resultsTableController;
		UISearchController searchController;
		bool searchControllerWasActive;
		bool searchControllerSearchFieldWasFirstResponder;

		public SPTPartialPlaylist PlayList { get; set; }
		AudioBookPlaylist _audioBookPlaylist;
		public AudioBookPlaylist ThisAudioBookPlaylist { 
			get 
			{ 
				if (_audioBookPlaylist == null)
					_audioBookPlaylist = new AudioBookPlaylist ();
				return _audioBookPlaylist;
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
			// Perform any additional setup after loading the view, typically from a nib.
			this.HoerbuchListeTableView.Delegate = new HoerbuchListeDelegate() { hoerbuchListeViewController = this };
			var ds = new HoerbuchListeDataSource () { hoerbuchListeViewController = this };
			this.HoerbuchListeTableView.DataSource = ds;
			ds.Changed += (object sender, EventArgs e) => {
				this.HoerbuchListeTableView.ReloadData();
				this.UpdateSearchResults();
			};
			HoerbuchListeTableView.TableFooterView = new UIView (CGRect.Empty);
				
			resultsTableController = new ResultsTableController() {
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

			resultsTableController.TableView.WeakDelegate = this;
			searchController.SearchBar.WeakDelegate = this;

			resultsTableController.TableView.TableFooterView = new UIView (CGRect.Empty);

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
			var tableController = (ResultsTableController)searchController.SearchResultsController;
			tableController.HoerbuchListeViewController = this;
			var newResult = PerformSearch (searchController.SearchBar.Text);
			if (!newResult.SequenceEqual (tableController.FilteredPlaylist)) {
				tableController.FilteredPlaylist = newResult;
				tableController.TableView.ReloadData ();
			} 
			if (this.PlayList.TrackCount < (nuint)this.ThisAudioBookPlaylist.Books.Count) {
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
					destinationViewController.Book = cell.CurrentBook;
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
	public class HoerbuchListeDelegate : UITableViewDelegate
	{
		public HoerbuchListeViewController hoerbuchListeViewController { get; set; }
	}
	public class HoerbuchListeDataSource : UITableViewDataSource
	{
		public event EventHandler Changed;
		private void OnChanged() {
			DispatchQueue.MainQueue.DispatchAsync(() => {
				if (Changed != null) 
					Changed (this, EventArgs.Empty);
			});
		}

		public HoerbuchListeViewController hoerbuchListeViewController { get; set; }
		SPTPlaylistSnapshot _playlistSnapshot = null;

		bool semi = false;
		public SPTPlaylistSnapshot PlaylistSnapshot
		{
			get 
			{	
				if (this._playlistSnapshot == null) 
					InitPlaylistSnapshot ();
				return this._playlistSnapshot;
			}
		}

		void InitPlaylistSnapshot()
		{
			if (!semi) {
				try {
					semi = true; 
					if (this._playlistSnapshot == null) {
						if (CurrentPlayer.Current.IsSessionValid) {
							SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
							var p = SPTRequest.SPTRequestHandlerProtocol;
							NSError errorOut;
							NSUrlRequest playlistReq = SPTPlaylistSnapshot.CreateRequestForPlaylistWithURI (hoerbuchListeViewController.PlayList.Uri, auth.Session.AccessToken, out errorOut);
							SPTRequestHandlerProtocol_Extensions.Callback (p, playlistReq, (er, resp, dat) => {
								semi = false;
								if (er != null) {
									return;
								}
								this._playlistSnapshot = SPTPlaylistSnapshot.PlaylistSnapshotFromData (dat, resp, out errorOut);
								AddListPageTracks(this._playlistSnapshot.FirstTrackPage);
							});
						}
					}
				} catch {
					// dieses hoerbuch hat fehlende Daten...
				}
			}
		}

		void AddListPageTracks(SPTListPage page)
		{
			if (page != null && page.Items != null) {
				try {
					var playlist = hoerbuchListeViewController.ThisAudioBookPlaylist;
					foreach (SPTPlaylistTrack track in page.Items)
					{
						var newPlaylistItem = new PlaylistBook();
						newPlaylistItem.Album = new AudioBookAlbum() {
							Name = track.Album.Name
						};
						newPlaylistItem.Artists = track.Artists.Cast<SPTPartialArtist>().Select(a => a.Name).ToList();
						newPlaylistItem.SmallestCoverURL = track.Album.SmallestCover.ImageURL.AbsoluteString;
						newPlaylistItem.LargestCoverURL = track.Album.LargestCover.ImageURL.AbsoluteString;
						newPlaylistItem.Uri = track.Album.GetUri().AbsoluteString;
						playlist.Books.Add(newPlaylistItem);
					}
					if (page.HasNextPage) {
						playlist.NextPageURL = page.NextPageURL.AbsoluteString;
						playlist.CurrentPage = page;
					}
					OnChanged ();
				} catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine (ex.ToString ());
				}
			}
		}

		public void LoadMore() 
		{
			var audiobooks = this.hoerbuchListeViewController.ThisAudioBookPlaylist;
			if (audiobooks != null) {
				if (hoerbuchListeViewController.PlayList.TrackCount > (nuint) hoerbuchListeViewController.ThisAudioBookPlaylist.Books.Count)
					LoadNextPageAsync (audiobooks.CurrentPage);
			}
		}
		void LoadNextPageAsync(SPTListPage page)
		{
			NSError errorOut, nsError;
			if (CurrentPlayer.Current.IsSessionValid) {
				if (page != null && page.HasNextPage) {
					if (!semi) {
						semi = true;
						SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
						var p = SPTRequest.SPTRequestHandlerProtocol;
						var nsUrlRequest = page.CreateRequestForNextPageWithAccessToken (auth.Session.AccessToken, out errorOut);
						SPTRequestHandlerProtocol_Extensions.Callback (p, nsUrlRequest, (er1, resp1, jsonData1) => {
							var nextpage = SPTListPage.ListPageFromData (jsonData1, resp1, true, "", out nsError);
							if (nextpage != null) {
								AddListPageTracks (nextpage);
							}
							semi = false;
						});
					}
				} else {
					InitPlaylistSnapshot ();
				}
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("HoerbuchCell") as HoerbuchTableViewCell;
			var audiobooks = this.hoerbuchListeViewController.ThisAudioBookPlaylist;
			if (audiobooks != null) {
				if (indexPath.Row >= audiobooks.Books.Count ()) {
					LoadNextPageAsync (audiobooks.CurrentPage);
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
			cell.CurrentBook = currentBook;
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
			return string.Format(" {0} ({1})",hoerbuchListeViewController.PlayList.Name, hoerbuchListeViewController.PlayList.TrackCount);
		}
		public override nint RowsInSection (UITableView tableView, nint section)
		{
			if (section == 0) 
				return (nint)hoerbuchListeViewController.PlayList.TrackCount;
			else
				return 0;
		}
	}

	public class ResultsTableController : UITableViewController
	{
		public HoerbuchListeViewController HoerbuchListeViewController { get; set; }
		public UITableView HoerbuchTableView { get; set; }
		public List<PlaylistBook> FilteredPlaylist { get; set; }

		public bool LoadMoreCell;

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			if (HoerbuchListeViewController == null)
				return 0;
			if (HoerbuchListeViewController.PlayList.TrackCount > (nuint)HoerbuchListeViewController.ThisAudioBookPlaylist.Books.Count) {
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
					var ds = HoerbuchTableView.DataSource as HoerbuchListeDataSource;
					if (ds != null)
						ds.LoadMore ();
				}
				var cell = HoerbuchTableView.DequeueReusableCell ("HoerbuchLoadMoreCell");
				return cell;
			}
		}
	}
}


