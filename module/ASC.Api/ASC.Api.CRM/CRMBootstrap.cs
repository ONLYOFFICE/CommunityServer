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


using System.Collections.Generic;
using ASC.Api.Interfaces;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Web.Core;
using ASC.Web.Core.Calendars;
using ASC.Web.Files.Api;

namespace ASC.Api.CRM
{
    public class CRMBootstrap : IApiBootstrapper
    {
        public void Configure()
        {
            if (!FilesIntegration.IsRegisteredFileSecurityProvider("crm", "crm_common"))
            {
                FilesIntegration.RegisterFileSecurityProvider("crm", "crm_common", new FileSecurityProvider());
            }

            //Register prodjects' calendar events
            CalendarManager.Instance.RegistryCalendarProvider(userid =>
                {
                    if (WebItemSecurity.IsAvailableForUser(WebItemManager.CRMProductID, userid))
                    {
                        return new List<BaseCalendar> {new CRMCalendar(userid)};
                    }
                    return new List<BaseCalendar>();
                });
        }
    }
}