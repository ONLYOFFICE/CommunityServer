/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


namespace ASC.Mail.Net.Mail
{
    #region usings

    using System;
    using System.Net;
    using System.Text;
    using MIME;

    #endregion

    /// <summary>
    /// Represents "Received:" header. Defined in RFC 5321 4.4.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 5321 4.4.
    ///     Time-stamp-line = "Received:" FWS Stamp CRLF
    ///     
    ///     Stamp           = From-domain By-domain Opt-info [CFWS] ";" FWS date-time
    ///                     ; where "date-time" is as defined in RFC 5322 [4]
    ///                     ; but the "obs-" forms, especially two-digit
    ///                     ; years, are prohibited in SMTP and MUST NOT be used.
    ///                     
    ///     From-domain     = "FROM" FWS Extended-Domain
    ///
    ///     By-domain       = CFWS "BY" FWS Extended-Domain
    ///
    ///     Extended-Domain = Domain / ( Domain FWS "(" TCP-info ")" ) / ( address-literal FWS "(" TCP-info ")" )
    ///
    ///     TCP-info        = address-literal / ( Domain FWS address-literal )
    ///                     ; Information derived by server from TCP connection not client EHLO.
    ///
    ///     Opt-info        = [Via] [With] [ID] [For] [Additional-Registered-Clauses]
    ///
    ///     Via             = CFWS "VIA" FWS Link
    ///
    ///     With            = CFWS "WITH" FWS Protocol
    ///
    ///     ID              = CFWS "ID" FWS ( Atom / msg-id )
    ///                     ; msg-id is defined in RFC 5322 [4]
    ///
    ///     For            = CFWS "FOR" FWS ( Path / Mailbox )
    ///     
    ///     Additional-Registered-Clauses  = CFWS Atom FWS String
    ///
    ///     Link           = "TCP" / Addtl-Link
    ///
    ///     Addtl-Link     = Atom
    ///     
    ///     Protocol       = "ESMTP" / "SMTP" / Attdl-Protocol
    ///     
    ///     Mailbox        = Local-part "@" ( Domain / address-literal )
    /// </code>
    /// </remarks>
    public class Mail_h_Received : MIME_h
    {
        #region Members

        private string m_By = "";
        private string m_For;
        private string m_From = "";
        private string m_ID;
        private bool m_IsModified;
        private string m_ParseValue;
        private Mail_t_TcpInfo m_pBy_TcpInfo;
        private Mail_t_TcpInfo m_pFrom_TcpInfo;
        private DateTime m_Time;
        private string m_Via;
        private string m_With;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="from">Host from where message was received.</param>
        /// <param name="by">Host name what received message.</param>
        /// <param name="time">Date time when message was received.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>from</b> or <b>by</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public Mail_h_Received(string from, string by, DateTime time)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (from == string.Empty)
            {
                throw new ArgumentException("Argument 'from' value must be specified.", "from");
            }
            if (by == null)
            {
                throw new ArgumentNullException("by");
            }
            if (by == string.Empty)
            {
                throw new ArgumentException("Argument 'by' value must be specified.", "by");
            }

            m_From = from;
            m_By = by;
            m_Time = time;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets host name what received message.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null reference passed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value passed.</exception>
        public string By
        {
            get { return m_By; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("By");
                }
                if (value == string.Empty)
                {
                    throw new ArgumentException("Property 'By' value must be specified", "By");
                }

                m_By = value;
                m_IsModified = true;
            }
        }

        /// <summary>
        /// Gets or sets By TCP-Info value. Value null means not specified.
        /// </summary>
        /// <remarks>RFC defines it, but i don't see any point about that value.</remarks>
        public Mail_t_TcpInfo By_TcpInfo
        {
            get { return m_pBy_TcpInfo; }

            set
            {
                m_pBy_TcpInfo = value;
                m_IsModified = true;
            }
        }

