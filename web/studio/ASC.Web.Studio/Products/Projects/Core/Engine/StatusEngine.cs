/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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