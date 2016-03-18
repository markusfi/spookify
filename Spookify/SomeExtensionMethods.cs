using System;

namespace Spookify
{
	public static class SomeExtensionMethods
	{
		public static DateTime NSDateToDateTime(this Foundation.NSDate date)
		{
			DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0);
			DateTime currentDate = reference.AddSeconds(date.SecondsSinceReferenceDate);
			DateTime localDate = currentDate.ToLocalTime ();
			return localDate;
		}
	}
}

