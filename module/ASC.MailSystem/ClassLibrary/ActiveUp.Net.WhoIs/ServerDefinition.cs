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
using System.Xml.Serialization;

namespace ActiveUp.Net.WhoIs
{
    #region class ServerDefinition

    /// <summary>
    /// Represents a collection of servers used for serialize and deserialize xml file contains the list of whois servers.
    /// </summary>
    [XmlRootAttribute("serverdefinition", IsNullable=false)]
    public class ServerDefinition
    {
        #region Variables

        /// <summary>
        /// List of whois servers.
        /// </summary>
        private ArrayList _servers = new ArrayList();

        #endregion

        #region Constructor

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ServerDefinition()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets / sets the list of whois servers.
        /// </summary>
        [XmlArray("servers")]
        [XmlArrayItem("server",typeof(Server))]
        public ArrayList Servers
        {
            get
            {
                return _servers;
            }

            set
            {
                _servers = value;
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Override the operator =. Affects a ServerDefinition object to a ServerCollection object.
        /// </summary>
        /// <param name="serverDefinition">ServerDefinition object contains all the whois servers</param>
        /// <returns>ServerCollection object contains all the whois servers</returns>
        public static implicit operator ServerCollection(ServerDefinition serverDefinition)
        {
            ServerCollection serverCollection = new ServerCollection();
            
            foreach(Server server in serverDefinition.Servers)
            {
                serverCollection.Add(server);
            }

            return serverCollection;
        }

        #endregion
    }

    #endregion
}
