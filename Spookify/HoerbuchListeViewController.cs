using System;
using System.Linq;
using UIKit;
using SpotifySDK;
using Foundation;
using CoreFoundation;

namespace Spookify
{
	public partial class HoerbuchListeViewController : UIViewController
	{
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
			};
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			// if (segue.Identifier.Equals(""))
			NSIndexPath indexPath = this.HoerbuchListeTableView.IndexPathForSelectedRow;
			var destinationViewController = segue.DestinationViewController as HoerbuchViewController;

			if (ThisAudioBookPlaylist.Books.Count > indexPath.Row) {
				try {
					destinationViewController.Book = ThisAudioBookPlaylist.Books[indexPath.Row];
				} catch {
					
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
					cell.AlbumLabel.Text = "wird geladen...";
					cell.AuthorLabel.Text = "";
					cell.AlbumImage.Image = null;
				}
				else 
				{
					var currentBook = audiobooks.Books[indexPath.Row];
					cell.AlbumLabel.Text = currentBook.Album.Name;
					cell.AuthorLabel.Text = currentBook.Artists.FirstOrDefault ();
					var imageView = cell.AlbumImage;
					if (imageView != null) {
						imageView.Image = null;
						var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
						gloalQueue.DispatchAsync (() => {
							NSError err = null;
							UIImage image = null;
							NSData imageData = NSData.FromUrl( new NSUrl(currentBook.SmallestCoverURL), 0, out err);
							if (imageData != null)
								image = UIImage.LoadFromData (imageData);

							DispatchQueue.MainQueue.DispatchAsync (() => {
								imageView.Image = image;
								if (image == null) {
									System.Diagnostics.Debug.WriteLine ("Could not load image with error: {0}", err);
									return;
								}
							});
						});
					}
				}
			}
			return cell;
		}


/*      
        public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("HoerbuchCell") as HoerbuchTableViewCell;
			var p = PlaylistSnapshot;
			if (p != null && p.FirstTrackPage != null && p.FirstTrackPage.Items != null) {
				if (p.FirstTrackPage.Items.Length > indexPath.Row) {
					try {
						var track = p.FirstTrackPage.Items.ElementAt (indexPath.Row) as SPTPlaylistTrack;
						if (track.Album != null)
							cell.AlbumLabel.Text = track.Album.Name;
						if (track.Artists != null)
							cell.AuthorLabel.Text = track.Artists.Cast<SPTPartialArtist>().Select(a => a.Name).FirstOrDefault();
						var imageView = cell.AlbumImage;
						if (imageView != null && track.Album != null) {
							imageView.Image = null;
							var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
							gloalQueue.DispatchAsync (() => {
								NSError err = null;
								UIImage image = null;
								NSData imageData = NSData.FromUrl( track.Album.SmallestCover.ImageURL, 0, out err);
								if (imageData != null)
									image = UIImage.LoadFromData (imageData);

								DispatchQueue.MainQueue.DispatchAsync (() => {
									imageView.Image = image;
									if (image == null) {
										System.Diagnostics.Debug.WriteLine ("Could not load image with error: {0}", err);
										return;
									}
								});
							});
						}
					} catch (Exception ex) {
						System.Diagnostics.Debug.WriteLine (ex.ToString ());
					}
				}
			}
			return cell;
		}
*/


		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}
		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return hoerbuchListeViewController.PlayList.Name;
		}
		public override nint RowsInSection (UITableView tableView, nint section)
		{
			if (section == 0)
				return (nint) hoerbuchListeViewController.PlayList.TrackCount;
			else
				return 0;
		}
	}
}


