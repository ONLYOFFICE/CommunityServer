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
// // <copyright company="Ascensio System Limited" file="Amp.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.amp
{
    /*
        <xs:element name='amp'>
            <xs:complexType>
              <xs:sequence>
                <xs:element ref='rule' minOccurs='1' maxOccurs='unbounded'/>
              </xs:sequence>
              <xs:attribute name='from' usage='optional' type='xs:string'/>
              <xs:attribute name='per-hop' use='optional' type='xs:bool' default='false'/>
              <xs:attribute name='status' usage='optional' type='xs:NCName'/>
              <xs:attribute name='to' usage='optional' type='xs:string'/>
            </xs:complexType>
        </xs:element>
    */

    public class Amp : DirectionalElement
    {
        public Amp()
        {
            TagName = "amp";
            Namespace = Uri.AMP;
        }

        /// <summary>
        ///   The 'status' attribute specifies the reason for the amp element. When specifying semantics to be applied (client to server), this attribute MUST NOT be present. When replying to a sending entity regarding a met condition, this attribute MUST be present and SHOULD be the value of the 'action' attribute for the triggered rule. (Note: Individual action definitions MAY provide their own requirements.)
        /// </summary>
        public Action Status
        {
            get { return (Action) GetAttributeEnum("status", typeof (Action)); }
            set
            {
                if (value == Action.Unknown)
                    RemoveAttribute("status");
                else
                    SetAttribute("status", value.ToString());
            }
        }

        /// <summary>
        ///   The 'per-hop' attribute flags the contained ruleset for processing at each server in the route between the original sender and original intended recipient. This attribute MAY be present, and MUST be either "true" or "false". If not present, the default is "false".
        /// </summary>
        public bool PerHop
        {
            get { return GetAttributeBool("per-hop"); }
            set { SetAttribute("per-hop", value); }
        }

        public void AddRule(Rule rule)
        {
            AddChild(rule);
        }

        public Rule AddRule()
        {
            var rule = new Rule();
            AddChild(rule);

            return rule;
        }

        /// <summary>
        ///   Gets a list of all form fields
        /// </summary>
        /// <returns> </returns>
        public Rule[] GetRules()
        {
            ElementList nl = SelectElements(typeof (Rule));
            var items = new Rule[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Rule) e;
                i++;
            }
            return items;
        }
    }
}