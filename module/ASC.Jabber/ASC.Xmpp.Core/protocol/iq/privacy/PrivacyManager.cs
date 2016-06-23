/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

using ASC.Xmpp;
using ASC.Xmpp.protocol.client;
using ASC.Xmpp.protocol.iq.privacy;

namespace ASC.Xmpp.protocol.iq.privacy
{
    using client;

    /// <summary>
    /// Helper class for managing server side privacy lists (blocking communication)
    /// </summary>
    public class PrivacyManager
    {
        private XmppClientConnection	m_connection	= null;

        public PrivacyManager(XmppClientConnection con)
        {
            m_connection = con;
        }


        /// <summary>
        /// Retrieving all Privacy Lists
        /// </summary>
        public void GetLists()
        {
            GetLists(null, null);
        }

        /// <summary>
        /// Retrieving all Privacy Lists
        /// </summary>
        /// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void GetLists(IqCB cb, object cbArg)
        {
            /*
                Example: Client requests names of privacy lists from server:

                <iq from='romeo@example.net/orchard' type='get' id='getlist1'>
                  <query xmlns='jabber:iq:privacy'/>
                </iq>

                Example: Server sends names of privacy lists to client, preceded by active list and default list:

                <iq type='result' id='getlist1' to='romeo@example.net/orchard'>
                  <query xmlns='jabber:iq:privacy'>
                    <active name='private'/>
                    <default name='public'/>
                    <list name='public'/>
                    <list name='private'/>
                    <list name='special'/>
                  </query>
                </iq>

            */

            PrivacyIq pIq = new PrivacyIq();

            pIq.Type = IqType.get;

            SendStanza(pIq, cb, cbArg);
        }

        /// <summary>
        /// Requests a privacy list from the server by its name
        /// </summary>
        /// <param name="name">name of the privacy list to retrieve</param>
        public void GetList(string name)
        {
            GetList(name, null, null);
        }

        /// <summary>
        /// Requests a privacy list from the server by its name
        /// </summary>
        /// <param name="name">name of the privacy list to retrieve</param>
        /// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void GetList(string name, IqCB cb, object cbArg)
        {
            /*
                Example: Client requests a privacy list from server:

                <iq from='romeo@example.net/orchard' type='get' id='getlist2'>
                  <query xmlns='jabber:iq:privacy'>
                    <list name='public'/>
                  </query>
                </iq>

                Example: Server sends a privacy list to client:

                <iq type='result' id='getlist2' to='romeo@example.net/orchard'>
                  <query xmlns='jabber:iq:privacy'>
                    <list name='public'>
                      <item type='jid'
                            value='tybalt@example.com'
                            action='deny'
                            order='1'/>
                      <item action='allow' order='2'/>
                    </list>
                  </query>
                </iq>

            */

            PrivacyIq pIq = new PrivacyIq();

            pIq.Type = IqType.get;
            pIq.Query.AddList(new List(name));

            SendStanza(pIq, cb, cbArg);            
        }

        /// <summary>
        /// Remove a privacy list
        /// </summary>
        /// <param name="name">name of the privacy list to remove</param>
        public void RemoveList(string name)
        {
            RemoveList(name, null, null);
        }

        /// <summary>
        /// Remove a privacy list
        /// </summary>
        /// <param name="name">name of the privacy list to remove</param>
        /// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void RemoveList(string name, IqCB cb, object cbArg)
        {
            PrivacyIq pIq = new PrivacyIq();

            pIq.Type = IqType.set;
            pIq.Query.AddList(new List(name));

            SendStanza(pIq, cb, cbArg);            
        }

        /// <summary>
        /// Decline the use of any active list
        /// </summary>
        public void DeclineActiveList()
        {
            DeclineActiveList(null, null);
        }

        /// <summary>
        /// Decline the use of any active list
        /// </summary>
        /// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void DeclineActiveList(IqCB cb, object cbArg)
        {
            /*
                In order to decline the use of any active list, the connected resource MUST send an empty <active/> element 
                with no 'name' attribute.

                Example: Client declines the use of active lists:

                <iq from='romeo@example.net/orchard' type='set' id='active3'>
                  <query xmlns='jabber:iq:privacy'>
                    <active/>
                  </query>
                </iq>

                Example: Server acknowledges success of declining any active list:

                <iq type='result' id='active3' to='romeo@example.net/orchard'/>            
            */

            PrivacyIq pIq = new PrivacyIq();

            pIq.Type = IqType.set;
            pIq.Query.Active = new Active();

            SendStanza(pIq, cb, cbArg);
        }

