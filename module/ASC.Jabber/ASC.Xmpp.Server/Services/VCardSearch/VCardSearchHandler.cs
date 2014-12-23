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
using ASC.Xmpp.Core.protocol.iq.search;
using ASC.Xmpp.Core.protocol.iq.vcard;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Server.Services.VCardSearch
{
	[XmppHandler(typeof(Search))]
	class VCardSearchHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (!iq.HasTo) iq.To = iq.From;
			if (iq.Type == IqType.get) return GetVCardSearch(stream, iq, context);
			else if (iq.Type == IqType.set) return SetVCardSearch(stream, iq, context);
			else return XmppStanzaError.ToBadRequest(iq);
		}

		private IQ GetVCardSearch(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.To = iq.From;
			answer.From = iq.To;

			var search = new Search();
			search.Nickname = string.Empty;
			search.Firstname = string.Empty;
			search.Lastname = string.Empty;

			answer.Query = search;
			return answer;
		}

		private IQ SetVCardSearch(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.To = iq.From;
			answer.From = iq.To;

			var search = (Search)iq.Query;

			var pattern = new Vcard();
			pattern.Nickname = search.Nickname;
			pattern.Name = new Name(search.Lastname, search.Firstname, null);
			//pattern.AddEmailAddress(new Email() { UserId = search.Email });

			search = new Search();
			foreach (var vcard in context.StorageManager.VCardStorage.Search(pattern))
			{
				var item = new SearchItem();
				item.Jid = vcard.JabberId;
				item.Nickname = vcard.Nickname;
				if (vcard.Name != null)
				{
					item.Firstname = vcard.Name.Given;
					item.Lastname = vcard.Name.Family;
				}
				var email = vcard.GetPreferedEmailAddress();
				if (email != null)
				{
					item.Email = email.UserId;
				}
				search.AddChild(item);
			}

			answer.Query = search;
			return answer;
		}
	}
}