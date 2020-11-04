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
    internal class ProjectAddressParser : AddressParser
    {
        protected override Regex GetRouteRegex()
        {
            return new Regex(@"^(?'type'task|message)_(?'projectId'\d+)$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        protected override ApiRequest ParseRequestInfo(IDictionary<string, string> groups, Tenant t)
        {
            var type = groups["type"];
            if (type == "task")
            {
                return new ApiRequest(string.Format("project/{0}/task", groups["projectId"]))
                {
                    Parameters = new List<RequestParameter>
                        {
                            new RequestParameter("description", new PlainTextContentResolver()),
                            new RequestParameter("deadline", new TaskDeadlineResolver()),
                            new RequestParameter("priority", new TaskPriorityResolver()),
                            new RequestParameter("milestoneid", new TaskMilestoneResolver()),
                            new RequestParameter("responsibles", new TaskResponsiblesResolver()),
                            new RequestParameter("responsible", new TaskResponsibleResolver()),
                            new RequestParameter("title", new TitleResolver(TaskDeadlineResolver.Pattern, TaskPriorityResolver.Pattern, TaskMilestoneResolver.Pattern, TaskResponsiblesResolver.Pattern))
                        }
                };
            }

            return new ApiRequest(string.Format("project/{0}/message", groups["projectId"]))
                {
                    Parameters = new List<RequestParameter>
                        {
                            new RequestParameter("title", new TitleResolver()),
                            new RequestParameter("content", new HtmlContentResolver())
                        }
                };
        }
    }
}