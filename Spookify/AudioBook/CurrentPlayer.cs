﻿using System;
using SpotifySDK;
using Foundation;
using System.Linq;
using UIKit;

namespace Spookify
{
	public class CurrentPlayer
	{
		public CurrentPlayer ()
		{
		}

		static CurrentPlayer _currentPlayer;
		public static CurrentPlayer Current
		{ 
			get {
				if (_currentPlayer == null) {
					_currentPlayer = new CurrentPlayer ();
				}
				return _currentPlayer;			
			} 
		}

		MySPTAudioStreamingDelegate _mySPTAudioStreamingDelegate;
		public void CreateSPTAudioStreamingDelegate(PlayerViewController vc)
		{
			_mySPTAudioStreamingDelegate = new MySPTAudioStreamingDelegate (vc);
			if (this.Player != null)
				this.Player.PlaybackDelegate = _mySPTAudioStreamingDelegate;
		}
		 
		SPTAuthViewController authViewController = null;
		SPTAuth _authPlayer;
		public SPTAuth AuthPlayer { 
			get { 
				if (_authPlayer == null) 
				{
					CreateNewSPTAuth ();
				}
				return _authPlayer;
			} 
		}
		public void CreateNewSPTAuth()
		{
			_authPlayer = new SPTAuth ();
			_authPlayer.ClientID = ConfigSpotify.kClientId;
			_authPlayer.RequestedScopes = new[]{ Constants.SPTAuthUserLibraryReadScope, Constants.SPTAuthStreamingScope };
			_authPlayer.RedirectURL = new NSUrl(ConfigSpotify.kCallbackURL);

			var c = CurrentSettings.Current;
			if (!string.IsNullOrEmpty(ConfigSpotify.kTokenSwapServiceURL) && c.kTokenSwapService)
				_authPlayer.TokenSwapURL = new NSUrl(ConfigSpotify.kTokenSwapServiceURL);

			if (!string.IsNullOrEmpty(ConfigSpotify.kTokenRefreshServiceURL) && c.kTokenRefreshService)
				_authPlayer.TokenRefreshURL = new NSUrl(ConfigSpotify.kTokenRefreshServiceURL);

			_authPlayer.SessionUserDefaultsKey = ConfigSpotify.kSessionPlayerUserDefaultsKey;
		}

		public void RenewSession(Action completionAction = null)
		{
			if (TriggerWaitingForSessionRenew) {
				// do nothing right now, waiting for session renew...
			} else {
				if (HasConnection) {
					SPTAuth auth = this.AuthPlayer;
					TriggerSessionRenew = DateTime.Now;
					auth.RenewSession (auth.Session, (error, session) => {
						TriggerSessionRenew = null;
						if (error == null) {
							auth.Session = session;
							if (completionAction != null)
								completionAction();
						}
						else  {
							new NSObject ().BeginInvokeOnMainThread (() => {
								new UIAlertView ("Fehler", error.LocalizedDescription + error.Description, null, "OK").Show ();
							});
						}			
					});
				}
			}
		}

		public bool HasConnection {
			get {
				bool hasConnection = Reachability.RemoteHostStatus () != NetworkStatus.NotReachable;
				return hasConnection; 
			}
		}
		public bool IsSessionValid {
			get {
				SPTAuth auth = this.AuthPlayer;
				return auth.Session != null &&
					auth.Session.IsValid &&
					_authPlayer.RequestedScopes.Any (sc => (NSString)sc == Constants.SPTAuthStreamingScope);
			}
		}

		public bool IsPlayerCreated { 
			get { 
				var p = this.RawPlayer;
				return this.IsSessionValid && p != null && p.LoggedIn; 
			}
		}

		public bool CanRenewSession {
			get {
				SPTAuth auth = this.AuthPlayer;
				return (auth.Session != null && !auth.Session.IsValid && auth.HasTokenRefreshService);
			}
		}
		public bool NeedToRenewSession {
			get {
				return !IsSessionValid && CanRenewSession;
			}
		}
		DateTime? TriggerSessionRenew = null;
		public bool TriggerWaitingForSessionRenew {
			get {
				return TriggerSessionRenew.HasValue &&
					(TriggerSessionRenew.Value.AddSeconds (30) > DateTime.Now) &&
					!IsSessionValid;
			}
		}

		DateTime? TriggerPlayerLogin = null;
		public bool TriggerWaitingForLogin {
			get {
				return TriggerPlayerLogin.HasValue &&
					(TriggerPlayerLogin.Value.AddSeconds (30) > DateTime.Now) &&
					!IsPlayerCreated;				
			}
		}

