/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Web.Community.News.Code
{
	public class RequestInfo
	{
		public Guid UserId = Guid.Empty;

		public string UserIdAttribute = string.Empty;

		public UserInfo User = null;

		public bool HasUser = false;

		public RequestInfo(HttpRequest request)
		{
			try
			{
				if (!string.IsNullOrEmpty(request["uid"]))
				{
					UserId = new Guid(request["uid"]);
					if (UserId != Guid.Empty)
					{
						HasUser = true;
						UserIdAttribute = string.Format(CultureInfo.CurrentCulture, "&uid={0}", UserId);
						User = CoreContext.UserManager.GetUsers(UserId);
					}
				}
			}
			catch { }
		}
	}
}
