using System;
using System.Linq;

using UIKit;
using SpotifySDK;
using Foundation;
using CoreFoundation;
using CoreImage;
using CoreGraphics;
using System.Collections.Generic;

namespace Simple
{
	public partial class PlayerViewController : UIViewController
	{
		SPTAudioStreamingController player;

		public PlayerViewController(IntPtr handle) : base(handle)
		{
		}

		public PlayerViewController () : base ("PlayerViewController", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.titleLabel.Text = @"Nothing Playing";
			this.trackLabel.Text = @"";
			this.artistLabel.Text = @"";
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		partial void OnPressPlay (UIButton sender)
		{
			this.player.SetIsPlaying(!this.player.IsPlaying, (error) => { });
		}

		partial void OnPressLogout (UIButton sender)
		{
			SPTAuth auth = SPTAuth.DefaultInstance;
			if (this.player != null) {
				this.player.Logout(arg0 => { 
					auth.Session = null;
					this.NavigationController.PopViewController(true);
				});
			} else {
				this.NavigationController.PopViewController(true);
			}
		}

		partial void OnPressBack (UIButton sender)
		{
			this.player.SkipPrevious((error) => {});
		}

		partial void OnPressForward (UIButton sender)
		{
			this.player.SkipNext((error) => {});
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			HandleNewSession ();
		}
		void HandleNewSession()
		{
			SPTAuth auth = SPTAuth.DefaultInstance;

			if (this.player == null) {
				this.player = new SPTAudioStreamingController(auth.ClientID);
				this.player.PlaybackDelegate = new MySPTAudioStreamingDelegate(this);
				// this.player.DiskCache = new SPTDiskCache(1024 * 1024 * 64).;
			}
			this.UpdateUI();
			this.player.LoginWithSession(auth.Session, error => {

				if (error != null) {
					System.Diagnostics.Debug.WriteLine(@"*** Enabling playback got error: {0}", error);
					return;
				}

				NSError errorOut;
				var p = SPTRequest.SPTRequestHandlerProtocol;


				var savedTracksReq = SPTYourMusic.CreateRequestForCurrentUsersSavedTracksWithAccessToken(auth.Session.AccessToken,  out errorOut);
				SPTRequestHandlerProtocol_Extensions.Callback(p, savedTracksReq, (er,resp,jsonData) => {
					if (er != null) {
						return;
					}
					NSError nsError;
					var tracks = SPTTrack.TracksFromData(jsonData, resp, out nsError);
					if (tracks != null) {
						this.player.PlayURIs(tracks, 0, (playURIError) => { });
					} 
					else 
					{
						var jsonObject = NSJsonSerialization.Deserialize (jsonData, 0, out nsError);
						if (jsonObject != null) {
							var jsonObject1 = (NSDictionary)jsonObject;
							if (jsonObject1 != null) {								
								var items = (NSMutableArray)jsonObject1.ValueForKey(new NSString("items"));
								if (items != null) {
									var list = new List<NSObject>();
									for (nuint i=0;i<items.Count;i++) {
										var item = items.GetItem<NSDictionary>(i);
										if (item != null) {
											var track = item.ValueForKey(new NSString("track"));
											if (track != null) {
												var uri = (NSString)track.ValueForKey(new NSString("uri"));
												if (uri != null) {
													list.Add(new NSUrl(uri));
												}
											}
										}
									}
									this.player.PlayURIs(list.ToArray(), 0, (playURIError) => { });
								}
							}
						}
					}
				});


				NSUrlRequest playlistReq = SPTPlaylistSnapshot.CreateRequestForPlaylistWithURI(new NSUrl("spotify:user:cariboutheband:playlist:4Dg0J0ICj9kKTGDyFu0Cv4"),auth.Session.AccessToken, out errorOut);
				SPTRequestHandlerProtocol_Extensions.Callback(p, playlistReq, (er,resp,dat) => {
					if (er != null) {
						return;
					}
					// System.Diagnostics.Debug.WriteLine(dat);

					SPTPlaylistSnapshot playlistSnapshot = SPTPlaylistSnapshot.PlaylistSnapshotFromData(dat, resp, out errorOut);

					// this.player.PlayURIs(playlistSnapshot.FirstTrackPage.Items, 0, (playURIError) => { });
				});
			});
		}

		public class MySPTAudioStreamingDelegate : SPTAudioStreamingPlaybackDelegate {
			PlayerViewController viewController = null;
			public MySPTAudioStreamingDelegate(PlayerViewController vc) {
				this.viewController = vc;
			}
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, bool isPlaying)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, double offset)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, NSDictionary trackMetadata)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreamingDidFailToPlayTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreamingDidLosePermissionForPlayback (SPTAudioStreamingController audioStreaming)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreamingDidSkipToNextTrack (SPTAudioStreamingController audioStreaming)
			{
				viewController.UpdateUI();
			}
			public override void AudioStreamingDidSkipToPreviousTrack (SPTAudioStreamingController audioStreaming)
			{
				viewController.UpdateUI();
			}
		}
		UIImage ApplyBlurOnImage(UIImage imageToBlur, float radius)
		{
			CIImage originalImage = CIImage.FromCGImage (imageToBlur.CGImage);

			CIFilter filter = CIFilter.FromName ("CIGaussianBlur");
			var keys = new NSString[] { CIFilterInputKey.Image, new NSString("inputRadius")};
			var values = new NSObject[] { originalImage, NSNumber.FromFloat (radius)};
			filter.SetValuesForKeysWithDictionary (NSDictionary.FromObjectsAndKeys (values,keys));
			CIImage outputImage = filter.OutputImage;
			CIContext context = CIContext.FromOptions (null);
			using (CGImage outImage = context.CreateCGImage (outputImage, outputImage.Extent)) {
				var ret = UIImage.FromImage (outImage);
				return ret;
			}
		}
		int updateCounter = 0;
		void UpdateUI() {
			updateCounter++;
			System.Diagnostics.Debug.WriteLine("UpdateUI called {0}",updateCounter);

			SPTAuth auth = SPTAuth.DefaultInstance;

			if (this.player.CurrentTrackURI == null) {
				this.coverView.Image = null;
				// this.coverView2.Image = null;
				return;
			}

			this.PlayButton.SetTitle (this.player.IsPlaying ? "||" : ">", UIControlState.Normal);
//			this.spinner.StartAnimating();

			SPTTrack.TrackWithURI(this.player.CurrentTrackURI, auth.Session, (error, obj) => {

				var track = obj as SPTTrack;
				var myCounter = updateCounter;
				this.titleLabel.Text = track.Name;
				this.trackLabel.Text = track.Album.Name;

				SPTPartialArtist artist = track.Artists[0] as SPTPartialArtist;
				this.artistLabel.Text = artist.Name;

				NSUrl imageURL = track.Album.LargestCover.ImageURL;
					if (imageURL == null) {
						System.Diagnostics.Debug.WriteLine("Album {0} doesn't have any images!", track.Album);
						this.coverView.Image = null;
						// this.coverView2.Image = null;
						return;
					}

				if (myCounter<updateCounter)
					return;
				
				var gloalQueue = DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default);
				gloalQueue.DispatchAsync(() => 
					{
						NSError err = null;
						UIImage image = null;
						NSData imageData = NSData.FromUrl(imageURL, 0, out err);
						if (imageData != null) 
							image = UIImage.LoadFromData(imageData);

						if (myCounter<updateCounter)
							return;
						
						DispatchQueue.MainQueue.DispatchAsync(() => 
							{
								if (myCounter<updateCounter)
									return;
								
								this.coverView.Image = image;
								if (image == null) {
									System.Diagnostics.Debug.WriteLine("Could not load cover image with error: {0}",error);
									return;
								}
							});
						/*
						var blurred = this.ApplyBlurOnImage(image, 10.0f);
						DispatchQueue.MainQueue.DispatchAsync(() => 
							{ 
								if (myCounter<updateCounter)
									return;
								
								this.coverView2.Image = blurred;
							});
							*/
					});
			});
		}
	}
}


