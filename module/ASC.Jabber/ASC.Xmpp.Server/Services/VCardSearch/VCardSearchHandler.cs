/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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