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


namespace ASC.Xmpp.Core.protocol.extensions.pubsub
{
    // Publish and retract looks exactly the same, so inherit from publish here
    public class Retract : Publish
    {
        /*
            A service SHOULD allow a publisher to delete an item once it has been published to a node that 
            supports persistent items.
            To delete an item, the publisher sends a retract request as shown in the following examples. 
            The <retract/> element MUST possess a 'node' attribute and SHOULD contain one <item/> element
            (but MAY contain more than one <item/> element for Batch Processing of item retractions); 
            the <item/> element MUST be empty and MUST possess an 'id' attribute.
            
            <iq type="set"
                from="pgm@jabber.org"
                to="pubsub.jabber.org"
                id="deleteitem1">
              <pubsub xmlns="http://jabber.org/protocol/pubsub">
                <retract node="generic/pgm-mp3-player">
                  <item id="current"/>
                </retract>
              </pubsub>
            </iq>
        */

        public Retract()
        {
            TagName = "retract";
        }

        public Retract(string node) : this()
        {
            Node = node;
        }

        public Retract(string node, string id) : this(node)
        {
            AddItem(new Item(id));
        }
    }
}