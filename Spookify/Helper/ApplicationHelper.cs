using System;
using UIKit;
using CoreFoundation;

namespace Spookify
{
	public static class ApplicationHelper
	{
		public static void SwitchToTab(this UITabBarController tabBarController, int page = 1)
		{
			if (tabBarController == null)
				return;
			if (page < 0 || page > tabBarController.ViewControllers.Length)
				return;
			
			// Switch tab.

			UIView  fromView = tabBarController.SelectedViewController.View;
			UIView  toView = tabBarController.ViewControllers[page].View;

			UIView.Transition(fromView,toView,0.5,UIViewAnimationOptions.CurveEaseInOut,() => { tabBarController.SelectedIndex = page; });
		}
		public static void SwitchToPlayWhenNoSession(this UITabBarController tabBarController)
		{
			bool hasConnection = Reachability.RemoteHostStatus () != NetworkStatus.NotReachable;

			if (!hasConnection ||
				(!CurrentPlayer.Current.IsSessionValid &&
				 !CurrentPlayer.Current.CanRenewSession)) {
				// Switch tab.
				tabBarController.SwitchToTab(1);
			}
		}
		public static void AsyncLoadWhenSession()
		{
			var gloalQueue = DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default);
			gloalQueue.DispatchAsync (() => {
				if (CurrentPlayer.Current.IsSessionValid) {
					var dummy = CurrentAudiobooks.Current.User.Playlists;
				}
			});
		}
	}
}

