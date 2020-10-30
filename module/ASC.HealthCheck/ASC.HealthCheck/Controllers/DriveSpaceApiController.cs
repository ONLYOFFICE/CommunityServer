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