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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Utility;
using ASC.Common.Threading;

namespace ASC.Web.Files.Services.DocumentService
{
    public enum ReportOrigin
    {
        CRM,
        Projects,
        ProjectsAuto,
    }

    public enum ReportStatus
    {
        Queued,
        Started,
        Done,
        Failed
    }

    public class ReportState
    {
        public string Id { get; set; } 
        public string FileName { get; set; } 
        public int FileId { get; set; }
        public int ReportType { get; set; }

        public string Exception { get; set; }
        public ReportStatus Status { get; set; }
        public ReportOrigin Origin { get; set; }

        internal string BuilderKey { get; set; }
        internal string Script { get; set; }
        internal string TmpFileName { get; set; }
        internal Action<ReportState, string> SaveFileAction { get; set; }

        internal int TenantId { get; set; }
        internal Guid UserId { get; set; }
        internal string ContextUrl { get; set; }

        public object Obj { get; set; }

        protected DistributedTask TaskInfo { get; private set; }

        public ReportState(string fileName, string tmpFileName, string script, int reportType, ReportOrigin origin, Action<ReportState, string> saveFileAction, object obj)
        {
            Id = DocbuilderReportsUtility.GetCacheKey(origin);
            Origin = origin;
            FileName = fileName;
            TmpFileName = tmpFileName;
            Script = script;
            ReportType = reportType;
            SaveFileAction = saveFileAction;
            TaskInfo = new DistributedTask();
            TenantId = TenantProvider.CurrentTenantID;
            UserId = SecurityContext.CurrentAccount.ID;
            ContextUrl = HttpContext.Current != null ? HttpContext.Current.Request.GetUrlRewriter().ToString() : null;
            Obj = obj;
        }

        public static ReportState FromTask(DistributedTask task)
        {
            return new ReportState(
                task.GetProperty<string>("fileName"),
                task.GetProperty<string>("tmpFileName"),
                task.GetProperty<string>("script"),
                task.GetProperty<int>("reportType"),
                task.GetProperty<ReportOrigin>("reportOrigin"),
                null,
                null)
            {
                Id =  task.GetProperty<string>("id"),
                FileId = task.GetProperty<int>("fileId"),
                Status = task.GetProperty<ReportStatus>("status"),
                Exception = task.GetProperty<string>("exception")
            };
        }

        public void GenerateReport(DistributedTask task, CancellationToken cancellationToken)
        {
            try
            {
                Status = ReportStatus.Started;
                PublishTaskInfo();

                if (HttpContext.Current == null && !WorkContext.IsMono && !string.IsNullOrEmpty(ContextUrl))
                {
                    HttpContext.Current = new HttpContext(
                        new HttpRequest("hack", ContextUrl, string.Empty),
                        new HttpResponse(new System.IO.StringWriter()));
                }

                CoreContext.TenantManager.SetCurrentTenant(TenantId);
                SecurityContext.AuthenticateMe(UserId);

                Dictionary<string, string> urls;
                BuilderKey = DocumentServiceConnector.DocbuilderRequest(null, Script, true, out urls);

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }

                    Task.Delay(1500, cancellationToken).Wait(cancellationToken);
                    var builderKey = DocumentServiceConnector.DocbuilderRequest(BuilderKey, null, true, out urls);
                    if (builderKey == null)
                        throw new NullReferenceException();

                    if (urls != null && !urls.Any()) throw new Exception("Empty response");

                    if (urls != null && urls.ContainsKey(TmpFileName))
                        break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                SaveFileAction(this, urls[TmpFileName]);

                Status = ReportStatus.Done;
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocbuilderReportsUtility error", e);
                Exception = e.Message;
                Status = ReportStatus.Failed;
            }

            PublishTaskInfo();
        }

        public DistributedTask GetDistributedTask()
        {
            FillDistributedTask();
            return TaskInfo;
        }

        protected void PublishTaskInfo()
        {
            var tries = 3;
            while (tries -- > 0)
            {
                try
                {
                    FillDistributedTask();
                    TaskInfo.PublishChanges();
                    return;
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("ASC").Error(" PublishTaskInfo DocbuilderReportsUtility", e);
                    if (tries == 0) throw;
                }
            }
        }

        protected void FillDistributedTask()
        {
            TaskInfo.SetProperty("id", Id);
            TaskInfo.SetProperty("fileName", FileName);
            TaskInfo.SetProperty("tmpFileName", TmpFileName);
            TaskInfo.SetProperty("reportType", ReportType);
            TaskInfo.SetProperty("fileId", FileId);
            TaskInfo.SetProperty("status", Status);
            TaskInfo.SetProperty("reportOrigin", Origin);
            TaskInfo.SetProperty("exception", Exception);
        }
    }

    public static class DocbuilderReportsUtility
    {
        private static readonly DistributedTaskQueue tasks;
        private static readonly object Locker;

        public static string TmpFileName
        {
            get
            {
                return string.Format("tmp{0}.xlsx", DateTime.UtcNow.Ticks);
            }
        }

        static DocbuilderReportsUtility()
        {
            tasks = new DistributedTaskQueue("DocbuilderReportsUtility", 10);
            Locker = new object();
        }

        public static void Enqueue(ReportState state)
        {
            lock (Locker)
            {
                tasks.QueueTask(state.GenerateReport, state.GetDistributedTask());
            }
        }

        public static void Terminate(ReportOrigin origin)
        {
            lock (Locker)
            {
                var result = tasks.GetTasks().Where(Predicate(origin));

                foreach (var t in result)
                {
                    tasks.CancelTask(t.Id);
                }
            }
        }

        public static ReportState Status(ReportOrigin origin)
        {
            lock (Locker)
            {
                var task = tasks.GetTasks().LastOrDefault(Predicate(origin));
                if (task == null) return null;
                
                var result = ReportState.FromTask(task);
                if ((int) result.Status > 1)
                {
                    tasks.RemoveTask(task.Id);
                }

                return result;
            }
        }

        private static Func<DistributedTask, bool> Predicate(ReportOrigin origin)
        {
            return t => t.GetProperty<string>("id") == GetCacheKey(origin);
        }

        internal static string GetCacheKey(ReportOrigin origin)
        {
            return string.Format("{0}_{1}_{2}", TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, (int)origin);
        }
    }
}
