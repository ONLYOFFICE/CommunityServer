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
using System.Collections;

namespace ActiveUp.Net.WhoIs
{
    #region class ResultIsAvailableCollection

    /// <summary>
    /// Collection of ResultIsAvailble object.
    /// </summary>
    public class ResultIsAvailableCollection : CollectionBase
    {
        #region Constructors

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ResultIsAvailableCollection()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ResultIsAvailable object at the specified index position.
        /// </summary>
        public ResultIsAvailable this[int index]
        {
            get
            {
                return (ResultIsAvailable) this.List[index];
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Add a ResultIsAvailable object in the collection specifying the ResultIsAvailable object.
        /// </summary>
        /// <param name="resultIsAvailable">ResultIsAvailable object to add.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(ResultIsAvailable resultIsAvailable)
        {
            return this.List.Add(resultIsAvailable);
        }

        /// <summary>
        /// Add a ResultIsAvailable object in the collection specifying isAvailable flag.
        /// </summary>
        /// <param name="isAvailable">Indicates if a domain is available for registration or not.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(bool isAvailable)
        {
            return this.List.Add(new ResultIsAvailable(isAvailable));
        }

        /// <summary>
        /// Add a ResultIsAvailable object in the collection specifying isAvailable flag and the whois server used.
        /// </summary>
        /// <param name="isAvailable">Indicates if a domain is available for registration or not.</param>
        /// <param name="serverUsed">Whois server used.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(bool isAvailable, Server serverUsed)
        {
            return this.List.Add(new ResultIsAvailable(isAvailable,serverUsed));
        }

        /// <summary>
        /// Add a ResultIsAvailable object in the collection specifying isAvailable flag, the whois server used and the exception.
        /// </summary>
        /// <param name="isAvailable">Indicates if a domain is available for registration or not.</param>
        /// <param name="serverUsed">Whois server used.</param>
        /// <param name="exception">Exception if an error occurs.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(bool isAvailable, Server serverUsed, Exception exception)
        {
            return this.List.Add(new ResultIsAvailable(isAvailable,serverUsed,exception));
        }

        #endregion
    }

    #endregion
}
