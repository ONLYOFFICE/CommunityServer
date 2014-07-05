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
// // <copyright company="Ascensio System Limited" file="Agent.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.iq.agent
{
    //	<agent jid="conference.myjabber.net"><name>Public Conferencing</name><service>public</service></agent>
    //	<agent jid="aim.myjabber.net"><name>AIM Transport</name><service>aim</service><transport>Enter ID</transport><register/></agent>
    //	<agent jid="yahoo.myjabber.net"><name>Yahoo! Transport</name><service>yahoo</service><transport>Enter ID</transport><register/></agent>
    //	<agent jid="icq.myjabber.net"><name>ICQ Transport</name><service>icq</service><transport>Enter ID</transport><register/></agent>
    //	<agent jid="msn.myjabber.net"><name>MSN Transport</name><service>msn</service><transport>Enter ID</transport><register/></agent>

    /// <summary>
    ///   Zusammenfassung f�r Agent.
    /// </summary>
    public class Agent : Element
    {
        public Agent()
        {
            TagName = "agent";
            Namespace = Uri.IQ_AGENTS;
        }

        public Jid Jid
        {
            get { return new Jid(GetAttribute("jid")); }
            set { SetAttribute("jid", value.ToString()); }
        }

        public string Name
        {
            get { return GetTag("name"); }
            set { SetTag("name", value); }
        }

        public string Service
        {
            get { return GetTag("service"); }
            set { SetTag("service", value); }
        }

        public string Description
        {
            get { return GetTag("description"); }
            set { SetTag("description", value); }
        }

        /// <summary>
        ///   Can we register this agent/transport
        /// </summary>
        public bool CanRegister
        {
            get { return HasTag("register"); }
            set
            {
                if (value)
                    SetTag("register");
                else
                    RemoveTag("register");
            }
        }

        /// <summary>
        ///   Can we search thru this agent/transport
        /// </summary>
        public bool CanSearch
        {
            get { return HasTag("search"); }
            set
            {
                if (value)
                    SetTag("search");
                else
                    RemoveTag("search");
            }
        }

        /// <summary>
        ///   Is this agent a transport?
        /// </summary>
        public bool IsTransport
        {
            get { return HasTag("transport"); }
            set
            {
                if (value)
                    SetTag("transport");
                else
                    RemoveTag("transport");
            }
        }

        /// <summary>
        ///   Is this agent for groupchat
        /// </summary>
        public bool IsGroupchat
        {
            get { return HasTag("groupchat"); }
            set
            {
                if (value)
                    SetTag("groupchat");
                else
                    RemoveTag("groupchat");
            }
        }
    }
}