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


using System;
using System.Runtime.Serialization;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.utils.Xml.Dom;
using StanzaError = ASC.Xmpp.Core.protocol.client.Error;
using StreamError = ASC.Xmpp.Core.protocol.Error;

namespace ASC.Xmpp.Server
{
    public class JabberException : Exception
    {
        public StreamErrorCondition StreamErrorCondition
        {
            get;
            private set;
        }


        public ErrorCode ErrorCode
        {
            get;
            private set;
        }

        public bool CloseStream
        {
            get;
            private set;
        }

        public bool StreamError
        {
            get;
            private set;
        }

        public JabberException(string message, Exception innerException)
            : base(message, innerException)
        {
            StreamError = false;
            ErrorCode = ErrorCode.InternalServerError;
        }

        public JabberException(StreamErrorCondition streamErrorCondition)
            : this(streamErrorCondition, true)
        {

        }

        public JabberException(StreamErrorCondition streamErrorCondition, bool closeStream)
            : base()
        {
            StreamError = true;
            CloseStream = closeStream;
            this.StreamErrorCondition = streamErrorCondition;
        }

        public JabberException(ErrorCode errorCode)
            : base()
        {
            StreamError = false;
            CloseStream = false;
            this.ErrorCode = errorCode;
        }

        protected JabberException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public virtual Element ToElement()
        {
            return StreamError ? (Element)new StreamError(StreamErrorCondition) : (Element)new StanzaError(ErrorCode);
        }
    }
}
