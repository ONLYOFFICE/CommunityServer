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


#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using ASC.Common.Logging;
using ASC.Data.Storage.Configuration;

using SelectelSharp;
using MimeMapping = ASC.Common.Web.MimeMapping;

#endregion

namespace ASC.Data.Storage.Selectel
{
    public class SelectelStorage : BaseStorage
    {
        private readonly Dictionary<string, ACL> _domainsAcl;
        private readonly ACL _moduleAcl;
        private String _authUser;
        private String _authPwd;
        private String _private_container;
        private String _public_container;
        private String _subDir;
        private Uri _cname;
        private Uri _cnameSSL;
        private bool _lowerCasing = true;

        private static readonly ILog _logger = LogManager.GetLogger("ASC.Data.Storage.Selectel.SelectelStorage");

        public SelectelStorage(String tenant)
        {
            _tenant = tenant;
            _modulename = string.Empty;
            _dataList = null;

            _domainsExpires = new Dictionary<string, TimeSpan> {{string.Empty, TimeSpan.Zero}};
            _domainsAcl = new Dictionary<string, ACL>();
            _moduleAcl = ACL.Auto;

        }

        public SelectelStorage(String tenant, HandlerConfigurationElement handlerConfig, ModuleConfigurationElement moduleConfig)
        {
            _tenant = tenant;
            _modulename = moduleConfig.Name;
            _dataList = new DataList(moduleConfig);

            _domainsExpires = moduleConfig.Domains.Cast<DomainConfigurationElement>()
                                                  .Where(x => x.Expires != TimeSpan.Zero)
                                                  .ToDictionary(x => x.Name, y => y.Expires);
            
            _domainsExpires.Add(String.Empty, moduleConfig.Expires);
            _domainsAcl = moduleConfig.Domains.Cast<DomainConfigurationElement>().ToDictionary(x => x.Name, y => y.Acl);
            _moduleAcl = moduleConfig.Acl;

        }

        public override IDataStore Configure(IDictionary<String, String> props)
        {
            _authUser = props["authUser"];
            _authPwd = props["authPwd"];
            _public_container = props["public_container"];
            _private_container = !string.IsNullOrEmpty(props["private_container"]) ? props["private_container"] : _public_container;
            

            if (string.IsNullOrEmpty(_public_container))
                throw new ArgumentException("_public_container");

            if (props.ContainsKey("lower"))
            {
                bool.TryParse(props["lower"], out _lowerCasing);
            }

            if (props.ContainsKey("subdir"))
            {
                _subDir = props["subdir"];
            }

            var client = GetClient().Result;

            _cname = props.ContainsKey("cname") && Uri.IsWellFormedUriString(props["cname"], UriKind.Absolute)
                         ? new Uri(props["cname"], UriKind.Absolute)
                         : new Uri(client.StorageUrl, UriKind.Absolute);

            _cnameSSL = props.ContainsKey("cnamessl") &&
                             Uri.IsWellFormedUriString(props["cnamessl"], UriKind.Absolute)
                                 ? new Uri(props["cnamessl"], UriKind.Absolute)
                                 : new Uri(client.StorageUrl, UriKind.Absolute);

            return this;
        }

        private async Task<SelectelClient> GetClient()
        {
            var client = new SelectelClient();

            await client.AuthorizeAsync(_authUser, _authPwd);

            return client;
        }

        private string MakePath(string domain, string path)
        {
            string result;

            path = path.TrimStart('\\', '/').TrimEnd('/').Replace('\\', '/');

            if (!String.IsNullOrEmpty(_subDir))
            {
                if (_subDir.Length == 1 && (_subDir[0] == '/' || _subDir[0] == '\\'))
                    result = path;
                else
                    result = String.Format("{0}/{1}", _subDir.TrimEnd('/'), path); // Ignory all, if _subDir is not null
            }
            else//Key combined from module+domain+filename
                result = string.Format("{0}/{1}{2}{3}",
                                                         _tenant,
                                                         string.IsNullOrEmpty(_modulename) ? "" : _modulename + "/",
                                                         string.IsNullOrEmpty(domain) ? "" : domain + "/",
                                                         path);

            result = result.Replace("//", "/").TrimStart('/');
            if (_lowerCasing)
            {
                result = result.ToLowerInvariant();
            }

            return result;
        }

