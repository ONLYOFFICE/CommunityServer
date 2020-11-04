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
using System.Collections.Generic;
using ASC.Web.Core.Subscriptions;

namespace ASC.Web.Core
{
    public class WebItemContext
    {
        public SpaceUsageStatManager SpaceUsageStatManager { get; set; }

        public Func<string> GetCreateContentPageAbsoluteUrl { get; set; }

        public ISubscriptionManager SubscriptionManager { get; set; }

        public Func<List<string>> UserOpportunities { get; set; }

        public Func<List<string>> AdminOpportunities { get; set; }

        public string SmallIconFileName { get; set; }

        public string DisabledIconFileName { get; set; }
        
        public string IconFileName { get; set; }

        public string LargeIconFileName { get; set; }

        public int DefaultSortOrder { get; set; }

        public bool HasComplexHierarchyOfAccessRights { get; set; }

        public bool CanNotBeDisabled { get; set; }

        public WebItemContext()
        {
            GetCreateContentPageAbsoluteUrl = () => string.Empty;
        }
    }
}