		DateTime? TriggerPlayUri = null;
		public bool TriggerWaitingForPlayUri {
			get {
				return TriggerPlayUri.HasValue &&
					(TriggerPlayUri.Value.AddSeconds (1) > DateTime.Now);
			}
		}
		public void SetTriggerWaitForPlayUri() {
			if (!TriggerWaitingForPlayUri)
				TriggerPlayUri=DateTime.Now;
		}


		SPTAudioStreamingController _player;
		public SPTAudioStreamingController RawPlayer { get { return this._player; } }
		public SPTAudioStreamingController Player
		{
			get 
			{ 
				if (this.NeedToRenewSession) {
					this.RenewSession ();
				}
					
				if (this.IsSessionValid) {
					if (_player == null) {
						SPTAuth auth = this.AuthPlayer;
						var ca = new SPTCoreAudioController ();
						_player = new SPTAudioStreamingController (auth.ClientID, ca);
						_player.PlaybackDelegate = _mySPTAudioStreamingDelegate;
						_player.DiskCache = new SPTDiskCache(1024 * 1024 * 64);
					} 
					if (_player.LoggedIn) {
						TriggerPlayerLogin = null;
					} 
					else 
					{
						if (TriggerWaitingForLogin)
						{
							// wait, do nothing now...
						}
						else {
							if (HasConnection) {
								SPTAuth auth = this.AuthPlayer;
								_player.LoginWithSession (auth.Session, error => {

									// login failed.
									Console.WriteLine (error);
									TriggerPlayerLogin = null;
								});
								TriggerPlayerLogin = DateTime.Now;
							}
						}
					}
				} else {
					TriggerPlayerLogin = null;
					_player = null;
					_authPlayer = null;
				}
				return _player;
			}
		}

		public int CurrentStartTrack;

		public void PlayCurrentAudioBook()
		{			
			if (this.IsPlayerCreated && HasConnection) {
				var ab = CurrentState.Current.CurrentAudioBook;
				if (ab != null && 
					ab.Tracks != null) {
					if (ab.CurrentPosition != null) {
						CurrentStartTrack = (ab.CurrentPosition.TrackIndex - (ab.CurrentPosition.TrackIndex % 25));
						if (CurrentStartTrack >= 0 &&
						    CurrentStartTrack < ab.Tracks.Count) {
							SPTPlayOptions options = new SPTPlayOptions () {
								TrackIndex = ab.CurrentPosition.TrackIndex - CurrentStartTrack, 
								StartTime = ab.CurrentPosition.PlaybackPosition
							};
							SetTriggerWaitForPlayUri ();
							CurrentState.Current.StoreCurrentState ();
							this.Player.PlayURIs (ab.Tracks.Skip (CurrentStartTrack).Take (50).Select (t => t.NSUrl).ToArray (), 
								options,
								(playURIError1) => {
								});
							Console.WriteLine ("Player.PlayURI(" + CurrentStartTrack + ",options(TrackIndex=" + options.TrackIndex + ",StartTime=" + options.StartTime + ")");
							return;
						}
					}
					CurrentStartTrack = 0;
					if (ab.CurrentPosition != null)
						ab.CurrentPosition.TrackIndex = 0;
					SetTriggerWaitForPlayUri ();
					CurrentState.Current.StoreCurrentState ();
					this.Player.PlayURIs (ab.Tracks.Take(50).Select (t => t.NSUrl).ToArray (), 
						0, 
						(playURIError) => {
						});
					Console.WriteLine ("Player.PlayURI(" + CurrentStartTrack + ")");
				}
			}
		}

		public bool SavePosition(bool store = true)
		{
			if (this.IsPlayerCreated) {
				var p = this.Player;
				if (p != null && p.CurrentTrackURI != null && p.LoggedIn) {
					int playerTrack = CurrentStartTrack + p.CurrentTrackIndex;
					var ab = CurrentState.Current.CurrentAudioBook;
					if (ab != null) {										
						ab.CurrentPosition = new AudioBookBookmark () {
							PlaybackPosition = p.CurrentPlaybackPosition,
							TrackIndex = playerTrack
						};
						if (store) {
							CurrentState.Current.StoreCurrentState ();
						}
					}
					return true;
				}
			}
			return false;
		}

		public void OpenLoginPage(UIViewController vc)
		{
			if (HasConnection) {
				if (this.AuthPlayer.Session == null)
					this._authPlayer = null;
				this.authViewController = SPTAuthViewController.AuthenticationViewControllerWithAuth (this.AuthPlayer);
				this.authViewController.HideSignup = true;
				this.authViewController.Delegate = new MySPTAuthViewDelegate (vc);
				this.authViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
				this.authViewController.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;

				vc.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
				vc.DefinesPresentationContext = true;

				vc.PresentViewController (this.authViewController, false, null);
			}
		}

