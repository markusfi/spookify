using System;

using Foundation;
using UIKit;
using System.Linq;
using CoreFoundation;
using CoreGraphics;

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

			this.NavigationController.NavigationBar.BarStyle = UIBarStyle.BlackTranslucent;
			this.NavigationController.NavigationBar.BarTintColor = ConfigSpookify.BackgroundColor;
			this.NavigationController.NavigationBar.Translucent = false;
			this.NavigationController.NavigationBar.TintColor = ConfigSpookify.BartTintColor;

			// Perform any additional setup after loading the view, typically from a nib.
			this.HoerbuchListeTableView.Delegate = new ZuletztListeDelegate() { zuletztViewController = this };
			var ds = new ZuletztDataSource () { zuletztViewController = this };
			this.HoerbuchListeTableView.DataSource = ds;
			ds.Changed += (object sender, EventArgs e) => {
				this.HoerbuchListeTableView.ReloadData();
			};
			this.HoerbuchListeTableView.TableFooterView = new UIView (CGRect.Empty);
		}
		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);
			var cell = sender as HoerbuchTableViewCell;
			if (cell != null) {
				var destinationViewController = segue.DestinationViewController as HoerbuchViewController;
				try {
					destinationViewController.Book = (PlaylistBook)cell.CurrentAudioBook;
				} catch {

				}
			}
		}
		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear (animated);
			this.HoerbuchListeTableView.ReloadData ();
			if (miniPlayerView == null)
				miniPlayerView = new MiniPlayerView (this.View, this);
		}
		MiniPlayerView miniPlayerView = null;

		public class ZuletztListeDelegate : UITableViewDelegate
		{
			public ZuletztViewController zuletztViewController { get; set; }

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				/*
				var selectedBook = CurrentState.Current.Audiobooks [indexPath.Row];
				if (selectedBook != null) {
					CurrentState.Current.CurrentAudioBook = selectedBook;
					CurrentState.Current.StoreCurrentState ();
					CurrentPlayer.Current.PlayCurrentAudioBook ();

					var tabBarController = this.zuletztViewController.TabBarController;

					UIView fromView = tabBarController.SelectedViewController.View;
					UIView toView = tabBarController.ViewControllers [1].View;

					UIView.Transition (fromView, toView, 0.5, UIViewAnimationOptions.CurveEaseInOut, () => {
						tabBarController.SelectedIndex = 1;
					});
				}
				*/
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
							CurrentState.Current.StoreCurrent ();
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
						cell.CurrentAudioBook = currentBook;
							
						cell.AlbumLabel.Text = currentBook.Album.Name;
						cell.AuthorLabel.Text = currentBook.Artists.FirstOrDefault ();

						var gesamtSeitAnfang = currentBook.Tracks.Sum (t => t.Duration);
						var tsSeitAnfang = TimeSpan.FromSeconds (gesamtSeitAnfang);
						if (!currentBook.Started && !currentBook.Finished) {
							cell.progressBar.Hidden = true;
							cell.DauerLabel.Hidden = true;
							cell.ZeitLabel.Hidden = false;
							cell.DauerLabel.Text = "";
							cell.ZeitLabel.Text = tsSeitAnfang.ToLongTimeText ();
						} else if(currentBook.Finished) {
							cell.ZeitLabel.Text = "Beendet";
							cell.progressBar.Hidden = true;
							cell.DauerLabel.Hidden = true;
							cell.ZeitLabel.Hidden = false;
						} else if (currentBook.CurrentPosition != null) {
							var position = currentBook.Tracks.Take (currentBook.CurrentPosition.TrackIndex).Sum (t => t.Duration) + currentBook.CurrentPosition.PlaybackPosition;
							cell.progressBar.Progress = (float)(position / gesamtSeitAnfang);
							cell.progressBar.Hidden = false;
							cell.DauerLabel.Hidden = false;
							cell.DauerLabel.Text = "noch " + TimeSpan.FromSeconds (gesamtSeitAnfang - position).ToTimeText ();
							cell.ZeitLabel.Text = "";
							cell.ZeitLabel.Hidden = true;
						} else {
							cell.progressBar.Hidden = true;
							cell.DauerLabel.Hidden = true;
							cell.ZeitLabel.Hidden = false;
							cell.DauerLabel.Text = "";
							cell.ZeitLabel.Text = tsSeitAnfang.ToLongTimeText ();
						}

						cell.AuthorLabel.Text = currentBook.Artists.FirstOrDefault ();

						currentBook.SetSmallImage (cell.AlbumImage);
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
