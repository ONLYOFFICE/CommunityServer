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
using ASC.Common.Security;

namespace ASC.CRM.Core.Entities
{
    public class Cases : DomainObject, ISecurityObjectId
    {
        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid? LastModifedBy { get; set; }

        public DateTime? LastModifedOn { get; set; }

        public String Title { get; set; }

        public bool IsClosed { get; set; }

        public HashSet<int> Members { get; set; }

        // TODO: to finish the field
        public DateTime ClosedDate { get; set; }

        public object SecurityId
        {
            get { return ID; }
        }

        public Type ObjectType
        {
            get { return GetType(); }
        }
    }
}