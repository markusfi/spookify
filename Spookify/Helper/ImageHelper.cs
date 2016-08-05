using System;
using UIKit;
using Foundation;
using System.Linq;
using CoreFoundation;
using CoreGraphics;
using System.Collections.Generic;

namespace Spookify
{
	public static class ImageHelper
	{
		public static UIImage GetTile(this UIImage image, CGRect rect)
		{
			if (image == null)
				return null;
			
			using(var drawImage = image.CGImage.WithImageInRect(rect)) {
				return UIImage.FromImage (drawImage);
			}
		}
		public static UIColor AverageColor(this UIImage image)
		{
			if (image == null)
				return UIColor.White;
			byte[] rgba = new byte[4];
			using (CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB ()) {
				using (var context = new CGBitmapContext (rgba, 1, 1, 8, 4, colorSpace, CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big)) {

					context.DrawImage (new CGRect (0, 0, 1, 1), image.CGImage);
				}
			}

			if(rgba[3] > 0) {
				var alpha = ((nfloat)rgba[3])/255.0f;
				var multiplier = alpha/255.0f;
				return UIColor.FromRGBA(((nfloat)rgba[0])*multiplier,
					((nfloat)rgba[1])*multiplier,
					((nfloat)rgba[2])*multiplier,
					alpha);
			}
			else {
				return UIColor.FromRGBA(((nfloat)rgba[0])/255.0f,
					((nfloat)rgba[1])/255.0f,
					((nfloat)rgba[2])/255.0f,
					((nfloat)rgba[3])/255.0f);
			}
		}
		public static float Luminance(this UIImage image)
		{
			if (image == null)
				return 0;
			var color = image.AverageColor ();
			var luminance = (0.299f*color.CGColor.Components[0] + 0.587f*color.CGColor.Components[1] + 0.114f*color.CGColor.Components[2]);
			return (float)luminance;
		}

		public static UIColor AverageColorDarkerAsRef(this UIImage image, UIColor refColor)
		{
			nfloat h, s, b, a;
			nfloat rh, rs, rb, ra;
			var c = image.AverageColor();
			c.GetHSBA(out h, out s, out b, out a);
			refColor.GetHSBA(out rh, out rs, out rb, out ra);
			return UIColor.FromHSBA(h, s, b < rb ? b : rb, a);
		}

		public static void LoadImage(this UIImageView imageView, UserPlaylist p, bool loadBookImage)
		{
			if (imageView == null)
				return;
			if (p != null) {
				if (loadBookImage) {
					if (p.Books != null && p.Books.FirstOrDefault() != null)
						LoadImage (imageView, 
							new NSUrl (p.Books.FirstOrDefault ().MediumCoverUrl));
				} else
					LoadImage (imageView, new NSUrl (p.MeidumImageUrl));			
			}
		}
		public static void LoadImage(this UIImageView imageView, PlaylistBook book)
		{
			if (imageView == null)
				return;
			if (book != null)
				LoadImage (imageView, new NSUrl (book.MediumCoverUrl));
		}
		public static void LoadImage(this UIImageView imageView, string imageURL, Action<NSData> dataSetter = null, bool useLRUCache = true)
		{
			LoadImage(imageView, new NSUrl (imageURL), dataSetter, useLRUCache);
		}

		static Dictionary<UIImageView,string> imageViewList = new Dictionary<UIImageView,string> (); 
		public static void LoadImage(this UIImageView imageView, NSUrl imageURL, Action<NSData> dataSetter = null, bool useLRUCache = true)
		{
			try {
				if (imageView == null)
					return;			
				var nd = useLRUCache ? CurrentLRUCache.Current.CoverCache.GetItem (imageURL.AbsoluteString) : null;
				if (nd != null) {
					string absoluteUrl;
					lock (imageViewList) {
						#if DEBUG
						Console.WriteLine ("ImageViewList Length:" + imageViewList.Count);
						#endif
						if (imageViewList.TryGetValue (imageView, out absoluteUrl)) {
							#if DEBUG
							Console.WriteLine ("Cache HIT + Update Dictionary because Request for this ImageView is running +++++++++++ +++++++++++++++++++++++++ +++++++++++++++++ +++++++++++");
							#endif
							imageViewList.Remove(imageView);
						}
					}
					imageView.Image = nd.Data.ToImage ();
					if (imageView.Image != null)
						imageView.Hidden = false;
				} else {
					lock (imageViewList) {
						#if DEBUG
						Console.WriteLine ("ImageViewList Length:" + imageViewList.Count);
						#endif
						string absoluteUrl;
						if (!imageViewList.TryGetValue (imageView, out absoluteUrl)) {
							imageViewList.Add (imageView, imageURL.AbsoluteString);
						} else {
							#if DEBUG
							Console.WriteLine ("Update Dictionary because Request for this ImageView is running +++++++++++ +++++++++++++++++++++++++ +++++++++++++++++ +++++++++++");
							#endif
							imageViewList [imageView] = imageURL.AbsoluteString;
						}
					}
					imageView.Image = null;
					imageView.BackgroundColor = UIColor.FromRGBA (100, 143, 0, 30);
					var gloalQueue = DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default);
					gloalQueue.DispatchAsync (() => {
						try {
							NSError err = null;
							UIImage image = null;
							NSData imageData = NSData.FromUrl (imageURL, 0, out err);
							if (imageData == null) {
								string absoluteUrl;
								lock (imageViewList) {
									if (imageViewList.TryGetValue (imageView, out absoluteUrl)) {
										if (absoluteUrl == imageURL.AbsoluteString)
											imageViewList.Remove (imageView);
									}
								}
								return;
							}
							if (dataSetter != null)
								dataSetter (imageData);

							if (useLRUCache)
								CurrentLRUCache.Current.CoverCache.Insert (imageURL.AbsoluteString, imageData.ToByteArray ());
							image = UIImage.LoadFromData (imageData);

							DispatchQueue.MainQueue.DispatchAsync (() => {
								try {
									lock (imageViewList) {
										string absoluteUrl;
										if (imageViewList.TryGetValue (imageView, out absoluteUrl)) {
											if (absoluteUrl != imageURL.AbsoluteString) {
												#if DEBUG
												Console.WriteLine ("Droped loaded Image because race codition occured ################## ################## ################## ##################");
												#endif
												return;
											} else {
												imageViewList.Remove (imageView);
												#if DEBUG
												Console.WriteLine ("Remove ImageView, length:" + imageViewList.Count);
												#endif
											}
										}
									}
									imageView.BackgroundColor = UIColor.Clear;
									imageView.Image = image;
									if (imageView.Image != null)
										imageView.Hidden = false;
									else {
										System.Diagnostics.Debug.WriteLine ("Could not load image with error: {0}", err);
										return;
									}
								} catch (Exception ex) {
									System.Diagnostics.Debug.WriteLine ("Got Exception in LoadImage (inner): "+ex.ToString());
								}
							});
						} catch (Exception ex) {
							System.Diagnostics.Debug.WriteLine ("Got Exception in LoadImage (middle): "+ex.ToString());
						}
					});
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine ("Got Exception in LoadImage (outer): "+ex.ToString());
			}
		}

		public static void SetUIImage(this UIImageView imageView, string imageURL, NSData coverData, Action<NSData> coverDataSetter)
		{
			if (imageView == null)
				return;
			if (coverData != null) {
				imageView.Image = UIImage.LoadFromData (coverData);
				if (imageView.Image != null)
					imageView.Hidden = false;
			} else if (imageURL != null) {
				imageView.LoadImage (imageURL, coverDataSetter);
			}	
		}
	}
}

