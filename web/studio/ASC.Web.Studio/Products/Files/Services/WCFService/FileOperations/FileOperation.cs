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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using ASC.Common.Logging;
using ASC.Common.Security.Authorizing;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Utils;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    abstract class FileOperation
    {
        public const string SPLIT_CHAR = ":";
        public const string OWNER = "Owner";
        public const string OPERATION_TYPE = "OperationType";
        public const string SOURCE = "Source";
        public const string PROGRESS = "Progress";
        public const string RESULT = "Result";
        public const string ERROR = "Error";
        public const string PROCESSED = "Processed";
        public const string FINISHED = "Finished";
        public const string HOLD = "Hold";

        public static Guid CurrentUserId
        {
            get
            {
                if (SecurityContext.IsAuthenticated)
                {
                    return SecurityContext.CurrentAccount.ID;
                }

                if (FileShareLink.TryGetSessionId(out var id))
                {
                    return id;
                }

                return default;
            }
        }

        private readonly IPrincipal principal;
        private readonly string culture;
        private int processed;
        private int successProcessed;
        private string contextUrl;


        public int Total { get; set; }

        protected DistributedTask TaskInfo { get; private set; }

        protected string Status { get; set; }

        protected string Error { get; set; }

        protected Tenant CurrentTenant { get; private set; }

        protected FileSecurity FilesSecurity { get; private set; }

        protected IFolderDao FolderDao { get; private set; }

        protected IFileDao FileDao { get; private set; }

        protected ITagDao TagDao { get; private set; }

        protected ILinkDao LinkDao { get; private set; }

        protected IProviderDao ProviderDao { get; private set; }

        protected ILog Logger { get; private set; }

        protected CancellationToken CancellationToken { get; private set; }

        protected List<object> Folders { get; private set; }

        protected List<object> Files { get; private set; }

        protected bool HoldResult { get; private set; }

        protected IEnumerable<HttpCookie> Cookies { get; private set; }

        public abstract FileOperationType OperationType { get; }
        public Guid LinkId { get; }
        public Guid SessionId { get; }
        public string PasswordKey { get; }


        protected FileOperation(List<object> folders, List<object> files, bool holdResult = true, Tenant tenant = null, IEnumerable<HttpCookie> cookies = null)
        {
            CurrentTenant = tenant ?? CoreContext.TenantManager.GetCurrentTenant();
            principal = Thread.CurrentPrincipal;
            culture = Thread.CurrentThread.CurrentCulture.Name;
            Cookies = cookies ?? new List<HttpCookie>();

            Folders = folders ?? new List<object>();
            Files = files ?? new List<object>();

            HoldResult = holdResult;

            TaskInfo = new DistributedTask();
            contextUrl = HttpContext.Current != null ? HttpContext.Current.Request.GetUrlRewriter().ToString() : null;
            
            LinkId = FileShareLink.TryGetCurrentLinkId(out var linkId) ? linkId : default;
            SessionId = FileShareLink.TryGetSessionId(out var sessionId) ? sessionId : default;
            PasswordKey = FileShareLink.GetPasswordKey(linkId);
        }

        public void RunJob(DistributedTask _, CancellationToken cancellationToken)
        {
            try
            {
                if (HttpContext.Current == null && !WorkContext.IsMono && !string.IsNullOrEmpty(contextUrl))
                {
                    HttpContext.Current = new HttpContext(
                        new HttpRequest("hack", contextUrl, string.Empty),
                        new HttpResponse(new StringWriter()));
                }
                
                FileShareLink.SetCurrentLinkData(LinkId, SessionId, PasswordKey);

                CancellationToken = cancellationToken;

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);
                Thread.CurrentPrincipal = principal;
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

                FolderDao = Global.DaoFactory.GetFolderDao();
                FileDao = Global.DaoFactory.GetFileDao();
                TagDao = Global.DaoFactory.GetTagDao();
                ProviderDao = Global.DaoFactory.GetProviderDao();
                FilesSecurity = new FileSecurity(Global.DaoFactory);
                LinkDao = Global.GetLinkDao();

                Logger = Global.Logger;

                Total = InitTotalProgressSteps();

                Do();
            }
            catch (AuthorizingException authError)
            {
                Error = FilesCommonResource.ErrorMassage_SecurityException;
                Logger.Error(Error, new SecurityException(Error, authError));
            }
            catch (AggregateException ae)
            {
                ae.Flatten().Handle(e => e is TaskCanceledException || e is OperationCanceledException);
            }
            catch (Exception error)
            {
                Error = error is TaskCanceledException || error is OperationCanceledException
                            ? FilesCommonResource.ErrorMassage_OperationCanceledException
                            : error.Message;
                Logger.Error(error, error);
            }
            finally
            {
                try
                {
                    TaskInfo.SetProperty(FINISHED, true);
                    PublishTaskInfo();

                    FolderDao.Dispose();
                    FileDao.Dispose();
                    TagDao.Dispose();
                    LinkDao.Dispose();

                    if (ProviderDao != null)
                        ProviderDao.Dispose();
                }
                catch { /* ignore */ }
            }
        }

        public virtual DistributedTask GetDistributedTask()
        {
            FillDistributedTask();
            return TaskInfo;
        }

        protected virtual void FillDistributedTask()
        {
            var progress = Total != 0 ? 100 * processed / Total : 0;

            TaskInfo.SetProperty(SOURCE, string.Join(SPLIT_CHAR, Folders.Select(f => "folder_" + f).Concat(Files.Select(f => "file_" + f)).ToArray()));
            TaskInfo.SetProperty(OPERATION_TYPE, OperationType);
            TaskInfo.SetProperty(OWNER, CurrentUserId);
            TaskInfo.SetProperty(PROGRESS, progress < 100 ? progress : 100);
            TaskInfo.SetProperty(RESULT, Status);
            TaskInfo.SetProperty(ERROR, Error);
            TaskInfo.SetProperty(PROCESSED, successProcessed);
            TaskInfo.SetProperty(HOLD, HoldResult);
        }

        protected virtual int InitTotalProgressSteps()
        {
            var count = Files.Count;
            Folders.ForEach(f => count += 1 + (FolderDao.CanCalculateSubitems(f) ? FolderDao.GetItemsCount(f) : 0));
            return count;
        }

        protected void ProgressStep(object folderId = null, object fileId = null)
        {
            if (folderId == null && fileId == null
                || folderId != null && Folders.Contains(folderId)
                || fileId != null && Files.Contains(fileId))
            {
                processed++;
                PublishTaskInfo();
            }
        }

        protected bool ProcessedFolder(object folderId)
        {
            successProcessed++;
            if (Folders.Contains(folderId))
            {
                Status += string.Format("folder_{0}{1}", folderId, SPLIT_CHAR);
                return true;
            }
            return false;
        }

        protected bool ProcessedFile(object fileId)
        {
            successProcessed++;
            if (Files.Contains(fileId))
            {
                Status += string.Format("file_{0}{1}", fileId, SPLIT_CHAR);
                return true;
            }
            return false;
        }

        protected void PublishTaskInfo()
        {
            FillDistributedTask();
            TaskInfo.PublishChanges();
        }

        protected abstract void Do();
    }
}