using System.Collections.Generic;
using System.Linq;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects;

namespace ASC.Projects.Engine
{
    public class StatusEngine
    {
        public IDaoFactory DaoFactory { get; set; }
        public ProjectSecurity ProjectSecurity { get; set; }

        public CustomTaskStatus Create(CustomTaskStatus status)
        {
            if (!ProjectSecurity.CurrentUserAdministrator) ProjectSecurity.CreateSecurityException();

            var statuses = DaoFactory.StatusDao.Get().Where(r => r.StatusType == status.StatusType).OrderBy(r => r.Order);
            var lastStatus = statuses.LastOrDefault();

            if (lastStatus != null)
            {
                status.Order = lastStatus.Order + 1;
            }
            else
            {
                status.Order = 1;
            }

            if (!status.Available.HasValue)
            {
                status.Available = statuses.All(r => r.Available == null || r.Available.Value);
            }

            return DaoFactory.StatusDao.Create(status);
        }

        public void Update(CustomTaskStatus status)
        {
            if (!ProjectSecurity.CurrentUserAdministrator) ProjectSecurity.CreateSecurityException();

            var statuses = DaoFactory.StatusDao.Get().Where(r => r.StatusType == status.StatusType).OrderBy(r => r.Order);

            if (!status.Available.HasValue)
            {
                status.Available = statuses.All(r => r.Available == null || r.Available.Value);
            }

            DaoFactory.StatusDao.Update(status);
        }

        public List<CustomTaskStatus> Get()
        {
            return DaoFactory.StatusDao.Get();
        }

        public List<CustomTaskStatus> GetWithDefaults()
        {
            var result = Get();

            if (!result.Any(r => r.StatusType == TaskStatus.Open && r.IsDefault))
            {
                result.Add(CustomTaskStatus.GetDefault(TaskStatus.Open));
            }
            if (!result.Any(r => r.StatusType == TaskStatus.Closed && r.IsDefault))
            {
                result.Add(CustomTaskStatus.GetDefault(TaskStatus.Closed));
            }

            return result;
        }


        public void Delete(int id)
        {
            if (!ProjectSecurity.CurrentUserAdministrator) ProjectSecurity.CreateSecurityException();
            var defaultTask = CustomTaskStatus.GetDefaults().FirstOrDefault(r => r.Id == id);
            if (defaultTask != null) return;

            DaoFactory.StatusDao.Delete(id);
        }
    }
}