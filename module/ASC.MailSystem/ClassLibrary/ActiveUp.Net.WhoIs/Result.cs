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
    #region class Result

    /// <summary>
    /// Base class for the results in case of global query.
    /// </summary>
    public class Result
    {
        #region Variables

        /// <summary>
        /// Whois server used.
        /// </summary>
        private Server _server;

        /// <summary>
        /// Exception if an error occurs, otherwise null.
        /// </summary>
        private Exception _exception;

        #endregion

        #region Constructor

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Result()
        {
            _Init(new Server(),null);
        }

        /// <summary>
        /// Creates a Result object from Server object.
        /// </summary>
        /// <param name="server">Whois server used.</param>
        public Result(Server server)
        {
            _Init(server,null);
        }

        /// <summary>
        /// Creates a Restult object from Server object and Exception object.
        /// </summary>
        /// <param name="server">Whois server used.</param>
        /// <param name="exception">Exception if an error occurs.</param>
        public Result(Server server, Exception exception)
        {
            _Init(server,exception);
        }

        /// <summary>
        /// Creates a Result object from Server object and Exception object.
        /// </summary>
        /// <param name="server">Whois server used.</param>
        /// <param name="exception">Exception if an error occurs.</param>
        private void _Init(Server server, Exception exception)
        {
            _server = server;
            _exception = exception;
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets / sets the whois server used.
        /// </summary>
        public Server ServerUsed
        {
            get
            {
                return _server;
            }

            set
            {
                _server = value;
            }
        }

        /// <summary>
        /// Gets / sets the exceptions if an error occurs, otherwise null.
        /// </summary>
        public Exception Error
        {
            get
            {
                return _exception;
            }

            set
            {
                _exception = value;
            }
        }

        #endregion
    }

    #endregion
}
