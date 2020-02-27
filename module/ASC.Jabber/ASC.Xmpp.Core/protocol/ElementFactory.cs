/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


#region using

using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.component;
using ASC.Xmpp.Core.protocol.extensions.amp;
using ASC.Xmpp.Core.protocol.extensions.bookmarks;
using ASC.Xmpp.Core.protocol.extensions.bytestreams;
using ASC.Xmpp.Core.protocol.extensions.caps;
using ASC.Xmpp.Core.protocol.extensions.chatstates;
using ASC.Xmpp.Core.protocol.extensions.commands;
using ASC.Xmpp.Core.protocol.extensions.compression;
using ASC.Xmpp.Core.protocol.extensions.featureneg;
using ASC.Xmpp.Core.protocol.extensions.filetransfer;
using ASC.Xmpp.Core.protocol.extensions.geoloc;
using ASC.Xmpp.Core.protocol.extensions.html;
using ASC.Xmpp.Core.protocol.extensions.ibb;
using ASC.Xmpp.Core.protocol.extensions.jivesoftware.phone;
using ASC.Xmpp.Core.protocol.extensions.msgreceipts;
using ASC.Xmpp.Core.protocol.extensions.multicast;
using ASC.Xmpp.Core.protocol.extensions.nickname;
using ASC.Xmpp.Core.protocol.extensions.ping;
using ASC.Xmpp.Core.protocol.extensions.primary;
using ASC.Xmpp.Core.protocol.extensions.pubsub;
using ASC.Xmpp.Core.protocol.extensions.pubsub.owner;
using ASC.Xmpp.Core.protocol.extensions.shim;
using ASC.Xmpp.Core.protocol.extensions.si;
using ASC.Xmpp.Core.protocol.iq.agent;
using ASC.Xmpp.Core.protocol.iq.bind;
using ASC.Xmpp.Core.protocol.iq.blocklist;
using ASC.Xmpp.Core.protocol.iq.browse;
using ASC.Xmpp.Core.protocol.iq.chatmarkers;
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Core.protocol.iq.jingle;
using ASC.Xmpp.Core.protocol.iq.last;
using ASC.Xmpp.Core.protocol.iq.oob;
using ASC.Xmpp.Core.protocol.iq.privacy;
using ASC.Xmpp.Core.protocol.iq.@private;
using ASC.Xmpp.Core.protocol.iq.register;
using ASC.Xmpp.Core.protocol.iq.roster;
using ASC.Xmpp.Core.protocol.iq.rpc;
using ASC.Xmpp.Core.protocol.iq.search;
using ASC.Xmpp.Core.protocol.iq.session;
using ASC.Xmpp.Core.protocol.iq.time;
using ASC.Xmpp.Core.protocol.iq.vcard;
using ASC.Xmpp.Core.protocol.sasl;
using ASC.Xmpp.Core.protocol.stream.feature.compression;
using ASC.Xmpp.Core.protocol.tls;
using ASC.Xmpp.Core.protocol.x;
using ASC.Xmpp.Core.protocol.x.data;
using ASC.Xmpp.Core.protocol.x.muc;
using ASC.Xmpp.Core.protocol.x.muc.iq;
using ASC.Xmpp.Core.protocol.x.muc.iq.admin;
using ASC.Xmpp.Core.protocol.x.muc.iq.owner;
using ASC.Xmpp.Core.protocol.x.rosterx;
using ASC.Xmpp.Core.protocol.x.tm.history;
using ASC.Xmpp.Core.protocol.x.vcard_update;
using ASC.Xmpp.Core.utils.Xml.Dom;
using System;
using System.Collections.Generic;
using Active = ASC.Xmpp.Core.protocol.iq.privacy.Active;
using Address = ASC.Xmpp.Core.protocol.extensions.multicast.Address;
using Affiliation = ASC.Xmpp.Core.protocol.extensions.pubsub.Affiliation;
using Auth = ASC.Xmpp.Core.protocol.iq.auth.Auth;
using Avatar = ASC.Xmpp.Core.protocol.iq.avatar.Avatar;
using Conference = ASC.Xmpp.Core.protocol.x.Conference;
using Configure = ASC.Xmpp.Core.protocol.extensions.pubsub.owner.Configure;
using Data = ASC.Xmpp.Core.protocol.x.data.Data;
using Delete = ASC.Xmpp.Core.protocol.extensions.pubsub.owner.Delete;
using Event = ASC.Xmpp.Core.protocol.x.Event;
using Failure = ASC.Xmpp.Core.protocol.tls.Failure;
using History = ASC.Xmpp.Core.protocol.x.muc.History;
using IQ = ASC.Xmpp.Core.protocol.client.IQ;
using Item = ASC.Xmpp.Core.protocol.iq.privacy.Item;
using Items = ASC.Xmpp.Core.protocol.extensions.pubsub.@event.Items;
using Message = ASC.Xmpp.Core.protocol.client.Message;
using Presence = ASC.Xmpp.Core.protocol.client.Presence;
using PubSub = ASC.Xmpp.Core.protocol.extensions.pubsub.owner.PubSub;
using Purge = ASC.Xmpp.Core.protocol.extensions.pubsub.owner.Purge;
using RosterItem = ASC.Xmpp.Core.protocol.iq.roster.RosterItem;
using Status = ASC.Xmpp.Core.protocol.x.muc.Status;
using Type = System.Type;
using Version = ASC.Xmpp.Core.protocol.iq.version.Version;

