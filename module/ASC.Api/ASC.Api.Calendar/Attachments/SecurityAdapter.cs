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


using System;
using System.Collections.Generic;

using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;

namespace ASC.Api.Calendar.Attachments
{
    public class SecurityAdapter : IFileSecurity
    {
        private BusinessObjects.Calendar calendarObj;
        private BusinessObjects.Event eventObj;

        public SecurityAdapter()
        {
        }

        public SecurityAdapter(string data)
        {
            if (int.TryParse(data, out int id))
            {
                using (var provider = new BusinessObjects.DataProvider())
                {
                    eventObj = provider.GetEventById(id);
                    if (eventObj != null) {
                        calendarObj = provider.GetCalendarById(Convert.ToInt32(eventObj.CalendarId));
                    }
                }
            }
        }

        public SecurityAdapter(BusinessObjects.Event eventObj, BusinessObjects.Calendar calendarObj)
        {
            this.eventObj = eventObj;
            this.calendarObj = calendarObj;
        }

        public bool CanRead(FileEntry entry, Guid userId)
        {
            if (entry == null) return false;

            if (userId == FileConstant.ShareLinkId)
            {
                return true;
            }

            if (eventObj != null)
            {
                return eventObj.OwnerId == userId || eventObj.SharingOptions.PublicItems.Find(x => x.Id == userId) != null;
            }

            return false;
        }

        public bool CanCustomFilterEdit(FileEntry file, Guid userId)
        {
            return false;
        }

        public bool CanComment(FileEntry entry, Guid userId)
        {
            return false;
        }

        public bool CanFillForms(FileEntry entry, Guid userId)
        {
            return false;
        }

        public bool CanReview(FileEntry entry, Guid userId)
        {
            return false;
        }

        public bool CanCreate(FileEntry entry, Guid userId)
        {
            return false;
        }

        public bool CanDelete(FileEntry entry, Guid userId)
        {
            if (eventObj != null)
            {
                return eventObj.OwnerId == userId
                    || SecurityContext.PermissionResolver.Check(CoreContext.Authentication.GetAccountByID(userId), eventObj, 
                                                                    null, CalendarAccessRights.FullAccessAction)
                    || SecurityContext.PermissionResolver.Check(CoreContext.Authentication.GetAccountByID(userId), calendarObj, 
                                                                    null, CalendarAccessRights.FullAccessAction);
            }

            return false;
        }

        public bool CanEdit(FileEntry entry, Guid userId)
        {
            return false;
        }

        public IEnumerable<Guid> WhoCanRead(FileEntry entry)
        {
            var list = new List<Guid> { FileConstant.ShareLinkId };

            if (eventObj != null)
            {
                list.Add(eventObj.OwnerId);

                foreach (var item in eventObj.SharingOptions.PublicItems)
                {
                    list.Add(item.Id);
                }
            }

            return list;
        }

        public bool CanDownload(FileEntry entry, Guid userId)
        {
            return CanRead(entry, userId);
        }
    }
}