        public override Uri GetInternalUri(string domain, string path, TimeSpan expire, IEnumerable<string> headers)
        {
            if (expire == TimeSpan.Zero || expire == TimeSpan.MinValue || expire == TimeSpan.MaxValue)
            {
                expire = GetExpire(domain);
            }
            if (expire == TimeSpan.Zero || expire == TimeSpan.MinValue || expire == TimeSpan.MaxValue)
            {
                return GetUriShared(domain, path);
            }

            var client = GetClient().Result;
            return client.GetPreSignUriAsync(_private_container, MakePath(domain, path), expire).Result;
        }

        private Uri GetUriShared(string domain, string path)
        {
            return new Uri(String.Format("{0}{1}/{2}", SecureHelper.IsSecure() ? _cnameSSL : _cname, _public_container, MakePath(domain, path)));
        }

        public override System.IO.Stream GetReadStream(string domain, string path)
        {
            return GetReadStream(domain, path, 0);

        }

        public override System.IO.Stream GetReadStream(string domain, string path, int offset)
        {
            var client = GetClient().Result;

            var file = client.GetFileAsync(_private_container, MakePath(domain, path), null, false).Result;

            if (file == null) return null;

            var responseStream = file.ResponseStream;

            Stream fileStream = responseStream;

            if (offset > 0)
            {
                if (!responseStream.CanSeek)
                {
                    fileStream = responseStream.GetBuffered();
                    responseStream.Close();
                }

                fileStream.Seek(offset, SeekOrigin.Begin);
            }

            return fileStream;
        }

        public override Uri Save(string domain, string path, System.IO.Stream stream)
        {
            return Save(domain, path, stream, null, null, ACL.Auto, null);
        }

        public override Uri Save(string domain, string path, System.IO.Stream stream, Configuration.ACL acl)
        {
            return Save(domain, path, stream, null, null, acl);
        }

        protected override Uri SaveWithAutoAttachment(string domain, string path, System.IO.Stream stream, string attachmentFileName)
        {
            var contentDisposition = string.Format("attachment; filename={0};",
                                                  HttpUtility.UrlPathEncode(attachmentFileName));
            if (attachmentFileName.Any(c => (int)c >= 0 && (int)c <= 127))
            {
                contentDisposition = string.Format("attachment; filename*=utf-8''{0};",
                                                   HttpUtility.UrlPathEncode(attachmentFileName));
            }

            return Save(domain, path, stream, null, null, ACL.Auto, null);
        }

        public override Uri Save(string domain, string path, System.IO.Stream stream, string contentType, string contentDisposition)
        {
            return Save(domain, path, stream, contentType, contentDisposition, ACL.Auto, null);
        }

        public override Uri Save(string domain, string path, System.IO.Stream stream, string contentEncoding, int cacheDays)
        {
            return Save(domain, path, stream, null, null, ACL.Auto, contentEncoding, cacheDays);
        }

