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
using System.Text.RegularExpressions;
using ASC.Core.Tenants;
using ASC.Mail.Autoreply.ParameterResolvers;

namespace ASC.Mail.Autoreply.AddressParsers
{
    internal class CommunityAddressParser : AddressParser
    {
        protected override Regex GetRouteRegex()
        {
            return new Regex(@"^(?'type'blog|event)$", RegexOptions.Compiled);
        }

        protected override ApiRequest ParseRequestInfo(IDictionary<string,string> groups, Tenant t)
        {
            var callInfo = new ApiRequest("community/" + groups["type"])
                {
                    Parameters = new List<RequestParameter>
                        {
                            new RequestParameter("content", new HtmlContentResolver()),
                            new RequestParameter("title", new TitleResolver(BlogTagsResolver.Pattern)),
                            new RequestParameter("subscribeComments", true),
                            new RequestParameter("type", 1),
                            new RequestParameter("tags", new BlogTagsResolver())
                        }
                };

            return callInfo;
        }
    }
}