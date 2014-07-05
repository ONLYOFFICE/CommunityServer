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

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="Features.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

namespace ASC.Xmpp.Core.protocol.iq.disco
{
    /// <summary>
    ///   Disco Features Enumeration
    /// </summary>
    public class Features
    {
        //	Application supports DNS SRV lookups of XMPP services. 	RFC 3920: XMPP Core, RFC 3921: XMPP IM
        // http://jabber.org/protocol/amp?action=alert 	Support for the "alert" action in Advanced Message Processing. 	JEP-0079: Advanced Message Processing
        public const string FEAT_AMP_ACTION_ALERT = "http://jabber.org/protocol/amp?action=alert";
        // http://jabber.org/protocol/amp?action=drop 	Support for the "drop" action in Advanced Message Processing. 	JEP-0079: Advanced Message Processing
        public const string FEAT_AMP_ACTION_DROP = "http://jabber.org/protocol/amp?action=drop";
        // http://jabber.org/protocol/amp?action=error 	Support for the "error" action in Advanced Message Processing. 	JEP-0079: Advanced Message Processing
        public const string FEAT_AMP_ACTION_ERROR = "http://jabber.org/protocol/amp?action=error ";

        /// http://jabber.org/protocol/amp?action=notify 	Support for the "notify" action in Advanced Message Processing. 	JEP-0079: Advanced Message Processing
        public const string FEAT_AMP_ACTION_NOTIFY = "http://jabber.org/protocol/amp?action=notify";

        // http://jabber.org/protocol/amp?condition=deliver 	Support for the "deliver" condition in Advanced Message Processing. 	JEP-0079: Advanced Message Processing
        public const string FEAT_AMP_CONDITION_DELIVER = "http://jabber.org/protocol/amp?condition=deliver ";
        // http://jabber.org/protocol/amp?condition=expire-at 	Support for the "expire-at" condition in Advanced Message Processing. 	JEP-0079: Advanced Message Processing
        public const string FEAT_AMP_CONDITION_EXPIRE_AT = "http://jabber.org/protocol/amp?condition=expire-at";
        // http://jabber.org/protocol/amp?condition=match-resource 	Support for the "match-resource" condition in Advanced Message Processing. 	JEP-0079: Advanced Message Processing
        public const string FEAT_AMP_CONDITION_MATCH_RESOURCE =
            "http://jabber.org/protocol/amp?condition=match-resource";

        public const string FEAT_DNSSRV = "dnssrv";
        // 	Application supports Unicode characters throughout, including in displayed text, JIDs, and passwords. 	N/A
        public const string FEAT_FULL_UNICODE = "fullunicode";
        // gc-1.0 	Support for the "groupchat 1.0" protocol. 	JEP-0045: Multi-User Chat		
        public const string FEAT_GROUPCHAT_1 = "gc-1.0";
        public const string FEAT_IPV6 = "ipv6";
        // msglog 	Application performs logging or archiving of messages. 	N/A
        public const string FEAT_MESSAGE_LOG = "msglog";
        // msgoffline 	Server stores messages offline for later delivery. 	N/A
        public const string FEAT_MESSAGE_OFFLINE = "msgoffline";

        // muc_anonymous 	Anonymous room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_ANONYMOUS = "muc_anonymous";
        // muc_hidden 	Hidden room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_HIDDEN = "muc_hidden";
        // muc_membersonly 	Members-only room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_MEMBERSONLY = "muc_membersonly";
        // muc_moderated 	Moderated room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_MODERATED = "muc_moderated";
        // muc_nonanonymous 	Non-anonymous room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_NONANONYMOUS = "muc_nonanonymous";
        // muc_open 	Open room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_OPEN = "muc_open";
        // muc_passwordprotected 	Password-protected room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_PASSWORDPROTECTED = "muc_passwordprotected";
        // muc_persistent 	Persistent room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_PERSISTANT = "muc_persistent";
        // muc_public 	Public room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_PUBLIC = "muc_public";

        // http://jabber.org/protocol/disco#publish 	Support for the "publishing" feature of service discovery. 	JEP-0030: Service Discovery

