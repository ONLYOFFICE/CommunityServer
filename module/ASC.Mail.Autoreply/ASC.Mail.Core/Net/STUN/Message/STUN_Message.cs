/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


namespace ASC.Mail.Net.STUN.Message
{
    #region usings

    using System;
    using System.Net;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements STUN message. Defined in RFC 3489.
    /// </summary>
    public class STUN_Message
    {
        #region Members

        private string m_Password;
        private IPEndPoint m_pChangedAddress;
        private STUN_t_ChangeRequest m_pChangeRequest;
        private STUN_t_ErrorCode m_pErrorCode;
        private IPEndPoint m_pMappedAddress;
        private IPEndPoint m_pReflectedFrom;
        private IPEndPoint m_pResponseAddress;
        private IPEndPoint m_pSourceAddress;
        private Guid m_pTransactionID = Guid.Empty;
        private string m_ServerName;
        private STUN_MessageType m_Type = STUN_MessageType.BindingRequest;
        private string m_UserName;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public STUN_Message()
        {
            m_pTransactionID = Guid.NewGuid();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets IP end point where STUN server will send response back to STUN client 
        /// if the "change IP" and "change port" flags had been set in the ChangeRequest.
        /// </summary>
        public IPEndPoint ChangedAddress
        {
            get { return m_pChangedAddress; }

            set { m_pChangedAddress = value; }
        }

        /// <summary>
        /// Gets or sets how and where STUN server must send response back to STUN client.
        /// Value null means not specified.
        /// </summary>
        public STUN_t_ChangeRequest ChangeRequest
        {
            get { return m_pChangeRequest; }

            set { m_pChangeRequest = value; }
        }

        //public MessageIntegrity

        /// <summary>
        /// Gets or sets error info. Returns null if not specified.
        /// </summary>
        public STUN_t_ErrorCode ErrorCode
        {
            get { return m_pErrorCode; }

            set { m_pErrorCode = value; }
        }

        /// <summary>
        /// Gets or sets IP end point what was actually connected to STUN server. Returns null if not specified.
        /// </summary>
        public IPEndPoint MappedAddress
        {
            get { return m_pMappedAddress; }

            set { m_pMappedAddress = value; }
        }

        /// <summary>
        /// Gets or sets password. Value null means not specified.
        /// </summary>
        public string Password
        {
            get { return m_Password; }

            set { m_Password = value; }
        }

        /// <summary>
        /// Gets or sets IP endpoint from which IP end point STUN server got STUN client request.
        /// Value null means not specified.
        /// </summary>
        public IPEndPoint ReflectedFrom
        {
            get { return m_pReflectedFrom; }

            set { m_pReflectedFrom = value; }
        }

        /// <summary>
        /// Gets or sets IP end point where to STUN client likes to receive response.
        /// Value null means not specified.
        /// </summary>
        public IPEndPoint ResponseAddress
        {
            get { return m_pResponseAddress; }

            set { m_pResponseAddress = value; }
        }

        /// <summary>
        /// Gets or sets server name.
        /// </summary>
        public string ServerName
        {
            get { return m_ServerName; }

            set { m_ServerName = value; }
        }

        /// <summary>
        /// Gets or sets STUN server IP end point what sent response to STUN client. Value null
        /// means not specified.
        /// </summary>
        public IPEndPoint SourceAddress
        {
            get { return m_pSourceAddress; }

            set { m_pSourceAddress = value; }
        }

        /// <summary>
        /// Gets transaction ID.
        /// </summary>
        public Guid TransactionID
        {
            get { return m_pTransactionID; }
        }

        /// <summary>
        /// Gets STUN message type.
        /// </summary>
        public STUN_MessageType Type
        {
            get { return m_Type; }

            set { m_Type = value; }
        }

        /// <summary>
        /// Gets or sets user name. Value null means not specified.
        /// </summary>          
        public string UserName
        {
            get { return m_UserName; }

            set { m_UserName = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses STUN message from raw data packet.
        /// </summary>
        /// <param name="data">Raw STUN message.</param>
        public void Parse(byte[] data)
        {
            /* RFC 3489 11.1.             
                All STUN messages consist of a 20 byte header:

                0                   1                   2                   3
                0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |      STUN Message Type        |         Message Length        |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                        Transaction ID
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                                                               |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
              
               The message length is the count, in bytes, of the size of the
               message, not including the 20 byte header.
            */

            if (data.Length < 20)
            {
                throw new ArgumentException("Invalid STUN message value !");
            }

            int offset = 0;

            //--- message header --------------------------------------------------

            // STUN Message Type
            int messageType = (data[offset++] << 8 | data[offset++]);
            if (messageType == (int) STUN_MessageType.BindingErrorResponse)
            {
                m_Type = STUN_MessageType.BindingErrorResponse;
            }
            else if (messageType == (int) STUN_MessageType.BindingRequest)
            {
                m_Type = STUN_MessageType.BindingRequest;
            }
            else if (messageType == (int) STUN_MessageType.BindingResponse)
            {
                m_Type = STUN_MessageType.BindingResponse;
            }
            else if (messageType == (int) STUN_MessageType.SharedSecretErrorResponse)
            {
                m_Type = STUN_MessageType.SharedSecretErrorResponse;
            }
            else if (messageType == (int) STUN_MessageType.SharedSecretRequest)
            {
                m_Type = STUN_MessageType.SharedSecretRequest;
            }
            else if (messageType == (int) STUN_MessageType.SharedSecretResponse)
            {
                m_Type = STUN_MessageType.SharedSecretResponse;
            }
            else
            {
                throw new ArgumentException("Invalid STUN message type value !");
            }

            // Message Length
            int messageLength = (data[offset++] << 8 | data[offset++]);

            // Transaction ID
            byte[] guid = new byte[16];
            Array.Copy(data, offset, guid, 0, 16);
            m_pTransactionID = new Guid(guid);
            offset += 16;

            //--- Message attributes ---------------------------------------------
            while ((offset - 20) < messageLength)
            {
                ParseAttribute(data, ref offset);
            }
        }

        /// <summary>
        /// Converts this to raw STUN packet.
        /// </summary>
        /// <returns>Returns raw STUN packet.</returns>
        public byte[] ToByteData()
        {
            /* RFC 3489 11.1.             
                All STUN messages consist of a 20 byte header:

                0                   1                   2                   3
                0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |      STUN Message Type        |         Message Length        |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                        Transaction ID
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                                                               |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             
               The message length is the count, in bytes, of the size of the
               message, not including the 20 byte header.

            */

            // We allocate 512 for header, that should be more than enough.
            byte[] msg = new byte[512];

            int offset = 0;

            //--- message header -------------------------------------

            // STUN Message Type (2 bytes)
            msg[offset++] = (byte) ((int) Type >> 8);
            msg[offset++] = (byte) ((int) Type & 0xFF);

            // Message Length (2 bytes) will be assigned at last.
            msg[offset++] = 0;
            msg[offset++] = 0;

            // Transaction ID (16 bytes)
            Array.Copy(m_pTransactionID.ToByteArray(), 0, msg, offset, 16);
            offset += 16;

            //--- Message attributes ------------------------------------

            /* RFC 3489 11.2.
                After the header are 0 or more attributes.  Each attribute is TLV
                encoded, with a 16 bit type, 16 bit length, and variable value:

                0                   1                   2                   3
                0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |         Type                  |            Length             |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |                             Value                             ....
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            */

            if (MappedAddress != null)
            {
                StoreEndPoint(AttributeType.MappedAddress, MappedAddress, msg, ref offset);
            }
            else if (ResponseAddress != null)
            {
                StoreEndPoint(AttributeType.ResponseAddress, ResponseAddress, msg, ref offset);
            }
            else if (ChangeRequest != null)
            {
                /*
                    The CHANGE-REQUEST attribute is used by the client to request that
                    the server use a different address and/or port when sending the
                    response.  The attribute is 32 bits long, although only two bits (A
                    and B) are used:

                     0                   1                   2                   3
                     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 A B 0|
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

                    The meaning of the flags is:

                    A: This is the "change IP" flag.  If true, it requests the server
                       to send the Binding Response with a different IP address than the
                       one the Binding Request was received on.

                    B: This is the "change port" flag.  If true, it requests the
                       server to send the Binding Response with a different port than the
                       one the Binding Request was received on.
                */

                // Attribute header
                msg[offset++] = (int) AttributeType.ChangeRequest >> 8;
                msg[offset++] = (int) AttributeType.ChangeRequest & 0xFF;
                msg[offset++] = 0;
                msg[offset++] = 4;

                msg[offset++] = 0;
                msg[offset++] = 0;
                msg[offset++] = 0;
                msg[offset++] =
                    (byte)
                    (Convert.ToInt32(ChangeRequest.ChangeIP) << 2 |
                     Convert.ToInt32(ChangeRequest.ChangePort) << 1);
            }
            else if (SourceAddress != null)
            {
                StoreEndPoint(AttributeType.SourceAddress, SourceAddress, msg, ref offset);
            }
            else if (ChangedAddress != null)
            {
                StoreEndPoint(AttributeType.ChangedAddress, ChangedAddress, msg, ref offset);
            }
            else if (UserName != null)
            {
                byte[] userBytes = Encoding.ASCII.GetBytes(UserName);

                // Attribute header
                msg[offset++] = (int) AttributeType.Username >> 8;
                msg[offset++] = (int) AttributeType.Username & 0xFF;
                msg[offset++] = (byte) (userBytes.Length >> 8);
                msg[offset++] = (byte) (userBytes.Length & 0xFF);

                Array.Copy(userBytes, 0, msg, offset, userBytes.Length);
                offset += userBytes.Length;
            }
            else if (Password != null)
            {
                byte[] userBytes = Encoding.ASCII.GetBytes(UserName);

                // Attribute header
                msg[offset++] = (int) AttributeType.Password >> 8;
                msg[offset++] = (int) AttributeType.Password & 0xFF;
                msg[offset++] = (byte) (userBytes.Length >> 8);
                msg[offset++] = (byte) (userBytes.Length & 0xFF);

                Array.Copy(userBytes, 0, msg, offset, userBytes.Length);
                offset += userBytes.Length;
            }
            else if (ErrorCode != null)
            {
                /* 3489 11.2.9.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |                   0                     |Class|     Number    |
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |      Reason Phrase (variable)                                ..
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                */

                byte[] reasonBytes = Encoding.ASCII.GetBytes(ErrorCode.ReasonText);

                // Header
                msg[offset++] = 0;
                msg[offset++] = (int) AttributeType.ErrorCode;
                msg[offset++] = 0;
                msg[offset++] = (byte) (4 + reasonBytes.Length);

                // Empty
                msg[offset++] = 0;
                msg[offset++] = 0;
                // Class
                msg[offset++] = (byte) Math.Floor((double) (ErrorCode.Code/100));
                // Number
                msg[offset++] = (byte) (ErrorCode.Code & 0xFF);
                // ReasonPhrase
                Array.Copy(reasonBytes, msg, reasonBytes.Length);
                offset += reasonBytes.Length;
            }
            else if (ReflectedFrom != null)
            {
                StoreEndPoint(AttributeType.ReflectedFrom, ReflectedFrom, msg, ref offset);
            }

            // Update Message Length. NOTE: 20 bytes header not included.
            msg[2] = (byte) ((offset - 20) >> 8);
            msg[3] = (byte) ((offset - 20) & 0xFF);

            // Make reatval with actual size.
            byte[] retVal = new byte[offset];
            Array.Copy(msg, retVal, retVal.Length);

            return retVal;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Parses attribute from data.
        /// </summary>
        /// <param name="data">SIP message data.</param>
        /// <param name="offset">Offset in data.</param>
        private void ParseAttribute(byte[] data, ref int offset)
        {
            /* RFC 3489 11.2.
                Each attribute is TLV encoded, with a 16 bit type, 16 bit length, and variable value:

                0                   1                   2                   3
                0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |         Type                  |            Length             |
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
               |                             Value                             ....
               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+                            
            */

            // Type
            AttributeType type = (AttributeType) (data[offset++] << 8 | data[offset++]);

            // Length
            int length = (data[offset++] << 8 | data[offset++]);

            // MAPPED-ADDRESS
            if (type == AttributeType.MappedAddress)
            {
                m_pMappedAddress = ParseEndPoint(data, ref offset);
            }
                // RESPONSE-ADDRESS
            else if (type == AttributeType.ResponseAddress)
            {
                m_pResponseAddress = ParseEndPoint(data, ref offset);
            }
                // CHANGE-REQUEST
            else if (type == AttributeType.ChangeRequest)
            {
                /*
                    The CHANGE-REQUEST attribute is used by the client to request that
                    the server use a different address and/or port when sending the
                    response.  The attribute is 32 bits long, although only two bits (A
                    and B) are used:

                     0                   1                   2                   3
                     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 A B 0|
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

                    The meaning of the flags is:

                    A: This is the "change IP" flag.  If true, it requests the server
                       to send the Binding Response with a different IP address than the
                       one the Binding Request was received on.

                    B: This is the "change port" flag.  If true, it requests the
                       server to send the Binding Response with a different port than the
                       one the Binding Request was received on.
                */

                // Skip 3 bytes
                offset += 3;

                m_pChangeRequest = new STUN_t_ChangeRequest((data[offset] & 4) != 0, (data[offset] & 2) != 0);
                offset++;
            }
                // SOURCE-ADDRESS
            else if (type == AttributeType.SourceAddress)
            {
                m_pSourceAddress = ParseEndPoint(data, ref offset);
            }
                // CHANGED-ADDRESS
            else if (type == AttributeType.ChangedAddress)
            {
                m_pChangedAddress = ParseEndPoint(data, ref offset);
            }
                // USERNAME
            else if (type == AttributeType.Username)
            {
                m_UserName = Encoding.Default.GetString(data, offset, length);
                offset += length;
            }
                // PASSWORD
            else if (type == AttributeType.Password)
            {
                m_Password = Encoding.Default.GetString(data, offset, length);
                offset += length;
            }
                // MESSAGE-INTEGRITY
            else if (type == AttributeType.MessageIntegrity)
            {
                offset += length;
            }
                // ERROR-CODE
            else if (type == AttributeType.ErrorCode)
            {
                /* 3489 11.2.9.
                    0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |                   0                     |Class|     Number    |
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |      Reason Phrase (variable)                                ..
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                */

                int errorCode = (data[offset + 2] & 0x7)*100 + (data[offset + 3] & 0xFF);

                m_pErrorCode = new STUN_t_ErrorCode(errorCode,
                                                    Encoding.Default.GetString(data, offset + 4, length - 4));
                offset += length;
            }
                // UNKNOWN-ATTRIBUTES
            else if (type == AttributeType.UnknownAttribute)
            {
                offset += length;
            }
                // REFLECTED-FROM
            else if (type == AttributeType.ReflectedFrom)
            {
                m_pReflectedFrom = ParseEndPoint(data, ref offset);
            }
                // XorMappedAddress
                // XorOnly
                // ServerName
            else if (type == AttributeType.ServerName)
            {
                m_ServerName = Encoding.Default.GetString(data, offset, length);
                offset += length;
            }
                // Unknown
            else
            {
                offset += length;
            }
        }

        /// <summary>
        /// Pasrses IP endpoint attribute.
        /// </summary>
        /// <param name="data">STUN message data.</param>
        /// <param name="offset">Offset in data.</param>
        /// <returns>Returns parsed IP end point.</returns>
        private IPEndPoint ParseEndPoint(byte[] data, ref int offset)
        {
            /*
                It consists of an eight bit address family, and a sixteen bit
                port, followed by a fixed length value representing the IP address.

                0                   1                   2                   3
                0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                |x x x x x x x x|    Family     |           Port                |
                +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                |                             Address                           |
                +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            */

            // Skip family
            offset++;
            offset++;

            // Port
            int port = (data[offset++] << 8 | data[offset++]);

            // Address
            byte[] ip = new byte[4];
            ip[0] = data[offset++];
            ip[1] = data[offset++];
            ip[2] = data[offset++];
            ip[3] = data[offset++];

            return new IPEndPoint(new IPAddress(ip), port);
        }

        /// <summary>
        /// Stores ip end point attribute to buffer.
        /// </summary>
        /// <param name="type">Attribute type.</param>
        /// <param name="endPoint">IP end point.</param>
        /// <param name="message">Buffer where to store.</param>
        /// <param name="offset">Offset in buffer.</param>
        private void StoreEndPoint(AttributeType type, IPEndPoint endPoint, byte[] message, ref int offset)
        {
            /*
                It consists of an eight bit address family, and a sixteen bit
                port, followed by a fixed length value representing the IP address.

                0                   1                   2                   3
                0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                |x x x x x x x x|    Family     |           Port                |
                +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                |                             Address                           |
                +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+             
            */

            // Header
            message[offset++] = (byte) ((int) type >> 8);
            message[offset++] = (byte) ((int) type & 0xFF);
            message[offset++] = 0;
            message[offset++] = 8;

            // Unused
            message[offset++] = 0;
            // Family
            message[offset++] = (byte) IPFamily.IPv4;
            // Port
            message[offset++] = (byte) (endPoint.Port >> 8);
            message[offset++] = (byte) (endPoint.Port & 0xFF);
            // Address
            byte[] ipBytes = endPoint.Address.GetAddressBytes();
            message[offset++] = ipBytes[0];
            message[offset++] = ipBytes[1];
            message[offset++] = ipBytes[2];
            message[offset++] = ipBytes[3];
        }

        #endregion

        #region Nested type: AttributeType

        /// <summary>
        /// Specifies STUN attribute type.
        /// </summary>
        private enum AttributeType
        {
            MappedAddress = 0x0001,
            ResponseAddress = 0x0002,
            ChangeRequest = 0x0003,
            SourceAddress = 0x0004,
            ChangedAddress = 0x0005,
            Username = 0x0006,
            Password = 0x0007,
            MessageIntegrity = 0x0008,
            ErrorCode = 0x0009,
            UnknownAttribute = 0x000A,
            ReflectedFrom = 0x000B,
            XorMappedAddress = 0x8020,
            XorOnly = 0x0021,
            ServerName = 0x8022,
        }

        #endregion

        #region Nested type: IPFamily

        /// <summary>
        /// Specifies IP address family.
        /// </summary>
        private enum IPFamily
        {
            IPv4 = 0x01,
            IPv6 = 0x02,
        }

        #endregion
    }
}