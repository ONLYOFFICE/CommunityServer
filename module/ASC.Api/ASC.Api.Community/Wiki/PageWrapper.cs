/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Specific;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Api.Wiki.Wrappers
{
    [DataContract(Name = "page", Namespace = "")]
    public class PageWrapper
    {
        ///<example>Page name</example>
        ///<order>0</order>
        [DataMember(Order = 0)]
        public string Name { get; set; }

        ///<example>Page content</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public string Content { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public EmployeeWraper UpdatedBy { get; set; }

        ///<example>2020-12-08T17:37:03.3164965Z</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public ApiDateTime Updated { get; set; }

        public PageWrapper(Page page)
        {
            Name = page.PageName;
            Content = page.Body;
            UpdatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(page.UserID));
            Updated = (ApiDateTime)page.Date;
        }

        public PageWrapper()
        {

        }

        public static PageWrapper GetSample()
        {
            return new PageWrapper
            {
                Name = "Page name",
                Content = "Page content",
                UpdatedBy = EmployeeWraper.GetSample(),
                Updated = ApiDateTime.GetSample()
            };
        }
    }
}
