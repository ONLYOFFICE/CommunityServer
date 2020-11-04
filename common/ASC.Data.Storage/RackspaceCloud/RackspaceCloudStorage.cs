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
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ASC.Data.Storage.Configuration;
using ASC.Common.Logging;

using net.openstack.Core.Domain;
using net.openstack.Providers.Rackspace;

using MimeMapping = ASC.Common.Web.MimeMapping;

namespace ASC.Data.Storage.RackspaceCloud
{
    public class RackspaceCloudStorage : BaseStorage
    {
        private string _region;
        private string _private_container;
        private String _public_container;
        private readonly List<string> _domains = new List<string>();
        private readonly Dictionary<String, ACL> _domainsAcl;
        private readonly ACL _moduleAcl;
        private String _subDir;
        private string _username;
        private string _apiKey;
        private bool _lowerCasing = true;
        private Uri _cname;
        private Uri _cnameSSL;     

        private static readonly ILog _logger = LogManager.GetLogger("ASC.Data.Storage.Rackspace.RackspaceCloudStorage");
        
        public RackspaceCloudStorage(string tenant)
        {
             
            _tenant = tenant;
            _modulename = string.Empty;
            _dataList = null;

            _domainsExpires = new Dictionary<string, TimeSpan> { { string.Empty, TimeSpan.Zero } };
            _domainsAcl = new Dictionary<string, ACL>();
            _moduleAcl = ACL.Auto;
        }

        public RackspaceCloudStorage(string tenant, HandlerConfigurationElement handlerConfig, ModuleConfigurationElement moduleConfig)
        {
             
            _tenant = tenant;
            _modulename = moduleConfig.Name;
            _dataList = new DataList(moduleConfig);

            _domains.AddRange(
                moduleConfig.Domains.Cast<DomainConfigurationElement>().Select(x => string.Format("{0}/", x.Name)));

            //Make acl
            _domainsExpires =
                moduleConfig.Domains.Cast<DomainConfigurationElement>().Where(x => x.Expires != TimeSpan.Zero).
                    ToDictionary(x => x.Name,
                                 y => y.Expires);

            _domainsExpires.Add(string.Empty, moduleConfig.Expires);

            _domainsAcl = moduleConfig.Domains.Cast<DomainConfigurationElement>().ToDictionary(x => x.Name,
                                                                                               y => y.Acl);
            _moduleAcl = moduleConfig.Acl;
            
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
                    result = String.Format("{0}/{1}", _subDir, path); // Ignory all, if _subDir is not null
            }
            else//Key combined from module+domain+filename
                result = string.Format("{0}/{1}/{2}/{3}",
                                                         _tenant,
                                                         _modulename,
                                                         domain,
                                                         path);

            result = result.Replace("//", "/").TrimStart('/');
            if (_lowerCasing)
            {
                result = result.ToLowerInvariant();
            }
            
            return result;
        }
        
        private CloudFilesProvider GetClient()
        {
            CloudIdentity cloudIdentity = new CloudIdentity()
            {
                Username = _username,
                APIKey = _apiKey
            };
                     
            return new CloudFilesProvider(cloudIdentity);
        }

        public override IDataStore Configure(IDictionary<string, string> props)
        {
            _private_container = props["private_container"];
            _region = props["region"];
            _apiKey = props["apiKey"];
            _username = props["username"];

            if (props.ContainsKey("lower"))
            {
                bool.TryParse(props["lower"], out _lowerCasing);
            }

            if (props.ContainsKey("subdir"))
            {
                _subDir = props["subdir"];
            }

            _public_container = props["public_container"];

            if (String.IsNullOrEmpty(_public_container))
                throw new ArgumentException("_public_container");

            var client = GetClient();

            var cdnHeaders = client.GetContainerCDNHeader(_public_container, _region);

            _cname = props.ContainsKey("cname") && Uri.IsWellFormedUriString(props["cname"], UriKind.Absolute)
                         ? new Uri(props["cname"], UriKind.Absolute)
                         : new Uri(cdnHeaders.CDNUri);

            _cnameSSL = props.ContainsKey("cnamessl") &&
                             Uri.IsWellFormedUriString(props["cnamessl"], UriKind.Absolute)
                                 ? new Uri(props["cnamessl"], UriKind.Absolute)
                                 : new Uri(cdnHeaders.CDNSslUri);

            return this;
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

            var client = GetClient();

            var accounMetaData = client.GetAccountMetaData(_region);
            var secretKey = String.Empty;
      
            if (accounMetaData.ContainsKey("Temp-Url-Key"))
            {
                secretKey = accounMetaData["Temp-Url-Key"];
            }
            else
            {
                secretKey = ASC.Common.Utils.RandomString.Generate(64);
                accounMetaData.Add("Temp-Url-Key", secretKey);
                client.UpdateAccountMetadata(accounMetaData, _region);
            }
            
            return client.CreateTemporaryPublicUri(
                                                        JSIStudios.SimpleRESTServices.Client.HttpMethod.GET,
                                                        _private_container,
                                                        MakePath(domain, path),
                                                        secretKey,                                                       
                                                        DateTime.UtcNow.Add(expire),
                                                        _region);
        }

