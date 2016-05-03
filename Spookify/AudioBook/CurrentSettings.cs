using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

namespace Spookify
{
	[Serializable]
	public class CurrentSettings : CurrentBase<CurrentSettings>
	{
		public CurrentSettings ()
		{
			kTokenSwapService = true;
			kTokenRefreshService = true;
		}

		public bool kTokenSwapService { get; set; }
		public bool kTokenRefreshService { get; set; }
	}
}

