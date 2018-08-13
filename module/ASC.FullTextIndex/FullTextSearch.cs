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


using ASC.Common.Caching;
using ASC.Common.Module;
using ASC.Core;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;

namespace ASC.FullTextIndex
{
    public static class FullTextSearch
    {
        private static readonly ICache cache = AscCache.Memory;

        public static ModuleInfo BlogsModule { get { return new ModuleInfo("community_blogs"); } }
        public static ModuleInfo NewsModule { get { return new ModuleInfo("community_news"); } }
        public static ModuleInfo BookmarksModule { get { return new ModuleInfo("community_bookmarks").Select("BookmarkID"); } }
        public static ModuleInfo WikiModule { get { return new ModuleInfo("community_wiki"); } }
        public static ModuleInfo ForumModule { get { return new ModuleInfo("community_topic"); } }
        public static ModuleInfo PostModule { get { return new ModuleInfo("community_post"); } }

        public static ModuleInfo ProjectsModule { get { return new ModuleInfo("projects_projects"); } }
        public static ModuleInfo ProjectsTasksModule { get { return new ModuleInfo("projects_tasks"); } }
        public static ModuleInfo ProjectsMilestonesModule { get { return new ModuleInfo("projects_milestones"); } }
        public static ModuleInfo ProjectsMessagesModule { get { return new ModuleInfo("projects_messages"); } }
        public static ModuleInfo ProjectsCommentsModule { get { return new ModuleInfo("projects_comments"); } }
        public static ModuleInfo ProjectsSubtasksModule { get { return new ModuleInfo("projects_subtasks"); } }

        public static ModuleInfo FileModule { get { return new ModuleInfo("files_file"); } }
        public static ModuleInfo FileFolderModule { get { return new ModuleInfo("files_folder"); } }

        public static ModuleInfo UserEmailsModule { get { return new ModuleInfo("UserEmails"); } }

        public static ModuleInfo MailModule
        {
            get
            {
                return new ModuleInfo("mail_mail").AddAttribute("user_id", SecurityContext.CurrentAccount.ID.ToString());
            }
        }

        public static ModuleInfo MailContactsModule { get { return new ModuleInfo("mail_contacts"); } }

        public static ModuleInfo CRMContactsModule { get { return new ModuleInfo("crm_contacts"); } }
        public static ModuleInfo CRMContactsInfoModule { get { return new ModuleInfo("crm_info"); } }
        public static ModuleInfo CRMCustomModule { get { return new ModuleInfo("crm_field"); } }
        public static ModuleInfo CRMDealsModule { get { return new ModuleInfo("crm_deal"); } }
        public static ModuleInfo CRMTasksModule { get { return new ModuleInfo("crm_task"); } }
        public static ModuleInfo CRMCasesModule { get { return new ModuleInfo("crm_cases"); } }
        public static ModuleInfo CRMEmailsModule { get { return new ModuleInfo("crm_email"); } }
        public static ModuleInfo CRMEventsModule { get { return new ModuleInfo("crm_events"); } }
        public static ModuleInfo CRMInvoicesModule { get { return new ModuleInfo("crm_invoices"); } }


        private static readonly ILog log = LogManager.GetLogger(typeof(FullTextSearch));

        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(1);

        private static DateTime lastErrorTime = default(DateTime);


        public static bool SupportModule(params ModuleInfo[] modules)
        {
            if (modules == null || modules.Length == 0 || CheckServiceAvailability())
            {
                return false;
            }

            var names = modules.Select(m => m.Name).ToArray();
            var key = string.Join("", names);
            var result = cache.Get<string>(key);
            if (result != null)
            {
                return bool.Parse(result);
            }

            try
            {
                using (var service = new TextIndexServiceClient())
                {
                    var support = service.SupportModule(names);
                    cache.Insert(key, support.ToString(), DateTime.Now.AddHours(1));
                    return support;
                }
            }
            catch (Exception e)
            {
                if (e is CommunicationException || e is TimeoutException)
                {
                    lastErrorTime = DateTime.Now;
                }
                log.Error(e);
            }
            return false;
        }

        public static List<int> Search(params ModuleInfo[] modules)
        {
            if (CheckServiceAvailability())
            {
                return new List<int>();
            }

            try
            {
                var info = modules.Select(m => m.Name + "|" + m.GetSqlQuery()).ToArray();

                using (var service = new TextIndexServiceClient())
                {
                    var result = service.Search(CoreContext.TenantManager.GetCurrentTenant().TenantId, info);
                    return result.ToList();
                }
            }
            catch (Exception e)
            {
                if (e is CommunicationException || e is TimeoutException)
                {
                    lastErrorTime = DateTime.Now;
                }
                log.Error(e);
            }

            return new List<int>();
        }

        public static bool CheckState()
        {
            if (CheckServiceAvailability()) return false;

            try
            {
                using (var service = new TextIndexServiceClient())
                {
                    return service.CheckState();
                }
            }
            catch (Exception e)
            {
                if (e is CommunicationException || e is TimeoutException)
                {
                    lastErrorTime = DateTime.Now;
                }
                log.Error(e);
            }

            return false;
        }

        private static bool CheckServiceAvailability()
        {
            var disabled = ConfigurationManager.AppSettings["fullTextSearch"] == "false";
            return disabled || (lastErrorTime != default(DateTime) && lastErrorTime + timeout > DateTime.Now);
        }
    }

    public class TextIndexServiceClient : BaseWcfClient<ITextIndexService>, ITextIndexService
    {
        public bool SupportModule(string[] modules)
        {
            return Channel.SupportModule(modules);
        }

        public int[] Search(int tenantId, string[] modules)
        {
            return Channel.Search(tenantId, modules);
        }

        public bool CheckState()
        {
            return Channel.CheckState();
        }
    }
}
