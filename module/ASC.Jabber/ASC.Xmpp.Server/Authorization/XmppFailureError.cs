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

using ASC.Xmpp.Core.protocol.sasl;

namespace ASC.Xmpp.Server.Authorization
{
	static class XmppFailureError
	{
		/// <summary>
		/// The receiving entity acknowledges an <abort/> element sent by the initiating entity; sent in reply to the <abort/> element.
		/// </summary>
		public static Failure Aborted;

		/// <summary>
		/// The data provided by the initiating entity could not be processed because the [BASE64] (Josefsson, S., “The Base16, Base32, and Base64 Data Encodings,” July 2003.) encoding is incorrect (e.g., because the encoding does not adhere to the definition in Section 3 of [BASE64] (Josefsson, S., “The Base16, Base32, and Base64 Data Encodings,” July 2003.)); sent in reply to a <response/> element or an <auth/> element with initial response data.
		/// </summary>
		public static Failure IncorrectEncoding;

		/// <summary>
		/// The authzid provided by the initiating entity is invalid, either because it is incorrectly formatted or because the initiating entity does not have permissions to authorize that ID; sent in reply to a <response/> element or an <auth/> element with initial response data.
		/// </summary>
		public static Failure InvalidAuthzid;

		/// <summary>
		/// The initiating entity did not provide a mechanism or requested a mechanism that is not supported by the receiving entity; sent in reply to an <auth/> element.
		/// </summary>
		public static Failure InvalidMechanism;

		/// <summary>
		/// The mechanism requested by the initiating entity is weaker than server policy permits for that initiating entity; sent in reply to a <response/> element or an <auth/> element with initial response data.
		/// </summary>
		public static Failure MechanismTooWeak;

		/// <summary>
		/// The authentication failed because the initiating entity did not provide valid credentials (this includes but is not limited to the case of an unknown username); sent in reply to a <response/> element or an <auth/> element with initial response data.
		/// </summary>
		public static Failure NotAuthorized;

		/// <summary>
		/// The authentication failed because of a temporary error condition within the receiving entity; sent in reply to an <auth/> element or <response/> element.
		/// </summary>
		public static Failure TemporaryAuthFailure;

		public static Failure UnknownCondition;

		static XmppFailureError()
		{
			Aborted = new Failure(FailureCondition.aborted);
			IncorrectEncoding = new Failure(FailureCondition.incorrect_encoding);
			InvalidAuthzid = new Failure(FailureCondition.invalid_authzid);
			InvalidMechanism = new Failure(FailureCondition.invalid_mechanism);
			MechanismTooWeak = new Failure(FailureCondition.mechanism_too_weak);
			NotAuthorized = new Failure(FailureCondition.not_authorized);
			TemporaryAuthFailure = new Failure(FailureCondition.temporary_auth_failure);
			UnknownCondition = new Failure(FailureCondition.UnknownCondition);
		}
	}
}
