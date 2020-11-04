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


namespace ASC.Mail.Net.SIP.Stack
{
    #region usings

    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using Message;

    #endregion

    /// <summary>
    /// SIP server request. Related RFC 3261.
    /// </summary>
    public class SIP_Request : SIP_Message
    {
        #region Members

        private readonly SIP_RequestLine m_pRequestLine;
        private SIP_Flow m_pFlow;
        private IPEndPoint m_pLocalEP;
        private IPEndPoint m_pRemoteEP;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="method">SIP request method.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>method</b> is null reference.</exception>
        public SIP_Request(string method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            m_pRequestLine = new SIP_RequestLine(method, new AbsoluteUri());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets request-line.
        /// </summary>
        public SIP_RequestLine RequestLine
        {
            get { return m_pRequestLine; }
        }

        /// <summary>
        /// Gets or sets flow what received or sent this request. Returns null if this request isn't sent or received.
        /// </summary>
        internal SIP_Flow Flow
        {
            get { return m_pFlow; }

            set { m_pFlow = value; }
        }

        /// <summary>
        /// Gets or sets local end point what sent/received this request. Returns null if this request isn't sent or received.
        /// </summary>
        internal IPEndPoint LocalEndPoint
        {
            get { return m_pLocalEP; }

            set { m_pLocalEP = value; }
        }

        /// <summary>
        /// Gets or sets remote end point what sent/received this request. Returns null if this request isn't sent or received.
        /// </summary>
        internal IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEP; }

            set { m_pRemoteEP = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses SIP_Request from byte array.
        /// </summary>
        /// <param name="data">Valid SIP request data.</param>
        /// <returns>Returns parsed SIP_Request obeject.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>data</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public static SIP_Request Parse(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Parse(new MemoryStream(data));
        }

        /// <summary>
        /// Parses SIP_Request from stream.
        /// </summary>
        /// <param name="stream">Stream what contains valid SIP request.</param>
        /// <returns>Returns parsed SIP_Request obeject.</returns>
        /// <exception cref="ArgumentNullException">Raised when <b>stream</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public static SIP_Request Parse(Stream stream)
        {
            /* Syntax:
                SIP-Method SIP-URI SIP-Version
                SIP-Message                          
            */

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Parse Response-line
            StreamLineReader r = new StreamLineReader(stream);
            r.Encoding = "utf-8";
            string[] method_uri_version = r.ReadLineString().Split(' ');
            if (method_uri_version.Length != 3)
            {
                throw new Exception(
                    "Invalid SIP request data ! Method line doesn't contain: SIP-Method SIP-URI SIP-Version.");
            }
            SIP_Request retVal = new SIP_Request(method_uri_version[0]);
            retVal.RequestLine.Uri = AbsoluteUri.Parse(method_uri_version[1]);
            retVal.RequestLine.Version = method_uri_version[2];

            // Parse SIP message
            retVal.InternalParse(stream);

            return retVal;
        }

        /// <summary>
        /// Clones this request.
        /// </summary>
        /// <returns>Returns new cloned request.</returns>
        public SIP_Request Copy()
        {
            SIP_Request retVal = Parse(ToByteData());
            retVal.Flow = m_pFlow;
            retVal.LocalEndPoint = m_pLocalEP;
            retVal.RemoteEndPoint = m_pRemoteEP;

            return retVal;
        }

        /// <summary>
        /// Checks if SIP request has all required values as request line,header fields and their values.
        /// Throws Exception if not valid SIP request.
        /// </summary>
        public void Validate()
        {
            // Request SIP version
            // Via: + branch prameter
            // To:
            // From:
            // CallID:
            // CSeq
            // Max-Forwards RFC 3261 8.1.1.

            if (!RequestLine.Version.ToUpper().StartsWith("SIP/2.0"))
            {
                throw new SIP_ParseException("Not supported SIP version '" + RequestLine.Version + "' !");
            }

            if (Via.GetTopMostValue() == null)
            {
                throw new SIP_ParseException("Via: header field is missing !");
            }
            if (Via.GetTopMostValue().Branch == null)
            {
                throw new SIP_ParseException("Via: header field branch parameter is missing !");
            }

            if (To == null)
            {
                throw new SIP_ParseException("To: header field is missing !");
            }

            if (From == null)
            {
                throw new SIP_ParseException("From: header field is missing !");
            }

            if (CallID == null)
            {
                throw new SIP_ParseException("CallID: header field is missing !");
            }

            if (CSeq == null)
            {
                throw new SIP_ParseException("CSeq: header field is missing !");
            }

            if (MaxForwards == -1)
            {
                // We can fix it by setting it to default value 70.
                MaxForwards = 70;
            }

            /* RFC 3261 12.1.2
                When a UAC sends a request that can establish a dialog (such as an INVITE) it MUST 
                provide a SIP or SIPS URI with global scope (i.e., the same SIP URI can be used in 
                messages outside this dialog) in the Contact header field of the request. If the 
                request has a Request-URI or a topmost Route header field value with a SIPS URI, the
                Contact header field MUST contain a SIPS URI.
            */
            if (SIP_Utils.MethodCanEstablishDialog(RequestLine.Method))
            {
                if (Contact.GetAllValues().Length == 0)
                {
                    throw new SIP_ParseException(
                        "Contact: header field is missing, method that can establish a dialog MUST provide a SIP or SIPS URI !");
                }
                if (Contact.GetAllValues().Length > 1)
                {
                    throw new SIP_ParseException(
                        "There may be only 1 Contact: header for the method that can establish a dialog !");
                }
                if (!Contact.GetTopMostValue().Address.IsSipOrSipsUri)
                {
                    throw new SIP_ParseException(
                        "Method that can establish a dialog MUST have SIP or SIPS uri in Contact: header !");
                }
            }

            // TODO: Invite must have From:/To: tag

            // TODO: Check that request-Method equals CSeq method

            // TODO: PRACK must have RAck and RSeq header fields.

            // TODO: get in transport made request, so check if sips and sip set as needed.
        }

        /// <summary>
        /// Stores SIP_Request to specified stream.
        /// </summary>
        /// <param name="stream">Stream where to store.</param>
        public void ToStream(Stream stream)
        {
            // Add request-line
            byte[] responseLine = Encoding.UTF8.GetBytes(m_pRequestLine.ToString());
            stream.Write(responseLine, 0, responseLine.Length);

            // Add SIP-message
            InternalToStream(stream);
        }

        /// <summary>
        /// Converts this request to raw srver request data.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteData()
        {
            MemoryStream retVal = new MemoryStream();
            ToStream(retVal);

            return retVal.ToArray();
        }

        /// <summary>
        /// Returns request as string.
        /// </summary>
        /// <returns>Returns request as string.</returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(ToByteData());
        }

        #endregion
    }
}