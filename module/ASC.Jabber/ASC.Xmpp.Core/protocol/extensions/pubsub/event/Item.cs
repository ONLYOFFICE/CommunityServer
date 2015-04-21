/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Xmpp.Core.protocol.extensions.pubsub.@event
{
    /*
        <message from='pubsub.shakespeare.lit' to='francisco@denmark.lit' id='foo'>
          <event xmlns='http://jabber.org/protocol/pubsub#event'>
            <items node='blogs/princely_musings'>
              <item id='ae890ac52d0df67ed7cfdf51b644e901'>
                <entry xmlns='http://www.w3.org/2005/Atom'>
                  <title>Soliloquy</title>
                  <summary>
                        To be, or not to be: that is the question:
                        Whether 'tis nobler in the mind to suffer
                        The slings and arrows of outrageous fortune,
                        Or to take arms against a sea of troubles,
                        And by opposing end them?
                  </summary>
                  <link rel='alternate' type='text/html' 
                        href='http://denmark.lit/2003/12/13/atom03'/>
                  <id>tag:denmark.lit,2003:entry-32397</id>
                  <published>2003-12-13T18:30:02Z</published>
                  <updated>2003-12-13T18:30:02Z</updated>
                </entry>
              </item>
            </items>
          </event>
        </message>
     
        <xs:element name='item'>
            <xs:complexType>
              <xs:choice minOccurs='0'>
                <xs:element name='retract' type='empty'/>
                <xs:any namespace='##other'/>
              </xs:choice>
              <xs:attribute name='id' type='xs:string' use='optional'/>
            </xs:complexType>
        </xs:element>
    */

    // This class is the same as the Item class in the main pubsub namespace,
    // so inherit it and overwrite some properties and functions

    public class Item : pubsub.Item
    {
        #region << Constructors >>

        public Item()
        {
            Namespace = Uri.PUBSUB_EVENT;
        }

        public Item(string id) : this()
        {
            Id = id;
        }

        #endregion

        private const string RETRACT = "retract";

        public bool Retract
        {
            get { return HasTag(RETRACT); }
            set
            {
                if (value)
                    SetTag(RETRACT);
                else
                    RemoveTag(RETRACT);
            }
        }
    }
}