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


using System;
using System.Globalization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Logging;
using ASC.Common.Security.Authentication;
using ASC.Common.Security.Authorizing;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Mail.Core.Engine.Operations.Base
{
    public abstract class MailOperation
    {
        public const string TENANT = "MailOperationOwnerTenant";
        public const string OWNER = "MailOperationOwnerID";
        public const string OPERATION_TYPE = "MailOperationType";
        public const string SOURCE = "MailOperationSource";
        public const string PROGRESS = "MailOperationProgress";
        public const string STATUS = "MailOperationResult";
        public const string ERROR = "MailOperationError";
        public const string FINISHED = "MailOperationFinished";

        private readonly string _culture;

        protected DistributedTask TaskInfo { get; private set; }

        protected int Progress { get; private set; }

        protected string Source { get; private set; }

        protected string Status { get; set; }

        protected string Error { get; set; }

        protected Tenant CurrentTenant { get; private set; }

        protected IAccount CurrentUser { get; private set; }

        protected ILog Logger { get; private set; }

        protected CancellationToken CancellationToken { get; private set; }

        public abstract MailOperationType OperationType { get; }

        protected MailOperation(Tenant tenant, IAccount user)
        {
            CurrentTenant = tenant ?? CoreContext.TenantManager.GetCurrentTenant();
            CurrentUser = user ?? SecurityContext.CurrentAccount;

            _culture = Thread.CurrentThread.CurrentCulture.Name;

            Source = "";
            Progress = 0;
            Status = "";
            Error = "";
            Source = "";

            TaskInfo = new DistributedTask();
        }

        public void RunJob(DistributedTask _, CancellationToken cancellationToken)
        {
            try
            {
                CancellationToken = cancellationToken;

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(_culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(_culture);

                Logger = LogManager.GetLogger("ASC.Mail.Operation");

                Do();
            }
            catch (AuthorizingException authError)
            {
                Error = "ErrorAccessDenied";
                Logger.Error(Error, new SecurityException(Error, authError));
            }
            catch (AggregateException ae)
            {
                ae.Flatten().Handle(e => e is TaskCanceledException || e is OperationCanceledException);
            }
            catch (TenantQuotaException e)
            {
                Error = "TenantQuotaSettled";
                Logger.Error("TenantQuotaException. {0}", e);
            }
            catch (FormatException e)
            {
                Error = "CantCreateUsers";
                Logger.Error("FormatException error. {0}", e);
            }
            catch (Exception e)
            {
                Error = "InternalServerError";
                Logger.Error("Internal server error. {0}", e);
            }
            finally
            {
                try
                {
                    TaskInfo.SetProperty(FINISHED, true);
                    PublishTaskInfo();
                }
                catch
                {
                    /* ignore */
                }
            }
        }

        public virtual DistributedTask GetDistributedTask()
        {
            FillDistributedTask();
            return TaskInfo;
        }

        protected virtual void FillDistributedTask()
        {
            TaskInfo.SetProperty(SOURCE, Source);
            TaskInfo.SetProperty(OPERATION_TYPE, OperationType);
            TaskInfo.SetProperty(TENANT, CurrentTenant.TenantId);
            TaskInfo.SetProperty(OWNER, CurrentUser.ID.ToString());
            TaskInfo.SetProperty(PROGRESS, Progress < 100 ? Progress : 100);
            TaskInfo.SetProperty(STATUS, Status);
            TaskInfo.SetProperty(ERROR, Error);
        }

        protected int GetProgress()
        {
            return Progress;
        }

        public void SetSource(string source)
        {
            Source = source;
        }

        public void SetProgress(int? currentPercent = null, string currentStatus = null, string currentSource = null)
        {
            if (!currentPercent.HasValue && currentStatus == null && currentSource == null)
                return;

            if (currentPercent.HasValue)
                Progress = currentPercent.Value;

            if (currentStatus != null)
                Status = currentStatus;

            if (currentSource != null)
                Source = currentSource;

            PublishTaskInfo();
        }

        protected void PublishTaskInfo()
        {
            FillDistributedTask();
            TaskInfo.PublishChanges();
        }

        protected abstract void Do();
    }
}