        private Uri GetUriShared(string domain, string path)
        {
            return new Uri(String.Format("{0}{1}", SecureHelper.IsSecure() ? _cnameSSL : _cname, MakePath(domain, path)));
        }

        public override Stream GetReadStream(string domain, string path)
        {
            return GetReadStream(domain, path, 0);
        }

        public override Stream GetReadStream(string domain, string path, int offset)
        {
            Stream outputStream = TempStream.Create();

            var client = GetClient();

            client.GetObject(_private_container, MakePath(domain, path), outputStream);

            outputStream.Position = 0;

            if (0 < offset) outputStream.Seek(Convert.ToInt64(offset), SeekOrigin.Begin);

            return outputStream;
        }

        public override Uri Save(string domain, string path, Stream stream)
        {
            return Save(domain, path, stream, string.Empty, string.Empty);
        }

        public override Uri Save(string domain, string path, Stream stream, ACL acl)
        {
            return Save(domain, path, stream, null, null, acl);
        }

        protected override Uri SaveWithAutoAttachment(string domain, string path, Stream stream, string attachmentFileName)
        {
            var contentDisposition = string.Format("attachment; filename={0};",
                                                HttpUtility.UrlPathEncode(attachmentFileName));
            if (attachmentFileName.Any(c => (int)c >= 0 && (int)c <= 127))
            {
                contentDisposition = string.Format("attachment; filename*=utf-8''{0};",
                                                   HttpUtility.UrlPathEncode(attachmentFileName));
            }

            return Save(domain, path, stream, null, contentDisposition);
        }

        public override Uri Save(string domain, string path, Stream stream, string contentType, string contentDisposition)
        {
            return Save(domain, path, stream, contentType, contentDisposition, ACL.Auto);
        }

        public override Uri Save(string domain, string path, Stream stream, string contentEncoding, int cacheDays)
        {
            return Save(domain, path, stream, string.Empty, string.Empty, ACL.Auto, contentEncoding, cacheDays);
        }

