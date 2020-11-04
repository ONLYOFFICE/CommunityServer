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


#region Import

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;

#endregion

namespace ASC.CRM.Core.Entities
{

    [DataContract]
    public class DealMilestone : DomainObject
    {
        [DataMember(Name = "title")]
        public String Title { get; set; }

        [DataMember(Name = "description")]
        public String Description { get; set; }

        [DataMember(Name = "color")]
        public String Color { get; set; }

        [DataMember(Name = "sort_order")]
        public int SortOrder { get; set; }

        [DataMember(Name = "probability")]
        public int Probability { get; set; }

        [DataMember(Name = "status")]
        public DealMilestoneStatus Status { get; set; }

    }
}