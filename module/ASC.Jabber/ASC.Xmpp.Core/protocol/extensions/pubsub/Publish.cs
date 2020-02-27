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
    /*
     Example 9. Entity publishes an item with an ItemID of the Payload Type UserTune

        <iq type="set"
            from="pgm@jabber.org"
            to="pubsub.jabber.org"
            id="publish1">
          <pubsub xmlns="http://jabber.org/protocol/pubsub">
            <publish node="generic/pgm-mp3-player">
              <item id="current">
                <tune xmlns="http://jabber.org/protocol/tune">
                  <artist>Ralph Vaughan Williams</artist>
                  <title>Concerto in F for Bass Tuba</title>
                  <source>Golden Brass: The Collector's Edition</source>
                </tune>
              </item>
            </publish>
          </pubsub>
        </iq>
     
    */

    public class Publish : Element
    {
        #region << Constructors >>

        public Publish()
        {
            TagName = "publish";
            Namespace = Uri.PUBSUB;
        }

        /// <summary>
        ///   Its recommended to use this constructor because a node is required
        /// </summary>
        /// <param name="node"> Node to publish </param>
        public Publish(string node) : this()
        {
            Node = node;
        }

        public Publish(string node, Item item) : this(node)
        {
            AddItem(item);
        }

        #endregion

        /// <summary>
        ///   The node to publish to. This Property is required
        /// </summary>
        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

        /// <summary>
        ///   Add a payload Item
        /// </summary>
        /// <returns> returns the added Item </returns>
        public Item AddItem()
        {
            var item = new Item();
            AddChild(item);
            return item;
        }

        /// <summary>
        /// </summary>
        /// <param name="item"> </param>
        /// <returns> returns the added item </returns>
        public Item AddItem(Item item)
        {
            AddChild(item);
            return item;
        }

        /// <summary>
        ///   This will return all payload items. Multiple items are possible, but doe the most implementaions one item should be enough
        /// </summary>
        /// <returns> returns an Array of Items </returns>
        public Item[] GetItems()
        {
            ElementList nl = SelectElements(typeof (Item));
            var items = new Item[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Item) e;
                i++;
            }
            return items;
        }
    }
}