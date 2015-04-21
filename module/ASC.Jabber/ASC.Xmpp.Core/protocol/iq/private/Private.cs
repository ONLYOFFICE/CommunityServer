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


using ASC.Xmpp.Core.protocol.extensions.bookmarks;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.iq.@private
{
    /// <summary>
    ///   Private XML Storage JEP-0049
    /// </summary>
    /// <remarks>
    ///   A Jabber client can store any arbitrary XML on the server side by sending an iq stanza of type "set" to the server with a query child scoped by the 'jabber:iq:private' namespace. The query element MAY contain any arbitrary XML fragment as long as the root element of that fragment is scoped by its own namespace. The data can then be retrieved by sending an iq stanza of type "get" with a query child scoped by the 'jabber:iq:private' namespace, which in turn contains a child element scoped by the namespace used for storage of that fragment. Using this method, Jabber entities can store private data on the server and retrieve it whenever necessary. The data stored might be anything, as long as it is valid XML. One typical usage for this namespace is the server-side storage of client-specific preferences; another is Bookmark Storage.
    /// </remarks>
    public class Private : Element
    {
        public Private()
        {
            TagName = "query";
            Namespace = Uri.IQ_PRIVATE;
        }

        /// <summary>
        ///   The <see cref="extensions.bookmarks.Storage">Storage</see> object
        /// </summary>
        public Storage Storage
        {
            get { return SelectSingleElement(typeof (Storage)) as Storage; }
            set
            {
                if (HasTag(typeof (Storage)))
                    RemoveTag(typeof (Storage));

                if (value != null)
                    AddChild(value);
            }
        }
    }
}