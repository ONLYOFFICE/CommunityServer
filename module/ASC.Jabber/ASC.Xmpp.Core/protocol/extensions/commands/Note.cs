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

namespace ASC.Xmpp.Core.protocol.extensions.commands
{
    /*
        <note type='info'>Service 'httpd' has been configured.</note>
        
        <xs:element name='note'>
            <xs:complexType>
              <xs:simpleContent>
                <xs:extension base='xs:string'>
                  <xs:attribute name='type' use='required'>
                    <xs:simpleType>
                      <xs:restriction base='xs:NCName'>
                        <xs:enumeration value='error'/>
                        <xs:enumeration value='info'/>
                        <xs:enumeration value='warn'/>
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:attribute>
                </xs:extension>
              </xs:simpleContent>
            </xs:complexType>
        </xs:element>
    */

    public class Note : Element
    {
        /// <summary>
        ///   Default constructor
        /// </summary>
        public Note()
        {
            TagName = "note";
            Namespace = Uri.COMMANDS;
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        public Note(NoteType type) : this()
        {
            Type = type;
        }

        /// <summary>
        /// </summary>
        /// <param name="text"> </param>
        /// <param name="type"> </param>
        public Note(string text, NoteType type) : this(type)
        {
            Value = text;
        }

        public NoteType Type
        {
            get { return (NoteType) GetAttributeEnum("type", typeof (NoteType)); }
            set { SetAttribute("type", value.ToString()); }
        }
    }
}