        public Uri Save(string domain, string path, Stream stream, string contentType,
                              string contentDisposition, ACL acl, string contentEncoding = null, int cacheDays = 5, 
            DateTime? deleteAt = null, long? deleteAfter = null)
        {
            var buffered = stream.GetBuffered();

            if (QuotaController != null)
            {
                QuotaController.QuotaUsedCheck(buffered.Length);
            }

            var client = GetClient();

            var mime = string.IsNullOrEmpty(contentType)
                                 ? MimeMapping.GetMimeMapping(Path.GetFileName(path))
                                 : contentType;

            if (mime == "application/octet-stream")
            {
                contentDisposition = "attachment";
            }

            var customHeaders = new Dictionary<String, String>();

            if (cacheDays > 0)
            {
                customHeaders.Add("Cache-Control", String.Format("public, maxage={0}", (int)TimeSpan.FromDays(cacheDays).TotalSeconds));
                customHeaders.Add("Expires", DateTime.UtcNow.Add(TimeSpan.FromDays(cacheDays)).ToString());
            }

            if (deleteAt.HasValue)
            {
                var ts = deleteAt.Value - new DateTime(1970, 1, 1, 0, 0, 0);
                var unixTimestamp =  (long)ts.TotalSeconds;

                customHeaders.Add("X-Delete-At", unixTimestamp.ToString());
            }

            if (deleteAfter.HasValue)
            {
                customHeaders.Add("X-Delete-After", deleteAfter.ToString());
            }


            if (!String.IsNullOrEmpty(contentEncoding))
                customHeaders.Add("Content-Encoding", contentEncoding);

            var cannedACL = acl == ACL.Auto ? GetDomainACL(domain) : ACL.Read;

            if (cannedACL == ACL.Read)
            {
                try
                {

                    using (var emptyStream = TempStream.Create())
                    {

                        var headers = new Dictionary<String, String>();

                        headers.Add("X-Object-Manifest", String.Format("{0}/{1}", _private_container, MakePath(domain, path)));
                        // create symlink
                        client.CreateObject(_public_container,
                                   emptyStream,
                                   MakePath(domain, path),
                                   mime,
                                   4096,
                                   headers,
                                   _region
                                  );

                        emptyStream.Close();
                    }

                    client.PurgeObjectFromCDN(_public_container, MakePath(domain, path));
                }
                catch (Exception exp)
                {
                    _logger.InfoFormat("The invalidation {0} failed", _public_container + "/" + MakePath(domain, path));
                    _logger.Error(exp);
                }            
            }
            stream.Position = 0;
            
            client.CreateObject(_private_container,
                                stream,
                                MakePath(domain, path),
                                mime,
                                4096,
                                customHeaders,
                                _region
                               );         
            
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
            var client = GetClient();
            var key = MakePath(domain, path);
            var size = GetFileSize(domain, path);

            client.DeleteObject(_private_container, MakePath(domain, path));

            QuotaUsedDelete(domain, size);

        }

        public override void DeleteFiles(string domain, string folderPath, string pattern, bool recursive)
        {
            var client = GetClient();

            var files = client.ListObjects(_private_container, null, null, null, MakePath(domain, folderPath), _region)
                              .Where(x => Wildcard.IsMatch(pattern, Path.GetFileName(x.Name)));

            if (!files.Any()) return;

            files.ToList().ForEach(x => client.DeleteObject(_private_container, x.Name));

            if (QuotaController != null)
            {
                QuotaUsedDelete(domain, files.Select(x => x.Bytes).Sum());
            }

        }

        public override void DeleteFiles(string domain, List<string> paths)
        {
            if (!paths.Any()) return;

            var keysToDel = new List<String>();

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

            var client = GetClient();

            keysToDel.ForEach(x => client.DeleteObject(_private_container, x));

            if (quotaUsed > 0)
            {
                QuotaUsedDelete(domain, quotaUsed);
            }
        }

        public override void DeleteFiles(string domain, string folderPath, DateTime fromDate, DateTime toDate)
        {
            var client = GetClient();

            var files = client.ListObjects(_private_container, null, null, null, MakePath(domain, folderPath), _region)
                               .Where(x => x.LastModified >= fromDate && x.LastModified <= toDate);

            if (!files.Any()) return;

            files.ToList().ForEach(x => client.DeleteObject(_private_container, x.Name));

            if (QuotaController != null)
            {
                QuotaUsedDelete(domain, files.Select(x => x.Bytes).Sum());
            }
        }

        public override void MoveDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            var client = GetClient();
            var srckey = MakePath(srcdomain, srcdir);
            var dstkey = MakePath(newdomain, newdir);

            var paths = client.ListObjects(_private_container, null, null, srckey, _region).Select(x => x.Name);

