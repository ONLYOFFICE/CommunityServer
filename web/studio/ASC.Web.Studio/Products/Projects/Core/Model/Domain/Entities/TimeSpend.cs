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

namespace ASC.Projects.Core.Domain
{
    public class TimeSpend : DomainObject<int>
    {
        public override EntityType EntityType { get { return EntityType.TimeSpend; } }

        public Task Task { get; set; }

        public DateTime Date { get; set; }

        public float Hours { get; set; }

        public Guid Person { get; set; }

        public string Note { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public DateTime StatusChangedOn { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid CreateBy { get; set; }
    }
}
