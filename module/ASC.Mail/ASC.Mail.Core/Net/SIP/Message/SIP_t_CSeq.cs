/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements SIP "Cseq" value. Defined in RFC 3261.
    /// A CSeq in a request contains a single decimal sequence number and 
    /// the request method. The method part of CSeq is case-sensitive. The CSeq header 
    /// field serves to order transactions within a dialog, to provide a means to uniquely 
    /// identify transactions, and to differentiate between new requests and request retransmissions.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     CSeq = 1*DIGIT LWS Method
    /// </code>
    /// </remarks>
    public class SIP_t_CSeq : SIP_t_Value
    {
        #region Members

        private string m_RequestMethod = "";
        private int m_SequenceNumber = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets sequence number.
        /// </summary>
        public int SequenceNumber
        {
            get { return m_SequenceNumber; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("Property SequenceNumber value must be >= 1 !");
                }

                m_SequenceNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets request method. Note: this value is case-sensitive !
        /// </summary>
        public string RequestMethod
        {
            get { return m_RequestMethod; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property RequestMethod value can't be null or empty !");
                }

                m_RequestMethod = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">CSeq: header field value.</param>
        public SIP_t_CSeq(string value)
        {
            Parse(new StringReader(value));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sequenceNumber">Command sequence number.</param>
        /// <param name="requestMethod">Request method.</param>
        public SIP_t_CSeq(int sequenceNumber, string requestMethod)
        {
            m_SequenceNumber = sequenceNumber;
            m_RequestMethod = requestMethod;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "CSeq" from specified value.
        /// </summary>
        /// <param name="value">SIP "CSeq" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>value</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "CSeq" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            // CSeq = 1*DIGIT LWS Method

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Get sequence number
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'CSeq' value, sequence number is missing !");
            }
            try
            {
                m_SequenceNumber = Convert.ToInt32(word);
            }
            catch
            {
                throw new SIP_ParseException("Invalid CSeq 'sequence number' value !");
            }

            // Get request method
            word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'CSeq' value, request method is missing !");
            }
            m_RequestMethod = word;
        }

        /// <summary>
        /// Converts this to valid "CSeq" value.
        /// </summary>
        /// <returns>Returns "CSeq" value.</returns>
        public override string ToStringValue()
        {
            return m_SequenceNumber + " " + m_RequestMethod;
        }

        #endregion
    }
}