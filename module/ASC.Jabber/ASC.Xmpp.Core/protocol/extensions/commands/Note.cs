/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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