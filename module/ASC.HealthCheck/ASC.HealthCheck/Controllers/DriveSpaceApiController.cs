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


using ASC.HealthCheck.Classes;
using ASC.HealthCheck.Resources;
using log4net;
using System;
using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using ASC.HealthCheck.Settings;

namespace ASC.HealthCheck.Controllers
{
    public class DriveSpaceApiController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(typeof(DownloadLogsApiController));
        public ResultHelper ResultHelper { get { return new ResultHelper(Configuration.Formatters.JsonFormatter); } }

        [HttpGet]
        public HttpResponseMessage GetDriveSpace()
        {
            try
            {
                log.Debug("GetDriveSpace");
                IDriveSpaceChecker spaceChecker = new DriveSpaceChecker();
                spaceChecker.GetTotalAndFreeSpace();
                var freeSpace = spaceChecker.TotalFreeSpace;
                var totalSpace = spaceChecker.TotalSpace;
                var driveName = spaceChecker.DriveName;

                if (freeSpace == -1 || totalSpace == -1)
                {
                    log.Error("Not found drive name");
                    return ResultHelper.Error(HealthCheckResource.NoSuchHardDrive);
                }

                var totalSpaceString = totalSpace >= HealthCheckRunner.OneGb ?
                                           string.Format(HealthCheckResource.GigaByte, Math.Round((double)totalSpace / HealthCheckRunner.OneGb, 1)) :
                                           string.Format(HealthCheckResource.MegaByte, totalSpace / HealthCheckRunner.OneMb);
                var freeSpaceString = freeSpace >= HealthCheckRunner.OneGb ?
                                          string.Format(HealthCheckResource.GigaByte, Math.Round((double)freeSpace / HealthCheckRunner.OneGb, 1)) :
                                          string.Format(HealthCheckResource.MegaByte, freeSpace / HealthCheckRunner.OneMb);

                var driveSpaceThreashold = HealthCheckCfgSectionHandler.Instance.DriveSpaceThreashold;

                return ResultHelper.GetContent(new
                        {
                            code = 1,
                            freeSpace = freeSpaceString,
                            totalSpace = totalSpaceString,
                            freeSpaceInBytes = freeSpace,
                            driveSpaceThreashold,
                            driveName,
                            status = string.Empty
                        });
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on GetDriveSpace: {0} {1}", ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return ResultHelper.Error(HealthCheckResource.GetDriveSpaceError);
            }
        }
    }
}