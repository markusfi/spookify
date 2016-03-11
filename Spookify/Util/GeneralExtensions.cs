using System;
using Foundation;
using UIKit;

namespace Spookify
{
	public static class GeneralExtensions
	{
		public static byte[] ToByteArray (this NSData data) {
			if (data != null) {
				var dataBytes = new byte[data.Length];
				System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));
				return dataBytes;
			} else
				return null;
		}
		public static UIImage ToImage(this byte[] arr)  
		{
			if (arr != null) {
				using (var data = NSData.FromArray (arr)) {
					return UIImage.LoadFromData (data);
				}
			} else
				return null;
		}
	}
}

