using System;
using UIKit;
using System.Threading.Tasks;
using Foundation;

namespace Spookify
{
	public interface ISleepTimerController
	{
		void DisplayAlbum();
		DateTime SleepTimerStartTime { get; set; }
		int SleepTimerOpion { get; set; }
		UIView View { get; }
	}
	
}
