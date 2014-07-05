/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace LumiSoft.Net.HTTP.Server
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public class HTTP_Session : SocketServerSession
    {
        private HTTP_Server m_pServer = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sessionID">Session ID.</param>
        /// <param name="socket">Server connected socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
        /// <param name="server">Reference to server.</param>
        internal HTTP_Session(string sessionID,SocketEx socket,BindInfo bindInfo,HTTP_Server server) : base(sessionID,socket,bindInfo,server)
        {
            m_pServer = server;

            StartSession();
        }


        #region method StartSession

		/// <summary>
		/// Starts session.
		/// </summary>
		private void StartSession()
		{
			// Add session to session list
			m_pServer.AddSession(this);
	
			try{
                BeginRecieveCmd();/*

				// Check if ip is allowed to connect this computer
				if(m_pServer.OnValidate_IpAddress(this.LocalEndPoint,this.RemoteEndPoint)){
                    //--- Dedicated SSL connection, switch to SSL -----------------------------------//
                    if(this.BindInfo.SSL){
                        try{
                            this.Socket.SwitchToSSL(this.BindInfo.SSL_Certificate);

                            if(this.Socket.Logger != null){
                                this.Socket.Logger.AddTextEntry("SSL negotiation completed successfully.");
                            }
                        }
                        catch(Exception x){
                            if(this.Socket.Logger != null){
                                this.Socket.Logger.AddTextEntry("SSL handshake failed ! " + x.Message);

                                EndSession();
                                return;
                            }
                        }
                    }
                    //-------------------------------------------------------------------------------//
                    					
					BeginRecieveCmd();
				}
				else{
					EndSession();
				}*/
			}
			catch(Exception x){
				OnError(x);
			}
		}

		#endregion

		#region method EndSession

		/// <summary>
		/// Ends session, closes socket.
		/// </summary>
		private void EndSession()
		{          
			try{
				// Write logs to log file, if needed
				if(m_pServer.LogCommands){
					this.Socket.Logger.Flush();
				}

				if(this.Socket != null){
					this.Socket.Shutdown(SocketShutdown.Both);
					this.Socket.Disconnect();
					//this.Socket = null;
				}
			}
			catch{ // We don't need to check errors here, because they only may be Socket closing errors.
			}
			finally{
				m_pServer.RemoveSession(this);
			}
		}

		#endregion


        #region method OnError

		/// <summary>
		/// Is called when error occures.
		/// </summary>
		/// <param name="x"></param>
		private void OnError(Exception x)
		{
			try{
                // We must see InnerException too, SocketException may be as inner exception.
                SocketException socketException = null;
                if(x is SocketException){
                    socketException = (SocketException)x;
                }
                else if(x.InnerException != null && x.InnerException is SocketException){
                    socketException = (SocketException)x.InnerException;
                }

				if(socketException != null){
					// Client disconnected without shutting down
					if(socketException.ErrorCode == 10054 || socketException.ErrorCode == 10053){
						if(m_pServer.LogCommands){
							this.Socket.Logger.AddTextEntry("Client aborted/disconnected");
						}

						EndSession();

						// Exception handled, return
						return;
					}
				}

                m_pServer.OnSysError("",x);                
			}
			catch(Exception ex){
				m_pServer.OnSysError("",ex);
			}
		}

		#endregion


        #region method BeginRecieveCmd
		
		/// <summary>
		/// Starts recieveing command.
		/// </summary>
		private void BeginRecieveCmd()
		{
			MemoryStream strm = new MemoryStream();
			this.Socket.BeginReadLine(strm,1024,strm,new SocketCallBack(this.EndRecieveCmd));
		}

		#endregion

		#region method EndRecieveCmd

		/// <summary>
		/// Is called if command is recieved.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="exception"></param>
		/// <param name="count"></param>
		/// <param name="tag"></param>
		private void EndRecieveCmd(SocketCallBackResult result,long count,Exception exception,object tag)
		{
			try{
				switch(result)
				{
					case SocketCallBackResult.Ok:
						MemoryStream strm = (MemoryStream)tag;

						string cmdLine = System.Text.Encoding.Default.GetString(strm.ToArray());
                                                
						// Exceute command
						if(SwitchCommand(cmdLine)){
							// Session end, close session
							EndSession();
						}
						break;

					case SocketCallBackResult.LengthExceeded:
						this.Socket.WriteLine("-ERR Line too long.");

						BeginRecieveCmd();
						break;

					case SocketCallBackResult.SocketClosed:
						EndSession();
						break;

					case SocketCallBackResult.Exception:
						OnError(exception);
						break;
				}
			}
			catch(Exception x){
				 OnError(x);
			}
		}

		#endregion


        #region method SwitchCommand

		/// <summary>
		/// Parses and executes HTTP commmand.
		/// </summary>
		/// <param name="commandLine">Command line.</param>
		/// <returns>Returns true,if session must be terminated.</returns>
		private bool SwitchCommand(string commandLine)
		{
            /* RFC 2616 5.1 Request-Line

                The Request-Line begins with a method token, followed by the
                Request-URI and the protocol version, and ending with CRLF. The
                elements are separated by SP characters. No CR or LF is allowed
                except in the final CRLF sequence.

                Request-Line   = Method SP Request-URI SP HTTP-Version CRLF
            */

            string[] parts       = TextUtils.SplitQuotedString(commandLine,' ');
            string   method      = parts[0].ToUpper();
            string   uri         = parts[1];
            string   httpVersion = parts[2];

            //if(method == "OPTIONS"){
            //}
            if(method == "GET"){
            }/*
            else if(method == "HEAD"){
            }
            else if(method == "POST"){
            }
            else if(method == "PUT"){
            }
            else if(method == "DELETE"){
            }
            else if(method == "TRACE"){
            }
            else if(method == "CONNECT"){
            }*/
            else{
            }

            return false;
        }

        #endregion


        private void GET()
        {
        }

    }
}
