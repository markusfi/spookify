using System;
using Foundation;
using UIKit;

namespace Spookify
{
	public static class UIViewControllerExtension
	{
		public static void SendAudiobook(this UIViewController uIViewController, AudioBook currentAudioBook, UIImage image)
		{
			var txt = new NSString( 
			    "Eine Audiobookempfehlung auf Bookify:\r\n" +
				$"{currentAudioBook.ToAlbumName()}\r\n" +
				$"von {currentAudioBook.ToAuthorName()}\r\n" +
				$"{currentAudioBook.TimeGesamt()}\r\n" +
				$"bookify://{currentAudioBook.Uri}\r\n\r\n" +
				"Bookify installieren:\r\n" +
				$"{ConfigSpookify.ItunesUri}");
			
			var subject = new NSString("Bookify Audiobookempfehlung");
			var item = NSObject.FromObject(txt);
			var activityItems = new NSObject[] { item, image, subject };
			UIActivity[] applicationActivities = null;

			var activityController = new UIActivityViewController(activityItems, applicationActivities)
			{
				ExcludedActivityTypes = new NSString[] {
					UIActivityType.PostToWeibo,
					UIActivityType.AddToReadingList,
					UIActivityType.PostToFlickr,
					UIActivityType.AssignToContact,
					UIActivityType.PostToVimeo,
					UIActivityType.OpenInIBooks,
					UIActivityType.Print,
					UIActivityType.SaveToCameraRoll
				}
			};
			activityController.SetValueForKey(subject, new NSString("Subject"));

			uIViewController.PresentViewController(activityController, true, () => {
				activityController.Dispose();
			});
		}
	}

}

