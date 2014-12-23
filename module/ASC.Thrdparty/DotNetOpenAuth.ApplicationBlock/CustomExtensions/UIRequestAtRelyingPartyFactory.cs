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

//-----------------------------------------------------------------------
// <copyright file="UIRequestAtRelyingPartyFactory.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock.CustomExtensions {
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions.UI;

	/// <summary>
	/// An extension factory that allows the <see cref="UIRequest"/> extension to be received by the relying party.
	/// </summary>
	/// <remarks>
	/// Typically UIRequest is only received by the Provider.  But Google mirrors back this data to the relying party
	/// if our web user is already logged into Google.
	/// See the OpenIdRelyingPartyWebForms sample's DetectGoogleSession.aspx page for usage of this factory.
	/// </remarks>
	public class UIRequestAtRelyingPartyFactory : IOpenIdExtensionFactory {
		/// <summary>
		/// The Type URI for the UI extension.
		/// </summary>
		private const string UITypeUri = "http://specs.openid.net/extensions/ui/1.0";

		/// <summary>
		/// Allows UIRequest extensions to be received by the relying party.  Useful when Google mirrors back the request
		/// to indicate that a user is logged in.
		/// </summary>
		/// <param name="typeUri">The type URI of the extension.</param>
		/// <param name="data">The parameters associated specifically with this extension.</param>
		/// <param name="baseMessage">The OpenID message carrying this extension.</param>
		/// <param name="isProviderRole">A value indicating whether this extension is being received at the OpenID Provider.</param>
		/// <returns>
		/// An instance of <see cref="IOpenIdMessageExtension"/> if the factory recognizes
		/// the extension described in the input parameters; <c>null</c> otherwise.
		/// </returns>
		public DotNetOpenAuth.OpenId.Messages.IOpenIdMessageExtension Create(string typeUri, IDictionary<string, string> data, IProtocolMessageWithExtensions baseMessage, bool isProviderRole) {
			if (typeUri == UITypeUri && !isProviderRole) {
				return new UIRequest();
			}

			return null;
		}
	}
}
