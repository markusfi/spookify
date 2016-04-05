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

			this.NavigationController.NavigationBar.BarStyle = UIBarStyle.BlackTranslucent;
			this.NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB (25, 25, 25);
			this.NavigationController.NavigationBar.Translucent = false;
			this.NavigationController.NavigationBar.TintColor = UIColor.White;

			// Perform any additional setup after loading the view, typically from a nib.
			var ds = new HoerbuecherSource ();
			this.HoerbuchTableView.Source = ds;
			ds.Changed += DataSourceChanged;
			ds.Selected += RowSelected;

			this.AutomaticallyAdjustsScrollViewInsets = false;

			HoerbuchTableView.TableFooterView = new UIView (CGRect.Empty);
		}
		public void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			// this.PerformSegue("Hoerbuecher",this);
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
				if (ds == null || ds.ObjectsNeedInitialisation())
					this.HoerbuchTableView.ReloadData ();
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
			NSIndexPath indexPath = this.HoerbuchTableView.IndexPathForSelectedRow;
			var p = (this.HoerbuchTableView.Source as HoerbuecherSource).Getplaylist (indexPath);
			var destinationViewController = segue.DestinationViewController as HoerbuchListeViewController;
			if (destinationViewController != null) {
				destinationViewController.PlayList = p;
			} else {
				var hoerbuchViewController = segue.DestinationViewController as HoerbuchViewController;
				if (hoerbuchViewController != null) {

					SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
					NSError errorOut;
					NSUrlRequest playlistReq = SPTPlaylistSnapshot.CreateRequestForPlaylistWithURI (p.Uri, auth.Session.AccessToken, out errorOut);
					SPTRequestHandlerProtocol_Extensions.Callback (SPTRequest.SPTRequestHandlerProtocol, playlistReq, (er, resp, dat) => {
						if (er != null) {
							return;
						}
						var page = SPTPlaylistSnapshot.PlaylistSnapshotFromData (dat, resp, out errorOut);
						if (page != null && page.FirstTrackPage != null && page.FirstTrackPage.Items != null && page.FirstTrackPage.Items.Any()) {
							var track = page.FirstTrackPage.Items.FirstOrDefault() as SPTPlaylistTrack;
							if (track != null) {
								var newPlaylistItem = new PlaylistBook() {
									Album = new AudioBookAlbum() {
										Name = track.Album.Name   
									},
									Authors = track.Artists.Cast<SPTPartialArtist>().Select(a => new Author() { Name = a.Name, URI = a.Uri.AbsoluteString }).ToList(),
									SmallestCoverURL = track.Album.SmallestCover.ImageURL.AbsoluteString,
									LargestCoverURL = track.Album.LargestCover.ImageURL.AbsoluteString,
									Uri = track.Album.GetUri().AbsoluteString
								};
								if (hoerbuchViewController.ViewDidAppearCalled) {
									hoerbuchViewController.InitialWithCurrentBook(newPlaylistItem);
								}
							}
						}
					});
				}
			}
		}
	}

	public class HoerbuecherSource : UITableViewSource
	{
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
			return _playListArray == null;
		}

		SPTPartialPlaylist[] _playListArray = null;
		SPTPartialPlaylist[] PlayListArray {
			get {
				if (this._playListArray == null) {
					if (this.PlaylistLists != null) {
						var items = PlaylistLists.Items as NSObject[];
						if (items != null) {
							this._playListArray = items.Select (a => a as SPTPartialPlaylist).ToArray ();
						}
					}
				}		
				return this._playListArray;
			}
		}
		SPTPlaylistList _playlistLists = null;
		SPTPlaylistList PlaylistLists {
			get {
				if (this._playlistLists == null) {
					if (CurrentPlayer.Current.IsSessionValid) {
						SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
						if (auth.Session != null) {
							SPTPlaylistList.PlaylistsForUser ("hoerbuecher", auth.Session.AccessToken, (nsError, obj) => {
								this._playlistLists = obj as SPTPlaylistList;
								this.OnChanged ();
							});
						}
					}
				}
				return this._playlistLists;
			}
		}			

		string[] keyList = "Geheimtipps,Alle,Bestseller,Neuheiten".Split(',');
		string keyTippDerWoche = "tipp der Woche";
		string keyHoerbuecher = "Hörbücher";
		string prefixHoerbuecher = "Hörbücher ";
		string prefixHoerspiele = "Hörspiele ";
		string[] sectionList = "Tipps der Woche,Hörbücher,Hörspiele,Genres".Split(',');

		public IEnumerable<SPTPartialPlaylist>GetSublist(nint section)
		{
			if (this.PlayListArray != null) {
				if (section == 0) {
					return this.PlayListArray.Where (p => p.Name.Contains (keyTippDerWoche));
				} else if (section == 1) {
					return this.PlayListArray.Where (p => p.Name.Contains (keyHoerbuecher) && keyList.Any(k => p.Name.Contains(k)));
				} else if (section == 2) {
					return this.PlayListArray.Where (p => !p.Name.Contains (keyHoerbuecher) && keyList.Any(k => p.Name.Contains(k)));
				} else if (section == 3) {
					return this.PlayListArray.Where (p => !p.Name.Contains (keyTippDerWoche) && !keyList.Any(k => p.Name.Contains(k)));
				}
			}
			return new SPTPartialPlaylist[0];
		}
			
		public int GetplaylistCount {		
			get {
				return this.PlayListArray != null ? this.PlayListArray.Length : 0;
			}
		}
		public SPTPartialPlaylist Getplaylist(Foundation.NSIndexPath indexPath) {	

			if (indexPath.LongSection >= 0 && indexPath.LongSection < 4) {
				var sublist = this.GetSublist (indexPath.LongSection);
				if (indexPath.Row >= 0 && indexPath.Row < sublist.Count())
					return sublist.ElementAt (indexPath.Row);
			}
			return null;
		}
			
		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			GenreTableViewCell cell = null;
			try {
				var p = Getplaylist(indexPath);
				cell = tableView.DequeueReusableCell (p.Name.Contains(keyTippDerWoche) ? "TippCell" : "GenereCell") as GenreTableViewCell;
				if (cell != null) {
					cell.GenereLabel.Text = p.Name.StartsWith(prefixHoerbuecher) ? p.Name.Substring(prefixHoerbuecher.Length) : p.Name.StartsWith(prefixHoerspiele) ? p.Name.Substring(prefixHoerspiele.Length) : p.Name;					
					var imageView = cell.GenereImage;
					if (imageView != null) {
						var nd = CurrentLRUCache.Current.CoverCache.GetItem(p.SmallestImage.ImageURL.AbsoluteString);
						if (nd != null) {
							imageView.Image = nd.Data.ToImage();
						}
						else {
							var gloalQueue = DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default);
							gloalQueue.DispatchAsync(() => 
							{
									NSError err = null;
									UIImage image = null;
									NSData imageData = NSData.FromUrl(p.SmallestImage.ImageURL, 0, out err);
									if (imageData != null) {
										CurrentLRUCache.Current.CoverCache.Insert(p.SmallestImage.ImageURL.AbsoluteString, imageData.ToByteArray());
										image = UIImage.LoadFromData(imageData);

										DispatchQueue.MainQueue.DispatchAsync(() => 
										{
											imageView.Image = image;
											if (image == null) {
												System.Diagnostics.Debug.WriteLine("Could not load image with error: {0}",err);
												return;
											}
										});
									}
							});
						}
					}
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (ex.ToString ());
			}
			return cell;
		}
		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return sectionList[section];
		}
		public override nint NumberOfSections (UITableView tableView)
		{
			return sectionList.Length;
		}
		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return this.GetSublist(section).Count();
		}

		public override UIView GetViewForHeader (UITableView tableView, nint section)
		{
			var view = new UIView (new CGRect (0, 0, tableView.Frame.Width, tableView.SectionHeaderHeight));
			var label = new UILabel ();
			view.BackgroundColor = label.BackgroundColor = UIColor.FromRGB (25, 25, 25);
			label.TextColor = UIColor.LightGray;
			var trackCount = this.GetSublist (section).Sum (x => (int)x.TrackCount);
			var prettyString = new NSMutableAttributedString (section == 0 
				? sectionList [section]
				: string.Format ("{0} ({1})", sectionList [section], trackCount),
				UIFont.FromName ("HelveticaNeue-Light", 15f));
			label.AttributedText = prettyString;

			label.TranslatesAutoresizingMaskIntoConstraints = false;
			view.AddSubview (label);
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, view, NSLayoutAttribute.Leading, 1f, 10f));
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1f, 10f));
			view.AddConstraint (NSLayoutConstraint.Create (label, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, view, NSLayoutAttribute.CenterY, 1f, 0));
			return view;
		}
	}
}