        //http://jabber.org/protocol/muc#register 	Support for the muc#register FORM_TYPE 	JEP-0045
        public const string FEAT_MUC_REGISTER = "http://jabber.org/protocol/muc#register";
        // http://jabber.org/protocol/muc#roomconfig 	Support for the muc#roomconfig FORM_TYPE 	JEP-0045
        public const string FEAT_MUC_ROOMCONFIG = "http://jabber.org/protocol/muc#roomconfig";
        // http://jabber.org/protocol/muc#roominfo 	Support for the muc#roominfo FORM_TYPE 	JEP-0045
        public const string FEAT_MUC_ROOMINFO = "http://jabber.org/protocol/muc#roominfo";
        public const string FEAT_MUC_ROOMS = "muc_rooms";
        public const string FEAT_MUC_TEMPORARY = "muc_temporary";
        // muc_unmoderated 	Unmoderated room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_UNMODERATED = "muc_unmoderated";
        // muc_unsecured 	Unsecured room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_MUC_UNSECURED = "muc_unsecured";
        public const string FEAT_OUBSUB_ITEM_IDS = "http://jabber.org/protocol/pubsub#item-ids";
        public const string FEAT_PUBLISH = "http://jabber.org/protocol/disco#publish";
        // http://jabber.org/protocol/pubsub#collections 	Collection nodes are supported. 	JEP-0060

        // http://jabber.org/protocol/pubsub#config-node 	Configuration of node options is supported. 	JEP-0060
        public const string FEAT_PUBSUB_CONFIG_NODE = "http://jabber.org/protocol/pubsub#collections";
        // http://jabber.org/protocol/pubsub#create-nodes 	Creation of nodes is supported. 	JEP-0060
        public const string FEAT_PUBSUB_CREATE_NODES = "http://jabber.org/protocol/pubsub#create-nodes";
        // http://jabber.org/protocol/pubsub#delete-any 	Any publisher may delete an item (not only the originating publisher). 	JEP-0060
        public const string FEAT_PUBSUB_DELETE_ANY = "http://jabber.org/protocol/pubsub#create-nodes";
        // http://jabber.org/protocol/pubsub#delete-nodes 	Deletion of nodes is supported. 	JEP-0060
        public const string FEAT_PUBSUB_DELETE_NODES = "http://jabber.org/protocol/pubsub#delete-nodes";
        // http://jabber.org/protocol/pubsub#instant-nodes 	Creation of instant nodes is supported. 	JEP-0060
        public const string FEAT_PUBSUB_INSTANT_NODES = "http://jabber.org/protocol/pubsub#instant-nodes";
        // http://jabber.org/protocol/pubsub#item-ids 	Publishers may specify item identifiers. 	JEP-0060
        // http://jabber.org/protocol/pubsub#leased-subscription 	Time-based subscriptions are supported. 	JEP-0060
        public const string FEAT_PUBSUB_LEASED_SUBSCRIPTION = "http://jabber.org/protocol/pubsub#leased-subscription";
        // http://jabber.org/protocol/pubsub#meta-data 	Node meta-data is supported. 	JEP-0060
        public const string FEAT_PUBSUB_META = "http://jabber.org/protocol/pubsub#meta-data";
        // http://jabber.org/protocol/pubsub#multi-subscribe 	A single entity may subscribe to a node multiple times. 	JEP-0060
        public const string FEAT_PUBSUB_MULTI_SUBSCRIBE = "http://jabber.org/protocol/pubsub#multi-subscribe";
        // http://jabber.org/protocol/pubsub#outcast-affiliation 	The outcast affiliation is supported. 	JEP-0060
        public const string FEAT_PUBSUB_OUTCAST_AFFILIATION = "http://jabber.org/protocol/pubsub#outcast-affiliation";
        // http://jabber.org/protocol/pubsub#persistent-items 	Persistent items are supported. 	JEP-0060
        public const string FEAT_PUBSUB_PERSISTENT_ITEMS = "http://jabber.org/protocol/pubsub#persistent-items";
        // http://jabber.org/protocol/pubsub#presence-notifications 	Presence-based delivery of event notifications is supported. 	JEP-0060
        public const string FEAT_PUBSUB_PRESENCE_NOTIFICATIONS =
            "http://jabber.org/protocol/pubsub#presence-notifications";

