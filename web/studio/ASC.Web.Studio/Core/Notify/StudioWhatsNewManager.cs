/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Security.Authentication;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Feed;
using ASC.Feed.Data;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;

namespace ASC.Web.Studio.Core.Notify
{
    public class StudioWhatsNewService
    {
        internal readonly StudioNotifySource source;

        private static StudioWhatsNewService _instance = new StudioWhatsNewService();

        public static StudioWhatsNewService Instance
        {
            get { return _instance; }
        }

        private StudioWhatsNewService()
        {
            source = new StudioNotifySource();
        }


        public void SendMsgWhatsNew(DateTime scheduleDate, INotifyClient client)
        {
            var log = LogManager.GetLogger("ASC.Notify.WhatsNew");

            if (WebItemManager.Instance.GetItemsAll<IProduct>().Count == 0)
            {
                log.Info("No products. Return from function");
                return;
            }

            log.Info("Start send whats new.");

            var products = WebItemManager.Instance.GetItemsAll().ToDictionary(p => p.GetSysName());

            foreach (var tenantid in GetChangedTenants(scheduleDate))
            {
                try
                {
                    var tenant = CoreContext.TenantManager.GetTenant(tenantid);
                    if (tenant == null ||
                        tenant.Status != TenantStatus.Active ||
                        !TimeToSendWhatsNew(TenantUtil.DateTimeFromUtc(tenant.TimeZone, scheduleDate)) ||
                        TariffState.NotPaid <= CoreContext.PaymentManager.GetTariff(tenantid).State)
                    {
                        continue;
                    }

                    CoreContext.TenantManager.SetCurrentTenant(tenant);

                    log.InfoFormat("Start send whats new in {0} ({1}).", tenant.TenantDomain, tenantid);
                    foreach (var user in CoreContext.UserManager.GetUsers())
                    {
                        if (!IsSubscribeToWhatsNew(user))
                        {
                            continue;
                        }

                        SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(user.ID));

                        var culture = string.IsNullOrEmpty(user.CultureName) ? tenant.GetCulture() : user.GetCulture();

                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var feeds = FeedAggregateDataProvider.GetFeeds(new FeedApiFilter
                        {
                            From = scheduleDate.Date.AddDays(-1),
                            To = scheduleDate.Date.AddSeconds(-1),
                            Max = 100,
                        });

                        var feedMinWrappers = feeds.ConvertAll(f => f.ToFeedMin());

                        var feedMinGroupedWrappers = feedMinWrappers
                            .Where(f =>
                                (f.CreatedDate == DateTime.MaxValue || f.CreatedDate >= scheduleDate.Date.AddDays(-1)) && //'cause here may be old posts with new comments
                                products.ContainsKey(f.Product) &&
                                !f.Id.StartsWith("participant")
                            )
                            .GroupBy(f => products[f.Product]);

                        var ProjectsProductName = products["projects"].Name; //from ASC.Feed.Aggregator.Modules.ModulesHelper.ProjectsProductName
 
                        var activities = feedMinGroupedWrappers
                            .Where(f => f.Key.Name != ProjectsProductName) //not for project product
                            .ToDictionary(
                            g => g.Key.Name,
                            g => g.Select(f => new WhatsNewUserActivity
                            {
                                Date = f.CreatedDate,
                                UserName = f.Author != null && f.Author.UserInfo != null ? f.Author.UserInfo.DisplayUserName() : string.Empty,
                                UserAbsoluteURL = f.Author != null && f.Author.UserInfo != null ? CommonLinkUtility.GetFullAbsolutePath(f.Author.UserInfo.GetUserProfilePageURL()) : string.Empty,
                                Title = HtmlUtil.GetText(f.Title, 512),
                                URL = CommonLinkUtility.GetFullAbsolutePath(f.ItemUrl),
                                BreadCrumbs = new string[0],
                                Action = getWhatsNewActionText(f)
                            }).ToList());


                        var projectActivities = feedMinGroupedWrappers
                            .Where(f => f.Key.Name == ProjectsProductName) // for project product
                            .SelectMany(f => f);

                        var projectActivitiesWithoutBreadCrumbs = projectActivities.Where(p => String.IsNullOrEmpty(p.ExtraLocation));

                        var whatsNewUserActivityGroupByPrjs = new List<WhatsNewUserActivity>();

                        foreach (var prawbc in projectActivitiesWithoutBreadCrumbs)
                        {
                            whatsNewUserActivityGroupByPrjs.Add(
                                        new WhatsNewUserActivity
                                        {
                                            Date = prawbc.CreatedDate,
                                            UserName = prawbc.Author != null && prawbc.Author.UserInfo != null ? prawbc.Author.UserInfo.DisplayUserName() : string.Empty,
                                            UserAbsoluteURL = prawbc.Author != null && prawbc.Author.UserInfo != null ? CommonLinkUtility.GetFullAbsolutePath(prawbc.Author.UserInfo.GetUserProfilePageURL()) : string.Empty,
                                            Title = HtmlUtil.GetText(prawbc.Title, 512),
                                            URL = CommonLinkUtility.GetFullAbsolutePath(prawbc.ItemUrl),
                                            BreadCrumbs = new string[0],
                                            Action = getWhatsNewActionText(prawbc)
                                        });
                        }

                        var groupByPrjs = projectActivities.Where(p => !String.IsNullOrEmpty(p.ExtraLocation)).GroupBy(f => f.ExtraLocation);
                        foreach (var gr in groupByPrjs)
                        {
                            var grlist = gr.ToList();
                            for (var i = 0; i < grlist.Count(); i++)
                            {
                                var ls = grlist[i];
                                whatsNewUserActivityGroupByPrjs.Add(
                                    new WhatsNewUserActivity
                                    {
                                        Date = ls.CreatedDate,
                                        UserName = ls.Author != null && ls.Author.UserInfo != null ? ls.Author.UserInfo.DisplayUserName() : string.Empty,
                                        UserAbsoluteURL = ls.Author != null && ls.Author.UserInfo != null ? CommonLinkUtility.GetFullAbsolutePath(ls.Author.UserInfo.GetUserProfilePageURL()) : string.Empty,
                                        Title = HtmlUtil.GetText(ls.Title, 512),
                                        URL = CommonLinkUtility.GetFullAbsolutePath(ls.ItemUrl),
                                        BreadCrumbs = i == 0 ? new string[1]{gr.Key} : new string[0],
                                        Action = getWhatsNewActionText(ls)
                                    });
                            }
                        }

                        if (whatsNewUserActivityGroupByPrjs.Count > 0)
                        {
                            activities.Add(ProjectsProductName, whatsNewUserActivityGroupByPrjs);
                        }
                           
                        if (0 < activities.Count)
                        {
                            log.InfoFormat("Send whats new to {0}", user.Email);
                            client.SendNoticeAsync(
                                Constants.ActionSendWhatsNew, null, user, null,
                                new TagValue(Constants.TagActivities, activities),
                                new TagValue(Constants.TagDate, DateToString(scheduleDate.AddDays(-1), culture)),
                                new TagValue(CommonTags.Priority, 1)
                            );
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error(error);
                }
            }
        }

        private string getWhatsNewActionText(FeedMin feed) {

            if (feed.Module == ASC.Feed.Constants.BookmarksModule)
                return WebstudioNotifyPatternResource.ActionCreateBookmark;
            else if (feed.Module == ASC.Feed.Constants.BlogsModule)
                return WebstudioNotifyPatternResource.ActionCreateBlog;
            else if (feed.Module == ASC.Feed.Constants.ForumsModule)
            {
                if (feed.Item == "forumTopic")
                    return WebstudioNotifyPatternResource.ActionCreateForum;
                if (feed.Item == "forumPost")
                    return WebstudioNotifyPatternResource.ActionCreateForumPost;
                if (feed.Item == "forumPoll")
                    return WebstudioNotifyPatternResource.ActionCreateForumPoll;
            }
            else if (feed.Module == ASC.Feed.Constants.EventsModule)
                return WebstudioNotifyPatternResource.ActionCreateEvent;
            else if (feed.Module == ASC.Feed.Constants.ProjectsModule)
                return WebstudioNotifyPatternResource.ActionCreateProject;
            else if (feed.Module == ASC.Feed.Constants.MilestonesModule)
                return WebstudioNotifyPatternResource.ActionCreateMilestone;
            else if (feed.Module == ASC.Feed.Constants.DiscussionsModule)
                return WebstudioNotifyPatternResource.ActionCreateDiscussion;
            else if (feed.Module == ASC.Feed.Constants.TasksModule)
                return WebstudioNotifyPatternResource.ActionCreateTask;
            else if (feed.Module == ASC.Feed.Constants.CommentsModule)
                return WebstudioNotifyPatternResource.ActionCreateComment;
            else if (feed.Module == ASC.Feed.Constants.CrmTasksModule)
                return WebstudioNotifyPatternResource.ActionCreateTask;
            else if (feed.Module == ASC.Feed.Constants.ContactsModule)
                return WebstudioNotifyPatternResource.ActionCreateContact;
            else if (feed.Module == ASC.Feed.Constants.DealsModule)
                return WebstudioNotifyPatternResource.ActionCreateDeal;
            else if (feed.Module == ASC.Feed.Constants.CasesModule)
                return WebstudioNotifyPatternResource.ActionCreateCase;
            else if (feed.Module == ASC.Feed.Constants.FilesModule)
                return WebstudioNotifyPatternResource.ActionCreateFile;
            else if (feed.Module == ASC.Feed.Constants.FoldersModule)
                return WebstudioNotifyPatternResource.ActionCreateFolder;

            return "";
        }

        private IEnumerable<int> GetChangedTenants(DateTime date)
        {
            return new FeedAggregateDataProvider().GetTenants(new TimeInterval(date.Date.AddDays(-1), date.Date.AddSeconds(-1)));
        }

        private bool TimeToSendWhatsNew(DateTime currentTime)
        {
            var hourToSend = 7;
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["web.whatsnew-time"]))
            {
                var hour = 0;
                if (int.TryParse(WebConfigurationManager.AppSettings["web.whatsnew-time"], out hour))
                {
                    hourToSend = hour;
                }
            }
            return currentTime.Hour == hourToSend;
        }

