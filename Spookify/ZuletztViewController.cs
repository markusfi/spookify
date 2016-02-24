﻿using System;

using Foundation;
using UIKit;
using System.Linq;
using CoreFoundation;

namespace Spookify
{
	public partial class ZuletztViewController : UIViewController
	{
		public ZuletztViewController (IntPtr handle) : base (handle)
		{
		}
		public ZuletztViewController () : base ("ZuletztViewController", null)
		{

		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
			this.HoerbuchListeTableView.Delegate = new ZuletztListeDelegate() { zuletztViewController = this };
			var ds = new ZuletztDataSource () { zuletztViewController = this };
			this.HoerbuchListeTableView.DataSource = ds;
			ds.Changed += (object sender, EventArgs e) => {
				this.HoerbuchListeTableView.ReloadData();
			};
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear (animated);
			this.HoerbuchListeTableView.ReloadData ();
		}

		public class ZuletztListeDelegate : UITableViewDelegate
		{
			public ZuletztViewController zuletztViewController { get; set; }

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				var selectedBook = CurrentState.Current.Audiobooks [indexPath.Row];
				if (selectedBook != null) {
					CurrentState.Current.CurrentAudioBook = selectedBook;
					CurrentState.Current.StoreCurrentState ();

					var tabBarController = this.zuletztViewController.TabBarController;

					UIView fromView = tabBarController.SelectedViewController.View;
					UIView toView = tabBarController.ViewControllers [1].View;

					UIView.Transition (fromView, toView, 0.5, UIViewAnimationOptions.CurveEaseInOut, () => {
						tabBarController.SelectedIndex = 1;
					});
				}
			}
			public override string TitleForDeleteConfirmation (UITableView tableView, NSIndexPath indexPath)
			{
				return "Buch entfernen";
			}
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return UITableViewCellEditingStyle.Delete;
			}
			
		}
		public class ZuletztDataSource : UITableViewDataSource
		{
			public event EventHandler Changed;
			private void OnChanged() {
				DispatchQueue.MainQueue.DispatchAsync(() => {
					if (Changed != null) 
						Changed (this, EventArgs.Empty);
				});
			}
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if (editingStyle == UITableViewCellEditingStyle.Delete) {
					if (CurrentState.Current.Audiobooks.Count > indexPath.Row) {
						var book = CurrentState.Current.Audiobooks [indexPath.Row];
						if (book != null) {
							if (book.Equals (CurrentState.Current.CurrentAudioBook)) {
								if (CurrentState.Current.Audiobooks.Count > 1)
									CurrentState.Current.CurrentAudioBook = CurrentState.Current.Audiobooks [Math.Max (0, indexPath.Row - 1)];
								else
									CurrentState.Current.CurrentAudioBook = null;
							}
							CurrentState.Current.Audiobooks.RemoveAt (indexPath.Row);
							CurrentState.Current.StoreCurrentState ();
							tableView.ReloadData ();
						}
					}
				}
			}
			public ZuletztViewController zuletztViewController { get; set; }

			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell ("HoerbuchCell") as HoerbuchTableViewCell;
				var audiobooks = CurrentState.Current.Audiobooks;
				if (audiobooks != null) {
					if (indexPath.Row < audiobooks.Count) {
						var currentBook = audiobooks[indexPath.Row];
						cell.AlbumLabel.Text = currentBook.Album.Name;
						cell.AuthorLabel.Text = currentBook.Artists.FirstOrDefault ();

						var gesamtSeitAnfang = currentBook.Tracks.Sum (t => t.Duration);
						var tsSeitAnfang = TimeSpan.FromSeconds (gesamtSeitAnfang);

						if (Math.Truncate(tsSeitAnfang.TotalHours) > 1.0)
							cell.ZeitLabel.Text = string.Format ("{0} Stunden {1:00} Minuten", Math.Truncate (tsSeitAnfang.TotalHours), tsSeitAnfang.Minutes);
						else if (Math.Truncate(tsSeitAnfang.TotalHours) > 0.0)
							cell.ZeitLabel.Text = string.Format ("{0} Stunde {1:00} Minuten", Math.Truncate (tsSeitAnfang.TotalHours), tsSeitAnfang.Minutes);
						else 
							cell.ZeitLabel.Text = string.Format ("{0} Minuten {1:00} Sekunden",  Math.Truncate(tsSeitAnfang.TotalMinutes), tsSeitAnfang.Seconds);
						
						cell.AuthorLabel.Text = currentBook.Artists.FirstOrDefault ();

						var imageView = cell.AlbumImage;
						if (imageView != null && currentBook.SmallestCoverURL != null) {
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

			public override nint NumberOfSections (UITableView tableView)
			{
				return 1;
			}
			public override string TitleForHeader (UITableView tableView, nint section)
			{
				return "";
			}
			public override nint RowsInSection (UITableView tableView, nint section)
			{
				if (section == 0)
					return (nint) CurrentState.Current.Audiobooks.Count;
				else
					return 0;
			}
		}
	}
}