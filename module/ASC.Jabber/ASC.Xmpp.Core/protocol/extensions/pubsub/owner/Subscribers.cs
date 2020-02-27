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