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

namespace ActiveUp.Net.Dns
{
    /// <summary>
    /// Base class for all Record types that caontain RDATA consisting of a single text string
    /// </summary>
    public class TextOnly : IRecordData
    {
        /// <summary>
        /// default Text Constructor
        /// </summary>
        public TextOnly() { text = new List<string>(); }
        /// <summary>
        /// create a text array from record bytes
        /// </summary>
        /// <param name="buffer"> buffer of bytes</param>
        public TextOnly(DataBuffer buffer) 
        {
            text = new List<string>();
            while(buffer.Next > 0)
                text.Add(buffer.ReadCharString()); 
        }
        /// <summary>
        /// create a text array from record bytes
        /// </summary>
        /// <param name="buffer"> buffer of bytes </param>
        /// <param name="length"> length of bytes to use</param>
        public TextOnly(DataBuffer buffer, int length)
        {
            int len = length;
            int pos = buffer.Position;
            text = new List<string>();
            byte next = buffer.Next;
            while (length > 0)
            {
                text.Add(buffer.ReadCharString());
                length -= next + 1;
                if (length < 0)
                {
                    buffer.Position = pos - len;  //Reset current Pointer of Buffer to end of this record
                    throw new DnsQueryException("Buffer Over Run in TextOnly Record Data Type", null);
                }
                next = buffer.Next;
            }
            if (length > 0)
            {
                buffer.Position = pos - len;  //Reset current Pointer of Buffer to end of this record
                throw new DnsQueryException("Buffer Under Run in TextOnly Record Data Type", null);
            }
        }
        private List<string> text;
        /// <summary>
        /// text of record
        /// </summary>
        protected string Text
        {
            get 
            {
                string res = String.Empty;                
                foreach (string s in text)
                    res += s + "\n"; 
                return res;
            }
        }
        /// <summary>
        /// return number of strings in text record
        /// </summary>
        protected int Count             { get { return text.Count; } }
        /// <summary>
        /// return list of strings in record
        /// </summary>
        protected List<string> Strings  { get { return text; } }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Text: " + Text;
        }
    }
}
