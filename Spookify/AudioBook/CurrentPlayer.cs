using System;
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
		public bool SessionDisabled { get; set; } = false;
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

		MySPTAudioStreamingPlaybackDelegate _mySPTAudioStreamingDelegate;

		public void CreateSPTAudioStreamingDelegate(PlayerViewController vc)
		{
			_mySPTAudioStreamingDelegate = new MySPTAudioStreamingPlaybackDelegate (vc);
			if (this.Player != null)
				this.Player.PlaybackDelegate = _mySPTAudioStreamingDelegate;
		}
		public void RemoveSPTAudioStreamingDelegate()
		{			
			if (this.Player != null)
				this.Player.PlaybackDelegate = null;
			if (_mySPTAudioStreamingDelegate != null)
				_mySPTAudioStreamingDelegate.Dispose ();
			_mySPTAudioStreamingDelegate = null;
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
		public void ClearAuthPlayer()
		{
			if (_authPlayer == null) {
				if (_authPlayer.Session != null) {
					_authPlayer.SessionUserDefaultsKey = null;
					_authPlayer.Session = null;
					_authPlayer.Session.Dispose ();
				}
				_authPlayer.Session = null;
				_authPlayer.Dispose ();
				_authPlayer = null;
			}
		}
		public void CreateNewSPTAuth()
		{
			_authPlayer = new SPTAuth ();
			_authPlayer.ClientID = ConfigSpotify.kClientId;
			_authPlayer.RequestedScopes = new[]{ ConstantsScope.SPTAuthUserLibraryReadScope, ConstantsScope.SPTAuthStreamingScope };
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
							refreshErrorOccured = false;
							auth.Session = session;
							if (completionAction != null)
								completionAction();
						}
						else  {
							refreshErrorOccured = true;
							new NSObject ().BeginInvokeOnMainThread (() => {
								var av = new UIAlertView ("Fehler", error.LocalizedDescription, null, "OK");
								av.Show();
								av.Dismissed += (object sender, UIButtonEventArgs e) => {
									refreshErrorOccured = false;
									av.Dispose();
								};
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
				return 
					!SessionDisabled &&
					auth.Session != null &&
					auth.Session.IsValid &&
					auth.Session.ExpirationDate.NSDateToDateTime() > DateTime.Now && 
					_authPlayer.RequestedScopes.Any (sc => sc != null || (NSString)sc == ConstantsScope.SPTAuthStreamingScope);
			}
		}

		public bool IsPlayerCreated { 
			get { 
				var p = this.RawPlayer;
				return this.IsSessionValid && p != null && p.LoggedIn; 
			}
		}
		bool refreshErrorOccured = false;
		public bool CanRenewSession {
			get {
				if (SessionDisabled)
					return false;
				SPTAuth auth = this.AuthPlayer; // !string.IsNullOrEmpty(auth.Session.EncryptedRefreshToken) && 
				return (auth.Session != null && !auth.Session.IsValid && auth.HasTokenRefreshService && !refreshErrorOccured);
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
						_player.Delegate = new MySPTAudioStreamingDelegate ();
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
									if (error != null) {
										// login failed.
										Console.WriteLine ("_player.LoginWithSession failed: " + error);
										TriggerPlayerLogin = null;
										var dummy = this.Player;
									}
								});
								TriggerPlayerLogin = DateTime.Now;
							}
						}
					}
				} else {
					ResetPlayer ();
				}
				return _player;
			}
		}
		public void ResetPlayer() {

			if (this._player != null) {
				if (this._player.PlaybackDelegate != null) {
					this._player.PlaybackDelegate = null;
				}
				this._player.Dispose ();
				this._player = null;
			}
			if (_authPlayer != null)
				_authPlayer.Dispose ();
			_authPlayer = null;
			TriggerPlayerLogin = null;
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
							CurrentState.Current.StoreCurrent ();
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
					CurrentState.Current.StoreCurrent ();
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
						ab.Started = ab.Tracks != null && 
							CurrentState.Current.CurrentTrack != null &&
							p.CurrentPlaybackPosition > 2.0;
						if (ab.Started) {
							ab.Finished = ab.Tracks == null || CurrentState.Current.CurrentTrack == null ||
							((playerTrack == ab.Tracks.Count - 1 &&
							(CurrentState.Current.CurrentTrack.Duration - p.CurrentPlaybackPosition) < 5.0));
							if (ab.Finished)
								ab.Started = false;
						}
						if (store) {
							CurrentState.Current.StoreCurrent ();
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
	}
}

