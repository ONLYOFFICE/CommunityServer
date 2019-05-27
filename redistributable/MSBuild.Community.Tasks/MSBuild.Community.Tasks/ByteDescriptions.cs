namespace MSBuild.Community.Tasks
{

    /// <summary>
    /// Describes certain byte measurements as nice strings.
    /// </summary>
    internal static class ByteDescriptions
    {
        public static string FormatByte(long bytes, int rounding)
        {
            if (bytes >= (1 << 60))
                return System.Math.Round((double)bytes / (1 << 60), rounding) + " EB"; //exabyte
            if (bytes >= (1 << 50))
                return System.Math.Round((double)bytes / (1 << 50), rounding) + " PB"; //petabyte
            if (bytes >= (1 << 40))
                return System.Math.Round((double)bytes / (1 << 40), rounding) + " TB"; //terabyte
            if (bytes >= (1 << 30))
                return System.Math.Round((double)bytes / (1 << 30), rounding) + " GB"; //gigabyte
            if (bytes >= (1 << 20))
                return System.Math.Round((double)bytes / (1 << 20), rounding) + " MB"; //megabyte
            if (bytes >= (1 << 10))
                return System.Math.Round((double)bytes / (1 << 10), rounding) + " KB"; //kilobyte

            return bytes + " Bytes"; //byte
        }

        public static string FormatBytesPerSecond(long bytes, double secounds, int rounding)
        {
            if (secounds < 1.0)
                return string.Empty;

            var bytesPerSecounds = bytes / secounds;

            if (bytesPerSecounds >= (1 << 60))
                return System.Math.Round(bytesPerSecounds / (1 << 60), rounding) + " EB/s"; //exabyte
            if (bytesPerSecounds >= (1 << 50))
                return System.Math.Round(bytesPerSecounds / (1 << 50), rounding) + " PB/s"; //petabyte
            if (bytesPerSecounds >= (1 << 40))
                return System.Math.Round(bytesPerSecounds / (1 << 40), rounding) + " TB/s"; //terabyte
            if (bytesPerSecounds >= (1 << 30))
                return System.Math.Round(bytesPerSecounds / (1 << 30), rounding) + " GB/s"; //gigabyte
            if (bytesPerSecounds >= (1 << 20))
                return System.Math.Round(bytesPerSecounds / (1 << 20), rounding) + " MB/s"; //megabyte

            return System.Math.Round(bytesPerSecounds / (1 << 10), rounding) + " KB/s"; //kilobyte
        }
    }
}
