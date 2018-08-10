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

namespace ASC.Xmpp.Core.protocol.extensions.commands
{
    /*
      <xs:element name='actions'>
        <xs:complexType>
          <xs:sequence>
            <xs:element name='prev' type='empty' minOccurs='0'/>
            <xs:element name='next' type='empty' minOccurs='0'/>
            <xs:element name='complete' type='empty' minOccurs='0'/>
          </xs:sequence>
          <xs:attribute name='execute' use='optional'>
            <xs:simpleType>
              <xs:restriction base='xs:NCName'>
                <xs:enumeration value='complete'/>
                <xs:enumeration value='next'/>
                <xs:enumeration value='prev'/>
              </xs:restriction>
            </xs:simpleType>
          </xs:attribute>
        </xs:complexType>
      </xs:element>
     
      <actions execute='complete'>
        <prev/>
        <complete/>
      </actions>
    */

    public class Actions : Element
    {
        public Actions()
        {
            TagName = "actions";
            Namespace = Uri.COMMANDS;
        }

        /// <summary>
        ///   Optional Execute Action, only complete, next and previous is allowed
        /// </summary>
        public Action Execute
        {
            get { return (Action) GetAttributeEnum("execute", typeof (Action)); }
            set
            {
                if (value == Action.NONE)
                    RemoveAttribute("execute");
                else
                    SetAttribute("execute", value.ToString());
            }
        }


        /// <summary>
        /// </summary>
        public bool Complete
        {
            get { return HasTag("complete"); }
            set
            {
                if (value)
                    SetTag("complete");
                else
                    RemoveTag("complete");
            }
        }

        public bool Next
        {
            get { return HasTag("next"); }
            set
            {
                if (value)
                    SetTag("next");
                else
                    RemoveTag("next");
            }
        }

        public bool Previous
        {
            get { return HasTag("prev"); }
            set
            {
                if (value)
                    SetTag("prev");
                else
                    RemoveTag("prev");
            }
        }

        /// <summary>
        ///   Actions, only complete, prev and next are allowed here and can be combined
        /// </summary>
        public Action Action
        {
            get
            {
                Action res = 0;

                if (Complete)
                    res |= Action.complete;
                if (Previous)
                    res |= Action.prev;
                if (Next)
                    res |= Action.next;

                if (res == 0)
                    return Action.NONE;
                else
                    return res;
            }
            set
            {
                if (value == Action.NONE)
                {
                    Complete = false;
                    Previous = false;
                    Next = false;
                }
                else
                {
                    Complete = ((value & Action.complete) == Action.complete);
                    Previous = ((value & Action.prev) == Action.prev);
                    Next = ((value & Action.next) == Action.next);
                }
            }
        }
    }
}