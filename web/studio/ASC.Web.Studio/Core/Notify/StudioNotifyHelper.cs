/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Web.Configuration;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Notify
{
    class StudioNotifyHelper
    {
        public static readonly string Helplink;

        public static readonly StudioNotifySource NotifySource;

        public static readonly ISubscriptionProvider SubscriptionProvider;

        public static readonly IRecipientProvider RecipientsProvider;

        private static readonly int CountMailsToNotActivated;

        private static readonly string NotificationImagePath;

        static StudioNotifyHelper()
        {
            Helplink = CommonLinkUtility.GetHelpLink(false);
            NotifySource = new StudioNotifySource();
            SubscriptionProvider = NotifySource.GetSubscriptionProvider();
            RecipientsProvider = NotifySource.GetRecipientsProvider();

            int.TryParse(WebConfigurationManager.AppSettings["core.notify.countspam"], out CountMailsToNotActivated);
            NotificationImagePath = WebConfigurationManager.AppSettings["web.notification.image.path"];
        }


        public static IEnumerable<UserInfo> GetRecipients(bool toadmins, bool tousers, bool toguests)
        {
            if (toadmins)
            {
                if (tousers)
                {
                    if (toguests)
                        return CoreContext.UserManager.GetUsers();

                    return CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User);
                }

                if (toguests)
                    return
                        CoreContext.UserManager.GetUsersByGroup(Constants.GroupAdmin.ID)
                                   .Concat(CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.Visitor));

                return CoreContext.UserManager.GetUsersByGroup(Constants.GroupAdmin.ID);
            }

            if (tousers)
            {
                if (toguests)
                    return CoreContext.UserManager.GetUsers()
                                      .Where(u => !CoreContext.UserManager.IsUserInGroup(u.ID, Constants.GroupAdmin.ID));

                return CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User)
                                  .Where(u => !CoreContext.UserManager.IsUserInGroup(u.ID, Constants.GroupAdmin.ID));
            }

            if (toguests)
                return CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.Visitor);

            return new List<UserInfo>();
        }

        public static IRecipient ToRecipient(Guid userId)
        {
            return RecipientsProvider.GetRecipient(userId.ToString());
        }

        public static IRecipient[] RecipientFromEmail(string email, bool checkActivation)
        {
            return RecipientFromEmail(new List<string> { email }, checkActivation);
        }

        public static IRecipient[] RecipientFromEmail(List<string> emails, bool checkActivation)
        {
            var res = new List<IRecipient>();

            if (emails == null) return res.ToArray();

            res.AddRange(emails.
                             Select(email => email.ToLower()).
                             Select(e => new DirectRecipient(e, null, new[] { e }, checkActivation)));

            if (!checkActivation
                && CountMailsToNotActivated > 0
                && TenantExtra.Saas && !CoreContext.Configuration.Personal)
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var tariff = CoreContext.TenantManager.GetTenantQuota(tenant.TenantId);
                if (tariff.Free || tariff.Trial)
                {
                    var sended = SpamEmailSettings.MailsSended;

                    var mayTake = Math.Max(0, CountMailsToNotActivated - sended);
                    var tryCount = res.Count();
                    if (mayTake < tryCount)
                    {
                        res = res.Take(mayTake).ToList();

                        LogManager.GetLogger("ASC.Notify")
                                  .Warn(string.Format("Free tenant {0} for today is trying to send {1} more letters without checking activation. Sent {2}",
                                                      tenant.TenantId, tryCount, mayTake));
                    }
                    SpamEmailSettings.MailsSended = sended + tryCount;
                }
            }

            return res.ToArray();
        }


        public static string GetNotifyAnalytics(int tenantId, INotifyAction action, bool toowner, bool toadmins,
                                                bool tousers, bool toguests)
        {
            if (string.IsNullOrEmpty(SetupInfo.NotifyAnalyticsUrl))
                return string.Empty;

            var target = "";

            if (toowner) target = "owner";
            if (toadmins) target += string.IsNullOrEmpty(target) ? "admin" : "-admin";
            if (tousers) target += string.IsNullOrEmpty(target) ? "user" : "-user";
            if (toguests) target += string.IsNullOrEmpty(target) ? "guest" : "-guest";

            return string.Format("<img src=\"{0}\" width=\"1\" height=\"1\"/>",
                                 string.Format(SetupInfo.NotifyAnalyticsUrl,
                                               tenantId,
                                               target,
                                               action.ID));
        }

        public static string GetNotificationImageUrl(string imageFileName)
        {
            if (string.IsNullOrEmpty(NotificationImagePath))
            {
                return
                    CommonLinkUtility.GetFullAbsolutePath(
                        WebImageSupplier.GetAbsoluteWebPath("notification/" + imageFileName));
            }

            return NotificationImagePath.TrimEnd('/') + "/" + imageFileName;
        }


        public static bool IsSubscribedToNotify(Guid userId, INotifyAction notifyAction)
        {
            return IsSubscribedToNotify(ToRecipient(userId), notifyAction);
        }

        public static bool IsSubscribedToNotify(IRecipient recipient, INotifyAction notifyAction)
        {
            return recipient != null && SubscriptionProvider.IsSubscribed(notifyAction, recipient, null);
        }

        public static void SubscribeToNotify(Guid userId, INotifyAction notifyAction, bool subscribe)
        {
            SubscribeToNotify(ToRecipient(userId), notifyAction, subscribe);
        }

        public static void SubscribeToNotify(IRecipient recipient, INotifyAction notifyAction, bool subscribe)
        {
            if (recipient == null) return;

            if (subscribe)
            {
                SubscriptionProvider.Subscribe(notifyAction, null, recipient);
            }
            else
            {
                SubscriptionProvider.UnSubscribe(notifyAction, null, recipient);
            }
        }
    }
}