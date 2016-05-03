using System;
using System.Collections.Generic;
using SpotifySDK;
using Foundation;
using System.Linq;
using System.Collections;
using System.Xml.Serialization;

namespace Spookify
{
	public class PlaylistChangedEventArgs : EventArgs
	{
		public PlaylistChangedEventArgs(string name) {
			this.Name = name; 
		}
		/// <summary>
		/// Name der geänderten Playlist
		/// </summary>
		public string Name { get; set; }
	}

	public delegate void PlaylistChangedEventHandler (object sender, PlaylistChangedEventArgs e);

	[Serializable]
	public class CurrentAudiobooks: CurrentBase<CurrentAudiobooks>
	{
		[NonSerialized]
		PlaylistChangedEventHandler _changed;
		public event PlaylistChangedEventHandler Changed
		{
			add {
				_changed += value;
			}
			remove {
				_changed -= value;
			}
		}
		public void OnChanged(object sender, PlaylistChangedEventArgs args)
		{
			if (_changed != null)
				_changed (sender, args);
		}
		public override string Filename()  { return "Audiobooks"; } 

		PlaylistOwner _user;
		public PlaylistOwner User { 
			get { 
				if (_user == null) {
					_user = new PlaylistOwner () { Name = "hoerbuecher" };
					_user.Changed += OnChanged;
					LastUpdate = DateTime.UtcNow;
				}
				return _user;
			}
			set {
				if (_user != value) {
					_user.Changed -= OnChanged;
					_user = value; 
					_user.Changed += OnChanged;
				}
			}
		}

		bool refreshRunning = false;
		public void Refresh() {
			if (!refreshRunning &&
				LastUpdate.AddDays (2) < DateTime.UtcNow) {
				refreshRunning = true;
				var newData = new CurrentAudiobooks ();
				newData.Changed += (object sender, PlaylistChangedEventArgs e) => {
					if (newData.IsComplete) {
						this.User = newData.User;
						newData._changed = null;
						refreshRunning = false;
					}
				};
			}
		}
		public bool IsComplete {
			get {
				return User.Playlists.All(p => p.IsComplete);
			}
		}
		public DateTime LastUpdate { get; set; }
	}

	[Serializable]
	public class PlaylistOwner
	{
		[NonSerialized]
		PlaylistChangedEventHandler _changed;
		public event PlaylistChangedEventHandler Changed
		{
			add {
				_changed += value;
			}
			remove {
				_changed -= value;
			}
		}
		public void OnChanged(object sender, PlaylistChangedEventArgs args)
		{
			if (_changed != null)
				_changed (sender, args);
		}

		public string Name { get; set; }

		List<UserPlaylist> _playlists;
		public List<UserPlaylist> Playlists {
			get { 
				if (_playlists == null) {
					_playlists = new List<UserPlaylist> ();
					GetUserPlaylistAsync (this, (newUserPlaylist, isComplete) => { 
						// will be called multiple times.
						if (newUserPlaylist != null) {
							lock(_playlists) {
								var p = _playlists.FirstOrDefault(p1 => p1.Name == newUserPlaylist.Name);
								if (p != null) {
									p.Books.AddRange(newUserPlaylist.Books);
									if (isComplete) 
										p.TrackCount = (uint)p.Books.Count;
									p.IsComplete = isComplete;
									Console.WriteLine("Playlist "+newUserPlaylist.Name + "  TrackCount: "+p.TrackCount+ "  Books.Count: "+p.Books.Count());
								} else {
									_playlists.Add(newUserPlaylist);
									if (isComplete)
										newUserPlaylist.TrackCount = (uint)newUserPlaylist.Books.Count;
									newUserPlaylist.IsComplete = isComplete;
									Console.WriteLine("Playlist "+newUserPlaylist.Name + "  TrackCount: "+newUserPlaylist.TrackCount+ "  Books.Count: "+newUserPlaylist.Books.Count());
								}
							}
						}
						OnChanged(this, new PlaylistChangedEventArgs(newUserPlaylist.Name));
					});
				}
				return _playlists;
			}
		}

