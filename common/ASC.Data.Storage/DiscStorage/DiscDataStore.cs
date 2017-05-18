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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Storage.DiscStorage
{
    public class DiscDataStore : BaseStorage
    {
        private const int BufferSize = 4096;
        private readonly Dictionary<string, MappedPath> _mappedPaths = new Dictionary<string, MappedPath>();


        public DiscDataStore(string tenant, HandlerConfigurationElement handlerConfig, ModuleConfigurationElement moduleConfig)
        {
            //Fill map path
            _modulename = moduleConfig.Name;
            _dataList = new DataList(moduleConfig);
            foreach (DomainConfigurationElement domain in moduleConfig.Domains)
            {
                _mappedPaths.Add(domain.Name, new MappedPath(tenant, moduleConfig.AppendTenant, domain.Path, domain.VirtualPath, handlerConfig.GetProperties()));
            }
            //Add default
            _mappedPaths.Add(string.Empty, new MappedPath(tenant, moduleConfig.AppendTenant, PathUtils.Normalize(moduleConfig.Path), moduleConfig.VirtualPath, handlerConfig.GetProperties()));
        }

        public String GetPhysicalPath(string domain, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var pathMap = GetPath(domain);

            return (pathMap.PhysicalPath + EnsureLeadingSlash(path)).Replace('\\', '/');
        }

        public override Uri GetPreSignedUri(string domain, string path, TimeSpan expire, IEnumerable<string> headers)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var pathMap = GetPath(domain);
            var uri = pathMap.VirtualPath.IsAbsoluteUri ?
                new MonoUri(pathMap.VirtualPath, pathMap.VirtualPath.LocalPath.TrimEnd('/') + EnsureLeadingSlash(path.Replace('\\', '/'))) :
                new MonoUri(pathMap.VirtualPath.ToString().TrimEnd('/') + EnsureLeadingSlash(path.Replace('\\', '/')), UriKind.Relative);

            return uri;
        }

        private static string EnsureLeadingSlash(string str)
        {
            return "/" + str.TrimStart('/');
        }

        public override Stream GetReadStream(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            var target = GetTarget(domain, path);

            if (File.Exists(target))
            {
                return File.OpenRead(target);
            }
            throw new FileNotFoundException("File not found", Path.GetFullPath(target));
        }


        public override Stream GetReadStream(string domain, string path, int offset)
        {
            if (path == null) throw new ArgumentNullException("path");
            var target = GetTarget(domain, path);

            if (File.Exists(target))
            {
                FileStream stream = File.OpenRead(target);
                if (0 < offset) stream.Seek(offset, SeekOrigin.Begin);
                return stream;
            }
            throw new FileNotFoundException("File not found", Path.GetFullPath(target));
        }

        protected override Uri SaveWithAutoAttachment(string domain, string path, Stream stream, string attachmentFileName)
        {
            return Save(domain, path, stream);
        }

        public override Uri Save(string domain, string path, Stream stream, string contentType, string contentDisposition)
        {
            return Save(domain, path, stream);
        }

        public override Uri Save(string domain, string path, Stream stream, string contentEncoding, int cacheDays)
        {
            return Save(domain, path, stream);

        }

        public override Uri Save(string domain, string path, Stream stream)
        {
            var buffered = stream.GetBuffered();
            if (QuotaController != null)
            {
                QuotaController.QuotaUsedCheck(buffered.Length);
            }

            if (path == null) throw new ArgumentNullException("path");
            if (buffered == null) throw new ArgumentNullException("stream");

            //Try seek to start
            if (buffered.CanSeek)
            {
                buffered.Seek(0, SeekOrigin.Begin);
            }
            //Lookup domain
            var target = GetTarget(domain, path);
            CreateDirectory(target);
            //Copy stream

            //optimaze disk file copy
            var fileStream = buffered as FileStream;
            long fslen;
            if (fileStream != null && WorkContext.IsMono)
            {
                File.Copy(fileStream.Name, target, true);
                fslen = fileStream.Length;
            }
            else
            {
                using (var fs = File.Open(target, FileMode.Create))
                {
                    var buffer = new byte[BufferSize];
                    int readed;
                    while ((readed = buffered.Read(buffer, 0, BufferSize)) != 0)
                    {
                        fs.Write(buffer, 0, readed);
                    }
                    fslen = fs.Length;
                }
            }

            QuotaUsedAdd(domain, fslen);

            return GetUri(domain, path);
        }

        public override Uri Save(string domain, string path, Stream stream, ACL acl)
        {
            return Save(domain, path, stream);
        }

        #region chunking

        public override string InitiateChunkedUpload(string domain, string path)
        {
            var target = GetTarget(domain, path);
            CreateDirectory(target);
            return target;
        }

        public override string UploadChunk(string domain, string path, string uploadId, Stream stream, int chunkNumber, long chunkLength)
        {
            var target = GetTarget(domain, path);
            var mode = chunkNumber == 0 ? FileMode.Create : FileMode.Append;
            using (var fs = new FileStream(target, mode))
            {
                var buffer = new byte[BufferSize];
                int readed;
                while ((readed = stream.Read(buffer, 0, BufferSize)) != 0)
                {
                    fs.Write(buffer, 0, readed);
                }
            }
            return string.Format("{0}_{1}", chunkNumber, uploadId);
        }

        public override Uri FinalizeChunkedUpload(string domain, string path, string uploadId, Dictionary<int, string> eTags)
        {
            if (QuotaController != null)
            {
                var size = GetFileSize(domain, path);
                QuotaUsedAdd(domain, size);
            }
            return GetUri(domain, path);
        }

        public override void AbortChunkedUpload(string domain, string path, string uploadId)
        {
            var target = GetTarget(domain, path);
            if (File.Exists(target))
            {
                File.Delete(target);
            }
        }

        public override bool IsSupportChunking
        {
            get { return true; }
        }

        #endregion

        public override void Delete(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            var target = GetTarget(domain, path);

            if (File.Exists(target))
            {
                var size = new FileInfo(target).Length;
                File.Delete(target);

                QuotaUsedDelete(domain, size);
            }
            else
            {
                throw new FileNotFoundException("file not found", target);
            }
        }

        public override void DeleteFiles(string domain, List<string> paths)
        {
            if (paths == null) throw new ArgumentNullException("paths");

            foreach (var path in paths)
            {
                var target = GetTarget(domain, path);

                if (!File.Exists(target))
                    continue;

                var obj = new FileInfo(target);

                File.Delete(target);

                QuotaUsedDelete(domain, obj.Length);
            }
        }

        public override void DeleteFiles(string domain, string folderPath, string pattern, bool recursive)
        {
            if (folderPath == null) throw new ArgumentNullException("folderPath");

            //Return dirs
            var targetDir = GetTarget(domain, folderPath);
            if (Directory.Exists(targetDir))
            {
                var entries = Directory.GetFiles(targetDir, pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (var entry in entries)
                {
                    var size = new FileInfo(entry).Length;
                    File.Delete(entry);
                    QuotaUsedDelete(domain, size);
                }
            }
            else
            {
                throw new DirectoryNotFoundException(string.Format("Directory '{0}' not found", targetDir));
            }
        }

        public override void MoveDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            var target = GetTarget(srcdomain, srcdir);
            var newtarget = GetTarget(newdomain, newdir);
            var newtargetSub = newtarget.Remove(newtarget.LastIndexOf(Path.DirectorySeparatorChar));

            if (!Directory.Exists(newtargetSub))
            {
                Directory.CreateDirectory(newtargetSub);
            }
            Directory.Move(target, newtarget);
        }

        public override Uri Move(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            if (srcpath == null) throw new ArgumentNullException("srcpath");
            if (newpath == null) throw new ArgumentNullException("srcpath");
            var target = GetTarget(srcdomain, srcpath);
            var newtarget = GetTarget(newdomain, newpath);

            if (File.Exists(target))
            {
                if (!Directory.Exists(Path.GetDirectoryName(newtarget)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newtarget));
                }

                var flength = new FileInfo(target).Length;

                //Delete file if exists
                if (File.Exists(newtarget))
                {
                    File.Delete(newtarget);
                }
                File.Move(target, newtarget);
               
                QuotaUsedDelete(srcdomain, flength);
                QuotaUsedAdd(newdomain, flength);
            }
            else
            {
                throw new FileNotFoundException("File not found", Path.GetFullPath(target));
            }
            return GetUri(newdomain, newpath);
        }

        public override bool IsDirectory(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            var targetDir = GetTarget(domain, path);
            if (!string.IsNullOrEmpty(targetDir) && !targetDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                targetDir += Path.DirectorySeparatorChar;
            }
            return !string.IsNullOrEmpty(targetDir) && Directory.Exists(targetDir);
        }

        public override void DeleteDirectory(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            var targetDir = GetTarget(domain, path);

            if (string.IsNullOrEmpty(targetDir)) throw new Exception("targetDir is null");

            if (!string.IsNullOrEmpty(targetDir) && !targetDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                targetDir += Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(targetDir)) return;

            var entries = Directory.GetFiles(targetDir, "*.*", SearchOption.AllDirectories);
            var size = entries.Select(entry => new FileInfo(entry)).Select(info => info.Length).Sum();

            var subDirs = Directory.GetDirectories(targetDir, "*", SearchOption.AllDirectories).ToList();
            subDirs.Reverse();
            subDirs.ForEach(subdir => Directory.Delete(subdir, true));

            Directory.Delete(targetDir, true);

            QuotaUsedDelete(domain, size);
        }

        public override long GetFileSize(string domain, string path)
        {
            var target = GetTarget(domain, path);

            if (File.Exists(target))
            {
                return new FileInfo(target).Length;
            }
            throw new FileNotFoundException("file not found " + target);
        }


        public override Uri SaveTemp(string domain, out string assignedPath, Stream stream)
        {
            assignedPath = Guid.NewGuid().ToString();
            return Save(domain, assignedPath, stream);
        }


        public override string SavePrivate(string domain, string path, Stream stream, DateTime expires)
        {
            return Save(domain, path, stream).ToString();
        }

        public override void DeleteExpired(string domain, string folderPath, TimeSpan oldThreshold)
        {
            if (folderPath == null) throw new ArgumentNullException("folderPath");

            //Return dirs
            var targetDir = GetTarget(domain, folderPath);
            if (!Directory.Exists(targetDir))
            {
                return;
            }

            var entries = Directory.GetFiles(targetDir, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var entry in entries)
            {
                var finfo = new FileInfo(entry);
                if ((DateTime.UtcNow - finfo.CreationTimeUtc) > oldThreshold)
                {
                    File.Delete(entry);

                    QuotaUsedDelete(domain, finfo.Length);
                }
            }
        }

        public override string GetUploadForm(string domain, string directoryPath, string redirectTo, long maxUploadSize, string contentType, string contentDisposition, string submitLabel)
        {
            throw new NotSupportedException("This operation supported only on s3 storage");
        }

        public override string GetUploadedUrl(string domain, string directoryPath)
        {
            throw new NotSupportedException("This operation supported only on s3 storage");
        }

        public override string GetUploadUrl()
        {
            throw new NotSupportedException("This operation supported only on s3 storage");
        }

        public override string GetPostParams(string domain, string directoryPath, long maxUploadSize, string contentType, string contentDisposition)
        {
            throw new NotSupportedException("This operation supported only on s3 storage");
        }


        public override Uri[] List(string domain, string path, bool recursive)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            var targetDir = GetTarget(domain, path);
            if (!string.IsNullOrEmpty(targetDir) && !targetDir.EndsWith(Path.DirectorySeparatorChar.ToString())) targetDir += Path.DirectorySeparatorChar;
            if (Directory.Exists(targetDir))
            {
                var entries = Directory.GetDirectories(targetDir, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                return Array.ConvertAll(
                    entries,
                    x => GetUri(domain, Path.Combine(path != null ? PathUtils.Normalize(path) : string.Empty, x.Substring(targetDir.Length))));
            }
            return new Uri[0];
        }

        public override Uri[] ListFiles(string domain, string path, string pattern, bool recursive)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            var targetDir = GetTarget(domain, path);
            if (!string.IsNullOrEmpty(targetDir) && !targetDir.EndsWith(Path.DirectorySeparatorChar.ToString())) targetDir += Path.DirectorySeparatorChar;
            if (Directory.Exists(targetDir))
            {
                var entries = Directory.GetFiles(targetDir, pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                return Array.ConvertAll(
                    entries,
                    x => GetUri(domain, Path.Combine(PathUtils.Normalize(path), x.Substring(targetDir.Length))));
            }
            return new Uri[0];
        }

        public override string[] ListFilesRelative(string domain, string path, string pattern, bool recursive)
        {
            var dirPath = GetUri(domain, path).ToString();
            return ListFiles(domain, path, pattern, /*true*/recursive).Select(x => x.ToString().Substring(dirPath.Length).TrimStart('/')).ToArray();
        }

        public override bool IsFile(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            var target = GetTarget(domain, path);
            var result = File.Exists(target);
            return result;
        }

        public override long ResetQuota(string domain)
        {
            if (QuotaController != null)
            {
                var size = GetUsedQuota(domain);
                QuotaController.QuotaUsedSet(_modulename, domain, _dataList.GetData(domain), size);
            }
            return 0;
        }

        public override long GetUsedQuota(string domain)
        {
            var target = GetTarget(domain, "");
            long size = 0;
            if (Directory.Exists(target))
            {
                var entries = Directory.GetFiles(target, "*.*", SearchOption.AllDirectories);
                size = entries.Select(entry => new FileInfo(entry)).Select(info => info.Length).Sum();
            }
            return size;
        }

        public override Uri Copy(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            if (srcpath == null) throw new ArgumentNullException("srcpath");
            if (newpath == null) throw new ArgumentNullException("srcpath");
            var target = GetTarget(srcdomain, srcpath);
            var newtarget = GetTarget(newdomain, newpath);

            if (File.Exists(target))
            {
                if (!Directory.Exists(Path.GetDirectoryName(newtarget)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newtarget));
                }

                File.Copy(target, newtarget, true);

                var flength = new FileInfo(target).Length;
                QuotaUsedAdd(newdomain, flength);
            }
            else
            {
                throw new FileNotFoundException("File not found", Path.GetFullPath(target));
            }
            return GetUri(newdomain, newpath);
        }

        public override void CopyDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            var target = GetTarget(srcdomain, srcdir);
            var newtarget = GetTarget(newdomain, newdir);

            var diSource = new DirectoryInfo(target);
            var diTarget = new DirectoryInfo(newtarget);

            CopyAll(diSource, diTarget, newdomain);
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target, string newdomain)
        {
            // Check if the target directory exists, if not, create it.
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (var fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                QuotaUsedAdd(newdomain, fi.Length);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir, newdomain);
            }
        }

        public override Uri GetUriInternal(string path)
        {
            return GetUri(string.Empty, path);
        }


        private MappedPath GetPath(string domain)
        {
            if (domain != null)
                if (_mappedPaths.ContainsKey(domain))
                {
                    return _mappedPaths[domain];
                }
            return _mappedPaths[string.Empty].AppendDomain(domain);
        }

        public Stream GetWriteStream(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            var target = GetTarget(domain, path);
            CreateDirectory(target);
            return File.Open(target, FileMode.Create);
        }

        private static void CreateDirectory(string target)
        {
            string targetDirectory = Path.GetDirectoryName(target);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
        }

        private string GetTarget(string domain, string path)
        {
            var pathMap = GetPath(domain);
            //Build Dir
            var target = Path.Combine(pathMap.PhysicalPath, PathUtils.Normalize(path));
            ValidatePath(target);
            return target;
        }

        private static void ValidatePath(string target)
        {
            if (Path.GetDirectoryName(target).IndexOfAny(Path.GetInvalidPathChars()) != -1 ||
                Path.GetFileName(target).IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                //Throw
                throw new ArgumentException("bad path");
            }
        }

        private class MonoUri : Uri
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