        public Uri Save(string domain, string path, Stream stream, string contentType,
                                string contentDisposition, ACL acl, string contentEncoding = null, int cacheDays = 5, DateTime? deleteAt = null, long? deleteAfter = null)
        {

            var client = GetClient().Result;


            var buffered = stream.GetBuffered();

            if (QuotaController != null)
            {
                QuotaController.QuotaUsedCheck(buffered.Length);
            }

            var mime = string.IsNullOrEmpty(contentType)
                                ? MimeMapping.GetMimeMapping(Path.GetFileName(path))
                                : contentType;

            if (mime == "application/octet-stream")
            {
                contentDisposition = "attachment";
            }

            var customHeaders = new Dictionary<string, object>();

            if (cacheDays > 0)
            {
                customHeaders.Add("Cache-Control", String.Format("public, maxage={0}", (int)TimeSpan.FromDays(cacheDays).TotalSeconds));
                customHeaders.Add("Expires", DateTime.UtcNow.Add(TimeSpan.FromDays(cacheDays)));
            }

            if (!String.IsNullOrEmpty(contentEncoding))
                customHeaders.Add("Content-Encoding", contentEncoding);

            client.UploadFileAsync(_private_container, MakePath(domain, path), false, true, buffered, null, contentDisposition, mime, deleteAt, deleteAfter, customHeaders).Wait();

            var cannedACL = acl == ACL.Auto ? GetDomainACL(domain) : ACL.Read;

            if (cannedACL == ACL.Read)
            {
                var createSymLinkStatus = client.CreateSymLink(
                    _public_container,
                    MakePath(domain, path),
                    SelectelSharp.Models.Link.Symlink.SymlinkType.Symlink,
                    String.Format("/{0}/{1}/", _private_container, MakePath(domain, path))).Result;

                if (!createSymLinkStatus)
                {
                    _logger.ErrorFormat("Symlink '{0}; not created", _public_container + "/" + MakePath(domain, path));

                    throw new Exception(String.Format("Symlink '{0}; not created", _public_container + "/" + MakePath(domain, path)));
                }

                try
                {
                    var invalidationResult = client.CDNIvalidationAsync(_public_container, new[] { MakePath(domain, path) }).Result;
                }
                catch (Exception exp)
                {
                    _logger.InfoFormat("The invalidation {0} failed", _public_container + "/" + MakePath(domain, path));
                    _logger.Error(exp);
                }
            }

            QuotaUsedAdd(domain, buffered.Length);

            return GetUri(domain, path);
        }


        private ACL GetDomainACL(string domain)
        {
            if (GetExpire(domain) != TimeSpan.Zero)
            {
                return ACL.Auto;
            }

            if (_domainsAcl.ContainsKey(domain))
            {
                return _domainsAcl[domain];
            }
            return _moduleAcl;
        }       

        public override void Delete(string domain, string path)
        {
            var client = GetClient().Result;

            var key = MakePath(domain, path);
            var size = GetFileSize(domain, path);

            client.DeleteFileAsync(_private_container, MakePath(domain, path)).Wait();

            QuotaUsedDelete(domain, size);

        }

        public override void DeleteFiles(string domain, string folderPath, string pattern, bool recursive)
        {

            var client = GetClient().Result;

            var files = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, MakePath(domain, folderPath), null, null)
                               .Result
                               .Where(x => Wildcard.IsMatch(pattern, Path.GetFileName(x.Name)));

            if (!files.Any()) return;

            files.ToList().ForEach(x => client.DeleteFileAsync(_private_container, x.Name).Wait());

            if (QuotaController != null)
            {
                QuotaUsedDelete(domain, files.Select(x => x.Bytes).Sum());
            }
        }

        public override void DeleteFiles(string domain, List<string> paths)
        {
            if (!paths.Any()) return;

            var keysToDel = new List<string>();

            long quotaUsed = 0;

            foreach (var path in paths)
            {
                try
                {
                    var key = MakePath(domain, path);

                    if (QuotaController != null)
                    {
                        quotaUsed += GetFileSize(domain, path);
                    }

                    keysToDel.Add(key);
                }
                catch (FileNotFoundException)
                {

                }
            }

            if (!keysToDel.Any()) return;

            var client = GetClient().Result;

            keysToDel.ForEach(x => client.DeleteFileAsync(_private_container, x).Wait());

            if (quotaUsed > 0)
            {
                QuotaUsedDelete(domain, quotaUsed);
            }
        }

        public override void DeleteFiles(string domain, string folderPath, DateTime fromDate, DateTime toDate)
        {
            var client = GetClient().Result;

            var files = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, MakePath(domain, folderPath), null, null)
                               .Result
                               .Where(x => x.Date >= fromDate && x.Date <= toDate);

