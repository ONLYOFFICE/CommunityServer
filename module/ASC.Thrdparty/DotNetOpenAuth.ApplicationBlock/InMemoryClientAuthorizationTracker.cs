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


//-----------------------------------------------------------------------
// <copyright file="InMemoryClientAuthorizationTracker.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.ServiceModel;
	using System.Text;
	using System.Threading;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

#if SAMPLESONLY
	internal class InMemoryClientAuthorizationTracker : IClientAuthorizationTracker {
		private readonly Dictionary<int, IAuthorizationState> savedStates = new Dictionary<int, IAuthorizationState>();
		private int stateCounter;

		#region Implementation of IClientTokenManager

		/// <summary>
		/// Gets the state of the authorization for a given callback URL and client state.
		/// </summary>
		/// <param name="callbackUrl">The callback URL.</param>
		/// <param name="clientState">State of the client stored at the beginning of an authorization request.</param>
		/// <returns>The authorization state; may be <c>null</c> if no authorization state matches.</returns>
		public IAuthorizationState GetAuthorizationState(Uri callbackUrl, string clientState) {
			IAuthorizationState state;
			if (this.savedStates.TryGetValue(int.Parse(clientState), out state)) {
				if (state.Callback != callbackUrl) {
					throw new DotNetOpenAuth.Messaging.ProtocolException("Client state and callback URL do not match.");
				}
			}

			return state;
		}

		#endregion

		internal IAuthorizationState NewAuthorization(HashSet<string> scope, out string clientState) {
			int counter = Interlocked.Increment(ref this.stateCounter);
			clientState = counter.ToString(CultureInfo.InvariantCulture);
			return this.savedStates[counter] = new AuthorizationState(scope);
		}
	}
#endif
}
