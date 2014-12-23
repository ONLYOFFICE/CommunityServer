/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2008 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */ 

using System;

using ASC.Xmpp.protocol.client;
using ASC.Xmpp.protocol.iq.disco;

namespace ASC.Xmpp.protocol.iq.disco
{
    using client;

    public class DiscoManager
    {
        private XmppClientConnection	xmppConnection	= null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="con"></param>
        public DiscoManager(XmppClientConnection con)
        {
            xmppConnection = con;
            xmppConnection.OnIq += new IqHandler(OnIq);
        }

        #region << Properties >>
        private bool m_AutoAnswerDiscoInfoRequests = true;  

        /// <summary>
        /// Automatically answer DiscoInfo requests.
        /// Set disco information (identties and features) in the DiscoInfo property object.        
        /// </summary>
        public bool AutoAnswerDiscoInfoRequests
        {
            get { return m_AutoAnswerDiscoInfoRequests; }
            set { m_AutoAnswerDiscoInfoRequests = value; }
        }
        #endregion

        private void OnIq(object sender, IQ iq)
        {
            // DiscoInfo
            if (m_AutoAnswerDiscoInfoRequests && iq.Query is DiscoInfo && iq.Type == IqType.get)
                ProcessDiscoInfo(iq);
        }

        private void ProcessDiscoInfo(IQ iq)
        {            
            IQ diiq = new IQ();
            diiq.To = iq.From;
            diiq.Id = iq.Id;
            diiq.Type = IqType.result;

            diiq.Query = xmppConnection.DiscoInfo;

            xmppConnection.Send(diiq);        
        }

        #region << Discover Info >>
        public void DiscoverInformation(Jid to)
        {
            DiscoverInformation(to, null, null, null, null);
        }

        public void DiscoverInformation(Jid to, Jid from)
        {
            DiscoverInformation(to, from, null, null, null);
        }

        public void DiscoverInformation(Jid to, IqCB cb)
        {
            DiscoverInformation(to, null, null, cb, null);
        }

        public void DiscoverInformation(Jid to, Jid from, IqCB cb)
        {
            DiscoverInformation(to, from, null, cb, null);
        }

        public void DiscoverInformation(Jid to, IqCB cb, object cbArgs)
        {
            DiscoverInformation(to, null, null, cb, cbArgs);
        }

        public void DiscoverInformation(Jid to, Jid from, IqCB cb, object cbArgs)
        {
            DiscoverInformation(to, from, null, cb, cbArgs);
        }

        public void DiscoverInformation(Jid to, string node)
        {
            DiscoverInformation(to, null, node, null, null);
        }

        public void DiscoverInformation(Jid to, Jid from, string node)
        {
            DiscoverInformation(to, from, node, null, null);
        }

        public void DiscoverInformation(Jid to, string node, IqCB cb)
        {
            DiscoverInformation(to, null, node, cb, null);
        }

        public void DiscoverInformation(Jid to, Jid from, string node, IqCB cb)
        {
            DiscoverInformation(to, from, node, cb, null);
        }

        public void DiscoverInformation(Jid to, string node, IqCB cb, object cbArgs)
        {
            DiscoverInformation(to, null, node, cb, cbArgs);
        }
        public void DiscoverInformation(Jid to, Jid from, string node, IqCB cb, object cbArgs)
        {
            /*
            
            Example 9. Querying a specific JID and node combination
            
            <iq type='get'
                from='romeo@montague.net/orchard'
                to='mim.shakespeare.lit'
                id='info3'>
              <query xmlns='http://jabber.org/protocol/disco#info' 
                     node='http://jabber.org/protocol/commands'/>
            </iq>
                  

            Example 10. JID+node result

            <iq type='result'
                from='mim.shakespeare.lit'
                to='romeo@montague.net/orchard'
                id='info3'>
              <query xmlns='http://jabber.org/protocol/disco#info' 
                     node='http://jabber.org/protocol/commands'>
                <identity
                    category='automation'
                    type='command-list'/>
              </query>
            </iq>
            */
            DiscoInfoIq discoIq = new DiscoInfoIq(IqType.get);
            discoIq.To = to;

            if (from != null)
                discoIq.From = from;

            if (node != null && node.Length > 0)
                discoIq.Query.Node = node;
            
            xmppConnection.IqGrabber.SendIq(discoIq, cb, cbArgs);
        }
        #endregion

        #region << Discover Items >>
        public void DiscoverItems(Jid to)
        {
            DiscoverItems(to, null, null, null);
        }

        public void DiscoverItems(Jid to, Jid from)
        {
            DiscoverItems(to, from, null, null, null);
        }

        public void DiscoverItems(Jid to, IqCB cb)
        {
            DiscoverItems(to, null, null, cb, null);
        }

        public void DiscoverItems(Jid to, Jid from, IqCB cb)
        {
            DiscoverItems(to, from, null, cb, null);
        }

        public void DiscoverItems(Jid to, IqCB cb, object cbArgs)
        {
            DiscoverItems(to, null, null, cb, cbArgs);
        }

        public void DiscoverItems(Jid to, Jid from, IqCB cb, object cbArgs)
        {
            DiscoverItems(to, from, null, cb, cbArgs);
        }

        public void DiscoverItems(Jid to, string node)
        {
            DiscoverItems(to, null, node, null, null);
        }

        public void DiscoverItems(Jid to, Jid from, string node)
        {
            DiscoverItems(to, from, node, null, null);
        }

        public void DiscoverItems(Jid to, string node, IqCB cb)
        {
            DiscoverItems(to, null, node, cb, null);
        }

        public void DiscoverItems(Jid to, Jid from, string node, IqCB cb)
        {
            DiscoverItems(to, from, node, cb, null);
        }

        public void DiscoverItems(Jid to, string node, IqCB cb, object cbArgs)
        {
            DiscoverItems(to, null, node, cb, cbArgs);
        }

        public void DiscoverItems(Jid to, Jid from, string node, IqCB cb, object cbArgs)
        {
            DiscoItemsIq discoIq = new DiscoItemsIq(IqType.get);
            discoIq.To = to;
            
            if (from != null)
                discoIq.From = from;

            if (node != null && node.Length > 0)
                discoIq.Query.Node = node;

            xmppConnection.IqGrabber.SendIq(discoIq, cb, cbArgs);
        }
        #endregion
                        
    }
}