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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Reflection;

namespace ActiveUp.Net.WhoIs
{
    #region class WhoIs

    /// <summary>
    /// Allow to query a whois server.
    /// </summary>
    public class WhoIs
    {
        #region Delegates
        
        private delegate string QueryDelegate(ServerCollection servers, string domainToQuery, bool checkNoMatch);
        private delegate ResultQueryCollection GlobalQueryDelegate(ServerCollection servers, string domainToQuery);
        private delegate bool IsAvailableDelegate(ServerCollection servers, string domainToQuery);
        private delegate ResultIsAvailableCollection GlobalIsAvailableDelegate(ServerCollection servers, string domainToQuery);
        
        #endregion

        #region Variables

        /// <summary>
        /// List of whois servers used in case of no server(s) is specified.
        /// </summary>
        private ServerCollection _servers = new ServerCollection();

        /// <summary>
        /// Separator between no match and the query result.
        /// </summary>
        private static string _sepNoMatchQuery = "@";

        /// <summary>
        /// Delegate using in case of asynchronous call of query.
        /// </summary>
        private QueryDelegate _queryDelegate;

        /// <summary>
        /// Delegate using in case of asynchronous call of global query.
        /// </summary>
        private GlobalQueryDelegate _globalQueryDelegate;

        /// <summary>
        /// Delegate using in case of asynchronous call of is available (domain).
        /// </summary>
        private IsAvailableDelegate _isAvailableDelegate;

        /// <summary>
        /// Delegate using in case of asynchronous call of global is available (domain).
        /// </summary>
        private GlobalIsAvailableDelegate _globalIsAvailableDelegate;

        #endregion

        #region Constructors

        /// <summary>
        /// The default constructor. Load the server definition from the resources.
        /// </summary>
        public WhoIs()
        {
            this.LoadServerDefinitionFromResources();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets / sets the default whois servers collection.
        /// </summary>
        public ServerCollection Servers
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

        #region Functions

        /// <summary>
        /// Load a list of whois servers definition from the resource file.
        /// </summary>
        public void LoadServerDefinitionFromResources()
        {
#if !PocketPC
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream stream = asm.GetManifestResourceStream("ActiveUp.Net.WhoIs.ServerDefinition.xml");

            TextReader reader = new StreamReader(stream);
            XmlSerializer serialize = new XmlSerializer(typeof(ServerDefinition));
            ServerDefinition serverDef = (ServerDefinition)serialize.Deserialize(reader);

            reader.Close();

            _servers = serverDef;
#endif

        }

        /// <summary>
        /// Load a list of whois servers definition from an external xml file.
        /// </summary>
        /// <param name="path">Path where the external xml file is located.</param>
        public void LoadServerDefinitionFromXml(string path)
        {
            if (File.Exists(path) == false)
                throw new FileNotFoundException();

            TextReader reader = new StreamReader(path);
            XmlSerializer serialize = new XmlSerializer(typeof(ServerDefinition));
            ServerDefinition serverDef = (ServerDefinition)serialize.Deserialize(reader);

            reader.Close();

            _servers = serverDef;
        }

        /// <summary>
        /// Query a whois server specifying the specified domain to query. In this case, it uses the default whois servers.
        /// </summary>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>Result of the whois server.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        WhoIs whoIs = new WhoIs();
        ///        string result = whoIs.Query("activeup.com");
        ///        Console.WriteLine(result);
        ///    }
        /// 
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        /// 
        /// [VB.NET] 
        /// 
        /// Try
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As String = whoIs.Query("activeup.com")
        ///        Console.WriteLine(result)
        ///
        ///    Catch we as WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///        
        ///    End Try
        /// </code>
        /// </example>
        public string Query(string domainToQuery)
        {
            return _Query(null,domainToQuery,false);
        }
    
        /// <summary>
        /// Query a whois server specifying the specified domain to query asynchronously. In this case, it uses the default whois servers.
        /// </summary>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function.</param>
        /// <returns>IAsyncResult object that represents the result of the QueryAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync("activeup.com",new AsyncCallback(MyCallbackWhoIs));
        /// 
        /// public void MyCallbackWhoIs(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            string result = whoIs.QueryAsyncResult(state);
        ///            Console.WriteLine(result);
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// Dim state As IAsyncResult = whoIs.QueryAsync("activeup.com", New AsyncCallback(AddressOf MyCallbackWhoIs))
        /// 
        /// Public Sub MyCallbackWhoIs(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///
        ///            Dim result As String = whoIs.QueryAsyncResult(state)
        ///            Console.WriteLine(result)
        ///
        ///        Catch we As WhoisException
        ///             Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult QueryAsync(string domainToQuery,AsyncCallback callBack)
        {
            _queryDelegate = new QueryDelegate(_Query);
            return _queryDelegate.BeginInvoke(null,domainToQuery,false,callBack,null);
        }

        /// <summary>
        /// Query a whois server specifying the specified host and domain to query.
        /// </summary>
        /// <param name="host">Host of the whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>Result of the whois query.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        WhoIs whoIs = new WhoIs();
        ///        string result = whoIs.Query("whois.networksolutions.com","activeup.com");
        ///        Console.WriteLine(result);
        /// }
        /// 
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///    
        /// [VB.NET]
        /// 
        /// Try
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As String = whoIs.Query("whois.networksolutions.com","activeup.com")
        ///        Console.WriteLine(result)
        ///
        ///    Catch we as WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///        
        ///    End Try
        /// </code>
        /// </example>
        public string Query(string host, string domainToQuery)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(new Server(host));

            return _Query(servers,domainToQuery,false);
        }

        /// <summary>
        /// Query a whois server specifying the specified host and domain to query asynchronously.
        /// </summary>
        /// <param name="host">Host of the whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function</param>
        /// <returns>IAsyncResult object that represents the result of the QueryAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync("whois.networksolutions.com","activeup.com",new AsyncCallback(MyCallbackWhoIs));
        /// 
        /// public void MyCallbackWhoIs(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            string result = whoIs.QueryAsyncResult(state);
        ///            Console.WriteLine(result);
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        ///     
        /// Private whoIs As whoIs = New whoIs()
        /// Dim state As IAsyncResult = whoIs.QueryAsync("whois.networksolutions.com","activeup.com", New AsyncCallback(AddressOf MyCallbackWhoIs))
        /// 
        /// Public Sub MyCallbackWhoIs(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///
        ///            Dim result As String = whoIs.QueryAsyncResult(state)
        ///            Console.WriteLine(result)
        ///
        ///        Catch we As WhoisException
        ///             Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult QueryAsync(string host, string domainToQuery,AsyncCallback callBack)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(new Server(host));

            _queryDelegate = new QueryDelegate(_Query);
            return _queryDelegate.BeginInvoke(servers,domainToQuery,false,callBack,null);
        }

        /// <summary>
        /// Query a whois server specifying the specified host, port and domain to query.
        /// </summary>
        /// <param name="host">Host of the whois server.</param>
        /// <param name="port">Port of the whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>Result of the whois server.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        WhoIs whoIs = new WhoIs();
        ///        string result = whoIs.Query("whois.networksolutions.com",43,"activeup.com");
        ///        Console.WriteLine(result);
        ///    }
        /// 
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///    
        /// [VB.NET]
        /// 
        /// Try
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As String = whoIs.Query("whois.networksolutions.com",43,"activeup.com")
        ///        Console.WriteLine(result)
        ///
        ///    Catch we as WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///        
        ///    End Try
        /// </code>
        /// </example>
        public string Query(string host, int port, string domainToQuery)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(new Server(host,port));

