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

using ASC.Common.Module;
using ASC.Core;
using ASC.FullTextIndex.Service.Config;
using log4net;
using System;
using System.Configuration;
using System.ServiceModel;

namespace ASC.FullTextIndex
{
    public static class FullTextSearch
    {
        public static readonly string BlogsModule = "Blogs";
        public static readonly string NewsModule = "News";
        public static readonly string PhotosModule = "Photos";
        public static readonly string BookmarksModule = "Bookmarks";
        public static readonly string WikiModule = "Wiki";
        public static readonly string ForumModule = "Forum";
        public static readonly string ProjectsModule = "Projects";
        public static readonly string UserEmailsModule = "UserEmails";
        public static readonly string FileModule = "Files";
        public static readonly string MailModule = "Mail";
        public static readonly string MailContactsModule = "Mail.Contacts";
        public static readonly string MailFromTextModule = "Mail.FromText";
        public static readonly string CRMTasksModule = "CRM.Tasks";
        public static readonly string CRMDealsModule = "CRM.Deals";
        public static readonly string CRMContactsModule = "CRM.Contacts";
        public static readonly string CRMCasesModule = "CRM.Cases";
        public static readonly string CRMEmailsModule = "CRM.Emails";
        public static readonly string CRMEventsModule = "CRM.Events";
        public static readonly string CRMInvoicesModule = "CRM.Invoices";

        public static readonly string[] AllModules = new[] 
        {
            BlogsModule, 
            NewsModule, 
            PhotosModule, 
            BookmarksModule, 
            ForumModule, 
            ProjectsModule,
            UserEmailsModule,
            WikiModule,
            FileModule,
            MailModule,
            MailContactsModule,
            MailFromTextModule,
            CRMTasksModule,
            CRMDealsModule,
            CRMContactsModule,
            CRMCasesModule,
            CRMEmailsModule,
            CRMEventsModule,
        };

        private static readonly ILog log = LogManager.GetLogger(typeof(FullTextSearch));

        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(1);

        private static DateTime lastErrorTime = default(DateTime);

        private static bool IsServiceProbablyNotAvailable()
        {
            var disabled = ConfigurationManager.AppSettings["fullTextSearch"] == "false";
            return disabled || (lastErrorTime != default(DateTime) && lastErrorTime + timeout > DateTime.Now);
        }


        public static bool SupportModule(params string[] modules)
        {
            if (modules == null || modules.Length == 0 || IsServiceProbablyNotAvailable()) return false;

            try
            {
                using (var service = new TextIndexServiceClient())
                {
                    try
                    {
                        return service.SupportModule(modules);
                    }
                    catch (FaultException fe)
                    {
                        LogError(fe);
                    }
                    catch (CommunicationException ce)
                    {
                        LogError(ce);
                        lastErrorTime = DateTime.Now;
                    }
                    catch (TimeoutException te)
                    {
                        LogError(te);
                        lastErrorTime = DateTime.Now;
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
                lastErrorTime = DateTime.Now;
            }

            return false;
        }

        public static TextSearchResult Search(string query, string module)
        {
            log.DebugFormat("TextSearchResult.Search. query = {0}; module = {1}; TenantId = {2};", query, module, CoreContext.TenantManager.GetCurrentTenant().TenantId);
            return Search(query, module, CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        public static TextSearchResult Search(string query, string module, int tenantId)
        {
            var result = new TextSearchResult();

            if (IsServiceProbablyNotAvailable() || string.IsNullOrEmpty(query))
            {
                return result;
            }
            if (TextIndexCfg.MaxQueryLength < query.Length)
            {
                query = query.Substring(0, TextIndexCfg.MaxQueryLength);
            }
            query = query.Replace(":", "\\:");

            using (var service = new TextIndexServiceClient())
            {
                try
                {
                    var ids = service.Search(tenantId, query, module);
                    foreach (var id in ids)
                    {
                        result.AddIdentifier(id);
                    }
                    return result;
                }
                catch (FaultException fe)
                {
                    LogError(fe);
                }
                catch (CommunicationException ce)
                {
                    LogError(ce);
                    lastErrorTime = DateTime.Now;
                }
                catch (TimeoutException te)
                {
                    LogError(te);
                    lastErrorTime = DateTime.Now;
                }
            }
            return result;
        }

        private static void LogError(Exception error)
        {
            log.Error(error);
        }


        private class TextIndexServiceClient : BaseWcfClient<ITextIndexService>, ITextIndexService
        {
            public bool SupportModule(string[] modules)
            {
                return Channel.SupportModule(modules);
            }

            public string[] Search(int tenant, string query, string module)
            {
                return Channel.Search(tenant, query, module);
            }
        }
    }
}