            if (!files.Any()) return;

            files.ToList().ForEach(x => client.DeleteFileAsync(_private_container, x.Name).Wait());

            if (QuotaController != null)
            {
                QuotaUsedDelete(domain, files.Select(x => x.Bytes).Sum());
            }
        }

        public override void MoveDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            var client = GetClient().Result;
            var srckey = MakePath(srcdomain, srcdir);
            var dstkey = MakePath(newdomain, newdir);

            var paths = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, srckey).Result.Select(x => x.Name);

            foreach (var path in paths)
            {
                client.CopyFileAsync(_private_container, path, _private_container, path.Replace(srckey, dstkey)).Wait();
                client.DeleteFileAsync(_private_container, path).Wait();
            }
        }

        public override Uri Move(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            var srcKey = MakePath(srcdomain, srcpath);
            var dstKey = MakePath(newdomain, newpath);
            var size = GetFileSize(srcdomain, srcpath);
            var client = GetClient().Result;

            client.CopyFileAsync(_private_container, srcKey, _private_container, dstKey).Wait();
            Delete(srcdomain, srcpath);

            QuotaUsedDelete(srcdomain, size);
            QuotaUsedAdd(newdomain, size);

            return GetUri(newdomain, newpath);
        }

        public override Uri SaveTemp(string domain, out string assignedPath, System.IO.Stream stream)
        {
            assignedPath = Guid.NewGuid().ToString();
            return Save(domain, assignedPath, stream);
        }

        public override string[] ListDirectoriesRelative(string domain, string path, bool recursive)
        {
            var client = GetClient().Result;

            return client.GetContainerFilesAsync(_private_container, int.MaxValue, null, path)
                  .Result
                  .Select(x => x.Name.Substring(MakePath(domain, path + "/").Length)).ToArray();
        }

        public override string[] ListFilesRelative(string domain, string path, string pattern, bool recursive)
        {
            var paths = new List<String>();
            var client = GetClient().Result;

            if (recursive)
            {
                paths = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, MakePath(domain, path)).Result.Select(x => x.Name).ToList();
            }
            else
            {
                paths = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, MakePath(domain, path)).Result.Select(x => x.Name).ToList();
            }

            return paths
                .Where(x => Wildcard.IsMatch(pattern, Path.GetFileName(x)))
                .Select(x => x.Substring(MakePath(domain, path + "/").Length).TrimStart('/')).ToArray();
        }

        public override bool IsFile(string domain, string path)
        {
            var client = GetClient().Result;

            var files = client.GetContainerFilesAsync(_private_container, 2, null, MakePath(domain, path), null, null).Result;

            if (files == null) return false;

            return files.Count() > 0;
        }

        public override bool IsDirectory(string domain, string path)
        {
            return IsFile(domain, path);
        }

        public override void DeleteDirectory(string domain, string path)
        {
            var client = GetClient().Result;

            var files = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, MakePath(domain, path), null, null).Result;

            if (!files.Any()) return;

            files.ForEach(x => client.DeleteFileAsync(_private_container, x.Name).Wait());

            if (QuotaController != null)
            {
                QuotaUsedDelete(domain, files.Select(x => x.Bytes).Sum());
            }
        }

        public override long GetFileSize(string domain, string path)
        {
            var client = GetClient().Result;

            var fileInfos = client.GetContainerFilesAsync(_private_container, 1, null, MakePath(domain, path), null, null).Result;

            if (!fileInfos.Any()) throw new FileNotFoundException();

            return fileInfos.Single().Bytes;
        }

        public override long GetDirectorySize(string domain, string path)
        {
            var client = GetClient().Result;

            var fileInfos = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, MakePath(domain, path), null, null).Result;

            long quotaUsed = 0;

            foreach (var info in fileInfos)
            {
                quotaUsed += info.Bytes;

            }

            return quotaUsed;
        }

        public override long ResetQuota(string domain)
        {
            if (QuotaController != null)
            {
                var client = GetClient().Result;

                var files = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, domain).Result;

                if (files == null) return 0;

                var size = files.Select(x => x.Bytes).Sum();

                QuotaController.QuotaUsedSet(_modulename, domain, _dataList.GetData(domain), size);

                return size;
            }

            return 0;
        }

        public override long GetUsedQuota(string domain)
        {
            var client = GetClient().Result;

            var files = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, MakePath(domain, String.Empty)).Result;

            if (files == null) return 0;

            var size = files.Select(x => x.Bytes).Sum();

            return size;
        }

        public override Uri Copy(string srcdomain, string path, string newdomain, string newpath)
        {
            var srcKey = MakePath(srcdomain, path);
            var dstKey = MakePath(newdomain, newpath);
            var size = GetFileSize(srcdomain, path);
            var client = GetClient().Result;

            client.CopyFileAsync(_private_container, srcKey, _private_container, dstKey).Wait();

            QuotaUsedAdd(newdomain, size);

            return GetUri(newdomain, newpath);

        }

        public override void CopyDirectory(string srcdomain, string dir, string newdomain, string newdir)
        {
            var srckey = MakePath(srcdomain, dir);
            var dstkey = MakePath(newdomain, newdir);
            var client = GetClient().Result;

            var files = client.GetContainerFilesAsync(_private_container, int.MaxValue, null, srckey).Result;

            foreach (var file in files)
            {
                client.CopyFileAsync(_private_container, file.Name, _private_container, file.Name.Replace(srckey, dstkey)).Wait();

                QuotaUsedAdd(newdomain, file.Bytes);
            }
        }

        public override string SavePrivate(string domain, string path, System.IO.Stream stream, DateTime expires)
        {
            var uri = Save(domain, path, stream, "application/octet-stream", "attachment", ACL.Auto, null, 5, expires);

            return uri.ToString();
        }

        public override void DeleteExpired(string domain, string path, TimeSpan oldThreshold)
        {
            // selectel run automatically deleting files
        }

        public override string GetUploadForm(string domain, string directoryPath, string redirectTo, long maxUploadSize, string contentType, string contentDisposition, string submitLabel)
        {
            throw new NotImplementedException();
        }

        public override string GetUploadedUrl(string domain, string directoryPath)
        {
            throw new NotImplementedException();
        }

        public override string GetUploadUrl()
        {
            throw new NotImplementedException();
        }

        public override string GetPostParams(string domain, string directoryPath, long maxUploadSize, string contentType, string contentDisposition)
        {
            throw new NotImplementedException();
        }

        #region chunking

        public override string InitiateChunkedUpload(string domain, string path)
        {
            return Path.GetTempFileName();
        }

        public override string UploadChunk(string domain, string path, string filePath, Stream stream, long defaultChunkSize, int chunkNumber, long chunkLength)
        {
            int BufferSize = 8192;                      

            var mode = chunkNumber == 0 ? FileMode.Create : FileMode.Append;

            using (var fs = new FileStream(filePath, mode))
            {
                var buffer = new byte[BufferSize];
                int readed;
                while ((readed = stream.Read(buffer, 0, BufferSize)) != 0)
                {
                    fs.Write(buffer, 0, readed);
                }
            }

            return string.Format("{0}_{1}", chunkNumber, filePath);         
        }

        public override void AbortChunkedUpload(string domain, string path, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public override Uri FinalizeChunkedUpload(string domain, string path, string filePath, Dictionary<int, string> eTags)
        {
            var stream = new FileStream(filePath, FileMode.Open);

            var client = GetClient().Result;

            client.UploadFileAsync(_private_container, MakePath(domain, path), true, true,stream).Wait();

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            if (QuotaController != null)
            {
                var size = GetFileSize(domain, path);

                QuotaUsedAdd(domain, size);
            }

            return GetUri(domain, path);
        }

        public override bool IsSupportChunking { get { return true; } }

        #endregion

    }
}
