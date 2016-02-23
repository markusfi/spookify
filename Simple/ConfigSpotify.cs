using System;

namespace Simple
{
	public class ConfigSpotify
	{
		public ConfigSpotify ()
		{
		}

		// Your client ID for ABStream App
		public static string kClientId = "66a58efd5a7d44f4b6a1d44d8d78f1cd"; // "e6695c6d22214e0f832006889566df9c";

		// Your applications callback URL
		public static string kCallbackURL = "abstreamspotifyios://";

		// The URL to your token swap endpoint
		// If you don't provide a token swap service url the login will use implicit grant tokens, which means that your user will need to sign in again every time the token expires.

		public static string kTokenSwapServiceURL = null; // "http://localhost:1234/swap"

		// The URL to your token refresh endpoint
		// If you don't provide a token refresh service url, the user will need to sign in again every time their token expires.

		public static string kTokenRefreshServiceURL = null; // "http://localhost:1234/refresh"

		public static string kSessionUserDefaultsKey = "SpotifySession";
	}
}

