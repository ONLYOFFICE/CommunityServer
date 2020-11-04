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
