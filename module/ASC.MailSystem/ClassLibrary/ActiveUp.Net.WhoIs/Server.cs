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
using System.Xml.Serialization;

namespace ActiveUp.Net.WhoIs
{

    #region class Server

    /// <summary>
    /// Represents a whois server.
    /// </summary>
    public class Server
    {
        #region Variables

        /// <summary>
        /// The host or IP address of the server.
        /// </summary>
        private string _host;

        /// <summary>
        /// The port number to use for the connection.
        /// </summary>
        private int _port;

        /// <summary>
        /// The domain extention of the server.
        /// </summary>
        private string _domain;

        /// <summary>
        /// String indicates the domain is not found.
        /// </summary>
        private string _noMatch;

        #endregion

        #region Constructor

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Server()
        {
            _Init("",43,"","");
        }

        /// <summary>
        /// Creates a whois Server object from the host or IP address.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        public Server(string host)
        {
            _Init(host,43,"","");
        }

        /// <summary>
        /// Creates a whois Server object from the host or IP address and port number.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="port">The port number to use for the connection.</param>
        public Server(string host, int port)
        {
            _Init(host,port,"","");
        }

        /// <summary>
        /// Creates a whois Server object from the host and the domain extention.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="domain">The domain extention.</param>
        public Server(string host, string domain)
        {
            _Init(host,43,domain,"");
        }

        /// <summary>
        /// Creates a whois Server object from the host or IP address, port number and the domain extention.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="port">The port number to use for the connection.</param>
        /// <param name="domain">The domain extention.</param>
        public Server(string host, int port, string domain)
        {
            _Init(host,port,domain,"");
        }

        /// <summary>
        /// Creates a whois Server object from the host or IP address, the domain extention and the string indicates the domain is not found.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="domain">The domain extention.</param>
        /// <param name="noMatch">String indicates the domain is not found.</param>
        public Server(string host, string domain, string noMatch)
        {
            _Init(host,43,domain,noMatch);
        }

        /// <summary>
        /// Creates a whois Server object from the host or IP address, port number and the string indicates the domain is not found.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="port">The port number to use for the connection.</param>
        /// <param name="domain">The domain extention of the server.</param>
        /// <param name="noMatch">String indicates the domain is not found.</param>
        public Server(string host, int port, string domain, string noMatch)
        {
            _Init(host,port,domain,noMatch);
        }

        /// <summary>
        /// Init the contructor.
        /// </summary>
        /// <param name="host">The host or IP address.</param>
        /// <param name="port">The port number to use for the connection.</param>
        /// <param name="domain">The domain extention of the server.</param>
        /// <param name="noMatch">String indicates the domain is not found.</param>
        private void _Init(string host, int port, string domain, string noMatch)
        {
            _host = host;
            _port = port;
            _domain = domain;
            _noMatch = noMatch;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets / sets the host or IP address.
        /// </summary>
        [XmlElement("host",DataType="string")]
        public string Host
        {
            get
            {
                return _host;
            }

            set
            {
                _host = value;
            }
        }

        /// <summary>
        /// Gets / sets the port used for the connection.
        /// </summary>
        [XmlElement("port",DataType="int")]
        public int Port
        {
            get
            {
                return _port;
            }

            set
            {
                _port = value;
            }
        }

        /// <summary>
        /// Gets / sets the domain extention.
        /// </summary>
        [XmlElement("domain",DataType="string")]
        public string Domain
        {
            get
            {
                return _domain;
            }

            set
            {
                _domain = value;
            }
        }

        /// <summary>
        /// Gets / sets the string indicates the domain is not found.
        /// </summary>
        [XmlElement("nomatch",DataType="string")]
        public string NoMatch
        {
            get
            {
                return _noMatch;
            }

            set
            {
                _noMatch =value;
            }
        }

        #endregion

    }

    #endregion
}
