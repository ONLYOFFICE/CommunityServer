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


using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

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

        private readonly IPrincipal principal;
        private readonly string culture;
        private int total;
        private int processed;
        private int successProcessed;


        protected DistributedTask TaskInfo { get; private set; }

        protected string Status { get; set; }

        protected string Error { get; set; }

        protected Tenant CurrentTenant { get; private set; }

        protected FileSecurity FilesSecurity { get; private set; }

        protected IFolderDao FolderDao { get; private set; }

        protected IFileDao FileDao { get; private set; }

        protected ITagDao TagDao { get; private set; }

        protected IProviderDao ProviderDao { get; private set; }

        protected ILog Logger { get; private set; }

        protected CancellationToken CancellationToken { get; private set; }

        protected List<object> Folders { get; private set; }

        protected List<object> Files { get; private set; }

        protected bool HoldResult { get; private set; }

        public abstract FileOperationType OperationType { get; }


        protected FileOperation(List<object> folders, List<object> files, bool holdResult = true, Tenant tenant = null)
        {
            CurrentTenant = tenant ?? CoreContext.TenantManager.GetCurrentTenant();
            principal = Thread.CurrentPrincipal;
            culture = Thread.CurrentThread.CurrentCulture.Name;

            Folders = folders ?? new List<object>();
            Files = files ?? new List<object>();

            HoldResult = holdResult;

            TaskInfo = new DistributedTask();
        }

        public void RunJob(DistributedTask _, CancellationToken cancellationToken)
        {
            try
            {
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

                Logger = Global.Logger;

                total = InitTotalProgressSteps();

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
                Error = error.Message;
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
            var progress = total != 0 ? 100 * processed / total : 0;

            TaskInfo.SetProperty(SOURCE, string.Join(SPLIT_CHAR, Folders.Select(f => "folder_" + f).Concat(Files.Select(f => "file_" + f)).ToArray()));
            TaskInfo.SetProperty(OPERATION_TYPE, OperationType);
            TaskInfo.SetProperty(OWNER, ((IAccount)Thread.CurrentPrincipal.Identity).ID);
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