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
    /// Represents a server.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Server
    {
        private string _host, _username, _password;
        private int _port;
        private bool _requiresAuthentication;
        private EncryptionType _encType;      


        /// <summary>
        /// The default constructor.
        /// </summary>
        public Server()
        {
            this.Host = "127.0.0.1";
            this.Port = 0;
            this.Username = string.Empty;
            this.Password = string.Empty;
        }

        /// <summary>
        /// Creates the Server object from the host or IP and port number.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public Server(string host, int port)
        {
            this.Host = host;
            this.Port = port;
            this.Username = string.Empty;
            this.Password = string.Empty;
        }

        /// <summary>
        /// Creates the Server object from the host or IP, port number, username and password.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="password"></param>
        /// <param name="username"></param>
        public Server(string host, int port, string username, string password)
        {
            this.Host = host;
            this.Port = port;
            this.Username = username;
            this.Password = password;
        }

        /// <summary>
        /// Allows adding a new server with the added option of specifying the encryption type if any and whether the server needs any authentication.
        /// </summary>
        /// <param name="host">Hostname/IP Address for server.</param>
        /// <param name="port">Port to be used to connect to server.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="password">Password for authentication.</param>
        /// <param name="RequiresAuthentication">true if authentication is needed, false otherwise.</param>
        /// <param name="EncType">Encryption type see EncryptionType enumeration.</param>
        public Server(string host, int port, string username, string password,bool RequiresAuthentication, EncryptionType EncType)
        {
            this.Host = host;
            this.Port = port;
            this.Username = username;
            this.Password = password;
            _requiresAuthentication = RequiresAuthentication;
            _encType = EncType;
            
        }

        /// <summary>
        /// The user name to use for authentication.
        /// </summary>
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
            }
        }

        /// <summary>
        /// The password to use for authentication.
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        /// <summary>
        /// The host or IP of the server.
        /// </summary>
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
        /// The port number to use for the connection.
        /// </summary>
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
        /// Gets or sets the flag is authentication is needed on this server.
        /// </summary>
        public bool RequiresAuthentication
        {
            get { return _requiresAuthentication; }
            set { _requiresAuthentication = value; }
        }

        /// <summary>
        /// Gets or sets the encryption type for the server.
        /// 
        /// </summary>
        public EncryptionType ServerEncryptionType
        {
            get { return _encType; }
            set { _encType = value; }
        }
    }
}