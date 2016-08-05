using System;
using System.Collections.Generic;

namespace Spookify
{
	public class ConfigListen
	{
		public static readonly string[] OrderedList = {
			"Hörbücher Neuheiten" ,
			"Hörbücher Bestseller & Ausgezeichnet" ,
			"Hörbücher Geheimtipps" ,
			"Mit Star bewertet",
			"Alle Hörbücher" ,
			"Hörspiele Neuheiten" ,
			"Hörspiele Bestseller & Ausgezeichnet" ,
			"Hörspiele Neuheiten" ,
			"Hörspiele Bestseller & Ausgezeichnet" ,
			"Hörspiele Geheimtipps" ,
			"Alle Hörspiele" ,
			"Hörbücher Special Playlist - EM 2016" ,
			"Hörbücher Charts Top 20" ,
			"Hörbücher Kinder" ,
			"Hörbücher Jugend" ,
			"Hörbücher Spannung" ,
			"Hörbücher Humor" ,
			"Hörbücher Comedy", 
			"Hörbücher Romane" ,
			"Hörbücher Sci-Fi & Fantasy" ,
			"Hörbücher Herzschmerz" ,
			"Hörbücher Kabarett" ,
			"Hörbücher Klassiker" ,
			"Hörbücher Sachbücher" ,
			"Hörbücher Balance & Wellness" ,
			"All Audiobooks" };

		public static readonly string[] StopList = {
			"Music For Readers", "Kinderlieder","Gute Nacht Sterne","Traumstern-Orchester", 
		};

		public static void ConfigPosition(IEnumerable<UserPlaylist>playlists, int start = 1000)
		{
			int pos = start;
			foreach (var p in playlists) {
				if (p.Position == 0) {
					var index = PlaylistIndex (p.Name);
					if (index == -1)
						p.Position = (++pos);
					else
						p.Position = index;
				}
			}
		}
		public static int PlaylistIndex(string playlist) {
			return Array.IndexOf(ConfigListen.OrderedList, playlist);
		}
		public static int Comparison(UserPlaylist s1, UserPlaylist s2)
		{			
			return s1.Position.CompareTo (s2.Position);
		}	
	}
}