		private static void GetUserPlaylistAsync(PlaylistOwner playlistOwner, Action<UserPlaylist, bool> completionHandler = null) 
		{
			if (CurrentPlayer.Current.IsSessionValid) {
				SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
				if (auth.Session != null) {
					SPTPlaylistList.PlaylistsForUser (playlistOwner.Name, auth.Session.AccessToken, (nsError, obj) => {
						if (nsError != null) 
							return;
						AddPlaylistListsPage(auth, obj as SPTPlaylistList, completionHandler);
					});
				}
			}
		}
		private static void AddPlaylistListsPage(SPTAuth auth, SPTListPage playlistlists, Action<UserPlaylist, bool> completionHandler)
		{
			if (playlistlists == null) 
				return;
			var items = playlistlists.Items as NSObject[];
			if (items == null)
				return;
			var playListArray = items.Select (a => a as SPTPartialPlaylist).ToArray ();
			if (playListArray != null && playListArray.Any()) {
				GetPlaylist(playListArray, completionHandler);
			}
			if (playlistlists.HasNextPage) {
				// next page of this users Playlists
				NSError errorOut;
				var nsUrlRequest = playlistlists.CreateRequestForNextPageWithAccessToken (auth.Session.AccessToken, out errorOut);
				if (errorOut != null)
					return;
				if (nsUrlRequest == null)
					return;
				SPTRequestHandlerProtocol_Extensions.Callback (SPTRequest.SPTRequestHandlerProtocol, nsUrlRequest, (er, resp1, jsonData1) => {
					if (er != null) {
						return;
					}
					var nextpage = SPTListPage.ListPageFromData (jsonData1, resp1, true, "", out errorOut);
					if (errorOut != null) {
						return;
					}
					AddPlaylistListsPage(auth, nextpage, completionHandler);
				});
			}
		}

		static readonly int maxConcorrent = 1;
		private static void GetPlaylist(IEnumerable<SPTPartialPlaylist> playlistTotalArray, Action<UserPlaylist, bool> completionHandler)
		{
			IEnumerable<SPTPartialPlaylist> playListArray = playlistTotalArray.Count () > maxConcorrent ? playlistTotalArray.Take (maxConcorrent) : playlistTotalArray;
			IEnumerable<SPTPartialPlaylist> playlistRestArray = playlistTotalArray.Count () > maxConcorrent ? playlistTotalArray.Skip(maxConcorrent) : null;

			foreach (var playlist in playListArray) {
				Console.WriteLine ("Start PlaylistBook.CreateFromFullAsync: "+playlist.Name);
				PlaylistBook.CreateFromFullAsync(playlist, (IEnumerable<PlaylistBook> playlistBooks, bool isComplete) => {
					// will be called multiple times.
					if (playlistBooks != null) {
						if (completionHandler != null) {
							completionHandler(new UserPlaylist() {
								Name = playlist.Name,
								TrackCount = (uint) playlist.TrackCount,
								LargeImageUrl = playlist.LargestImage.ImageURL.AbsoluteString,
								SmallImageUrl = playlist.SmallestImage.ImageURL.AbsoluteString,
								Books = playlistBooks.ToList(),
							}, isComplete);
						}
					}
				}, () => {
					// next book.
					if (playlistRestArray != null) {						
						GetPlaylist(playlistRestArray, completionHandler);
						Console.WriteLine ("Continue PlaylistBook.CreateFromFullAsync: "+playlist.Name + "  Left:" +playlistRestArray.Count());
					}
					else 
						Console.WriteLine ("Stop PlaylistBook.CreateFromFullAsync: "+playlist.Name);
				});
			}
		}

	}

	[Serializable]
	public class UserPlaylist 
	{
		public string Name { get; set; }
		public uint TrackCount { get; set; }
		public string LargeImageUrl { get; set; }
		public string SmallImageUrl { get; set; }
		public List<PlaylistBook> Books { get; set; }

		public bool IsComplete { get; set; } = false;
	}
}

