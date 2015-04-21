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


namespace ASC.Xmpp.Core.protocol
{
    /// <summary>
    /// </summary>
    public class Uri
    {
        #region Constants

        /// <summary>
        ///   jabber:component:accept
        /// </summary>
        public const string ACCEPT = "jabber:component:accept";

        public const string ADDRESS = "http://jabber.org/protocol/address";

        /// <summary>
        ///   <para></para> <para>http://jabber.org/protocol/amp</para>
        /// </summary>
        public const string AMP = "http://jabber.org/protocol/amp";

        /// <summary>
        /// </summary>
        public const string BIND = "urn:ietf:params:xml:ns:xmpp-bind";

        /// <summary>
        ///   JEP-0065 SOCKS5 bytestreams http://jabber.org/protocol/bytestreams
        /// </summary>
        public const string BYTESTREAMS = "http://jabber.org/protocol/bytestreams";

        /// <summary>
        ///   XEP-0115: Entity Capabilities (http://jabber.org/protocol/caps)
        /// </summary>
        public const string CAPS = "http://jabber.org/protocol/caps";

        /// <summary>
        ///   JEP-0085 Chat State Notifications http://jabber.org/protocol/chatstates
        /// </summary>
        public const string CHATSTATES = "http://jabber.org/protocol/chatstates";

        /// <summary>
        /// </summary>
        public const string CLIENT = "jabber:client";

        /// <summary>
        ///   Ad-Hoc Commands (http://jabber.org/protocol/commands)
        /// </summary>
        public const string COMMANDS = "http://jabber.org/protocol/commands";

        /// <summary>
        ///   JEP-0138: Stream Compression
        /// </summary>
        public const string COMPRESS = "http://jabber.org/protocol/compress";

        /// <summary>
        /// </summary>
        public const string DISCO_INFO = "http://jabber.org/protocol/disco#info";

        /// <summary>
        /// </summary>
        public const string DISCO_ITEMS = "http://jabber.org/protocol/disco#items";

        /// <summary>
        ///   XEP-0202: Entity Time urn:xmpp:time
        /// </summary>
        public const string ENTITY_TIME = "urn:xmpp:time";

        /// <summary>
        ///   Stream Compression http://jabber.org/features/compress
        /// </summary>
        public const string FEATURE_COMPRESS = "http://jabber.org/features/compress";

        /// <summary>
        /// </summary>
        public const string FEATURE_IQ_REGISTER = "http://jabber.org/features/iq-register";

        /// <summary>
        ///   JEP-0020: Feature Negotiation http://jabber.org/protocol/feature-neg
        /// </summary>
        public const string FEATURE_NEG = "http://jabber.org/protocol/feature-neg";

        /// <summary>
        ///   GeoLoc (http://jabber.org/protocol/geoloc)
        /// </summary>
        public const string GEOLOC = "http://jabber.org/protocol/geoloc";

        /// <summary>
        /// </summary>
        public const string HTTP_BIND = "http://jabber.org/protocol/httpbind";

        /// <summary>
        ///   <para>XEP-0047: In-Band Bytestreams (IBB)</para> <para>http://jabber.org/protocol/ibb</para>
        /// </summary>
        public const string IBB = "http://jabber.org/protocol/ibb";

        /// <summary>
        /// </summary>
        public const string IQ_AGENTS = "jabber:iq:agents";

        /// <summary>
        /// </summary>
        public const string IQ_AUTH = "jabber:iq:auth";

        /// <summary>
        /// </summary>
        public const string IQ_AVATAR = "jabber:iq:avatar";

        /// <summary>
        /// </summary>
        public const string IQ_BLOCKLIST = "urn:xmpp:blocking";

        /// <summary>
        /// </summary>
        public const string IQ_BROWSE = "jabber:iq:browse";

        public const string IQ_GOOGLE_JINGLE = "google:jingleinfo";
        public const string IQ_JINGLE0 = "urn:xmpp:jingle:0";
        public const string IQ_JINGLE1 = "urn:xmpp:jingle:1";

