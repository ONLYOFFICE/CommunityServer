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
using System.Web;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.CRM.Core;
using ASC.Web.Core.Users;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Projects.Core.Engine;
using ASC.Web.Studio.Core.Notify;
using Autofac;
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
        private readonly bool _deleteProfile;

        private readonly IFileStorageService _docService;
        private readonly ProjectsReassign _projectsReassign;

        public object Id { get; set; }
        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public bool IsCompleted { get; set; }
        public Guid FromUser { get { return _fromUserId; } }
        public Guid ToUser { get { return _toUserId; } }

        public ReassignProgressItem(HttpContext context, int tenantId, Guid fromUserId, Guid toUserId, Guid currentUserId, bool deleteProfile)
        {
            _context = context;
            _httpHeaders = QueueWorker.GetHttpHeaders(context.Request);

            _tenantId = tenantId;
            _fromUserId = fromUserId;
            _toUserId = toUserId;
            _currentUserId = currentUserId;
            _deleteProfile = deleteProfile;

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
            var logger = log4net.LogManager.GetLogger("ASC.Web");

            try
            {
                Percentage = 0;
                Status = ProgressStatus.Started;

                CoreContext.TenantManager.SetCurrentTenant(_tenantId);
                SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);

                logger.InfoFormat("reassignment of data from {0} to {1}", _fromUserId, _toUserId);

                logger.Info("reassignment of data from documents");

                Percentage = 33;
                _docService.ReassignStorage(_fromUserId, _toUserId);

                logger.Info("reassignment of data from projects");

                Percentage = 66;
                _projectsReassign.Reassign(_fromUserId, _toUserId);

                logger.Info("reassignment of data from crm");

                Percentage = 99;
                using (var scope = DIHelper.Resolve(_tenantId))
                {
                    var crmDaoFactory = scope.Resolve<CrmDaoFactory>();
                    crmDaoFactory.ContactDao.ReassignContactsResponsible(_fromUserId, _toUserId);
                    crmDaoFactory.DealDao.ReassignDealsResponsible(_fromUserId, _toUserId);
                    crmDaoFactory.TaskDao.ReassignTasksResponsible(_fromUserId, _toUserId);
                    crmDaoFactory.CasesDao.ReassignCasesResponsible(_fromUserId, _toUserId);
                }

                SendSuccessNotify();

                Percentage = 100;
                Status = ProgressStatus.Done;

                if (_deleteProfile)
                {
                    DeleteUserProfile();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Status = ProgressStatus.Failed;
                Error = ex.Message;
                SendErrorNotify(ex.Message);
            }
            finally
            {
                logger.Info("data reassignment is complete");
                IsCompleted = true;
                SecurityContext.AuthenticateMe(_currentUserId);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        private void SendSuccessNotify()
        {
            var fromUser = CoreContext.UserManager.GetUsers(_fromUserId);
            var toUser = CoreContext.UserManager.GetUsers(_toUserId);

            StudioNotifyService.Instance.SendMsgReassignsCompleted(_currentUserId, fromUser, toUser);

            var fromUserName = fromUser.DisplayUserName(false);
            var toUserName = toUser.DisplayUserName(false);

            if (_httpHeaders != null)
                MessageService.Send(_httpHeaders, MessageAction.UserDataReassigns, MessageTarget.Create(_fromUserId),
                                    new[] {fromUserName, toUserName});
            else
                MessageService.Send(_context.Request, MessageAction.UserDataReassigns, MessageTarget.Create(_fromUserId),
                                    fromUserName, toUserName);
        }

        private void SendErrorNotify(string errorMessage)
        {
            var fromUser = CoreContext.UserManager.GetUsers(_fromUserId);
            var toUser = CoreContext.UserManager.GetUsers(_toUserId);

            StudioNotifyService.Instance.SendMsgReassignsFailed(_currentUserId, fromUser, toUser, errorMessage);
        }

        private void DeleteUserProfile()
        {
            var user = CoreContext.UserManager.GetUsers(_fromUserId);
            var userName = user.DisplayUserName(false);

            UserPhotoManager.RemovePhoto(user.ID);
            CoreContext.UserManager.DeleteUser(user.ID);
            QueueWorker.StartRemove(_context, _tenantId, user, _currentUserId, false);

            if (_httpHeaders != null)
                MessageService.Send(_httpHeaders, MessageAction.UserDeleted, MessageTarget.Create(_fromUserId),
                                    new[] {userName});
            else
                MessageService.Send(_context.Request, MessageAction.UserDeleted, MessageTarget.Create(_fromUserId),
                                    userName);
        }
    }
}
