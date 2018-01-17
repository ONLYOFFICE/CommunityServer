using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Projects.Core.Engine;
using ASC.Web.Studio.Core.Notify;
using CrmDaoFactory = ASC.CRM.Core.Dao.DaoFactory;

namespace ASC.Data.Reassigns
{
    public class ReassignProgressItem : IProgressItem
    {
        private readonly HttpContext _context;
        private readonly Dictionary<string, string> _httpHeaders;

        private readonly int _tenantId;
        private readonly Guid _fromUserId;
        private readonly Guid _toUserId;
        private readonly Guid _currentUserId;

        private readonly CrmDaoFactory _crmDaoFactory;
        private readonly IFileStorageService _docService;
        private readonly ProjectsReassign _projectsReassign;

        public object Id { get; set; }
        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public bool IsCompleted { get; set; }

        public ReassignProgressItem(HttpContext context, int tenantId, Guid fromUserId, Guid toUserId, Guid currentUserId)
        {
            _context = context;
            _httpHeaders = GetHttpHeaders(context.Request);

            _tenantId = tenantId;
            _fromUserId = fromUserId;
            _toUserId = toUserId;
            _currentUserId = currentUserId;

            _crmDaoFactory = Web.CRM.Classes.Global.DaoFactory;
            _docService = Web.Files.Classes.Global.FileStorageService;
            _projectsReassign = new ProjectsReassign();

            Id = QueueWorker.GetProgressItemId(tenantId, fromUserId, typeof(ReassignProgressItem));
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
                SecurityContext.AuthenticateMe(_currentUserId);

                _crmDaoFactory.GetContactDao().ReassignContactsResponsible(_fromUserId, _toUserId);
                _crmDaoFactory.GetDealDao().ReassignDealsResponsible(_fromUserId, _toUserId);
                _crmDaoFactory.GetTaskDao().ReassignTasksResponsible(_fromUserId, _toUserId);
                _crmDaoFactory.GetCasesDao().ReassignCasesResponsible(_fromUserId, _toUserId);

                _docService.ReassignStorage(_fromUserId, _toUserId);

                _projectsReassign.Reassign(_fromUserId, _toUserId);

                SendNotify();

                Percentage = 100;
                Status = ProgressStatus.Done;
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.Web").Error(ex);
                Percentage = 0;
                Status = ProgressStatus.Failed;
                Error = ex.Message;
            }
            finally
            {
                IsCompleted = true;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        private static Dictionary<string, string> GetHttpHeaders(HttpRequest httpRequest)
        {
            return httpRequest == null
                       ? null
                       : httpRequest.Headers.AllKeys.ToDictionary(key => key, key => httpRequest.Headers[key]);
        }

        private void SendNotify()
        {
            var fromUser = CoreContext.UserManager.GetUsers(_fromUserId);
            var toUser = CoreContext.UserManager.GetUsers(_toUserId);

            StudioNotifyService.Instance.SendMsgReassignsCompleted(_currentUserId, fromUser, toUser);

            var fromUserName = fromUser.DisplayUserName(false);
            var toUserName = toUser.DisplayUserName(false);

            if (_httpHeaders != null)
            {
                MessageService.Send(_httpHeaders, MessageAction.UserDataReassigns,
                                    new[] {fromUserName, toUserName});
            }
            else
            {
                MessageService.Send(_context.Request, MessageAction.UserDataReassigns, fromUserName,
                                    toUserName);
            }
        }
    }
}
