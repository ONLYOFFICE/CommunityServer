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