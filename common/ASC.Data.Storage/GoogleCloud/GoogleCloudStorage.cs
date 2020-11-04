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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Data.Storage.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using MimeMapping = ASC.Common.Web.MimeMapping;


#endregion

namespace ASC.Data.Storage.GoogleCloud
{
    public class GoogleCloudStorage : BaseStorage
    {
        private String _subDir = String.Empty;
        private readonly Dictionary<string, PredefinedObjectAcl> _domainsAcl;
        private readonly PredefinedObjectAcl _moduleAcl;

        private string _bucket = "";
        private string _json = "";

        private Uri _bucketRoot;
        private Uri _bucketSSlRoot;

        private bool _lowerCasing = true;

        public GoogleCloudStorage(string tenant)
        {
            _tenant = tenant;

            _modulename = string.Empty;
            _dataList = null;

            _domainsExpires = new Dictionary<string, TimeSpan> { { string.Empty, TimeSpan.Zero } };
            _domainsAcl = new Dictionary<string, PredefinedObjectAcl>();
            _moduleAcl = PredefinedObjectAcl.PublicRead;

        }

        public GoogleCloudStorage(string tenant, HandlerConfigurationElement handlerConfig, ModuleConfigurationElement moduleConfig)
        {
            _tenant = tenant;

            _modulename = moduleConfig.Name;
            _dataList = new DataList(moduleConfig);

            _domainsExpires =
                moduleConfig.Domains.Cast<DomainConfigurationElement>().Where(x => x.Expires != TimeSpan.Zero).
                    ToDictionary(x => x.Name,
                                 y => y.Expires);

            _domainsExpires.Add(string.Empty, moduleConfig.Expires);

            _domainsAcl = moduleConfig.Domains.Cast<DomainConfigurationElement>().ToDictionary(x => x.Name,
                                                                                   y => GetGoogleCloudAcl(y.Acl));
            _moduleAcl = GetGoogleCloudAcl(moduleConfig.Acl);

        }

        public override IDataStore Configure(IDictionary<string, string> props)
        {

            _bucket = props["bucket"];

            _bucketRoot = props.ContainsKey("cname") && Uri.IsWellFormedUriString(props["cname"], UriKind.Absolute)
                              ? new Uri(props["cname"], UriKind.Absolute)
                              : new Uri(string.Format("https://storage.googleapis.com/{0}/", _bucket), UriKind.Absolute);

            _bucketSSlRoot = props.ContainsKey("cnamessl") &&
                             Uri.IsWellFormedUriString(props["cnamessl"], UriKind.Absolute)
                                 ? new Uri(props["cnamessl"], UriKind.Absolute)
                                 : new Uri(string.Format("https://storage.googleapis.com/{0}/", _bucket), UriKind.Absolute);

            if (props.ContainsKey("lower"))
            {
                bool.TryParse(props["lower"], out _lowerCasing);
            }

            _json = props["json"];

            if (props.ContainsKey("subdir"))
            {
                _subDir = props["subdir"];
            }

            return this;
        }

