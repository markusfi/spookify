using System;
using SpotifySDK;
using Foundation;
using System.Linq;
using System.Collections.Generic;

namespace Spookify
{
	public class PlaylistManager
	{
		public void AddConstantUris(Action<IEnumerable<UserPlaylist>> completionHandler = null)
		{
			var playlists = new UserPlaylist [] {

				new UserPlaylist("Hörspielzeit: Sherlock Holmes","spotify:user:spotify_germany:playlist:3arBnuazIAvgcGmSdNGwRH",132,"https://u.scdn.co/images/pl/default/f2fdc90378136aa6c701784845cbb1247a1bb1a6"),
				new UserPlaylist("Hörspielzeit: John Sinclair","spotify:user:spotify_germany:playlist:40xNhvCEjBJRe3w7s1MDXI",185,"https://u.scdn.co/images/pl/default/f0d9cbb33f0ab99b6d7cd2db9f765eecfe39a4be"),
				new UserPlaylist("Hörspielzeit: Hanni & Nanni","spotify:user:spotify_germany:playlist:3tlt2dDC1fMNKpNiJNDqpb",240,"https://u.scdn.co/images/pl/default/46d95949fb7465e408958422b9320ea9e2955de2"),
				new UserPlaylist("Hörspielzeit: Fünf Freunde","spotify:user:spotify_germany:playlist:1bwcStTX5DlTCoA7M92H6L",280,"https://u.scdn.co/images/pl/default/c4f99e35587ca749941a80e5649c72569f605587"),
				new UserPlaylist("Hörspielzeit: Bibi & Tina","spotify:user:spotify_germany:playlist:22Z063yUuv9QaoUuPLfErr",65,"https://u.scdn.co/images/pl/default/daf6b4f38862108f73c2da1df41014556bab301d"),
				new UserPlaylist("Hörspielzeit: Bibi Blocksberg","spotify:user:spotify_germany:playlist:3WUHW4ET94SyZ15AQGo0c7",67,"https://u.scdn.co/images/pl/default/5327f2c6597a23af3e3c14ac53f4d9fc43db6201"),
				new UserPlaylist("Hörspielzeit: Benjamin Blümchen","spotify:user:spotify_germany:playlist:79iIsF0EcFdlRm4dDopcI8",57,"https://u.scdn.co/images/pl/default/342e7d2359ff9343ed2413f48e06a6cd0dbc4118"),
				new UserPlaylist("Hörspielzeit: Sherlock Holmes","spotify:user:spotify_germany:playlist:3arBnuazIAvgcGmSdNGwRH",132,"https://u.scdn.co/images/pl/default/f2fdc90378136aa6c701784845cbb1247a1bb1a6"),
				new UserPlaylist("Hörspielzeit: John Sinclair","spotify:user:spotify_germany:playlist:40xNhvCEjBJRe3w7s1MDXI",185,"https://u.scdn.co/images/pl/default/f0d9cbb33f0ab99b6d7cd2db9f765eecfe39a4be"),
				new UserPlaylist("Hörspielzeit: Die Drei ???","spotify:user:spotify_germany:playlist:1HFgQcuzfzYzWtyeGHrhmF",200,"https://u.scdn.co/images/pl/default/4b985b446f4325625bf6c130782002651e9fe998"),
				new UserPlaylist("Hörspielzeit: TKKG","spotify:user:spotify_germany:playlist:0buSSThFPuzw3XAe3BJ1Qg",200,"https://u.scdn.co/images/pl/default/b132b286106741bb38594eb98d30cb9912bff57e"),
				new UserPlaylist("Schlaue Kids","spotify:user:spotify_germany:playlist:2LFZnJoLfk9whiVaQYhgYB",63,"https://u.scdn.co/images/pl/default/04e686416002e2a276c40dce52b5221976554ec4"),
				new UserPlaylist("Für die Kleinen","spotify:user:spotify_germany:playlist:7gt9xtbuUKP7uVe74vO1cl",264,"https://u.scdn.co/images/pl/default/94e41f6e27c62173b8d22cc51cd581ed4e32606f"),
				new UserPlaylist("Es war einmal","spotify:user:spotify_germany:playlist:1Uxc6clcAiP0k0zuATzjWY",60,"https://u.scdn.co/images/pl/default/25c76bc52fc0788d7edea724878647b3814af165"),
				new UserPlaylist("Hex Hex!","spotify:user:spotify_germany:playlist:7GQp80P1CHJbk60Th0gf7u",135,"https://u.scdn.co/images/pl/default/1b0b00360f15446eff5469ad2ff7c7c14dbb1197"),
				new UserPlaylist("Spannende Abenteuer","spotify:user:spotify_germany:playlist:6zFFuw3ODtmpFfgqyUOiLo",304,"https://u.scdn.co/images/pl/default/bbca607338121e23cd751f6a1d9c18c442e391b2"),
				new UserPlaylist("Detektiv Geschichten","spotify:user:spotify_germany:playlist:7132o4nsyWreqMU2PVSD84",175,"https://u.scdn.co/images/pl/default/9236ee021017389a2806bac74c0b9c787438c153"),

				new UserPlaylist("Audiobooks","spotify:user:spotify:playlist:30XOvpDSaJGTyWlHprlp2W",122,"https://i.scdn.co/image/015dd01e3a9d100baedc54574d9d3f1777fa4a20"),
				new UserPlaylist("Science Fiction","spotify:user:spotify:playlist:2ZGrUJ8GVyw3gGO3XgFTly",197,"https://i.scdn.co/image/483411639824644cdfa7f78ffc4539425213026b"),
				new UserPlaylist("Action & Adventure","spotify:user:spotify:playlist:5Zm7c0w7vbyygbrHhcCy2L",71,"https://i.scdn.co/image/d07a526e430bed6be56d9577e1fbbb846fd6498e"),
				new UserPlaylist("Scary Stories","spotify:user:spotify:playlist:3bboSFT6qxw9HB1mzqDj4O",140,"https://u.scdn.co/images/pl/default/f8263e87de7a968a44cabca4e2f0c69b02888ea0"),
				new UserPlaylist("Once Upon A Time","spotify:user:spotify:playlist:2fNFXgMy0x3IkJKg2kF94y",43,"https://u.scdn.co/images/pl/default/28da796f8c4a60408ddabf4852a24c4c4595a54f"),
				new UserPlaylist("Short Stories","spotify:user:spotify:playlist:2FmFvgiinkzpEESJ4zT0Va",65,"https://u.scdn.co/images/pl/default/aece8ec928c746e9b8bebe373ab9b6571acacd51"),

				/*
				new UserPlaylist("Shakespeare: The Poetry","spotify:user:spotify:playlist:18uLNnbFxvQhr8gnytJlYJ",162,"https://i.scdn.co/image/33408d37ef01e05a594c71a988f635c8989e78f7"),
				new UserPlaylist("Shakespeare: The Tragedies","spotify:user:spotify:playlist:5pplHTRwheMn6u5ae9FteQ",215,"https://i.scdn.co/image/c88c2ab2c33d4a8daeaa8edf151e36ef7448085d"),
				new UserPlaylist("Shakespeare: The Histories","spotify:user:spotify:playlist:6b8uOjZTIzs5oWynD9Pr9A",87,"https://i.scdn.co/image/53fdf11d05aa527fba931793fcfcb53c4ec5ddff"),
				new UserPlaylist("Shakespeare: The Comedies","spotify:user:spotify:playlist:7ef4WMBlqAISMDnphSvyRc",50,"https://i.scdn.co/image/c1301bce4d4eae8b6b8d06d48095cc3ad3f36a1a"),
				new UserPlaylist("Guided Meditation","spotify:user:spotify:playlist:7BI8kVITNyvDtW4x7lf3qq",123,"https://u.scdn.co/images/pl/default/6b639e101e76d78d4606b5089f1e47de36b34d9a"),
				new UserPlaylist("Scary Stories","spotify:user:spotify:playlist:3bboSFT6qxw9HB1mzqDj4O",140,"https://u.scdn.co/images/pl/default/f8263e87de7a968a44cabca4e2f0c69b02888ea0"),
				new UserPlaylist("Once Upon A Time","spotify:user:spotify:playlist:2fNFXgMy0x3IkJKg2kF94y",43,"https://u.scdn.co/images/pl/default/28da796f8c4a60408ddabf4852a24c4c4595a54f"),
				new UserPlaylist("Short Stories","spotify:user:spotify:playlist:2FmFvgiinkzpEESJ4zT0Va",65,"https://u.scdn.co/images/pl/default/aece8ec928c746e9b8bebe373ab9b6571acacd51"),
				new UserPlaylist("Mythologies","spotify:user:spotify:playlist:1OKh1wJPvv5U64L1hdfuc3",88,"https://i.scdn.co/image/e05731fda5c44842ec77bbfd76cd58844880a654"),
				new UserPlaylist("Self Help Gems","spotify:user:spotify:playlist:6IDvoSzchyq0rUTt78XUKY",174,"https://i.scdn.co/image/004d3b9d66f7fc347b6e480d5b6f1b844d6452e6"),
				new UserPlaylist("The Adventures of Sherlock Holmes","spotify:user:spotify:playlist:77tCMayJUagF3vlna28b4p",163,"https://i.scdn.co/image/ccdececbbec588610662e7f487fb9ced68af01bd"),
				new UserPlaylist("Love Poems","spotify:user:spotify:playlist:74JqL2BBmeS7X6KmoINXpX",35,"https://u.scdn.co/images/pl/default/b73caac412d825bd1995871cee59d714953e0ce3"),
				new UserPlaylist("Animal Stories","spotify:user:spotify:playlist:2at1I5TCLf6a1OH2jL8CCB",42,"https://u.scdn.co/images/pl/default/d89d8d6c21294024fe1f45415281edf1e14705f6"),
				new UserPlaylist("Science Fiction","spotify:user:spotify:playlist:2ZGrUJ8GVyw3gGO3XgFTly",197,"https://i.scdn.co/image/483411639824644cdfa7f78ffc4539425213026b"),
				new UserPlaylist("Presidential Voices","spotify:user:spotify:playlist:2SKskB12BHErcT8njaduHu",126,"https://u.scdn.co/images/pl/default/52b9e4478759583eb50690d9aa7583d445af9011"),
				new UserPlaylist("Andersen's Fairy Tales","spotify:user:spotify:playlist:4te4paSJJz8x8HeYDW2ZKP",20,"https://i.scdn.co/image/166bcb351c9bfb792ae677c0b414d92903d31329"),
				new UserPlaylist("Stories for your Inner Child","spotify:user:spotify:playlist:5TQKebIP2y3YGxoklr72aV",56,"https://u.scdn.co/images/pl/default/884fbadeebbdb33459dae9e033c7f9ace46cf1bb"),
				new UserPlaylist("The Essential Edgar Allen Poe","spotify:user:spotify:playlist:20qnZDC2bTpETZf5sLkuFg",66,"https://u.scdn.co/images/pl/default/6a953364906163aa953e5e890f41cd488150c77c"),
				new UserPlaylist("The Lectures of Joseph Campbell","spotify:user:spotify:playlist:61Ox0LYO2W8cWnXGabMDz5",592,"https://u.scdn.co/images/pl/default/2544b1557e8bc25fc607ecde238bb76b9fb54bda"),
				new UserPlaylist("Eastern Spirituality","spotify:user:spotify:playlist:0F5yjqu3wZ0Y84Q0goW0xg",107,"https://u.scdn.co/images/pl/default/d71fc90817035d2664d3bb3e6614a10caccbfb99"),
				new UserPlaylist("Sci-Fi Radio Dramas","spotify:user:spotify:playlist:1CYblmWDusIJDkpOUFgdaD",42,"https://i.scdn.co/image/b02dbb5ed0822559a2232b5b2b8062de5f7eb962"),
				new UserPlaylist("The H. P. Lovecraft Compendium","spotify:user:spotify:playlist:3uoRM3wbD1D0L4sHeBzCAb",64,"https://i.scdn.co/image/41ae21044513268531c910758453e99223412790"),
				new UserPlaylist("Radio Crime Dramas","spotify:user:spotify:playlist:5RqbKhmtueFLyJ9oCE6gLr",28,"https://u.scdn.co/images/pl/default/1c65a0de1952883132a2f058c478a7b92ca0ad03"),
				new UserPlaylist("Jane Austen","spotify:user:spotify:playlist:4GDkm6mxTUHpu10afSv49t",128,"https://u.scdn.co/images/pl/default/0ae8e5821a4289fb090f016fa76045074924cb26"),
				new UserPlaylist("Action & Adventure","spotify:user:spotify:playlist:5Zm7c0w7vbyygbrHhcCy2L",71,"https://i.scdn.co/image/d07a526e430bed6be56d9577e1fbbb846fd6498e"),
				new UserPlaylist("A Noam Chomsky Chronology","spotify:user:spotify:playlist:5dtafxqXHxYgzzSMBgkE5u",410,"https://i.scdn.co/image/eda18a8f6d17c85a7d031a4a3c4334eafe3c035e"),
				new UserPlaylist("Martin Luther King, Jr.","spotify:user:spotify:playlist:4iF9nBCL292c75YavnV8VI",53,"https://u.scdn.co/images/pl/default/6346834454456783f0033e9fa5e339e047e4a138"),
				new UserPlaylist("Poetry: In Their Own Voices","spotify:user:spotify:playlist:6vOC3SPWKaY1EPHxRJQwZ3",82,"https://u.scdn.co/images/pl/default/ac87118286be136ae6cf4cde5d2be216ce4eae03"),
				new UserPlaylist("A Hipster's Guide to Poetry","spotify:user:spotify:playlist:0FcbOsuw4YYAFS7DE2lCDc",31,"https://u.scdn.co/images/pl/default/93a260f273d12773cff38d2d1552634c4c75b21b"),
				new UserPlaylist("Vintage Radio Dramas","spotify:user:spotify:playlist:2CcJGqHqbLyWUoe2A7Uzrp",30,"https://i.scdn.co/image/e3e28e4ddb90387d2f7d76e8c0786c593d37605b"),
				new UserPlaylist("The Beats","spotify:user:spotify:playlist:31wIPggkodosrdDkEdv8Md",211,"https://u.scdn.co/images/pl/default/dca751d4dc68963a3d793bad096a61a490616977"),
				new UserPlaylist("Modern Poetry","spotify:user:spotify:playlist:7teeiXGjLUjfirGhBq5BXe",168,"https://i.scdn.co/image/ecdc361cbd281dc1f8ec2788d6bf96d82e28ce50"),
				new UserPlaylist("Sylvia Plath","spotify:user:spotify:playlist:3z3d6jfVn9Ag8Iu9MQ4MZV",41,"https://u.scdn.co/images/pl/default/0f8c3432ae5684799a2d37b6003507ec8b9bb616"),
				new UserPlaylist("Women's Lit","spotify:user:spotify:playlist:6Mzzarmszx614wbWzpkDgn",38,"https://u.scdn.co/images/pl/default/b5e86ed71fd3ce6ac7d02d25f6607482f35413a2"),
				new UserPlaylist("Editor's Choice: Fiction","spotify:user:spotify:playlist:3Ayb0OH54mBVXfFl8XF4Vu",94,"https://i.scdn.co/image/e30a7b3570d147492dabd1db86ec41a7a4243101"),
				new UserPlaylist("Langston Hughes","spotify:user:spotify:playlist:1iFdxLliGy3WKGoF7GaXY0",57,"https://u.scdn.co/images/pl/default/d98bde0c25f622bc829f270d849497d0bd80b7e7"),
				new UserPlaylist("The Brontës","spotify:user:spotify:playlist:54RJPE1jscE1aMopfwbSi6",141,"https://u.scdn.co/images/pl/default/f0a9a2e32cd3130e2ffbd71c5dcf20feccfa3a79"),
				new UserPlaylist("Irish Lit","spotify:user:spotify:playlist:64JZJ22Lx03Py1Pe0hu8jp",239,"https://i.scdn.co/image/d63683ae046df887f15c9dc6a965b7a865a95acb"),
				new UserPlaylist("Russian Lit","spotify:user:spotify:playlist:4BdcFzBlwtPrYD5EKgNI0O",65,"https://i.scdn.co/image/e6d68c3687539c646ee30b9daf42da24eac831a6"),
				new UserPlaylist("Editor's Choice: Nonfiction","spotify:user:spotify:playlist:7cdiohpIY7o2AxPpnhcm9h",59,"https://i.scdn.co/image/861855dc25e4e1243fc436939cedab470f9ed2cc"),
				new UserPlaylist("The Romantics","spotify:user:spotify:playlist:7LDothHkyw6to148j3ICWl",101,"https://i.scdn.co/image/c3ea99e8cb01a16f76da2e73af0cf058cf2b964f"),
				new UserPlaylist("Emily Dickinson","spotify:user:spotify:playlist:7kZYDERyqLZ13jLSVti8O8",150,"https://u.scdn.co/images/pl/default/37d778868893f5094ed8873ed084b36d8e65d612"),
				new UserPlaylist("La poésie française","spotify:user:spotify:playlist:3xc14Fuk2tqQKjLYmS32Y9",46,"https://i.scdn.co/image/a678986e7e4fb0f72c547f377dfc8309b773de2b"),
				new UserPlaylist("Lives & Works of Classical Composers","spotify:user:spotify:playlist:5lrY3FDoELssyngI6VSYGO",1150,"https://i.scdn.co/image/b3f16f6e0ca82f7ff93c68330e1df63cbb1dcc2d"),
				new UserPlaylist("Charles Dickens' Classics","spotify:user:spotify:playlist:03S7lNZ8oyvLlw5cS5kZKs",68,"https://i.scdn.co/image/073320072ebe67e44956286a528e234f9cad37ef"),
				new UserPlaylist("The Robert Frost Reading Room","spotify:user:spotify:playlist:6KD6rj23cRoghuMTjzdcTH",64,"https://i.scdn.co/image/44cbec65bcef9f77eaedf898a6c5a4e5d7dffb54"),
				new UserPlaylist("Poetry & Music","spotify:user:spotify:playlist:5eKZfTqyjEQHIC11r7fRKi",26,"https://i.scdn.co/image/1bdc4963af2b7f4125221d3ff9176e3264ffdb85"),
				new UserPlaylist("Christmas Stories","spotify:user:spotify:playlist:77PLo6ee7tr4RlvqcWAlGX",38,"https://u.scdn.co/images/pl/default/9b1068eb54956e0e63cdb203a47ce75ed59f1144"),
				new UserPlaylist("La littérature française","spotify:user:spotify:playlist:5S0tZYsndmxsNea05WRc0A",95,"https://i.scdn.co/image/ffca00aadf6ee7e1db068327713f6a4a39864d00"),
				new UserPlaylist("French Literature","spotify:user:spotify:playlist:1CvsrtWCYp46ewtpPt2npe",40,"https://i.scdn.co/image/aa664706b538b9b0fa48fda2de7d1c1309116dbf"),
				new UserPlaylist("Poems for Spring","spotify:user:spotify:playlist:1ZvcwQzR7Cy0mhy8J3LfzR",32,"https://i.scdn.co/image/68cc369e1987c94a7b74a49521f15229f2803b95"),
				new UserPlaylist("Blake’s 7: The Audio Series","spotify:user:spotify:playlist:2UiH8FRStT4RlfYHHA14KT",107,"https://i.scdn.co/image/35628ff0b6fdbd575845afa5393d8f97d604c068"),
				new UserPlaylist("The Artist Speaks","spotify:user:spotify:playlist:1RItkvGykHLAdmxXxiecUD",76,"https://i.scdn.co/image/22249a9fd8dd696031017e5b84c87114fb2d5990"),
				new UserPlaylist("A Child's Garden of Verses","spotify:user:spotify:playlist:4Eo0ZMBdHTJXUXNEkH6p8o",41,"https://i.scdn.co/image/802030fb6927e0d2200124b0b9a27893bb97b0e5"),
				new UserPlaylist("Irish Studies","spotify:user:spotify:playlist:0WuqMujO2Q0bgcsEYYB9wq",122,"https://i.scdn.co/image/8b9a0fa385ce068402e4868e37ff3b7d4219197a"),
				new UserPlaylist("The Victorians","spotify:user:spotify:playlist:7hx1an8C8YbmKyL6xENjIM",289,"https://i.scdn.co/image/1d8d1497c93cdb31a213570da6d1730da5f1c319"),
				new UserPlaylist("Joyceana","spotify:user:spotify:playlist:6baiMPinhbFRxqp85eqoEt",41,"https://i.scdn.co/image/93cdc0a38da10ce976c3e1460dc1952a3ea2a077"),
				new UserPlaylist("The Selected W. B. Yeats","spotify:user:spotify:playlist:2nyrqC0HzcLOpCnnuoiMFj",42,"https://i.scdn.co/image/fc069a948c491bbb363e27895f34aa5e72273bfb"),
				new UserPlaylist("Music Lessons from Pete Seeger","spotify:user:spotify:playlist:0Be9F7mXZnkgEYWqcBZ3r9",65,"https://i.scdn.co/image/94dd22b7014acf0b0c2e557a906718822de92f4f"),
				new UserPlaylist("Readings from Dylan Thomas","spotify:user:spotify:playlist:3RR7Kf8Y2fVslLZa4CJC7A",102,"https://i.scdn.co/image/c69b2af5cb8af458af3891cdfd38ca5553cd5cea"),
				new UserPlaylist("The Selected D. H. Lawrence","spotify:user:spotify:playlist:3EP7PyeGMYkw1qAgrOAWdW",84,"https://u.scdn.co/images/pl/default/e5fd19bf836fc49cc0d1729b89d253bc4fba7141"),
				new UserPlaylist("Margaret Walker","spotify:user:spotify:playlist:3LiPbHyegMUgxRcVNHAvN0",52,"https://u.scdn.co/images/pl/default/42ba6b13a6bb6abdc07395ed943c3d9a2d6c0efb"),
				new UserPlaylist("Edna St. Vincent Millay","spotify:user:spotify:playlist:4qm9xqoIxtZqN6JFMGcMDC",18,"https://u.scdn.co/images/pl/default/ce5d32438bca9f55d0183e23cbffb5d0e07bd93f"),			
				new UserPlaylist("Learn Spanish","spotify:user:spotify:playlist:4Rv5Y7NC1sSaZMzmMKuMNy",459,"https://i.scdn.co/image/6d2f4c0d2afad95e1595ba801f9205fcf10404f7"),
				new UserPlaylist("Learn French","spotify:user:spotify:playlist:7N0juWIEz0YwVjQLEvN4Xe",472,"https://i.scdn.co/image/13a41c2ea034560aa4c8361717805b4653f570f6"),
				new UserPlaylist("Learn Italian","spotify:user:spotify:playlist:5Jyk6w3uopeJcNSWBkyi2Z",608,"https://i.scdn.co/image/cbb8a3443061cba2feeb1045d3af52c5f99624e2"),
				new UserPlaylist("Learn German","spotify:user:spotify:playlist:2ReADxGZJBZvFt4qjvZsMw",409,"https://i.scdn.co/image/459644a384b4e795c939420bde539b009d27bc50"),
				new UserPlaylist("Learn Chinese","spotify:user:spotify:playlist:1MCHFaDzxodOX1tkStGEKj",263,"https://i.scdn.co/image/ec45be79ab01923273b957ea322ef7c65b03b254"),
				new UserPlaylist("Learn Irish","spotify:user:spotify:playlist:0uR8L2755Bi3o98v5rqrli",71,"https://i.scdn.co/image/b7d231885f5ec817c4ea1d765de7ce46e4a3f7d2"),
				new UserPlaylist("Learn Russian","spotify:user:spotify:playlist:6e5OcF9KdRKtIIwxyXQe1y",174,"https://u.scdn.co/images/pl/default/97b6a44dea4705ce571d5540168a168aecc7b4d0"),
				new UserPlaylist("Learn Arabic","spotify:user:spotify:playlist:41RIKkwh90vZpUHgXkdnae",89,"https://i.scdn.co/image/630782013596f395e3e5e14263d2bf52b2a8b8ca"),
				new UserPlaylist("Learn Portuguese","spotify:user:spotify:playlist:1CzILE6Y8rNPBQOGkN3EaA",53,"https://i.scdn.co/image/df7cf4dfcf461421460a815dae00e08a680a6c06"),
				new UserPlaylist("Learn Swedish","spotify:user:spotify:playlist:30rZMfd0AM9zRZyNB0fzLw",85,"https://i.scdn.co/image/20568c7e3757f360bc2a8b18345bbabe969de539"),
				*/
			};

			var playlists1 = new UserPlaylist [] {

				new UserPlaylist ("Hörspielzeit: Hanni & Nanni", "spotify:user:spotify_germany:playlist:3tlt2dDC1fMNKpNiJNDqpb", 240, "https://u.scdn.co/images/pl/default/46d95949fb7465e408958422b9320ea9e2955de2"),

			};
			completionHandler (playlists);
		}
		public void GetPlaylistTracks(IEnumerable<UserPlaylist> playlists, Action<UserPlaylist, bool> completionHandler = null)
		{
			if (CurrentPlayer.Current.IsSessionValid) {
				SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
				if (auth.Session != null) {
					var RequestsQueue = new Queue<Tuple<UserPlaylist, SPTListPage>> ();
					foreach (var p in playlists) {
						RequestsQueue.Enqueue (new Tuple<UserPlaylist, SPTListPage> (p, null));
					}
					FinishedHandlerForPlaylistBooks (completionHandler, RequestsQueue);
				}
			}
		}
		void CompletionHandlerForPlaylistBooks(UserPlaylist playlist,  Action<UserPlaylist, bool> completionHandler, Queue<Tuple<UserPlaylist, SPTListPage>> RequestsQueue, IEnumerable<PlaylistBook> playlistBooks, bool isComplete, SPTListPage nextPageForPlaylist)
		{
			Console.WriteLine ("Callback PlaylistBook.CreateFromFullAsync: "+playlist.Name);
			if (playlistBooks == null)
				return;
			if (completionHandler == null)
				return;
			var newPlaylist = playlist.Clone ();
			newPlaylist.Books = playlistBooks != null ? playlistBooks.ToList () : null; 
			completionHandler (newPlaylist, isComplete);
			if (nextPageForPlaylist == null)
				return;
			if (RequestsQueue == null)
				return;
			if (nextPageForPlaylist.HasNextPage) {
				RequestsQueue.Enqueue(new Tuple<UserPlaylist, SPTListPage>(playlist, nextPageForPlaylist));
			}
		}
		void FinishedHandlerForPlaylistBooks(Action<UserPlaylist, bool> completionHandler, Queue<Tuple<UserPlaylist, SPTListPage>> RequestsQueue)
		{
			if (completionHandler == null)
				return;
			if (RequestsQueue.Count > 0) {
				var t = RequestsQueue.Dequeue();
				Console.WriteLine ("Continue Next Queued Page PlaylistBook.CreateFromFullAsync: "+t.Item1.Name);
				var playlist = t.Item1;
				var nextPage = t.Item2;
				if (nextPage == null) {
					PlaylistBook.CreatePlaylistFromUriAsync (new NSUrl (playlist.Uri), (IEnumerable<PlaylistBook> playlistBooks, bool isComplete, SPTListPage nextPageForPlaylist) => {
						// will be called only one time for breath first.
						CompletionHandlerForPlaylistBooks (playlist, completionHandler, RequestsQueue, playlistBooks, isComplete, nextPageForPlaylist);
					}, () => {
						// will be called when finished with this list
						FinishedHandlerForPlaylistBooks (completionHandler, RequestsQueue);
					});		
				} else {
					PlaylistBook.CreateRequestNextPage (nextPage, (IEnumerable<PlaylistBook> playlistBooks, bool isComplete, SPTListPage nextPageForPlaylist) => {
						// will be called only one time for breath first.
						CompletionHandlerForPlaylistBooks (playlist, completionHandler, RequestsQueue, playlistBooks, isComplete, nextPageForPlaylist);
					}, () => {
						// finished handler
						FinishedHandlerForPlaylistBooks (completionHandler, RequestsQueue);
					});
				}
			} else {
				Console.WriteLine ("Stop GetPlaylistTracks");
				completionHandler(null, true);
			}
		}

