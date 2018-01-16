/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.InterThreadException.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap
{
    public class InterThreadException : LdapException
    {
        /// <summary>
        ///     Returns the message ID of this message request.
        /// </summary>
        /// <returns>
        ///     the message ID.  Returns -1 if no message
        ///     is associated with this exception.
        /// </returns>
        internal virtual int MessageID
        {
            get
            {
                if (request == null)
                {
                    return -1;
                }
                return request.MessageID;
            }
        }

        /// <summary>
        ///     Returns the message type expected as a reply to
        ///     the message associated with this message's request type.
        /// </summary>
        /// <returns>
        ///     the message type of the expected reply.  Returns -1
        ///     if no reply expected.
        /// </returns>
        internal virtual int ReplyType
        {
            get
            {
                if (request == null)
                {
                    return -1;
                }
                var reqType = request.MessageType;
                var responseType = -1;
                switch (reqType)
                {
                    case LdapMessage.BIND_REQUEST:
                        responseType = LdapMessage.BIND_RESPONSE;
                        break;

                    case LdapMessage.UNBIND_REQUEST:
                        responseType = -1;
                        break;

                    case LdapMessage.SEARCH_REQUEST:
                        responseType = LdapMessage.SEARCH_RESULT;
                        break;

                    case LdapMessage.MODIFY_REQUEST:
                        responseType = LdapMessage.MODIFY_RESPONSE;
                        break;

                    case LdapMessage.ADD_REQUEST:
                        responseType = LdapMessage.ADD_RESPONSE;
                        break;

                    case LdapMessage.DEL_REQUEST:
                        responseType = LdapMessage.DEL_RESPONSE;
                        break;

                    case LdapMessage.MODIFY_RDN_REQUEST:
                        responseType = LdapMessage.MODIFY_RDN_RESPONSE;
                        break;

                    case LdapMessage.COMPARE_REQUEST:
                        responseType = LdapMessage.COMPARE_RESPONSE;
                        break;

                    case LdapMessage.ABANDON_REQUEST:
                        responseType = -1;
                        break;

                    case LdapMessage.EXTENDED_REQUEST:
                        responseType = LdapMessage.EXTENDED_RESPONSE;
                        break;
                }
                return responseType;
            }
        }

        private readonly Message request;

        /// <summary>
        ///     Constructs a InterThreadException with its associated message.
        /// </summary>
        /// <param name="message">
        ///     The text providign additional error information.
        /// </param>
        /// <param name="resultCode">
        ///     The error result code.
        /// </param>
        /// <param name="request">
        ///     The Message class associated with this exception.
        /// </param>
        internal InterThreadException(string message, object[] arguments, int resultCode, Exception rootException,
            Message request) : base(message, arguments, resultCode, null, rootException)
        {
            this.request = request;
        }
    }
}