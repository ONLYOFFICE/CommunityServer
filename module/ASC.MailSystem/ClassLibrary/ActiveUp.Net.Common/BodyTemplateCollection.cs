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
    public class BodyTemplateCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public BodyTemplateCollection()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Add a templated body in the collection.
        /// </summary>
        public void Add(BodyTemplate bodyTemplate)
        {
            List.Add(bodyTemplate);
        }

        /// <summary>
        /// Add a templated body in the collection based on the specified content.
        /// </summary>
        /// <param name="content">The content to use.</param>
        public void Add(string content)
        {
            List.Add(new BodyTemplate(content));
        }

        /// <summary>
        /// Add a templated body in the collection based on the specified content and body format.
        /// </summary>
        /// <param name="content">The content to use.</param>
        /// <param name="format">The message body format.</param>
        public void Add(string content, BodyFormat format)
        {
            List.Add(new BodyTemplate(content, format));
        }

        /// <summary>
        /// Remove the body template at the specified index position.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            // Check to see if there is a MessageTemplate at the supplied index.
            if (index < Count || index >= 0)
            {
                List.RemoveAt(index); 
            }
        }

        /// <summary>
        /// Returns the body template at the specified index position.
        /// </summary>
        public BodyTemplate this[int index]
        {
            get
            {
                return (BodyTemplate) List[index];
            }
        }
    }
}
