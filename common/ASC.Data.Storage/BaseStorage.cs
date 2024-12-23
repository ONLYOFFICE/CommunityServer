/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using ASC.Core;
using ASC.Core.ChunkedUploader;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.ZipOperators;
using ASC.Security.Cryptography;

namespace ASC.Data.Storage
{
    public abstract class BaseStorage : IDataStore
    {
        #region IDataStore Members

        internal string _modulename;
        internal bool _cache;
        internal bool _attachment;
        internal DataList _dataList;
        internal string _tenant;
        internal Dictionary<string, TimeSpan> _domainsExpires = new Dictionary<string, TimeSpan>();
        internal Dictionary<string, IDataStoreValidator> _domainsValidators = new Dictionary<string, IDataStoreValidator>();

        public IQuotaController QuotaController { get; set; }

        public TimeSpan GetExpire(string domain)
        {
            return _domainsExpires.ContainsKey(domain) ? _domainsExpires[domain] : _domainsExpires[string.Empty];
        }

        public IDataStoreValidator GetValidator(string domain)
        {
            return _domainsValidators.ContainsKey(domain) ? _domainsValidators[domain] : _domainsValidators[string.Empty];
        }

        protected IDataStoreValidator CreateValidator(string type, string param)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            var validatorType = Type.GetType(type, false);

            return validatorType == null ? null : (IDataStoreValidator)Activator.CreateInstance(validatorType, param);
        }

        public Uri GetUri(string path)
        {
            return GetUri(string.Empty, path);
        }

        public Uri GetUri(string domain, string path)
        {
            return GetPreSignedUri(domain, path, TimeSpan.MaxValue, null);
        }

