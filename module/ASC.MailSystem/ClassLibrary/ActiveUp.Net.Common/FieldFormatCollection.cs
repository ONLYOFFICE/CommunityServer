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

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// A collection of field format options.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class FieldFormatCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public FieldFormatCollection()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Add an FieldFormat object in the collection.
        /// </summary>
        /// <param name="fieldFormat">The FieldFormat.</param>
        public void Add(FieldFormat fieldFormat)
        {
            List.Add(fieldFormat);
        }

        /// <summary>
        /// Add an FieldFormat object in the collection based the fiel name and format string.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="format">The format string of the field.</param>
        public void Add(string name, string format)
        {
            List.Add(new FieldFormat(name, format));
        }

        /// <summary>
        /// Add an FieldFormat object based the field name, format string, padding direction, total width and padding char.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="format">The format string of the field.</param>
        /// <param name="paddingDir">The padding direction.</param>
        /// <param name="totalWidth">The total width.</param>
        /// <param name="paddingChar">The padding char.</param>
        public void Add(string name, string format, PaddingDirection paddingDir, int totalWidth, char paddingChar)
        {
            List.Add(new FieldFormat(name, format, paddingDir, totalWidth, paddingChar));
        }
        
        /// <summary>
        /// Remove the FieldFormat object from the collection at the specified index position.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            // Check to see if there is a EmbeddedObject at the supplied index.
            if (index < Count || index >= 0)
            {
                List.RemoveAt(index); 
            }
        }

        /// <summary>
        /// Returns the FieldFormat object at the specified index position in the collection.
        /// </summary>
        public FieldFormat this[int index]
        {
            get
            {
                return (FieldFormat) List[index];
            }
        }

        /// <summary>
        /// Returns the FieldFormat of the specified name.
        /// </summary>
        public FieldFormat this[string name]
        {
            get
            {
                foreach (FieldFormat fieldFormat in List)
                {
                    if (fieldFormat.Name.ToLower() == name.ToLower())
                        return fieldFormat;
                }
                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified field is in the list.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>true if the collection contain the specified field; false otherwise.</returns>
        public bool Contains(string name)
        {
            foreach(FieldFormat fieldFormat in List)
            {
                if (fieldFormat.Name.ToLower() == name.ToLower())
                    return true;
            }

            return false;
        }
    }
}