        /// <summary>
        /// Change the active list
        /// </summary>
        /// <param name="name"></param>
        public void ChangeActiveList(string name)
        {
            ChangeActiveList(name, null, null);
        }

        /// <summary>
        /// Change the active list
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void ChangeActiveList(string name, IqCB cb, object cbArg)
        {
            /*
                Example: Client requests change of active list:

                <iq from='romeo@example.net/orchard' type='set' id='active1'>
                  <query xmlns='jabber:iq:privacy'>
                    <active name='special'/>
                  </query>
                </iq>

                The server MUST activate and apply the requested list before sending the result back to the client.

                Example: Server acknowledges success of active list change:

                <iq type='result' id='active1' to='romeo@example.net/orchard'/>

                If the user attempts to set an active list but a list by that name does not exist, the server MUST return an <item-not-found/> stanza error to the user:

                Example: Client attempts to set a non-existent list as active:

                <iq to='romeo@example.net/orchard' type='error' id='active2'>
                  <query xmlns='jabber:iq:privacy'>
                    <active name='The Empty Set'/>
                  </query>
                  <error type='cancel'>
                    <item-not-found
                        xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
                  </error>
                </iq>                
             
            */

            PrivacyIq pIq = new PrivacyIq();

            pIq.Type = IqType.set;
            pIq.Query.Active = new Active(name);

            SendStanza(pIq, cb, cbArg);
        }

        /// <summary>
        /// Change the default list
        /// </summary>
        /// <param name="name">name of the new default list</param>
        public void ChangeDefaultList(string name)
        {
            ChangeDefaultList(name, null, null);
        }

        /// <summary>
        /// Change the default list
        /// </summary>
        /// <param name="name">name of the new default list</param>
        /// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void ChangeDefaultList(string name, IqCB cb, object cbArg)
        {
            PrivacyIq pIq = new PrivacyIq();

            pIq.Type = IqType.set;
            pIq.Query.Default = new Default(name);

            SendStanza(pIq, cb, cbArg);
        }

        /// <summary>
        /// Decline the use of the default list
        /// </summary>
        public void DeclineDefaultList()
        {
            DeclineDefaultList(null, null);
        }

        /// <summary>
        /// Decline the use of the default list
        /// </summary>
        /// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void DeclineDefaultList(IqCB cb, object cbArg)
        {
            PrivacyIq pIq = new PrivacyIq();

            pIq.Type = IqType.set;
            pIq.Query.Default = new Default();

            SendStanza(pIq, cb, cbArg);
        }

      

        /// <summary>
        /// Update the list with the given name and rules.
        /// </summary>
        /// <remarks>
        /// Specify the desired changes to the list by including all elements/rules in the list 
        /// (not the "delta")
        /// </remarks>
        /// <param name="name">name of the list</param>
        /// <param name="rules">rules of this list</param>
        public void UpdateList(string name, Item[] rules)
        {
            UpdateList(name, rules, null, null);
        }

        /// <summary>
        /// Update the list with the given name and rules.
        /// </summary>
        /// <remarks>
        /// Specify the desired changes to the list by including all elements/rules in the list 
        /// (not the "delta")
        /// </remarks>
        /// <param name="name">name of this list</param>
        /// <param name="rules">rules of this list</param>
        /// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void UpdateList(string name, Item[] rules, IqCB cb, object cbArg)
        {
            PrivacyIq pIq = new PrivacyIq();
            pIq.Type = IqType.set;

            // create a new list with the given name
            List list = new List(name);
            list.AddItems(rules);
            // add the list to the query
            pIq.Query.AddList(list);

            SendStanza(pIq, cb, cbArg);
        }

        /// <summary>
        /// Add a new list with the given name and rules
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rules"></param>
        public void AddList(string name, Item[] rules)
        {
            AddList(name, rules, null, null);
        }

        /// <summary>
        /// Add a new list with the given name and rules.
        /// </summary>        
        /// <param name="name"></param>
        /// <param name="rules"></param>
        ///// <param name="cb">Callback for the server result</param>
        /// <param name="cbArg">Callback arguments for the result when needed</param>
        public void AddList(string name, Item[] rules, IqCB cb, object cbArg)
        {
            UpdateList(name, rules, cb, cbArg);
        }

        /// <summary>
        /// Sends a PrivacyIq over the active connection
        /// </summary>
        /// <param name="pIq"></param>
        /// <param name="cb"></param>
        /// <param name="cbArg"></param>
        private void SendStanza(PrivacyIq pIq, IqCB cb, object cbArg)
        {
            if (cb == null)
                m_connection.Send(pIq);
            else
                m_connection.IqGrabber.SendIq(pIq, cb, cbArg);
        }
        

    }
}
