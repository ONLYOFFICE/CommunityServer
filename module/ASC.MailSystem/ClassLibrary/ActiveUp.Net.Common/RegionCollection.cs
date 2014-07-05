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
    public class RegionCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public RegionCollection()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Add an Region object in the collection.
        /// </summary>
        /// <param name="region">The Region.</param>
        public void Add(Region region)
        {
            List.Add(region);
        }

        /// <summary>
        /// Add an Condition object in the collection based the region id, field, and value.
        /// </summary>
        /// <param name="regionid">The id of the region.</param>
        /// <param name="url">The url to retrieve.</param>
        public void Add(string regionid, string url)
        {
            List.Add(new Region(regionid, url));
        }
        
        /// <summary>
        /// Remove the Condition object from the collection at the specified index position.
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
        /// Returns the Region object at the specified index position in the collection.
        /// </summary>
        public Region this[int index]
        {
            get
            {
                return (Region) List[index];
            }
        }

        /// <summary>
        /// Returns the Region of the specified Region ID.
        /// </summary>
        public Region this[string regionID]
        {
            get
            {
                foreach (Region region in List)
                {
                    if (region.RegionID == regionID)
                        return region;
                }
                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified regionID is in the list.
        /// </summary>
        /// <param name="regionID">The regionID of the Region.</param>
        /// <returns>true if the collection contain the specified regionID; false otherwise.</returns>
        public bool Contains(string regionID)
        {
            foreach(Region region in List)
            {
                if (region.RegionID == regionID)
                    return true;
            }

            return false;
        }
    }
}
