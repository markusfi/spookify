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
					_authPlayer = new SPTAuth ();
					_authPlayer.ClientID = ConfigSpotify.kClientId;
					_authPlayer.RequestedScopes = new[]{ Constants.SPTAuthUserLibraryReadScope, Constants.SPTAuthStreamingScope };
					_authPlayer.RedirectURL = new NSUrl(ConfigSpotify.kCallbackURL);

					if (!string.IsNullOrEmpty(ConfigSpotify.kTokenSwapServiceURL))
						_authPlayer.TokenSwapURL = new NSUrl(ConfigSpotify.kTokenSwapServiceURL);

					if (!string.IsNullOrEmpty(ConfigSpotify.kTokenRefreshServiceURL))
						_authPlayer.TokenRefreshURL = new NSUrl(ConfigSpotify.kTokenRefreshServiceURL);
					_authPlayer.SessionUserDefaultsKey = ConfigSpotify.kSessionPlayerUserDefaultsKey;

				}
				return _authPlayer;
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
				return this.IsSessionValid && this.Player != null && this.Player.LoggedIn; 
			}
		}

		SPTAudioStreamingController _player;
		public SPTAudioStreamingController Player
		{
			get 
			{ 
				if (this.IsSessionValid) {
					if (_player == null) {
						SPTAuth auth = this.AuthPlayer;
						var ca = new SPTCoreAudioController ();
						_player = new SPTAudioStreamingController (auth.ClientID, ca);
						_player.PlaybackDelegate = _mySPTAudioStreamingDelegate;
						_player.DiskCache = new SPTDiskCache(1024 * 1024 * 64);
					} 
					if (!_player.LoggedIn) {
						SPTAuth auth = this.AuthPlayer;
						_player.LoginWithSession (auth.Session, error => {

							// login failed.
							Console.WriteLine(error);
						});
					}
				} else {
					_player = null;
					_authPlayer = null;
				}
				return _player;
			}
		}

		int CurrentStartTrack;

		public void PlayCurrentAudioBook()
		{			
			if (this.IsPlayerCreated) {
				if (CurrentState.Current.CurrentAudioBook != null) {
					if (CurrentState.Current.CurrentAudioBook.CurrentPosition != null) {

						CurrentStartTrack = CurrentState.Current.CurrentAudioBook.CurrentPosition.TrackIndex;
						SPTPlayOptions options = new SPTPlayOptions ();
						options.TrackIndex = 0;
						options.StartTime = CurrentState.Current.CurrentAudioBook.CurrentPosition.PlaybackPosition;

						this.Player.PlayURIs (CurrentState.Current.CurrentAudioBook.Tracks.Skip(CurrentStartTrack).Take(50).Select (t => t.NSUrl).ToArray (), 
							options,
							(playURIError1) => {
							});

					} else {
						CurrentStartTrack = 0;
						this.Player.PlayURIs (CurrentState.Current.CurrentAudioBook.Tracks.Take(50).Select (t => t.NSUrl).ToArray (), 
							0, 
							(playURIError) => {
							});
					}
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

		public void OpenLoginPage(PlayerViewController vc)
		{
			if (this.AuthPlayer.Session == null)
				this._authPlayer = null;
			this.authViewController = SPTAuthViewController.AuthenticationViewControllerWithAuth(this.AuthPlayer);
			this.authViewController.HideSignup = false;
			this.authViewController.Delegate = new MySPTAuthViewDelegate (vc);
			this.authViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
			this.authViewController.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;

			vc.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
			vc.DefinesPresentationContext = true;

			vc.PresentViewController (this.authViewController, false, null);
		}

		public class MySPTAuthViewDelegate : SPTAuthViewDelegate {
			PlayerViewController viewController = null;
			public MySPTAuthViewDelegate(PlayerViewController vc)  {
				this.viewController = vc;
			}

			#region implemented abstract members of SPTAuthViewDelegate

			public override void AuthenticationViewControllerDidCancelLogin (SPTAuthViewController authenticationViewController)
			{
				//this.viewController.AlbumLabel.Text = "Login abgebrochen";
			}

			public override void AuthenticationViewControllerFail (SPTAuthViewController authenticationViewController, NSError error)
			{
				//this.viewController.AlbumLabel.Text = "Login Fehler";
			}

			public override void AuthenticationViewControllerLogin (SPTAuthViewController authenticationViewController, SPTSession session)
			{
				if (this.viewController != null) {
					this.viewController.UpdateUI ();

					if (CurrentState.Current.Audiobooks.Count == 0) {
						viewController.SwitchTab (2); // liste der Bücher in Playlists
					} else if (CurrentState.Current.CurrentAudioBook == null) {
						viewController.SwitchTab (0); // Liste der gewählten Bücher
					} else {
						viewController.SwitchTab (1);
						viewController.PlayCurrentAudioBook ();
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
	}
}

