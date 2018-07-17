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


using System;
using System.Web;
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
            Status = PdfProgressStatus.Queued;
            Error = null;
            Percentage = 0;
            IsCompleted = false;
        }

        public void RunJob()
        {
            try
            {
                Percentage = 0;
                Status = PdfProgressStatus.Started;

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
                Status = PdfProgressStatus.Done;
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Web").Error(ex);
                Percentage = 0;
                Status = PdfProgressStatus.Failed;
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

    public enum PdfProgressStatus
    {
        Queued,
        Started,
        Done,
        Failed
    }
}