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

namespace ActiveUp.Net.WhoIs
{
    #region class ResultQuery

    /// <summary>
    /// Result of a query from whois server.
    /// </summary>
    public class ResultQuery : Result
    {
        #region Variables
        
        /// <summary>
        /// Result of the whois server.
        /// </summary>
        private string _result;
        
        #endregion

        #region Constructors

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ResultQuery() : base()
        {
            _result = "";
        }

        /// <summary>
        /// Creates a ResultQuery object from result.
        /// </summary>
        /// <param name="result">The result string of the whois server.</param>
        public ResultQuery(string result) : base()
        {
            _result = result;
        }

        /// <summary>
        /// Creates a RestultQuery object from result and Server object.
        /// </summary>
        /// <param name="result">Result string of the whois server.</param>
        /// <param name="server">Whois server used.</param>
        public ResultQuery(string result, Server server) : base(server)
        {
            _result = result;
        }

        /// <summary>
        /// Creates a RestultQuery object from result, Server object and Exception object.
        /// </summary>
        /// <param name="result">Result string of the whois server.</param>
        /// <param name="server">Whois server used.</param>
        /// <param name="exception">Exception if an error occurs.</param>
        public ResultQuery(string result, Server server, Exception exception) : base(server,exception)
        {
            _result = result;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets / sets the result string of the whois server.
        /// </summary>
        public string Result
        {
            get
            {
                return _result;
            }

            set
            {
                _result = value;
            }
        }

        #endregion
    }

    #endregion
}
