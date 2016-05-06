using System;
using UIKit;
using Foundation;
using System.Linq;
using CoreFoundation;

namespace Spookify
{
	public static class ImageHelper
	{
		public static void LoadImage(this UIImageView imageView, UserPlaylist p, bool loadBookImage)
		{
			if (imageView == null)
				return;
			if (p != null) {
				if (loadBookImage) {
					if (p.Books != null && p.Books.FirstOrDefault() != null)
						LoadImage (imageView, 
							new NSUrl (p.Books.FirstOrDefault ().LargestCoverURL));
				} else
					LoadImage (imageView, new NSUrl (p.SmallImageUrl));			
			}
		}
		public static void LoadImage(this UIImageView imageView, PlaylistBook book)
		{
			if (imageView == null)
				return;
			if (book != null)
				LoadImage (imageView, new NSUrl (book.LargestCoverURL));
		}
		public static void LoadImage(this UIImageView imageView, string imageURL, Action<NSData> dataSetter = null, bool useLRUCache = true)
		{
			LoadImage(imageView, new NSUrl (imageURL), dataSetter, useLRUCache);
		}
		public static void LoadImage(this UIImageView imageView, NSUrl imageURL, Action<NSData> dataSetter = null, bool useLRUCache = true)
		{
			if (imageView == null)
				return;			
			var nd = useLRUCache ? CurrentLRUCache.Current.CoverCache.GetItem(imageURL.AbsoluteString) : null;
			if (nd != null) {
				imageView.Image = nd.Data.ToImage();
				if (imageView.Image != null)
					imageView.Hidden = false;
			}
			else {
				imageView.Image = null;
				imageView.BackgroundColor = UIColor.FromRGBA (100, 143, 0, 30);
				var gloalQueue = DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default);
				gloalQueue.DispatchAsync(() => 
					{
						NSError err = null;
						UIImage image = null;
						NSData imageData = NSData.FromUrl(imageURL, 0, out err);
						if (imageData != null) {
							if (dataSetter != null)
								dataSetter(imageData);

							if (useLRUCache)
								CurrentLRUCache.Current.CoverCache.Insert(imageURL.AbsoluteString, imageData.ToByteArray());
							image = UIImage.LoadFromData(imageData);

							DispatchQueue.MainQueue.DispatchAsync(() => 
								{
									imageView.BackgroundColor = UIColor.Clear;
									imageView.Image = image;
									if (imageView.Image != null)
										imageView.Hidden = false;
									else {
										System.Diagnostics.Debug.WriteLine("Could not load image with error: {0}",err);
										return;
									}
								});
						}
					});
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