        /// <summary>
        /// </summary>
        public const string IQ_LAST = "jabber:iq:last";

        /// <summary>
        /// </summary>
        public const string IQ_OOB = "jabber:iq:oob";

        /// <summary>
        /// </summary>
        public const string IQ_PRIVACY = "jabber:iq:privacy";

        /// <summary>
        /// </summary>
        public const string IQ_PRIVATE = "jabber:iq:private";

        /// <summary>
        /// </summary>
        public const string IQ_REGISTER = "jabber:iq:register";

        /// <summary>
        /// </summary>
        public const string IQ_ROSTER = "jabber:iq:roster";

        /// <summary>
        ///   JEP-0009: Jabber-RPC
        /// </summary>
        public const string IQ_RPC = "jabber:iq:rpc";

        /// <summary>
        /// </summary>
        public const string IQ_SEARCH = "jabber:iq:search";

        /// <summary>
        /// </summary>
        public const string IQ_TIME = "jabber:iq:time";

        /// <summary>
        /// </summary>
        public const string IQ_VERSION = "jabber:iq:version";

        /// <summary>
        ///   Jingle http://jabber.org/protocol/jingle
        /// </summary>
        public const string JINGLE = "http://jabber.org/protocol/jingle";

        /// <summary>
        ///   Jingle audio format description http://jabber.org/protocol/jingle/description/audio
        /// </summary>
        public const string JINGLE_AUDIO_DESCRIPTION = "http://jabber.org/protocol/jingle/description/audio";

        /// <summary>
        ///   Jingle Info audio http://jabber.org/protocol/jingle/info/audio;
        /// </summary>
        public const string JINGLE_AUDIO_INFO = "http://jabber.org/protocol/jingle/info/audio";

        /// <summary>
        /// </summary>
        public const string JINGLE_VIDEO_DESCRIPTION = "http://jabber.org/protocol/jingle/description/video";

        /// <summary>
        ///   Jivesoftware asterisk-im extension (http://jivesoftware.com/xmlns/phone);
        /// </summary>
        public const string JIVESOFTWARE_PHONE = "http://jivesoftware.com/xmlns/phone";

        /// <summary>
        ///   <para>XEP-0184: Message Receipts</para> <para>urn:xmpp:receipts</para>
        /// </summary>
        public const string MSG_RECEIPT = "urn:xmpp:receipts";

        /// <summary>
        ///   Multi User Chat (MUC) JEP-0045 http://jabber.org/protocol/muc
        /// </summary>
        public const string MUC = "http://jabber.org/protocol/muc";

        /// <summary>
        ///   "http://jabber.org/protocol/muc#admin
        /// </summary>
        public const string MUC_ADMIN = "http://jabber.org/protocol/muc#admin";

        /// <summary>
        ///   http://jabber.org/protocol/muc#owner
        /// </summary>
        public const string MUC_OWNER = "http://jabber.org/protocol/muc#owner";

        public const string MUC_UNIQUE = "http://jabber.org/protocol/muc#unique";

        /// <summary>
        ///   http://jabber.org/protocol/muc#user
        /// </summary>
        public const string MUC_USER = "http://jabber.org/protocol/muc#user";

        /// <summary>
        ///   JEP-0172 User nickname http://jabber.org/protocol/nick
        /// </summary>
        public const string NICK = "http://jabber.org/protocol/nick";

        /// <summary>
        ///   <para>XMPP ping</para> <para>Namespace: urn:xmpp:ping</para> <para>
        ///                                                                  <seealso
        ///                                                                    cref="http://www.xmpp.org/extensions/xep-0199.html">http://www.xmpp.org/extensions/xep-0199.html</seealso>
        ///                                                                </para>
        /// </summary>
        public const string PING = "urn:xmpp:ping";

        /// <summary>
        ///   "stream" namespace prefix
        /// </summary>
        public const string PREFIX = "stream";