		Queue<string> userQueue;
		public void EnqueueUser(IEnumerable<string> users)
		{
			if (userQueue == null)
				userQueue = new Queue<string> ();
			foreach (var user in users)
				userQueue.Enqueue (user);
		}
		public bool HasEnqueuedUser { get { return userQueue != null && userQueue.Count > 0; } }

		public void GetUserPlaylistsAsync(Action<IEnumerable<UserPlaylist>, PlaylistManager> completionHandler = null)
		{
			if (userQueue != null && userQueue.Count > 0)
				GetUserPlaylistsAsync (userQueue.Dequeue (), completionHandler);
			else
				completionHandler (null, this);
		}

		public void GetUserPlaylistsAsync(string playlistOwner, Action<IEnumerable<UserPlaylist>, PlaylistManager> completionHandler = null) 
		{
			if (CurrentPlayer.Current.IsSessionValid) {
				SPTAuth auth = CurrentPlayer.Current.AuthPlayer;
				if (auth.Session != null) {
					SPTPlaylistList.PlaylistsForUser (playlistOwner, auth.Session.AccessToken, (nsError, obj) => {
						if (nsError != null) 
							return;
						AddPlaylistListsPage(auth, obj as SPTPlaylistList, completionHandler);
					});
				}
			}
		}

