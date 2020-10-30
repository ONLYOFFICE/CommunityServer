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


using ASC.Core;
using log4net;
using System;
using System.IO;
using System.Linq;

namespace ASC.HealthCheck.Classes
{
    public class DriveSpaceChecker : IDriveSpaceChecker
    {
        public long TotalFreeSpace { get; private set; }
        public long TotalSpace { get; private set; }
        public string DriveName { get; private set; }

        private readonly ILog log = LogManager.GetLogger(typeof(DriveSpaceChecker));

        public DriveSpaceChecker()
        {
            TotalFreeSpace = -1;
            TotalSpace = -1;

            try
            {
                DriveName = Path.GetPathRoot(Environment.CurrentDirectory);
            }
            catch
            {
                DriveName = "C:\\";
            }
        }

        public void GetTotalAndFreeSpace()
        {
            log.DebugFormat("GetTotalAndFreeSpace: driveName = {0}", DriveName);

            var docker = WorkContext.IsMono && File.Exists("/.dockerinit");

            // Reverse because A:// is inactive and slow
            foreach (var drive in DriveInfo.GetDrives().Reverse())
            {
                if (drive.IsReady && (drive.Name == DriveName || docker))
                {
                    TotalFreeSpace = drive.TotalFreeSpace;
                    TotalSpace = drive.TotalSize;
                    log.DebugFormat("GetTotalAndFreeSpace: DriveName = {0} TotalFreeSpace = {1} TotalSpace = {2}", DriveName, TotalFreeSpace, TotalSpace);
                    return;
                }
            }
        }
    }
}