        /// <summary>
        /// </summary>
        public const string PRIMARY = "http://jabber.org/protocol/primary";

        // Pubsub stuff
        /// <summary>
        /// </summary>
        public const string PUBSUB = "http://jabber.org/protocol/pubsub";

        /// <summary>
        /// </summary>
        public const string PUBSUB_EVENT = "http://jabber.org/protocol/pubsub#event";

        /// <summary>
        /// </summary>
        public const string PUBSUB_OWNER = "http://jabber.org/protocol/pubsub#owner";

        /// <summary>
        /// </summary>
        public const string ROSTER_DELIMITER = "roster:delimiter";

        /// <summary>
        /// </summary>
        public const string SASL = "urn:ietf:params:xml:ns:xmpp-sasl";

        /// <summary>
        /// </summary>
        public const string SERVER = "jabber:server";

        /// <summary>
        /// </summary>
        public const string SESSION = "urn:ietf:params:xml:ns:xmpp-session";

        /// <summary>
        /// </summary>
        public const string SHIM = "http://jabber.org/protocol/shim";

        /// <summary>
        ///   JEO-0095 http://jabber.org/protocol/si
        /// </summary>
        public const string SI = "http://jabber.org/protocol/si";

        /// <summary>
        ///   JEO-0096 http://jabber.org/protocol/si/profile/file-transfer
        /// </summary>
        public const string SI_FILE_TRANSFER = "http://jabber.org/protocol/si/profile/file-transfer";

        /// <summary>
        /// </summary>
        public const string STANZAS = "urn:ietf:params:xml:ns:xmpp-stanzas";

        /// <summary>
        /// </summary>
        public const string STORAGE_AVATAR = "storage:client:avatar";

        // Http-Binding XEP-0124

        /// <summary>
        ///   <para>XEP-0048: Bookmark Storage</para> <para>storage:bookmarks</para>
        /// </summary>
        public const string STORAGE_BOOKMARKS = "storage:bookmarks";

        /// <summary>
        /// </summary>
        public const string STREAM = "http://etherx.jabber.org/streams";

        /// <summary>
        ///   urn:ietf:params:xml:ns:xmpp-streams
        /// </summary>
        public const string STREAMS = "urn:ietf:params:xml:ns:xmpp-streams";

        /// <summary>
        /// </summary>
        public const string TLS = "urn:ietf:params:xml:ns:xmpp-tls";

        /// <summary>
        /// </summary>
        public const string VCARD = "vcard-temp";

        /// <summary>
        ///   <para>XEP-0153: vCard-Based Avatars</para> <para>vcard-temp:x:update</para>
        /// </summary>
        public const string VCARD_UPDATE = "vcard-temp:x:update";

        /// <summary>
        /// </summary>
        public const string X_AVATAR = "jabber:x:avatar";

        /// <summary>
        /// </summary>
        public const string X_CONFERENCE = "jabber:x:conference";

        /// <summary>
        ///   jabber:x:data
        /// </summary>
        public const string X_DATA = "jabber:x:data";

        /// <summary>
        /// </summary>
        public const string X_DELAY = "jabber:x:delay";

        /// <summary>
        /// </summary>
        public const string X_EVENT = "jabber:x:event";

        /// <summary>
        ///   JEP-0144 Roster Item Exchange
        /// </summary>
        public const string X_ROSTERX = "http://jabber.org/protocol/rosterx";

        public const string X_TM_IQ_HISTORY = "x:xmpp:tm:history";
        public const string X_TM_IQ_PRIVATELOG = "x:xmpp:tm:privatelog";

        /// <summary>
        /// </summary>
        public const string XHTML = "http://www.w3.org/1999/xhtml";

        /// <summary>
        ///   JEP-0071: XHTML-IM (http://jivesoftware.com/xmlns/phone)
        /// </summary>
        public const string XHTML_IM = "http://jabber.org/protocol/xhtml-im";

        #endregion
    }
}