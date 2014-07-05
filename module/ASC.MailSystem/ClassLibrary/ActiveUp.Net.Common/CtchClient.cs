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
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.IO;

namespace ActiveUp.Net.Mail
{
    public class CtchClient
    {
        /// <summary>
        /// Queries the server.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="messageFilename">The message filename.</param>
        /// <returns></returns>
        public static CtchResponse QueryServer(string host, int port, string messageFilename)
        {
            return QueryServer(host, port, null, messageFilename);
        }

        /// <summary>
        /// Queries the server.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static CtchResponse QueryServer(string host, int port, Message message)
        {
            return QueryServer(host, port, message, string.Empty);
        }

        /// <summary>
        /// Queries the server.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="message">The message.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        private static CtchResponse QueryServer(string host, int port, Message message, string filename)
        {
            bool reference = true;

            if (message != null)
                reference = false;
            else
                message = Parser.ParseMessageFromFile(filename);
            
            string version = "0000001";
            
            // Prepare the commtouch headers
            string content = string.Format("X-CTCH-PVer: {0}\r\nX-CTCH-MailFrom: {1}\r\nX-CTCH-SenderIP: {2}\r\n", version, message.Sender.Email, message.SenderIP);

            if (reference)
            {
                content += string.Format("X-CTCH-FileName: {0}\r\n", filename);
            }
            else
            {
                content += string.Format("\r\n{0}", message.ToMimeString());
            }

            // Prepare the request with HTTP header
            string request = string.Format("POST /ctasd/{0} HTTP/1.0\r\nContent-Length: {1}\r\n\r\n", (reference ? "ClassifyMessage_File" : "ClassifyMessage_Inline"), content.Length)
                + content;

            //try
            //{

                TcpClient client = new TcpClient();
                client.Connect(host, port);

                Byte[] data = System.Text.Encoding.ASCII.GetBytes(request);

                //  Stream stream = client.GetStream();
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

#if DEBUG
                Console.WriteLine("<requestSent>");
                Console.WriteLine("{0}", request);
                Console.WriteLine("</requestSent>");
#endif
                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

#if DEBUG
                Console.WriteLine("<responseReceived>");
                Console.WriteLine("{0}", responseData);
                Console.WriteLine("</responseReceived>");
#endif

                CtchResponse ctchResponse = CtchResponse.ParseFromString(responseData);

#if DEBUG
                Console.WriteLine(ctchResponse.ToString());
#endif
                // Close everything.
                stream.Close();
                client.Close();
            //}
            //catch (ArgumentNullException e)
            //{
            //    Console.WriteLine("ArgumentNullException: {0}", e);
            //}
            //catch (SocketException e)
            //{
            //    Console.WriteLine("SocketException: {0}", e);
            //}

            return ctchResponse;
        }

    }

    public class CtchResponse
    {
        //X-CTCH-Pver   string
        private string _version = string.Empty;
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }
        //X-CTCH-Spam   Confirmed, Bulk, Suspected, Unknown, Non-Spam
        private CtchSpam _spamClassification = CtchSpam.Unknown;
        /// <summary>
        /// Gets or sets the spam classification.
        /// </summary>
        /// <value>The spam classification.</value>
        public CtchSpam SpamClassification
        {
            get
            {
                return _spamClassification;
            }
            set
            {
                _spamClassification = value;
            }
        }
        //X-CTCH-VOD    Virus, High, Medium, Unknown, Non-Virus
        private CtchVod _vodClassification = CtchVod.Unknown;
        /// <summary>
        /// Gets or sets the vod classification.
        /// </summary>
        /// <value>The vod classification.</value>
        public CtchVod VodClassification
        {
            get
            {
                return _vodClassification;
            }
            set
            {
                _vodClassification = value;
            }
        }
        //X-CTCH-Flags  string
        private string _ctchFlag = string.Empty;
        /// <summary>
        /// Gets or sets the CTCH flag.
        /// </summary>
        /// <value>The CTCH flag.</value>
        public string CtchFlag
        {
            get
            {
                return _ctchFlag;
            }
            set
            {
                _ctchFlag = value;
            }
        }
        //X-CTCH-RefID string
        private string _refID = string.Empty;
        /// <summary>
        /// Gets or sets the ref ID.
        /// </summary>
        /// <value>The ref ID.</value>
        public string RefID
        {
            get
            {
                return _refID;
            }
            set
            {
                _refID = value;
            }
        }

        private NameValueCollection _headers = new NameValueCollection();
        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>The headers.</value>
        public NameValueCollection Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
            }
        }

        private string _fullResponse = string.Empty;
        /// <summary>
        /// Gets or sets the full response.
        /// </summary>
        /// <value>The full response.</value>
        public string FullResponse
        {
            get
            {
                return _fullResponse;
            }
            set
            {
                _fullResponse = value;
            }
        }

        /// <summary>
        /// Reads the name of the header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns></returns>
        private static string ReadHeaderName(string header)
        {
            return header.Split(':')[0].Trim();
        }

        /// <summary>
        /// Reads the header value.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns></returns>
        private static string ReadHeaderValue(string header)
        {
            if (header.IndexOf(':') > -1)
                return header.Split(':')[1].Trim();
            else
                return string.Empty;
        }

        /// <summary>
        /// Parses from string.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        public static CtchResponse ParseFromString(string response)
        {
            CtchResponse ctchResponse = new CtchResponse();

            ctchResponse.FullResponse = response;

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StringReader sr = new StringReader(response))
                {
                    String line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        string headerName = ReadHeaderName(line);

                        switch (headerName.ToUpper())
                        {
                            case "X-CTCH-PVER":
                                ctchResponse.Version = ReadHeaderValue(line); break;
                            case "X-CTCH-SPAM":
                                ctchResponse.SpamClassification = (CtchSpam)Enum.Parse(typeof(CtchSpam), ReadHeaderValue(line), true); break;
                            case "X-CTCH-VOD":
                                ctchResponse.VodClassification = (CtchVod)Enum.Parse(typeof(CtchVod), ReadHeaderValue(line), true); break;
                            case "X-CTCH-FLAGS":
                                ctchResponse.CtchFlag = ReadHeaderValue(line); break;
                            case "X-CTCH-REFID":
                                ctchResponse.RefID = ReadHeaderValue(line); break;
                            default:
                                ctchResponse.Headers.Add(headerName, ReadHeaderValue(line)); break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The response could not be read:");
                Console.WriteLine(e.Message);
            }

            return ctchResponse;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("X-CTCH-Pver: ");
            builder.Append(this.Version);
            builder.Append("\r\n");
            builder.Append("X-CTCH-Spam: ");
            builder.Append(this.SpamClassification.ToString());
            builder.Append("\r\n");
            builder.Append("X-CTCH-VOD: ");
            builder.Append(this.VodClassification.ToString());
            builder.Append("\r\n");
            builder.Append("X-CTCH-Flags: ");
            builder.Append(this.CtchFlag);
            builder.Append("\r\n");
            builder.Append("X-CTCH-RefID: ");

            builder.Append(this.RefID);
            builder.Append("\r\n");
            
            foreach (string key in this.Headers.AllKeys)
            {
                if (key.Trim() != string.Empty)
                {
                    builder.Append(key);
                    builder.Append(": ");
                    builder.Append(this.Headers[key]);
                    builder.Append("\r\n");
            
                }
            }

            return builder.ToString();
        }
    }

    public enum CtchSpam
    {
        Confirmed, Bulk, Suspected, Unknown, NonSpam
    }

    public enum CtchVod
    {
        Virus, High, Medium, Unknown, NonVirus
    }
}
