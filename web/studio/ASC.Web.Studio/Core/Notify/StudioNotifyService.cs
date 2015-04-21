/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
    public class StudioNotifyService
    {
        private readonly INotifyClient client;
        internal readonly StudioNotifySource source;

        private static string EMailSenderName { get { return ASC.Core.Configuration.Constants.NotifyEMailSenderSysName; } }

        public static StudioNotifyService Instance
        {
            get;
            private set;
        }


        static StudioNotifyService()
        {
            Instance = new StudioNotifyService();
        }


        private StudioNotifyService()
        {
            source = new StudioNotifySource();
            client = WorkContext.NotifyContext.NotifyService.RegisterClient(source);
        }

        public void RegisterSendMethod()
        {
            if (WebConfigurationManager.AppSettings["core.notify.tariff"] != "false")
            {
                client.RegisterSendMethod(SendTariffWarnings, "0 0 5 ? * *"); // 5am every day
            }
            if (CoreContext.Configuration.Personal)
            {
                client.RegisterSendMethod(SendLettersPersonal, "0 0 5 ? * *");
            }
            else
            {
                client.RegisterSendMethod(SendMsgWhatsNew, "0 0 * ? * *"); // every hour
            }
        }


        public void SendMsgWhatsNew(DateTime scheduleDate)
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
                        Thread.CurrentThread.CurrentCulture = user.GetCulture();
                        Thread.CurrentThread.CurrentUICulture = user.GetCulture();

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
                                new TagValue(Constants.TagDate, DateToString(scheduleDate.AddDays(-1), user.GetCulture())),
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
                return WebstudioPatternResource.ActionCreateBookmark;
            else if (feed.Module == ASC.Feed.Constants.BlogsModule)
                return WebstudioPatternResource.ActionCreateBlog;
            else if (feed.Module == ASC.Feed.Constants.ForumsModule)
            {
                if (feed.Item == "forumTopic")
                    return WebstudioPatternResource.ActionCreateForum;
                if (feed.Item == "forumPost")
                    return WebstudioPatternResource.ActionCreateForumPost;
                if (feed.Item == "forumPoll")
                    return WebstudioPatternResource.ActionCreateForumPoll;
            }
            else if (feed.Module == ASC.Feed.Constants.EventsModule)
                return WebstudioPatternResource.ActionCreateEvent;
            else if (feed.Module == ASC.Feed.Constants.ProjectsModule)
                return WebstudioPatternResource.ActionCreateProject;
            else if (feed.Module == ASC.Feed.Constants.MilestonesModule)
                return WebstudioPatternResource.ActionCreateMilestone;
            else if (feed.Module == ASC.Feed.Constants.DiscussionsModule)
                return WebstudioPatternResource.ActionCreateDiscussion;
            else if (feed.Module == ASC.Feed.Constants.TasksModule)
                return WebstudioPatternResource.ActionCreateTask;
            else if (feed.Module == ASC.Feed.Constants.CommentsModule)
                return WebstudioPatternResource.ActionCreateComment;
            else if (feed.Module == ASC.Feed.Constants.CrmTasksModule)
                return WebstudioPatternResource.ActionCreateTask;
            else if (feed.Module == ASC.Feed.Constants.ContactsModule)
                return WebstudioPatternResource.ActionCreateContact;
            else if (feed.Module == ASC.Feed.Constants.DealsModule)
                return WebstudioPatternResource.ActionCreateDeal;
            else if (feed.Module == ASC.Feed.Constants.CasesModule)
                return WebstudioPatternResource.ActionCreateCase;
            else if (feed.Module == ASC.Feed.Constants.FilesModule)
                return WebstudioPatternResource.ActionCreateFile;
            else if (feed.Module == ASC.Feed.Constants.FoldersModule)
                return WebstudioPatternResource.ActionCreateFolder;

            return "";
        }

        private IList<string> GetBreadCrumbs(Dictionary<string, IWebItem> products, FeedMin f)
        {
            var result = new List<string>();
            if (f.Product == "projects")
            {
                if (f.Id.StartsWith("taskComment"))
                {
                    result.Add(f.AdditionalInfo2);
                }
                else if (f.Module == "projects")
                {
                    result.Add(f.Title);
                }
                else
                {
                    result.Add(f.AdditionalInfo);
                }
            }
            else if (f.Product == "community")
            {
                if (f.Module == "blogs" && products.ContainsKey("community-blogs"))
                {
                    result.Add(products["community-blogs"].Name);
                }
                else if (f.Module == "forums" && products.ContainsKey("community-forum"))
                {
                    result.Add(products["community-forum"].Name);
                }
                else if (f.Module == "bookmarks" && products.ContainsKey("community-bookmarking"))
                {
                    result.Add(products["community-bookmarking"].Name);
                }
                else if (f.Module == "events" && products.ContainsKey("community-news"))
                {
                    result.Add(products["community-news"].Name);
                }
            }
            if (result.Count == 0)
            {
                result.Add(string.Empty);
            }
            return result
                .Select(s => string.IsNullOrEmpty(s) ? "    " : s)
                .ToList();
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

        public bool IsSubscribeToAdminNotify(Guid userID)
        {
            return source.GetSubscriptionProvider().IsSubscribed(Constants.ActionAdminNotify, ToRecipient(userID), null);
        }

        public void SubscribeToAdminNotify(Guid userID, bool subscribe)
        {
            var recipient = source.GetRecipientsProvider().GetRecipient(userID.ToString());
            if (recipient != null)
            {
                if (subscribe)
                {
                    source.GetSubscriptionProvider().Subscribe(Constants.ActionAdminNotify, null, recipient);
                }
                else
                {
                    source.GetSubscriptionProvider().UnSubscribe(Constants.ActionAdminNotify, null, recipient);
                }
            }
        }


        public void SendMsgToAdminAboutProfileUpdated()
        {
            client.SendNoticeAsync(Constants.ActionSelfProfileUpdated, null, null);
        }

        public void SendMsgToAdminFromNotAuthUser(string email, string message)
        {
            client.SendNoticeAsync(Constants.ActionUserMessageToAdmin, null, null,
                new TagValue(Constants.TagBody, message), new TagValue(Constants.TagUserEmail, email));
        }

        public void SendToAdminSmsCount(int balance)
        {
            client.SendNoticeAsync(Constants.ActionSmsBalance, null, null,
                new TagValue(Constants.TagBody, balance));
        }

        public void SendToAdminVoipWarning(double balance)
        {
            client.SendNoticeAsync(Constants.ActionVoipWarning, null, null,
                new TagValue(Constants.TagBody, balance));
        }

        public void SendToAdminVoipBlocked()
        {
            client.SendNoticeAsync(Constants.ActionVoipBlocked, null, null);
        }

        public void UserPasswordChange(UserInfo userInfo)
        {
            var hash = Hasher.Base64Hash(CoreContext.Authentication.GetUserPasswordHash(userInfo.ID));
            client.SendNoticeToAsync(
                CoreContext.Configuration.Personal ? Constants.ActionPasswordChangePersonal : Constants.ActionPasswordChange,
                        null,
                        RecipientFromEmail(new[] { userInfo.Email }, false),
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName, SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)),
                        new TagValue(Constants.TagInviteLink, CommonLinkUtility.GetConfirmationUrl(userInfo.Email, ConfirmType.PasswordChange, hash)),
                        new TagValue(Constants.TagBody, string.Empty),
                        new TagValue(Constants.TagUserDisplayName, userInfo.DisplayUserName()),
                        Constants.TagSignatureStart,
                        Constants.TagSignatureEnd,
                        new TagValue(CommonTags.WithPhoto, CoreContext.Configuration.Personal ? "personal" : ""),
                        new TagValue(CommonTags.IsPromoLetter, CoreContext.Configuration.Personal ? "true" : "false"),
                        Constants.UnsubscribeLink);
        }

        public void UserPasswordChanged(Guid userID, string password)
        {
            var author = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var user = CoreContext.UserManager.GetUsers(userID);

            ISendInterceptor initInterceptor = null;
            if (!ASC.Core.Users.Constants.LostUser.Equals(author))
            {
                initInterceptor = new InitiatorInterceptor(new[] { ToRecipient(author.ID) });
                client.AddInterceptor(initInterceptor);
            }

            client.SendNoticeToAsync(
                           Constants.ActionPasswordChanged,
                           null,
                           new[] { ToRecipient(user.ID) },
                           new[] { EMailSenderName },
                           null,
                           new TagValue(Constants.TagUserName, user.DisplayUserName()),
                           new TagValue(Constants.TagUserEmail, user.Email),
                           new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                           new TagValue(Constants.TagPassword, password));

            if (initInterceptor != null)
            {
                client.RemoveInterceptor(initInterceptor.Name);
            }
        }

        public void SendUserPassword(UserInfo ui, string password)
        {
            client.SendNoticeToAsync(
                        Constants.ActionSendPassword,
                        null,
                        new[] { ToRecipient(ui.ID) },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagPassword, password),
                        new TagValue(Constants.TagUserName, ui.DisplayUserName()),
                        new TagValue(Constants.TagUserEmail, ui.Email),
                        new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                        new TagValue(Constants.TagAuthor, (HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null));
        }

        public void SendEmailChangeInstructions(UserInfo user, string email)
        {
            client.SendNoticeToAsync(
                CoreContext.Configuration.Personal ? Constants.ActionEmailChangePersonal : Constants.ActionEmailChange,
                        null,
                        RecipientFromEmail(new[] { email }, false),
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName, SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)),
                        new TagValue(Constants.TagInviteLink, CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmailChange)),
                        new TagValue(Constants.TagBody, string.Empty),
                        new TagValue(Constants.TagUserDisplayName, string.Empty),
                        Constants.TagSignatureStart,
                        Constants.TagSignatureEnd,
                        new TagValue(CommonTags.WithPhoto, CoreContext.Configuration.Personal ? "personal" : ""),
                        new TagValue(CommonTags.IsPromoLetter, CoreContext.Configuration.Personal ? "true" : "false"),
                        new TagValue(CommonTags.Culture, user.GetCulture().Name),
                        Constants.UnsubscribeLink);
        }

        public void SendEmailActivationInstructions(UserInfo user, string email)
        {
            client.SendNoticeToAsync(
                        Constants.ActionActivateEmail,
                        null,
                        RecipientFromEmail(new[] { email }, false),
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName, SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)),
                        new TagValue(Constants.TagInviteLink, CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmailActivation)),
                        new TagValue(Constants.TagBody, string.Empty),
                        new TagValue(Constants.TagUserDisplayName, (user.DisplayUserName() ?? string.Empty).Trim()),
                        new TagValue(CommonTags.WithPhoto, CoreContext.Configuration.Personal ? "personal" : "links"),
                        new TagValue(CommonTags.IsPromoLetter, CoreContext.Configuration.Personal ? "true" : "false"),
                        Constants.UnsubscribeLink);
        }

        public void SendMsgMobilePhoneChange(UserInfo userInfo)
        {
            client.SendNoticeToAsync(
                Constants.ActionPhoneChange,
                null,
                RecipientFromEmail(new[] { userInfo.Email.ToLower() }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, CommonLinkUtility.GetConfirmationUrl(userInfo.Email.ToLower(), ConfirmType.PhoneActivation)),
                new TagValue(Constants.TagUserDisplayName, userInfo.DisplayUserName()));
        }


        public void UserHasJoin()
        {
            client.SendNoticeAsync(Constants.ActionUserHasJoin, null, null);
        }

        public void InviteUsers(string emailList, string inviteMessage, bool join, EmployeeType emplType)
        {
            if (string.IsNullOrWhiteSpace(emailList))
            {
                return;
            }

            foreach (var email in emailList.Split(new[] { " ", ",", ";", Environment.NewLine, "\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                SendInvite(new UserInfo() { Email = email }, inviteMessage, join, emplType);
            }
        }

        private void SendInvite(UserInfo user, string inviteMessage, bool join, EmployeeType emplType)
        {
            var inviteUrl = CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.EmpInvite, (int)emplType, SecurityContext.CurrentAccount.ID)
                            + String.Format("&firstname={0}&lastname={1}&emplType={2}",
                                            HttpUtility.UrlEncode(user.FirstName),
                                            HttpUtility.UrlEncode(user.LastName),
                                            (int)emplType);

            client.SendNoticeToAsync(
                        join ? Constants.ActionJoinUsers : Constants.ActionInviteUsers,
                        null,
                        RecipientFromEmail(new string[] { user.Email }, join),/*if it's invite - don't check activation status*/
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName, SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)),
                        new TagValue(Constants.TagInviteLink, inviteUrl),
                        new TagValue(Constants.TagBody, inviteMessage ?? string.Empty),
                        Constants.TagTableTop(),
                        Constants.TagTableItem(1),
                        Constants.TagTableItem(2),
                        Constants.TagTableItem(3),
                        Constants.TagTableBottom(),
                        new TagValue(CommonTags.WithPhoto, "links"),
                        new TagValue(Constants.TagUserDisplayName, (user.DisplayUserName() ?? "").Trim()),
                        CreateSendFromTag());
        }

        public void UserInfoAddedAfterInvite(UserInfo newUserInfo, string password)
        {
            if (CoreContext.UserManager.UserExists(newUserInfo.ID))
            {
                client.SendNoticeToAsync(
                    CoreContext.Configuration.Personal ? Constants.ActionAfterRegistrationPersonal1 : Constants.ActionYouAddedAfterInvite,
                    null,
                    RecipientFromEmail(new[] { newUserInfo.Email }, false),
                    new[] { EMailSenderName },
                    null,
                    new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                    new TagValue(Constants.TagUserEmail, newUserInfo.Email),
                    new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                    new TagValue(Constants.TagPassword, password),
                    Constants.TagMarkerStart,
                    Constants.TagMarkerEnd,
                    Constants.TagFrameStart,
                    Constants.TagFrameEnd,
                    Constants.TagHeaderStart,
                    Constants.TagHeaderEnd,
                    Constants.TagStrongStart,
                    Constants.TagStrongEnd,
                    Constants.TagSignatureStart,
                    Constants.TagSignatureEnd,
                    new TagValue(CommonTags.WithPhoto, CoreContext.Configuration.Personal ? "personal" : ""),
                    new TagValue(CommonTags.IsPromoLetter, CoreContext.Configuration.Personal ? "true" : "false"),
                    Constants.UnsubscribeLink);
            }
        }

        public void GuestInfoAddedAfterInvite(UserInfo newUserInfo, string password)
        {
            if (CoreContext.UserManager.UserExists(newUserInfo.ID))
            {
                client.SendNoticeToAsync(
                            Constants.ActionYouAddedLikeGuest,
                            null,
                            RecipientFromEmail(new[] { newUserInfo.Email }, false),
                            new[] { EMailSenderName },
                            null,
                            new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                            new TagValue(Constants.TagUserEmail, newUserInfo.Email),
                            new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                            new TagValue(Constants.TagPassword, password));
            }
        }

        public void UserInfoActivation(UserInfo newUserInfo)
        {
            if (newUserInfo.IsActive)
            {
                throw new ArgumentException("User is already activated!");
            }

            client.SendNoticeToAsync(
                Constants.ActionActivateUsers,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email.ToLower() }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, GenerateActivationConfirmUrl(newUserInfo)),
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                new TagValue(Constants.TagBlogLink, "http://www.onlyoffice.com/blog/2013/12/teamlab-personal-christmas-gift-for-your-family-and-friends/"),
                Constants.TagTableTop(),
                Constants.TagTableItem(1),
                Constants.TagTableItem(2),
                Constants.TagTableItem(3),
                Constants.TagTableBottom(),
                Constants.TagMarkerStart,
                Constants.TagMarkerEnd,
                Constants.TagFrameStart,
                Constants.TagFrameEnd,
                Constants.TagHeaderStart,
                Constants.TagHeaderEnd,
                Constants.TagStrongStart,
                Constants.TagStrongEnd,
                Constants.TagSignatureStart,
                Constants.TagSignatureEnd,
                new TagValue(CommonTags.WithPhoto, "links"),
                new TagValue(CommonTags.IsPromoLetter, "false"),
                CreateSendFromTag());
        }

        public void GuestInfoActivation(UserInfo newUserInfo)
        {
            if (newUserInfo.IsActive)
            {
                throw new ArgumentException("User is already activated!");
            }
            client.SendNoticeToAsync(
                Constants.ActionActivateGuests,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email.ToLower() }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, GenerateActivationConfirmUrl(newUserInfo)),
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                CreateSendFromTag());
        }

        public void SendMsgProfileDeletion(string email)
        {
            client.SendNoticeToAsync(
                        Constants.ActionProfileDelete,
                        null,
                        RecipientFromEmail(new[] { email }, false),
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagInviteLink, CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.ProfileRemove)));
        }


        public void SendMsgBackupCompleted(Guid userId, string link)
        {
            client.SendNoticeToAsync(
                Constants.ActionBackupCreated,
                null,
                new[] {ToRecipient(userId)},
                new[] {EMailSenderName},
                null,
                new TagValue(Constants.TagOwnerName, CoreContext.UserManager.GetUsers(userId).DisplayUserName()));
        }

        public void SendMsgRestoreStarted(bool notifyAllUsers)
        {
            IRecipient[] users =
                notifyAllUsers
                    ? CoreContext.UserManager.GetUsers(EmployeeStatus.Active).Select(u => ToRecipient(u.ID)).ToArray()
                    : new[] { ToRecipient(CoreContext.TenantManager.GetCurrentTenant().OwnerId) };

            client.SendNoticeToAsync(
                Constants.ActionRestoreStarted,
                null,
                users,
                new[] { EMailSenderName },
                null);
        }

        public void SendMsgRestoreCompleted(bool notifyAllUsers)
        {
            IRecipient[] users =
                notifyAllUsers
                    ? CoreContext.UserManager.GetUsers(EmployeeStatus.Active).Select(u => ToRecipient(u.ID)).ToArray()
                    : new[] {ToRecipient(CoreContext.TenantManager.GetCurrentTenant().OwnerId)};

            client.SendNoticeToAsync(
                Constants.ActionRestoreCompleted,
                null,
                users,
                new[] {EMailSenderName},
                null);
        }

        public void SendMsgPortalDeactivation(Tenant t, string d_url, string a_url)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionPortalDeactivate,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagActivateUrl, a_url),
                        new TagValue(Constants.TagDeactivateUrl, d_url),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }

        public void SendMsgPortalDeletion(Tenant t, string url)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionPortalDelete,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagDeleteUrl, url),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }

        public void SendMsgPortalDeletionSuccess(Tenant t, string url)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);

            client.SendNoticeToAsync(
                        Constants.ActionPortalDeleteSuccess,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue("FeedBackUrl", url),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }

        public void SendMsgDnsChange(Tenant t, string confirmDnsUpdateUrl, string portalAddress, string portalDns)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionDnsChange,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue("ConfirmDnsUpdate", confirmDnsUpdateUrl),
                        new TagValue("PortalAddress", AddHttpToUrl(portalAddress)),
                        new TagValue("PortalDns", AddHttpToUrl(portalDns ?? string.Empty)),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }


        public void SendMsgConfirmChangeOwner(Tenant t, string newOwnerName, string confirmOwnerUpdateUrl)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionConfirmOwnerChange,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue("ConfirmPortalOwnerUpdate", confirmOwnerUpdateUrl),
                        new TagValue(Constants.TagUserName, newOwnerName),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }


        public void SendCongratulations(UserInfo u)
        {
            var tariff = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            client.SendNoticeToAsync(
                tariff != null && tariff.Trial ? Constants.ActionCongratulations : Constants.ActionCongratulationsNoTrial,
                null,
                RecipientFromEmail(new[] { u.Email.ToLower() }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagUserName, u.DisplayUserName()),
                new TagValue(Constants.TagUserEmail, u.Email),
                new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                new TagValue(Constants.TagSettingsLink, CommonLinkUtility.GetAdministration(ManagementType.General)),
                new TagValue(Constants.TagInviteLink, CommonLinkUtility.GetConfirmationUrl(u.Email, ConfirmType.EmailActivation)),
                Constants.TagNoteStart,
                Constants.TagNoteEnd,
                new TagValue(CommonTags.WithPhoto, "links"));
        }

        public void SendTariffWarnings(DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var now = scheduleDate.Date;

            log.Info("Start SendTariffWarnings.");

            var activeTenants = CoreContext.TenantManager.GetTenants().Where(t => t.Status == TenantStatus.Active).ToList();

            if (activeTenants.Count > 0)
            {
                var monthQuotas = CoreContext.TenantManager.GetTenantQuotas()
                                .Where(r => !r.Trial && r.Visible && !r.Year && !r.Free && !r.NonProfit)
                                .ToList();
                var monthQuotasIds = monthQuotas.Select(q => q.Id).ToArray();

            foreach (var tenant in activeTenants)
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);
                    var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                    var quota = CoreContext.TenantManager.GetTenantQuota(tenant.TenantId);
                    var duedate = tariff.DueDate.Date;

                    INotifyAction action = null;
                    var onlyadmins = true;
                    var footer = "links";

                    var greenButtonText = string.Empty;
                    var greenButtonUrl = string.Empty;

                    var tableItemText1 = string.Empty;
                    var tableItemText2 = string.Empty;
                    var tableItemText3 = string.Empty;
                    var tableItemText4 = string.Empty;
                    var tableItemText5 = string.Empty;

                    var tableItemUrl1 = string.Empty;
                    var tableItemUrl2 = string.Empty;
                    var tableItemUrl3 = string.Empty;
                    var tableItemUrl4 = string.Empty;
                    var tableItemUrl5 = string.Empty;

                    var tableItemImg1 = string.Empty;
                    var tableItemImg2 = string.Empty;
                    var tableItemImg3 = string.Empty;
                    var tableItemImg4 = string.Empty;
                    var tableItemImg5 = string.Empty;

                    var tableItemComment1 = string.Empty;
                    var tableItemComment2 = string.Empty;
                    var tableItemComment3 = string.Empty;
                    var tableItemComment4 = string.Empty;
                    var tableItemComment5 = string.Empty;

                    #region 3 days after registration

                    if (tenant.CreatedDateTime.Date.AddDays(3) == now)
                    {
                        action = Constants.ActionAfterCreation1;

                        tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/integrate_documents.jpg";
                        tableItemText1 = "ItemCreateWorkspaceDocs";
                        tableItemUrl1 = "http://helpcenter.onlyoffice.com/tipstricks/add-resource.aspx?utm_medium=newsletter&utm_source=after_signup_1&utm_campaign=email";

                        tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/import_projects.jpg";
                        tableItemText2 = "ItemImportProjectsBasecamp";
                        tableItemUrl2 = "http://helpcenter.onlyoffice.com/tipstricks/basecamp-import.aspx?utm_medium=newsletter&utm_source=after_signup_1&utm_campaign=email";

                        tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/csv.jpg";
                        tableItemText3 = "ItemUploadCrmContactsCsv";
                        tableItemUrl3 = "http://helpcenter.onlyoffice.com/guides/import-contacts.aspx?utm_medium=newsletter&utm_source=after_signup_1&utm_campaign=email";

                        tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/mail.jpg";
                        tableItemText4 = "ItemAddTeamlabMail";
                        tableItemUrl4 = "http://helpcenter.onlyoffice.com/gettingstarted/mail.aspx?utm_medium=newsletter&utm_source=after_signup_1&utm_campaign=email";
                    }

                    #endregion

                    #region 7 days after registration

                    if (tenant.CreatedDateTime.Date.AddDays(7) == now)
                    {
                        action = Constants.ActionAfterCreation4;

                        tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/integrate_documents.jpg";
                        tableItemText1 = "ItemAddFilesCreatWorkspace";
                        tableItemUrl1 = "http://helpcenter.onlyoffice.com/tipstricks/add-resource.aspx?utm_medium=newsletter&utm_source=after_signup_7&utm_campaign=email";

                        tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/coedit.jpg";
                        tableItemText2 = "ItemTryOnlineDocEditor";
                        tableItemUrl2 = "http://helpcenter.onlyoffice.com/guides/collaborative-editing.aspx?utm_medium=newsletter&utm_source=after_signup_7&utm_campaign=email";

                        tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/crm_customization.jpg";
                        tableItemText3 = "ItemUploadCrmContacts";
                        tableItemUrl3 = "http://helpcenter.onlyoffice.com/guides/import-contacts.aspx?utm_medium=newsletter&utm_source=after_signup_7&utm_campaign=email";

                        tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/mail.jpg";
                        tableItemText4 = "ItemAddTeamlabMail";
                        tableItemUrl4 = "http://helpcenter.onlyoffice.com/gettingstarted/mail.aspx?utm_medium=newsletter&utm_source=after_signup_7&utm_campaign=email";

                        tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/jabber.jpg";
                        tableItemText5 = "ItemIntegrateIM";
                        tableItemUrl5 = "http://helpcenter.onlyoffice.com/tipstricks/integrating-talk.aspx?utm_medium=newsletter&utm_source=after_signup_7&utm_campaign=email";
                    }

                    #endregion

                    #region 2 weeks after registration

                    if (tenant.CreatedDateTime.Date.AddDays(14) == now)
                    {
                        onlyadmins = false;
                        action = Constants.ActionAfterCreation2;

                        tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/arm.jpg";
                        tableItemText1 = "ItemFeatureARM";
                        tableItemUrl1 = "http://helpcenter.teamlab.com/video/manage-access-rights.aspx?utm_medium=newsletter&utm_source=after_signup_14&utm_campaign=email";
                        tableItemComment1 = "ItemFeatureARMText";

                        tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/coedit.jpg";
                        tableItemText2 = "ItemFeatureCoediting";
                        tableItemUrl2 = "http://www.youtube.com/watch?v=yzZLn3RBBE8?utm_medium=newsletter&utm_source=after_signup_14&utm_campaign=email";
                        tableItemComment2 = "ItemFeatureCoeditingText";

                        tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/mail.jpg";
                        tableItemText3 = "ItemFeatureMail";
                        tableItemUrl3 = "http://helpcenter.onlyoffice.com/gettingstarted/mail.aspx?utm_medium=newsletter&utm_source=after_signup_14&utm_campaign=email";
                        tableItemComment3 = "ItemFeatureMailText";

                        tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/calendars.jpg";
                        tableItemText4 = "ItemFeatureCalendar";
                        tableItemUrl4 = "http://helpcenter.onlyoffice.com/video/share-calendar.aspx?utm_medium=newsletter&utm_source=after_signup_14&utm_campaign=email";
                        tableItemComment4 = "ItemFeatureCalendarText";

                        tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/crm_customization.jpg";
                        tableItemText5 = "ItemFeatureCRM";
                        tableItemUrl5 = "http://helpcenter.onlyoffice.com/tipstricks/integrating-talk.aspx?utm_medium=newsletter&utm_source=after_signup_14&utm_campaign=email";
                        tableItemComment5 = "ItemFeatureCRMText";

                        greenButtonText = "ButtonLogInTeamlab";
                        greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/');
                    }

                    #endregion

                    #region 3 weeks after registration

                    if (tenant.CreatedDateTime.Date.AddDays(21) == now)
                    {
                        onlyadmins = false;
                        action = Constants.ActionAfterCreation3;

                        tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/online_editor.jpg";
                        tableItemText1 = "ItemUseOnlineEditor";
                        tableItemUrl1 = "http://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/index.aspx?utm_medium=newsletter&utm_source=after_signup_21&utm_campaign=email";

                        tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/coedit.jpg";
                        tableItemText2 = "ItemCreatCoeditDocs";
                        tableItemUrl2 = "http://www.youtube.com/watch?v=yzZLn3RBBE8?utm_medium=newsletter&utm_source=after_signup_21&utm_campaign=email";

                        tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/document_sharing.jpg";
                        tableItemText3 = "ItemShareDocsPeople";
                        tableItemUrl3 = "http://helpcenter.onlyoffice.com/guides/collaborative-editing.aspx?utm_medium=newsletter&utm_source=after_signup_21&utm_campaign=email";

                        tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/manage_documentation.jpg";
                        tableItemText4 = "ItemOrganazeTeamDocs";
                        tableItemUrl4 = "http://helpcenter.onlyoffice.com/administratorguides/organize-company-documentation.aspx?utm_medium=newsletter&utm_source=after_signup_21&utm_campaign=email";

                        tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/crm_attach.jpg";
                        tableItemText5 = "ItemAttachFilesCrm";
                        tableItemUrl5 = "http://helpcenter.onlyoffice.com/gettingstarted/crm.aspx?utm_medium=newsletter&utm_source=after_signup_21&utm_campaign=email";

                        greenButtonText = "ButtonLogInTeamlab";
                        greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/');
                    }

                    #endregion

                    #region 5 days before trial ends

                    if (quota.Trial && duedate.AddDays(-5) == now)
                    {
                        action = Constants.ActionTariffWarningTrial;
                        footer = "links";
                        greenButtonText = "ButtonSelectPricingPlans";
                        greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                    }

                    #endregion

                    #region trial expires today

                    if (quota.Trial && duedate == now)
                    {
                        action = Constants.ActionTariffWarningTrial2;
                        footer = "links";
                    }

                    #endregion

                    #region 5 days after trial expired

                    if (quota.Trial && duedate.AddDays(5) == now && tenant.VersionChanged <= tenant.CreatedDateTime)
                    {
                        action = Constants.ActionTariffWarningTrial3;
                        footer = "links";
                        greenButtonText = "ButtonExtendTrialButton";
                        greenButtonUrl = "mailto:sales@onlyoffice.com";
                    }

                    #endregion

                    #region 30 days after trial expired

                    if (quota.Trial && duedate.AddDays(30) == now && CoreContext.UserManager.GetUsers().Count() == 1)
                    {
                        action = Constants.ActionTariffWarningTrial4;
                        footer = "links";
                        greenButtonText = "ButtonSignUpPersonal";
                        greenButtonUrl = "https://personal.onlyoffice.com";
                    }

                    #endregion


                    var dbid = "webstudio";

                    #region 2 weeks after 3 times paid

                    try
                    {
                        if (tariff.State == TariffState.Paid)
                        {
                            var lastDatePayment = DateTime.MinValue;

                            var query = new SqlQuery("tenants_tariff")
                                .Select("max(create_on)")
                                .Where(Exp.Eq("tenant", tenant.TenantId) & Exp.In("tariff", monthQuotasIds))
                                .Having(Exp.Sql("count(*) >= 3"));

                            using (var db = new DbManager(dbid))
                            {
                                lastDatePayment = db.ExecuteScalar<DateTime>(query);
                            }

                            if (lastDatePayment != DateTime.MinValue && lastDatePayment.AddDays(14) == now){
                                action = Constants.ActionAfterPayment1;
                                footer = "photo";
                            }
                        }
                    }
                    catch (Exception e) {
                        log.Error(e);
                    }

                    #endregion



                    #region 5 days after registration without activity in 1 or more days

                    if (tenant.CreatedDateTime.Date.AddDays(5) == now)
                    {

                        var datesWithActivity = new List<DateTime>();
                        var query = new SqlQuery("feed_aggregate")
                            .Select(new SqlExp("cast(created_date as date) as short_date"))

                            .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                            .Where(Exp.Le("created_date", now.AddDays(-1)))
                            .GroupBy("short_date");

                        using (var db = new DbManager(dbid))
                        {
                            datesWithActivity = db
                                .ExecuteList(query)
                                .ConvertAll(r => Convert.ToDateTime(r[0]));
                        }

                        if (datesWithActivity.Count < 5)
                        {
                            action = Constants.ActionAfterCreation5;
                        }
                    }

                    #endregion


                    if (action != null)
                    {
                        var users = onlyadmins ? CoreContext.UserManager.GetUsersByGroup(ASC.Core.Users.Constants.GroupAdmin.ID) : CoreContext.UserManager.GetUsers();
                        foreach (var u in users)
                        {
                            var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                            Thread.CurrentThread.CurrentCulture = culture;
                            Thread.CurrentThread.CurrentUICulture = culture;
                            var rquota = TenantExtra.GetRightQuota();

                            client.SendNoticeToAsync(
                                action,
                                null,
                                new[] { ToRecipient(u.ID) },
                                new[] { EMailSenderName },
                                null,
                                new TagValue(Constants.TagUserName, u.DisplayUserName()),
                                new TagValue(Constants.TagPricingPage, CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx")),
                                new TagValue(Constants.TagFAQ, getHelpCenterLink() + "faq/pricing.aspx"),
                                new TagValue(Constants.TagActiveUsers, CoreContext.UserManager.GetUsers().Count()),
                                new TagValue(Constants.TagPrice, rquota.Price),//TODO: use price partner
                                new TagValue(Constants.TagPricePeriod, rquota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth),
                                Constants.TagBlueButton("ButtonRequestCallButton", "http://www.onlyoffice.com/call-back-form.aspx"),
                                Constants.TagGreenButton(greenButtonText, greenButtonUrl),
                                Constants.TagTableTop(),
                                Constants.TagTableItem(1, tableItemText1, tableItemUrl1, tableItemImg1, tableItemComment1),
                                Constants.TagTableItem(2, tableItemText2, tableItemUrl2, tableItemImg2, tableItemComment2),
                                Constants.TagTableItem(3, tableItemText3, tableItemUrl3, tableItemImg3, tableItemComment3),
                                Constants.TagTableItem(4, tableItemText4, tableItemUrl4, tableItemImg4, tableItemComment4),
                                Constants.TagTableItem(5, tableItemText5, tableItemUrl5, tableItemImg5, tableItemComment5),
                                Constants.TagTableBottom(),
                                new TagValue(CommonTags.WithPhoto, string.IsNullOrEmpty(tenant.PartnerId) ? footer : string.Empty));
                        }
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }
            }
            log.Info("End SendTariffWarnings.");
        }

        public void SendLettersPersonal(DateTime scheduleDate)
        {

            var log = LogManager.GetLogger("ASC.Notify");

            log.Info("Start SendLettersPersonal...");

            foreach (var tenant in CoreContext.TenantManager.GetTenants().Where(t => t.Status == TenantStatus.Active))
            {
                try
                {
                    int sendCount = 0;

                    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);

                    log.InfoFormat("Current tenant: {0}", tenant.TenantId);

                    var users = CoreContext.UserManager.GetUsers(EmployeeStatus.Active);


                    foreach (var user in users)
                    {
                        INotifyAction action;

                        SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(user.ID));

                        var culture = tenant.GetCulture();

                        if (!string.IsNullOrEmpty(user.CultureName))
                        {
                            try
                            {
                                culture = user.GetCulture();
                            }
                            catch (CultureNotFoundException exception)
                            {

                                log.Error(exception);
                            }

                        }

                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var dayAfterRegister = (int)scheduleDate.Date.Subtract(user.CreateDate.Date).TotalDays;

                        switch (dayAfterRegister)
                        {
                            case 7:
                                action = Constants.ActionAfterRegistrationPersonal7;
                                break;
                            case 14:
                                action = Constants.ActionAfterRegistrationPersonal14;
                                break;
                            case 21:
                                action = Constants.ActionAfterRegistrationPersonal21;
                                break;
                            default:
                                continue;

                        }

                        log.InfoFormat(@"Send letter personal '{1}'  to {0} culture {2}. tenant id: {3} user culture {4} create on {5} now date {6}",
                              user.Email, action.Name, culture, tenant.TenantId, user.GetCulture(), user.CreateDate, scheduleDate.Date);

                        sendCount++;

                        client.SendNoticeToAsync(
                          action,
                          null,
                          RecipientFromEmail(new[] { user.Email.ToLower() }, true),
                          new[] { EMailSenderName },
                          null,
                          Constants.TagMarkerStart,
                          Constants.TagMarkerEnd,
                          Constants.TagFrameStart,
                          Constants.TagFrameEnd,
                          Constants.TagHeaderStart,
                          Constants.TagHeaderEnd,
                          Constants.TagStrongStart,
                          Constants.TagStrongEnd,
                          Constants.TagSignatureStart,
                          Constants.TagSignatureEnd,
                          new TagValue(CommonTags.WithPhoto, "personal"),
                          new TagValue(CommonTags.IsPromoLetter, "true"));
                    }

                    log.InfoFormat("Total send count: {0}", sendCount);

                }
                catch (Exception err)
                {
                    log.Error(err);
                }




            }

            log.Info("End SendLettersPersonal.");
        }

        public void SendInvitePersonal(string email, string additionalMember = "")
        {
            var newUserInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (!CoreContext.UserManager.UserExists(newUserInfo.ID))
            {
                var confirmUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmpInvite, (int)EmployeeType.User)
                                 + "&emplType=" + (int)EmployeeType.User
                                 + "&lang=" + Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
                                 + additionalMember;

                client.SendNoticeToAsync(
                            Constants.ActionConfirmationPersonal,
                            null,
                            RecipientFromEmail(new string[] { email }, false),
                            new[] { EMailSenderName },
                            null,
                            new TagValue(Constants.TagInviteLink, confirmUrl),
                            Constants.TagSignatureStart,
                            Constants.TagSignatureEnd,
                            new TagValue(CommonTags.WithPhoto, "personal"),
                            new TagValue(CommonTags.IsPromoLetter, "true"),
                            Constants.UnsubscribeLink,
                            new TagValue(CommonTags.Culture, Thread.CurrentThread.CurrentUICulture.Name));
            }
        }

        public void SendUserWelcomePersonal(UserInfo newUserInfo)
        {
            client.SendNoticeToAsync(
                Constants.ActionAfterRegistrationPersonal1,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email.ToLower() }, true),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, GenerateActivationConfirmUrl(newUserInfo)),
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                new TagValue(Constants.TagBlogLink, "http://www.onlyoffice.com/blog/2013/12/teamlab-personal-christmas-gift-for-your-family-and-friends/"),
                Constants.TagTableTop(),
                Constants.TagTableItem(1),
                Constants.TagTableItem(2),
                Constants.TagTableItem(3),
                Constants.TagTableBottom(),
                Constants.TagMarkerStart,
                Constants.TagMarkerEnd,
                Constants.TagFrameStart,
                Constants.TagFrameEnd,
                Constants.TagHeaderStart,
                Constants.TagHeaderEnd,
                Constants.TagStrongStart,
                Constants.TagStrongEnd,
                Constants.TagSignatureStart,
                Constants.TagSignatureEnd,
                new TagValue(CommonTags.WithPhoto, "personal"),
                new TagValue(CommonTags.IsPromoLetter, "true"),
                Constants.UnsubscribeLink,
                CreateSendFromTag());
        }

        public void MigrationPortalStart(string region, bool notify)
        {
            MigrationNotify(Constants.ActionMigrationPortalStart, region, string.Empty, notify);
        }

        public void MigrationPortalSuccess(string region, string url, bool notify)
        {
            MigrationNotify(Constants.ActionMigrationPortalSuccess, region, url, notify);
        }

        public void MigrationPortalError(string region, string url, bool notify)
        {
            MigrationNotify(!string.IsNullOrEmpty(region) ? Constants.ActionMigrationPortalError : Constants.ActionMigrationPortalServerFailure, region, url, notify);
        }

        private void MigrationNotify(INotifyAction action, string region, string url, bool notify)
        {
            var users = CoreContext.UserManager.GetUsers()
                .Where(u => notify ? u.ActivationStatus == EmployeeActivationStatus.Activated : u.IsOwner())
                .Select(u => ToRecipient(u.ID));

            if (users.Any())
            {
                client.SendNoticeToAsync(
                    action,
                    null,
                    users.ToArray(),
                    new[] { EMailSenderName },
                    null,
                    new TagValue(Constants.TagRegionName, TransferResourceHelper.GetRegionDescription(region)),
                    new TagValue(Constants.TagPortalUrl, url));
            }
        }


        #region Helpers

        private IRecipient ToRecipient(Guid userID)
        {
            return source.GetRecipientsProvider().GetRecipient(userID.ToString());
        }

        private IDirectRecipient[] RecipientFromEmail(string[] emails, bool checkActivation)
        {
            return (emails ?? new string[0])
                .Select(e => new DirectRecipient(e, null, new[] { e }, checkActivation))
                .ToArray();
        }

        private static TagValue CreateSendFromTag()
        {
            return new TagValue(CommonTags.SendFrom,
                SecurityContext.IsAuthenticated && SecurityContext.CurrentAccount is IUserAccount ?
                    DisplayUserSettings.GetFullUserName(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID), false).Replace(">", "&#62").Replace("<", "&#60") :
                    CoreContext.TenantManager.GetCurrentTenant().Name);
        }

        private string GetMyStaffLink()
        {
            return CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetMyStaff());
        }

        private string AddHttpToUrl(string url)
        {
            var httpPrefix = Uri.UriSchemeHttp + Uri.SchemeDelimiter;
            return !string.IsNullOrEmpty(url) && !url.StartsWith(httpPrefix) ? httpPrefix + url : url;
        }

        private static string GenerateActivationConfirmUrl(UserInfo user)
        {
            var confirmUrl = CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.Activation);

            return confirmUrl + String.Format("&uid={0}&firstname={1}&lastname={2}",
                                              SecurityContext.CurrentAccount.ID,
                                              HttpUtility.UrlEncode(user.FirstName),
                                              HttpUtility.UrlEncode(user.LastName));
        }

        private string getHelpCenterLink()
        {
            var url = "http://helpcenter.onlyoffice.com/{ru|de|fr|es|lv|it}";

            if (string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
            {
                if (url.Contains("{"))
                {
                    var parts = url.Split('{');
                    url = parts[0];
                    if (parts[1].Contains(CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
                    {
                        url += CultureInfo.CurrentCulture.TwoLetterISOLanguageName + "/";
                    }
                }
            }
            else
            {
                url = CommonLinkUtility.GetHelpLink();
            }
            return url;
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