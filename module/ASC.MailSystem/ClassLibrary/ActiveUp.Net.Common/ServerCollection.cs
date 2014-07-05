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
    /// A collection of Smtp servers.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class ServerCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public ServerCollection()
        {

        }

        /// <summary>
        /// Allows the developer to add a collection of Server objects in another one.
        /// </summary>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <returns>The concacened collection.</returns>
        public static ServerCollection operator +(ServerCollection first, ServerCollection second) 
        {
            ServerCollection newServers = first;
            foreach(ActiveUp.Net.Mail.Server server in second)
                newServers.Add(server);

            return newServers;
        }

        /// <summary>
        /// Add an Server object in the collection.
        /// </summary>
        public void Add(Server server)
        {
            List.Add(server);
        }

        /// <summary>
        /// Add an Server object in the collection specifying the host and port.
        /// </summary>
        /// <param name="host">The hostname or IP address</param>
        /// <param name="port">The port number</param>
        public void Add(string host, int port)
        {
            List.Add(new Server(host, port));
        }

        /// <summary>
        /// Add an Server object in the collection specifying the host and using the default port number (25).
        /// </summary>
        /// <param name="host">The hostname or IP address</param>
        public void Add(string host)
        {
            List.Add(new Server(host, 25));
        }

        /// <summary>
        /// Remove the Server object at the specified index position.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            // Check to see if there is a Server at the supplied index.
            if (index < Count || index >= 0)
            {
                List.RemoveAt(index); 
            }
        }

        /// <summary>
        /// Returns the Server object at the specified index position.
        /// </summary>
        public Server this[int index]
        {
            get
            {
                return (Server) List[index];
            }
        }
    }
}
