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

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Contains Mx Records.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    //[Obsolete("Please use ActiveUp.Net.Dns.MXRecord instead")]
    public class MxRecordCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public MxRecordCollection()
        {
            //
        }

        /// <summary>
        /// Add a MxRecord object in the collection.
        /// </summary>
        /// <param name="mxRecord">The MxRecord object.</param>
        public void Add(ActiveUp.Net.Mail.MxRecord mxRecord)
        {
            List.Add(mxRecord);
        }

        /// <summary>
        /// Add a MxRecord in the collection specifing it's exchange name and preference level.
        /// </summary>
        /// <param name="exchange">The exchange name.</param>
        /// <param name="preference">The preference level.</param>
        public void Add(string exchange, int preference)
        {
            List.Add(new MxRecord(exchange, preference));
        }

        /// <summary>
        /// Remove the Mx Record at the specified index position.
        /// </summary>
        /// <param name="index">The index position.</param>
        public void Remove(int index)
        {
            // Check to see if there is a MxRecord at the supplied index.
            if (index < Count || index >= 0)
            {
                List.RemoveAt(index); 
            }
        }

        /// <summary>
        /// Returns the MxRecord at the specified index position.
        /// </summary>
        public MxRecord this[int index]
        {
            get
            {
                return (MxRecord) List[index];
            }
        }

        /// <summary>
        /// Returns the prefered MX record in the list.
        /// </summary>
        /// <returns>The prefered MX record.</returns>
        public MxRecord GetPrefered()
        {
            int index, minIndex = 0;

            for(index=0;index<List.Count;index++)
            {
                if (minIndex == -1 || minIndex > this[index].Preference)
                    minIndex = index;
            }

            if (minIndex < this.Count && minIndex != -1)
                return this[minIndex];
            else
                return null;
        }

    }
}
