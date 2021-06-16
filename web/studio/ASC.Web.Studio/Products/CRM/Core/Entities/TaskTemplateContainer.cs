/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

#endregion

namespace ASC.CRM.Core.Entities
{
    public class TaskTemplateContainer : DomainObject
    {

        public String Title { get; set; }

        public EntityType EntityType { get; set; }

    }

    public class TaskTemplate : DomainObject
    {
        public int ContainerID { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public Guid ResponsibleID { get; set; }

        public int CategoryID { get; set; }

        public bool isNotify { get; set; }

        public TimeSpan Offset { get; set; }

        public bool DeadLineIsFixed { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

    }
}