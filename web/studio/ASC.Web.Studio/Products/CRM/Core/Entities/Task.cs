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


#region Usings

using System;
using ASC.Common.Security;

#endregion

namespace ASC.CRM.Core.Entities
{
    public class Task : DomainObject, ISecurityObjectId
    {

        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid? LastModifedBy { get; set; }

        public DateTime? LastModifedOn { get; set; }

        public int ContactID { get; set; }

        public Contact Contact { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DeadLine { get; set; }

        public Guid ResponsibleID { get; set; }
      
        public bool IsClosed { get; set; }

        public int CategoryID { get; set; }

        public EntityType EntityType { get; set; }

        public int EntityID { get; set; }

        public int AlertValue { get; set; }

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
