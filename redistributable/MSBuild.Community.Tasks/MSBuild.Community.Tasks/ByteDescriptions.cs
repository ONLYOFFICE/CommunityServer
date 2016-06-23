using System;
using System.Collections.Generic;
using System.Text;

namespace MSBuild.Community.Tasks {

	/// <summary>
	/// Describes certain byte measurements as nice strings.
	/// </summary>
	static internal class ByteDescriptions {

		public static string FormatByte(long bytes, int rounding) {
			if (bytes >= System.Math.Pow(2, 80))
				return System.Math.Round(bytes / System.Math.Pow(2, 70), rounding).ToString() + " YB"; //yettabyte
			if (bytes >= System.Math.Pow(2, 70))
				return System.Math.Round(bytes / System.Math.Pow(2, 70), rounding).ToString() + " ZB"; //zettabyte
			if (bytes >= System.Math.Pow(2, 60))
				return System.Math.Round(bytes / System.Math.Pow(2, 60), rounding).ToString() + " EB"; //exabyte
			if (bytes >= System.Math.Pow(2, 50))
				return System.Math.Round(bytes / System.Math.Pow(2, 50), rounding).ToString() + " PB"; //petabyte
			if (bytes >= System.Math.Pow(2, 40))
				return System.Math.Round(bytes / System.Math.Pow(2, 40), rounding).ToString() + " TB"; //terabyte
			if (bytes >= System.Math.Pow(2, 30))
				return System.Math.Round(bytes / System.Math.Pow(2, 30), rounding).ToString() + " GB"; //gigabyte
			if (bytes >= System.Math.Pow(2, 20))
				return System.Math.Round(bytes / System.Math.Pow(2, 20), rounding).ToString() + " MB"; //megabyte
			if (bytes >= System.Math.Pow(2, 10))
				return System.Math.Round(bytes / System.Math.Pow(2, 10), rounding).ToString() + " KB"; //kilobyte

			return bytes.ToString() + " Bytes"; //byte
		}

		public static string FormatBytesPerSecond(long bytes, double secounds, int rounding) {
			double bytesPerSecounds = bytes / secounds;

			if (bytesPerSecounds >= System.Math.Pow(2, 80))
				return System.Math.Round(bytesPerSecounds / System.Math.Pow(2, 70), rounding).ToString() + " YB/s"; //yettabyte
			if (bytesPerSecounds >= System.Math.Pow(2, 70))
				return System.Math.Round(bytesPerSecounds / System.Math.Pow(2, 70), rounding).ToString() + " ZB/s"; //zettabyte
			if (bytesPerSecounds >= System.Math.Pow(2, 60))
				return System.Math.Round(bytesPerSecounds / System.Math.Pow(2, 60), rounding).ToString() + " EB/s"; //exabyte
			if (bytesPerSecounds >= System.Math.Pow(2, 50))
				return System.Math.Round(bytesPerSecounds / System.Math.Pow(2, 50), rounding).ToString() + " PB/s"; //petabyte
			if (bytesPerSecounds >= System.Math.Pow(2, 40))
				return System.Math.Round(bytesPerSecounds / System.Math.Pow(2, 40), rounding).ToString() + " TB/s"; //terabyte
			if (bytesPerSecounds >= System.Math.Pow(2, 30))
				return System.Math.Round(bytesPerSecounds / System.Math.Pow(2, 30), rounding).ToString() + " GB/s"; //gigabyte
			if (bytesPerSecounds >= System.Math.Pow(2, 20))
				return System.Math.Round(bytesPerSecounds / System.Math.Pow(2, 20), rounding).ToString() + " MB/s"; //megabyte

			return System.Math.Round(bytesPerSecounds / System.Math.Pow(2, 10), rounding).ToString() + " KB/s"; //kilobyte
		}


	}
}
