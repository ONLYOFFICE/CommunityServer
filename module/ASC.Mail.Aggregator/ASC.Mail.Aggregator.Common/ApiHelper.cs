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
using System.Web;
using System.Web.Configuration;
using ASC.Core;

namespace ASC.Mail.Aggregator.Common
{
    public static class ApiHelper
    {
        public static UriBuilder GetApiRequestUrl(string api_url)
        {
            var temp_url = api_url;
            var request_uri_builder =
                new UriBuilder(HttpContext.Current != null ? HttpContext.Current.Request.Url.Scheme : Uri.UriSchemeHttp,
                               CoreContext.TenantManager.GetCurrentTenant().TenantAlias);

            if (CoreContext.TenantManager.GetCurrentTenant().TenantAlias == "localhost")
            {
                var virtual_dir = WebConfigurationManager.AppSettings["core.virtual-dir"];
                if (!string.IsNullOrEmpty(virtual_dir)) temp_url = virtual_dir.Trim('/') + "/" + temp_url;

                var host = WebConfigurationManager.AppSettings["core.host"];
                if (!string.IsNullOrEmpty(host)) request_uri_builder.Host = host;

                var port = WebConfigurationManager.AppSettings["core.port"];
                if (!string.IsNullOrEmpty(port)) request_uri_builder.Port = int.Parse(port);
            }
            else
                request_uri_builder.Host += "." + WebConfigurationManager.AppSettings["core.base-domain"];

            request_uri_builder.Path = temp_url;
            return request_uri_builder;
        }

        public static Cookie GetAuthCookie(Guid user_id, string host)
        {
            return new Cookie("asc_auth_key", SecurityContext.AuthenticateMe(user_id), "/", host);
        }
    }
}
