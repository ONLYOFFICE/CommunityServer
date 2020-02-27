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
    public class Subscription : Element
    {
        #region << Constructors >>

        public Subscription()
        {
            TagName = "subscription";
            Namespace = Uri.PUBSUB;
        }

        public Subscription(Jid jid) : this()
        {
            Jid = jid;
        }

        public Subscription(Jid jid, SubscriptionState subType) : this(jid)
        {
            SubscriptionState = subType;
        }

        public Subscription(Jid jid, string node) : this()
        {
            Node = node;
        }

        public Subscription(Jid jid, string node, SubscriptionState subType) : this(jid, node)
        {
            SubscriptionState = subType;
        }

        public Subscription(Jid jid, string node, string subId) : this(jid, node)
        {
            SubId = subId;
        }

        public Subscription(Jid jid, string node, string subId, SubscriptionState subType) : this(jid, node, subId)
        {
            SubscriptionState = subType;
        }

        #endregion

        /*
            Example 23. Service replies with success

            <iq type='result'
                from='pubsub.shakespeare.lit'
                to='francisco@denmark.lit/barracks'
                id='sub1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                <subscription 
                    node='blogs/princely_musings'
                    jid='francisco@denmark.lit'
                    subid='ba49252aaa4f5d320c24d3766f0bdcade78c78d3'
                    subscription='subscribed'/>
              </pubsub>
            </iq>
         
            
            Example 36. Service replies with success and indicates that subscription configuration is required

            <iq type='result'
                from='pubsub.shakespeare.lit'
                to='francisco@denmark.lit/barracks'
                id='sub1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                <subscription 
                    node='blogs/princely_musings'
                    jid='francisco@denmark.lit'
                    subscription='unconfigured'>
                  <subscribe-options>
                    <required/>
                  </subscribe-options>
                </subscription>
              </pubsub>
            </iq>
                
    
            <xs:element name='subscription'>
                <xs:complexType>
                  <xs:sequence>
                    <xs:element ref='subscribe-options' minOccurs='0'/>
                  </xs:sequence>
                  <xs:attribute name='jid' type='xs:string' use='required'/>
                  <xs:attribute name='node' type='xs:string' use='optional'/>
                  <xs:attribute name='subid' type='xs:string' use='optional'/>
                  <xs:attribute name='subscription' use='optional'>
                    <xs:simpleType>
                      <xs:restriction base='xs:NCName'>
                        <xs:enumeration value='pending'/>
                        <xs:enumeration value='subscribed'/>
                        <xs:enumeration value='unconfigured'/>
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:attribute>
                </xs:complexType>
            </xs:element>
        */

        /// <summary>
        ///   Node (optional)
        /// </summary>
        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

        public Jid Jid
        {
            get
            {
                if (HasAttribute("jid"))
                    return new Jid(GetAttribute("jid"));
                else
                    return null;
            }
            set
            {
                if (value != null)
                    SetAttribute("jid", value.ToString());
                else
                    RemoveAttribute("jid");
            }
        }

        /// <summary>
        ///   Subscription ID (optional)
        /// </summary>
        public string SubId
        {
            get { return GetAttribute("subid"); }
            set
            {
                if (value != null)
                    SetAttribute("subid", value);
                else
                    RemoveAttribute("subid");
            }
        }

        //public Affiliation Affiliation
        //{
        //    get 
        //    {
        //        return (Affiliation)GetAttributeEnum("affiliation", typeof(Affiliation)); 
        //    }
        //    set 
        //    {
        //        SetAttribute("affiliation", value.ToString()); 
        //    }
        //}

        public SubscriptionState SubscriptionState
        {
            get { return (SubscriptionState) GetAttributeEnum("subscription", typeof (SubscriptionState)); }
            set { SetAttribute("subscription", value.ToString()); }
        }

        public SubscribeOptions SubscribeOptions
        {
            get { return SelectSingleElement(typeof (SubscribeOptions)) as SubscribeOptions; }
            set
            {
                if (HasTag(typeof (SubscribeOptions)))
                    RemoveTag(typeof (SubscribeOptions));

                if (value != null)
                    AddChild(value);
            }
        }
    }
}