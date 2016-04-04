using System;
using System.Linq;
using UIKit;
using SpotifySDK;
using CoreFoundation;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Spookify
{
	public partial class HoerbuchViewController : UIViewController
	{
		public PlaylistBook Book { get; set; }
		public AudioBook NewBook { get; set; }

		public HoerbuchViewController (IntPtr handle) : base (handle)
		{
		}
		public HoerbuchViewController () : base ("HoerbuchViewController", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.BuchSelektiertButton.Layer.CornerRadius = 15;
			this.BuchSelektiertButton.Layer.BackgroundColor = UIColor.FromRGB (100, 143, 0).CGColor;

			this.SucheAmazonButton.Layer.CornerRadius = 10;
			this.SucheAmazonButton.Layer.BorderWidth = 0.5f;
			this.SucheAmazonButton.Layer.BorderColor = UIColor.LightGray.CGColor;
			this.SucheAmazonButton.BackgroundColor = UIColor.FromRGBA (0, 0, 0, 0);

			this.SucheBuechertreffButton.Layer.CornerRadius = 10;
			this.SucheBuechertreffButton.Layer.BorderWidth = 0.5f;
			this.SucheBuechertreffButton.Layer.BorderColor = UIColor.LightGray.CGColor;
			this.SucheBuechertreffButton.BackgroundColor = UIColor.FromRGBA (0, 0, 0, 0);

			this.MehrVomAutorButton.Layer.CornerRadius = 10;
			this.MehrVomAutorButton.Layer.BorderWidth = 0.5f;
			this.MehrVomAutorButton.Layer.BorderColor = UIColor.LightGray.CGColor;
			this.MehrVomAutorButton.BackgroundColor = UIColor.FromRGBA (0, 0, 0, 0);

			SetBookTitleAndAuthor ();
			LoadBookTracksAsync ();
			LoadCoverAsync ();
			LoadDescriptionAsync ();

			ScrollView.TranslatesAutoresizingMaskIntoConstraints = false;
			ContainerView.TranslatesAutoresizingMaskIntoConstraints = false;
		}
		void SetBookTitleAndAuthor()
		{
			if (Book != null) {
				if (Book.Album != null)
					this.AlbumLabel.Text = MakeSearchKey (Book.Album.Name);

				if (Book.Artists != null) {
					var firstAttributes = new UIStringAttributes {
						Font = UIFont.FromName ("HelveticaNeue-Light", 15f)
					};
					var secondAttributes = new UIStringAttributes {
						Font = UIFont.FromName ("HelveticaNeue", 15f)
					};
					var prettyString = new NSMutableAttributedString ("Von " + Book.Artists.FirstOrDefault ());
					prettyString.SetAttributes (firstAttributes.Dictionary, new NSRange (0, 4));
					prettyString.SetAttributes (secondAttributes.Dictionary, new NSRange (4, prettyString.Length - 4));

					this.AuthorLabel.AttributedText = prettyString;
				}
			}
		}
		void SetBookLength()
		{
			if (NewBook != null && NewBook.Tracks != null) {
				var gesamtSeitAnfang = NewBook.Tracks.Sum (t => t.Duration);
				var tsSeitAnfang = TimeSpan.FromSeconds (gesamtSeitAnfang);
				string txt;
				if (Math.Truncate(tsSeitAnfang.TotalHours) > 1.0)
					txt = string.Format ("{0} Stunden {1:00} Minuten", Math.Truncate (tsSeitAnfang.TotalHours), tsSeitAnfang.Minutes);
				else if (Math.Truncate(tsSeitAnfang.TotalHours) > 0.0)
					txt = string.Format ("{0} Stunde {1:00} Minuten", Math.Truncate (tsSeitAnfang.TotalHours), tsSeitAnfang.Minutes);
				else 
					txt = string.Format ("{0} Minuten {1:00} Sekunden",  Math.Truncate(tsSeitAnfang.TotalMinutes), tsSeitAnfang.Seconds);

				DispatchQueue.MainQueue.DispatchAsync (() => {
					this.LengthLabel.Text = txt;
				});
			}
		}
		void LoadDescriptionAsync()
		{
			if (Book != null && Book.Album != null) {
				var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
				gloalQueue.DispatchAsync (() => {
					NSError err = null;
					var name = MakeSearchKey (Book.Album.Name);
					var author = MakeSearchKey (Book.Artists.FirstOrDefault ());
					var data = NSData.FromUrl (new NSUrl (string.Format ("{0}?q=intitle:{1}+inauthor:{2}&key={3}",
						           ConfigGoogle.kSearchTitleURL,
						           System.Web.HttpUtility.UrlEncode (name),
						           System.Web.HttpUtility.UrlEncode (author),
						           ConfigGoogle.kDeveloperKey)), 0, out err);
				
					if (err == null) {
						var json = (string)NSString.FromData (data, NSStringEncoding.UTF8);
						if (json != null) {
							string description = string.Empty;
							var volume = Newtonsoft.Json.Linq.JObject.Parse (json);
							if (volume != null && volume.HasValues) {
								var items = volume ["items"];
								if (items != null) {
									JToken book = null;
									if (items.Count () > 1) {
										book = items.FirstOrDefault (x => string.Compare ((string)x ["volumeInfo"] ["title"], name, true) == 0);
									} else if (items.Count () > 0) {
										book = items [0];
									}
									if (book != null) {
										JToken volumeInfo = book ["volumeInfo"];
										if (volumeInfo != null) {
											description = (string)volumeInfo ["description"];
											var tmp = (string)volumeInfo ["publishedDate"];
											if (!string.IsNullOrWhiteSpace (tmp)) {
												DateTime date;
												if (DateTime.TryParseExact (tmp, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
													description += "\n\nErscheinungsdatum: " + date.ToShortDateString ();
												}
											}
											tmp = (string)volumeInfo ["publisher"];
											if (!string.IsNullOrWhiteSpace (tmp)) {
												description += "\n\nVerlag: " + tmp;
											}
										}
									}
								}
							}
							DispatchQueue.MainQueue.DispatchAsync (() => {
								this.DescriptionTextView.Text = description;
								this.DescriptionTextView.TextColor = this.AlbumLabel.TextColor;
								this.DescriptionTextView.Font = UIFont.FromName(@"HelveticaNeue-Light",14.0f);

								var frame = this.DescriptionTextView.Frame;
								var height = this.DescriptionTextView.SizeThatFits (this.DescriptionTextView.Frame.Size).Height;
								this.DescriptionTextView.Frame = new CGRect (frame.Location, new CGSize (frame.Width, height));
								this.DescriptionTextViewHeightLayoutConstraint.Constant = height;

								// Find the right size for the content view - that is the view inside the scrollview that holds all controls that can grow or shrink
								var size = new CGSize (this.ContainerView.Frame.Width, ContainerView.Subviews.Max (x => x.Frame.Bottom) - this.ContainerView.Frame.Y);
								// now adjust the content view by - do this by changing its height constraint and bottom alignment to its parent scroll view
								var delta = size.Height - this.ScrollView.Frame.Height;
								this.ContainerBottomLayoutConstraint.Constant = -delta;
								this.ContainerHeightLayoutConstraint.Constant = delta;
								// now recalculate the constraints
								this.View.NeedsUpdateConstraints ();
								this.View.UpdateConstraintsIfNeeded();
								// now recalculate the layout
								this.View.SetNeedsLayout();
								this.View.LayoutIfNeeded();
								// now we have the right size for the scrollview content after all children have been moved around
								size = new CGSize (this.ContainerView.Frame.Width, ContainerView.Subviews.Max (x => x.Frame.Bottom) - this.ContainerView.Frame.Y);
								this.ScrollView.ContentSize = size;
							});
						}	
					}
				});
			}
		}
		void LoadCoverAsync()
		{
			if (Book != null) {
				var imageView = this.AlbumImage;
				if (imageView != null) {
					imageView.Image = null;
					var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
					gloalQueue.DispatchAsync (() => {
						NSError err = null;
						UIImage image = null;
						NSData imageData = NSData.FromUrl (new NSUrl (Book.LargestCoverURL), 0, out err);
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
		string MakeSearchKey(string name)
		{
			if (string.IsNullOrWhiteSpace (name))
				return name;
			// remove everything in ( xxx ) when not starting with it...
			int pos = name.IndexOf ('(');
			if (pos != -1 && pos > 1) {
				int endPos = name.IndexOf (')', pos);
				if (endPos != -1) {
					if (name.Length > endPos)
						name = name.Substring (0, pos) + name.Substring (endPos);
					else
						name = name.Substring (0, pos);
				}
			}
			name = name.Replace("Ungekürzte Fassung","");
			name = name.Replace("Ungekürzt","");
			name = name.Replace("Gekürzte Fassung","");
			name = name.Replace("Gekürzt","");
			foreach (var source in name.ToCharArray().Where(c => c == '(' || c == ')' || c == '[' || c == ']' || char.IsWhiteSpace(c))) {
				name = name.Replace(source,' ');
			}
			while (name.Contains("  ")) {
				name = name.Replace("  "," ");
			}
			if (name.EndsWith(" "))
				name = name.Substring(0,name.Length-1);
			return name;
		}
		string MakeSearchKeyUrlEncode(string name)
		{
			return System.Web.HttpUtility.UrlEncode(MakeSearchKey(name));
		}
		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			try {
				if (segue.DestinationViewController is RechercheViewController) {
					var rechercheViewController = segue.DestinationViewController as RechercheViewController;
					var name = MakeSearchKeyUrlEncode(Book.Album.Name);
					var author = MakeSearchKeyUrlEncode(Book.Artists.FirstOrDefault());
					if (sender == SucheAmazonButton)
						rechercheViewController.Url = string.Format(@"https://www.google.de/search?q='{0}'{1}'{2}'&as_sitesearch=amazon.de&num=1",name,System.Web.HttpUtility.UrlEncode(" "),author);
					else if (sender == SucheBuechertreffButton)
						rechercheViewController.Url = string.Format(@"https://www.google.de/search?q='{0}'{1}'{2}'&as_sitesearch=buechertreff.de&num=1",name,System.Web.HttpUtility.UrlEncode(" "),author);
					else
						rechercheViewController.Url = string.Format(@"https://www.google.de/search?q='{0}'{1}'{2}'",name,System.Web.HttpUtility.UrlEncode(" "),author);
				}
				if (segue.DestinationViewController is AutorViewController) {
					var autorViewController = segue.DestinationViewController as AutorViewController;
					// suche nach anderen Büchern des Autors
					autorViewController.NewBook = this.NewBook;
				}
			}
			catch(System.Exception ex) {
				System.Diagnostics.Debug.WriteLine ("Bug: " + ex.ToString ());
			}	
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		void LoadBookTracksAsync()
		{
			if (Book != null && Book.Uri != null) {
				var url = new NSUrl (Book.Uri);
				SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
				var p = SPTRequest.SPTRequestHandlerProtocol;
				NSError errorOut;

				var nsUrlRequest = SPTAlbum.CreateRequestForAlbum (url, auth.Session.AccessToken, "DE", out errorOut);
				SPTRequestHandlerProtocol_Extensions.Callback (p, nsUrlRequest, (er, resp, jsonData) => {
					if (er != null) {
						return;
					}
					NSError nsError;
					var album = SPTAlbum.AlbumFromData (jsonData, resp, out nsError);
					if (album != null) {
						int kapitelNummer = 1;
						NewBook = new AudioBook () {
							Uri = album.Uri.AbsoluteString,
							Album = new AudioBookAlbum () { Name = album.Name }, 
							Tracks = album.FirstTrackPage.Items
							.Cast<SPTPartialTrack> ()
							.Where (pt => pt.IsPlayable)
							.Select (pt => new AudioBookTrack () { 
								Url = pt.GetUri ().AbsoluteString, 
								Name = pt.Name, 
								Duration = pt.Duration, 
								Index = kapitelNummer++
							})
							.ToList (),
							Authors = album.Artists.Cast<SPTPartialArtist> ().Select (a => new Author () {
								Name = a.Name,
								URI = a.Uri.AbsoluteString
							}).ToList (),
							LargestCoverURL = album.LargestCover.ImageURL.AbsoluteString,
							SmallestCoverURL = album.SmallestCover.ImageURL.AbsoluteString
						};
						SetBookLength ();
						LoadNextPageAsync (NewBook, album.FirstTrackPage, auth, p);
					}
				});
			}
		}

		partial void OnSucheAmazon (UIKit.UIButton sender)
		{
			this.PerformSegue("RechercheSegue",sender);
		}

		partial void OnSucheBuechertreff (UIKit.UIButton sender)
		{
			this.PerformSegue("RechercheSegue",sender);
		}

		partial void OnBuchSelektiert (UIKit.UIButton sender)
		{
			if (NewBook != null) {

				var thisBook = CurrentState.Current.Audiobooks.FirstOrDefault(a => a.Album.Name == NewBook.Album.Name);
				if (thisBook != null) {
					CurrentState.Current.Audiobooks.Remove(thisBook);
					NewBook.CurrentPosition = thisBook.CurrentPosition;
				}								

				CurrentState.Current.Audiobooks.Add(NewBook);
				CurrentState.Current.CurrentAudioBook = NewBook;
				CurrentState.Current.StoreCurrentState();
				CurrentPlayer.Current.PlayCurrentAudioBook();

				var tabBarController = this.TabBarController;

				UIView  fromView = tabBarController.SelectedViewController.View;
				UIView  toView = tabBarController.ViewControllers[1].View;

				UIView.Transition(fromView,toView,0.5,UIViewAnimationOptions.CurveEaseInOut,() => { tabBarController.SelectedIndex = 1; });

				var fromNavController = tabBarController.SelectedViewController as UINavigationController;
				if (fromNavController != null && 
					fromNavController.ViewControllers != null &&
					fromNavController.ViewControllers.Length > 0 && 
					fromNavController.ViewControllers[0].GetType() == typeof(ZuletztViewController))
					this.NavigationController.PopToRootViewController (false);
			} 
		}

		void LoadNextPageAsync(AudioBook newbook, SPTListPage page, SPTAuth auth, SPTRequestHandlerProtocol p)
		{
			NSError errorOut, nsError;
			if (page != null && page.HasNextPage) {
				var nsUrlRequest = page.CreateRequestForNextPageWithAccessToken(auth.Session.AccessToken, out errorOut);
			    SPTRequestHandlerProtocol_Extensions.Callback(p, nsUrlRequest, (er1,resp1,jsonData1) => {
					var nextpage = SPTListPage.ListPageFromData(jsonData1, resp1, true, "", out nsError);
					if (nextpage != null) {
						int kapitelNummer = newbook.Tracks.Max(t => t.Index) + 1;
						newbook.Tracks.AddRange(nextpage.Items
							.Cast<SPTPartialTrack>()
							.Where(pt => pt.IsPlayable)
							.Select(pt => new AudioBookTrack() { 
								Url = pt.GetUri().AbsoluteString, 
								Name = pt.Name, 
								Duration = pt.Duration, 
								Index = kapitelNummer++  } )
							.ToList());
						SetBookLength();
						LoadNextPageAsync(newbook, nextpage, auth, p);
					}
				});
			}
		}
	}
}


