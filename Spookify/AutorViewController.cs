using System;

using UIKit;
using SpotifySDK;
using System.Linq;
using CoreFoundation;
using Foundation;
using CoreGraphics;
using System.Collections.Generic;

namespace Spookify
{
	public partial class AutorViewController : UIViewController
	{
		public AudioBook NewBook { get; set; }

		public AutorViewController () : base ("AutorViewController", null)
		{
		}
		public AutorViewController (IntPtr handle) : base (handle)
		{
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var source = new AutorTableViewSource () { AutorViewController = this };
			this.HoerbuchTableView.Source = source;
			source.Changed += (object sender, EventArgs e) => {
				this.HoerbuchTableView.ReloadData();
			};
			source.Selected += (tableView, indexPath) => {
				tableView.DeselectRow(indexPath, true);
			};
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
			} 
		}

		public class AutorTableViewSource : UITableViewSource
		{
			public AutorViewController AutorViewController { get; set; }

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
				if (this.AutorViewController != null && this.AutorViewController.NewBook != null) {
					var prettyString = new NSMutableAttributedString (string.Format("{0} ({1})",
							AutorViewController.NewBook.Artists.FirstOrDefault(),			
							(ThisAudioBookPlaylist != null && ThisAudioBookPlaylist.Books != null ? ThisAudioBookPlaylist.Books.Count : 1)),
						UIFont.FromName ("HelveticaNeue-Light", 15f));
					label.AttributedText = prettyString;
				}
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


			int semi = 0;
			AudioBookPlaylist _audioBookPlaylist;
			public AudioBookPlaylist ThisAudioBookPlaylist { 
				get 
				{ 
					if (_audioBookPlaylist == null) {
						_audioBookPlaylist = new AudioBookPlaylist ();
						if (AutorViewController.NewBook != null) {
							GetAlbum (AutorViewController.NewBook);
						}
					}
					return _audioBookPlaylist;
				}
			}
			void GetAlbum(AudioBook newBook)
			{
				if (semi == 0) {
					try {
						if (CurrentPlayer.Current.IsSessionValid) {
							semi++; 
							foreach (var author in newBook.Authors) {
								LoadPage(new NSUrl (author.URI), author);
							}
							if (semi == 1) {
								semi--;
								OnChanged ();
							}
						}
						else {
							if (CurrentPlayer.Current.CanRenewSession) {
								CurrentPlayer.Current.RenewSession(() => GetAlbum(newBook)); 
							}
						}
					} catch {
						// dieses hoerbuch hat fehlende Daten...
						semi = 0;
					}
				}
			}
			void LoadPage(NSUrl uri, Author author)
			{
				NSError nsError;
				var nsUrlRequestArtist = SPTArtist.CreateRequestForAlbumsByArtist (uri, SPTAlbumType.Album, CurrentPlayer.Current.AuthPlayer.Session.AccessToken, "DE", out nsError);
				if (nsError == null) {
					semi++; 
					SPTRequestHandlerProtocol_Extensions.Callback (SPTRequest.SPTRequestHandlerProtocol, nsUrlRequestArtist, (er, resp, jsonData) => {
						AddListPageAlben (jsonData, resp, er, author);

					});
				}
			}
			void AddListPageAlben(NSData jsonData, NSUrlResponse resp, NSError error, Author author)
			{
				try {
					if (error == null) {
						NSError nsError;
						var page = SPTListPage.ListPageFromData (jsonData, resp, true, "", out nsError);
						if (nsError == null) {
							if (page != null) {
								if (page.Items != null) {
									foreach (SPTPartialAlbum partialAlbum in page.Items) {
										ThisAudioBookPlaylist.Books.Add (new PlaylistBook () {
											Album = new AudioBookAlbum () { Name = partialAlbum.Name },
											SmallestCoverURL = partialAlbum.SmallestCover.ImageURL.AbsoluteString,
											LargestCoverURL = partialAlbum.LargestCover.ImageURL.AbsoluteString,
											Uri = partialAlbum.Uri.AbsoluteString,
											Authors = new List<Author> () { author }
										});
									}
								}
								if (page.HasNextPage) {
									LoadNextPage (page, author);
								}
							}
						}
					}
				}
				finally {
					semi--;
					if (semi <= 1) {
						semi = 0;
						OnChanged ();
					}
				}
			}
			void LoadNextPage(SPTListPage page, Author author)
			{
				NSError nsError;
				if (CurrentPlayer.Current.IsSessionValid) {
					if (page != null && page.HasNextPage) {
						var nsUrlRequest = page.CreateRequestForNextPageWithAccessToken (CurrentPlayer.Current.AuthPlayer.Session.AccessToken, out nsError);
						if (nsError == null) {
							semi++; 
							SPTRequestHandlerProtocol_Extensions.Callback (SPTRequest.SPTRequestHandlerProtocol, nsUrlRequest, (er, resp, jsonData) => {
								AddListPageAlben (jsonData, resp, er, author);
							});
						}
					}
				} else {
					if (CurrentPlayer.Current.CanRenewSession) {
						CurrentPlayer.Current.RenewSession(() => LoadNextPage(page, author)); 
					}
				}
			}

			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell ("HoerbuchCell") as HoerbuchTableViewCell;
				var audiobooks = ThisAudioBookPlaylist;
				if (audiobooks != null) {
					if (indexPath.Row >= audiobooks.Books.Count ()) {
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
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				if (section == 0 && ThisAudioBookPlaylist != null && ThisAudioBookPlaylist.Books != null) 
					return (nint)ThisAudioBookPlaylist.Books.Count;
				else
					return 0;
			}
		}
	}
}


