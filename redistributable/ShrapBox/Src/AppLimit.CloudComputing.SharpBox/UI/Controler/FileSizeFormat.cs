using System;

namespace AppLimit.CloudComputing.SharpBox.UI
{
    /// <summary>
    /// Class to generate file size formatted strings
    /// </summary>
    public class FileSizeFormat
    {
        /// <summary>
        /// Size unit
        /// </summary>
        public enum FileSizeUnit : long
        {
            /// <summary>
            /// Byte unit
            /// </summary>
            B = 1,
            /// <summary>
            /// Kilo bytes unit
            /// </summary>
            KB = 1024,
            /// <summary>
            /// Mega bytes unit
            /// </summary>
            MB = 1048576,
            /// <summary>
            /// giga bytes unit
            /// </summary>
            GB = 1073741824,
            /// <summary>
            /// Tera byes unit
            /// </summary>
            TB = 1099511627776
        }

        /// <summary>
        /// Automatically format a size
        /// </summary>
        /// <param name="size">The size</param>
        /// <returns></returns>
        public static string Format(decimal size)
        {
            // return "Unknow" resource string if < 0
            if (size < 0)
                return "N/A";

            FileSizeUnit unit = FileSizeUnit.TB; // must be last item of FileSizeUnit enum
            for (decimal UnitSize = 1024; UnitSize <= (long)FileSizeUnit.TB; UnitSize *= 1024)
            {
                if (size < UnitSize)
                {
                    unit = (FileSizeUnit)(long)(UnitSize / 1024);
                    break;
                }
            }
            return FormatToUnit(size, unit);
        }

        /// <summary>
        /// Format a size to a specific unit
        /// </summary>
        /// <param name="size">The size</param>
        /// <param name="unit">The unit</param>
        /// <returns></returns>
        public static string FormatToUnit(decimal size, FileSizeUnit unit)
        {
            decimal sizeConverted = ConvertToUnit(size, unit);
            decimal sizeRounded = 0;
            if (unit == FileSizeUnit.KB)
                sizeRounded = Math.Round(sizeConverted, 0);
            else
                sizeRounded = Math.Round(sizeConverted, 2);
            return String.Format("{0} {1}", sizeRounded, unit);
        }

        /// <summary>
        /// Convert bytes to unit (KB or MB...)
        /// </summary>
        /// <param name="size">The size</param>
        /// <param name="unit">The unit</param>
        /// <returns></returns>
        public static decimal ConvertToUnit(decimal size, FileSizeUnit unit)
        {
            return size / (decimal)unit;
        }
    }
}