		public class MySPTAuthViewDelegate : SPTAuthViewDelegate {
			UIViewController viewController = null;
			public MySPTAuthViewDelegate(UIViewController vc)  {
				this.viewController = vc;
			}

			#region implemented abstract members of SPTAuthViewDelegate

			public override void AuthenticationViewControllerDidCancelLogin (SPTAuthViewController authenticationViewController)
			{
				BeginInvokeOnMainThread (() => {
					new UIAlertView("Fehler","Login abgebrochen",null,"OK").Show();
				});
			}

			public override void AuthenticationViewControllerFail (SPTAuthViewController authenticationViewController, NSError error)
			{
				BeginInvokeOnMainThread (() => {
					new UIAlertView("Fehler",error.LocalizedDescription + error.Description,null,"OK").Show();
				});
			}

			public override void AuthenticationViewControllerLogin (SPTAuthViewController authenticationViewController, SPTSession session)
			{
				if (this.viewController != null) {
					if (this.viewController is PlayerViewController) {
						var playerViewController = this.viewController as PlayerViewController;
						playerViewController.DisplayAlbum ();

						if (CurrentState.Current.Audiobooks.Count == 0) {
							playerViewController.SwitchTab (2); // liste der Bücher in Playlists
						} else if (CurrentState.Current.CurrentAudioBook == null) {
							playerViewController.SwitchTab (0); // Liste der gewählten Bücher
						} else {
							playerViewController.SwitchTab (1);
							playerViewController.PlayCurrentAudioBook ();
						}
					} else {
						new UIAlertView("Spookify","Login erfolgreich",null,"OK").Show();
					}
				}
			}

			#endregion
		}

		public class MySPTAudioStreamingDelegate : SPTAudioStreamingPlaybackDelegate {
			PlayerViewController viewController = null;
		
			public MySPTAudioStreamingDelegate(PlayerViewController vc) {
				this.viewController = vc;
			}
			void LogState(string txt)
			{
				#if DEBUG
				Console.WriteLine(txt);
				var p = this.viewController.Player;
				if (p != null) {
					Console.WriteLine ("Player.CurrentTrackIndex="+p.CurrentTrackIndex+", Player.CurrentPlaybackPosition"+p.CurrentPlaybackPosition+") CurrentStartTrack="+CurrentPlayer.Current.CurrentStartTrack);
				}
				#endif
			}
				
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, bool isPlaying)
			{
				LogState ("AudioStreaming (isPlaying = " + isPlaying + ")");
				viewController.DisplayAlbum();
			}
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, double offset)
			{
				LogState ("AudioStreaming (double offset = " + offset + ")");
				viewController.DisplayAlbum();
			}
			public override void AudioStreaming (SPTAudioStreamingController audioStreaming, NSDictionary trackMetadata)
			{
				LogState ("AudioStreaming (trackMetadata)");
				viewController.DisplayAlbum();
			}
			public override void AudioStreamingDidFailToPlayTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri)
			{
				LogState ("AudioStreamingDidFailToPlayTrack (NSUrl trackUri)");
				viewController.DisplayAlbum();
			}
			public override void AudioStreamingDidLosePermissionForPlayback (SPTAudioStreamingController audioStreaming)
			{
				LogState ("AudioStreamingDidLosePermissionForPlayback ()");
				viewController.DisplayAlbum();
			}
			public override void AudioStreamingDidSkipToNextTrack (SPTAudioStreamingController audioStreaming)
			{
				LogState ("AudioStreamingDidSkipToNextTrack ()");
				viewController.DisplayAlbum();
			}
			public override void AudioStreamingDidSkipToPreviousTrack (SPTAudioStreamingController audioStreaming)
			{
				LogState ("AudioStreamingDidSkipToPreviousTrack ()");
				viewController.DisplayAlbum();
			}
			public override void AudioStreamingDidStartPlayingTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri)
			{
				LogState ("AudioStreamingDidStartPlayingTrack (trackUri="+trackUri.AbsoluteString+")");
				viewController.DisplayAlbum();
			}
			public override void AudioStreamingDidStopPlayingTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri)
			{
				LogState ("AudioStreamingDidStopPlayingTrack (trackUri="+trackUri.AbsoluteString+")");
				viewController.DisplayAlbum();
			}
		}
	}
}

