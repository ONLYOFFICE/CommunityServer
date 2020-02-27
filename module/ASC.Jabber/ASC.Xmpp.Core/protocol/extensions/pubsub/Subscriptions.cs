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

namespace ASC.Xmpp.Core.protocol.extensions.pubsub
{
    public class Subscriptions : Element
    {
        /*
            Example 14. Entity requests all current subscriptions

            <iq type='get'
                from='francisco@denmark.lit/barracks'
                to='pubsub.shakespeare.lit'
                id='subscriptions1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                <subscriptions/>
              </pubsub>
            </iq>
                       

            Example 15. Service returns all current subscriptions

            <iq type='result'
                from='pubsub.shakespeare.lit'
                to='francisco@denmark.lit'
                id='subscriptions1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                <subscriptions>
                  <subscription node='node1' jid='francisco@denmark.lit' subscription='subscribed'/>
                  <subscription node='node2' jid='francisco@denmark.lit' subscription='subscribed'/>
                  <subscription node='node5' jid='francisco@denmark.lit' subscription='unconfigured'/>
                  <subscription node='node6' jid='francisco@denmark.lit' subscription='pending'/>
                </subscriptions>
              </pubsub>
            </iq>
    
        */

        public Subscriptions()
        {
            TagName = "subscriptions";
            Namespace = Uri.PUBSUB;
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public Subscription AddSubscription()
        {
            var sub = new Subscription();
            AddChild(sub);
            return sub;
        }

        /// <summary>
        /// </summary>
        /// <param name="item"> </param>
        /// <returns> </returns>
        public Subscription AddSubscription(Subscription sub)
        {
            AddChild(sub);
            return sub;
        }

        public Subscription[] GetSubscriptions()
        {
            ElementList nl = SelectElements(typeof (Subscription));
            var items = new Subscription[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Subscription) e;
                i++;
            }
            return items;
        }
    }
}