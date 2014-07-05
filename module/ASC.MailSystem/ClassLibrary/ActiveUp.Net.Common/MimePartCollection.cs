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

using ActiveUp.Net.Mail;

namespace ActiveUp.Net.Mail 
{
#region MimePartCollection Object
    /// <summary>
    /// Represents a collection of MimePart objects.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class MimePartCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Adds the MimePart object to the collection.
        /// </summary>
        /// <param name="part">The MimePart to be added.</param>
        public void Add(MimePart part)
        {
            this.List.Add(part);
        }
        public void Add(string path, bool generateContentId)
        {
            this.List.Add(new MimePart(path,generateContentId));
        }
        public void Add(string path, bool generateContentId, string charset)
        {
            this.List.Add(new MimePart(path,generateContentId,charset));
        }
        /// <summary>
        /// Indexer.
        /// </summary>
        public MimePart this[int index]
        {
            get
            {
                return (MimePart)this.List[index];
            }
        }

        /// <summary>
        /// Indexer. Returns the first object containing the specified filename.
        /// </summary>
        public MimePart this[string filename]
        {
            get
            {
                foreach(MimePart part in this.List)
                    if (part.ContentDisposition.FileName == filename)
                        return part;
                return null;
            }
        }
        internal MimePartCollection ConcatMessagesAsPart(MessageCollection input)
        {
            MimePartCollection output = new MimePartCollection();
            foreach(MimePart part in this) output.Add(part);
            foreach(Message message in input) output.Add(message.ToMimePart());
            return output;            
        }
        /// <summary>
        /// Allows the developer to add a collection of MimePart objects in another one.
        /// </summary>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <returns>The concatened collection.</returns>
        public static MimePartCollection operator +(MimePartCollection first, MimePartCollection second) 
        {
            MimePartCollection newParts = first;
            foreach(MimePart part in second)
                newParts.Add(part);

            return newParts;
        }

        /// <summary>
        /// Check if the collection contain the specified filename.
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <returns>True if the collection contains the file; False otherwise.</returns>
        public bool Contains(string filename)
        {
            filename = System.IO.Path.GetFileName(filename);

            foreach(MimePart part in this.List)
            {
                if (part.ContentDisposition.FileName == filename)
                    return true;
            }

            return false;
        }
    }
    #endregion
}