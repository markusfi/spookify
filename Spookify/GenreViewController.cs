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
			this.HoerbuchTableView.Delegate = new HoerbuecherDelegate();
			var ds = new HoerbuecherDataSource ();
			this.HoerbuchTableView.DataSource = ds;
			ds.Changed += DataSourceChanged;
			var dg = new HoerbuecherDelegate ();
			this.HoerbuchTableView.Delegate = dg;
			dg.Selected += RowSelected;

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
				CurrentPlayer.Current.RenewSession ();
			}

			if (CurrentPlayer.Current.IsSessionValid) {
				var ds = this.HoerbuchTableView.DataSource as HoerbuecherDataSource;
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
			var destinationViewController = segue.DestinationViewController as HoerbuchListeViewController;
			var p = (this.HoerbuchTableView.DataSource as HoerbuecherDataSource).Getplaylist (indexPath.Row);
			destinationViewController.PlayList = p;
		}
	}

	public class HoerbuecherDelegate : UITableViewDelegate 
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
	}

	public class HoerbuecherDataSource : UITableViewDataSource
	{		
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


		public int GetplaylistCount {		
			get {
				return this.PlayListArray != null ? this.PlayListArray.Length : 0;
			}
		}
		public SPTPartialPlaylist Getplaylist(int index) {			
			if (this.PlayListArray != null && index >= 0 && index < this.PlayListArray.Length) {
				return this.PlayListArray [index];
			}
			throw new IndexOutOfRangeException ();
		}
		#region implemented abstract members of UITableViewDataSource
		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			GenreTableViewCell cell = tableView.DequeueReusableCell ("GenereCell") as GenreTableViewCell;
			if (cell != null) {
				try {
					var p = Getplaylist (indexPath.Row);
					cell.GenereLabel.Text = p.Name.StartsWith("Hörbücher ") ? p.Name.Substring("Hörbücher ".Length) : p.Name;					
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
				} catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine (ex.ToString ());
				}
			}
			return cell;
		}
		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return "";
		}
		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}
		public override nint RowsInSection (UITableView tableView, nint section)
		{
			if (section == 0)
				return this.GetplaylistCount;
			else
				return 0;
		}

		#endregion
	}
}


