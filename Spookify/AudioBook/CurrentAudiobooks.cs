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
			get
			{
				var current = CurrentBase<CurrentAudiobooks>.Current;
				current.TriggerRefresh();
				if (!current.HasPlaylists && CurrentAudiobooks.refreshRunning != null)
					return CurrentAudiobooks.refreshRunning;
				else
					return current;
			}
		}
		public static CurrentAudiobooks CurrentNoTriggerRefresh
		{
			get {
				var current = CurrentBase<CurrentAudiobooks>.Current;
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
					_user = new PlaylistOwner () { Name =
//						 "europa.kinderprogramm" 
						 "argonhörbücher,hoerbuecher,europa.kinderprogramm" 
					};
					_user.Changed += OnChanged;
					LastUpdate = DateTime.UtcNow;
				}
				return _user;
			}
			set {
				if (_user != value) {
					if (_user != null)
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
			if (refreshRunning != null)
				return;
			if (this.HasPlaylists &&
			    (DateTime.UtcNow - LastUpdate).TotalDays < 1)
				return;

			var remoteHoststatus = Reachability.RemoteHostStatus ();
			if (remoteHoststatus == NetworkStatus.NotReachable)
				return;
			if ((remoteHoststatus == NetworkStatus.ReachableViaCarrierDataNetwork) &&
				((DateTime.UtcNow - LastUpdate).TotalDays < 7))
				return;
			
			if (!CurrentPlayer.Current.IsSessionValid)
				return;
			
			lock (typeof(CurrentAudiobooks)) {
				if (refreshRunning == null) {
					refreshRunning = new CurrentAudiobooks ();
					PlaylistChangedEventHandler handler = (object sender, PlaylistChangedEventArgs e) => {
						if (refreshRunning.IsComplete) {
							if ((this._user == null) || 
								(refreshRunning.User.Playlists.Count > 20))
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
							}
						}
					};
					refreshRunning.Changed += handler;
					// trigger reload here.
					var dummy = refreshRunning.User.Playlists;
				}
			}
		}
		public bool IsComplete {
			get {
				return HasPlaylists && User.Playlists.All(p => p.IsComplete);
			}
		}
		public bool HasPlaylists {
			get {
				return this._user != null && this._user.PlaylistsInitialized && User.Playlists.Any () && this._user.Playlists.Any (p => p.Books != null && p.Books.Any (b => b != null && b.Uri != null));
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
		public bool PlaylistsInitialized { get { return _playlists != null; } }

		List<UserPlaylist> _playlists;
		public List<UserPlaylist> Playlists {
			get { 
				if (_playlists == null) {
					_playlists = new List<UserPlaylist> ();
					var playlistManager = new PlaylistManager ();
					playlistManager.EnqueueUser (this.Name.Split (new [] { "," }, StringSplitOptions.RemoveEmptyEntries));
					/*
					playlistManager.AddConstantUris (playlists => {
						foreach(var playlist in playlists) 
							AddPlaylists(playlist, false);
					});
					*/
					ConfigListen.ConfigPosition (_playlists, 10000);
					playlistManager.GetUserPlaylistsAsync (GetPlaylistCompletionhandler);
				}
				if (_playlists != null) {
					if (!ordered) {
						ConfigListen.ConfigPosition (_playlists);
								
						_playlists.Sort (ConfigListen.Comparison);
						ordered = true;
					}
				}
				return _playlists;
			}
		}
		bool AddPlaylists(UserPlaylist newUserPlaylist, bool isComplete)
		{
			if (newUserPlaylist == null)
				return false;
						
			lock(_playlists) {
				var p = _playlists.FirstOrDefault(p1 => p1.Name == newUserPlaylist.Name);
				if (p != null) {
					bool FirstBooksAdded = p.Books == null || p.Books.Count == 0;
					// p.Books.AddRange(newUserPlaylist.Books);
					AddWithoutDups(_playlists, p, newUserPlaylist);
					if (isComplete && p.Books!= null) 
						p.TrackCount = (uint)p.Books.Count;
					p.IsComplete = isComplete;
					#if DEBUG
						Console.WriteLine("Playlist "+newUserPlaylist.Name + "  TrackCount: "+p.TrackCount+ "  Books.Count: "+p.Books.Count());
					#endif
					return true; //FirstBooksAdded && newUserPlaylist.Books != null && newUserPlaylist.Books.Count > 0;
				} else {
					p = new UserPlaylist() { Books = new List<PlaylistBook>(), Name = newUserPlaylist.Name };
					AddWithoutDups(_playlists, p, newUserPlaylist);
					newUserPlaylist.Books = p.Books;
					_playlists.Add(newUserPlaylist);
					if (isComplete && newUserPlaylist.Books != null)
						newUserPlaylist.TrackCount = (uint)newUserPlaylist.Books.Count;
					newUserPlaylist.IsComplete = isComplete;
					#if DEBUG
					Console.WriteLine("Playlist "+newUserPlaylist.Name + "  TrackCount: "+newUserPlaylist.TrackCount+ "  Books.Count: "+newUserPlaylist.Books.Count());
					#endif
					return newUserPlaylist.Books != null && newUserPlaylist.Books.Count > 0;
				}
			}
		}
		void GetPlaylistCompletionhandler(IEnumerable<UserPlaylist> newUserPlaylists, PlaylistManager playlistManager)
		{
			// will be called one time when all playlists are enumerated.
			if (newUserPlaylists != null) {
				foreach (var newUserPlaylist in newUserPlaylists)
					AddPlaylists (newUserPlaylist, false);
				ordered = false;
				OnChanged (this, new PlaylistChangedEventArgs (""));
			}
			if (playlistManager.HasEnqueuedUser) {
				playlistManager.GetUserPlaylistsAsync (completionHandler: GetPlaylistCompletionhandler);
				ordered = false;
				OnChanged (this, new PlaylistChangedEventArgs (""));
			} else {
				playlistManager.GetPlaylistTracks (_playlists, completionHandler: (userPlaylist, isBookReadComplete) => {
					bool firstBooksAdded = AddPlaylists (userPlaylist, isBookReadComplete);
					if (firstBooksAdded ||_playlists.All (p => p.IsComplete)) {
						ordered = false;
						OnChanged (this, new PlaylistChangedEventArgs (userPlaylist?.Name));
					}  
						
				});
			}
		}
		bool ordered = false;

		void AddWithoutDups(List<UserPlaylist> master, UserPlaylist playlist, UserPlaylist newSublist)
		{
			if (playlist == null || playlist.Books == null)
				return;
			string[] noDups = { "Bestseller", "Geheimtipps" };
			bool doNotAllowDups = noDups.Any(s => playlist.Name.Contains(s));
			var sortedList = doNotAllowDups 
				? master.SelectMany (p => p?.Books ?? new List<PlaylistBook> ()).ToList ()
				: playlist.Books.ToList();
			if (newSublist == null || newSublist.Books == null) 
				return;
			sortedList.Sort (PlaylistBook.CompareUri);
			newSublist.Books.Sort (PlaylistBook.CompareUri);
			PlaylistBook lastBook = null;
			foreach (var book in newSublist.Books) {
				if (PlaylistBook.CompareUri(lastBook,book)!=0) {
					var index = sortedList.BinarySearch (book, PlaylistBook.Empty);
					if (index < 0)
						playlist.Books.Add (book);
				}
				lastBook = book;
			}
		}
	}

	[Serializable]
	public class UserPlaylist
	{
		public UserPlaylist() {}
		//"Shakespeare: The Poetry","spotify:user:spotify:playlist:18uLNnbFxvQhr8gnytJlYJ",162,300,"https://i.scdn.co/image/33408d37ef01e05a594c71a988f635c8989e78f7",
		public UserPlaylist(string name, string uri, uint trackCount, string imageUrl) {
			this.Name = name;
			this.Uri = uri;
			this.TrackCount = trackCount;
			this.SmallImageUrl = imageUrl;	
		}
		public string Name { get; set; }
		public uint TrackCount { get; set; }
		public string LargeImageUrl { get; set; }
		public string[] ImageUrls { get; set; }
		public string SmallImageUrl { get; set; }
		public string MeidumImageUrl { 
			get { 
				var medium = ImageUrls?.FirstOrDefault (i => i != LargeImageUrl && i != SmallImageUrl);
				if (medium == null)
					medium = SmallImageUrl;
				return medium;
			}
		}
		public List<PlaylistBook> Books { get; set; }
		public string Uri { get; set; }
		public int Position { get; set; }

		public bool IsComplete { get; set; } = false;	

		public UserPlaylist Clone ()
		{
			return new UserPlaylist() {
				Name = this.Name,
				TrackCount = this.TrackCount,
				LargeImageUrl = this.LargeImageUrl,
				ImageUrls = this.ImageUrls,
				SmallImageUrl = this.SmallImageUrl,
				Books = this.Books,
				Uri = this.Uri,
				IsComplete = this.IsComplete
			};
		}
	}

}

