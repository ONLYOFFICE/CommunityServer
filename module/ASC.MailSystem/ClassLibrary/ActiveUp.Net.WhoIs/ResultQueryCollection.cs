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
    #region class ResultQueryCollection
    
    /// <summary>
    /// Collection of ResultQuery object.
    /// </summary>
    public class ResultQueryCollection : CollectionBase
    {
        #region Constructors

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ResultQueryCollection()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ResultQuery object at the specified index position.
        /// </summary>
        public ResultQuery this[int index]
        {
            get
            {
                return (ResultQuery) this.List[index];
            }
        }
    
        #endregion

        #region Functions

        /// <summary>
        /// Add a ResultQuery object in the collection specifying the ResultQuery object.
        /// </summary>
        /// <param name="resultQuery">ResultQuery object to add.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(ResultQuery resultQuery)
        {
            return this.List.Add(resultQuery);
        }

        /// <summary>
        /// Add a ResultQuery object in the collection specifying result string.
        /// </summary>
        /// <param name="result">Result string of the whois server.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(string result)
        {
            return this.List.Add(new ResultQuery(result));
        }

        /// <summary>
        /// Add a ResultQuery object in the collection specifying result string and the whois server used.
        /// </summary>
        /// <param name="result">Result string of the whois server.</param>
        /// <param name="serverUsed">Whois server used.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(string result, Server serverUsed)
        {
            return this.List.Add(new ResultQuery(result, serverUsed));
        }

        /// <summary>
        /// Add a ResultQuery object in the collection specifying result string, the whois server used and the exception.
        /// </summary>
        /// <param name="result">Result string of the whois server.</param>
        /// <param name="serverUsed">Whois server used.</param>
        /// <param name="exception">Exception if an error occurs.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(string result, Server serverUsed, Exception exception)
        {
            return this.List.Add(new ResultQuery(result, serverUsed, exception));
        }

        #endregion
    }

    #endregion
}
