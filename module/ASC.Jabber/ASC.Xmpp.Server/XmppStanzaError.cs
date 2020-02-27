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