        private StorageClient GetStorage()
        {
            var credential = GoogleCredential.FromJson(_json);

            return StorageClient.Create(credential);
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


        public static long DateToUnixTimestamp(DateTime date)
        {
            var ts = date - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)ts.TotalSeconds;
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

            var storage = GetStorage();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(_json ?? "")))
            {
                var preSignedURL = UrlSigner.FromServiceAccountData(stream)
                                            .Sign(_bucket, MakePath(domain, path), expire, HttpMethod.Get);

                return MakeUri(preSignedURL);
            }
        }

        public Uri GetUriShared(string domain, string path)
        {
            return new Uri(SecureHelper.IsSecure() ? _bucketSSlRoot : _bucketRoot, MakePath(domain, path));
        }

        private Uri MakeUri(string preSignedURL)
        {
            var uri = new Uri(preSignedURL);
            var signedPart = uri.PathAndQuery.TrimStart('/');

            return new Uri(uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? _bucketSSlRoot : _bucketRoot, signedPart);

        }

        public override System.IO.Stream GetReadStream(string domain, string path)
        {
            return GetReadStream(domain, path, 0);
        }

        public override System.IO.Stream GetReadStream(string domain, string path, int offset)
        {
            var tempStream = TempStream.Create();

            var storage = GetStorage();

            storage.DownloadObject(_bucket, MakePath(domain, path), tempStream, null, null);

            if (offset > 0)
                tempStream.Seek(offset, SeekOrigin.Begin);

            tempStream.Position = 0;

            return tempStream;
        }

        public override Uri Save(string domain, string path, System.IO.Stream stream)
        {
            return Save(domain, path, stream, string.Empty, string.Empty);
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
            return Save(domain, path, stream, null, contentDisposition);
        }

        public override Uri Save(string domain, string path, System.IO.Stream stream, string contentType, string contentDisposition)
        {
            return Save(domain, path, stream, contentType, contentDisposition, ACL.Auto);
        }

        public override Uri Save(string domain, string path, System.IO.Stream stream, string contentEncoding, int cacheDays)
        {
            return Save(domain, path, stream, string.Empty, string.Empty, ACL.Auto, contentEncoding, cacheDays);
        }

        public Uri Save(string domain, string path, Stream stream, string contentType,
                          string contentDisposition, ACL acl, string contentEncoding = null, int cacheDays = 5)
        {

            var buffered = stream.GetBuffered();

            if (QuotaController != null)
            {
                QuotaController.QuotaUsedCheck(buffered.Length);
            }

            var mime = string.IsNullOrEmpty(contentType)
                        ? MimeMapping.GetMimeMapping(Path.GetFileName(path))
                        : contentType;

            var storage = GetStorage();

            UploadObjectOptions uploadObjectOptions = new UploadObjectOptions
            {
                PredefinedAcl = acl == ACL.Auto ? GetDomainACL(domain) : GetGoogleCloudAcl(acl)
            };

            buffered.Position = 0;

            var uploaded = storage.UploadObject(_bucket, MakePath(domain, path), mime, buffered, uploadObjectOptions, null);

            uploaded.ContentEncoding = contentEncoding;
            uploaded.CacheControl = String.Format("public, maxage={0}", (int)TimeSpan.FromDays(cacheDays).TotalSeconds);

            if (uploaded.Metadata == null)
                uploaded.Metadata = new Dictionary<String, String>();

            uploaded.Metadata["Expires"] = DateTime.UtcNow.Add(TimeSpan.FromDays(cacheDays)).ToString("R");

            if (!string.IsNullOrEmpty(contentDisposition))
            {
                uploaded.ContentDisposition = contentDisposition;
            }
            else if (mime == "application/octet-stream")
            {
                uploaded.ContentDisposition = "attachment";
            }

            storage.UpdateObject(uploaded);

            //           InvalidateCloudFront(MakePath(domain, path));

            QuotaUsedAdd(domain, buffered.Length);

            return GetUri(domain, path);
        }

        private void InvalidateCloudFront(params string[] paths)
        {
            throw new NotImplementedException();
        }

        private PredefinedObjectAcl GetGoogleCloudAcl(ACL acl)
        {
            switch (acl)
            {
                case ACL.Read:
                    return PredefinedObjectAcl.PublicRead;
                default:
                    return PredefinedObjectAcl.PublicRead;
            }
        }

        private PredefinedObjectAcl GetDomainACL(string domain)
        {
            if (GetExpire(domain) != TimeSpan.Zero)
            {
                return PredefinedObjectAcl.Private;
            }

            if (_domainsAcl.ContainsKey(domain))
            {
                return _domainsAcl[domain];
            }
            return _moduleAcl;
        }

        public override void Delete(string domain, string path)
        {
            var storage = GetStorage();

            var key = MakePath(domain, path);
            var size = GetFileSize(domain, path);

            storage.DeleteObject(_bucket, key);

            QuotaUsedDelete(domain, size);
        }


        public override void DeleteFiles(string domain, string folderPath, string pattern, bool recursive)
        {
            var storage = GetStorage();

            IEnumerable<Google.Apis.Storage.v1.Data.Object> objToDel;

            if (recursive)
                objToDel = storage
                           .ListObjects(_bucket, MakePath(domain, folderPath))
                           .Where(x => Wildcard.IsMatch(pattern, Path.GetFileName(x.Name)));
            else
                objToDel = new List<Google.Apis.Storage.v1.Data.Object>();

            foreach (var obj in objToDel)
            {
                storage.DeleteObject(_bucket, obj.Name);
                QuotaUsedDelete(domain, Convert.ToInt64(obj.Size));
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

            var storage = GetStorage();

            keysToDel.ForEach(x => storage.DeleteObject(_bucket, x));

            if (quotaUsed > 0)
            {
                QuotaUsedDelete(domain, quotaUsed);
            }
        }

        public override void DeleteFiles(string domain, string folderPath, DateTime fromDate, DateTime toDate)
        {
            var storage = GetStorage();

            var objToDel = GetObjects(domain, folderPath, true)
                              .Where(x => x.Updated >= fromDate && x.Updated <= toDate);

            foreach (var obj in objToDel)
            {
                storage.DeleteObject(_bucket, obj.Name);
                QuotaUsedDelete(domain, Convert.ToInt64(obj.Size));
            }
        }

        public override void MoveDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            var storage = GetStorage();

            var srckey = MakePath(srcdomain, srcdir);
            var dstkey = MakePath(newdomain, newdir);

            var objects = storage.ListObjects(_bucket, srckey);

            foreach (var obj in objects)
            {
                storage.CopyObject(_bucket, srckey, _bucket, dstkey, new CopyObjectOptions
                {
                    DestinationPredefinedAcl = GetDomainACL(newdomain)
                });

                storage.DeleteObject(_bucket, srckey);

            }
        }

        public override Uri Move(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            var storage = GetStorage();

            var srcKey = MakePath(srcdomain, srcpath);
            var dstKey = MakePath(newdomain, newpath);
            var size = GetFileSize(srcdomain, srcpath);

            storage.CopyObject(_bucket, srcKey, _bucket, dstKey, new CopyObjectOptions
            {
                DestinationPredefinedAcl = GetDomainACL(newdomain)
            });

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
            return GetObjects(domain, path, recursive)
                   .Select(x => x.Name.Substring(MakePath(domain, path + "/").Length))
                   .ToArray();
        }

        private IEnumerable<Google.Apis.Storage.v1.Data.Object> GetObjects(string domain, string path, bool recursive)
        {
            var storage = GetStorage();

            var items = storage.ListObjects(_bucket, MakePath(domain, path));

            if (recursive) return items;

            return items.Where(x => x.Name.IndexOf('/', MakePath(domain, path + "/").Length) == -1);
        }

        public override string[] ListFilesRelative(string domain, string path, string pattern, bool recursive)
        {
            return GetObjects(domain, path, recursive).Where(x => Wildcard.IsMatch(pattern, Path.GetFileName(x.Name)))
                   .Select(x => x.Name.Substring(MakePath(domain, path + "/").Length).TrimStart('/')).ToArray();
        }

        public override bool IsFile(string domain, string path)
        {
            var storage = GetStorage();

            var objects = storage.ListObjects(_bucket, MakePath(domain, path), null);

            return objects.Count() > 0;
        }

        public override bool IsDirectory(string domain, string path)
        {
            return IsFile(domain, path);
        }

        public override void DeleteDirectory(string domain, string path)
        {
            var storage = GetStorage();

            var objToDel = storage
                          .ListObjects(_bucket, MakePath(domain, path));

            foreach (var obj in objToDel)
            {
                storage.DeleteObject(_bucket, obj.Name);
                QuotaUsedDelete(domain, Convert.ToInt64(obj.Size));
            }
        }

        public override long GetFileSize(string domain, string path)
        {
            var storage = GetStorage();

            var obj = storage.GetObject(_bucket, MakePath(domain, path));

            if (obj.Size.HasValue)
                return Convert.ToInt64(obj.Size.Value);

            return 0;
        }

        public override long GetDirectorySize(string domain, string path)
        {
            var storage = GetStorage();

            var objToDel = storage
                          .ListObjects(_bucket, MakePath(domain, path));

            long result = 0;

            foreach (var obj in objToDel)
            {
                if (obj.Size.HasValue)
                    result += Convert.ToInt64(obj.Size.Value);
            }

            return result;
        }

        public override long ResetQuota(string domain)
        {
            var storage = GetStorage();

            var objects = storage
                          .ListObjects(_bucket, MakePath(domain, String.Empty));

            if (QuotaController != null)
            {
                long size = 0;

                foreach (var obj in objects)
                {
                    if (obj.Size.HasValue)
                        size += Convert.ToInt64(obj.Size.Value);
                }

                QuotaController.QuotaUsedSet(_modulename, domain, _dataList.GetData(domain), size);

                return size;
            }

            return 0;
        }

        public override long GetUsedQuota(string domain)
        {
            var storage = GetStorage();

            var objects = storage
                          .ListObjects(_bucket, MakePath(domain, String.Empty));

            long result = 0;

            foreach (var obj in objects)
            {
                if (obj.Size.HasValue)
                    result += Convert.ToInt64(obj.Size.Value);
            }

            return result;
        }

        public override Uri Copy(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            var storage = GetStorage();

            var srcKey = MakePath(srcdomain, srcpath);
            var dstKey = MakePath(newdomain, newpath);

            var size = GetFileSize(srcdomain, srcpath);

            CopyObjectOptions options = new CopyObjectOptions();

            options.DestinationPredefinedAcl = GetDomainACL(newdomain);

            storage.CopyObject(_bucket, MakePath(srcdomain, srcpath), _bucket, MakePath(newdomain, newpath), options);

            QuotaUsedAdd(newdomain, size);

            return GetUri(newdomain, newpath);
        }

        public override void CopyDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            var srckey = MakePath(srcdomain, srcdir);
            var dstkey = MakePath(newdomain, newdir);
            //List files from src

            var storage = GetStorage();


            var options = new ListObjectsOptions();

            var objects = storage.ListObjects(_bucket, srckey);

            foreach (var obj in objects)
            {
                storage.CopyObject(_bucket, srckey, _bucket, dstkey, new CopyObjectOptions
                {
                    DestinationPredefinedAcl = GetDomainACL(newdomain)
                });


                QuotaUsedAdd(newdomain, Convert.ToInt64(obj.Size));
            }
        }

        public override string SavePrivate(string domain, string path, System.IO.Stream stream, DateTime expires)
        {
            var storage = GetStorage();

            var objectKey = MakePath(domain, path);
            var buffered = stream.GetBuffered();

            UploadObjectOptions uploadObjectOptions = new UploadObjectOptions
            {
                PredefinedAcl = PredefinedObjectAcl.BucketOwnerFullControl
            };

            buffered.Position = 0;

            var uploaded = storage.UploadObject(_bucket, MakePath(domain, path), "application/octet-stream", buffered, uploadObjectOptions, null);

            uploaded.CacheControl = String.Format("public, maxage={0}", (int)TimeSpan.FromDays(5).TotalSeconds);
            uploaded.ContentDisposition = "attachment";

            if (uploaded.Metadata == null)
                uploaded.Metadata = new Dictionary<String, String>();

            uploaded.Metadata["Expires"] = DateTime.UtcNow.Add(TimeSpan.FromDays(5)).ToString("R");
            uploaded.Metadata.Add("private-expire", expires.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture));

            storage.UpdateObject(uploaded);

            using (var mStream = new MemoryStream(Encoding.UTF8.GetBytes(_json ?? "")))
            {
                var preSignedURL = UrlSigner.FromServiceAccountData(mStream)
                                .Sign(_bucket, MakePath(domain, path), expires, null);

                //TODO: CNAME!
                return preSignedURL;
            }
        }

        public override void DeleteExpired(string domain, string path, TimeSpan oldThreshold)
        {
            var storage = GetStorage();

            var objects = storage.ListObjects(_bucket, MakePath(domain, path));

            foreach (var obj in objects)
            {
                var objInfo = storage.GetObject(_bucket, MakePath(domain, path), null);

                var privateExpireKey = objInfo.Metadata["private-expire"];

                if (string.IsNullOrEmpty(privateExpireKey)) continue;

                long fileTime;

                if (!long.TryParse(privateExpireKey, out fileTime)) continue;
                if (DateTime.UtcNow <= DateTime.FromFileTimeUtc(fileTime)) continue;

                storage.DeleteObject(_bucket, MakePath(domain, path));

            }
        }

        #region chunking

        public override String InitiateChunkedUpload(string domain, string path)
        {
            var storage = GetStorage();

            var tempUploader = storage.CreateObjectUploader(_bucket, MakePath(domain, path), null, new MemoryStream());

            var sessionUri = tempUploader.InitiateSessionAsync().Result;

            return sessionUri.ToString();
        }

        public override string UploadChunk(String domain,
                                           String path,
                                           String uploadUri,
                                           Stream stream,
                                           long defaultChunkSize,
                                           int chunkNumber,
                                           long chunkLength)
        {

            String bytesRangeStart = Convert.ToString((chunkNumber - 1) * defaultChunkSize);
            String bytesRangeEnd = Convert.ToString((chunkNumber - 1) * defaultChunkSize + chunkLength - 1);

            String totalBytes = "*";

            int BufferSize = 2 * 4096;

            if (chunkLength != defaultChunkSize)
                totalBytes = Convert.ToString((chunkNumber - 1) * defaultChunkSize + chunkLength);

            String contentRangeHeader = String.Format("bytes {0}-{1}/{2}", bytesRangeStart, bytesRangeEnd, totalBytes);

            var request = HttpWebRequest.CreateHttp(uploadUri);

            request.Method = HttpMethod.Put.ToString();
            request.ContentLength = chunkLength;
            request.Headers.Add("Content-Range", contentRangeHeader);

            using (Stream rs = request.GetRequestStream())
            {
                var buffer = new byte[BufferSize];

                int readed;

                while ((readed = stream.Read(buffer, 0, BufferSize)) != 0)
                {
                    rs.Write(buffer, 0, readed);
                }

                stream.Close();
            }

            long MAX_RETRIES = 100;
            int millisecondsTimeout;

            for (int i = 0; i < MAX_RETRIES; i++)
            {
                Random random = new Random();

                millisecondsTimeout = Math.Min(Convert.ToInt32(Math.Pow(2, i)) + random.Next(0, 1000), 32 * 1000);

                try
                {
                    var response = request.GetResponse();
                    var status = ((HttpWebResponse)response).StatusCode;

                    break;
                }
                catch (WebException ex)
                {
                    var response = (HttpWebResponse)ex.Response;

                    var status = (int)response.StatusCode;

                    if (status == 408 || status == 500 || status == 502 || status == 503 || status == 504)
                    {
                        Thread.Sleep(millisecondsTimeout);
                        continue;
                    }

                    if ((int)status != 308)
                        throw (ex);

                    break;
                }
                catch (Exception ex)
                {
                    AbortChunkedUpload(domain, path, uploadUri);
                    throw (ex);
                }
            }

            return String.Empty;
        }

        public override Uri FinalizeChunkedUpload(string domain, string path, string uploadUri, Dictionary<int, string> eTags)
        {
            if (QuotaController != null)
            {
                var size = GetFileSize(domain, path);
                QuotaUsedAdd(domain, size);
            }

            return GetUri(domain, path);
        }

        public override void AbortChunkedUpload(string domain, string path, string uploadUri)
        {

        }

        public override bool IsSupportChunking { get { return true; } }

        #endregion

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
    }
}

