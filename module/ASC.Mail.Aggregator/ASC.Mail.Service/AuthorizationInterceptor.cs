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
using System.Net;
using System.Reflection;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.Web;
using ASC.Core;
using Microsoft.ServiceModel.Web;

namespace ASC.Mail.Service
{
	public class Error
	{
		public string Message { get; set; }
	}

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class AuthorizationInterceptor : RequestInterceptor
	{
		public AuthorizationInterceptor()
			: base(false)
		{

		}

		public override void ProcessRequest(ref RequestContext requestContext)
		{
			if (!IsValidUserKey(requestContext))
			{
				GenerateErrorResponse(ref requestContext, HttpStatusCode.Unauthorized, "Unauthorized");
			}
		}

		public bool IsValidUserKey(RequestContext requestContext)
		{
			if (requestContext != null && requestContext.RequestMessage != null)
			{
				var prop = (HttpRequestMessageProperty)requestContext.RequestMessage.Properties[HttpRequestMessageProperty.Name];
				var cookie = prop.Headers[HttpRequestHeader.Cookie];
				if (string.IsNullOrEmpty(cookie)) return false;

				var coockieName = CookiesManager.GetCookiesName();
				foreach (string s in cookie.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (s.IndexOf('=') == -1) continue;

					var key = s.Substring(0, s.IndexOf('=')).Trim();
					if (key.Equals(coockieName, StringComparison.Ordinal))
					{
						try
						{
							var value = s.Substring(s.IndexOf('=') + 1).Trim();
							if (SecurityContext.IsAuthenticated || SecurityContext.AuthenticateMe(value))
							{
								return true;
							}
						}
						catch { }
						break;
					}
				}
			}
			return false;
		}

		private HttpContext TryGetContext(object hosting)
		{
			HttpContext context = HttpContext.Current;
			if (context == null && hosting != null)
			{
				try
				{
					//Get context through reflections
					var currentThreadDataField = hosting.GetType().GetField("currentThreadData", BindingFlags.Instance | BindingFlags.NonPublic);
					if (currentThreadDataField != null)
					{
						var currentThreadData = currentThreadDataField.GetValue(hosting);
						if (currentThreadData != null)
						{
							var currentHttpContextDataField = currentThreadData.GetType().GetField("httpContext", BindingFlags.Instance | BindingFlags.NonPublic);
							if (currentHttpContextDataField != null)
							{
								var currentContext = currentHttpContextDataField.GetValue(currentThreadData);
								if (currentContext is HttpContext)
								{
									context = (HttpContext)currentContext;
								}
							}
						}
					}
				}
				catch
				{

				}
			}
			return context;
		}

		public void GenerateErrorResponse(ref RequestContext requestContext, HttpStatusCode statusCode, string errorMessage)
		{
			// The error message is padded so that IE shows the response by default
			var reply = Message.CreateMessage(MessageVersion.None, null, new Error() { Message = errorMessage });
			var responseProp = new HttpResponseMessageProperty()
			{
				StatusCode = statusCode
			};
			reply.Properties[HttpResponseMessageProperty.Name] = responseProp;
			requestContext.Reply(reply);
			requestContext = null;
		}
	}
}