        // http://jabber.org/protocol/pubsub#publisher-affiliation 	The publisher affiliation is supported. 	JEP-0060
        public const string FEAT_PUBSUB_PUBLISHER_AFFILIATION =
            "http://jabber.org/protocol/pubsub#publisher-affiliation";

        // http://jabber.org/protocol/pubsub#purge-nodes 	Purging of nodes is supported. 	JEP-0060
        public const string FEAT_PUBSUB_PURGE_NODES = "http://jabber.org/protocol/pubsub#purge-nodes";
        // http://jabber.org/protocol/pubsub#retract-items 	Item retraction is supported. 	JEP-0060
        public const string FEAT_PUBSUB_RETRACT_ITEMS = "http://jabber.org/protocol/pubsub#retract-items";
        // http://jabber.org/protocol/pubsub#retrieve-affiliations 	Retrieval of current affiliations is supported. 	JEP-0060
        public const string FEAT_PUBSUB_RETRIEVE_AFFILIATIONS =
            "http://jabber.org/protocol/pubsub#retrieve-affiliations";

        // http://jabber.org/protocol/pubsub#retrieve-items 	Item retrieval is supported. 	JEP-0060
        public const string FEAT_PUBSUB_RETRIEVE_ITEMS = "http://jabber.org/protocol/pubsub#retrieve-items";
        // http://jabber.org/protocol/pubsub#subscribe 	Subscribing and unsubscribing are supported. 	JEP-0060
        public const string FEAT_PUBSUB_SUBSCRIBE = "http://jabber.org/protocol/pubsub#subscribe";
        // http://jabber.org/protocol/pubsub#subscription-options 	Configuration of subscription options is supported. 	JEP-0060
        public const string FEAT_PUBSUB_SUBSCRIPTIONS_OPTIONS = "http://jabber.org/protocol/pubsub#subscription-options";
        public const string FEAT_SASL_C2S = "urn:ietf:params:xml:ns:xmpp-sasl#c2s";
        // urn:ietf:params:xml:ns:xmpp-sasl#s2s 	Application supports server-to-server SASL. 	RFC 3920: XMPP Core
        public const string FEAT_SASL_S2S = "urn:ietf:params:xml:ns:xmpp-sasl#s2s";
        // http://jabber.org/protocol/waitinglist/schemes/mailto 	Waiting list service supports the mailto: URI scheme. 	JEP-0130
        // muc_semianonymous 	Semi-anonymous room in Multi-User Chat (MUC) 	JEP-0045
        public const string FEAT_SEMIANONYMOUS = "muc_semianonymous";
        // muc_temporary 	Temporary room in Multi-User Chat (MUC) 	JEP-0045

        // sslc2s 	Application supports old-style (pre-TLS) SSL connections on a dedicated port. 	N/A
        public const string FEAT_SSL_C2S = "sslc2s";
        // stringprep 	Application supports the nameprep, nodeprep, and resourceprep profiles of stringprep. 	RFC 3920: XMPP Core
        public const string FEAT_STRINGPREP = "stringprep";

        // urn:ietf:params:xml:ns:xmpp-sasl#c2s 	Application supports client-to-server SASL. 	RFC 3920: XMPP Core
        // urn:ietf:params:xml:ns:xmpp-tls#c2s 	Application supports client-to-server TLS. 	RFC 3920: XMPP Core
        public const string FEAT_TLS_C2S = "urn:ietf:params:xml:ns:xmpp-tls#c2s";
        // urn:ietf:params:xml:ns:xmpp-tls#s2s 	Application supports server-to-server TLS. 	RFC 3920: XMPP Core
        public const string FEAT_TLS_S2S = "urn:ietf:params:xml:ns:xmpp-tls#s2s";
        public const string FEAT_WAITINGLIST_MAILTO = "http://jabber.org/protocol/waitinglist/schemes/mailto";
        // http://jabber.org/protocol/waitinglist/schemes/tel 	Waiting list service supports the tel: URI scheme. 	JEP-0130
        public const string FEAT_WAITINGLIST_TEL = "http://jabber.org/protocol/waitinglist/schemes/tel";

        // xmllang 	Application supports the 'xml:lang' attribute as described in RFC 3920. 	RFC 3920: XMPP Core
        public const string FEAT_XMLLANG = "xmllang";
    }
}