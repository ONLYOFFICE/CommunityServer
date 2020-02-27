/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration;

namespace ASC.AuditTrail
{
    public class BaseEvent
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public Guid UserId { get; set; }

        public bool Mobile { get; set; }

        public IList<string> Description { get; set; }

        [Event("IpCol")]
        public string IP { get; set; }

        [Event("BrowserCol")]
        public string Browser { get; set; }

        [Event("PlatformCol")]
        public string Platform { get; set; }

        [Event("DateCol")]
        public DateTime Date { get; set; }

        [Event("UserCol")]
        public string UserName { get; set; }

        [Event("PageCol")]
        public string Page { get; set; }

        [Event("ActionCol")]
        public string ActionText { get; set; }
    }

    internal class BaseEventMap<T> : ClassMap<T> where T : BaseEvent
    {
        public BaseEventMap()
        {
            var eventType = typeof(T);
            var eventProps = eventType
                .GetProperties()
                .Where(r => r.GetCustomAttribute<EventAttribute>() != null)
                .OrderBy(r=> r.GetCustomAttribute<EventAttribute>().Order);

            foreach (var prop in eventProps)
            {
                var attr = prop.GetCustomAttribute<EventAttribute>().Resource;
                Map(eventType, prop).Name(AuditReportResource.ResourceManager.GetString(attr));
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class EventAttribute : Attribute
    {
        public string Resource { get; private set; }

        public int Order { get; private set; }

        public EventAttribute(string resource, int order = 0)
        {
            Resource = resource;
            Order = order;
        }
    }
}