            return _Query(servers,domainToQuery,false);
        }

        /// <summary>
        /// Query a whois server specifying the specified host, port and domain to query asynchronously.
        /// </summary>
        /// <param name="host">Host of the whois server.</param>
        /// <param name="port">Port of the whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function.</param>
        /// <returns>IAsyncResult object that represents the result of the QueryAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync("whois.networksolutions.com",43,"activeup.com",new AsyncCallback(MyCallbackWhoIs));
        /// 
        /// public void MyCallbackWhoIs(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            string result = whoIs.QueryAsyncResult(state);
        ///            Console.WriteLine(result);
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// Dim state As IAsyncResult = whoIs.QueryAsync("whois.networksolutions.com",43,"activeup.com", New AsyncCallback(AddressOf MyCallbackWhoIs))
        /// 
        /// Public Sub MyCallbackWhoIs(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///
        ///            Dim result As String = whoIs.QueryAsyncResult(state)
        ///            Console.WriteLine(result)
        ///
        ///        Catch we As WhoisException
        ///             Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult QueryAsync(string host, int port, string domainToQuery,AsyncCallback callBack)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(new Server(host,port));

            _queryDelegate = new QueryDelegate(_Query);
            return _queryDelegate.BeginInvoke(servers,domainToQuery,false,callBack,null);
        }

        /// <summary>
        /// Query a whois server specifying the specified Server object and domain to query.
        /// </summary>
        /// <param name="server">Whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>Result of the whois server.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        Server server = new Server();
        ///        server.Host = "whois.networksolutions.com";
        ///        server.Port = 43;
        ///        server.Domain = ".com";
        /// 
        ///        WhoIs whoIs = new WhoIs();
        ///        string result = whoIs.Query(server,"activeup.com");
        ///        Console.WriteLine(result);
        ///    }
        /// 
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///    
        /// [VB.NET]
        /// 
        /// Try
        ///
        ///        Dim server As New Server()
        ///        server.Host = "whois.networksolutions.com"
        ///        server.Port = 43
        ///        server.Domain = ".com"
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As String = whoIs.Query("whois.networksolutions.com",43,"activeup.com")
        ///        Console.WriteLine(result)
        ///
        ///    Catch we as WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///        
        ///    End Try
        /// </code>
        /// </example>
        public string Query(Server server, string domainToQuery)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(server);

            return _Query(servers,domainToQuery,false);
        }

        /// <summary>
        /// Query a whois server specifying the specified Server object and domain to query asynchronously.
        /// </summary>
        /// <param name="server">Whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function.</param>
        /// <returns>IAsyncResult object that represents the result of the QueryAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();    
        /// 
        /// Server server = new Server();
        /// server.Host = "whois.networksolutions.com";
        /// server.Port = 43;
        /// server.Domain = ".com";
        /// 
        /// IAsyncResult state = whoIs.QueryAsync(server,"activeup.com",new AsyncCallback(MyCallbackWhoIs));
        /// 
        /// public void MyCallbackWhoIs(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            string result = whoIs.QueryAsyncResult(state);
        ///            Console.WriteLine(result);
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// 
        ///    Dim server As New Server()
        ///    server.Host = "whois.networksolutions.com"
        ///    server.Port = 43
        ///    server.Domain = ".com"
        /// Dim state As IAsyncResult = whoIs.QueryAsync(server, New AsyncCallback(AddressOf MyCallbackWhoIs))
        /// 
        /// Public Sub MyCallbackWhoIs(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///
        ///            Dim result As String = whoIs.QueryAsyncResult(state)
        ///            Console.WriteLine(result)
        ///
        ///        Catch we As WhoisException
        ///             Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example> 
        public IAsyncResult QueryAsync(Server server, string domainToQuery,AsyncCallback callBack)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(server);

            _queryDelegate = new QueryDelegate(_Query);
            return _queryDelegate.BeginInvoke(servers,domainToQuery,false,callBack,null);
        }

        /// <summary>
        /// Query a whois server specifying the specified ServerCollection object and domain to query.
        /// </summary>
        /// <param name="servers">Collection contening a list of whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>Result of the whois server.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        ServerCollection servers = new ServerCollection();
        /// 
        ///        Server server1 = new Server();
        ///        server1.Host = "whois.networksolutions.com";
        ///        server1.Port = 43;
        ///        server1.Domain = ".com";
        ///        servers.Add(server1);
        /// 
        ///        Server server2 = new Server();
        ///        server2.Host = "whois.nic.co.uk";
        ///        server2.Port = 43;
        ///        server2.Domain = ".co.uk";
        ///        servers.Add(server2);
        /// 
        ///        WhoIs whoIs = new WhoIs();
        ///        string result = whoIs.Query(servers,"activeup.com");
        ///        Console.WriteLine(result);
        ///    }
        /// 
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///    
        /// [VB.NET]
        /// 
        /// Try
        ///
        ///        Dim servers As New ServerCollection()
        ///
        ///        Dim server1 As New Server()
        ///        server1.Host = "whois.networksolutions.com"
        ///        server1.Port = 43
        ///        server1.Domain = ".com"
        ///        servers.Add(server1)
        ///
        ///        Dim server2 As New Server()
        ///        server2.Host = "whois.nic.co.uk"
        ///        server2.Port = 43
        ///        server2.Domain = ".co.uk"
        ///        servers.Add(server2)
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As String = whoIs.Query(servers, "activeup.com")
        ///        Console.WriteLine(result)
        ///
        ///    Catch we as WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///    End Try
        /// </code>
        /// </example>
        public string Query(ServerCollection servers, string domainToQuery)
        {
            return _Query(servers,domainToQuery,false);
        }

        /// <summary>
        /// Query a whois server specifying the specified ServerCollection object and domain to query asynchronously.
        /// </summary>
        /// <param name="servers">Collection contening a list of whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function</param>
        /// <returns>IAsyncResult object that represents the result of the QueryAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// 
        ///    Server server1 = new Server();
        ///    server1.Host = "whois.networksolutions.com";
        ///    server1.Port = 43;
        ///    server1.Domain = ".com";
        ///    servers.Add(server1);
        /// 
        ///    Server server2 = new Server();
        ///    server2.Host = "whois.nic.co.uk";
        ///    server2.Port = 43;
        ///    server2.Domain = ".co.uk";
        ///    servers.Add(server2);
        ///  
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync(servers,"activeup.com",new AsyncCallback(MyCallbackWhoIs));
        /// 
        /// public void MyCallbackWhoIs(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            string result = whoIs.QueryAsyncResult(state);
        ///            Console.WriteLine(result);
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// 
        ///    Dim servers As New ServerCollection()
        ///
        ///    Dim server1 As New Server()
        ///    server1.Host = "whois.networksolutions.com"
        ///    server1.Port = 43
        ///    server1.Domain = ".com"
        ///    servers.Add(server1)
        ///
        ///    Dim server2 As New Server()
        ///    server2.Host = "whois.nic.co.uk"
        ///    server2.Port = 43
        ///    server2.Domain = ".co.uk"
        ///    servers.Add(server2)
        /// Dim state As IAsyncResult = whoIs.QueryAsync(servers, New AsyncCallback(AddressOf MyCallbackWhoIs))
        /// 
        /// Public Sub MyCallbackWhoIs(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///
        ///            Dim result As String = whoIs.QueryAsyncResult(state)
        ///            Console.WriteLine(result)
        ///
        ///        Catch we As WhoisException
        ///             Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult QueryAsync(ServerCollection servers, string domainToQuery,AsyncCallback callBack)
        {
            _queryDelegate = new QueryDelegate(_Query);
            return _queryDelegate.BeginInvoke(servers,domainToQuery,false,callBack,null);
        }

        /// <summary>
        /// Blocks the execution until the request is completed.
        /// </summary>
        /// <param name="asynResultQuery">State of the operation.</param>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync("activeup.com",new AsyncCallback(MyCallbackWhoIs));
        /// whoIs.QueryAsyncWait(state);
        /// 
        /// public void MyCallbackWhoIs(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            string result = whoIs.QueryAsyncResult(state);
        ///            Console.WriteLine(result);
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET]
        /// </code>
        /// </example>
        public void QueryAsyncWait(IAsyncResult asynResultQuery)
        {
            asynResultQuery.AsyncWaitHandle.WaitOne();

            while(asynResultQuery.IsCompleted == false) 
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Result of the asynchronously query.
        /// </summary>
        /// <param name="state">State of the operation.</param>
        /// <returns>Result of the whois server query.</returns>
        public string QueryAsyncResult(IAsyncResult state)
        {
            return _queryDelegate.EndInvoke(state);
        }

        /// <summary>
        ///  Query a whois server specifying the specified ServerCollection, domain to query and if the no match string have to be checked.
        /// </summary>
        /// <param name="servers">Collection contening a list of whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="checkNoMatch">Indicates if you have to use 'no match' in query.</param>
        /// <returns>Result of the whois server.</returns>
        private string _Query(ServerCollection servers, string domainToQuery, bool checkNoMatch)
        {
            if (servers == null)
                servers = _servers;

            string domainExt = "";
            string domainName = "";

            int indexSeparator = domainToQuery.IndexOf(".");
            if (indexSeparator == -1)
                throw new WhoisException(string.Format("The domain extention is not present in '{0}'",domainToQuery));
            
            domainName = domainToQuery.Substring(0,indexSeparator);
            domainExt = domainToQuery.Substring(indexSeparator);

#if (TRIAL)
            if (domainExt.ToUpper() != ".ORG")
                throw new TrialException();
#endif

            Server whoisServer = null;
            if (servers.Count == 1)
                whoisServer = servers[0];
            else
            {
                for(int i = 0 ; i < servers.Count ; i++)
                {
                    if (domainExt.ToLower() == servers[i].Domain.ToLower())
                    {
                        whoisServer = servers[i];
                        break;
                    }
                }
            }
            
            if (whoisServer == null)
                throw new WhoisException(string.Format("No server present for '{0}'",domainExt));

            if (checkNoMatch == true)
            {
                if (whoisServer.NoMatch == null)
                    throw new WhoisException(string.Format("NoMatch property from host '{0}' cannot be null",whoisServer.Host));

                if (whoisServer.NoMatch.Trim() == "")
                    throw new WhoisException(string.Format("NoMatch property from host '{0}' cannot be blank",whoisServer.Host));

            }

            // when all parameter are ok, take information on whois server

            TcpClient tcpConnection = null;
            Stream baseStream = null;
            StringBuilder resultString = new StringBuilder();
        
            try 
            {
                tcpConnection = new TcpClient(whoisServer.Host, whoisServer.Port);

                NetworkStream  networkStream = tcpConnection.GetStream();
#if !PocketPC
                baseStream = new BufferedStream(networkStream);
#else
                baseStream = networkStream;
#endif
            }
            catch(SocketException ex) 
            {
                WhoisException whoisException = new WhoisException(string.Format("Error attempting to open connection to '{0}' : {1}", whoisServer.Host,ex.Message));
                throw whoisException;
            }

            // create the output stream
            try 
            {
                StreamWriter   outputStream  = null;
                outputStream = new StreamWriter(baseStream);
                outputStream.WriteLine(domainToQuery); // domain to query
                outputStream.Flush();
            }
            catch(Exception ex) 
            { 
                tcpConnection.Close();

                WhoisException whoisException = new WhoisException(string.Format("Error attempting to send data to '{0}' : {1}", whoisServer.Host,ex.Message));
                throw whoisException;
            }

            // read the response
            try 
            {
                StreamReader inputStream = new StreamReader(baseStream);
                
                string intermediateOutput;
                
                while(null != (intermediateOutput = inputStream.ReadLine())) 
                {
                    resultString.Append(intermediateOutput);
                    resultString.Append("\n");
                }
            }
            catch(Exception ex) 
            {
                tcpConnection.Close();

                WhoisException whoisException = new WhoisException(string.Format("Error attempting to read data to '{0}' : {1}", whoisServer.Host,ex.Message));
                throw whoisException;
            }

            tcpConnection.Close();

            if (checkNoMatch == true)
            {
                resultString.Insert(0,string.Format("{0}@",whoisServer.NoMatch));
            }

            return resultString.ToString();
        }

        /// <summary>
        ///  Query all the whois servers specifying the top level domain.
        ///  </summary>
        /// <returns>ResultQueryCollection object contening all the informations about the query.</returns>
        /// <example>
        ///     <code lang="CS">
        /// try
        /// {
        ///         WhoIs whoIs = new WhoIs();
        ///         ResultQueryCollection results = whoIs.GlobalQuery("activeup");
        ///  
        ///         foreach(ResultQuery result in results)
        ///         {
        ///             Console.WriteLine(result.Result);
        ///             Console.WriteLine(result.ServerUsed.Host);
        ///             if (result.Error != null)
        ///                 Console.WriteLine(result.Error.ToString());
        ///             Console.WriteLine("press enter to continue...");
        ///             Console.ReadLine();
        ///         }
        ///     }
        ///     
        ///     catch(Exception ex)
        ///     {
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///     }
        ///     </code>
        ///     <code lang="VB" title="VB.NET">
        /// Try
        ///  
        ///         Dim whoIs As WhoIs = New WhoIs()
        ///         Dim results As ResultQueryCollection = whoIs.GlobalQuery("activeup")
        ///         Dim result As ResultQuery
        ///         
        ///         For Each result In results
        ///             Console.WriteLine(result.Result)
        ///             Console.WriteLine(result.ServerUsed.Host)
        ///             If (Not (result.Error Is Nothing)) Then
        ///                 Console.WriteLine(result.Error.ToString())
        ///             End If
        ///             Console.WriteLine("press enter to continue...")
        ///             Console.ReadLine()
        ///         Next result
        ///         
        ///     Catch ex As Exception
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///         
        ///     End Try
        ///     </code>
        /// </example>
        /// <param name="domainToQuery">Top level domain to query.</param>
        public ResultQueryCollection GlobalQuery(string domainToQuery)
        {
            return _GlobalQuery(null,domainToQuery);
        }

        /// <summary>
        ///  Query all the whois servers specifying the top level domain asynchronously.
        ///  </summary>
        /// <returns>IAsyncResult object that represents the result of the GlobalQueryAsync operation.</returns>
        /// <example>
        ///     <code lang="CS">
        /// WhoIs whoIs = new WhoIs();
        ///  
        ///     IAsyncResult state = whoIs.GlobalQueryAsync("activeup",new AsyncCallback(MyCallbackGlobalWhoIs));
        ///     
        ///     public static void MyCallbackGlobalWhoIs(IAsyncResult state)
        ///     {
        ///         try
        ///         {
        ///             ResultQueryCollection results = whoIs.GlobalQueryAsyncResult(state);
        ///  
        ///             foreach(ResultQuery result in results)
        ///             {
        ///                 Console.WriteLine(result.Result);
        ///                 Console.WriteLine(result.ServerUsed.Host);
        ///                 if (result.Error != null)
        ///                     Console.WriteLine(result.Error.ToString());
        ///                 Console.WriteLine("press enter to continue...");
        ///                 Console.ReadLine();
        ///             }
        ///         }
        ///                     
        ///         catch(Exception ex)
        ///         {
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///         }
        ///     </code>
        ///     <code lang="VB" title="VB.NET">
        /// Dim whoIs As WhoIs = New WhoIs()
        ///  
        ///     Dim state As IAsyncResult = whoIs.GlobalQueryAsync("activeup", New AsyncCallback(AddressOf MyCallbackGlobalWhoIs))
        ///  
        ///         Public Sub MyCallbackGlobalWhoIs(ByVal state As IAsyncResult)
        ///  
        ///             Try
        ///  
        ///                 Dim results As ResultQueryCollection = whoIs.GlobalQueryAsyncResult(state)
        ///  
        ///                 Dim result As ResultQuery
        ///                 For Each result In results
        ///                     Console.WriteLine(result.Result)
        ///                     Console.WriteLine(result.ServerUsed.Host)
        ///                     If (Not (result.Error Is Nothing)) Then
        ///                         Console.WriteLine(result.Error.ToString())
        ///                     End If
        ///                     Console.WriteLine("press enter to continue...")
        ///                     Console.ReadLine()
        ///                 Next result
        ///  
        ///             Catch ex As Exception
        ///                  Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///  
        ///             End Try
        ///  
        ///     End Sub
        ///     </code>
        /// </example>
        /// <param name="domainToQuery">Top level domain to query.</param>
        /// <param name="callBack">Call back function.</param>
        public IAsyncResult GlobalQueryAsync(string domainToQuery, AsyncCallback callBack)
        {
            _globalQueryDelegate = new GlobalQueryDelegate(_GlobalQuery);
            return _globalQueryDelegate.BeginInvoke(null,domainToQuery,callBack,null);
        }

        /// <summary>
        ///  Query all the whois server specifying list of whois servers and the top level domain.
        ///  </summary>
        /// <returns>ResultQueryCollection object contening all the informations about the query.</returns>
        /// <example>
        ///     <code lang="CS">
        /// try
        /// {
        ///         ServerCollection servers = new ServerCollection();
        ///         
        ///         Server server1 = new Server();
        ///         server1.Host = "whois.networksolutions.com";
        ///         server1.Port = 43;
        ///         server1.Domain = ".com";
        ///         server1.NoMatch = "no match";
        ///         servers.Add(server1);
        ///         
        ///         Server server2 = new Server();
        ///         server2.Host = "whois.nic.co.uk";
        ///         server2.Port = 43;
        ///         server2.Domain = ".co.uk";
        ///         server2.NoMatch = "no match";
        ///         servers.Add(server2);
        ///  
        ///         WhoIs whoIs = new WhoIs();
        ///         ResultQueryCollection results = whoIs.GlobalQuery(servers,"activeup");
        ///  
        ///         foreach(ResultQuery result in results)
        ///         {
        ///             Console.WriteLine(result.Result);
        ///             Console.WriteLine(result.ServerUsed.Host);
        ///             if (result.Error != null)
        ///                 Console.WriteLine(result.Error.ToString());
        ///             Console.WriteLine("press enter to continue...");
        ///             Console.ReadLine();
        ///         }
        ///     }
        ///     
        ///     catch(Exception ex)
        ///     {
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///     }
        /// </code>
        ///     <code lang="VB" title="VB.NET">
        /// Try
        ///  
        ///         Dim servers As ServerCollection = New ServerCollection()
        ///         Dim server1 As Server = New Server()
        ///         server1.Host = "whois.networksolutions.com"
        ///         server1.Port = 43
        ///         server1.Domain = ".com"
        ///         server1.NoMatch = "no match"
        ///         servers.Add(server1)
        ///         
        ///         Dim server2 As Server = New Server()
        ///         server2.Host = "whois.nic.co.uk"
        ///         server2.Port = 43
        ///         server2.Domain = ".co.uk"
        ///         server2.NoMatch = "no match"
        ///         servers.Add(server2)
        ///  
        ///         Dim whoIs As WhoIs = New WhoIs()
        ///         Dim results As ResultQueryCollection = whoIs.GlobalQuery(servers,"activeup")
        ///         Dim result As ResultQuery
        ///         
        ///         For Each result In results
        ///             Console.WriteLine(result.Result)
        ///             Console.WriteLine(result.ServerUsed.Host)
        ///             If (Not (result.Error Is Nothing)) Then
        ///                 Console.WriteLine(result.Error.ToString())
        ///             End If
        ///             Console.WriteLine("press enter to continue...")
        ///             Console.ReadLine()
        ///         Next result
        ///         
        ///     Catch ex As Exception
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///         
        ///     End Try
        ///     </code>
        /// </example>
        /// <param name="servers">Whois servers list.</param>
        /// <param name="domainToQuery">Top level domain to query.</param>
        public ResultQueryCollection GlobalQuery(ServerCollection servers, string domainToQuery)
        {
            return _GlobalQuery(servers,domainToQuery);
        }

        /// <summary>
        ///  Query all the whois server specifying list of whois servers and the top level domain asynchronously
        ///  </summary>
        /// <returns>IAsyncResult object that represents the result of the GlobalQueryAsync operation.</returns>
        /// <example>
        ///     <code lang="CS">
        /// WhoIs whoIs = new WhoIs();
        ///  
        /// Server server1 = new Server();
        ///     server1.Host = "whois.networksolutions.com";
        ///     server1.Port = 43;
        ///     server1.Domain = ".com";
        ///     server1.NoMatch = "no match";
        ///     servers.Add(server1);
        ///             
        ///     Server server2 = new Server();
        ///     server2.Host = "whois.nic.co.uk";
        ///     server2.Port = 43;
        ///     server2.Domain = ".co.uk";
        ///     server2.NoMatch = "no match";
        ///     servers.Add(server2);
        ///             
        ///     IAsyncResult state = whoIs.GlobalQueryAsync(servers,"activeup",new AsyncCallback(MyCallbackGlobalWhoIs));
        ///     
        ///     public static void MyCallbackGlobalWhoIs(IAsyncResult state)
        ///     {
        ///         try
        ///         {
        ///             ResultQueryCollection results = whoIs.GlobalQueryAsyncResult(state);
        ///  
        ///             foreach(ResultQuery result in results)
        ///             {
        ///                 Console.WriteLine(result.Result);
        ///                 Console.WriteLine(result.ServerUsed.Host);
        ///                 if (result.Error != null)
        ///                     Console.WriteLine(result.Error.ToString());
        ///                 Console.WriteLine("press enter to continue...");
        ///                 Console.ReadLine();
        ///             }
        ///         }
        ///                     
        ///         catch(Exception ex)
        ///         {
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///         }
        /// </code>
        ///     <code lang="VB" title="VB.NET">
        /// Dim whoIs As WhoIs = New WhoIs()
        ///  
        /// Dim servers As ServerCollection = New ServerCollection()
        ///  
        ///     Dim server1 As Server = New Server()
        ///     server1.Host = "whois.networksolutions.com"
        ///     server1.Port = 43
        ///     server1.Domain = ".com"
        ///     server1.NoMatch = "no match"
        ///     servers.Add(server1)
        ///  
        ///     Dim server2 As Server = New Server()
        ///     server2.Host = "whois.nic.co.uk"
        ///     server2.Port = 43
        ///     server2.Domain = ".co.uk"
        ///     server2.NoMatch = "no match"
        ///     servers.Add(server2)
        ///  
        ///     Dim state As IAsyncResult = whoIs.GlobalQueryAsync(servers, "activeup", New AsyncCallback(AddressOf MyCallbackGlobalWhoIs))
        ///  
        ///         Public Sub MyCallbackGlobalWhoIs(ByVal state As IAsyncResult)
        ///  
        ///             Try
        ///  
        ///                 Dim results As ResultQueryCollection = whoIs.GlobalQueryAsyncResult(state)
        ///  
        ///                 Dim result As ResultQuery
        ///                 For Each result In results
        ///                     Console.WriteLine(result.Result)
        ///                     Console.WriteLine(result.ServerUsed.Host)
        ///                     If (Not (result.Error Is Nothing)) Then
        ///                         Console.WriteLine(result.Error.ToString())
        ///                     End If
        ///                     Console.WriteLine("press enter to continue...")
        ///                     Console.ReadLine()
        ///                 Next result
        ///  
        ///             Catch ex As Exception
        ///                  Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///  
        ///             End Try
        ///  
        ///     End Sub
        ///     </code>
        /// </example>
        /// <param name="servers">Whois servers list.</param>
        /// <param name="domainToQuery">Top level domain to query.</param>
        /// <param name="callBack">Call back function.</param>
        public IAsyncResult GlobalQueryAsync(ServerCollection servers, string domainToQuery, AsyncCallback callBack)
        {
            _globalQueryDelegate = new GlobalQueryDelegate(_GlobalQuery);
            return _globalQueryDelegate.BeginInvoke(servers,domainToQuery,callBack,null);

        }
        
        /// <summary>
        /// Result of the asynchronously query.
        /// </summary>
        /// <param name="state">State of the operation.</param>
        /// <returns>Result of the global whois server query.</returns>
        public ResultQueryCollection GlobalQueryAsyncResult(IAsyncResult state)
        {
            return _globalQueryDelegate.EndInvoke(state);
        }

        /// <summary>
        ///  Blocks the execution until the request is completed.
        ///  </summary>
        /// <example>
        ///     <code lang="CS">
        /// WhoIs whoIs = new WhoIs();
        ///  
        ///     IAsyncResult state = whoIs.GlobalQueryAsync("activeup",new AsyncCallback(MyCallbackGlobalWhoIs));
        ///     whoIs.GlobalQueryAsyncWait(state);
        ///     
        ///     public static void MyCallbackGlobalWhoIs(IAsyncResult state)
        ///     {
        ///         try
        ///         {
        ///             ResultQueryCollection results = whoIs.GlobalQueryAsyncResult(state);
        ///  
        ///             foreach(ResultQuery result in results)
        ///             {
        ///                 Console.WriteLine(result.Result);
        ///                 Console.WriteLine(result.ServerUsed.Host);
        ///                 if (result.Error != null)
        ///                     Console.WriteLine(result.Error.ToString());
        ///                 Console.WriteLine("press enter to continue...");
        ///                 Console.ReadLine();
        ///             }
        ///         }
        ///                     
        ///         catch(Exception ex)
        ///         {
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///         }
        /// </code>
        ///     <code lang="VB" title="VB.NET">
        /// Dim whoIs As WhoIs = New WhoIs()
        ///  
        ///     Dim state As IAsyncResult = whoIs.GlobalQueryAsync("activeup", New AsyncCallback(AddressOf MyCallbackGlobalWhoIs))
        ///     whoIs.GlobalQueryAsyncWait(state)
        ///  
        ///         Public Sub MyCallbackGlobalWhoIs(ByVal state As IAsyncResult)
        ///  
        ///             Try
        ///  
        ///                 Dim results As ResultQueryCollection = whoIs.GlobalQueryAsyncResult(state)
        ///  
        ///                 Dim result As ResultQuery
        ///                 For Each result In results
        ///                     Console.WriteLine(result.Result)
        ///                     Console.WriteLine(result.ServerUsed.Host)
        ///                     If (Not (result.Error Is Nothing)) Then
        ///                         Console.WriteLine(result.Error.ToString())
        ///                     End If
        ///                     Console.WriteLine("press enter to continue...")
        ///                     Console.ReadLine()
        ///                 Next result
        ///  
        ///             Catch ex As Exception
        ///                  Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///  
        ///             End Try
        ///  
        ///     End Sub
        ///     </code>
        /// </example>
        /// <param name="asynResultQuery">State of the operation.</param>
        public void GlobalQueryAsyncWait(IAsyncResult asynResultQuery)
        {
            asynResultQuery.AsyncWaitHandle.WaitOne();

            while(asynResultQuery.IsCompleted == false) 
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Query all the whois server specifying list of whois servers and the top level domain.
        /// </summary>
        /// <param name="servers">Whois servers list.</param>
        /// <param name="domain">Top level domain.</param>
        /// <returns>ResultQueryCollection object contening all the informations about the query.</returns>
        private ResultQueryCollection _GlobalQuery(ServerCollection servers, string domain)
        {
            ResultQueryCollection results = new ResultQueryCollection();
            string result = "";
            ServerCollection whoisServers = new ServerCollection();
            if (servers == null)
                whoisServers = _servers;
            else
                whoisServers = servers;

            foreach(Server server in whoisServers)
            {
                try
                {
                    result = "";
                    result = Query(server,domain+server.Domain);
                    results.Add(result,server);
                }

                catch(TrialException te)
                {
                    results.Add(result,server,te);
                }

                catch(WhoisException we)
                {
                    results.Add(result,server,we);
                }

                catch(Exception ex)
                {
                    results.Add(result,server,ex);
                }
            }

            return results;
        }

        /// <summary>
        /// Checks if a domain is available for registration specifying the domain to query.
        /// </summary>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>True if the domain is available for registration. False if the domain name is not available for registration and is already taken by someone else.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        WhoIs whoIs = new WhoIs();
        ///        bool result = whoIs.IsAvailable("activeup.com");
        /// 
        ///        if (result == true)
        ///            Console.WriteLine("The domain is available for registration.");
        ///        else
        ///            Console.WriteLine("The domain is NOT available for registration.");
        ///    }
        ///        
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///    
        ///    [VB.NET]
        ///        
        ///    Try
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As Boolean = whoIs.IsAvailable("activeup.com")
        ///        If (result = True) Then
        ///            Console.WriteLine("The domain is available for registration.")
        ///        Else
        ///            Console.WriteLine("The domain is NOT available for registration.")
        ///        End If
        ///
        ///    Catch we As WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        /// End Try
        /// </code>
        /// </example>
        public bool IsAvailable(string domainToQuery)
        {
            return _IsAvailable(null,domainToQuery);
        }

        /// <summary>
        /// Checks to see if a domain is available for registration asynchronously specifying the domain to query.
        /// </summary>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function</param>
        /// <returns>IAsyncResult object that represents the result of the IsAvailableAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync("activeup.com",new AsyncCallback(MyCallbackIsAvailable));
        /// 
        /// public void MyCallbackIsAvailable(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            bool result = whoIs.IsAvailableAsyncResult(state);
        ///            if (result == true)
        ///                Console.WriteLine("The domain is available for registration.");
        ///            else
        ///                Console.WriteLine("The domain is NOT available for registration.");
        ///            
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// Dim state As IAsyncResult = whoIs.IsAvailableAsync("activeup.com", New AsyncCallback(AddressOf MyCallbackIsAvailable))
        /// 
        /// Public Sub MyCallbackIsAvailable(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///            Dim result As Boolean = whoIs.IsAvailableAsyncResult(state)
        ///            If (result = True) Then
        ///                Console.WriteLine("The domain is available for registration.")
        ///            Else
        ///                Console.WriteLine("The domain is NOT available for registration.")
        ///            End If
        ///
        ///        Catch we As WhoisException
        ///            Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult IsAvailableAsync(string domainToQuery,AsyncCallback callBack)
        {
            _isAvailableDelegate = new IsAvailableDelegate(_IsAvailable);
            return _isAvailableDelegate.BeginInvoke(null,domainToQuery,callBack,null);
        }

        /// <summary>
        /// Checks if a domain is available for registration specifying the specified host, domain to query and the string indicates the domain doesn't exist.
        /// </summary>
        /// <param name="host">Host or IP address of the whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="noMatch">String indicates the domain doesn't exist.</param>
        /// <returns>True if the domain is available for registration. False if the domain name is not available for registration and is already taken by someone else.
        /// </returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        WhoIs whoIs = new WhoIs();
        ///        bool result = whoIs.IsAvailable("whois.networksolutions.com","activeup.com","no match");
        /// 
        ///        if (result == true)
        ///            Console.WriteLine("The domain is available for registration.");
        ///        else
        ///            Console.WriteLine("The domain is NOT available for registration.");
        ///    }
        ///        
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///        
        ///    [VB.NET]
        ///        
        ///    Try
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As Boolean = whoIs.IsAvailable("whois.networksolutions.com", "activeup.com", "no match")
        ///        If (result = True) Then
        ///            Console.WriteLine("The domain is available for registration.")
        ///        Else
        ///            Console.WriteLine("The domain is NOT available for registration.")
        ///        End If
        ///
        ///    Catch we As WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        /// End Try
        /// </code>
        /// </example>
        public bool IsAvailable(string host, string domainToQuery, string noMatch)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(new Server(host,"",noMatch));

            return _IsAvailable(servers, domainToQuery);
        }

        /// <summary>
        /// Checks to see if a domain is available for registration asynchronously specifying the specified host and the domain to query.
        /// </summary>
        /// <param name="host">Host of the whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function.</param>
        /// <returns>IAsyncResult object that represents the result of the IsAvailableAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync("whois.networksolutions.com","activeup.com",new AsyncCallback(MyCallbackIsAvailable));
        /// 
        /// public void MyCallbackIsAvailable(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            bool result = whoIs.IsAvailableAsyncResult(state);
        ///            if (result == true)
        ///                Console.WriteLine("The domain is available for registration.");
        ///            else
        ///                Console.WriteLine("The domain is NOT available for registration.");
        ///            
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// Dim state As IAsyncResult = whoIs.IsAvailableAsync("whois.networksolutions.com","activeup.com", New AsyncCallback(AddressOf MyCallbackIsAvailable))
        /// 
        /// Public Sub MyCallbackIsAvailable(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///            Dim result As Boolean = whoIs.IsAvailableAsyncResult(state)
        ///            If (result = True) Then
        ///                Console.WriteLine("The domain is available for registration.")
        ///            Else
        ///                Console.WriteLine("The domain is NOT available for registration.")
        ///            End If
        ///
        ///        Catch we As WhoisException
        ///            Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult IsAvailableAsync(string host,string domainToQuery,AsyncCallback callBack)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(new Server(host));

            _isAvailableDelegate = new IsAvailableDelegate(_IsAvailable);
            return _isAvailableDelegate.BeginInvoke(servers,domainToQuery,callBack,null);
        }

        /// <summary>
        /// Checks if a domain is available for registration specifying the specified host, port, domain to query and the string indicates the domain doesn't exist.
        /// </summary>
        /// <param name="host">Host or IP address of the whois server.</param>
        /// <param name="port">Port of the whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="noMatch">String indicates the domain doesn't exist</param>
        /// <returns>True if the domain is available for registration. False if the domain name is not available for registration and is already taken by someone else.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        WhoIs whoIs = new WhoIs();
        ///        bool result = whoIs.IsAvailable("whois.networksolutions.com",43,"activeup.com","no match");
        /// 
        ///        if (result == true)
        ///            Console.WriteLine("The domain is available for registration.");
        ///        else
        ///            Console.WriteLine("The domain is NOT available for registration.");
        ///    }
        ///        
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///        
        ///    [VB.NET]
        ///        
        ///    Try
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As Boolean = whoIs.IsAvailable("whois.networksolutions.com",43, "activeup.com", "no match")
        ///        If (result = True) Then
        ///            Console.WriteLine("The domain is available for registration.")
        ///        Else
        ///            Console.WriteLine("The domain is NOT available for registration.")
        ///        End If
        ///
        ///    Catch we As WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        /// End Try
        /// </code>
        /// </example>
        public bool IsAvailable(string host, int port, string domainToQuery, string noMatch)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(new Server(host,port,"",noMatch));

            return _IsAvailable(servers,domainToQuery);
        }

        /// <summary>
        /// Checks to see if a domain is available for registration asynchronously specifying the specified host, domain to query and the string indicates the domain doesn't exist.
        /// </summary>
        /// <param name="host">Host of the whois server.</param>
        /// <param name="port">Port of the whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function</param>
        /// <returns>IAsyncResult object that represents the result of the IsAvailableAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync("activeup.com",43,new AsyncCallback(MyCallbackIsAvailable));
        /// 
        /// public void MyCallbackIsAvailable(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            bool result = whoIs.IsAvailableAsyncResult(state);
        ///            if (result == true)
        ///                Console.WriteLine("The domain is available for registration.");
        ///            else
        ///                Console.WriteLine("The domain is NOT available for registration.");
        ///            
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// Dim state As IAsyncResult = whoIs.IsAvailableAsync("activeup.com",43, New AsyncCallback(AddressOf MyCallbackIsAvailable))
        /// 
        /// Public Sub MyCallbackIsAvailable(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///            Dim result As Boolean = whoIs.IsAvailableAsyncResult(state)
        ///            If (result = True) Then
        ///                Console.WriteLine("The domain is available for registration.")
        ///            Else
        ///                Console.WriteLine("The domain is NOT available for registration.")
        ///            End If
        ///
        ///        Catch we As WhoisException
        ///            Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult IsAvailableAsync(string host, int port, string domainToQuery,AsyncCallback callBack)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(new Server(host,port));

            _isAvailableDelegate = new IsAvailableDelegate(_IsAvailable);
            return _isAvailableDelegate.BeginInvoke(servers,domainToQuery,callBack,null);
        }

        /// <summary>
        /// Checks if a domain is available for registration specifying the specified Server object and  domain to query.
        /// </summary>
        /// <param name="server">Whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>True if the domain is available for registration. False if the domain name is not available for registration and is already taken by someone else.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        Server server = new Server();
        ///        server.Host = "";
        ///        server.Port = 43;
        ///        server.Domain = ".com";
        ///        server.NoMatch = "no match";
        /// 
        ///        WhoIs whoIs = new WhoIs();
        ///        bool result = whoIs.IsAvailable(server,"activeup.com");
        ///        if (result == true)
        ///            Console.WriteLine("The domain is available for registration.");
        ///        else
        ///            Console.WriteLine("The domain is NOT available for registration.");
        ///    }
        ///    
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///        
        ///    [VB.NET]
        ///    
        ///    Try
        ///    
        ///        Dim server As New Server()
        ///        server.Host = "whois.networksolutions.com"
        ///        server.Port = 43
        ///        server.Domain = ".com"
        ///        server.NoMatch = "no match"
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As Boolean = whoIs.IsAvailable(server, "activeup.com")
        ///        If (result = True) Then
        ///            Console.WriteLine("The domain is available for registration.")
        ///        Else
        ///            Console.WriteLine("The domain is NOT available for registration.")
        ///        End If
        ///
        ///    Catch we As WhoisException
        ///         Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///    End Try
        /// </code>
        /// </example>
        public bool IsAvailable(Server server, string domainToQuery)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(server);

            return _IsAvailable(servers,domainToQuery);
        }

        /// <summary>
        /// Checks to see if a domain is available for registration asynchronously sing the specified Server object and  domain to query.
        /// </summary>
        /// <param name="server">Whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function.</param>
        /// <returns>IAsyncResult object that represents the result of the IsAvailableAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// Server server = new Server();
        /// server.Host = "whois.networksolutions.com";
        /// server.Port = 43;
        /// server.Domain = ".com";
        /// server.NoMatch = "no match";
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.IsAvailableAsync(server,"activeup.com",new AsyncCallback(MyCallbackIsAvailable));
        /// 
        /// public void MyCallbackIsAvailable(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            bool result = whoIs.IsAvailableAsyncResult(state);
        ///            if (result == true)
        ///                Console.WriteLine("The domain is available for registration.");
        ///            else
        ///                Console.WriteLine("The domain is NOT available for registration.");
        ///            
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// 
         ///    Dim server As New Server()
        ///    server.Host = "whois.networksolutions.com"
        ///    server.Port = 43
        ///    server.Domain = ".com"
        ///    server.NoMatch = "no match"
        /// 
        /// Dim state As IAsyncResult = whoIs.IsAvailableAsync(server,"activeup.com", New AsyncCallback(AddressOf MyCallbackIsAvailable))
        /// 
        /// Public Sub MyCallbackIsAvailable(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///            Dim result As Boolean = whoIs.IsAvailableAsyncResult(state)
        ///            If (result = True) Then
        ///                Console.WriteLine("The domain is available for registration.")
        ///            Else
        ///                Console.WriteLine("The domain is NOT available for registration.")
        ///            End If
        ///
        ///        Catch we As WhoisException
        ///            Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult IsAvailableAsync(Server server, string domainToQuery,AsyncCallback callBack)
        {
            ServerCollection servers = new ServerCollection();
            servers.Add(server);

            _isAvailableDelegate = new IsAvailableDelegate(_IsAvailable);
            return _isAvailableDelegate.BeginInvoke(servers,domainToQuery,callBack,null);
        }

        /// <summary>
        /// Checks if a domain is available for registration specifying the specified ServerCollection object and domain to query.
        /// </summary>
        /// <param name="servers">Collection contening a list of whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>True if the domain is available for registration. False if the domain name is not available for registration and is already taken by someone else.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// try
        /// {
        ///        ServerCollection servers = new ServerCollection();
        /// 
        ///        Server server1 = new Server();
        ///        server1.Host = "whois.networksolutions.com";
        ///        server1.Port = 43;
        ///        server1.Domain = ".com";
        ///        server1.NoMatch = "no match";
        ///        servers.Add(server1);
        /// 
        ///        Server server2 = new Server();
        ///        server2.Host = "whois.nic.co.uk";
        ///        server2.Port = 43;
        ///        server2.Domain = ".co.uk";
        ///        server2.NoMatch = "no match";
        ///        servers.Add(server2);
        /// 
        ///        WhoIs whoIs = new WhoIs();
        ///        bool result = whoIs.IsAvailable(servers,"activeup.com");
        /// 
        ///        if (result == true)
        ///            Console.WriteLine("The domain is available for registration.");
        ///        else
        ///            Console.WriteLine("The domain is NOT available for registration.");
        ///    }
        ///    
        /// catch(WhoisException we)
        ///    {
        ///        Console.WriteLine("WhoisException : " + we.Message);
        ///    }
        ///
        ///    catch(Exception ex)
        ///    {
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///    }
        ///        
        ///    [VB.NET]
        ///
        ///    Try
        ///
        ///        Dim servers As New ServerCollection()
        ///
        ///        Dim server1 As New Server()
        ///        server1.Host = "whois.networksolutions.com"
        ///        server1.Port = 43
        ///        server1.Domain = ".com"
        ///        server1.NoMatch = "no match"
        ///        servers.Add(server1)
        ///
        ///        Dim server2 As New Server()
        ///        server2.Host = "whois.nic.co.uk"
        ///        server2.Port = 43
        ///        server2.Domain = ".co.uk"
        ///        server2.NoMatch = "no match"
        ///        servers.Add(server2)
        ///
        ///        Dim whoIs As New WhoIs()
        ///        Dim result As Boolean = whoIs.IsAvailable(servers, "activeup.com")
        ///        If (result = True) Then
        ///            Console.WriteLine("The domain is available for registration.")
        ///        Else
        ///            Console.WriteLine("The domain is NOT available for registration.")
        ///        End If
        ///
        ///    Catch we As WhoisException
        ///        Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///    Catch ex As Exception
        ///        Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///    End Try
        /// </code>
        /// </example>
        public bool IsAvailable(ServerCollection servers, string domainToQuery)
        {
            return _IsAvailable(servers,domainToQuery);
        }

        /// <summary>
        /// Checks to see if a domain is available for registration asynchronously specifying the specified ServerCollection object and domain to query.
        /// </summary>
        /// <param name="servers">Collection contening a list of whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <param name="callBack">Callback function.</param>
        /// <returns>IAsyncResult object that represents the result of the IsAvailableAsync operation.</returns>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// ServerCollection servers = new ServerCollection();
        /// 
        ///    Server server1 = new Server();
        ///    server1.Host = "whois.networksolutions.com";
        ///    server1.Port = 43;
        ///    server1.Domain = ".com";
        ///    server1.NoMatch = "no match";
        ///    servers.Add(server1);
        /// 
        ///    Server server2 = new Server();
        ///    server2.Host = "whois.nic.co.uk";
        ///    server2.Port = 43;
        ///    server2.Domain = ".co.uk";
        ///    server2.NoMatch = "no match";
        ///    servers.Add(server2);
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.QueryAsync(servers,new AsyncCallback(MyCallbackIsAvailable));
        /// 
        /// public void MyCallbackIsAvailable(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            bool result = whoIs.IsAvailableAsyncResult(state);
        ///            if (result == true)
        ///                Console.WriteLine("The domain is available for registration.");
        ///            else
        ///                Console.WriteLine("The domain is NOT available for registration.");
        ///            
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// 
        ///    Dim server1 As New Server()
        ///    server1.Host = "whois.networksolutions.com"
        ///    server1.Port = 43
        ///    server1.Domain = ".com"
        ///    server1.NoMatch = "no match"
        ///    servers.Add(server1)
        ///
        ///    Dim server2 As New Server()
        ///    server2.Host = "whois.nic.co.uk"
        ///    server2.Port = 43
        ///    server2.Domain = ".co.uk"
        ///    server2.NoMatch = "no match"
        ///    servers.Add(server2)
        /// 
        /// Dim state As IAsyncResult = whoIs.IsAvailableAsync(servers,"activeup.com", New AsyncCallback(AddressOf MyCallbackIsAvailable))
        /// 
        /// Public Sub MyCallbackIsAvailable(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///            Dim result As Boolean = whoIs.IsAvailableAsyncResult(state)
        ///            If (result = True) Then
        ///                Console.WriteLine("The domain is available for registration.")
        ///            Else
        ///                Console.WriteLine("The domain is NOT available for registration.")
        ///            End If
        ///
        ///        Catch we As WhoisException
        ///            Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public IAsyncResult IsAvailableAsync(ServerCollection servers, string domainToQuery,AsyncCallback callBack)
        {
            _isAvailableDelegate = new IsAvailableDelegate(_IsAvailable);
            return _isAvailableDelegate.BeginInvoke(servers,domainToQuery,callBack,null);
        }

        /// <summary>
        /// Blocks the execution until the request is completed.
        /// </summary>
        /// <param name="state">State of the operation.</param>
        /// <example>
        /// <code>
        /// [C#]
        /// 
        /// WhoIs whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.IsAvailableAsync("activeup.com",new AsyncCallback(MyCallbackIsAvailable));
        /// whoIs.IsAvailableAsyncWait(state);
        /// 
        /// public void MyCallbackIsAvailable(IAsyncResult state)
        /// {
        ///        try
        ///        {
        ///            bool result = whoIs.IsAvailableAsyncResult(state);
        ///            if (result == true)
        ///                Console.WriteLine("The domain is available for registration.");
        ///            else
        ///                Console.WriteLine("The domain is NOT available for registration.");
        ///            
        ///        }
        ///
        ///        catch(WhoisException we)
        ///        {
        ///            Console.WriteLine("WhoisException : " + we.Message);
        ///        }
        ///
        ///        catch(Exception ex)
        ///        {
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///        }
        /// }
        /// 
        /// [VB.NET] 
        /// 
        /// Private whoIs As whoIs = New whoIs()
        /// Dim state As IAsyncResult = whoIs.IsAvailableAsync("activeup.com", New AsyncCallback(AddressOf MyCallbackIsAvailable))
        /// whoIs.IsAvailableAsyncWait(state)
        /// 
        /// Public Sub MyCallbackIsAvailable(ByVal state As IAsyncResult)
        ///
        ///        Try
        ///            Dim result As Boolean = whoIs.IsAvailableAsyncResult(state)
        ///            If (result = True) Then
        ///                Console.WriteLine("The domain is available for registration.")
        ///            Else
        ///                Console.WriteLine("The domain is NOT available for registration.")
        ///            End If
        ///
        ///        Catch we As WhoisException
        ///            Console.WriteLine("WhoisException : " + we.Message)
        ///
        ///        Catch ex As Exception
        ///            Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///
        ///        End Try
        ///
        ///    End Sub
        /// </code>
        /// </example>
        public void IsAvailableAsyncWait(IAsyncResult state)
        {
            state.AsyncWaitHandle.WaitOne();

            while(state.IsCompleted == false) 
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Result of the asynchronously IsAvailable query.
        /// </summary>
        /// <param name="state">State of the operation.</param>
        /// <returns>Result of the asynchronously IsAvailable query.</returns>
        public bool IsAvailableAsyncResult(IAsyncResult state)
        {
            return (bool)_isAvailableDelegate.EndInvoke(state);
        }

        /// <summary>
        /// Checks if a domain is available for registration specifying the specified ServerCollection object and domain to query.
        /// </summary>
        /// <param name="servers">Collection contening a list of whois server.</param>
        /// <param name="domainToQuery">Domain to query.</param>
        /// <returns>True if the domain is available for registration. False if the domain name is not available for registration and is already taken by someone else.</returns>
        private bool _IsAvailable(ServerCollection servers, string domainToQuery)
        {
            string restultQuery = _Query(servers,domainToQuery,true);
            
            int index = restultQuery.IndexOf(_sepNoMatchQuery);
            if (index != -1)
            {
                string noMatch = restultQuery.Substring(0,index);
                string resultWhois = restultQuery.Substring(index + 1);

                if (resultWhois.ToLower().IndexOf(noMatch.ToLower()) != -1) return true;

                return false;
            }

            return false;
        }

        /// <summary>
        ///  Checks if a domain is available for registration specifying top leve domain to query.
        ///  </summary>
        /// <returns>ResultIsAvailableCollection object contening all the informations about the query.</returns>
        /// <example>
        ///     <code lang="CS">
        /// try
        ///     {
        ///         WhoIs whoIs = new WhoIs();
        ///         ResultIsAvailableCollection results = whoIs.GlobalIsAvailable("activeup");
        ///  
        ///         foreach(ResultIsAvailable result in results)
        ///         {
        ///             Console.WriteLine(result.ServerUsed.Host);
        ///  
        ///             if (result.Error != null)
        ///                 Console.WriteLine(result.Error.ToString());
        ///             else
        ///             {
        ///  
        ///                 if (result.IsAvailable == true)
        ///                     Console.WriteLine("The domain is available for registration ({0}).",result.ServerUsed.Domain);
        ///                 else
        ///                     Console.WriteLine("The domain is NOT available for registration ({0}).",result.ServerUsed.Domain);
        ///             }
        ///                                 
        ///             Console.WriteLine("press enter to continue...");
        ///             Console.ReadLine();
        ///         }
        ///     }
        ///  
        ///     catch(Exception ex)
        ///     {
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///     }
        /// </code>
        ///     <code lang="VB" title="VB.NET">
        /// Try
        ///  
        ///         Dim whoIs As New Whois()
        ///         Dim results As ResultIsAvailableCollection = whoIs.GlobalIsAvailable("activeup")
        ///  
        ///         Dim result As ResultIsAvailable
        ///         For Each result In results
        ///             Console.WriteLine(result.ServerUsed.Host)
        ///  
        ///             If (Not (result.Error Is Nothing)) Then
        ///                 Console.WriteLine(result.Error.ToString())
        ///             Else
        ///                 If (result.IsAvailable = True) Then
        ///                     Console.WriteLine("The domain is available for registration ({0}).", result.ServerUsed.Domain)
        ///                 Else
        ///                     Console.WriteLine("The domain is NOT available for registration ({0}).", result.ServerUsed.Domain)
        ///                 End If
        ///  
        ///             End If
        ///  
        ///         Console.WriteLine("press enter to continue")
        ///         Console.ReadLine()
        ///  
        ///         Next result
        ///  
        ///     Catch ex As Exception
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///  
        ///     End Try
        /// </code>
        /// </example>
        /// <param name="domainToQuery">Top level domain to query.</param>
        public ResultIsAvailableCollection GlobalIsAvailable(string domainToQuery)
        {
            return _GlobalIsAvailable(null,domainToQuery);
        }

        /// <summary>
        ///  Checks if a domain is available for registration specifying top leve domain to query asynchronously.
        ///  </summary>
        /// <returns>Result of the global 'is available' query.</returns>
        /// <example>
        ///     <code lang="CS">
        /// Whois whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.GlobalIsAvailableAsync("activeup",new AsyncCallback(MyCallbackGlobalIsAvailable));
        ///  
        ///     public void MyCallbackGlobalIsAvailable(IAsyncResult state)
        ///     {
        ///         try
        ///         {
        ///             ResultIsAvailableCollection results = whoIs.GlobalIsAvailableAsyncResult(state);
        ///  
        ///             foreach(ResultIsAvailable result in results)
        ///             {
        ///                 Console.WriteLine(result.ServerUsed.Host);
        ///  
        ///                 if (result.Error != null)
        ///                     Console.WriteLine(result.Error.ToString());
        ///                 else
        ///                 {
        ///                     if (result.IsAvailable == true)
        ///                         Console.WriteLine("The domain is available for registration ({0}).",result.ServerUsed.Domain);
        ///                     else
        ///                         Console.WriteLine("The domain is NOT available for registration ({0}).",result.ServerUsed.Domain);
        ///                 }
        ///                                 
        ///                 Console.WriteLine("press enter to continue...");
        ///                 Console.ReadLine();
        ///            }
        ///         }
        ///  
        ///         catch(Exception ex)
        ///         {
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///         }
        ///     }
        /// </code>
        ///     <code lang="VB" title="VB.NET">
        /// Dim whoIs As WhoIs = New WhoIs()
        /// Dim state As IAsyncResult = whoIs.GlobalIsAvailableAsync("activeup", New AsyncCallback(AddressOf MyCallBackGlobalIsAvailable))
        ///  
        /// Public Sub MyCallBackGlobalIsAvailable(ByVal state As IAsyncResult)
        ///  
        ///         Try
        ///             Dim results As ResultIsAvailableCollection = whoIs.GlobalIsAvailableAsyncResult(state)
        ///  
        ///             Dim result As ResultIsAvailable
        ///             For Each result In results
        ///  
        ///               Console.WriteLine(result.ServerUsed.Host)
        ///  
        ///               If (Not (result.Error Is Nothing)) Then
        ///                  Console.WriteLine(result.Error.ToString())
        ///               Else
        ///                 If (result.IsAvailable = True) Then
        ///                     Console.WriteLine("The domain is available for registration ({0}).", result.ServerUsed.Domain)
        ///                 Else
        ///                     Console.WriteLine("The domain is NOT available for registration ({0}).", result.ServerUsed.Domain)
        ///                 End If
        ///  
        ///               End If
        ///  
        ///             Console.WriteLine("press enter to continue")
        ///             Console.ReadLine()
        ///  
        ///            Next result
        ///  
        ///         Catch ex As Exception
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///  
        ///         End Try
        ///  
        ///     End Sub
        ///     </code>
        /// </example>
        /// <param name="domainToQuery">Top level domain to query.</param>
        /// <param name="callBack">Call back function.</param>
        public IAsyncResult GlobalIsAvailableAsync(string domainToQuery, AsyncCallback callBack)
        {
            _globalIsAvailableDelegate = new GlobalIsAvailableDelegate(_GlobalIsAvailable);
            return _globalIsAvailableDelegate.BeginInvoke(null,domainToQuery,callBack,null);
        }

        /// <summary>
        ///  Checks if a domain is available for registration specifying the list of whois servers and the top leve domain to query.
        ///  </summary>
        /// <returns>ResultIsAvailableCollection object contening all the informations about the query.</returns>
        /// <example>
        ///     <code lang="CS">
        /// try
        ///     {
        ///         ServerCollection servers = new ServerCollection();
        ///  
        ///         Server server1 = new Server();
        ///         server1.Host = "whois.networksolutions.com";
        ///         server1.Port = 43;
        ///         server1.Domain = ".com";
        ///         server1.NoMatch = "no match";
        ///         servers.Add(server1);
        ///             
        ///         Server server2 = new Server();
        ///         server2.Host = "whois.nic.co.uk";
        ///         server2.Port = 43;
        ///         server2.Domain = ".co.uk";
        ///         server2.NoMatch = "no match";
        ///         servers.Add(server2);
        ///     
        ///         WhoIs whoIs = new WhoIs();
        ///         ResultIsAvailableCollection results = whoIs.GlobalIsAvailable(servers,"activeup");
        ///  
        ///         foreach(ResultIsAvailable result in results)
        ///         {
        ///             Console.WriteLine(result.ServerUsed.Host);
        ///  
        ///             if (result.Error != null)
        ///                 Console.WriteLine(result.Error.ToString());
        ///             else
        ///             {
        ///  
        ///                 if (result.IsAvailable == true)
        ///                     Console.WriteLine("The domain is available for registration ({0}).",result.ServerUsed.Domain);
        ///                 else
        ///                     Console.WriteLine("The domain is NOT available for registration ({0}).",result.ServerUsed.Domain);
        ///             }
        ///                                 
        ///             Console.WriteLine("press enter to continue...");
        ///             Console.ReadLine();
        ///         }
        ///     }
        ///  
        ///     catch(Exception ex)
        ///     {
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///     }
        /// </code>
        ///     <code lang="VB" title="VB.NET">
        /// Try
        ///  
        ///         Dim servers As ServerCollection = New ServerCollection()
        ///  
        ///         Dim server1 As Server = New Server()
        ///         server1.Host = "whois.networksolutions.com"
        ///         server1.Port = 43
        ///         server1.Domain = ".com"
        ///         server1.NoMatch = "no match"
        ///         servers.Add(server1)
        ///  
        ///         Dim server2 As Server = New Server()
        ///         server2.Host = "whois.nic.co.uk"
        ///         server2.Port = 43
        ///         server2.Domain = ".co.uk"
        ///         server2.NoMatch = "no match"
        ///         servers.Add(server2)
        ///  
        ///         Dim whoIs As New Whois()
        ///         Dim results As ResultIsAvailableCollection = whoIs.GlobalIsAvailable(servers,"activeup")
        ///  
        ///         Dim result As ResultIsAvailable
        ///         For Each result In results
        ///             Console.WriteLine(result.ServerUsed.Host)
        ///  
        ///             If (Not (result.Error Is Nothing)) Then
        ///                 Console.WriteLine(result.Error.ToString())
        ///             Else
        ///                 If (result.IsAvailable = True) Then
        ///                     Console.WriteLine("The domain is available for registration ({0}).", result.ServerUsed.Domain)
        ///                 Else
        ///                     Console.WriteLine("The domain is NOT available for registration ({0}).", result.ServerUsed.Domain)
        ///                 End If
        ///  
        ///             End If
        ///  
        ///         Console.WriteLine("press enter to continue")
        ///         Console.ReadLine()
        ///  
        ///         Next result
        ///  
        ///     Catch ex As Exception
        ///         Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///  
        ///     End Try
        /// </code>
        /// </example>
        /// <param name="servers">Whois servers list.</param>
        /// <param name="domainToQuery">Top level domain to query.</param>
        public ResultIsAvailableCollection GlobalIsAvailable(ServerCollection servers, string domainToQuery)
        {
            return _GlobalIsAvailable(servers,domainToQuery);
        }

        /// <summary>
        ///  Checks if a domain is available for registration specifying the list of whois servers and the top leve domain to query asynchronously.
        ///  </summary>
        /// <returns>Result of the global 'is available' query.</returns>
        /// <example>
        ///     <code lang="CS">
        /// Whois whoIs = new WhoIs();
        ///  
        /// ServerCollection servers = new ServerCollection();
        ///     
        ///     Server server1 = new Server();
        ///     server1.Host = "whois.networksolutions.com";
        ///     server1.Port = 43;
        ///     server1.Domain = ".com";
        ///     server1.NoMatch = "no match";
        ///     servers.Add(server1);
        ///     
        /// Server server2 = new Server();
        /// server2.Host = "whois.nic.co.uk";
        /// server2.Port = 43;
        /// server2.Domain = ".co.uk";
        /// server2.NoMatch = "no match";
        /// servers.Add(server2);
        ///  
        /// IAsyncResult state = whoIs.GlobalIsAvailableAsync(servers,"activeup",new AsyncCallback(MyCallbackGlobalIsAvailable));
        ///  
        ///     public void MyCallbackGlobalIsAvailable(IAsyncResult state)
        ///     {
        ///         try
        ///         {
        ///             ResultIsAvailableCollection results = whoIs.GlobalIsAvailableAsyncResult(state);
        ///  
        ///             foreach(ResultIsAvailable result in results)
        ///             {
        ///                 Console.WriteLine(result.ServerUsed.Host);
        ///  
        ///                 if (result.Error != null)
        ///                     Console.WriteLine(result.Error.ToString());
        ///                 else
        ///                 {
        ///                     if (result.IsAvailable == true)
        ///                         Console.WriteLine("The domain is available for registration ({0}).",result.ServerUsed.Domain);
        ///                     else
        ///                         Console.WriteLine("The domain is NOT available for registration ({0}).",result.ServerUsed.Domain);
        ///                 }
        ///                                 
        ///                 Console.WriteLine("press enter to continue...");
        ///                 Console.ReadLine();
        ///            }
        ///         }
        ///  
        ///         catch(Exception ex)
        ///         {
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///         }
        ///     }
        /// </code>
        ///     <code lang="VB" title="VB.NET">
        /// Dim whoIs As WhoIs = New WhoIs()
        ///  
        ///     Dim servers As ServerCollection = New ServerCollection()
        ///  
        ///     Dim server1 As Server = New Server()
        ///     server1.Host = "whois.networksolutions.com"
        ///     server1.Port = 43
        ///     server1.Domain = ".com"
        ///     server1.NoMatch = "no match"
        ///     servers.Add(server1)
        ///  
        ///     Dim server2 As Server = New Server()
        ///     server2.Host = "whois.nic.co.uk"
        ///     server2.Port = 43
        ///     server2.Domain = ".co.uk"
        ///     server2.NoMatch = "no match"
        ///     servers.Add(server2)    
        ///  
        /// Dim state As IAsyncResult = whoIs.GlobalIsAvailableAsync(servers,"activeup", New AsyncCallback(AddressOf MyCallBackGlobalIsAvailable))
        ///  
        /// Public Sub MyCallBackGlobalIsAvailable(ByVal state As IAsyncResult)
        ///  
        ///         Try
        ///             Dim results As ResultIsAvailableCollection = whoIs.GlobalIsAvailableAsyncResult(state)
        ///  
        ///             Dim result As ResultIsAvailable
        ///             For Each result In results
        ///  
        ///               Console.WriteLine(result.ServerUsed.Host)
        ///  
        ///               If (Not (result.Error Is Nothing)) Then
        ///                  Console.WriteLine(result.Error.ToString())
        ///               Else
        ///                 If (result.IsAvailable = True) Then
        ///                     Console.WriteLine("The domain is available for registration ({0}).", result.ServerUsed.Domain)
        ///                 Else
        ///                     Console.WriteLine("The domain is NOT available for registration ({0}).", result.ServerUsed.Domain)
        ///                 End If
        ///  
        ///               End If
        ///  
        ///             Console.WriteLine("press enter to continue")
        ///             Console.ReadLine()
        ///  
        ///            Next result
        ///  
        ///         Catch ex As Exception
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///  
        ///         End Try
        ///  
        ///     End Sub
        ///     </code>
        /// </example>
        /// <param name="servers">Whois servers list.</param>
        /// <param name="domainToQuery">Top level domain to query.</param>
        /// <param name="callBack">Call back function.</param>
        public IAsyncResult GlobalIsAvailableAsync(ServerCollection servers, string domainToQuery, AsyncCallback callBack)
        {
            _globalIsAvailableDelegate = new GlobalIsAvailableDelegate(_GlobalIsAvailable);
            return _globalIsAvailableDelegate.BeginInvoke(servers,domainToQuery,callBack,null);
        }

        /// <summary>
        /// Result of the asynchronously query.
        /// </summary>
        /// <param name="state">State of the operation.</param>
        /// <returns>Result of the global whois server query.</returns>
        public ResultIsAvailableCollection GlobalIsAvailableAsyncResult(IAsyncResult state)
        {
            return _globalIsAvailableDelegate.EndInvoke(state);
        }

        /// <example>
        ///     <code lang="CS">
        /// Whois whoIs = new WhoIs();
        /// IAsyncResult state = whoIs.GlobalIsAvailableAsync("activeup",new AsyncCallback(MyCallbackGlobalIsAvailable));
        /// whoIs.GlobalIsAvailableAsyncWait(state);
        ///  
        ///     public void MyCallbackGlobalIsAvailable(IAsyncResult state)
        ///     {
        ///         try
        ///         {
        ///             ResultIsAvailableCollection results = whoIs.GlobalIsAvailableAsyncResult(state);
        ///  
        ///             foreach(ResultIsAvailable result in results)
        ///             {
        ///                 Console.WriteLine(result.ServerUsed.Host);
        ///  
        ///                 if (result.Error != null)
        ///                     Console.WriteLine(result.Error.ToString());
        ///                 else
        ///                 {
        ///                     if (result.IsAvailable == true)
        ///                         Console.WriteLine("The domain is available for registration ({0}).",result.ServerUsed.Domain);
        ///                     else
        ///                         Console.WriteLine("The domain is NOT available for registration ({0}).",result.ServerUsed.Domain);
        ///                 }
        ///                                 
        ///                 Console.WriteLine("press enter to continue...");
        ///                 Console.ReadLine();
        ///            }
        ///         }
        ///  
        ///         catch(Exception ex)
        ///         {
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message);
        ///         }
        ///     }
        /// </code>
        ///     <code lang="VB" title="VB.NET">
        /// Dim whoIs As WhoIs = New WhoIs()
        /// Dim state As IAsyncResult = whoIs.GlobalIsAvailableAsync("activeup", New AsyncCallback(AddressOf MyCallBackGlobalIsAvailable))
        /// whoIs.GlobalIsAvailableAsyncWait(state)
        ///  
        /// Public Sub MyCallBackGlobalIsAvailable(ByVal state As IAsyncResult)
        ///  
        ///         Try
        ///             Dim results As ResultIsAvailableCollection = whoIs.GlobalIsAvailableAsyncResult(state)
        ///  
        ///             Dim result As ResultIsAvailable
        ///             For Each result In results
        ///  
        ///               Console.WriteLine(result.ServerUsed.Host)
        ///  
        ///               If (Not (result.Error Is Nothing)) Then
        ///                  Console.WriteLine(result.Error.ToString())
        ///               Else
        ///                 If (result.IsAvailable = True) Then
        ///                     Console.WriteLine("The domain is available for registration ({0}).", result.ServerUsed.Domain)
        ///                 Else
        ///                     Console.WriteLine("The domain is NOT available for registration ({0}).", result.ServerUsed.Domain)
        ///                 End If
        ///  
        ///               End If
        ///  
        ///             Console.WriteLine("press enter to continue")
        ///             Console.ReadLine()
        ///  
        ///            Next result
        ///  
        ///         Catch ex As Exception
        ///             Console.WriteLine("An unhandled exception was thrown : " + ex.Message)
        ///  
        ///         End Try
        ///  
        ///     End Sub
        ///     </code>
        /// </example>
        public void GlobalIsAvailableAsyncWait(IAsyncResult asynResultQuery)
        {
            asynResultQuery.AsyncWaitHandle.WaitOne();

            while(asynResultQuery.IsCompleted == false) 
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Checks if a domain is available for registration specifying the list of whois servers and the top leve domain to query.
        /// </summary>
        /// <param name="servers">Whois servers list.</param>
        /// <param name="domainToQuery">Top level domain to query.</param>
        /// <returns>ResultIsAvailableCollection object contening all the informations about the query.</returns>
        private ResultIsAvailableCollection _GlobalIsAvailable(ServerCollection servers, string domainToQuery)
        {
            ResultIsAvailableCollection results = new ResultIsAvailableCollection();
            bool result = false;
            ServerCollection whoisServers = new ServerCollection();
            if (servers == null)
                whoisServers = _servers;
            else
                whoisServers = servers;

            foreach(Server server in whoisServers)
            {
                try
                {
                    result = false;
                    result = IsAvailable(server,domainToQuery+server.Domain);
                    results.Add(result,server);
                }

                catch(WhoisException we)
                {
                    results.Add(result,server,we);
                }

                catch(Exception ex)
                {
                    results.Add(result,server,ex);
                }
            }

            return results;
        }

        #endregion
    }

    #endregion
}
