using System;

namespace Spookify
{
	public class ConfigListen
	{
		public static readonly string[] OrderedList = {
			"Hörbücher Neuheiten" ,
			"Hörbücher Bestseller & Ausgezeichnet" ,
			"Hörbücher Geheimtipps" ,
			"Alle Hörbücher" ,
			"Hörspiele Neuheiten" ,
			"Hörspiele Bestseller & Ausgezeichnet" ,
			"Hörspiele Neuheiten" ,
			"Hörspiele Bestseller & Ausgezeichnet" ,
			"Hörspiele Geheimtipps" ,
			"Alle Hörspiele" ,
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
			"All Audiobooks" ,
			"Music For Readers" };
		
		public static int PlaylistIndex(string playlist) {
			return Array.IndexOf(ConfigListen.OrderedList, playlist);
		}
		public static int Comparison(UserPlaylist s1, UserPlaylist s2)
		{
			int i1 = ConfigListen.PlaylistIndex (s1?.Name ?? "");
			int i2 = ConfigListen.PlaylistIndex (s2?.Name ?? "");
			return i1.CompareTo (i2);
		}	
	}
}

