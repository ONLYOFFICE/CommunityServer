// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Mail
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
