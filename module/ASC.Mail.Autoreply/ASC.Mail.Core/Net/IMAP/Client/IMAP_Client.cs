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


namespace ASC.Mail.Net.IMAP.Client
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Principal;
    using System.Timers;
    using IO;
    using Server;
    using TCP;
    using StringReader=StringReader;
    using System.Text;

    #endregion

    /// <summary>
    /// IMAP client.
    /// </summary>
    /// <example>
    /// <code>
    /// using(IMAP_Client c = new IMAP_Client()){
    ///		c.Connect("ivx",143);
    ///		c.Authenticate("test","test");
    ///				
    ///		c.SelectFolder("Inbox");
    ///				
    ///		IMAP_SequenceSet sequence_set = new IMAP_SequenceSet();
    ///		// First message
    ///		sequence_set.Parse("1");
    ///		// All messages
    ///	//  sequence_set.Parse("1:*");
    ///		// Messages 1,3,6 and 100 to last
    ///	//  sequence_set.Parse("1,3,6,100:*");
    ///	
    ///		// Get messages flags and header
    ///		IMAP_FetchItem msgsInfo = c.FetchMessages(sequence_set,IMAP_FetchItem_Flags.MessageFlags | IMAP_FetchItem_Flags.Header,true,false);
    ///		
    ///		// Do your suff
    ///	}
    /// </code>
    /// </example>
    public class IMAP_Client : TCP_Client
    {
        private const int NoopInterval = 60000;
        private const int NoopDelay = 5000;

        #region Members

        //private readonly TimerEx m_Timer = new TimerEx(NoopInterval);
        private int m_CmdNumber;
        private int m_MsgCount;
        private int m_NewMsgCount;
        private char m_PathSeparator = '\0';
        private GenericIdentity m_pAuthdUserIdentity;
        private string m_SelectedFolder = "";
        private long m_UIDNext;
        private long m_UIDValidity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets session authenticated user identity, returns null if not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and POP3 client is not connected.</exception>
        public override GenericIdentity AuthenticatedUserIdentity
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pAuthdUserIdentity;
            }
        }

        /// <summary>
        /// Gets IMAP server path separator char.
        /// </summary>
        public char PathSeparator
        {
            get
            {
                // Get separator
                if (m_PathSeparator == '\0')
                {
                    m_PathSeparator = GetFolderSeparator()[0];
                }

                return m_PathSeparator;
            }
        }

        /// <summary>
        /// Gets selected folder.
        /// </summary>
        public string SelectedFolder
        {
            get { return m_SelectedFolder; }
        }

        /// <summary>
        /// Gets folder UID.
        /// </summary>
        public long UIDValidity
        {
            get { return m_UIDValidity; }
        }

        /// <summary>
        /// Gets next predicted message UID.
        /// </summary>
        public long UIDNext
        {
            get { return m_UIDNext; }
        }

        /// <summary>
        /// Gets numbers of recent(not accessed messages) in selected folder.
        /// </summary>
        public int RecentMessagesCount
        {
            get { return NewMsgCount; }
        }

        /// <summary>
        /// Gets numbers of messages in selected folder.
        /// </summary>
        public int MessagesCount
        {
            get { return MsgCount; }
        }

        ///<summary>
        /// 
        ///</summary>
        public bool HasNewMessages { get; set; }

        private int MsgCount
        {
            get { return m_MsgCount; }
            set
            {
                if (value > m_MsgCount)
                {
                    HasNewMessages = true;
                    m_NewMsgCount = value - m_MsgCount;
                }
                m_MsgCount = value;
            }
        }

        private int NewMsgCount
        {
            get { return m_NewMsgCount; }
            set
            {
                HasNewMessages = m_NewMsgCount < value;
                m_NewMsgCount = value;
            }
        }

        public List<string> Capabilities
        {
            get { return capabilities; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_Client()
        {
            //m_Timer.AutoReset = true;
            //m_Timer.Elapsed += Timer_Elapsed;
            //m_Timer.Enabled = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public override void Dispose()
        {
            Debug.Print("IMAP client disposed");
            //m_Timer.Enabled = false;
            //m_Timer.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Closes connection to POP3 server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected.</exception>
        public override void Disconnect()
        {
            Debug.Print("IMAP client disconect");
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("IMAP client is not connected.");
            }

            try
            {
                // Send LOGOUT command to server.                
                int countWritten = TcpStream.WriteLine("a1 LOGOUT");
                LogAddWrite(countWritten, "a1 LOGOUT");
                string line = "";
                while (true)
                {
                    line = ReadLine();

                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }
                    if (line.StartsWith("a1 OK"))
                    {
                        break;
                    }
                }
            }
            catch {}

            try
            {
                base.Disconnect();
            }
            catch {}
        }

        private List<string> capabilities = new List<string>(); 

        private void GetCapabilities()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new Exception("You must connect first !");
            }

            string line = GetNextCmdTag() + " CAPABILITY";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            Capabilities.Clear();
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessCapsResponse(line);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        private void ProcessCapsResponse(string line)
        {
            if (line.IndexOf("CAPABILITY") > -1)
            {
                string[] idsLine = line.Substring(line.IndexOf(' ', line.IndexOf("CAPABILITY"))).Split(
                    new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Capabilities.AddRange(idsLine);
            }
        }

        /// <summary>
        /// Switches IMAP connection to SSL.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected or is authenticated or is already secure connection.</exception>
        public void StartTLS()
        {
            /* RFC 2595 3. IMAP STARTTLS extension.
             
                Example:    C: a001 CAPABILITY
                            S: * CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED
                            S: a001 OK CAPABILITY completed
                            C: a002 STARTTLS
                            S: a002 OK Begin TLS negotiation now
                            <TLS negotiation, further commands are under TLS layer>
            */

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "The STARTTLS command is only valid in non-authenticated state !");
            }
            if (IsSecureConnection)
            {
                throw new InvalidOperationException("Connection is already secure.");
            }

            string line = GetNextCmdTag() + " STARTTLS";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);
            TcpStream.ReadLine(args, false);
            if (args.Error != null)
            {
                throw args.Error;
            }
            line = args.LineUtf8;
            LogAddRead(args.BytesInBuffer, line);
            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            SwitchToSecure();
        }

        /// <summary>
        /// Authenticates user.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected or is already authenticated.</exception>
        public void Authenticate(string userName, string password)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException("Session is already authenticated.");
            }

            string line = GetNextCmdTag() + " LOGIN " + TextUtils.QuoteString(userName) + " " +
                          TextUtils.QuoteString(password);
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten,
                        string.IsNullOrEmpty(password) ? line : line.Replace(password, "<***REMOVED***>"));

            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);
            TcpStream.ReadLine(args, false);
            if (args.Error != null)
            {
                throw args.Error;
            }
            line = args.LineUtf8;
            LogAddRead(args.BytesInBuffer, line);
            line = RemoveCmdTag(line);
            if (line.ToUpper().StartsWith("OK"))
            {
                m_pAuthdUserIdentity = new GenericIdentity(userName, "login");
            }
            else
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Creates specified folder.
        /// </summary>
        /// <param name="folderName">Folder name. Eg. test, Inbox/SomeSubFolder. NOTE: use GetFolderSeparator() to get right folder separator.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void CreateFolder(string folderName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The CREATE command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " CREATE " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);
            TcpStream.ReadLine(args, false);
            if (args.Error != null)
            {
                throw args.Error;
            }
            line = args.LineUtf8;
            LogAddRead(args.BytesInBuffer, line);
            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Deletes specified folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void DeleteFolder(string folderName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The DELETE command is only valid in authenticated state.");
            }

            string line = GetNextCmdTag() + " DELETE " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);
            TcpStream.ReadLine(args, false);
            if (args.Error != null)
            {
                throw args.Error;
            }
            line = args.LineUtf8;
            LogAddRead(args.BytesInBuffer, line);
            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Renames specified folder.
        /// </summary>
        /// <param name="sourceFolderName">Source folder name.</param>
        /// <param name="destinationFolderName">Destination folder name.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void RenameFolder(string sourceFolderName, string destinationFolderName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The RENAME command is only valid in authenticated state.");
            }

            string line = GetNextCmdTag() + " RENAME " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(sourceFolderName)) + " " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(destinationFolderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);
            TcpStream.ReadLine(args, false);
            if (args.Error != null)
            {
                throw args.Error;
            }
            line = args.LineUtf8;
            LogAddRead(args.BytesInBuffer, line);
            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        ///  Gets all available folders.
        /// </summary>
        /// <returns>Returns user folders.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public string[] GetFolders()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The LIST command is only valid in authenticated state.");
            }

            List<string> list = new List<string>();

            string line = GetNextCmdTag() + " LIST \"\" \"*\"";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Must get lines with * and cmdTag + OK or cmdTag BAD/NO.
            while (true)
            {
                line = ReadLine();

                if (line.StartsWith("*"))
                {
                    // don't show not selectable folders
                    if (line.ToLower().IndexOf("\\noselect") == -1)
                    {
                        line = line.Substring(line.IndexOf(")") + 1).Trim(); // Remove * LIST(..)
                        line = line.Substring(line.IndexOf(" ")).Trim(); // Remove Folder separator

                        list.Add(TextUtils.UnQuoteString(Core.Decode_IMAP_UTF7_String(line.Trim())));
                    }
                }
                else
                {
                    break;
                }
            }

            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Gets all subscribed folders.
        /// </summary>
        /// <returns>Returns user subscribed folders.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public string[] GetSubscribedFolders()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The LSUB command is only valid in authenticated state.");
            }

            List<string> list = new List<string>();

            string line = GetNextCmdTag() + " LSUB \"\" \"*\"";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Must get lines with * and cmdTag + OK or cmdTag BAD/NO.
            while (true)
            {
                line = ReadLine();

                if (line.StartsWith("*"))
                {
                    // don't show not selectable folders
                    if (line.ToLower().IndexOf("\\noselect") == -1)
                    {
                        line = line.Substring(line.IndexOf(")") + 1).Trim(); // Remove * LIST(..)
                        line = line.Substring(line.IndexOf(" ")).Trim(); // Remove Folder separator

                        list.Add(TextUtils.UnQuoteString(Core.Decode_IMAP_UTF7_String(line.Trim())));
                    }
                }
                else
                {
                    break;
                }
            }

            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Subscribes specified folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void SubscribeFolder(string folderName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "The SUBSCRIBE command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " SUBSCRIBE " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            line = ReadLine();
            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// UnSubscribes specified folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void UnSubscribeFolder(string folderName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "The UNSUBSCRIBE command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " UNSUBSCRIBE " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            line = ReadLine();
            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Selects specified folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void ExamineFolder(string folderName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The SELECT command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " EXAMINE " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Must get lines with * and cmdTag + OK or cmdTag BAD/NO.
            while (true)
            {
                line = ReadLine();

                if (line.StartsWith("*"))
                {
                    // Get rid of *
                    line = line.Substring(1).Trim();

                    if (line.ToUpper().IndexOf("EXISTS") > -1 && line.ToUpper().IndexOf("FLAGS") == -1)
                    {
                        MsgCount = Convert.ToInt32(line.Substring(0, line.IndexOf(" ")).Trim());
                    }
                    else if (line.ToUpper().IndexOf("RECENT") > -1 && line.ToUpper().IndexOf("FLAGS") == -1)
                    {
                        NewMsgCount = Convert.ToInt32(line.Substring(0, line.IndexOf(" ")).Trim());
                    }
                    else if (line.ToUpper().IndexOf("UIDNEXT") > -1)
                    {
                        m_UIDNext =
                            Convert.ToInt64(line.Substring(line.ToUpper().IndexOf("UIDNEXT") + 8,
                                                           line.IndexOf(']') -
                                                           line.ToUpper().IndexOf("UIDNEXT") - 8));
                    }
                    else if (line.ToUpper().IndexOf("UIDVALIDITY") > -1)
                    {
                        m_UIDValidity =
                            Convert.ToInt64(line.Substring(line.ToUpper().IndexOf("UIDVALIDITY") + 12,
                                                           line.IndexOf(']') -
                                                           line.ToUpper().IndexOf("UIDVALIDITY") - 12));
                    }
                }
                else
                {
                    break;
                }
            }

            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Selects specified folder.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void SelectFolder(string folderName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The SELECT command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " SELECT " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Must get lines with * and cmdTag + OK or cmdTag BAD/NO.
            while (true)
            {
                line = ReadLine();

                if (line.StartsWith("*"))
                {
                    // Get rid of *
                    line = line.Substring(1).Trim();

                    if (line.ToUpper().IndexOf("EXISTS") > -1 && line.ToUpper().IndexOf("FLAGS") == -1)
                    {
                        MsgCount = Convert.ToInt32(line.Substring(0, line.IndexOf(" ")).Trim());
                    }
                    else if (line.ToUpper().IndexOf("RECENT") > -1 && line.ToUpper().IndexOf("FLAGS") == -1)
                    {
                        NewMsgCount = Convert.ToInt32(line.Substring(0, line.IndexOf(" ")).Trim());
                    }
                    else if (line.ToUpper().IndexOf("UIDNEXT") > -1)
                    {
                        m_UIDNext =
                            Convert.ToInt64(line.Substring(line.ToUpper().IndexOf("UIDNEXT") + 8,
                                                           line.IndexOf(']') -
                                                           line.ToUpper().IndexOf("UIDNEXT") - 8));
                    }
                    else if (line.ToUpper().IndexOf("UIDVALIDITY") > -1)
                    {
                        m_UIDValidity =
                            Convert.ToInt64(line.Substring(line.ToUpper().IndexOf("UIDVALIDITY") + 12,
                                                           line.IndexOf(']') -
                                                           line.ToUpper().IndexOf("UIDVALIDITY") - 12));
                    }
                }
                else
                {
                    break;
                }
            }

            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            m_SelectedFolder = folderName;
        }

        /// <summary>
        /// Gets specified folder quota info. Throws Exception if server doesn't support QUOTA.
        /// </summary>
        /// <param name="folder">Folder name.</param>
        /// <returns>Returns specified folder quota info.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public IMAP_Quota GetFolderQuota(string folder)
        {
            /* RFC 2087 4.3. GETQUOTAROOT Command

                    Arguments:  mailbox name

                    Data:       untagged responses: QUOTAROOT, QUOTA

                    Result:     OK - getquota completed
                                NO - getquota error: no such mailbox, permission denied
                                BAD - command unknown or arguments invalid

               The GETQUOTAROOT command takes the name of a mailbox and returns the
               list of quota roots for the mailbox in an untagged QUOTAROOT
               response.  For each listed quota root, it also returns the quota
               root's resource usage and limits in an untagged QUOTA response.

                   Example:    C: A003 GETQUOTAROOT INBOX
                               S: * QUOTAROOT INBOX ""
                               S: * QUOTA "" (STORAGE 10 512)
                               S: A003 OK Getquota completed
            */

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }

            IMAP_Quota retVal = null;

            // Ensure that we send right separator to server, we accept both \ and /.
            folder = folder.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " GETQUOTAROOT " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folder));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Must get lines with * and cmdTag + OK or cmdTag BAD/NO.
            while (true)
            {
                line = ReadLine();

                if (line.StartsWith("*"))
                {
                    // Get rid of *
                    line = line.Substring(1).Trim();

                    if (line.ToUpper().StartsWith("QUOTAROOT"))
                    {
                        // Skip QUOTAROOT
                    }
                    else if (line.ToUpper().StartsWith("QUOTA"))
                    {
                        StringReader r = new StringReader(line);
                        // Skip QUOTA word
                        r.ReadWord();

                        string qoutaRootName = r.ReadWord();
                        long storage = -1;
                        long maxStorage = -1;
                        long messages = -1;
                        long maxMessages = -1;

                        string limits = r.ReadParenthesized();
                        r = new StringReader(limits);
                        while (r.Available > 0)
                        {
                            string limitName = r.ReadWord();
                            // STORAGE usedBytes maximumAllowedBytes
                            if (limitName.ToUpper() == "STORAGE")
                            {
                                storage = Convert.ToInt64(r.ReadWord());
                                maxStorage = Convert.ToInt64(r.ReadWord());
                            }
                                // STORAGE messagesCount maximumAllowedMessages
                            else if (limitName.ToUpper() == "MESSAGE")
                            {
                                messages = Convert.ToInt64(r.ReadWord());
                                maxMessages = Convert.ToInt64(r.ReadWord());
                            }
                        }

                        retVal = new IMAP_Quota(qoutaRootName, messages, maxMessages, storage, maxStorage);
                    }
                }
                else
                {
                    break;
                }
            }

            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return retVal;
        }

        /// <summary>
        /// Gets IMAP server namespaces info.
        /// </summary>
        /// <returns>Returns user namespaces.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public IMAP_NamespacesInfo GetNamespacesInfo()
        {
            /* RFC 2342 5. NAMESPACE Command.
               Arguments: none

               Response:  an untagged NAMESPACE response that contains the prefix
                          and hierarchy delimiter to the server's Personal
                          Namespace(s), Other Users' Namespace(s), and Shared
                          Namespace(s) that the server wishes to expose. The
                          response will contain a NIL for any namespace class
                          that is not available. Namespace_Response_Extensions
                          MAY be included in the response.
                          Namespace_Response_Extensions which are not on the IETF
                          standards track, MUST be prefixed with an "X-".

               Result:    OK - Command completed
                          NO - Error: Can't complete command
                          BAD - argument invalid
             
             
                Example:
                    < A server that supports a single personal namespace.  No leading
                    prefix is used on personal mailboxes and "/" is the hierarchy
                    delimiter.>

                    C: A001 NAMESPACE
                    S: * NAMESPACE (("" "/")) NIL NIL
                    S: A001 OK NAMESPACE command completed             
            */

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "The NAMESPACE command is only valid in authenticated state.");
            }

            string line = GetNextCmdTag() + " NAMESPACE";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            IMAP_NamespacesInfo namespacesInfo = new IMAP_NamespacesInfo(null, null, null);
            while (true)
            {
                line = ReadLine();

                if (line.StartsWith("*"))
                {
                    line = RemoveCmdTag(line);

                    if (line.ToUpper().StartsWith("NAMESPACE"))
                    {
                        namespacesInfo = IMAP_NamespacesInfo.Parse(line);
                    }
                }
                else
                {
                    break;
                }
            }

            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return namespacesInfo;
        }

        /// <summary>
        /// Gets specified folder ACL entries.
        /// </summary>
        /// <param name="folderName">Folder which ACL entries to get.</param>
        /// <returns>Returns specified folder ACL entries.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public IMAP_Acl[] GetFolderACL(string folderName)
        {
            /* RFC 2086 4.3. GETACL
				Arguments:  mailbox name

				Data:       untagged responses: ACL

				Result:     OK - getacl completed
							NO - getacl failure: can't get acl
							BAD - command unknown or arguments invalid

					The GETACL command returns the access control list for mailbox in
					an untagged ACL reply.

				Example:    C: A002 GETACL INBOX
							S: * ACL INBOX Fred rwipslda
							S: A002 OK Getacl complete
							
							.... Multiple users
							S: * ACL INBOX Fred rwipslda test rwipslda
							
							.... No acl flags for Fred
							S: * ACL INBOX Fred "" test rwipslda
									
			*/

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The GETACL command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " GETACL " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            List<IMAP_Acl> retVal = new List<IMAP_Acl>();
            while (true)
            {
                line = ReadLine();

                if (line.StartsWith("*"))
                {
                    line = RemoveCmdTag(line);

                    if (line.ToUpper().StartsWith("ACL"))
                    {
                        retVal.Add(IMAP_Acl.Parse(line));
                    }
                }
                else
                {
                    break;
                }
            }

            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Sets specified user ACL permissions for specified folder.
        /// </summary>
        /// <param name="folderName">Folder name which ACL to set.</param>
        /// <param name="userName">User name who's ACL to set.</param>
        /// <param name="acl">ACL permissions to set.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void SetFolderACL(string folderName, string userName, IMAP_ACL_Flags acl)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The SETACL command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " SETACL " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName)) + " " +
                          TextUtils.QuoteString(userName) + " " + IMAP_Utils.ACL_to_String(acl);
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            line = ReadLine();
            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Deletes specified user access to specified folder.
        /// </summary>
        /// <param name="folderName">Folder which ACL to remove.</param>
        /// <param name="userName">User name who's ACL to remove.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public void DeleteFolderACL(string folderName, string userName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "The DELETEACL command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " DELETEACL " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName)) + " " +
                          TextUtils.QuoteString(userName);
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            line = ReadLine();
            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Gets myrights to specified folder.
        /// </summary>
        /// <param name="folderName">Folder which my rifgts to get.</param>
        /// <returns>Returns myrights to specified folder.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected and authenticated.</exception>
        public IMAP_ACL_Flags GetFolderMyrights(string folderName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "The MYRIGHTS command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " MYRIGHTS " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            IMAP_ACL_Flags aclFlags = IMAP_ACL_Flags.None;

            // Must get lines with * and cmdTag + OK or cmdTag BAD/NO.
            while (true)
            {
                line = ReadLine();

                if (line.StartsWith("*"))
                {
                    line = RemoveCmdTag(line);

                    if (line.ToUpper().IndexOf("MYRIGHTS") > -1)
                    {
                        aclFlags = IMAP_Utils.ACL_From_String(line.Substring(0, line.IndexOf(" ")).Trim());
                    }
                }
                else
                {
                    break;
                }
            }

            line = RemoveCmdTag(line);
            if (!line.ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return aclFlags;
        }

        /// <summary>
        /// Copies specified messages to specified folder.
        /// </summary>
        /// <param name="sequence_set">IMAP sequence-set.</param>
        /// <param name="destFolder">Destination folder name.</param>
        /// <param name="uidCopy">Specifies if UID COPY or COPY. 
        /// For UID COPY all sequence_set numers must be message UID values and for normal COPY message numbers.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>
        public void CopyMessages(IMAP_SequenceSet sequence_set, string destFolder, bool uidCopy)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The COPY command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The COPY command is only valid in selected state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            destFolder = destFolder.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = "";
            if (uidCopy)
            {
                line = GetNextCmdTag() + " UID COPY " + sequence_set.ToSequenceSetString() + " " +
                       TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(destFolder));
            }
            else
            {
                line = GetNextCmdTag() + " COPY " + sequence_set.ToSequenceSetString() + " " +
                       TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(destFolder));
            }
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessStatusResponse(line);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Moves specified messages to specified folder.
        /// </summary>
        /// <param name="sequence_set">IMAP sequence-set.</param>
        /// <param name="destFolder">Folder where to copy messages.</param>
        /// <param name="uidMove">Specifies if sequence-set contains message UIDs or message numbers.</param>
        public void MoveMessages(IMAP_SequenceSet sequence_set, string destFolder, bool uidMove)
        {
            if (!IsConnected)
            {
                throw new Exception("You must connect first !");
            }
            if (!IsAuthenticated)
            {
                throw new Exception("You must authenticate first !");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new Exception("You must select folder first !");
            }

            CopyMessages(sequence_set, destFolder, uidMove);
            DeleteMessages(sequence_set, uidMove);
        }

        /// <summary>
        /// Deletes specified messages.
        /// </summary>
        /// <param name="sequence_set">IMAP sequence-set.</param>
        /// <param name="uidDelete">Specifies if sequence-set contains message UIDs or message numbers.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>
        public void DeleteMessages(IMAP_SequenceSet sequence_set, bool uidDelete)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            // 1) Set deleted flag
            // 2) Delete messages with EXPUNGE command

            string line = "";
            if (uidDelete)
            {
                line = GetNextCmdTag() + " UID STORE " + sequence_set.ToSequenceSetString() + " " +
                       "+FLAGS.SILENT (\\Deleted)";
            }
            else
            {
                line = GetNextCmdTag() + " STORE " + sequence_set.ToSequenceSetString() + " " +
                       "+FLAGS.SILENT (\\Deleted)";
            }
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessStatusResponse(line);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            line = GetNextCmdTag() + " EXPUNGE";
            countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessStatusResponse(line);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Stores message to specified folder.
        /// </summary>
        /// <param name="folderName">Folder where to store message.</param>
        /// <param name="data">Message data which to store.</param>	
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>	
        public void StoreMessage(string folderName, byte[] data)
        {
            StoreMessage(folderName, IMAP_MessageFlags.Seen, DateTime.Now, data);
        }

        /// <summary>
        /// Stores message to specified folder.
        /// </summary>
        /// <param name="folderName">Folder where to store message.</param>
        /// <param name="messageFlags">Message flags what are stored for message.</param>
        /// <param name="inernalDate">Internal date value what are stored for message.</param>
        /// <param name="data">Message data which to store.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>
        public void StoreMessage(string folderName,
                                 IMAP_MessageFlags messageFlags,
                                 DateTime inernalDate,
                                 byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.
            folderName = folderName.Replace('\\', PathSeparator).Replace('/', PathSeparator);

            string line = GetNextCmdTag() + " APPEND " +
                          TextUtils.QuoteString(Core.Encode_IMAP_UTF7_String(folderName)) + " (" +
                          IMAP_Utils.MessageFlagsToString(messageFlags) + ") \"" +
                          IMAP_Utils.DateTimeToString(inernalDate) + "\" {" + data.Length + "}";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line or + send data.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessStatusResponse(line);
                }
                    // Send data.
                else if (line.StartsWith("+"))
                {
                    // Send message.
                    TcpStream.Write(data, 0, data.Length);
                    LogAddWrite(data.Length, "Wrote " + data.Length + " bytes.");

                    // Send CRLF, ends splitted command line.
                    TcpStream.Write(new[] {(byte) '\r', (byte) '\n'}, 0, 2);
                    LogAddWrite(data.Length, "Wrote CRLF.");
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }


        public int[] Search(string searchPattern, bool uidSearch)
        {
            if (!IsConnected)
            {
                throw new Exception("You must connect first !");
            }
            if (!IsAuthenticated)
            {
                throw new Exception("You must authenticate first !");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new Exception("You must select folder first !");
            }

            string line = GetNextCmdTag() + (uidSearch ? " UID" : "") + " SEARCH " +
                          Core.Encode_IMAP_UTF7_String(searchPattern);
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            List<int> messageIDs = new List<int>();
            // Read un-tagged response lines while we get final response line or + send data.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessSearchResponse(line, messageIDs);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return messageIDs.ToArray();
        }

        public int[] SearchText(string textToFind, bool uidSearch, Encoding charset)
        {
            if (!IsConnected)
            {
                throw new Exception("You must connect first !");
            }
            if (!IsAuthenticated)
            {
                throw new Exception("You must authenticate first !");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new Exception("You must select folder first !");
            }

            string line = GetNextCmdTag() + (uidSearch ? " UID" : "") + " SEARCH " + (charset==null?"":string.Format("CHARSET {0} ", charset.WebName.ToUpper()))+string.Format("TEXT {0}",
                (charset==null?string.Format("\"{0}\"",Core.Encode_IMAP_UTF7_String(textToFind)):string.Format("{{{0}}}", textToFind.Length)));
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);
            if (charset != null)
            {
                line = ReadLine();
                if (line.StartsWith("+"))
                {
                    countWritten = TcpStream.WriteLine(textToFind);
                    LogAddWrite(countWritten, textToFind);
                }
            }

            List<int> messageIDs = new List<int>();
            // Read un-tagged response lines while we get final response line or + send data.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessSearchResponse(line, messageIDs);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return messageIDs.ToArray();
        }

        /// <summary>
        /// Fetches specifes messages specified fetch items.
        /// </summary>
        /// <param name="sequence_set">IMAP sequence-set.</param>
        /// <param name="fetchFlags">Specifies what data to fetch from IMAP server.</param>
        /// <param name="setSeenFlag">If true message seen flag is setted.</param>
        /// <param name="uidFetch">Specifies if sequence-set contains message UIDs or message numbers.</param>
        /// <returns>Returns requested fetch items.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>
        public IMAP_FetchItem[] FetchMessages(IMAP_SequenceSet sequence_set,
                                              IMAP_FetchItem_Flags fetchFlags,
                                              bool setSeenFlag,
                                              bool uidFetch)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            List<IMAP_FetchItem> fetchItems = new List<IMAP_FetchItem>();

            //--- Construct FETCH command line -----------------------------------------------------------------------//
            string fetchCmdLine = GetNextCmdTag();
            if (uidFetch)
            {
                fetchCmdLine += " UID";
            }
            fetchCmdLine += " FETCH " + sequence_set.ToSequenceSetString() + " (UID";

            // FLAGS
            if ((fetchFlags & IMAP_FetchItem_Flags.MessageFlags) != 0)
            {
                fetchCmdLine += " FLAGS";
            }
            // RFC822.SIZE
            if ((fetchFlags & IMAP_FetchItem_Flags.Size) != 0)
            {
                fetchCmdLine += " RFC822.SIZE";
            }
            // INTERNALDATE
            if ((fetchFlags & IMAP_FetchItem_Flags.InternalDate) != 0)
            {
                fetchCmdLine += " INTERNALDATE";
            }
            // ENVELOPE
            if ((fetchFlags & IMAP_FetchItem_Flags.Envelope) != 0)
            {
                fetchCmdLine += " ENVELOPE";
            }
            // BODYSTRUCTURE
            if ((fetchFlags & IMAP_FetchItem_Flags.BodyStructure) != 0)
            {
                fetchCmdLine += " BODYSTRUCTURE";
            }
            // BODY[] or BODY.PEEK[]
            if ((fetchFlags & IMAP_FetchItem_Flags.Message) != 0)
            {
                if (setSeenFlag)
                {
                    fetchCmdLine += " BODY[]";
                }
                else
                {
                    fetchCmdLine += " BODY.PEEK[]";
                }
            }
            // BODY[HEADER] or BODY.PEEK[HEADER] ---> This needed only if full message isn't requested.
            if ((fetchFlags & IMAP_FetchItem_Flags.Message) == 0 &&
                (fetchFlags & IMAP_FetchItem_Flags.Header) != 0)
            {
                if (setSeenFlag)
                {
                    fetchCmdLine += " BODY[HEADER]";
                }
                else
                {
                    fetchCmdLine += " BODY.PEEK[HEADER]";
                }
            }
            //--------------------------------------------------------------------------------------------------------//

            fetchCmdLine += ")";

            // Send fetch command line to server.
            int countWritten = TcpStream.WriteLine(fetchCmdLine);
            LogAddWrite(countWritten, fetchCmdLine);

            // Read un-tagged response lines while we get final response line.
            byte[] lineBuffer = new byte[Workaround.Definitions.MaxStreamLineLength];
            string line = "";
            while (true)
            {
                SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(lineBuffer,
                                                                                   SizeExceededAction.
                                                                                       JunkAndThrowException);
                TcpStream.ReadLine(args, false);
                if (args.Error != null)
                {
                    throw args.Error;
                }
                line = args.LineUtf8;
                LogAddRead(args.BytesInBuffer, line);

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    if (IsStatusResponse(line))
                    {
                        ProcessStatusResponse(line);
                    }
                    else
                    {
                        int no = 0;
                        int uid = 0;
                        int size = 0;
                        byte[] data = null;
                        IMAP_MessageFlags flags = IMAP_MessageFlags.Recent;
                        string envelope = "";
                        string bodystructure = "";
                        string internalDate = "";

                        // Remove *
                        line = RemoveCmdTag(line);

                        // Get message number
                        no = Convert.ToInt32(line.Substring(0, line.IndexOf(" ")));

                        // Get rid of FETCH  and parse params. Reply:* 1 FETCH (UID 12 BODY[] ...)
                        line = line.Substring(line.IndexOf("FETCH (") + 7);

                        StringReader r = new StringReader(line);
                        // Loop fetch result fields
                        while (r.Available > 0)
                        {
                            r.ReadToFirstChar();

                            // Fetch command closing ) parenthesis
                            if (r.SourceString == ")")
                            {
                                break;
                            }

                                #region UID <value>

                                // UID <value>
                            else if (r.StartsWith("UID", false))
                            {
                                // Remove UID word from reply
                                r.ReadSpecifiedLength("UID".Length);
                                r.ReadToFirstChar();

                                // Read <value>
                                string word = r.ReadWord();
                                if (word == null)
                                {
                                    throw new Exception("IMAP server didn't return UID <value> !");
                                }
                                else
                                {
                                    uid = Convert.ToInt32(word);
                                }
                            }

                                #endregion

                                #region RFC822.SIZE <value>

                                // RFC822.SIZE <value>
                            else if (r.StartsWith("RFC822.SIZE", false))
                            {
                                // Remove RFC822.SIZE word from reply
                                r.ReadSpecifiedLength("RFC822.SIZE".Length);
                                r.ReadToFirstChar();

                                // Read <value>
                                string word = r.ReadWord();
                                if (word == null)
                                {
                                    throw new Exception("IMAP server didn't return RFC822.SIZE <value> !");
                                }
                                else
                                {
                                    try
                                    {
                                        size = Convert.ToInt32(word);
                                    }
                                    catch
                                    {
                                        throw new Exception(
                                            "IMAP server returned invalid RFC822.SIZE <value> '" + word +
                                            "' !");
                                    }
                                }
                            }

                                #endregion

                                #region INTERNALDATE <value>

                                // INTERNALDATE <value>
                            else if (r.StartsWith("INTERNALDATE", false))
                            {
                                // Remove INTERNALDATE word from reply
                                r.ReadSpecifiedLength("INTERNALDATE".Length);
                                r.ReadToFirstChar();

                                // Read <value>
                                string word = r.ReadWord();
                                if (word == null)
                                {
                                    throw new Exception("IMAP server didn't return INTERNALDATE <value> !");
                                }
                                else
                                {
                                    internalDate = word;
                                }
                            }

                                #endregion

                                #region ENVELOPE (<envelope-string>)

                            else if (r.StartsWith("ENVELOPE", false))
                            {
                                // Remove ENVELOPE word from reply
                                r.ReadSpecifiedLength("ENVELOPE".Length);
                                r.ReadToFirstChar();

                                /* 
                                    Handle string literals {count-to-read}<CRLF>data(length = count-to-read).
                                    (string can be quoted string or literal)
                                    Loop while get envelope,invalid response or timeout.
                                */

                                while (true)
                                {
                                    try
                                    {
                                        envelope = r.ReadParenthesized();
                                        break;
                                    }
                                    catch (Exception x)
                                    {
                                        string s = r.ReadToEnd();

                                        /* partial_envelope {count-to-read}
                                            Example: ENVELOPE ("Mon, 03 Apr 2006 10:10:10 GMT" {35}
                                        */
                                        if (s.EndsWith("}"))
                                        {
                                            // Get partial envelope and append it back to reader
                                            r.AppenString(s.Substring(0, s.LastIndexOf('{')));

                                            // Read remaining envelope and append it to reader.
                                            int countToRead =
                                                Convert.ToInt32(s.Substring(s.LastIndexOf('{') + 1,
                                                                            s.LastIndexOf('}') -
                                                                            s.LastIndexOf('{') - 1));
                                            string reply = TcpStream.ReadFixedCountString(countToRead);
                                            LogAddRead(countToRead, reply);
                                            r.AppenString(TextUtils.QuoteString(reply));

                                            // Read fetch continuing line.
                                            TcpStream.ReadLine(args, false);
                                            if (args.Error != null)
                                            {
                                                throw args.Error;
                                            }
                                            line = args.LineUtf8;
                                            LogAddRead(args.BytesInBuffer, line);
                                            r.AppenString(line);
                                        }
                                            // Unexpected response
                                        else
                                        {
                                            throw x;
                                        }
                                    }
                                }
                            }

                                #endregion

                                #region BODYSTRUCTURE (<bodystructure-string>)

                            else if (r.StartsWith("BODYSTRUCTURE", false))
                            {
                                // Remove BODYSTRUCTURE word from reply
                                r.ReadSpecifiedLength("BODYSTRUCTURE".Length);
                                r.ReadToFirstChar();

                                bodystructure = r.ReadParenthesized();
                            }

                                #endregion

                                #region BODY[] or BODY[HEADER]

                                // BODY[] or BODY[HEADER]
                            else if (r.StartsWith("BODY", false))
                            {
                                if (r.StartsWith("BODY[]", false))
                                {
                                    // Remove BODY[]
                                    r.ReadSpecifiedLength("BODY[]".Length);
                                }
                                else if (r.StartsWith("BODY[HEADER]", false))
                                {
                                    // Remove BODY[HEADER]
                                    r.ReadSpecifiedLength("BODY[HEADER]".Length);
                                }
                                else
                                {
                                    throw new Exception("Invalid FETCH response: " + r.SourceString);
                                }
                                r.ReadToFirstChar();

                                // We must now have {<size-to-read>}, or there is error
                                if (!r.StartsWith("{"))
                                {
                                    throw new Exception("Invalid FETCH BODY[] or BODY[HEADER] response: " +
                                                        r.SourceString);
                                }
                                // Read <size-to-read>
                                int dataLength = Convert.ToInt32(r.ReadParenthesized());

                                // Read data
                                MemoryStream storeStrm = new MemoryStream(dataLength);
                                TcpStream.ReadFixedCount(storeStrm, dataLength);
                                LogAddRead(dataLength, "Readed " + dataLength + " bytes.");
                                data = storeStrm.ToArray();

                                // Read fetch continuing line.
                                TcpStream.ReadLine(args, false);
                                if (args.Error != null)
                                {
                                    throw args.Error;
                                }
                                line = args.LineUtf8;
                                LogAddRead(args.BytesInBuffer, line);
                                r.AppenString(line);
                            }

                                #endregion

                                #region FLAGS (<flags-list>)

                                // FLAGS (<flags-list>)
                            else if (r.StartsWith("FLAGS", false))
                            {
                                // Remove FLAGS word from reply
                                r.ReadSpecifiedLength("FLAGS".Length);
                                r.ReadToFirstChar();

                                // Read (<flags-list>)
                                string flagsList = r.ReadParenthesized();
                                if (flagsList == null)
                                {
                                    throw new Exception("IMAP server didn't return FLAGS (<flags-list>) !");
                                }
                                else
                                {
                                    flags = IMAP_Utils.ParseMessageFlags(flagsList);
                                }
                            }

                                #endregion

                            else
                            {
                                throw new Exception("Not supported fetch reply: " + r.SourceString);
                            }
                        }

                        fetchItems.Add(new IMAP_FetchItem(no,
                                                          uid,
                                                          size,
                                                          data,
                                                          flags,
                                                          internalDate,
                                                          envelope,
                                                          bodystructure,
                                                          fetchFlags));
                    }
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return fetchItems.ToArray();
        }

        /// <summary>
        /// Gets specified message from server and stores to specified stream.
        /// </summary>
        /// <param name="uid">Message UID which to get.</param>
        /// <param name="storeStream">Stream where to store message.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>
        public void FetchMessage(int uid, Stream storeStream)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            // Send fetch command line to server.
            string line = GetNextCmdTag() + " UID FETCH " + uid + " BODY[]";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.            
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    if (IsStatusResponse(line))
                    {
                        ProcessStatusResponse(line);
                    }
                    else if (line.ToUpper().IndexOf("BODY[") > - 1)
                    {
                        if (line.IndexOf('{') > -1)
                        {
                            StringReader r = new StringReader(line);
                            while (r.Available > 0 && !r.StartsWith("{"))
                            {
                                r.ReadSpecifiedLength(1);
                            }
                            int sizeOfData = Convert.ToInt32(r.ReadParenthesized());
                            TcpStream.ReadFixedCount(storeStream, sizeOfData);
                            LogAddRead(sizeOfData, "Readed " + sizeOfData + " bytes.");
                            line = ReadLine();
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Stores specified message flags to specified messages.
        /// </summary>
        /// <param name="sequence_set">IMAP sequence-set.</param>
        /// <param name="msgFlags">Message flags.</param>
        /// <param name="uidStore">Specifies if UID STORE or STORE. 
        /// For UID STORE all sequence_set numers must be message UID values and for normal STORE message numbers.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>
        public void StoreMessageFlags(IMAP_SequenceSet sequence_set, IMAP_MessageFlags msgFlags, bool uidStore)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            string line = "";
            if (uidStore)
            {
                line = GetNextCmdTag() + " UID STORE " + sequence_set.ToSequenceSetString() + " FLAGS (" +
                       IMAP_Utils.MessageFlagsToString(msgFlags) + ")";
            }
            else
            {
                line = GetNextCmdTag() + " STORE " + sequence_set.ToSequenceSetString() + " FLAGS (" +
                       IMAP_Utils.MessageFlagsToString(msgFlags) + ")";
            }
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessStatusResponse(line);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        public void AddMessageFlags(IMAP_SequenceSet sequence_set, IMAP_MessageFlags msgFlags, bool uidStore)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            string line = "";
            if (uidStore)
            {
                line = GetNextCmdTag() + " UID STORE " + sequence_set.ToSequenceSetString() +
                       " +FLAGS.SILENT (" + IMAP_Utils.MessageFlagsToString(msgFlags) + ")";
            }
            else
            {
                line = GetNextCmdTag() + " STORE " + sequence_set.ToSequenceSetString() + " +FLAGS.SILENT (" +
                       IMAP_Utils.MessageFlagsToString(msgFlags) + ")";
            }
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessStatusResponse(line);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        public void RemoveMessageFlags(IMAP_SequenceSet sequence_set,
                                       IMAP_MessageFlags msgFlags,
                                       bool uidStore)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            string line = "";
            if (uidStore)
            {
                line = GetNextCmdTag() + " UID STORE " + sequence_set.ToSequenceSetString() +
                       " -FLAGS.SILENT (" + IMAP_Utils.MessageFlagsToString(msgFlags) + ")";
            }
            else
            {
                line = GetNextCmdTag() + " STORE " + sequence_set.ToSequenceSetString() + " -FLAGS.SILENT (" +
                       IMAP_Utils.MessageFlagsToString(msgFlags) + ")";
            }
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessStatusResponse(line);
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }

        /// <summary>
        /// Gets messages total size in selected folder.
        /// </summary>
        /// <returns>Returns messages total size in selected folder.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>
        public int GetMessagesTotalSize()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            int totalSize = 0;

            string line = GetNextCmdTag() + " FETCH 1:* (RFC822.SIZE)";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    if (IsStatusResponse(line))
                    {
                        ProcessStatusResponse(line);
                    }
                    else
                    {
                        // Get rid of * 1 FETCH  and parse params. Reply:* 1 FETCH (UID 12 BODY[] ...)
                        line = line.Substring(line.IndexOf("FETCH (") + 7);

                        // RFC822.SIZE field
                        if (line.ToUpper().StartsWith("RFC822.SIZE"))
                        {
                            line = line.Substring(11).Trim(); // Remove RFC822.SIZE word from reply

                            totalSize += Convert.ToInt32(line.Substring(0, line.Length - 1).Trim());
                                // Remove ending ')'						
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return totalSize;
        }

        /// <summary>
        /// Gets unseen messages count in selected folder.
        /// </summary>
        /// <returns>Returns number of unseen messages.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when IMAP client is not connected,not authenticated and folder not selected.</exception>
        public int GetUnseenMessagesCount()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            int count = 0;

            string line = GetNextCmdTag() + " FETCH 1:* (FLAGS)";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    if (IsStatusResponse(line))
                    {
                        ProcessStatusResponse(line);
                    }
                    else
                    {
                        // Get rid of * 1 FETCH  and parse params. Reply:* 1 FETCH (UID 12 BODY[] ...)
                        line = line.Substring(line.IndexOf("FETCH (") + 7);

                        if (line.ToUpper().IndexOf("\\SEEN") == -1)
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return count;
        }

        /// <summary>
        /// Gets IMAP server folder separator char.
        /// </summary>
        /// <returns>Returns IMAP server folder separator char.</returns>
        public string GetFolderSeparator()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }

            string folderSeparator = "";

            string line = GetNextCmdTag() + " LIST \"\" \"\"";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    if (IsStatusResponse(line))
                    {
                        ProcessStatusResponse(line);
                    }
                    else
                    {
                        line = line.Substring(line.IndexOf(")") + 1).Trim(); // Remove * LIST(..)

                        // get folder separator
                        folderSeparator = line.Substring(0, line.IndexOf(" ")).Trim();
                    }
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }

            return folderSeparator.Replace("\"", "");
        }

        #endregion

        #region Overrides

        /// <summary>
        /// This method is called after TCP client has sucessfully connected.
        /// </summary>
        protected override void OnConnected()
        {
            SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[Workaround.Definitions.MaxStreamLineLength],
                                                                               SizeExceededAction.
                                                                                   JunkAndThrowException);
            TcpStream.ReadLine(args, false);
            if (args.Error != null)
            {
                throw args.Error;
            }
            string line = args.LineUtf8;
            LogAddRead(args.BytesInBuffer, line);
            line = RemoveCmdTag(line);
            if (line.ToUpper().StartsWith("OK"))
            {
                // Clear path separator, so next access will get it.
                m_PathSeparator = '\0';
            }
            else
            {
                throw new Exception("Server returned: " + line);
            }
            GetCapabilities();
            if (Capabilities.Contains("STARTTLS") && Capabilities.Contains("LOGINDISABLED"))
            {
                //Switch to ssl
                StartTLS();
                //get caps again
                GetCapabilities();
            }
        }

        #endregion

        #region Event handlers

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsConnected && IsAuthenticated && !IsDisposed)
            {
                if ((DateTime.Now - LastActivity) > TimeSpan.FromMilliseconds(NoopInterval))
                {
                    //Send noop
                    //SendNoop();//No noop
                }
            }
        }

        #endregion

        #region Utility methods

        private DateTime lastStatus;

        public void SendNoop()
        {
            SendNoop(false);
        }

        public void SendNoop(bool bForce)
        {
            if (IsConnected && IsAuthenticated && !IsDisposed)
            {
                if (((DateTime.Now - lastStatus).TotalMilliseconds > NoopDelay)||bForce)
                {
                    string cmdTag = GetNextCmdTag();
                    string line = cmdTag + " NOOP";
                    int countWritten = TcpStream.WriteLine(line);
                    LogAddWrite(countWritten, line);

                    while (true)
                    {
                        line = ReadLine();

                        if (line.StartsWith("*"))
                        {
                            ProcessStatusResponse(line);
                        }
                        else
                        {
                            break;
                        }
                    }

                    line = RemoveCmdTag(line);
                    if (!line.ToUpper().StartsWith("OK"))
                    {
                        throw new IMAP_ClientException(line);
                    }
                    lastStatus = DateTime.Now;
                }
            }
        }

        private void ProcessSearchResponse(string line, List<int> ds)
        {
            if (line.IndexOf("SEARCH") > -1 && !line.Equals("* SEARCH")/*empty responce*/)
            {
                string[] idsLine = line.Substring(line.IndexOf(' ', line.IndexOf("SEARCH"))).Split(
                    new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string num in idsLine)
                {
                    int number;
                    if (int.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                    {
                        ds.Add(number);
                    }
                }
            }
        }

        /// <summary>
        /// Removes command tag from response line.
        /// </summary>
        /// <param name="responseLine">Response line with command tag.</param>
        /// <returns></returns>
        private string RemoveCmdTag(string responseLine)
        {
            return responseLine.Substring(responseLine.IndexOf(" ")).Trim();
        }

        /// <summary>
        /// Processes IMAP STATUS response and updates this class status info.
        /// </summary>
        /// <param name="statusResponse">IMAP STATUS response line.</param>
        private void ProcessStatusResponse(string statusResponse)
        {
            /* RFC 3501 7.3.1.  EXISTS Response
				Example:    S: * 23 EXISTS
			*/

            /* RFC 3501 7.3.2.  RECENT Response
				Example:    S: * 5 RECENT
			*/

            statusResponse = statusResponse.ToUpper();

            // Get rid of *
            statusResponse = statusResponse.Substring(1).Trim();

            if (statusResponse.IndexOf("EXISTS") > -1 && statusResponse.IndexOf("FLAGS") == -1)
            {
                MsgCount = Convert.ToInt32(statusResponse.Substring(0, statusResponse.IndexOf(" ")).Trim());
                lastStatus = DateTime.Now;
            }
            else if (statusResponse.IndexOf("RECENT") > -1 && statusResponse.IndexOf("FLAGS") == -1)
            {
                NewMsgCount = Convert.ToInt32(statusResponse.Substring(0, statusResponse.IndexOf(" ")).Trim());
                lastStatus = DateTime.Now;
            }
            else if (statusResponse.IndexOf("EXPUNGE") > -1 && statusResponse.IndexOf("FLAGS") == -1)
            {
                //Dec number by 1
                MsgCount--;
                lastStatus = DateTime.Now;
            }
        }

        /// <summary>
        /// Gets if specified line is STATUS response.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private bool IsStatusResponse(string line)
        {
            if (!line.StartsWith("*"))
            {
                return false;
            }

            // Remove *
            line = line.Substring(1).TrimStart();

            // RFC 3951 7.1.2.  NO Response (untagged)
            if (line.ToUpper().StartsWith("NO"))
            {
                return true;
            }
                // RFC 3951 7.1.3.  BAD Response (untagged)
            else if (line.ToUpper().StartsWith("BAD"))
            {
                return true;
            }

            // * 1 EXISTS
            // * 1 RECENT
            // * 1 EXPUNGE
            if (line.ToLower().IndexOf("exists") > -1)
            {
                return true;
            }
            if (line.ToLower().IndexOf("recent") > -1 && line.ToLower().IndexOf("flags") == -1)
            {
                return true;
            }
            else if (line.ToLower().IndexOf("expunge") > -1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets next command tag.
        /// </summary>
        /// <returns>Returns next command tag.</returns>
        private string GetNextCmdTag()
        {
            m_CmdNumber++;

            return "ls" + m_CmdNumber;
        }

        #endregion

        public byte[] GetFilters()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }
            if (m_SelectedFolder.Length == 0)
            {
                throw new InvalidOperationException("The command is only valid in selected state.");
            }

            // Send fetch command line to server.
            string line = GetNextCmdTag() + " X-GET-FILTER ";
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);
            using (MemoryStream store = new MemoryStream())
            {
                // Read un-tagged response lines while we get final response line.            
                while (true)
                {
                    line = ReadLine();

                    // We have un-tagged resposne.
                    if (line.StartsWith("*"))
                    {
                        if (IsStatusResponse(line))
                        {
                            ProcessStatusResponse(line);
                        }
                        else if (line.ToUpper().IndexOf("FILTER") > -1)
                        {
                            if (line.IndexOf('{') > -1)
                            {
                                StringReader r = new StringReader(line);
                                while (r.Available > 0 && !r.StartsWith("{"))
                                {
                                    r.ReadSpecifiedLength(1);
                                }
                                int sizeOfData = Convert.ToInt32(r.ReadParenthesized());
                                TcpStream.ReadFixedCount(store, sizeOfData);
                                LogAddRead(sizeOfData, "Readed " + sizeOfData + " bytes.");
                                line = ReadLine();
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
                {
                    throw new IMAP_ClientException(line);
                }
                byte[] buffer =  store.GetBuffer();
                if (buffer.Length>0)
                {
                    return Convert.FromBase64String(Encoding.UTF8.GetString(buffer));
                }
                else
                {
                    return null;
                }
            }

        }

        public void SetFilters(byte[] bufferRaw)
        {
            if (bufferRaw == null)
            {
                throw new ArgumentNullException("bufferRaw");
            }
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The command is only valid in authenticated state.");
            }

            // Ensure that we send right separator to server, we accept both \ and /.

            byte[] buffer = Encoding.UTF8.GetBytes(Convert.ToBase64String(bufferRaw));
            string line = string.Format("{0} X-SET-FILTER {{{1}}}", GetNextCmdTag(), buffer.Length);
            int countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            // Read un-tagged response lines while we get final response line or + send data.
            while (true)
            {
                line = ReadLine();

                // We have un-tagged resposne.
                if (line.StartsWith("*"))
                {
                    ProcessStatusResponse(line);
                }
                // Send data.
                else if (line.StartsWith("+"))
                {
                    // Send message.
                    TcpStream.Write(buffer, 0, buffer.Length);
                    LogAddWrite(buffer.Length, "Wrote " + buffer.Length + " bytes.");

                    // Send CRLF, ends splitted command line.
                    TcpStream.Write(new[] { (byte)'\r', (byte)'\n' }, 0, 2);
                    LogAddWrite(buffer.Length, "Wrote CRLF.");
                }
                else
                {
                    break;
                }
            }

            if (!RemoveCmdTag(line).ToUpper().StartsWith("OK"))
            {
                throw new IMAP_ClientException(line);
            }
        }
    }
}