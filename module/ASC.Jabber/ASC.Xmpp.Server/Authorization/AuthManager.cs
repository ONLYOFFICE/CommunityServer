/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Xmpp.Server.Utils;

namespace ASC.Xmpp.Server.Authorization
{
	public class AuthManager
	{
		private readonly Dictionary<string, AuthToken> tokenInfos = new Dictionary<string, AuthToken>();

		private const int tokenValidity = 30;//30 min

		public string GetUserToken(string username)
		{
			if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");

			var token = new AuthToken(username);
			if (!tokenInfos.ContainsKey(token.Token))
			{
				tokenInfos.Add(token.Token, token);
				return token.Token;
			}
			return string.Empty;
		}

		public string RestoreUserToken(string token)
		{
			if (string.IsNullOrEmpty(token)) throw new ArgumentNullException("token");

			if (tokenInfos.ContainsKey(token) && (DateTime.UtcNow - tokenInfos[token].Created).TotalMinutes < tokenValidity)
			{
				var user = tokenInfos[token].Username;
				CleanTokens(token);
				return user;
			}
			return null;
		}

		private void CleanTokens(string tokeset)
		{
			tokenInfos.Remove(tokeset);
			foreach (var info in new Dictionary<string, AuthToken>(tokenInfos))
			{
				if ((DateTime.UtcNow - info.Value.Created).TotalMinutes > tokenValidity)
				{
					tokenInfos.Remove(info.Key);//remove token
				}
			}
		}


		private class AuthToken
		{
			public string Username
			{
				get;
				private set;
			}

			public DateTime Created
			{
				get;
				private set;
			}

			public string Token
			{
				get;
				private set;
			}

			public AuthToken(string username)
			{
				Username = username;
				Created = DateTime.UtcNow;
				Token = UniqueId.CreateNewId(50);
			}
		}
	}
}