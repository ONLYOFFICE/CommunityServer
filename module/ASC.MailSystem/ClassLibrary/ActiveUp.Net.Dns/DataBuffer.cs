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
using System.Net;

namespace ActiveUp.Net.Dns
{
    public class DataBuffer
    {
        /// <summary>
        /// Create Data Buffer from byte array and set the ptr to the first byte
        /// </summary>
        /// <param name="data"></param>
        public DataBuffer(Byte[] data) : this(data, 0) { }
        /// <summary>
        /// Create a Data Buffer froma byte array and set the pos pointer to pos
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        public DataBuffer(Byte[] data, int pos)
        {
            this.data = data;
            this.pos = pos;
        }

        /// <summary>
        /// Peek at next byte
        /// </summary>
        public byte Next                        { get { return data[pos]; } }
        /// <summary>
        /// Read the next byte and advance the pointer
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()                    { return data[pos++]; }
        /// <summary>
        /// Read a short int from the buffer
        /// </summary>
        /// <returns></returns>
        public short ReadShortInt()             { return (short)(ReadByte() | ReadByte() << 8); }        
        /// <summary>
        /// Read a short int from the buffer
        ///Big Endian version of ReadShortInt 
        /// </summary>
        /// <returns></returns>
        public short ReadBEShortInt()           { return (short)(ReadByte() << 8 | ReadByte() ); }
        /// <summary>
        /// Read Unsigned Short Int
        /// </summary>
        /// <returns></returns>
        public ushort ReadShortUInt()           { return (ushort)(ReadByte() | ReadByte() << 8); }
        /// <summary>
        /// Read Unsigned Short Int
        /// BigEndian version of ReadShortUInt 
        /// </summary>
        /// <returns></returns>   
        public ushort ReadBEShortUInt()         { return (ushort)(ReadByte() << 8 | ReadByte()); }
        /// <summary>
        /// Read 32 bit Integer from buffer
        /// </summary>
        /// <returns></returns>
        public int ReadInt()                    { return (int)(ReadBEShortUInt() << 16 | ReadBEShortUInt() ); }
        /// <summary>
        /// Read unsigned 32 bit int from buffer
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt()                  { return (uint)(ReadBEShortUInt() << 16| ReadBEShortUInt() ); }
        /// <summary>
        /// Read long int 64 bit from buffer
        /// </summary>
        /// <returns></returns>
        public long ReadLongInt()               { return ReadInt() | ReadInt() << 32; }
       
        /// <summary>
        /// Reads a domain name from the byte array. Reference to RFC1035 - 4.1.4. 
        /// To minimise the size of the message, if part of a domain
        /// name has already been seen in the message, a pointer to the existing definition is used. 
        /// Each word in a domain name is a label, and is preceded by its length the series is 
        /// terminated with a null
        ///         
        /// </summary>
        /// <returns>string containing Domain Name</returns>
        public string ReadDomainName() { return ReadDomainName(1); }
        public string ReadDomainName(int depth)
        {
            //if (depth > 3) return String.Empty;
            StringBuilder domain = new StringBuilder();
            int length = 0;
            //read in each labels length and chars iuntil there is no more
            length = ReadByte();
            while (length != 0)
            {
                //Is this name conpressed?
                if ((length & 0xc0) == 0xc0)
                {   //Yes it is
                    //calculate address of reference label
                    int posReference = ((length & 0x3f) << 8 | ReadByte());
                    int oldPosition = pos;
                    pos = posReference;
                    domain.Append( ReadDomainName(depth + 1));  
                    pos = oldPosition;
                    //length = ReadByte();
                    return domain.ToString();
                }
                else
                {   //No it isn't read the label
                    for (int i = 0; i < length; i++)
                    {
                        domain.Append((char)ReadByte());
                    }
                }
                if (Next != 0) //Not the end of the domain name get the next segment
                    domain.Append('.');
                length = ReadByte();
            }
            return domain.ToString();
        }
        /// <summary>
        /// Read IP Addres from data buffer
        /// </summary>
        /// <returns></returns>
        public IPAddress ReadIPAddress()
        {
            Byte[] address = new Byte[4];
            for (int i = 0; i < 4; i++)
                address[i] = ReadByte();
            return new IPAddress(address);
        }
        /// <summary>
        /// Read IPv6 from Data Buffer
        /// </summary>
        /// <returns></returns>
        public IPAddress ReadIPv6Address()
        {
            Byte[] address = new Byte[16];
            for (int i = 0; i < 16; i++)
                address[i] = ReadByte();
            return new IPAddress(address);
        }
        /// <summary>
        /// Read a bytre array of length from Data Buffer
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Byte[] ReadBytes(int length)
        {
            Byte[] res = new Byte[length];
            for (int i = 0; i < length; i++)
                res[i] = ReadByte();
            return res;
        }
        /// <summary>
        /// Read a short from the buffer and then read a string of bytes of that length 
        /// and interpret them as a character string
        /// </summary>
        /// <returns></returns>
        public string ReadCharString()
        {
            int length = ReadByte();
            StringBuilder txt = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                txt.Append((char)ReadByte());
            }
            return txt.ToString();
        }

        Byte[] data;
        int pos = 0;
        /// <summary>
        /// Get current position being read in the buffer
        /// </summary>
        public int Position
        {
            get { return pos; }
            set { pos = value; }
        }
    }
}
