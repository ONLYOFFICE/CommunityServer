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
using System.IO;
using System.Net;
using ASC.Data.Backup.Logging;
using System.Threading.Tasks;
using SelectelSharp;

#endregion

namespace ASC.Data.Backup.Storage
{
    internal class SelectelBackupStorage : IBackupStorage
    {
        private readonly string _authUser;
        private readonly string _authPwd;
        private readonly string _container;
        private readonly string _region;

        private readonly ILog log = LogFactory.Create("ASC.Backup.Service");

        public SelectelBackupStorage(string authUser, string authPwd, string container, string region)
        {
            this._authUser = authUser;
            this._authPwd = authPwd;
            this._container = container;
            this._region = region;
        }

        public string Upload(string storageBasePath, string localPath, Guid userId)
        {
            String key = String.Empty;

            if (String.IsNullOrEmpty(storageBasePath))
                key = "backup/" + Path.GetFileName(localPath);
            else
                key = String.Concat(storageBasePath.Trim(new char[] { ' ', '/', '\\' }), "/", Path.GetFileName(localPath));

            var client = GetClient().Result;

            using (var inputStream = new FileStream(localPath, FileMode.Open))
            {
                client.UploadFileAsync(_container, key, true, true, inputStream);
            }

            return key;
        }

        public void Download(string storagePath, string targetLocalPath)
        {
            var client = GetClient().Result;

            using (var responseStream = client.GetFileAsync(_container, GetKey(storagePath)).Result.ResponseStream)
            using (var file = new FileStream(targetLocalPath, FileMode.Create))
            {
                var buffer = new byte[4096];

                int readed;

                while ((readed = responseStream.Read(buffer, 0, 4096)) != 0)
                {
                    file.Write(buffer, 0, readed);

                }
            }          
        }

        public void Delete(string storagePath)
        {
            var client = GetClient().Result;

            client.DeleteFileAsync(_container, GetKey(storagePath)).Wait();
        }

        public bool IsExists(string storagePath)
        {
            var client = GetClient().Result;

            try
            {
                var files = client.GetContainerFilesAsync(_container, int.MaxValue, null, GetKey(storagePath), null, null).Result;

                return files.Count > 0;
            }
            catch (Exception ex)
            {
                log.Warn(ex);

                return false;
            }

        }

        public string GetPublicLink(string storagePath)
        {
            var client = GetClient().Result;

            return client.GetPreSignUriAsync(_container, GetKey(storagePath), TimeSpan.FromDays(1)).Result.ToString();
        }

        private string GetKey(string fileName)
        {
            // return "backup/" + Path.GetFileName(fileName);
            return fileName;
        }

        private async Task<SelectelClient> GetClient()
        {
            var client = new SelectelClient();

            await client.AuthorizeAsync(_authUser, _authPwd);

            return client;
        }
    }
}
