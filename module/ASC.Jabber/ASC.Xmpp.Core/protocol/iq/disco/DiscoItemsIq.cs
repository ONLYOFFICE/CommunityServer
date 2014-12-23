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

using ASC.Xmpp.Core.protocol.client;

namespace ASC.Xmpp.Core.protocol.iq.disco
{
    /*
	Example 10. Requesting all items

	<iq type='get'
	from='romeo@montague.net/orchard'
	to='shakespeare.lit'
	id='items1'>
	<query xmlns='http://jabber.org/protocol/disco#items'/>
	</iq>
	
	
	Example 11. Result-set for all items

	<iq type='result'
		from='shakespeare.lit'
		to='romeo@montague.net/orchard'
		id='items1'>
	<query xmlns='http://jabber.org/protocol/disco#items'>
		<item jid='people.shakespeare.lit'
			name='Directory of Characters'/>
		<item jid='plays.shakespeare.lit'
			name='Play-Specific Chatrooms'/>
		<item jid='mim.shakespeare.lit'
			name='Gateway to Marlowe IM'/>
		<item jid='words.shakespeare.lit'
			name='Shakespearean Lexicon'/>
		<item jid='globe.shakespeare.lit'
			name='Calendar of Performances'/>
		<item jid='headlines.shakespeare.lit'
			name='Latest Shakespearean News'/>
		<item jid='catalog.shakespeare.lit'
			name='Buy Shakespeare Stuff!'/>
		<item jid='en2fr.shakespeare.lit'
			name='French Translation Service'/>
	</query>
	</iq>
      
     */

    /// <summary>
    ///   Discovering the Items Associated with a Jabber Entity
    /// </summary>
    public class DiscoItemsIq : IQ
    {
        private readonly DiscoItems m_DiscoItems = new DiscoItems();

        public DiscoItemsIq()
        {
            base.Query = m_DiscoItems;
            GenerateId();
        }

        public DiscoItemsIq(IqType type) : this()
        {
            Type = type;
        }

        public new DiscoItems Query
        {
            get { return m_DiscoItems; }
        }
    }
}