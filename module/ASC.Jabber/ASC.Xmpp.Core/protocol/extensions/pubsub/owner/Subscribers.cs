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
// // <copyright company="Ascensio System Limited" file="Subscribers.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.pubsub.owner
{
    /*
        <iq type='result'
            from='pubsub.shakespeare.lit'
            to='hamlet@denmark.lit/elsinore'
            id='subman1'>
          <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
            <subscribers node='blogs/princely_musings'>
              <subscriber jid='hamlet@denmark.lit' subscription='subscribed'/>
              <subscriber jid='polonius@denmark.lit' subscription='unconfigured'/>
            </subscribers>
          </pubsub>
        </iq>
        
        <xs:element name='subscribers'>
            <xs:complexType>
              <xs:sequence>
                <xs:element ref='subscriber' minOccurs='0' maxOccurs='unbounded'/>
              </xs:sequence>
              <xs:attribute name='node' type='xs:string' use='required'/>
            </xs:complexType>
        </xs:element>
    */

    public class Subscribers : Element
    {
        #region << Constructors >>

        public Subscribers()
        {
            TagName = "subscribers";
            Namespace = Uri.PUBSUB_OWNER;
        }

        public Subscribers(string node) : this()
        {
            Node = node;
        }

        #endregion

        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

        /// <summary>
        ///   Add a Subscriber
        /// </summary>
        /// <returns> </returns>
        public Subscriber AddSubscriber()
        {
            var subscriber = new Subscriber();
            AddChild(subscriber);
            return subscriber;
        }

        /// <summary>
        ///   Add a Subscriber
        /// </summary>
        /// <param name="subscriber"> the Subscriber to add </param>
        /// <returns> </returns>
        public Subscriber AddSubscriber(Subscriber subscriber)
        {
            AddChild(subscriber);
            return subscriber;
        }

        public void AddSubscribers(Subscriber[] subscribers)
        {
            foreach (Subscriber subscriber in subscribers)
            {
                AddSubscriber(subscriber);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public Subscriber[] GetSubscribers()
        {
            ElementList nl = SelectElements(typeof (Subscriber));
            var subscribers = new Subscriber[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                subscribers[i] = (Subscriber) e;
                i++;
            }
            return subscribers;
        }
    }
}