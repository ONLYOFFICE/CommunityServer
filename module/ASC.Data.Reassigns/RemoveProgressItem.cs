using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Mail.GarbageEraser;
using ASC.Web.Core.Users;
using ASC.Web.Files.Services.WCFService;
using CrmDaoFactory = ASC.CRM.Core.Dao.DaoFactory;

namespace ASC.Data.Reassigns
{
    public class RemoveProgressItem : IProgressItem
    {
        private readonly int _tenantId;
        private readonly Guid _userId;
        private readonly Guid _currentUserId;

        private readonly CrmDaoFactory _crmDaoFactory;
        private readonly IFileStorageService _docService;
        private readonly MailGarbageEraser _mailEraser;

        public object Id { get; set; }
        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public bool IsCompleted { get; set; }

        public RemoveProgressItem(int tenantId, Guid userId, Guid currentUserId)
        {
            _tenantId = tenantId;
            _userId = userId;
            _currentUserId = currentUserId;

            _crmDaoFactory = Web.CRM.Classes.Global.DaoFactory;
            _docService = Web.Files.Classes.Global.FileStorageService;
            _mailEraser = new MailGarbageEraser();

            Id = QueueWorker.GetProgressItemId(tenantId, userId, typeof(RemoveProgressItem));
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

                _crmDaoFactory.GetReportDao().DeleteFiles(_userId);

                _docService.DeleteStorage(_userId);

                DeleteTalkStorage();

                _mailEraser.ClearUserMail(_userId);

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

        private void DeleteTalkStorage()
        {
            var data = MD5.Create().ComputeHash(Encoding.Default.GetBytes(_userId.ToString()));

            var sBuilder = new StringBuilder();

            for (int i = 0, n = data.Length; i < n; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            var md5Hash = sBuilder.ToString();

            var storage = StorageFactory.GetStorage(_tenantId.ToString(CultureInfo.InvariantCulture), "talk");

            if (storage != null && storage.IsDirectory(md5Hash))
            {
                storage.DeleteDirectory(md5Hash);
            }
        }

    }
}
