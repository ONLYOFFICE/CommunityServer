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