        /// <summary>
        /// Gets or sets mailbox for who message was received. Value null means not specified.
        /// </summary>
        public string For
        {
            get { return m_For; }

            set
            {
                m_For = value;
                m_IsModified = true;
            }
        }

        /// <summary>
        /// Gets or sets host from where message was received.
        /// </summary>
        /// <remarks>Normally this is just EHLO/HELO host name what client reported to SMTP server.</remarks>
        /// <exception cref="ArgumentNullException">Is raised when null reference passed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value passed.</exception>
        public string From
        {
            get { return m_From; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("From");
                }
                if (value == string.Empty)
                {
                    throw new ArgumentException("Property 'From' value must be specified", "From");
                }

                m_From = value;
                m_IsModified = true;
            }
        }

        /// <summary>
        /// Gets or sets From TCP-Info value. Value null means not specified.
        /// </summary>
        /// <remarks>This value is message sender host IP and optional dns host name.
        /// This value is based on server connection info, not client reported info(EHLO/HELO).
        /// </remarks>
        public Mail_t_TcpInfo From_TcpInfo
        {
            get { return m_pFrom_TcpInfo; }

            set
            {
                m_pFrom_TcpInfo = value;
                m_IsModified = true;
            }
        }

        /// <summary>
        /// Gets or sets ID value. Value null means not specified.
        /// </summary>
        public string ID
        {
            get { return m_ID; }

            set
            {
                m_ID = value;
                m_IsModified = true;
            }
        }

        /// <summary>
        /// Gets if this header field is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public override bool IsModified
        {
            get { return m_IsModified; }
        }

        /// <summary>
        /// Returns always "Received".
        /// </summary>
        public override string Name
        {
            get { return "Received"; }
        }

        /// <summary>
        /// Gets or sets time when message was received.
        /// </summary>
        public DateTime Time
        {
            get { return m_Time; }

            set
            {
                m_Time = value;
                m_IsModified = true;
            }
        }

        /// <summary>
        /// Gets or sets non-internet transport. Value null means not specified.
        /// </summary>
        public string Via
        {
            get { return m_Via; }

            set
            {
                m_Via = value;
                m_IsModified = true;
            }
        }

        /// <summary>
        /// Gets or sets receiving protocol. Value null means not specified.
        /// </summary>
        public string With
        {
            get { return m_With; }

            set
            {
                m_With = value;
                m_IsModified = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses header field from the specified value.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Sender: john.doe@domain.com'.</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public static Mail_h_Received Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string[] name_value = value.Split(new[] {':'}, 2);
            if (name_value.Length != 2)
            {
                throw new ParseException("Invalid header field value '" + value + "'.");
            }

            Mail_h_Received retVal = new Mail_h_Received("a", "b", DateTime.MinValue);

            MIME_Reader r = new MIME_Reader(name_value[1]);

            while (true)
            {
                string word = r.Word();
                // We processed all data.
                if (word == null && r.Available == 0)
                {
                    break;
                }
                    // We have comment, just eat it.
                else if (r.StartsWith("("))
                {
                    r.ReadParenthesized();
                }
                    // We have date-time.
                else if (r.StartsWith(";"))
                {
                    // Eat ';'
                    r.Char(false);

                    retVal.m_Time = MIME_Utils.ParseRfc2822DateTime(r.ToEnd());
                }
                else
                {
                    // We have some unexpected char like: .,= ... . Just eat it.
                    if (word == null)
                    {
                        r.Char(true);
                        continue;
                    }

                    word = word.ToUpperInvariant();

                    if (word == "FROM")
                    {
                        retVal.m_From = r.DotAtom();

                        r.ToFirstChar();
                        if (r.StartsWith("("))
                        {
                            string[] parts = r.ReadParenthesized().Split(' ');
                            if (parts.Length == 1)
                            {
                                if (Net_Utils.IsIPAddress(parts[0]))
                                {
                                    retVal.m_pFrom_TcpInfo = new Mail_t_TcpInfo(IPAddress.Parse(parts[0]),
                                                                                null);
                                }
                            }
                            else if (parts.Length == 2)
                            {
                                if (Net_Utils.IsIPAddress(parts[1]))
                                {
                                    retVal.m_pFrom_TcpInfo = new Mail_t_TcpInfo(IPAddress.Parse(parts[1]),
                                                                                parts[0]);
                                }
                            }
                        }
                    }
                    else if (word == "BY")
                    {
                        retVal.m_By = r.DotAtom();

                        r.ToFirstChar();
                        if (r.StartsWith("("))
                        {
                            string[] parts = r.ReadParenthesized().Split(' ');
                            if (parts.Length == 1)
                            {
                                if (Net_Utils.IsIPAddress(parts[0]))
                                {
                                    retVal.m_pBy_TcpInfo = new Mail_t_TcpInfo(IPAddress.Parse(parts[0]), null);
                                }
                            }
                            else if (parts.Length == 2)
                            {
                                if (Net_Utils.IsIPAddress(parts[1]))
                                {
                                    retVal.m_pBy_TcpInfo = new Mail_t_TcpInfo(IPAddress.Parse(parts[1]),
                                                                              parts[0]);
                                }
                            }
                        }
                    }
                    else if (word == "VIA")
                    {
                        retVal.m_Via = r.Word();
                    }
                    else if (word == "WITH")
                    {
                        retVal.m_With = r.Word();
                    }
                    else if (word == "ID")
                    {
                        // msg-id = [CFWS] "<" id-left "@" id-right ">" [CFWS]

                        if (r.StartsWith("<"))
                        {
                            retVal.m_ID = r.ReadParenthesized();
                        }
                        else
                        {
                            retVal.m_ID = r.Atom();
                        }
                    }
                    else if (word == "FOR")
                    {
                        r.ToFirstChar();

                        // path / angle-address
                        if (r.StartsWith("<"))
                        {
                            retVal.m_For = r.ReadParenthesized();
                        }
                        else
                        {
                            string mailbox = Mail_Utils.SMTP_Mailbox(r);
                            if (mailbox == null)
                            {
                                throw new ParseException("Invalid Received: For parameter value '" + r.ToEnd() +
                                                         "'.");
                            }
                            retVal.m_For = mailbox;
                        }
                    }
                        // Unknown, just eat value.
                    else
                    {
                        r.Word();
                    }
                }
            }

            retVal.m_ParseValue = value;

            return retVal;
        }

        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit characters. Value null means parameters not encoded.</param>
        /// <returns>Returns header field as string.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            if (IsModified)
            {
                StringBuilder retVal = new StringBuilder();

                retVal.Append("Received: ");
                retVal.Append("FROM " + m_From);
                if (m_pFrom_TcpInfo != null)
                {
                    retVal.Append(" (" + m_pFrom_TcpInfo + ")");
                }
                retVal.Append(" BY " + m_By);
                if (m_pBy_TcpInfo != null)
                {
                    retVal.Append(" (" + m_pBy_TcpInfo + ")");
                }
                if (!string.IsNullOrEmpty(m_Via))
                {
                    retVal.Append(" VIA " + m_Via);
                }
                if (!string.IsNullOrEmpty(m_With))
                {
                    retVal.Append(" WITH " + m_With);
                }
                if (!string.IsNullOrEmpty(m_ID))
                {
                    retVal.Append(" ID " + m_ID);
                }
                if (!string.IsNullOrEmpty(m_For))
                {
                    retVal.Append(" FOR " + m_For);
                }
                retVal.Append("; " + MIME_Utils.DateTimeToRfc2822(m_Time));
                retVal.Append("\r\n");

                return retVal.ToString();
            }
            else
            {
                return m_ParseValue;
            }
        }

        #endregion
    }
}