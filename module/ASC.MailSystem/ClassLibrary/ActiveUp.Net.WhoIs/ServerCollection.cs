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
    #region classe ServerCollection

    /// <summary>
    /// A collection of whois servers.
    /// </summary>
    public class ServerCollection : CollectionBase
    {
        #region Constructor

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ServerCollection()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Server object at the specified index position.
        /// </summary>
        public Server this[int index]
        {
            get
            {
                return (Server) this.List[index];
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Add an Server object in the collection specifying the Server object.
        /// </summary>
        /// <param name="server">Server object to add.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(Server server)
        {
            return this.List.Add(server);
        }

        /// <summary>
        /// Add an Server object in the collection specifying the host and port.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="port">The port number to use for the connection.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(string host, int port)
        {
            return this.List.Add(new Server(host,port));
        }

        /// <summary>
        /// Add an Server object in the collection specifying the host and domain.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="domain">The domain extention of the server.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(string host, string domain)
        {
            return this.List.Add(new Server(host,domain));
        }

        /// <summary>
        /// Add an Server object in the collection specifying the host, port and domain.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="port">The port number to use for the connection.</param>
        /// <param name="domain">The domain extention of the server.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(string host, int port, string domain)
        {
            return this.List.Add(new Server(host,port,domain));
        }

        /// <summary>
        /// Add an Server object in the collection specifying the host, port, domain and no match string.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="port">The port number to use for the connection.</param>
        /// <param name="domain">The domain extention of the server.</param>
        /// <param name="noMatch">String indicates the domain is not found.</param>
        /// <returns>Index of the list where the object has been added.</returns>
        public int Add(string host, int port, string domain, string noMatch)
        {
            return this.List.Add(new Server(host,port,domain,noMatch));
        }

        #endregion
    }

    #endregion
}
