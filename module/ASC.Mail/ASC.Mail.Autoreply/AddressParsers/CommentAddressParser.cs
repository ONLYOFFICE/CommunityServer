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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ASC.Core.Tenants;
using ASC.Mail.Autoreply.ParameterResolvers;

namespace ASC.Mail.Autoreply.AddressParsers
{
    internal class CommentAddressParser : AddressParser
    {
        protected readonly string[] CommunityTypes = new[] {"blog", @"forum\.topic", "event", "wiki", "bookmark"};
        protected readonly string[] ProjectsTypes = new[] {@"project\.milestone", @"project\.task", @"project\.message"};
        protected readonly string[] FilesTypes = new[] {"file"};

        private Regex _routeRegex;

        protected override Regex GetRouteRegex()
        {
            if (_routeRegex == null)
            {
                var regex = new StringBuilder();

                regex.Append("^reply_(?'type'");
                regex.Append(string.Join("|", CommunityTypes));
                regex.Append("|");
                regex.Append(string.Join("|", ProjectsTypes));
                regex.Append("|");
                regex.Append(string.Join("|", FilesTypes));
                regex.Append(")_(?'postId'[-0-9a-zA-Z]+)_(?'parentId'[-0-9a-zA-Z]*)$");

                _routeRegex = new Regex(regex.ToString(), RegexOptions.Compiled);
            }

            return _routeRegex;
        }

        protected override ApiRequest ParseRequestInfo(IDictionary<string, string> groups, Tenant t)
        {
            ApiRequest requestInfo;
            if (groups["type"] == @"forum.topic")
            {
                requestInfo = new ApiRequest(string.Format("community/{0}/{1}", groups["type"].Replace(@"\", "").Replace('.', '/'), groups["postId"]))
                    {
                        Parameters = new List<RequestParameter>
                            {
                                new RequestParameter("subject", new TitleResolver()),
                                new RequestParameter("content", new HtmlContentResolver())
                            }
                    };

                if (!string.IsNullOrEmpty(groups["parentId"]))
                {
                    requestInfo.Parameters.Add(new RequestParameter("parentPostId", groups["parentId"]));
                }
            }
            else
            {
                requestInfo = new ApiRequest(string.Format("{0}/{1}/comment", groups["type"].Replace(@"\", "").Replace('.', '/'), groups["postId"]))
                    {
                        Parameters = new List<RequestParameter>
                            {
                                new RequestParameter("content", new HtmlContentResolver())
                            }
                    };

                if (!string.IsNullOrEmpty(groups["parentId"]))
                {
                    requestInfo.Parameters.Add(new RequestParameter("parentId", groups["parentId"]));
                }
            }

            if (CommunityTypes.Contains(groups["type"]))
            {
                requestInfo.Url = "community/" + requestInfo.Url;
            }

            return requestInfo;
        }
    }
}
