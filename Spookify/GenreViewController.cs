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

			this.NavigationController.NavigationBar.TopItem.Title = this.NavigationController.TabBarItem.Title;

			this.NavigationController.NavigationBar.BarStyle = UIBarStyle.BlackTranslucent;
			this.NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB (25, 25, 25);
			this.NavigationController.NavigationBar.Translucent = false;
			this.NavigationController.NavigationBar.TintColor = UIColor.White;

			// Perform any additional setup after loading the view, typically from a nib.
			var ds = new HoerbuecherSource (this);
			this.HoerbuchTableView.Source = ds;
			ds.Changed += DataSourceChanged;
			ds.Selected += RowSelected;

			this.AutomaticallyAdjustsScrollViewInsets = false;

			HoerbuchTableView.TableFooterView = new UIView (CGRect.Empty);
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
			if (sender is GenreImagesTableViewCell) {
				var genreImagesTableViewCell = sender as GenreImagesTableViewCell;
				var destinationViewController = segue.DestinationViewController as HoerbuchListeViewController;
				if (destinationViewController != null && genreImagesTableViewCell != null) {
					destinationViewController.ThisAudioBookPlaylist = genreImagesTableViewCell.UserPlaylist;
				}
			} 
			else if (sender is GenreCollectionViewCell) {
				var genreCollectionViewCell = sender as GenreCollectionViewCell;
				var hoerbuchViewController = segue.DestinationViewController as HoerbuchViewController;
				if (hoerbuchViewController != null && genreCollectionViewCell != null) {
					hoerbuchViewController.InitialWithCurrentBook (genreCollectionViewCell.Book);
				}
			}
			else {
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
			CurrentAudiobooks.Current.User.Changed += (object sender, PlaylistChangedEventArgs e) => {
				OnChanged();
			};
		}
			
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
			return false; //  _playListArray == null;
		}

		List<UserPlaylist> PlaylistLists {
			get {
				return CurrentAudiobooks.Current.User.Playlists; 
			}
		}

		string[] keyList = "Geheimtipps,Alle,Bestseller,Neuheiten".Split(',');
		string keyTippDerWoche = "tipp der Woche";
		string keyHoerbuecher = "Hörbücher";
		string prefixHoerbuecher = "Hörbücher ";
		string prefixHoerspiele = "Hörspiele ";
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
			else
				return 0;
		}

		public IEnumerable<UserPlaylist>GetSublist(nint section)
		{
			if (this.PlaylistLists != null) {
				if (section == 0) {
					return this.PlaylistLists.Where (p => p.Name.Contains (keyTippDerWoche));
				} else if (section == 1) {
					return this.PlaylistLists.Where (p => p.Name.Contains (keyHoerbuecher) && keyList.Any(k => p.Name.Contains(k)));
				} else if (section == 2) {
					return this.PlaylistLists.Where (p => !p.Name.Contains (keyHoerbuecher) && keyList.Any(k => p.Name.Contains(k)));
				} else if (section == 3) {
					return this.PlaylistLists.Where (p => !p.Name.Contains (keyTippDerWoche) && !keyList.Any(k => p.Name.Contains(k)));
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

			if (indexPath.LongSection >= 0 && indexPath.LongSection < 4) {
				var sublist = this.GetBarSublist (indexPath.LongSection);
				if (indexPath.Row >= 0 && indexPath.Row < sublist.Count())
					return sublist.ElementAt (indexPath.Row);
			}
			return null;
		}

		UITableViewCell ConfigureCollectionViewCell(UITableView tableView, UserPlaylist playlist, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (indexPath.Row % 2 == 1 ? "ImagesCell" : "ImagesCellTiny") as GenreImagesTableViewCell;
			if (cell != null) {
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
					LoadImage(collectionViewCell.ImageView, _playlist.Books.ElementAt((int)indexPath.Item));
				}
				return collectionViewCell;
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			GenreTableViewCell cell = null;
			try {
				var p = Getplaylist(indexPath);
				bool tippCell = p != null && p.Name != null && p.Name.Contains(keyTippDerWoche);
				if (!tippCell)
					return ConfigureCollectionViewCell(tableView, p, indexPath);
				cell = tableView.DequeueReusableCell (tippCell ? "TippCell" : "GenereCell") as GenreTableViewCell;
				if (cell != null) {
					if (!tippCell) {
						cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
						if (p != null && p.Name != null)
							cell.GenereLabel.Text = p.Name.StartsWith(prefixHoerbuecher) ? p.Name.Substring(prefixHoerbuecher.Length) : p.Name.StartsWith(prefixHoerspiele) ? p.Name.Substring(prefixHoerspiele.Length) : p.Name;					
					}

					LoadImage(cell.GenereImage, p, tippCell);
					if (tippCell) {						
						cell.SelectionStyle = UITableViewCellSelectionStyle.None;
						cell.GenereImage.AddGestureRecognizer(new UITapGestureRecognizer(() => { this.genreViewController.PerformSegue("HB1",cell.GenereImage); }));
						cell.GenereImage2.AddGestureRecognizer(new UITapGestureRecognizer(() => { this.genreViewController.PerformSegue("HB2",cell.GenereImage); }));
						var p2 = Getplaylist(NSIndexPath.FromItemSection(indexPath.Item+1,indexPath.Section));
						LoadImage(cell.GenereImage2, p2, tippCell);
					}
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (ex.ToString ());
			}
			return cell;
		}
		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var p = Getplaylist(indexPath);
			if (p != null) {
				bool tippCell = p.Name.Contains (keyTippDerWoche);
				return tippCell ? 180 : (indexPath.Row % 2 == 1 ? 125 : 90);
			} else
				return 88;
		}
		static void LoadImage(UIImageView imageView, UserPlaylist p, bool loadBookImage)
		{
			if (p != null) {
				if (loadBookImage)
					LoadImage (imageView, new NSUrl (p.Books.FirstOrDefault ().LargestCoverURL));
				else
					LoadImage (imageView, new NSUrl (p.SmallImageUrl));			
			}
		}
		static void LoadImage(UIImageView imageView, PlaylistBook book)
		{
			LoadImage (imageView, new NSUrl (book.LargestCoverURL));
		}
		static void LoadImage(UIImageView imageView, NSUrl imageURL)
		{
			if (imageView != null) {
				var nd = CurrentLRUCache.Current.CoverCache.GetItem(imageURL.AbsoluteString);
				if (nd != null) {
					imageView.Image = nd.Data.ToImage();
				}
				else {
					var gloalQueue = DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default);
					gloalQueue.DispatchAsync(() => 
						{
							NSError err = null;
							UIImage image = null;
							NSData imageData = NSData.FromUrl(imageURL, 0, out err);
							if (imageData != null) {
								CurrentLRUCache.Current.CoverCache.Insert(imageURL.AbsoluteString, imageData.ToByteArray());
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
		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return null; //GetBarSectionList()[section];
		}
		public override nint NumberOfSections (UITableView tableView)
		{
			return this.PlaylistLists != null && this.PlaylistLists.Any() ? GetBarNumberOfSection()
				: 0;
		}
		public override nint RowsInSection (UITableView tableView, nint section)
		{			
			if (section == 0) {
				var tag = this.genreViewController.NavigationController.TabBarItem.Tag;
				if (tag == 2)
					return 1;
			}
			return this.GetBarSublist(section).Count();
		}
		/*
		public override UIView GetViewForHeader (UITableView tableView, nint section)
		{
			var view = new UIView (new CGRect (0, 0, tableView.Frame.Width, tableView.SectionHeaderHeight));
			var label = new UILabel ();
			view.BackgroundColor = label.BackgroundColor = UIColor.FromRGB (25, 25, 25);
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


