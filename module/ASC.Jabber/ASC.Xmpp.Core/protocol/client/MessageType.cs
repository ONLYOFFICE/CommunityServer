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


namespace ASC.Xmpp.Core.protocol.client
{
    /// <summary>
    ///   Enumeration that represents the type of a message
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        ///   This in a normal message, much like an email. You dont expect a fast
        /// </summary>
        normal = -1,

        /// <summary>
        ///   a error messages
        /// </summary>
        error,

        /// <summary>
        ///   is for chat like messages, person to person. Send this if you expect a fast reply. reply or no reply at all.
        /// </summary>
        chat,

        /// <summary>
        ///   is used for sending/receiving messages from/to a chatroom (IRC style chats)
        /// </summary>
        groupchat,

        /// <summary>
        ///   Think of this as a news broadcast, or RRS Feed, the message will normally have a URL and Description Associated with it.
        /// </summary>
        headline
    }
}