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
using System.Web;
using ASC.Common.Logging;
using ASC.CRM.Core;
using ASC.Common.Threading.Progress;
using ASC.Common.Web;
using ASC.Core;

namespace ASC.Web.CRM.Classes
{
    public class PdfQueueWorker
    {
        private static readonly ProgressQueue Queue = new ProgressQueue(1, TimeSpan.FromMinutes(5), true);

        public static string GetTaskId(int tenantId, int invoiceId)
        {
            return string.Format("{0}_{1}", tenantId, invoiceId);
        }

        public static PdfProgressItem GetTaskStatus(int tenantId, int invoiceId)
        {
            var id = GetTaskId(tenantId, invoiceId);
            return Queue.GetStatus(id) as PdfProgressItem;
        }

        public static void TerminateTask(int tenantId, int invoiceId)
        {
            var item = GetTaskStatus(tenantId, invoiceId);

            if (item != null)
                Queue.Remove(item);
        }

        public static PdfProgressItem StartTask(HttpContext context, int tenantId, Guid userId, int invoiceId)
        {
            lock (Queue.SynchRoot)
            {
                var task = GetTaskStatus(tenantId, invoiceId);

                if (task != null && task.IsCompleted)
                {
                    Queue.Remove(task);
                    task = null;
                }

                if (task == null)
                {
                    task = new PdfProgressItem(context, tenantId, userId, invoiceId);
                    Queue.Add(task);
                }

                if (!Queue.IsStarted)
                    Queue.Start(x => x.RunJob());

                return task;
            }
        }
    }

    public class PdfProgressItem : IProgressItem
    {
        private readonly string _contextUrl;
        private readonly int _tenantId;
        private readonly int _invoiceId;
        private readonly Guid _userId;

        public object Id { get; set; }
        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public bool IsCompleted { get; set; }

        public PdfProgressItem(HttpContext context, int tenantId, Guid userId, int invoiceId)
        {
            _contextUrl = context != null ? context.Request.GetUrlRewriter().ToString() : null;
            _tenantId = tenantId;
            _invoiceId = invoiceId;
            _userId = userId;

            Id = PdfQueueWorker.GetTaskId(tenantId, invoiceId);
            Status = ProgressStatus.Queued;
            Error = null;
            Percentage = 0;
            IsCompleted = false;
        }

        public void RunJob()
        {
            try
            {
                Percentage = 0;
                Status = ProgressStatus.Started;

                CoreContext.TenantManager.SetCurrentTenant(_tenantId);

                SecurityContext.AuthenticateMe(_userId);

                if (HttpContext.Current == null && !WorkContext.IsMono)
                {
                    HttpContext.Current = new HttpContext(
                        new HttpRequest("hack", _contextUrl, string.Empty),
                        new HttpResponse(new System.IO.StringWriter()));
                }

                PdfCreator.CreateAndSaveFile(_invoiceId);

                Percentage = 100;
                Status = ProgressStatus.Done;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Web").Error(ex);
                Percentage = 0;
                Status = ProgressStatus.Failed;
                Error = ex.Message;
            }
            finally
            {
                // fake httpcontext break configuration manager for mono
                if (!WorkContext.IsMono)
                {
                    if (HttpContext.Current != null)
                    {
                        new DisposableHttpContext(HttpContext.Current).Dispose();
                        HttpContext.Current = null;
                    }
                }

                IsCompleted = true;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}