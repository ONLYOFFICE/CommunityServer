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
using System.IO;

namespace ASC.Data.Backup.Storage
{
    internal class LocalBackupStorage : IBackupStorage
    {
        public string Upload(string storageBasePath, string localPath, Guid userId)
        {
            if (!Directory.Exists(storageBasePath))
            {
                throw new FileNotFoundException("Directory not found.");
            }
            var storagePath = Path.Combine(storageBasePath, Path.GetFileName(localPath));
            if (localPath != storagePath)
            {
                File.Copy(localPath, storagePath, true);
            }
            return storagePath;
        }

        public void Download(string storagePath, string targetLocalPath)
        {
            File.Copy(storagePath, targetLocalPath, true);
        }

        public void Delete(string storagePath)
        {
            File.Delete(storagePath);
        }

        public bool IsExists(string storagePath)
        {
            return File.Exists(storagePath);
        }

        public string GetPublicLink(string storagePath)
        {
            return string.Empty;
        }
    }
}
