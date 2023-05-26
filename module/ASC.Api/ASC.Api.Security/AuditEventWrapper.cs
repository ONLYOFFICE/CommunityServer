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


using System;
using System.Collections.Generic;

using ASC.AuditTrail;
using ASC.AuditTrail.Mappers;
using ASC.AuditTrail.Types;
using ASC.MessagingSystem;
using ASC.Specific;

namespace ASC.Api.Security
{
    public class AuditEventWrapper
    {
        public AuditEventWrapper(AuditEvent auditEvent)
        {
            Id = auditEvent.Id;
            Date = (ApiDateTime)auditEvent.Date;
            User = auditEvent.UserName;
            UserId = auditEvent.UserId;
            Action = auditEvent.ActionText;
            ActionId = (MessageAction)auditEvent.Action;
            IP = auditEvent.IP;
            Browser = auditEvent.Browser;
            Platform = auditEvent.Platform;
            Page = auditEvent.Page;

            var maps = AuditActionMapper.GetMessageMaps(auditEvent.Action);

            ActionType = maps.ActionType;
            Product = maps.ProductType;
            Module = maps.ModuleType;

            var list = new List<EntryType>(2);

            if (maps.EntryType1 != EntryType.None)
            {
                list.Add(maps.EntryType1);
            }

            if (maps.EntryType2 != EntryType.None)
            {
                list.Add(maps.EntryType2);
            }

            Entries = list;

            if (auditEvent.Target != null)
            {
                Target = auditEvent.Target.GetItems();
            }
        }

        public int Id { get; set; }

        public ApiDateTime Date { get; set; }

        public string User { get; set; }

        public Guid UserId { get; set; }

        public string Action { get; set; }

        public MessageAction ActionId { get; set; }

        public string IP { get; set; }

        public string Browser { get; set; }

        public string Platform { get; set; }

        public string Page { get; set; }

        public ActionType ActionType { get; set; }

        public ProductType Product { get; set; }

        public ModuleType Module { get; set; }

        public IEnumerable<string> Target { get; set; }

        public IEnumerable<EntryType> Entries { get; set; }

    }
}