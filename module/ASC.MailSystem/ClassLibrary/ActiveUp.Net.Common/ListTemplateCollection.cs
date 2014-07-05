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
    /// A collection of templated bodies.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class ListTemplateCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public ListTemplateCollection()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Add a templated list in the collection.
        /// </summary>
        public void Add(ListTemplate listTemplate)
        {
            List.Add(listTemplate);
        }

        /// <summary>
        /// Add a templated list in the collection based on the specified content.
        /// </summary>
        /// <param name="name">The name to use.</param>
        /// <param name="content">The content to use.</param>
        public void Add(string name, string content)
        {
            List.Add(new ListTemplate(name, content));
        }

        /// <summary>
        /// Remove the body template at the specified index position.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            // Check to see if there is list at the supplied index.
            if (index < Count || index >= 0)
            {
                List.RemoveAt(index); 
            }
        }

        /// <summary>
        /// Returns the body template at the specified index position.
        /// </summary>
        public ListTemplate this[int index]
        {
            get
            {
                return (ListTemplate) List[index];
            }
        }
    }
}
