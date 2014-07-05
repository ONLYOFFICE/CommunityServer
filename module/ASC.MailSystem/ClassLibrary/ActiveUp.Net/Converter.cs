using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net
{
    public class Converter
    {
        public static byte[] ToByteArray(int input)
        {
            byte a = (byte)(input >> 24);
            byte b = (byte)((input & 16711680) >> 16);
            byte c = (byte)((input & 65280) >> 8);
            byte d = (byte)(input & 255);
            byte[] e = { a, b, c, d };
            return e;
        }
        public static ulong ToULong(byte[] input)
        {
            ulong l = (((ulong)input[0]) << 56)
                + (((ulong)input[1]) << 48)
                + (((ulong)input[2]) << 40)
                + (((ulong)input[3]) << 32)
                + (((ulong)input[4]) << 24)
                + (((ulong)input[5]) << 16)
                + (((ulong)input[6]) << 8)
                + ((ulong)input[7]);
                return l;
        }
        public static short ToShort(byte[] input)
        {
            short l = (short)((((short)input[0]) << 8) + (short)input[1]);
            return l;
        }
        public static int ToInt(byte[] input)
        {
            int l = (((int)input[0]) << 24)
                + (((int)input[1]) << 16)
                + (((int)input[2]) << 8)
                + (int)input[3];
            return l;
        }
        public static long ToLong(byte[] input)
        {
            long l = ((long)input[0] << 56)
                | ((long)input[1] << 48)
                | ((long)input[2] << 40)
                | ((long)input[3] << 32)
                | ((long)input[4] << 24)
                | ((long)input[5] << 16)
                | ((long)input[6] << 8)
                | (long)input[7];
            return l;
        }
        public static DateTime UnixTimeStampToDateTime(int timeStamp)
        {
            return new DateTime(1970, 1, 1).AddSeconds(timeStamp);
        }
    }
}