		private void AddPlaylistListsPage(SPTAuth auth, SPTListPage playlistlists, Action<IEnumerable<UserPlaylist>, PlaylistManager> completionHandler, IEnumerable<SPTPartialPlaylist>resultPlaylists = null)
		{
			if (playlistlists == null) 
				return;
			if (auth == null || auth.Session == null)
				auth = CurrentPlayer.Current.AuthPlayer;
			if (auth == null || auth.Session == null)
				return;
			var items = playlistlists.Items as NSObject[];
			if (items != null) {
				if (resultPlaylists == null)
					resultPlaylists = items.Select (a => a as SPTPartialPlaylist);
				else
					resultPlaylists = resultPlaylists.Union (items.Select (a => a as SPTPartialPlaylist));
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
					AddPlaylistListsPage (auth, nextpage, completionHandler, resultPlaylists);
				});
			} else {
				// last Page...
				if (resultPlaylists == null)
					return;				
				if (completionHandler == null)
					return;

				Console.WriteLine ("Stop PlaylistBook.CreateFromFullAsync: "+resultPlaylists.Aggregate("", (a,pl) => a == null ? pl.Name : a + ", "+pl.Name));

				completionHandler(resultPlaylists.Select(playlist => new UserPlaylist() {
					Name = playlist.Name,
					TrackCount = (uint) playlist.TrackCount,
					LargeImageUrl = playlist.LargestImage?.ImageURL?.AbsoluteString,
					SmallImageUrl = playlist.SmallestImage?.ImageURL?.AbsoluteString,
					ImageUrls = playlist.Images?.Select(i => i?.ImageURL?.AbsoluteString).ToArray(),
					Books =null,
					Uri = playlist.Uri.AbsoluteString
				}), this);
			}
		}

	}
}

