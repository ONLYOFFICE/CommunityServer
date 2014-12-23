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

using System;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;

namespace ASC.Xmpp.Server
{
	public static class XmppStanzaError
	{
		public static Stanza ToBadRequest(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.BadRequest);
		}

		public static Stanza ToConflict(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.Conflict);
		}

		public static Stanza ToNotAuthorized(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.Unauthorized);
		}

		public static Stanza ToNotFound(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.NotFound);
		}

		public static Stanza ToRecipientUnavailable(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorType.wait, ErrorCondition.RecipientUnavailable);
		}

		public static Stanza ToServiceUnavailable(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.ServiceUnavailable);
		}

		public static Stanza ToNotAcceptable(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.NotAcceptable);
		}

		public static Stanza ToItemNotFound(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorType.wait, ErrorCondition.ItemNotFound);
		}

		public static Stanza ToForbidden(Stanza stanza)
		{
			return ToErrorStanzaWithError(stanza, ErrorCode.Forbidden);
		}


		public static T ToBadRequest<T>(T stanza) where T : Stanza
		{
			return (T)ToBadRequest((Stanza)stanza);
		}

		public static T ToConflict<T>(T stanza) where T : Stanza
		{
			return (T)ToConflict((Stanza)stanza);
		}

		public static T ToNotAuthorized<T>(T stanza) where T : Stanza
		{
			return (T)ToNotAuthorized((Stanza)stanza);
		}

		public static T ToNotFound<T>(T stanza) where T : Stanza
		{
			return (T)ToNotFound((Stanza)stanza);
		}

		public static T ToRecipientUnavailable<T>(T stanza) where T : Stanza
		{
			return (T)ToRecipientUnavailable((Stanza)stanza);
		}

		public static T ToServiceUnavailable<T>(T stanza) where T : Stanza
		{
			return (T)ToServiceUnavailable((Stanza)stanza);
		}

		public static T ToNotAcceptable<T>(T stanza) where T : Stanza
		{
			return (T)ToNotAcceptable((Stanza)stanza);
		}

		public static T ToItemNotFound<T>(T stanza) where T : Stanza
		{
			return (T)ToItemNotFound((Stanza)stanza);
		}

		public static T ToForbidden<T>(T stanza) where T : Stanza
		{
			return (T)ToForbidden((Stanza)stanza);
		}


		public static Stanza ToErrorStanza(Stanza stanza, Error error)
		{
			if (stanza == null) throw new ArgumentNullException("stanza");
			if (error == null) throw new ArgumentNullException("error");

			stanza.SetAttribute("type", "error");
			stanza.ReplaceChild(error);
			if (!stanza.Switched) stanza.SwitchDirection();
			return stanza;
		}

		private static Stanza ToErrorStanzaWithError(Stanza stanza, ErrorCode code)
		{
			return ToErrorStanza(stanza, new Error(code));
		}

		private static Stanza ToErrorStanzaWithError(Stanza stanza, ErrorType type, ErrorCondition condition)
		{
			return ToErrorStanza(stanza, new Error(type, condition));
		}
	}
}