        public Uri GetPreSignedUri(string domain, string path, TimeSpan expire, IEnumerable<string> headers)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrEmpty(_tenant) && IsSupportInternalUri)
            {
                return GetInternalUri(domain, path, expire, headers);
            }

            var headerAttr = string.Empty;
            if (headers != null)
            {
                headerAttr = string.Join("&", headers.Select(HttpUtility.UrlEncode));
            }

            if (expire == TimeSpan.Zero || expire == TimeSpan.MinValue || expire == TimeSpan.MaxValue)
            {
                expire = GetExpire(domain);
            }

            var query = string.Empty;
            if (expire != TimeSpan.Zero && expire != TimeSpan.MinValue && expire != TimeSpan.MaxValue)
            {
                var expireString = expire.TotalMinutes.ToString(CultureInfo.InvariantCulture);

                int currentTenantId;
                var currentTenant = CoreContext.TenantManager.GetCurrentTenant(false);
                if (currentTenant != null)
                {
                    currentTenantId = currentTenant.TenantId;
                }
                else if (!TenantPath.TryGetTenant(_tenant, out currentTenantId))
                {
                    currentTenantId = 0;
                }

                var auth = EmailValidationKeyProvider.GetEmailKey(currentTenantId, path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) + "." + headerAttr + "." + expireString);
                query = string.Format("{0}{1}={2}&{3}={4}",
                                      path.Contains("?") ? "&" : "?",
                                      Constants.QUERY_EXPIRE,
                                      expireString,
                                      Constants.QUERY_AUTH,
                                      auth);
            }

            if (!string.IsNullOrEmpty(headerAttr))
            {
                query += string.Format("{0}{1}={2}",
                                       query.Contains("?") ? "&" : "?",
                                       Constants.QUERY_HEADER,
                                       HttpUtility.UrlEncode(headerAttr));
            }

            var tenant = _tenant.Trim('/');
            var vpath = PathUtils.ResolveVirtualPath(_modulename, domain);
            vpath = PathUtils.ResolveVirtualPath(vpath, false);
            vpath = string.Format(vpath, tenant);
            var virtualPath = new Uri(vpath + "/", UriKind.RelativeOrAbsolute);

            var uri = virtualPath.IsAbsoluteUri ?
                          new MonoUri(virtualPath, virtualPath.LocalPath.TrimEnd('/') + EnsureLeadingSlash(path.Replace('\\', '/')) + query) :
                          new MonoUri(virtualPath.ToString().TrimEnd('/') + EnsureLeadingSlash(path.Replace('\\', '/')) + query, UriKind.Relative);

            return uri;
        }

        public virtual bool IsSupportInternalUri
        {
            get { return true; }
        }

        public virtual bool IsContentDispositionAsAttachment
        {
            get { return _attachment; }
        }

        public virtual Uri GetInternalUri(string domain, string path, TimeSpan expire, IEnumerable<string> headers)
        {
            return null;
        }

        public abstract Stream GetReadStream(string domain, string path);
        public abstract Stream GetReadStream(string domain, string path, long offset);
        public abstract Task<Stream> GetReadStreamAsync(string domain, string path, long offset);

        public abstract Uri Save(string domain, string path, Stream stream);
        public abstract Uri Save(string domain, string path, Stream stream, Guid ownerId);
        public abstract Uri Save(string domain, string path, Stream stream, ACL acl);

        public Uri Save(string domain, string path, Stream stream, string attachmentFileName)
        {
            return Save(domain, path, Guid.Empty, stream, attachmentFileName);
        }

        public Uri Save(string domain, string path, Guid ownerId, Stream stream, string attachmentFileName)
        {
            if (!string.IsNullOrEmpty(attachmentFileName))
            {
                return SaveWithAutoAttachment(domain, path, ownerId, stream, attachmentFileName);
            }
            return Save(domain, path, stream, ownerId);
        }

        protected abstract Uri SaveWithAutoAttachment(string domain, string path, Stream stream, string attachmentFileName);
        protected abstract Uri SaveWithAutoAttachment(string domain, string path, Guid ownerId, Stream stream, string attachmentFileName);

        public abstract Uri Save(string domain, string path, Stream stream, string contentType,
                                 string contentDisposition);
        public abstract Uri Save(string domain, string path, Guid ownerId, Stream stream, string contentType,
                                 string contentDisposition);
        public abstract Uri Save(string domain, string path, Stream stream, string contentEncoding, int cacheDays);

        public virtual bool IsSupportedPreSignedUri
        {
            get
            {
                return true;
            }
        }

        #region chunking

        public virtual string InitiateChunkedUpload(string domain, string path)
        {
            throw new NotImplementedException();
        }

        public virtual string UploadChunk(string domain, string path, string uploadId, Stream stream, long defaultChunkSize, int chunkNumber, long chunkLength)
        {
            throw new NotImplementedException();
        }

        public virtual Task<string> UploadChunkAsync(string domain, string path, string uploadId, Stream stream, long defaultChunkSize, int chunkNumber, long chunkLength)
        {
            throw new NotImplementedException();
        }

        public virtual Uri FinalizeChunkedUpload(string domain, string path, string uploadId, Dictionary<int, string> eTags)
        {
            throw new NotImplementedException();
        }

        public virtual void AbortChunkedUpload(string domain, string path, string uploadId)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsSupportChunking { get { return false; } }

        public virtual IDataWriteOperator CreateDataWriteOperator(
            CommonChunkedUploadSession chunkedUploadSession,
            CommonChunkedUploadSessionHolder sessionHolder)
        {
            return null;
        }

        #endregion

        public abstract void Delete(string domain, string path);
        public abstract void DeleteFiles(string domain, string folderPath, string pattern, bool recursive, Guid ownerId);
        public abstract void DeleteFiles(string domain, string folderPath, string pattern, bool recursive);
        public abstract void DeleteFiles(string domain, List<string> paths);
        public abstract void DeleteFiles(string domain, string folderPath, DateTime fromDate, DateTime toDate);
        public abstract void MoveDirectory(string srcdomain, string srcdir, string newdomain, string newdir);
        public abstract Uri Move(string srcdomain, string srcpath, string newdomain, string newpath, bool quotaCheckFileSize = true);
        public abstract Uri Move(string srcdomain, string srcpath, string newdomain, string newpath, Guid ownerId, bool quotaCheckFileSize = true);
        public abstract Uri SaveTemp(string domain, out string assignedPath, Stream stream);
        public abstract string[] ListDirectoriesRelative(string domain, string path, bool recursive);
        public abstract IEnumerable<string> ListFilesRelative(string domain, string path, string pattern, bool recursive);
        public abstract bool IsFile(string domain, string path);
        public abstract Task<bool> IsFileAsync(string domain, string path);
        public abstract bool IsDirectory(string domain, string path);
        public abstract void DeleteDirectory(string domain, string path, Guid ownerId);
        public abstract void DeleteDirectory(string domain, string path);
        public abstract long GetFileSize(string domain, string path);
        public abstract Task<long> GetFileSizeAsync(string domain, string path);
        public abstract long GetDirectorySize(string domain, string path);
        public abstract long ResetQuota(string domain);
        public abstract long GetUsedQuota(string domain);
        public abstract Uri Copy(string srcdomain, string path, string newdomain, string newpath);
        public abstract void CopyDirectory(string srcdomain, string dir, string newdomain, string newdir);


        public Stream GetReadStream(string path)
        {
            return GetReadStream(string.Empty, path);
        }

        public Uri Save(string path, Stream stream, string attachmentFileName)
        {
            return Save(string.Empty, path, stream, attachmentFileName);
        }

        public Uri Save(string path, Stream stream)
        {
            return Save(string.Empty, path, stream);
        }

        public void Delete(string path)
        {
            Delete(string.Empty, path);
        }

        public void DeleteFiles(string folderPath, string pattern, bool recursive)
        {
            DeleteFiles(string.Empty, folderPath, pattern, recursive);
        }

        public Uri Move(string srcpath, string newdomain, string newpath)
        {
            return Move(string.Empty, srcpath, newdomain, newpath);
        }

        public Uri SaveTemp(out string assignedPath, Stream stream)
        {
            return SaveTemp(string.Empty, out assignedPath, stream);
        }

        public string[] ListDirectoriesRelative(string path, bool recursive)
        {
            return ListDirectoriesRelative(string.Empty, path, recursive);
        }

        public Uri[] ListFiles(string path, string pattern, bool recursive)
        {
            return ListFiles(string.Empty, path, pattern, recursive);
        }

        public Uri[] ListFiles(string domain, string path, string pattern, bool recursive)
        {
            var filePaths = ListFilesRelative(domain, path, pattern, recursive).ToArray();
            return Array.ConvertAll(
                filePaths,
                x => GetUri(domain, Path.Combine(PathUtils.Normalize(path), x)));
        }

        public bool IsFile(string path)
        {
            return IsFile(string.Empty, path);
        }

        public bool IsDirectory(string path)
        {
            return IsDirectory(string.Empty, path);
        }
        public void DeleteDirectory(Guid ownerId, string path)
        {
            DeleteDirectory(string.Empty, path, ownerId);
        }
        public void DeleteDirectory(string path)
        {
            DeleteDirectory(string.Empty, path);
        }

        public long GetFileSize(string path)
        {
            return GetFileSize(string.Empty, path);
        }

        public long GetDirectorySize(string path)
        {
            return GetDirectorySize(string.Empty, path);
        }


        public Uri Copy(string path, string newdomain, string newpath)
        {
            return Copy(string.Empty, path, newdomain, newpath);
        }

        public void CopyDirectory(string dir, string newdomain, string newdir)
        {
            CopyDirectory(string.Empty, dir, newdomain, newdir);
        }

        public virtual IDataStore Configure(IDictionary<string, string> props)
        {
            return this;
        }

        public IDataStore SetQuotaController(IQuotaController controller)
        {
            QuotaController = controller;
            return this;
        }

        public abstract string SavePrivate(string domain, string path, Stream stream, DateTime expires);
        public abstract void DeleteExpired(string domain, string path, TimeSpan oldThreshold);

        public abstract string GetUploadForm(string domain, string directoryPath, string redirectTo, long maxUploadSize,
                                             string contentType, string contentDisposition, string submitLabel);

        public abstract string GetUploadUrl();

        public abstract string GetPostParams(string domain, string directoryPath, long maxUploadSize, string contentType,
                                             string contentDisposition);

        #endregion
        internal void QuotaUsedAdd(string domain, long size, bool quotaCheckFileSize = true)
        {
            QuotaUsedAdd(domain, size, Guid.Empty, quotaCheckFileSize);
        }
        internal void QuotaUsedAdd(string domain, long size, Guid ownerId, bool quotaCheckFileSize = true)
        {
            if (QuotaController != null)
            {
                QuotaController.QuotaUsedAdd(_modulename, domain, _dataList.GetData(domain), size, ownerId, quotaCheckFileSize);
            }
        }
        internal void QuotaUsedDelete(string domain, long size)
        {
            QuotaUsedDelete(domain, size, Guid.Empty);
        }
        internal void QuotaUsedDelete(string domain, long size, Guid ownerId)
        {
            if (QuotaController != null)
            {
                QuotaController.QuotaUsedDelete(_modulename, domain, _dataList.GetData(domain), size, ownerId);
            }
        }

        internal static string EnsureLeadingSlash(string str)
        {
            return "/" + str.TrimStart('/');
        }

        protected abstract DateTime GetLastModificationDate(string domain, string path);

        public bool TryGetFileEtag(string domain, string path, out string etag)
        {
            etag = "";

            if (_cache)
            {
                var lastModificationDate = GetLastModificationDate(domain, path);
                etag = '"' + lastModificationDate.Ticks.ToString("X8", CultureInfo.InvariantCulture) + '"';
                return true;
            }

            return false;
        }

        internal class MonoUri : Uri
        {
            public MonoUri(Uri baseUri, string relativeUri)
                : base(baseUri, relativeUri)
            {
            }

            public MonoUri(string uriString, UriKind uriKind)
                : base(uriString, uriKind)
            {
            }

            public override string ToString()
            {
                var s = base.ToString();
                if (WorkContext.IsMono && s.StartsWith(UriSchemeFile + SchemeDelimiter))
                {
                    return s.Substring(7);
                }
                return s;
            }
        }
    }
}