#endregion

namespace ASC.Xmpp.Core.protocol
{

    #region using

    #endregion

    /// <summary>
    ///   Factory class that implements the factory pattern for builing our Elements.
    /// </summary>
    public class ElementFactory
    {
        #region Members

        /// <summary>
        ///   This Hashtable stores Mapping of protocol (tag/namespace) to the agsXMPP objects
        /// </summary>
        private static readonly Dictionary<string, Type> m_table = new Dictionary<string, Type>();

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        static ElementFactory()
        {
            AddElementType("iq", Uri.CLIENT, typeof (IQ));
            AddElementType("message", Uri.CLIENT, typeof (Message));
            AddElementType("presence", Uri.CLIENT, typeof (Presence));
            AddElementType("error", Uri.CLIENT, typeof (client.Error));

            AddElementType("agent", Uri.IQ_AGENTS, typeof (Agent));

            AddElementType("item", Uri.IQ_ROSTER, typeof (RosterItem));
            AddElementType("group", Uri.IQ_ROSTER, typeof (Group));
            AddElementType("group", Uri.X_ROSTERX, typeof (Group));

            AddElementType("item", Uri.IQ_SEARCH, typeof (SearchItem));

            // Stream stuff
            AddElementType("stream", Uri.STREAM, typeof (Stream));
            AddElementType("error", Uri.STREAM, typeof (Error));

            AddElementType("server", Uri.IQ_GOOGLE_JINGLE, typeof (Server));
            AddElementType("stun", Uri.IQ_GOOGLE_JINGLE, typeof (Stun));
            AddElementType("query", Uri.IQ_GOOGLE_JINGLE, typeof (GoogleJingle));

            AddElementType("query", Uri.IQ_AUTH, typeof (Auth));
            AddElementType("query", Uri.IQ_AGENTS, typeof (Agents));
            AddElementType("query", Uri.IQ_ROSTER, typeof (Roster));
            AddElementType("query", Uri.IQ_LAST, typeof (Last));
            AddElementType("query", Uri.IQ_VERSION, typeof (Version));
            AddElementType("query", Uri.IQ_TIME, typeof (Time));
            AddElementType("query", Uri.IQ_OOB, typeof (Oob));
            AddElementType("query", Uri.IQ_SEARCH, typeof (Search));
            AddElementType("query", Uri.IQ_BROWSE, typeof (Browse));
            AddElementType("query", Uri.IQ_AVATAR, typeof (Avatar));
            AddElementType("query", Uri.IQ_REGISTER, typeof (Register));
            AddElementType("query", Uri.IQ_PRIVATE, typeof (Private));

            AddElementType("blocklist", Uri.IQ_BLOCKLIST, typeof (Blocklist));
            AddElementType("block", Uri.IQ_BLOCKLIST, typeof (Block));
            AddElementType("unblock", Uri.IQ_BLOCKLIST, typeof (Unblock));

            // Privacy Lists
            AddElementType("query", Uri.IQ_PRIVACY, typeof (Privacy));
            AddElementType("item", Uri.IQ_PRIVACY, typeof (Item));
            AddElementType("list", Uri.IQ_PRIVACY, typeof (List));
            AddElementType("active", Uri.IQ_PRIVACY, typeof (Active));
            AddElementType("default", Uri.IQ_PRIVACY, typeof (Default));

            // Browse
            AddElementType("service", Uri.IQ_BROWSE, typeof (Service));
            AddElementType("item", Uri.IQ_BROWSE, typeof (BrowseItem));

            // Service Discovery			
            AddElementType("query", Uri.DISCO_ITEMS, typeof (DiscoItems));
            AddElementType("query", Uri.DISCO_INFO, typeof (DiscoInfo));
            AddElementType("feature", Uri.DISCO_INFO, typeof (DiscoFeature));
            AddElementType("identity", Uri.DISCO_INFO, typeof (DiscoIdentity));
            AddElementType("item", Uri.DISCO_ITEMS, typeof (DiscoItem));

            AddElementType("x", Uri.X_DELAY, typeof (Delay));
            AddElementType("x", Uri.X_AVATAR, typeof (x.Avatar));
            AddElementType("x", Uri.X_CONFERENCE, typeof (Conference));
            AddElementType("x", Uri.X_EVENT, typeof (Event));

            // AddElementType("x",					Uri.STORAGE_AVATAR,	typeof(agsXMPP.protocol.storage.Avatar));
            AddElementType("query", Uri.STORAGE_AVATAR, typeof (storage.Avatar));

            // XData Stuff
            AddElementType("x", Uri.X_DATA, typeof (x.data.Data));
            AddElementType("field", Uri.X_DATA, typeof (Field));
            AddElementType("option", Uri.X_DATA, typeof (Option));
            AddElementType("value", Uri.X_DATA, typeof (Value));
            AddElementType("reported", Uri.X_DATA, typeof (Reported));
            AddElementType("item", Uri.X_DATA, typeof (x.data.Item));

            AddElementType("features", Uri.STREAM, typeof (Features));

            AddElementType("register", Uri.FEATURE_IQ_REGISTER, typeof (stream.feature.Register));
            AddElementType("compression", Uri.FEATURE_COMPRESS, typeof (Compression));
            AddElementType("method", Uri.FEATURE_COMPRESS, typeof (Method));

            AddElementType("jingle", Uri.IQ_JINGLE1, typeof (Jingle));
            AddElementType("jingle", Uri.IQ_JINGLE0, typeof (Jingle));
            AddElementType("bind", Uri.BIND, typeof (Bind));
            AddElementType("unbind", Uri.BIND, typeof (Bind));
            AddElementType("session", Uri.SESSION, typeof (Session));

            // TLS stuff
            AddElementType("failure", Uri.TLS, typeof (Failure));
            AddElementType("proceed", Uri.TLS, typeof (Proceed));
            AddElementType("starttls", Uri.TLS, typeof (StartTls));

            // SASL stuff
            AddElementType("mechanisms", Uri.SASL, typeof (Mechanisms));
            AddElementType("mechanism", Uri.SASL, typeof (Mechanism));
            AddElementType("auth", Uri.SASL, typeof (sasl.Auth));
            AddElementType("x-tmtoken", Uri.SASL, typeof (TMToken)); //TeamLab token
            AddElementType("response", Uri.SASL, typeof (Response));
            AddElementType("challenge", Uri.SASL, typeof (Challenge));

            // TODO, this is a dirty hacks for the buggy BOSH Proxy
            // BEGIN
            AddElementType("challenge", Uri.CLIENT, typeof (Challenge));
            AddElementType("success", Uri.CLIENT, typeof (Success));

            // END
            AddElementType("failure", Uri.SASL, typeof (sasl.Failure));
            AddElementType("abort", Uri.SASL, typeof (Abort));
            AddElementType("success", Uri.SASL, typeof (Success));

            // Vcard stuff
            AddElementType("vCard", Uri.VCARD, typeof (Vcard));
            AddElementType("TEL", Uri.VCARD, typeof (Telephone));
            AddElementType("ORG", Uri.VCARD, typeof (Organization));
            AddElementType("N", Uri.VCARD, typeof (Name));
            AddElementType("EMAIL", Uri.VCARD, typeof (Email));
            AddElementType("ADR", Uri.VCARD, typeof (Address));
#if !CF
            AddElementType("PHOTO", Uri.VCARD, typeof (Photo));
#endif

            // Server stuff
            // AddElementType("stream",            Uri.SERVER,                 typeof(agsXMPP.protocol.server.Stream));
            // AddElementType("message",           Uri.SERVER,                 typeof(agsXMPP.protocol.server.Message));

            // Component stuff
            AddElementType("handshake", Uri.ACCEPT, typeof (Handshake));
            AddElementType("log", Uri.ACCEPT, typeof (Log));
            AddElementType("route", Uri.ACCEPT, typeof (Route));
            AddElementType("iq", Uri.ACCEPT, typeof (component.IQ));
            AddElementType("message", Uri.ACCEPT, typeof (component.Message));
            AddElementType("presence", Uri.ACCEPT, typeof (component.Presence));
            AddElementType("error", Uri.ACCEPT, typeof (component.Error));

            // Extensions (JEPS)
            AddElementType("headers", Uri.SHIM, typeof (Header));
            AddElementType("header", Uri.SHIM, typeof (Headers));
            AddElementType("roster", Uri.ROSTER_DELIMITER, typeof (Delimiter));
            AddElementType("p", Uri.PRIMARY, typeof (Primary));
            AddElementType("nick", Uri.NICK, typeof (Nickname));

            AddElementType("item", Uri.X_ROSTERX, typeof (x.rosterx.RosterItem));
            AddElementType("x", Uri.X_ROSTERX, typeof (RosterX));

            // Filetransfer stuff
            AddElementType("file", Uri.SI_FILE_TRANSFER, typeof (File));
            AddElementType("range", Uri.SI_FILE_TRANSFER, typeof (Range));

            // FeatureNeg
            AddElementType("feature", Uri.FEATURE_NEG, typeof (FeatureNeg));

            // Bytestreams
            AddElementType("query", Uri.BYTESTREAMS, typeof (ByteStream));
            AddElementType("streamhost", Uri.BYTESTREAMS, typeof (StreamHost));
            AddElementType("streamhost-used", Uri.BYTESTREAMS, typeof (StreamHostUsed));
            AddElementType("activate", Uri.BYTESTREAMS, typeof (Activate));
            AddElementType("udpsuccess", Uri.BYTESTREAMS, typeof (UdpSuccess));

            AddElementType("si", Uri.SI, typeof (SI));

            AddElementType("html", Uri.XHTML_IM, typeof (Html));
            AddElementType("body", Uri.XHTML, typeof (Body));

            AddElementType("compressed", Uri.COMPRESS, typeof (Compressed));
            AddElementType("compress", Uri.COMPRESS, typeof (Compress));
            AddElementType("failure", Uri.COMPRESS, typeof (extensions.compression.Failure));

            // MUC (JEP-0045 Multi User Chat)
            AddElementType("x", Uri.MUC, typeof (Muc));
            AddElementType("x", Uri.MUC_USER, typeof (User));
            AddElementType("item", Uri.MUC_USER, typeof (x.muc.Item));
            AddElementType("status", Uri.MUC_USER, typeof (Status));
            AddElementType("invite", Uri.MUC_USER, typeof (Invite));
            AddElementType("decline", Uri.MUC_USER, typeof (Decline));
            AddElementType("actor", Uri.MUC_USER, typeof (Actor));
            AddElementType("history", Uri.MUC, typeof (History));
            AddElementType("query", Uri.MUC_ADMIN, typeof (Admin));
            AddElementType("item", Uri.MUC_ADMIN, typeof (x.muc.iq.admin.Item));
            AddElementType("query", Uri.MUC_OWNER, typeof (Owner));
            AddElementType("destroy", Uri.MUC_OWNER, typeof (Destroy));
            AddElementType("unique", Uri.MUC_UNIQUE, typeof (Unique));

            //Jabber xep-003 Addressing
            AddElementType("addresses", Uri.ADDRESS, typeof (Addresses));
            AddElementType("address", Uri.ADDRESS, typeof (Address));


            // Jabber RPC JEP 0009            
            AddElementType("query", Uri.IQ_RPC, typeof (Rpc));
            AddElementType("methodCall", Uri.IQ_RPC, typeof (MethodCall));
            AddElementType("methodResponse", Uri.IQ_RPC, typeof (MethodResponse));

            // Chatstates Jep-0085
            AddElementType("active", Uri.CHATSTATES, typeof (extensions.chatstates.Active));
            AddElementType("inactive", Uri.CHATSTATES, typeof (Inactive));
            AddElementType("composing", Uri.CHATSTATES, typeof (Composing));
            AddElementType("paused", Uri.CHATSTATES, typeof (Paused));
            AddElementType("gone", Uri.CHATSTATES, typeof (Gone));

            // Jivesoftware Extenstions
            AddElementType("phone-event", Uri.JIVESOFTWARE_PHONE, typeof (PhoneEvent));
            AddElementType("phone-action", Uri.JIVESOFTWARE_PHONE, typeof (PhoneAction));
            AddElementType("phone-status", Uri.JIVESOFTWARE_PHONE, typeof (PhoneStatus));

            // Jingle stuff is in heavy development, we commit this once the most changes on the Jeps are done            
            // AddElementType("jingle",            Uri.JINGLE,                 typeof(agsXMPP.protocol.extensions.jingle.Jingle));
            // AddElementType("candidate",         Uri.JINGLE,                 typeof(agsXMPP.protocol.extensions.jingle.Candidate));
            AddElementType("c", Uri.CAPS, typeof (Capabilities));

            AddElementType("geoloc", Uri.GEOLOC, typeof (GeoLoc));

            // Xmpp Ping
            AddElementType("ping", Uri.PING, typeof (Ping));

            // Ad-Hock Commands
            AddElementType("command", Uri.COMMANDS, typeof (Command));
            AddElementType("actions", Uri.COMMANDS, typeof (Actions));
            AddElementType("note", Uri.COMMANDS, typeof (Note));

            // **********
            // * PubSub *
            // **********
            // Owner namespace
            AddElementType("affiliate", Uri.PUBSUB_OWNER, typeof (Affiliate));
            AddElementType("affiliates", Uri.PUBSUB_OWNER, typeof (Affiliates));
            AddElementType("configure", Uri.PUBSUB_OWNER, typeof (Configure));
            AddElementType("delete", Uri.PUBSUB_OWNER, typeof (Delete));
            AddElementType("pending", Uri.PUBSUB_OWNER, typeof (Pending));
            AddElementType("pubsub", Uri.PUBSUB_OWNER, typeof (PubSub));
            AddElementType("purge", Uri.PUBSUB_OWNER, typeof (Purge));
            AddElementType("subscriber", Uri.PUBSUB_OWNER, typeof (Subscriber));
            AddElementType("subscribers", Uri.PUBSUB_OWNER, typeof (Subscribers));

            // Event namespace
            AddElementType("delete", Uri.PUBSUB_EVENT, typeof (extensions.pubsub.@event.Delete));
            AddElementType("event", Uri.PUBSUB_EVENT, typeof (extensions.pubsub.@event.Event));
            AddElementType("item", Uri.PUBSUB_EVENT, typeof (extensions.pubsub.@event.Item));
            AddElementType("items", Uri.PUBSUB_EVENT, typeof (Items));
            AddElementType("purge", Uri.PUBSUB_EVENT, typeof (extensions.pubsub.@event.Purge));

            // Main Pubsub namespace
            AddElementType("affiliation", Uri.PUBSUB, typeof (Affiliation));
            AddElementType("affiliations", Uri.PUBSUB, typeof (Affiliations));
            AddElementType("configure", Uri.PUBSUB, typeof (extensions.pubsub.Configure));
            AddElementType("create", Uri.PUBSUB, typeof (Create));
            AddElementType("configure", Uri.PUBSUB, typeof (extensions.pubsub.Configure));
            AddElementType("item", Uri.PUBSUB, typeof (extensions.pubsub.Item));
            AddElementType("items", Uri.PUBSUB, typeof (extensions.pubsub.Items));
            AddElementType("options", Uri.PUBSUB, typeof (Options));
            AddElementType("publish", Uri.PUBSUB, typeof (Publish));
            AddElementType("pubsub", Uri.PUBSUB, typeof (extensions.pubsub.PubSub));
            AddElementType("retract", Uri.PUBSUB, typeof (Retract));
            AddElementType("subscribe", Uri.PUBSUB, typeof (Subscribe));
            AddElementType("subscribe-options", Uri.PUBSUB, typeof (SubscribeOptions));
            AddElementType("subscription", Uri.PUBSUB, typeof (Subscription));
            AddElementType("subscriptions", Uri.PUBSUB, typeof (Subscriptions));
            AddElementType("unsubscribe", Uri.PUBSUB, typeof (Unsubscribe));

            // HTTP Binding XEP-0124
            AddElementType("body", Uri.HTTP_BIND, typeof (extensions.bosh.Body));

            // Message receipts XEP-0184
            AddElementType("received", Uri.MSG_RECEIPT, typeof (Received));
            AddElementType("request", Uri.MSG_RECEIPT, typeof (Request));

            // Bookmark storage XEP-0048         
            AddElementType("storage", Uri.STORAGE_BOOKMARKS, typeof (Storage));
            AddElementType("url", Uri.STORAGE_BOOKMARKS, typeof (Url));
            AddElementType("conference",
                           Uri.STORAGE_BOOKMARKS,
                           typeof (extensions.bookmarks.Conference));

            // XEP-0047: In-Band Bytestreams (IBB)
            AddElementType("open", Uri.IBB, typeof (Open));
            AddElementType("data", Uri.IBB, typeof (extensions.ibb.Data));
            AddElementType("close", Uri.IBB, typeof (Close));

            // XEP-0153: vCard-Based Avatars
            AddElementType("x", Uri.VCARD_UPDATE, typeof (VcardUpdate));

            // AMP
            AddElementType("amp", Uri.AMP, typeof (Amp));
            AddElementType("rule", Uri.AMP, typeof (Rule));

            // XEP-0202: Entity Time
            AddElementType("time", Uri.ENTITY_TIME, typeof (EntityTime));

            //Team lab
            AddElementType("query", Uri.X_TM_IQ_HISTORY, typeof (x.tm.history.History));
            AddElementType("item", Uri.X_TM_IQ_HISTORY, typeof (HistoryItem));

            AddElementType("query", Uri.MSG_CHAT_MARKERS, typeof(Chatmarkers));
            AddElementType("item", Uri.MSG_CHAT_MARKERS, typeof(Chatmarkers));

            AddElementType("query", Uri.X_TM_IQ_PRIVATELOG, typeof (PrivateLog));
            AddElementType("item", Uri.X_TM_IQ_PRIVATELOG, typeof (PrivateLogItem));
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Adds new Element Types to the Hashtable Use this function also to register your own created Elements. If a element is already registered it gets overwritten. This behaviour is also useful if you you want to overwrite classes and add your own derived classes to the factory.
        /// </summary>
        /// <param name="tag"> FQN </param>
        /// <param name="ns"> </param>
        /// <param name="t"> </param>
        public static void AddElementType(string tag, string ns, Type t)
        {
            var et = new ElementType(tag, ns);
            string key = et.ToString();

            // added thread safety on a user request
            lock (m_table)
            {
                m_table[key] = t;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="prefix"> </param>
        /// <param name="tag"> </param>
        /// <param name="ns"> </param>
        /// <returns> </returns>
        public static Element GetElement(string prefix, string tag, string ns)
        {
            if (ns == null)
            {
                ns = string.Empty;
            }

            var et = new ElementType(tag, ns);
            var key = et.ToString();

            Element ret;
            if (m_table.ContainsKey(key))
            {
                ret = (Element) Activator.CreateInstance(m_table[key]);
            }
            else
            {
                ret = new Element(tag);
            }

            ret.Prefix = prefix;

            if (ns != string.Empty)
            {
                ret.Namespace = ns;
            }

            return ret;
        }

        public static string GetElementNamespace(Type type)
        {
            foreach (var entry in m_table)
            {
                if (entry.Value == type)
                {
                    var name = (string) entry.Key;
                    return name.Contains(":") ? name.Substring(0, name.LastIndexOf(":")) : null;
                }
            }
            return null;
        }

        #endregion
    }
}