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