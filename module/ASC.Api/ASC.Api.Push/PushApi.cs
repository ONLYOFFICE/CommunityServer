/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Core;
using ASC.Core.Common.Notify.Push;
using ASC.Specific;

namespace ASC.Api.Push
{
    public class PushApi : IApiEntryPoint
    {
        public string Name
        {
            get { return "push"; }
        }

        [Create("device_token")]
        public object RegisterDevice(string token, DeviceType type)
        {
            UsageData.SetIsMobileAppUser(SecurityContext.CurrentAccount.ID, type);

            using (var pushClient = new PushServiceClient())
            {
                string regid = pushClient.RegisterDevice(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    SecurityContext.CurrentAccount.ID.ToString(),
                    token,
                    type);

                return new {regid};
            }
        }

        [Delete("device_token")]
        public void DeregisterDevice(string token)
        {
            using (var pushClient = new PushServiceClient())
            {
                pushClient.DeregisterDevice(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    SecurityContext.CurrentAccount.ID.ToString(),
                    token);
            }
        }

        [Create("send")]
        public void SendNotification(string message, int? badge, IEnumerable<string> deviceTokens)
        {
            using (var pushClient = new PushServiceClient())
            {
                pushClient.EnqueueNotification(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    SecurityContext.CurrentAccount.ID.ToString(),
                    PushNotification.ApiNotification(message, badge),
                    deviceTokens.ToList());
            }
        }

        [Read("feed")]
        public IEnumerable<GetFeedResponse> GetFeed(string deviceToken, ApiDateTime from, ApiDateTime to)
        {
            using (var pushClient = new PushServiceClient())
            {
                return pushClient
                    .GetFeed(CoreContext.TenantManager.GetCurrentTenant().TenantId,
                             SecurityContext.CurrentAccount.ID.ToString(),
                             deviceToken,
                             from ?? DateTime.MinValue,
                             to ?? DateTime.MaxValue)
                    .Select(notification => new GetFeedResponse(notification));
            }
        }

        [Read("isappuser")]
        public bool IsMobileAppUser(DeviceType? type)
        {
            return UsageData.GetIsMobileAppUser(SecurityContext.CurrentAccount.ID, type);
        }

        [DataContract(Name = "entry")]
        public class GetFeedResponse
        {
            [DataMember(Name = "module")]
            public string Module { get; set; }

            [DataMember(Name = "action")]
            public string Action { get; set; }

            [DataMember(Name = "item_type")]
            public string ItemType { get; set; }

            [DataMember(Name = "item_id")]
            public string ItemID { get; set; }

            [DataMember(Name = "item_description")]
            public string ItemDescription { get; set; }

            [DataMember(Name = "additional")]
            public Dictionary<string, string> AdditionalInfo { get; set; }

            [DataMember(Name = "queued_on")]
            public ApiDateTime QueuedOn { get; set; }

            public GetFeedResponse(PushNotification notification)
            {
                Module = notification.Module.ToString().ToLowerInvariant();
                Action = notification.Action.ToString().ToLowerInvariant();
                ItemType = notification.Item.Type.ToString().ToLowerInvariant();
                ItemID = notification.Item.ID;
                ItemDescription = notification.Item.Description;
                QueuedOn = (ApiDateTime)notification.QueuedOn;

                if (notification.ParentItem != null)
                    AdditionalInfo = new Dictionary<string, string>
                        {
                            {"parent_type", notification.ParentItem.Type.ToString().ToLowerInvariant()},
                            {"parent_id", notification.ParentItem.ID},
                            {"parent_title", notification.ParentItem.Description}
                        };
            }
        }
    }
}
