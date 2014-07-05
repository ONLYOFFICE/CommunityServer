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