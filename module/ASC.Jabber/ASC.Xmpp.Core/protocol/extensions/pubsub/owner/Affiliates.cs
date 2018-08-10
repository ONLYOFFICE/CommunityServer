/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
            id='ent1'>
          <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
            <affiliates node='blogs/princely_musings'>
              <affiliate jid='hamlet@denmark.lit' affiliation='owner'/>
              <affiliate jid='polonius@denmark.lit' affiliation='outcast'/>
            </affiliates>
          </pubsub>
        </iq>
     
     
        <xs:element name='affiliates'>
            <xs:complexType>
              <xs:sequence>
                <xs:element ref='affiliate' minOccurs='0' maxOccurs='unbounded'/>
              </xs:sequence>
              <xs:attribute name='node' type='xs:string' use='required'/>
            </xs:complexType>
          </xs:element>
    */

    public class Affiliates : Element
    {
        #region << Constructors >>

        public Affiliates()
        {
            TagName = "affiliates";
            Namespace = Uri.PUBSUB_OWNER;
        }

        public Affiliates(string node) : this()
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
        /// </summary>
        /// <returns> </returns>
        public Affiliate AddAffiliate()
        {
            var affiliate = new Affiliate();
            AddChild(affiliate);
            return affiliate;
        }

        /// <summary>
        /// </summary>
        /// <param name="affiliate"> </param>
        /// <returns> </returns>
        public Affiliate AddAffiliate(Affiliate affiliate)
        {
            AddChild(affiliate);
            return affiliate;
        }

        /// <summary>
        /// </summary>
        /// <param name="affiliates"> </param>
        public void AddAffiliates(Affiliate[] affiliates)
        {
            foreach (Affiliate affiliate in affiliates)
            {
                AddAffiliate(affiliate);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public Affiliate[] GetAffiliates()
        {
            ElementList nl = SelectElements(typeof (Affiliate));
            var affiliates = new Affiliate[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                affiliates[i] = (Affiliate) e;
                i++;
            }
            return affiliates;
        }
    }
}