            foreach (var path in paths)
            {
                client.CopyObject(_private_container, path, _private_container, path.Replace(srckey, dstkey));
                client.DeleteObject(_private_container, path);
            }
        }

        public override Uri Move(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            var srcKey = MakePath(srcdomain, srcpath);
            var dstKey = MakePath(newdomain, newpath);
            var size = GetFileSize(srcdomain, srcpath);

            var client = GetClient();

            client.CopyObject(_private_container, srcKey, _private_container, dstKey);

            Delete(srcdomain, srcpath);

            QuotaUsedDelete(srcdomain, size);
            QuotaUsedAdd(newdomain, size);

            return GetUri(newdomain, newpath);
        }

        public override Uri SaveTemp(string domain, out string assignedPath, Stream stream)
        {
            assignedPath = Guid.NewGuid().ToString();

            return Save(domain, assignedPath, stream);
        }

        public override string[] ListDirectoriesRelative(string domain, string path, bool recursive)
        {
            var client = GetClient();

            return client.ListObjects(_private_container, null, null, null, MakePath(domain, path), _region)
                  .Select(x => x.Name.Substring(MakePath(domain, path + "/").Length)).ToArray();
        }

        public override string[] ListFilesRelative(string domain, string path, string pattern, bool recursive)
        {
            var paths = new List<String>();

            var client = GetClient();

            paths = client.ListObjects(_private_container, null, null, null, MakePath(domain, path), _region).Select(x => x.Name).ToList();

            return paths
                .Where(x => Wildcard.IsMatch(pattern, Path.GetFileName(x)))
                .Select(x => x.Substring(MakePath(domain, path + "/").Length).TrimStart('/')).ToArray();
        }

        public override bool IsFile(string domain, string path)
        {
            var client = GetClient();
            var objects = client.ListObjects(_private_container, null, null, null, MakePath(domain, path), _region);

            return objects.Count() > 0;
        }

        public override bool IsDirectory(string domain, string path)
        {
            return IsFile(domain, path);
        }

        public override void DeleteDirectory(string domain, string path)
        {
            var client = GetClient();

            var objToDel = client.ListObjects(_private_container, null, null, null, MakePath(domain, path), _region);

            foreach (var obj in objToDel)
            {
                client.DeleteObject(_private_container, obj.Name);
                QuotaUsedDelete(domain, Convert.ToInt64(obj.Bytes));
            }
        }

        public override long GetFileSize(string domain, string path)
        {
            var client = GetClient();

            var obj = client
                          .ListObjects(_private_container, null, null, null, MakePath(domain, path));

            if (obj.Any())
                return obj.Single().Bytes;

            return 0;
        }

        public override long GetDirectorySize(string domain, string path)
        {
            var client = GetClient();

            var objToDel = client
                          .ListObjects(_private_container, null, null, null, MakePath(domain, path));

            long result = 0;

            foreach (var obj in objToDel)
            {
                result += obj.Bytes;
            }

            return result;
        }

        public override long ResetQuota(string domain)
        {
            var client = GetClient();

            var objects = client
                          .ListObjects(_private_container, null, null, null, MakePath(domain, String.Empty), _region);

            if (QuotaController != null)
            {
                long size = 0;

                foreach (var obj in objects)
                {
                    size += obj.Bytes;
                }

                QuotaController.QuotaUsedSet(_modulename, domain, _dataList.GetData(domain), size);

                return size;
            }

            return 0;
        }

        public override long GetUsedQuota(string domain)
        {
            var client = GetClient();

            var objects = client
                          .ListObjects(_private_container, null, null, null, MakePath(domain, String.Empty), _region);

            long result = 0;

            foreach (var obj in objects)
            {
                result += obj.Bytes;
            }

            return result;
        }

        public override Uri Copy(string srcdomain, string path, string newdomain, string newpath)
        {
            var srcKey = MakePath(srcdomain, path);
            var dstKey = MakePath(newdomain, newpath);
            var size = GetFileSize(srcdomain, path);
            var client = GetClient();

            client.CopyObject(_private_container, srcKey, _private_container, dstKey);

            QuotaUsedAdd(newdomain, size);

            return GetUri(newdomain, newpath);
        }

        public override void CopyDirectory(string srcdomain, string dir, string newdomain, string newdir)
        {
            var srckey = MakePath(srcdomain, dir);
            var dstkey = MakePath(newdomain, newdir);
            var client = GetClient();

            var files = client.ListObjects(_private_container, null, null, null, srckey, _region);

            foreach (var file in files)
            {
                client.CopyObject(_private_container, file.Name, _private_container, file.Name.Replace(srckey, dstkey));

                QuotaUsedAdd(newdomain, file.Bytes);
            }
        }

        public override string SavePrivate(string domain, string path, Stream stream, DateTime expires)
        {
            var uri = Save(domain, path, stream, "application/octet-stream", "attachment", ACL.Auto, null, 5, expires);

            return uri.ToString();
        }

        public override void DeleteExpired(string domain, string path, TimeSpan oldThreshold)
        {
            // When the file is saved is specified life time
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
            int BufferSize = 4096;

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
            var client = GetClient();

            client.CreateObjectFromFile(_private_container, filePath, MakePath(domain, path));

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
