/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System.ServiceModel;
using ASC.Common.Module;

namespace ASC.Data.Backup.Service
{
    public class BackupServiceLauncher : IServiceController
    {
        private ServiceHost host;
        private BackupCleanerService cleanerService;
        private BackupSchedulerService schedulerService;

        public void Start()
        {
            var config = BackupConfigurationSection.GetSection();

            BackupWorker.Start(config);
            host = new ServiceHost(typeof(BackupService));
            host.Open();

            if (config.Cleaner.ElementInformation.IsPresent)
            {
                cleanerService = new BackupCleanerService { Period = config.Cleaner.Period };
                cleanerService.Start();
            }
            if (config.Scheduler.ElementInformation.IsPresent)
            {
                schedulerService = new BackupSchedulerService { Period = config.Scheduler.Period };
                schedulerService.Start();
            }
        }

        public void Stop()
        {
            BackupWorker.Stop();
            if (host != null)
            {
                host.Close();
                host = null;
            }
            if (cleanerService != null)
            {
                cleanerService.Stop();
                cleanerService = null;
            }
            if (schedulerService != null)
            {
                schedulerService.Stop();
                schedulerService = null;
            }
        }
    }
}
