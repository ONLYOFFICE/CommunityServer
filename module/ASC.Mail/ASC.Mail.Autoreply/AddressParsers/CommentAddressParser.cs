/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
