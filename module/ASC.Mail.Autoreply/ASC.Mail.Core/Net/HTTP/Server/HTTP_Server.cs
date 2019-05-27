/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace LumiSoft.Net.HTTP.Server
{
    /// <summary>
    /// HTTP server component.
    /// </summary>
    public class HTTP_Server : SocketServer
    {
        /// <summary>
		/// Defalut constructor.
		/// </summary>
		public HTTP_Server() : base()
		{
			this.BindInfo = new BindInfo[]{new BindInfo(IPAddress.Any,80,false,null)};
		}


        #region override InitNewSession

		/// <summary>
		/// Initialize and start new session here. Session isn't added to session list automatically, 
		/// session must add itself to server session list by calling AddSession().
		/// </summary>
		/// <param name="socket">Connected client socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
		protected override void InitNewSession(Socket socket,BindInfo bindInfo)
		{/*
            // Check maximum conncurent connections from 1 IP.
            if(m_MaxConnectionsPerIP > 0){
                lock(this.Sessions){
                    int nSessions = 0;
                    foreach(SocketServerSession s in this.Sessions){
                        IPEndPoint ipEndpoint = s.RemoteEndPoint;
                        if(ipEndpoint != null){
                            if(ipEndpoint.Address.Equals(((IPEndPoint)socket.RemoteEndPoint).Address)){
                                nSessions++;
                            }
                        }

                        // Maximum allowed exceeded
                        if(nSessions >= m_MaxConnectionsPerIP){
                            socket.Send(System.Text.Encoding.ASCII.GetBytes("-ERR Maximum connections from your IP address is exceeded, try again later !\r\n"));
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                            return;
                        }
                    }
                }
            }*/

            string   sessionID = Guid.NewGuid().ToString();
            SocketEx socketEx  = new SocketEx(socket);
            if(LogCommands){
                //socketEx.Logger = new SocketLogger(socket,this.SessionLog);
				//socketEx.Logger.SessionID = sessionID;
            }
			HTTP_Session session = new HTTP_Session(sessionID,socketEx,bindInfo,this);
		}

		#endregion

    }
}
