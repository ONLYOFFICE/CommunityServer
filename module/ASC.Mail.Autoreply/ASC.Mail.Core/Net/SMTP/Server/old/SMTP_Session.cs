/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;

using LumiSoft.Net;
using LumiSoft.Net.SMTP;
using LumiSoft.Net.AUTH;

namespace LumiSoft.Net.SMTP.Server
{
	/// <summary>
	/// SMTP Session.
	/// </summary>
	public class SMTP_Session : SocketServerSession
	{				
		private SMTP_Server        m_pServer          = null;
		private Stream             m_pMsgStream       = null;
        private SMTP_Cmd_Validator m_CmdValidator     = null;
        private long               m_BDAT_ReadedCount = 0;
		private string             m_EhloName         = "";      
		private string             m_Reverse_path     = "";      // Holds sender's reverse path.
		private Hashtable          m_Forward_path     = null;    // Holds Mail to.	
		private int                m_BadCmdCount      = 0;       // Holds number of bad commands.
		private BodyType           m_BodyType;
		private bool               m_BDat             = false;

		/// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sessionID">Session ID.</param>
        /// <param name="socket">Server connected socket.</param>
        /// <param name="bindInfo">BindInfo what accepted socket.</param>
        /// <param name="server">Reference to server.</param>
        internal SMTP_Session(string sessionID,SocketEx socket,IPBindInfo bindInfo,SMTP_Server server) : base(sessionID,socket,bindInfo,server)
        {	        
            m_pServer      = server;
			m_BodyType     = BodyType.x7_bit;
			m_Forward_path = new Hashtable();
			m_CmdValidator = new SMTP_Cmd_Validator();

			// Start session proccessing
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
				// Check if ip is allowed to connect this computer
				ValidateIP_EventArgs oArg = m_pServer.OnValidate_IpAddress(this);
				if(oArg.Validated){
                    //--- Dedicated SSL connection, switch to SSL -----------------------------------//
                    if(this.BindInfo.SslMode == SslMode.SSL){
                        try{
                            this.Socket.SwitchToSSL(this.BindInfo.Certificate);

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

					if(!string.IsNullOrEmpty(m_pServer.GreetingText)){
						this.Socket.WriteLine("220 " + m_pServer.GreetingText);
					}
					else{
						this.Socket.WriteLine("220 " + Net_Utils.GetLocalHostName(this.BindInfo.HostName) + " SMTP Server ready");
					}

					BeginRecieveCmd();
				}
				else{
					// There is user specified error text, send it to connected socket
					if(oArg.ErrorText.Length > 0){
						this.Socket.WriteLine(oArg.ErrorText);
					}

					EndSession();
				}
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
                try{
                    // Message storing not completed successfully, otherwise it must be null here. 
                    // This can happen if BDAT -> QUIT and LAST BDAT block wasn't sent or
                    // when session times out on DATA or BDAT command.
                    if(m_pMsgStream != null){
                        // We must call that method to notify Message stream owner to close/dispose that stream.
                        m_pServer.OnMessageStoringCompleted(this,"Message storing not completed successfully",m_pMsgStream);
                        m_pMsgStream = null;
                    }
                }
                catch{
                }

				if(this.Socket != null){
                    // Write logs to log file, if needed
				    if(m_pServer.LogCommands){
					    this.Socket.Logger.Flush();
				    }

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


        #region method Kill

        /// <summary>
        /// Kill this session.
        /// </summary>
        public override void Kill()
        {
            EndSession();
        }

        #endregion

		#region method OnSessionTimeout

		/// <summary>
		/// Is called by server when session has timed out.
		/// </summary>
		internal protected override void OnSessionTimeout()
		{
			try{
				this.Socket.WriteLine("421 Session timeout, closing transmission channel");
			}
			catch{
			}

			EndSession();
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
					// Client disconnected without shutting down.
					if(socketException.ErrorCode == 10054 || socketException.ErrorCode == 10053){
						if(m_pServer.LogCommands){
							this.Socket.Logger.AddTextEntry("Client aborted/disconnected");
						}

						EndSession();

						// Exception handled, return
						return;
					}
                    // Connection timed out.
                    else if(socketException.ErrorCode == 10060){
                        if(m_pServer.LogCommands){
							this.Socket.Logger.AddTextEntry("Connection timeout.");
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
		/// <param name="count"></param>
		/// <param name="exception"></param>
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
						this.Socket.WriteLine("500 Line too long.");

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
            catch(ReadException x){
                if(x.ReadReplyCode == ReadReplyCode.LengthExceeded){
			        this.Socket.WriteLine("500 Line too long.");
                
					BeginRecieveCmd();
				}
                else if(x.ReadReplyCode == ReadReplyCode.SocketClosed){
					EndSession();
				}
                else if(x.ReadReplyCode == ReadReplyCode.UnKnownError){
					OnError(x);
				}
            }
			catch(Exception x){
				 OnError(x);
			}
		}

		#endregion

		
		#region method SwitchCommand

		/// <summary>
		/// Executes SMTP command.
		/// </summary>
		/// <param name="SMTP_commandTxt">Original command text.</param>
		/// <returns>Returns true if must end session(command loop).</returns>
		private bool SwitchCommand(string SMTP_commandTxt)
		{
			//---- Parse command --------------------------------------------------//
			string[] cmdParts = SMTP_commandTxt.TrimStart().Split(new char[]{' '});
			string SMTP_command = cmdParts[0].ToUpper().Trim();
			string argsText = Core.GetArgsText(SMTP_commandTxt,SMTP_command);
			//---------------------------------------------------------------------//

			bool getNextCmd = true;

			switch(SMTP_command)
			{
				case "HELO":
					HELO(argsText);
					getNextCmd = false;
					break;

				case "EHLO":
					EHLO(argsText);
					getNextCmd = false;
					break;

                case "STARTTLS":
					STARTTLS(argsText);
					getNextCmd = false;
					break;

				case "AUTH":
					AUTH(argsText);
					break;

				case "MAIL":
					MAIL(argsText);
					getNextCmd = false;
					break;
					
				case "RCPT":
					RCPT(argsText);
					getNextCmd = false;
					break;

				case "DATA":
					BeginDataCmd(argsText);
					getNextCmd = false;
					break;

				case "BDAT":
					BeginBDATCmd(argsText);
					getNextCmd =  false;
					break;

				case "RSET":
					RSET(argsText);
					getNextCmd = false;
					break;

			//	case "VRFY":
			//		VRFY();
			//		break;

			//	case "EXPN":
			//		EXPN();
			//		break;

				case "HELP":
					HELP();
					break;

				case "NOOP":
					NOOP();
					getNextCmd = false;
				break;
				
				case "QUIT":
					QUIT(argsText);
					getNextCmd = false;
					return true;
										
				default:					
					this.Socket.WriteLine("500 command unrecognized");

					//---- Check that maximum bad commands count isn't exceeded ---------------//
					if(m_BadCmdCount > m_pServer.MaxBadCommands-1){
						this.Socket.WriteLine("421 Too many bad commands, closing transmission channel");
						return true;
					}
					m_BadCmdCount++;
					//-------------------------------------------------------------------------//

					break;				
			}

			if(getNextCmd){
				BeginRecieveCmd();
			}
			
			return false;
		}

		#endregion


		#region method HELO

		private void HELO(string argsText)
		{
			/* Rfc 2821 4.1.1.1
			    These commands, and a "250 OK" reply to one of them, confirm that
			    both the SMTP client and the SMTP server are in the initial state,
			    that is, there is no transaction in progress and all state tables and
			    buffers are cleared.
    		    
                Arguments:
                     Host name.
             
			    Syntax:
				     "HELO" SP Domain CRLF
			*/

			m_EhloName = argsText;

			ResetState();

			this.Socket.BeginWriteLine("250 " + Net_Utils.GetLocalHostName(this.BindInfo.HostName) + " Hello [" + this.RemoteEndPoint.Address.ToString() + "]",new SocketCallBack(this.EndSend));
			m_CmdValidator.Helo_ok = true;
		}

		#endregion

		#region method EHLO

		private void EHLO(string argsText)
		{		
			/* Rfc 2821 4.1.1.1
			These commands, and a "250 OK" reply to one of them, confirm that
			both the SMTP client and the SMTP server are in the initial state,
			that is, there is no transaction in progress and all state tables and
			buffers are cleared.
			*/

			m_EhloName = argsText;

			ResetState();

			//--- Construct supported AUTH types value ----------------------------//
			string authTypesString = "";
			if((m_pServer.SupportedAuthentications & SaslAuthTypes.Login) != 0){
				authTypesString += "LOGIN ";
			}
			if((m_pServer.SupportedAuthentications & SaslAuthTypes.Cram_md5) != 0){
				authTypesString += "CRAM-MD5 ";
			}
			if((m_pServer.SupportedAuthentications & SaslAuthTypes.Digest_md5) != 0){
				authTypesString += "DIGEST-MD5 ";
			}
			authTypesString = authTypesString.Trim();
			//-----------------------------------------------------------------------//

			string reply = "";
				reply += "250-" + Net_Utils.GetLocalHostName(this.BindInfo.HostName) + " Hello [" + this.RemoteEndPoint.Address.ToString() + "]\r\n";
				reply += "250-PIPELINING\r\n";
				reply += "250-SIZE " + m_pServer.MaxMessageSize + "\r\n";
		//		reply += "250-DSN\r\n";
		//		reply += "250-HELP\r\n";
				reply += "250-8BITMIME\r\n";
				reply += "250-BINARYMIME\r\n";
				reply += "250-CHUNKING\r\n";
				if(authTypesString.Length > 0){	
					reply += "250-AUTH " + authTypesString +  "\r\n";
				}
                if(!this.Socket.SSL && this.BindInfo.Certificate != null){
                    reply += "250-STARTTLS\r\n";
                }
			    reply += "250 Ok\r\n";
			
			this.Socket.BeginWriteLine(reply,null,new SocketCallBack(this.EndSend));
				
			m_CmdValidator.Helo_ok = true;
		}

		#endregion

        #region method STARTTLS

        private void STARTTLS(string argsText)
        {
            /* RFC 2487 STARTTLS 5. STARTTLS Command.
                The format for the STARTTLS command is:

               STARTTLS

               with no parameters.

               After the client gives the STARTTLS command, the server responds with
               one of the following reply codes:

               220 Ready to start TLS
               501 Syntax error (no parameters allowed)
               454 TLS not available due to temporary reason
             
               5.2 Result of the STARTTLS Command
                    Upon completion of the TLS handshake, the SMTP protocol is reset to
                    the initial state (the state in SMTP after a server issues a 220
                    service ready greeting).             
            */

            if(this.Socket.SSL){
                this.Socket.WriteLine("500 TLS already started !");
				return;
            }
                        
            if(this.BindInfo.Certificate == null){
                this.Socket.WriteLine("454 TLS not available, SSL certificate isn't specified !");
				return;
            }

            this.Socket.WriteLine("220 Ready to start TLS");

            try{               
                this.Socket.SwitchToSSL(this.BindInfo.Certificate);

                if(m_pServer.LogCommands){
                    this.Socket.Logger.AddTextEntry("TLS negotiation completed successfully.");
                }
            }
            catch(Exception x){
                this.Socket.WriteLine("500 TLS handshake failed ! " + x.Message);
            }

            ResetState();

            BeginRecieveCmd();
        }

        #endregion

        #region method AUTH

        private void AUTH(string argsText)
		{
			/* Rfc 2554 AUTH --------------------------------------------------//
			Restrictions:
		         After an AUTH command has successfully completed, no more AUTH
				 commands may be issued in the same session.  After a successful
				 AUTH command completes, a server MUST reject any further AUTH
				 commands with a 503 reply.
				 
			Remarks: 
				If an AUTH command fails, the server MUST behave the same as if
				the client had not issued the AUTH command.
			*/
			if(this.Authenticated){
				this.Socket.WriteLine("503 already authenticated");
				return;
			}
			
            try{				
			    //------ Parse parameters -------------------------------------//
			    string userName = "";
			    string password = "";
			    AuthUser_EventArgs aArgs = null;
    
	    		string[] param = argsText.Split(new char[]{' '});
		    	switch(param[0].ToUpper())
			    {
				    case "PLAIN":
					    this.Socket.WriteLine("504 Unrecognized authentication type.");
					    break;

				    case "LOGIN":

					    #region LOGIN authentication

				        //---- AUTH = LOGIN ------------------------------
					    /* Login
					        C: AUTH LOGIN
					        S: 334 VXNlcm5hbWU6
					        C: username_in_base64
	    			        S: 334 UGFzc3dvcmQ6
		        			C: password_in_base64
                        
                            or (initial-response argument included to avoid one 334 server response)
                    
                            C: AUTH LOGIN username_in_base64
					        S: 334 UGFzc3dvcmQ6
					        C: password_in_base64
                    
					
					        VXNlcm5hbWU6 base64_decoded= USERNAME
					        UGFzc3dvcmQ6 base64_decoded= PASSWORD
					    */
					    // Note: all strings are base64 strings eg. VXNlcm5hbWU6 = UserName.
			
                        // No user name included (initial-response argument)
                        if(param.Length == 1){
                            // Query UserName
					        this.Socket.WriteLine("334 VXNlcm5hbWU6");

					        string userNameLine = this.Socket.ReadLine();
					        // Encode username from base64
					        if(userNameLine.Length > 0){
						        userName = System.Text.Encoding.Default.GetString(Convert.FromBase64String(userNameLine));
					        }
                        }
                        // User name included, use it
                        else{
                            userName = System.Text.Encoding.Default.GetString(Convert.FromBase64String(param[1]));
                        }					
											
					    // Query Password
					    this.Socket.WriteLine("334 UGFzc3dvcmQ6");

					    string passwordLine = this.Socket.ReadLine();
					    // Encode password from base64
					    if(passwordLine.Length > 0){
						    password = System.Text.Encoding.Default.GetString(Convert.FromBase64String(passwordLine));
					    }
					
					    aArgs = m_pServer.OnAuthUser(this,userName,password,"",AuthType.Plain);
					    if(aArgs.Validated){
						    this.Socket.WriteLine("235 Authentication successful.");
						
						    this.SetUserName(userName);
					    }
					    else{
						    this.Socket.WriteLine("535 Authentication failed");
					    }

    					#endregion

					    break;

				    case "CRAM-MD5":
					
					    #region CRAM-MD5 authentication

					    /* Cram-M5
					        C: AUTH CRAM-MD5
					        S: 334 <md5_calculation_hash_in_base64>
					        C: base64(username password_hash)
					    */
					
					    string md5Hash = "<" + Guid.NewGuid().ToString().ToLower() + ">";
					    this.Socket.WriteLine("334 " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(md5Hash)));

					    string reply = this.Socket.ReadLine();

					    reply = System.Text.Encoding.Default.GetString(Convert.FromBase64String(reply));
					    string[] replyArgs = reply.Split(' ');
					    userName = replyArgs[0];
					
					    aArgs = m_pServer.OnAuthUser(this,userName,replyArgs[1],md5Hash,AuthType.CRAM_MD5);
					    if(aArgs.Validated){
						    this.Socket.WriteLine("235 Authentication successful.");
						
						    this.SetUserName(userName);
					    }
					    else{
						    this.Socket.WriteLine("535 Authentication failed");
					    }

    					#endregion

	    				break;

		    		case "DIGEST-MD5":
					
			    		#region DIGEST-MD5 authentication

				    	/* RFC 2831 AUTH DIGEST-MD5
					     * 
					    * Example:
					    * 
					    * C: AUTH DIGEST-MD5
					    * S: 334 base64(realm="elwood.innosoft.com",nonce="OA6MG9tEQGm2hh",qop="auth",algorithm=md5-sess)
					    * C: base64(username="chris",realm="elwood.innosoft.com",nonce="OA6MG9tEQGm2hh",
					    *    nc=00000001,cnonce="OA6MHXh6VqTrRk",digest-uri="smtp/elwood.innosoft.com",
                        *    response=d388dad90d4bbd760a152321f2143af7,qop=auth)
					    * S: 334 base64(rspauth=ea40f60335c427b5527b84dbabcdfffd)
					    * C:
					    * S: 235 Authentication successful.
					    */
                    
                        string nonce  = Auth_HttpDigest.CreateNonce();
                        string opaque = Auth_HttpDigest.CreateOpaque(); 
                        Auth_HttpDigest digest = new Auth_HttpDigest(this.BindInfo.HostName,nonce,opaque);
                        digest.Algorithm = "md5-sess";
                                        
                        this.Socket.WriteLine("334 " + AuthHelper.Base64en(digest.ToChallange(false)));

                        string clientResponse = AuthHelper.Base64de(this.Socket.ReadLine());
                        digest = new Auth_HttpDigest(clientResponse,"AUTHENTICATE");

                        // Check that realm,nonce and opaque in client response are same as we specified.
                        if(this.BindInfo.HostName != digest.Realm){
                            this.Socket.WriteLine("535 Authentication failed, 'realm' won't match.");
                            return;
                        }
                        if(nonce != digest.Nonce){
                            this.Socket.WriteLine("535 Authentication failed, 'nonce' won't match.");
                            return;
                        }
                        if(opaque != digest.Opaque){
                            this.Socket.WriteLine("535 Authentication failed, 'opaque' won't match.");
                            return;
                        }

                        userName = digest.UserName;
                    					
                        aArgs = m_pServer.OnAuthUser(this,userName,digest.Response,clientResponse,AuthType.DIGEST_MD5);
					    if(aArgs.Validated){
    					    // Send server computed password hash
	    					this.Socket.WriteLine("334 " + AuthHelper.Base64en("rspauth=" + aArgs.ReturnData));
					
		    				// We must got empty line here
			    			clientResponse = this.Socket.ReadLine();
				    		if(clientResponse == ""){
					    		this.Socket.WriteLine("235 Authentication successful.");
								
						    	this.SetUserName(userName);
						    }
						    else{
							    this.Socket.WriteLine("535 Authentication failed, unexpected client response.");
						    }
					    }
					    else{
						    this.Socket.WriteLine("535 Authentication failed.");
					    }

					    #endregion
                    
					    break;

				    default:
					    this.Socket.WriteLine("504 Unrecognized authentication type.");
					    break;
			    }
			    //-----------------------------------------------------------------//
            }
            catch{
                this.Socket.WriteLine("535 Authentication failed.");
            }
		}

		#endregion

		#region method MAIL

		private void MAIL(string argsText)
		{
			/* RFC 2821 3.3
			NOTE:
				This command tells the SMTP-receiver that a new mail transaction is
				starting and to reset all its state tables and buffers, including any
				recipients or mail data.  The <reverse-path> portion of the first or
				only argument contains the source mailbox (between "<" and ">"
				brackets), which can be used to report errors (see section 4.2 for a
				discussion of error reporting).  If accepted, the SMTP server returns
				 a 250 OK reply.
				 
				MAIL FROM:<reverse-path> [SP <mail-parameters> ] <CRLF>
				reverse-path = "<" [ A-d-l ":" ] Mailbox ">"
				Mailbox = Local-part "@" Domain
				
				body-value ::= "7BIT" / "8BITMIME" / "BINARYMIME"
				
				Examples:
					C: MAIL FROM:<ned@thor.innosoft.com>
					C: MAIL FROM:<ned@thor.innosoft.com> SIZE=500000 BODY=8BITMIME AUTH=xxxx
			*/

			if(!m_CmdValidator.MayHandle_MAIL){
				if(m_CmdValidator.MailFrom_ok){
					this.Socket.BeginWriteLine("503 Sender already specified",new SocketCallBack(this.EndSend));
				}
				else{
					this.Socket.BeginWriteLine("503 Bad sequence of commands",new SocketCallBack(this.EndSend));
				}
				return;
			}

			//------ Parse parameters -------------------------------------------------------------------//
			string   senderEmail  = "";
			long     messageSize  = 0;
			BodyType bodyType     = BodyType.x7_bit;
			bool     isFromParam  = false;

			// Parse while all params parsed or while is breaked
			while(argsText.Length > 0){
				if(argsText.ToLower().StartsWith("from:")){
					// Remove from:
                    argsText = argsText.Substring(5).Trim();

					// If there is more parameters
					if(argsText.IndexOf(" ") > -1){
						senderEmail = argsText.Substring(0,argsText.IndexOf(" "));
						argsText = argsText.Substring(argsText.IndexOf(" ")).Trim();
					}
					else{
						senderEmail = argsText;
						argsText = "";
					}

					// If address between <>, remove <>
					if(senderEmail.StartsWith("<") && senderEmail.EndsWith(">")){
						senderEmail = senderEmail.Substring(1,senderEmail.Length - 2);
					}

					isFromParam = true;
				}
				else if(argsText.ToLower().StartsWith("size=")){
					// Remove size=
					argsText = argsText.Substring(5).Trim();

					string sizeS = "";
					// If there is more parameters
					if(argsText.IndexOf(" ") > -1){
						sizeS = argsText.Substring(0,argsText.IndexOf(" "));
						argsText  = argsText.Substring(argsText.IndexOf(" ")).Trim();
					}
					else{
						sizeS = argsText;
						argsText  = "";
					}

					// See if value ok
					if(Core.IsNumber(sizeS)){
						messageSize = Convert.ToInt64(sizeS);
					}
					else{
						this.Socket.BeginWriteLine("501 SIZE parameter value is invalid. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}",new SocketCallBack(this.EndSend));
						return;
					}
				}
				else if(argsText.ToLower().StartsWith("body=")){
					// Remove body=
					argsText = argsText.Substring(5).Trim();

					string bodyTypeS = "";
					// If there is more parameters
					if(argsText.IndexOf(" ") > -1){
						bodyTypeS = argsText.Substring(0,argsText.IndexOf(" "));
						argsText = argsText.Substring(argsText.IndexOf(" ")).Trim();
					}
					else{
						bodyTypeS = argsText;
						argsText = "";
					}

					// See if value ok
					switch(bodyTypeS.ToUpper())
					{						
						case "7BIT":
							bodyType = BodyType.x7_bit;
							break;
						case "8BITMIME":
							bodyType = BodyType.x8_bit;
							break;
						case "BINARYMIME":
							bodyType = BodyType.binary;									
							break;
						default:
							this.Socket.BeginWriteLine("501 BODY parameter value is invalid. Syntax:{MAIL FROM:<address> [BODY=(7BIT/8BITMIME)]}",new SocketCallBack(this.EndSend));
							return;					
					}
				}
				else if(argsText.ToLower().StartsWith("auth=")){
					// Currently just eat AUTH keyword

					// Remove auth=
					argsText = argsText.Substring(5).Trim();

					string authS = "";
					// If there is more parameters
					if(argsText.IndexOf(" ") > -1){
						authS = argsText.Substring(0,argsText.IndexOf(" "));
						argsText  = argsText.Substring(argsText.IndexOf(" ")).Trim();
					}
					else{
						authS = argsText;
						argsText  = "";
					}
				}
				else{
					this.Socket.BeginWriteLine("501 Error in parameters. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}",new SocketCallBack(this.EndSend));
					return;
				}
			}
			
			// If required parameter 'FROM:' is missing
			if(!isFromParam){
				this.Socket.BeginWriteLine("501 Required param FROM: is missing. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}",new SocketCallBack(this.EndSend));
				return;
			}
			//---------------------------------------------------------------------------------------------//
			
			//--- Check message size
			if(m_pServer.MaxMessageSize > messageSize){
				// Check if sender is ok
				ValidateSender_EventArgs eArgs = m_pServer.OnValidate_MailFrom(this,senderEmail,senderEmail);
				if(eArgs.Validated){															
					// See note above
					ResetState();

					// Store reverse path
					m_Reverse_path = senderEmail;
					m_CmdValidator.MailFrom_ok = true;

					//-- Store params
					m_BodyType = bodyType;

					this.Socket.BeginWriteLine("250 OK <" + senderEmail + "> Sender ok",new SocketCallBack(this.EndSend));
				}			
				else{
					if(eArgs.ErrorText != null && eArgs.ErrorText.Length > 0){
						this.Socket.BeginWriteLine("550 " + eArgs.ErrorText,new SocketCallBack(this.EndSend));
					}
					else{
						this.Socket.BeginWriteLine("550 You are refused to send mail here",new SocketCallBack(this.EndSend));
					}
				}
			}
			else{
				this.Socket.BeginWriteLine("552 Message exceeds allowed size",new SocketCallBack(this.EndSend));
			}			
		}

		#endregion

		#region method RCPT

		private void RCPT(string argsText)
		{
			/* RFC 2821 4.1.1.3 RCPT
			NOTE:
				This command is used to identify an individual recipient of the mail
				data; multiple recipients are specified by multiple use of this
				command.  The argument field contains a forward-path and may contain
				optional parameters.
				
				Relay hosts SHOULD strip or ignore source routes, and
				names MUST NOT be copied into the reverse-path.  
				
				Example:
					RCPT TO:<@hosta.int,@jkl.org:userc@d.bar.org>

					will normally be sent directly on to host d.bar.org with envelope
					commands

					RCPT TO:<userc@d.bar.org>
					RCPT TO:<userc@d.bar.org> SIZE=40000
						
				RCPT TO:<forward-path> [ SP <rcpt-parameters> ] <CRLF>			
			*/

			/* RFC 2821 3.3
				If a RCPT command appears without a previous MAIL command, 
				the server MUST return a 503 "Bad sequence of commands" response.
			*/
			if(!m_CmdValidator.MayHandle_RCPT || m_BDat){
				this.Socket.BeginWriteLine("503 Bad sequence of commands",new SocketCallBack(this.EndSend));
				return;
			}

			// Check that recipient count isn't exceeded
			if(m_Forward_path.Count > m_pServer.MaxRecipients){
				this.Socket.BeginWriteLine("452 Too many recipients",new SocketCallBack(this.EndSend));
				return;
			}

			//------ Parse parameters -------------------------------------------------------------------//
			string recipientEmail = "";
			long   messageSize    = 0;
			bool   isToParam      = false;

			// Parse while all params parsed or while is breaked
			while(argsText.Length > 0){
				if(argsText.ToLower().StartsWith("to:")){
					// Remove to:
                    argsText = argsText.Substring(3).Trim();

					// If there is more parameters
					if(argsText.IndexOf(" ") > -1){
						recipientEmail = argsText.Substring(0,argsText.IndexOf(" "));
						argsText = argsText.Substring(argsText.IndexOf(" ")).Trim();
					}
					else{
						recipientEmail = argsText;
						argsText = "";
					}

					// If address between <>, remove <>
					if(recipientEmail.StartsWith("<") && recipientEmail.EndsWith(">")){
						recipientEmail = recipientEmail.Substring(1,recipientEmail.Length - 2);
					}

					// See if value ok
					if(recipientEmail.Length == 0){
						this.Socket.BeginWriteLine("501 Recipient address isn't specified. Syntax:{RCPT TO:<address> [SIZE=msgSize]}",new SocketCallBack(this.EndSend));
						return;
					}

					isToParam = true;
				}
				else if(argsText.ToLower().StartsWith("size=")){
					// Remove size=
					argsText = argsText.Substring(5).Trim();

					string sizeS = "";
					// If there is more parameters
					if(argsText.IndexOf(" ") > -1){
						sizeS = argsText.Substring(0,argsText.IndexOf(" "));
						argsText  = argsText.Substring(argsText.IndexOf(" ")).Trim();
					}
					else{
						sizeS = argsText;
						argsText  = "";
					}

					// See if value ok
					if(Core.IsNumber(sizeS)){
						messageSize = Convert.ToInt64(sizeS);
					}
					else{
						this.Socket.BeginWriteLine("501 SIZE parameter value is invalid. Syntax:{RCPT TO:<address> [SIZE=msgSize]}",new SocketCallBack(this.EndSend));
						return;
					}
				}
				else{
					this.Socket.BeginWriteLine("501 Error in parameters. Syntax:{RCPT TO:<address> [SIZE=msgSize]}",new SocketCallBack(this.EndSend));
					return;
				}
			}
			
			// If required parameter 'TO:' is missing
			if(!isToParam){
				this.Socket.BeginWriteLine("501 Required param TO: is missing. Syntax:<RCPT TO:{address> [SIZE=msgSize]}",new SocketCallBack(this.EndSend));
				return;
			}
			//---------------------------------------------------------------------------------------------//

			// Check message size
			if(m_pServer.MaxMessageSize > messageSize){
				// Check if email address is ok
				ValidateRecipient_EventArgs rcpt_args = m_pServer.OnValidate_MailTo(this,recipientEmail,recipientEmail,this.Authenticated);
				if(rcpt_args.Validated){
					// Check if mailbox size isn't exceeded
					if(m_pServer.Validate_MailBoxSize(this,recipientEmail,messageSize)){
						// Store reciptient
						if(!m_Forward_path.Contains(recipientEmail)){
							m_Forward_path.Add(recipientEmail,recipientEmail);
						}				
						m_CmdValidator.RcptTo_ok = true;

						this.Socket.BeginWriteLine("250 OK <" + recipientEmail + "> Recipient ok",new SocketCallBack(this.EndSend));						
					}
					else{					
						this.Socket.BeginWriteLine("552 Mailbox size limit exceeded",new SocketCallBack(this.EndSend));
					}
				}
				// Recipient rejected
				else{
					if(rcpt_args.LocalRecipient){
						this.Socket.BeginWriteLine("550 <" + recipientEmail + "> No such user here",new SocketCallBack(this.EndSend));
					}
					else{
						this.Socket.BeginWriteLine("550 <" + recipientEmail + "> Relay not allowed",new SocketCallBack(this.EndSend));
					}
				}
			}
			else{
				this.Socket.BeginWriteLine("552 Message exceeds allowed size",new SocketCallBack(this.EndSend));
			}
		}

		#endregion

		#region method DATA

		#region method BeginDataCmd

		private void BeginDataCmd(string argsText)
		{	
			/* RFC 2821 4.1.1
			NOTE:
				Several commands (RSET, DATA, QUIT) are specified as not permitting
				parameters.  In the absence of specific extensions offered by the
				server and accepted by the client, clients MUST NOT send such
				parameters and servers SHOULD reject commands containing them as
				having invalid syntax.
			*/

			if(argsText.Length > 0){
				this.Socket.BeginWriteLine("500 Syntax error. Syntax:{DATA}",new SocketCallBack(this.EndSend));
				return;
			}


			/* RFC 2821 4.1.1.4 DATA
			NOTE:
				If accepted, the SMTP server returns a 354 Intermediate reply and
				considers all succeeding lines up to but not including the end of
				mail data indicator to be the message text.  When the end of text is
				successfully received and stored the SMTP-receiver sends a 250 OK
				reply.
				
				The mail data is terminated by a line containing only a period, that
				is, the character sequence "<CRLF>.<CRLF>" (see section 4.5.2).  This
				is the end of mail data indication.
					
				
				When the SMTP server accepts a message either for relaying or for
				final delivery, it inserts a trace record (also referred to
				interchangeably as a "time stamp line" or "Received" line) at the top
				of the mail data.  This trace record indicates the identity of the
				host that sent the message, the identity of the host that received
				the message (and is inserting this time stamp), and the date and time
				the message was received.  Relayed messages will have multiple time
				stamp lines.  Details for formation of these lines, including their
				syntax, is specified in section 4.4.
   
			*/


			/* RFC 2821 DATA
			NOTE:
				If there was no MAIL, or no RCPT, command, or all such commands
				were rejected, the server MAY return a "command out of sequence"
				(503) or "no valid recipients" (554) reply in response to the DATA
				command.
			*/
			if(!m_CmdValidator.MayHandle_DATA || m_BDat){
				this.Socket.BeginWriteLine("503 Bad sequence of commands",new SocketCallBack(this.EndSend));
				return;
			}

			if(m_Forward_path.Count == 0){
				this.Socket.BeginWriteLine("554 no valid recipients given",new SocketCallBack(this.EndSend));
				return;
			}

            // Get message store stream
            GetMessageStoreStream_eArgs eArgs = m_pServer.OnGetMessageStoreStream(this);
            m_pMsgStream = eArgs.StoreStream;

			// reply: 354 Start mail input
			this.Socket.WriteLine("354 Start mail input; end with <CRLF>.<CRLF>");

			//---- Construct server headers for message----------------------------------------------------------------//
			string header  = "Received: from " + Core.GetHostName(this.RemoteEndPoint.Address) + " (" + this.RemoteEndPoint.Address.ToString() + ")\r\n"; 
			header += "\tby " + this.BindInfo.HostName + " with SMTP; " + DateTime.Now.ToUniversalTime().ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\r\n";
					    
			byte[] headers = System.Text.Encoding.ASCII.GetBytes(header);
			m_pMsgStream.Write(headers,0,headers.Length);
			//---------------------------------------------------------------------------------------------------------//

            // Begin recieving data
            this.Socket.BeginReadPeriodTerminated(m_pMsgStream,m_pServer.MaxMessageSize,null,new SocketCallBack(this.EndDataCmd));	
		}

		#endregion

		#region method EndDataCmd

		/// <summary>
		/// Is called when DATA command is finnished.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="count"></param>
		/// <param name="exception"></param>
		/// <param name="tag"></param>
		private void EndDataCmd(SocketCallBackResult result,long count,Exception exception,object tag)
		{
			try{
				switch(result)
				{
					case SocketCallBackResult.Ok:								
						// Notify Message stream owner that message storing completed ok.
                        MessageStoringCompleted_eArgs oArg = m_pServer.OnMessageStoringCompleted(this,null,m_pMsgStream);
						if(oArg.ServerReply.ErrorReply){								
							this.Socket.BeginWriteLine(oArg.ServerReply.ToSmtpReply("500","Error storing message"),new SocketCallBack(this.EndSend));
						}
						else{
							this.Socket.BeginWriteLine(oArg.ServerReply.ToSmtpReply("250","OK"),new SocketCallBack(this.EndSend));
						}												
						break;

					case SocketCallBackResult.LengthExceeded:
                        // We must call that method to notify Message stream owner to close/dispose that stream.
                        m_pServer.OnMessageStoringCompleted(this,"Requested mail action aborted: exceeded storage allocation",m_pMsgStream);

						this.Socket.BeginWriteLine("552 Requested mail action aborted: exceeded storage allocation",new SocketCallBack(this.EndSend));
						break;

					case SocketCallBackResult.SocketClosed:
                        if(m_pMsgStream != null){
                            // We must call that method to notify Message stream owner to close/dispose that stream.
                            m_pServer.OnMessageStoringCompleted(this,"SocketClosed",m_pMsgStream);
                            m_pMsgStream = null;
                        }
                        // Stream is already closed, probably by the EndSession method, do nothing.
                        //else{
                        //}

						EndSession();
						break;

					case SocketCallBackResult.Exception:
                        if(m_pMsgStream != null){
                            // We must call that method to notify Message stream owner to close/dispose that stream.
                            m_pServer.OnMessageStoringCompleted(this,"Exception: " + exception.Message,m_pMsgStream);
                            m_pMsgStream = null;
                        }
                        // Stream is already closed, probably by the EndSession method, do nothing.
                        //else{
                        //}

						OnError(exception);
						break;
				}

				/* RFC 2821 4.1.1.4 DATA
					NOTE:
						Receipt of the end of mail data indication requires the server to
						process the stored mail transaction information.  This processing
						consumes the information in the reverse-path buffer, the forward-path
						buffer, and the mail data buffer, and on the completion of this
						command these buffers are cleared.
				*/
				ResetState();
			}
			catch(Exception x){
				OnError(x);
			}
		}

		#endregion
		
		#endregion

		#region function BDAT

		#region method BeginBDATCmd

		private void BeginBDATCmd(string argsText)
		{
			/*RFC 3030 2
				The BDAT verb takes two arguments.The first argument indicates the length, 
                in octets, of the binary data chunk. The second optional argument indicates 
                that the data chunk	is the last.
				
				The message data is sent immediately after the trailing <CR>
				<LF> of the BDAT command line.  Once the receiver-SMTP receives the
				specified number of octets, it will return a 250 reply code.

				The optional LAST parameter on the BDAT command indicates that this
				is the last chunk of message data to be sent.  The last BDAT command
				MAY have a byte-count of zero indicating there is no additional data
				to be sent.  Any BDAT command sent after the BDAT LAST is illegal and
				MUST be replied to with a 503 "Bad sequence of commands" reply code.
				The state resulting from this error is indeterminate.  A RSET command
				MUST be sent to clear the transaction before continuing.
				
				A 250 response MUST be sent to each successful BDAT data block within
				a mail transaction.

				bdat-cmd   ::= "BDAT" SP chunk-size [ SP end-marker ] CR LF
				chunk-size ::= 1*DIGIT
				end-marker ::= "LAST"
			*/

			if(!m_CmdValidator.MayHandle_BDAT){
				this.Socket.BeginWriteLine("503 Bad sequence of commands",new SocketCallBack(this.EndSend));
				return;
			}

			string[] param = argsText.Split(new char[]{' '});
			if(param.Length > 0 && param.Length < 3){				
				if(Core.IsNumber(param[0])){
                    int countToRead = Convert.ToInt32(param[0]);

					// LAST specified
					bool lastChunk = false;
					if(param.Length == 2){
						lastChunk = true;
					}
						
				    // First BDAT command call.
					if(!m_BDat){
                        // Get message store stream
                        GetMessageStoreStream_eArgs eArgs = m_pServer.OnGetMessageStoreStream(this);
                        m_pMsgStream = eArgs.StoreStream;

                        // Add header to first bdat block only
						//---- Construct server headers for message----------------------------------------------------------------//
						string header  = "Received: from " + Core.GetHostName(this.RemoteEndPoint.Address) + " (" + this.RemoteEndPoint.Address.ToString() + ")\r\n"; 
						header += "\tby " + this.BindInfo.HostName + " with SMTP; " + DateTime.Now.ToUniversalTime().ToString("r",System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\r\n";
					    
						byte[] headers = System.Text.Encoding.ASCII.GetBytes(header);
						m_pMsgStream.Write(headers,0,headers.Length);
						//---------------------------------------------------------------------------------------------------------//
					}

                    // Begin junking data, maximum allowed message size exceeded.
                    // BDAT comman is dummy, after commandline binary data is at once follwed,
                    // so server server must junk all data and then report error.
                    if((m_BDAT_ReadedCount + countToRead) > m_pServer.MaxMessageSize){
                        this.Socket.BeginReadSpecifiedLength(new JunkingStream(),countToRead,lastChunk,new SocketCallBack(this.EndBDatCmd));
                    }
                    // Begin reading data
                    else{
                        this.Socket.BeginReadSpecifiedLength(m_pMsgStream,countToRead,lastChunk,new SocketCallBack(this.EndBDatCmd));
                    }

					m_BDat = true;
				}
				else{
					this.Socket.BeginWriteLine("500 Syntax error. Syntax:{BDAT chunk-size [LAST]}",new SocketCallBack(this.EndSend));
				}
			}
			else{
				this.Socket.BeginWriteLine("500 Syntax error. Syntax:{BDAT chunk-size [LAST]}",new SocketCallBack(this.EndSend));
			}		
		}

		#endregion

		#region method EndBDatCmd

		private void EndBDatCmd(SocketCallBackResult result,long count,Exception exception,object tag)
		{
			try{
				switch(result)
				{
					case SocketCallBackResult.Ok:
                        m_BDAT_ReadedCount += count;

						// BDAT command completed, got all data junks
						if((bool)tag){
				            // Maximum allowed message size exceeded.
                            if((m_BDAT_ReadedCount) > m_pServer.MaxMessageSize){
                                m_pServer.OnMessageStoringCompleted(this,"Requested mail action aborted: exceeded storage allocation",m_pMsgStream);

                                this.Socket.BeginWriteLine("552 Requested mail action aborted: exceeded storage allocation",new SocketCallBack(this.EndSend));
                            }
                            else{
							    // Notify Message stream owner that message storing completed ok.
							    MessageStoringCompleted_eArgs oArg = m_pServer.OnMessageStoringCompleted(this,null,m_pMsgStream);
							    if(oArg.ServerReply.ErrorReply){								
								    this.Socket.BeginWriteLine(oArg.ServerReply.ToSmtpReply("500","Error storing message"),new SocketCallBack(this.EndSend));
							    }
							    else{
								    this.Socket.BeginWriteLine(oArg.ServerReply.ToSmtpReply("250","Message(" + m_BDAT_ReadedCount + " bytes) stored ok."),new SocketCallBack(this.EndSend));
							    }
                            }

							/* RFC 2821 4.1.1.4 DATA
							NOTE:
								Receipt of the end of mail data indication requires the server to
								process the stored mail transaction information.  This processing
								consumes the information in the reverse-path buffer, the forward-path
								buffer, and the mail data buffer, and on the completion of this
								command these buffers are cleared.
							*/
							ResetState();						
						}
                        // Got BDAT data block, BDAT must continue, that wasn't last data block.
						else{
                            // Maximum allowed message size exceeded.
                            if((m_BDAT_ReadedCount) > m_pServer.MaxMessageSize){
                                this.Socket.BeginWriteLine("552 Requested mail action aborted: exceeded storage allocation",new SocketCallBack(this.EndSend));
                            }
                            else{
							    this.Socket.BeginWriteLine("250 Data block of " + count + " bytes recieved OK.",new SocketCallBack(this.EndSend));
                            }
						}
						break;

					case SocketCallBackResult.SocketClosed:
                        if(m_pMsgStream != null){
                            // We must call that method to notify Message stream owner to close/dispose that stream.
                            m_pServer.OnMessageStoringCompleted(this,"SocketClosed",m_pMsgStream);
                            m_pMsgStream = null;
                        }
                        // Stream is already closed, probably by the EndSession method, do nothing.
                        //else{
                        //}

						EndSession();
						return;

					case SocketCallBackResult.Exception:
                        if(m_pMsgStream != null){
                            // We must call that method to notify Message stream owner to close/dispose that stream.
                            m_pServer.OnMessageStoringCompleted(this,"Exception: " + exception.Message,m_pMsgStream);
                            m_pMsgStream = null;
                        }
                        // Stream is already closed, probably by the EndSession method, do nothing.
                        //else{
                        //}

						OnError(exception);
						return;
				}
			}
			catch(Exception x){
				OnError(x);
			}
		}

		#endregion

		#endregion

		#region method RSET

		private void RSET(string argsText)
		{
			/* RFC 2821 4.1.1
			NOTE:
				Several commands (RSET, DATA, QUIT) are specified as not permitting
				parameters.  In the absence of specific extensions offered by the
				server and accepted by the client, clients MUST NOT send such
				parameters and servers SHOULD reject commands containing them as
				having invalid syntax.
			*/

			if(argsText.Length > 0){
				this.Socket.BeginWriteLine("500 Syntax error. Syntax:{RSET}",new SocketCallBack(this.EndSend));
				return;
			}

			/* RFC 2821 4.1.1.5 RESET (RSET)
			NOTE:
				This command specifies that the current mail transaction will be
				aborted.  Any stored sender, recipients, and mail data MUST be
				discarded, and all buffers and state tables cleared.  The receiver
				MUST send a "250 OK" reply to a RSET command with no arguments.
			*/

            try{
                // Message storing aborted by RSET. 
                // This can happen if BDAT -> RSET and LAST BDAT block wasn't sent.
                if(m_pMsgStream != null){
                    // We must call that method to notify Message stream owner to close/dispose that stream.
                    m_pServer.OnMessageStoringCompleted(this,"Message storing aborted by RSET",m_pMsgStream);
                    m_pMsgStream = null;
                }
            }
            catch{
            }
			
			ResetState();

			this.Socket.BeginWriteLine("250 OK",new SocketCallBack(this.EndSend));
		}

		#endregion

		#region method VRFY

		private void VRFY()
		{
			/* RFC 821 VRFY 
			Example:
				S: VRFY Lumi
				R: 250 Ivar Lumi <ivx@lumisoft.ee>
				
				S: VRFY lum
				R: 550 String does not match anything.			 
			*/

			// ToDo: Parse user, add new event for cheking user

			this.Socket.BeginWriteLine("502 Command not implemented",new SocketCallBack(this.EndSend));
		}

		#endregion

		#region mehtod NOOP

		private void NOOP()
		{
			/* RFC 2821 4.1.1.9 NOOP (NOOP)
			NOTE:
				This command does not affect any parameters or previously entered
				commands.  It specifies no action other than that the receiver send
				an OK reply.
			*/

			this.Socket.BeginWriteLine("250 OK",new SocketCallBack(this.EndSend));
		}

		#endregion

		#region method QUIT

		private void QUIT(string argsText)
		{
			/* RFC 2821 4.1.1
			NOTE:
				Several commands (RSET, DATA, QUIT) are specified as not permitting
				parameters.  In the absence of specific extensions offered by the
				server and accepted by the client, clients MUST NOT send such
				parameters and servers SHOULD reject commands containing them as
				having invalid syntax.
			*/

			if(argsText.Length > 0){
				this.Socket.BeginWriteLine("500 Syntax error. Syntax:<QUIT>",new SocketCallBack(this.EndSend));
				return;
			}

			/* RFC 2821 4.1.1.10 QUIT (QUIT)
			NOTE:
				This command specifies that the receiver MUST send an OK reply, and
				then close the transmission channel.
			*/

			// reply: 221 - Close transmission cannel
			this.Socket.WriteLine("221 Service closing transmission channel");
		//	this.Socket.BeginSendLine("221 Service closing transmission channel",null);	
		}

		#endregion


		//---- Optional commands
		
		#region function EXPN

		private void EXPN()
		{
			/* RFC 821 EXPN 
			NOTE:
				This command asks the receiver to confirm that the argument
				identifies a mailing list, and if so, to return the
				membership of that list.  The full name of the users (if
				known) and the fully specified mailboxes are returned in a
				multiline reply.
			
			Example:
				S: EXPN lsAll
				R: 250-ivar lumi <ivx@lumisoft.ee>
				R: 250-<willy@lumisoft.ee>
				R: 250 <kaido@lumisoft.ee>
			*/

			this.Socket.WriteLine("502 Command not implemented");
		}

		#endregion

		#region function HELP

		private void HELP()
		{
			/* RFC 821 HELP
			NOTE:
				This command causes the receiver to send helpful information
				to the sender of the HELP command.  The command may take an
				argument (e.g., any command name) and return more specific
				information as a response.
			*/

			this.Socket.WriteLine("502 Command not implemented");
		}

		#endregion


		#region method ResetState

		private void ResetState()
		{
			//--- Reset variables
			m_BodyType = BodyType.x7_bit;
			m_Forward_path.Clear();
			m_Reverse_path  = "";
	//		m_Authenticated = false; // Keep AUTH 
			m_CmdValidator.Reset();
			m_CmdValidator.Helo_ok = true;

			m_pMsgStream = null;
            m_BDat = false;
            m_BDAT_ReadedCount = 0;
		}

		#endregion


		#region function EndSend
		
		/// <summary>
		/// Is called when asynchronous send completes.
		/// </summary>
		/// <param name="result">If true, then send was successfull.</param>
		/// <param name="count">Count sended.</param>
		/// <param name="exception">Exception happend on send. NOTE: available only is result=false.</param>
		/// <param name="tag">User data.</param>
		private void EndSend(SocketCallBackResult result,long count,Exception exception,object tag)
		{
			try{
				switch(result)
				{
					case SocketCallBackResult.Ok:
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

        
		#region Properties Implementation
				
		/// <summary>
		/// Gets client reported EHLO/HELO name.
		/// </summary>
		public string EhloName
		{
			get{ return m_EhloName; }
		}

		/// <summary>
		/// Gets body type.
		/// </summary>
		public BodyType BodyType
		{
			get{ return m_BodyType; }
		}

		/// <summary>
		/// Gets sender.
		/// </summary>
		public string MailFrom
		{
			get{ return m_Reverse_path; }
		}

		/// <summary>
		/// Gets recipients.
		/// </summary>
		public string[] MailTo
		{
			get{
				string[] to = new string[m_Forward_path.Count];
				m_Forward_path.Values.CopyTo(to,0);

				return to; 
			}
		}

		#endregion

	}
}
