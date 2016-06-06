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
		new public static CurrentAudiobooks Current
		{
			get {
				var current = CurrentBase<CurrentAudiobooks>.Current;
				current.TriggerRefresh ();
				if (!current.HasPlaylists && CurrentAudiobooks.refreshRunning != null)
					return CurrentAudiobooks.refreshRunning;
				else
					return current;
			}
		}

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
		[NonSerialized]
		static CurrentAudiobooks _refreshRunning;
		static CurrentAudiobooks refreshRunning { 
			get {  
				return _refreshRunning;
			}
			set {
				_refreshRunning = value;
			}
		}
		public void TriggerRefresh() {
			if (refreshRunning == null &&
			    (!HasPlaylists ||
				(DateTime.UtcNow - LastUpdate).TotalDays > 1)) 
			{
				if (!CurrentPlayer.Current.IsSessionValid)
					return;
				bool needToLoadAgain = false;
				lock (typeof(CurrentAudiobooks)) {
					if (refreshRunning == null) {
						refreshRunning = new CurrentAudiobooks ();
						PlaylistChangedEventHandler handler = (object sender, PlaylistChangedEventArgs e) => {
							if (refreshRunning.IsComplete) {
								if (refreshRunning.User.Playlists.Count > 10 &&
									(this._user == null || 
									 (refreshRunning.User.Playlists.Count-5) > this.User.Playlists.Count)) 
								{
									this.User = refreshRunning.User;
									this.LastUpdate = DateTime.UtcNow;
									this.StoreCurrent ();
									refreshRunning._changed = null;
									refreshRunning = null;
									this.OnChanged(this,new PlaylistChangedEventArgs(""));
								}
								else {
									refreshRunning._changed = null;
									refreshRunning = null;
									needToLoadAgain = true;
								}
							}
						};
						refreshRunning.Changed += handler;
						// trigger reload here.
						var dummy = refreshRunning.User.Playlists;
					}
				}
				if (needToLoadAgain)
					TriggerRefresh ();
			}
		}
		public bool IsComplete {
			get {
				return HasPlaylists && User.Playlists.All(p => p.IsComplete);
			}
		}
		public bool HasPlaylists {
			get {
				return User.Playlists.Any();
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
									// p.Books.AddRange(newUserPlaylist.Books);
									AddWithoutDups(_playlists, p, newUserPlaylist);
									if (isComplete) 
										p.TrackCount = (uint)p.Books.Count;
									p.IsComplete = isComplete;
									Console.WriteLine("Playlist "+newUserPlaylist.Name + "  TrackCount: "+p.TrackCount+ "  Books.Count: "+p.Books.Count());
								} else {
									p = new UserPlaylist() { Books = new List<PlaylistBook>(), Name = newUserPlaylist.Name };
									AddWithoutDups(_playlists, p, newUserPlaylist);
									newUserPlaylist.Books = p.Books;
									_playlists.Add(newUserPlaylist);
									if (isComplete)
										newUserPlaylist.TrackCount = (uint)newUserPlaylist.Books.Count;
									newUserPlaylist.IsComplete = isComplete;
									Console.WriteLine("Playlist "+newUserPlaylist.Name + "  TrackCount: "+newUserPlaylist.TrackCount+ "  Books.Count: "+newUserPlaylist.Books.Count());
								}
							}
						}
						OnChanged(this, new PlaylistChangedEventArgs(newUserPlaylist?.Name));
						ordered = false;
					});
				}
				if (_playlists != null) {
					if (!ordered) {
						_playlists.Sort (ConfigListen.Comparison);
						ordered = true;
					}
				}
				return _playlists;
			}
		}
		bool ordered = false;

		void AddWithoutDups(List<UserPlaylist> master, UserPlaylist playlist, UserPlaylist newSublist)
		{
			string[] noDups = { "Bestseller", "Geheimtipps" };
			bool doNotAllowDups = noDups.Any(s => playlist.Name.Contains(s));
			var sortedList = doNotAllowDups 
				? master.SelectMany (p => p?.Books ?? new List<PlaylistBook> ()).OrderBy (b => b?.Uri).ToList ()
				: playlist.Books.OrderBy(b => b.Uri).ToList();
			foreach (var book in newSublist.Books) {
				var index = sortedList.BinarySearch (book, PlaylistBook.Empty);
				if (index < 0)
					playlist.Books.Add (book);
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
				// AddPlaylistListsPageFromQueue (auth, completionHandler);
			}
		}
			
		private static void AddPlaylistListsPage(SPTAuth auth, SPTListPage playlistlists, Action<UserPlaylist, bool> completionHandler)
		{
			if (playlistlists == null) 
				return;
			if (auth == null || auth.Session == null)
				auth = CurrentPlayer.Current.AuthPlayer;
			if (auth == null || auth.Session == null)
				return;
			var items = playlistlists.Items as NSObject[];
			if (items == null)
				return;
			var playListArray = items.Select (a => a as SPTPartialPlaylist).ToArray ();
			if (playListArray != null && playListArray.Any()) {
				Queue<Tuple<SPTPartialPlaylist, SPTListPage>> RequestsQueue = new Queue<Tuple<SPTPartialPlaylist, SPTListPage>> ();
				GetPlaylist(playListArray, completionHandler, RequestsQueue);
			}
			if (playlistlists.HasNextPage) {
				// BreathFirstQueue.Enqueue (playlistlists);

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
		private static void GetPlaylist(IEnumerable<SPTPartialPlaylist> playlistTotalArray, Action<UserPlaylist, bool> completionHandler, Queue<Tuple<SPTPartialPlaylist, SPTListPage>> RequestsQueue)
		{
			IEnumerable<SPTPartialPlaylist> playListArray = playlistTotalArray.Count () > maxConcorrent ? playlistTotalArray.Take (maxConcorrent) : playlistTotalArray;
			IEnumerable<SPTPartialPlaylist> playlistRestArray = playlistTotalArray.Count () > maxConcorrent ? playlistTotalArray.Skip(maxConcorrent) : null;
		
			foreach (var playlist in playListArray) {
				Console.WriteLine ("Start PlaylistBook.CreateFromFullAsync: "+playlist.Name);
				CallCreateFromAsync (playlist, completionHandler, playlistRestArray, RequestsQueue);
			}
		}

		static void CallCreateFromAsync(SPTPartialPlaylist playlist, Action<UserPlaylist, bool> completionHandler, IEnumerable<SPTPartialPlaylist> playlistRestArray, Queue<Tuple<SPTPartialPlaylist, SPTListPage>> RequestsQueue)
		{
			PlaylistBook.CreateFromFullAsync(playlist, (IEnumerable<PlaylistBook> playlistBooks, bool isComplete, SPTListPage nextPageForPlaylist) => {
				// will be called only one time for breath first.
				CompletionHandlerForPlaylist(playlist, completionHandler, RequestsQueue, playlistBooks, isComplete, nextPageForPlaylist);
			}, () => {
				// will be called when finished with this list
				FinishedHandlerForPlaylist(playlist, completionHandler, playlistRestArray, RequestsQueue);
			});			
		}
		static void CompletionHandlerForPlaylist(SPTPartialPlaylist playlist,  Action<UserPlaylist, bool> completionHandler, Queue<Tuple<SPTPartialPlaylist, SPTListPage>> RequestsQueue, IEnumerable<PlaylistBook> playlistBooks, bool isComplete, SPTListPage nextPageForPlaylist)
		{
			Console.WriteLine ("Callback PlaylistBook.CreateFromFullAsync: "+playlist.Name);
			if (playlistBooks != null) {
				if (completionHandler != null) {
					completionHandler(new UserPlaylist() {
						Name = playlist.Name,
						TrackCount = (uint) playlist.TrackCount,
						LargeImageUrl = playlist.LargestImage?.ImageURL?.AbsoluteString,
						SmallImageUrl = playlist.SmallestImage?.ImageURL?.AbsoluteString,
						Books = playlistBooks.ToList(),
					}, isComplete);
				}
			}
			if (nextPageForPlaylist != null && nextPageForPlaylist.HasNextPage) {
				RequestsQueue.Enqueue(new Tuple<SPTPartialPlaylist, SPTListPage>(playlist, nextPageForPlaylist));
			}
		}
		static void FinishedHandlerForPlaylist(SPTPartialPlaylist playlist,  Action<UserPlaylist, bool> completionHandler, IEnumerable<SPTPartialPlaylist> playlistRestArray, Queue<Tuple<SPTPartialPlaylist, SPTListPage>> RequestsQueue)
		{
			// next book.
			if (playlistRestArray != null) {						
				GetPlaylist(playlistRestArray, completionHandler, RequestsQueue);
				Console.WriteLine ("Continue Next Playlist PlaylistBook.CreateFromFullAsync: "+playlist.Name + "  Left:" +playlistRestArray.Count());
			}
			else if (RequestsQueue.Count > 0) {
				var t = RequestsQueue.Dequeue();
				Console.WriteLine ("Continue Next Queued Page PlaylistBook.CreateFromFullAsync: "+t.Item1.Name);
				PlaylistBook.CreateRequestNextPage(t.Item2, (IEnumerable<PlaylistBook> playlistBooks, bool isComplete, SPTListPage nextPageForPlaylist) => {
					// will be called only one time for breath first.
					CompletionHandlerForPlaylist(t.Item1, completionHandler, RequestsQueue, playlistBooks, isComplete, nextPageForPlaylist);
				}, () => {
					// finished handler
					FinishedHandlerForPlaylist(t.Item1, completionHandler, playlistRestArray, RequestsQueue);
				});
			} else {
				Console.WriteLine ("Stop PlaylistBook.CreateFromFullAsync: "+playlist.Name);
				if (completionHandler != null)
					completionHandler(null, true);
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