        private string DateToString(DateTime d, CultureInfo c)
        {
            return d.ToString(c.TwoLetterISOLanguageName == "ru" ? "d MMMM" : "M", c);
        }

        public bool IsSubscribeToWhatsNew(Guid userID)
        {
            return IsSubscribeToWhatsNew(ToRecipient(userID));
        }

        private bool IsSubscribeToWhatsNew(IRecipient recipient)
        {
            if (recipient == null) return false;
            return source.GetSubscriptionProvider().IsSubscribed(Constants.ActionSendWhatsNew, recipient, null);
        }

        public void SubscribeToWhatsNew(Guid userID, bool subscribe)
        {
            var recipient = ToRecipient(userID);
            if (recipient != null)
            {
                if (subscribe)
                {
                    source.GetSubscriptionProvider().Subscribe(Constants.ActionSendWhatsNew, null, recipient);
                }
                else
                {
                    source.GetSubscriptionProvider().UnSubscribe(Constants.ActionSendWhatsNew, null, recipient);
                }
            }
        }


        #region Helpers

        private IRecipient ToRecipient(Guid userID)
        {
            return source.GetRecipientsProvider().GetRecipient(userID.ToString());
        }

        #endregion

        class WhatsNewUserActivity
        {
            public IList<string> BreadCrumbs { get; set; }
            public string Title { get; set; }
            public string URL { get; set; }
            public string UserName { get; set; }
            public string UserAbsoluteURL { get; set; }
            public DateTime Date { get; set; }
            public string Action { get; set; }
        }
    }
}