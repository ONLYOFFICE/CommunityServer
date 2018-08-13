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


#region Import


using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using ASC.Data.Backup.Logging;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Upload;
using System.Net.Http;

#endregion

namespace ASC.Data.Backup.Storage
{
    internal class GoogleBackupStorage : IBackupStorage
    {
        private readonly string _jsonPath;
        private readonly string _bucket;
   
        private readonly ILog log = LogFactory.Create("ASC.Backup.Service");

        public GoogleBackupStorage(string jsonPath, string bucket)
        {
            this._jsonPath = jsonPath;
            this._bucket = bucket;
        }

        public string Upload(string storageBasePath, string localPath, Guid userId)
        {
            String key = String.Empty;

            if (String.IsNullOrEmpty(storageBasePath))
                key = "backup/" + Path.GetFileName(localPath);
            else                        
                key = String.Concat(storageBasePath.Trim(new char[] {' ', '/', '\\'}), "/", Path.GetFileName(localPath));

            var storage = GetStorage();

            using (var inputStream = new FileStream(localPath, FileMode.Open))
            {
                storage.UploadObject(_bucket, key, null, inputStream);
            }

            return key;
        }

        public void Download(string storagePath, string targetLocalPath)
        {
            var storage = GetStorage();

            using (var outputStream = new FileStream(targetLocalPath, FileMode.Create))
            {
                storage.DownloadObject(_bucket, GetKey(storagePath), outputStream, null, null);
            }
        }

        public void Delete(string storagePath)
        {
            var storage = GetStorage();

            storage.DeleteObject(_bucket, GetKey(storagePath));
        }

        public bool IsExists(string storagePath)
        {
            var storage = GetStorage();

            try
            {
                return storage.ListObjects(_bucket, GetKey(storagePath)).Count() > 0;
            }
            catch(Exception ex)
            {
                log.Warn(ex);
                
                return false;
            }            
        }

        public string GetPublicLink(string storagePath)
        {
            var storage = GetStorage();

            var preSignedURL = UrlSigner.FromServiceAccountPath(_jsonPath)
                                        .Sign(_bucket, 
                                              GetKey(storagePath), 
                                              TimeSpan.FromDays(1), 
                                              HttpMethod.Get);

            return preSignedURL;
        }

        private string GetKey(string fileName)
        {
           // return "backup/" + Path.GetFileName(fileName);
            return fileName;
        }

        private StorageClient GetStorage()
        {
            GoogleCredential credential = null;

            using (var jsonStream = new FileStream(_jsonPath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                credential = GoogleCredential.FromStream(jsonStream);
            }

            return StorageClient.Create(credential);
        }
    }
}
