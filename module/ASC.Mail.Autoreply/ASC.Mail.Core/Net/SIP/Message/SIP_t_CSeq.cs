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