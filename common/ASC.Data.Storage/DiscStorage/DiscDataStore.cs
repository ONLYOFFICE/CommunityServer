/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using ASC.Core.Tenants;
using ASC.Data.Storage.Configuration;

namespace ASC.Data.Storage.DiscStorage
{
    internal class MappedPath : ICloneable
    {
        public MappedPath(HttpContext context, string tenant, bool appendTenant, string physicalPath, string virtualPath, string basedir,
                          long maxquota, bool overwrite)
        {
            Overwrite = overwrite;

            virtualPath = virtualPath.TrimEnd('/');
            physicalPath = physicalPath.TrimEnd(System.IO.Path.PathSeparator);

            if (physicalPath.IndexOf('{') == -1 && appendTenant)
                Path = string.Format("{0}\\", System.IO.Path.Combine(physicalPath, tenant)).Replace("\\\\", "\\").
                       TrimEnd(System.IO.Path.PathSeparator);
            else
                Path = (string.Format(physicalPath, tenant) + '\\').Replace("\\\\", "\\").TrimEnd(
                    System.IO.Path.PathSeparator);

            string vpath;

            if (virtualPath.IndexOf('{') == -1 && appendTenant)
                vpath = string.Format("{0}/{1}/", virtualPath, tenant).TrimEnd('/');
            else
                vpath = (string.Format("{0}/", string.Format(virtualPath, tenant))).TrimEnd('/');

            //Append trailing
            Path = Path.TrimEnd('\\', '/') + "\\";
            if (vpath.StartsWith("~", StringComparison.OrdinalIgnoreCase) &&
                !Uri.IsWellFormedUriString(vpath, UriKind.Absolute))
            {
                if (context != null && vpath.IndexOfAny(new[] { '?', '=', '&' }) == -1)
                {
                    vpath = VirtualPathUtility.ToAbsolute(vpath);
                }
                else
                {
                    //Combine from current domain
                    vpath = vpath.Substring(1); //Remove ~
                }
            }
            VirtualPath = new Uri(vpath + "/", UriKind.RelativeOrAbsolute);

            //Fix abs path
            if (Path.StartsWith("~"))
            {
                Path = Path.Substring(1); //Remove ~
            }


            if (!IsDiscPathAbsolute(Path))
            {
                Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(basedir, Path.TrimStart('\\', '/')));
            }

            MaxQuotaSize = maxquota;
        }

        public bool Overwrite { get; private set; }

        public long MaxQuotaSize { get; set; }
        public string Path { get; set; }
        public Uri VirtualPath { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        private static bool IsDiscPathAbsolute(string path)
        {
            return path.IndexOf(System.IO.Path.VolumeSeparatorChar) != -1;
        }

        public MappedPath AppendDomain(string domain)
        {
            //Domain prep. Remove dots
            domain = domain.Replace('.', '_');

            var path = (MappedPath)Clone();
            if (VirtualPath.IsAbsoluteUri)
            {
                path.VirtualPath = new Uri(VirtualPath, domain);
            }
            else
            {
                path.VirtualPath = new Uri(VirtualPath + domain, UriKind.Relative);
            }
            path.Path = System.IO.Path.Combine(Path, domain.Replace('/', '\\')) + "\\";
            return path;
        }
    }

    public class DiscDataStore : BaseStorage
    {
        private const int BufferSize = 4096;
        private readonly DataList _dataList;
        private readonly Dictionary<string, MappedPath> _mappedPaths = new Dictionary<string, MappedPath>();
        private readonly string _modulename;


        public DiscDataStore(string tenant, ModuleConfigurationElement moduleConfig, HttpContext context)
        {
            //Fill map path
            _modulename = moduleConfig.Name;
            _dataList = new DataList(moduleConfig);
            foreach (DomainConfigurationElement domain in moduleConfig.Domains)
            {
                _mappedPaths.Add(domain.Name,
                                 new MappedPath(context, tenant, moduleConfig.AppendTenant, domain.Path, domain.VirtualPath,
                                                SelectDirectory(moduleConfig), domain.Quota, domain.Overwrite));
            }
            //Add default
            _mappedPaths.Add(string.Empty,
                             new MappedPath(context, tenant, moduleConfig.AppendTenant, moduleConfig.Path, moduleConfig.VirtualPath,
                                            SelectDirectory(moduleConfig),
                                            moduleConfig.Quota, moduleConfig.Overwrite));
        }

        private static string SelectDirectory(ModuleConfigurationElement moduleConfig)
        {
            if (string.IsNullOrEmpty(moduleConfig.BaseDir))
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/"));
                }
                if (AppDomain.CurrentDomain.GetData(Constants.CustomDataDirectory) != null)
                {
                    return (string)AppDomain.CurrentDomain.GetData(Constants.CustomDataDirectory);
                }
                return AppDomain.CurrentDomain.BaseDirectory;
            }
            return moduleConfig.BaseDir;
        }

        public override Uri GetPreSignedUri(string domain, string path, TimeSpan expire, IEnumerable<string> headers)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var pathMap = GetPath(domain);
            return pathMap.VirtualPath.IsAbsoluteUri ?
                new Uri(pathMap.VirtualPath, pathMap.VirtualPath.LocalPath.TrimEnd('/') + EnsureLeadingSlash(path.Replace('\\', '/'))) :
                new Uri(pathMap.VirtualPath.ToString().TrimEnd('/') + EnsureLeadingSlash(path.Replace('\\', '/')), UriKind.Relative);
        }

        private static string EnsureLeadingSlash(string str)
        {
            return "/" + str.TrimStart('/');
        }

        private static string EnsureSingleSlashes(string str)
        {
            string[] parts = str.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("/", parts);
        }

        private static string EnsureEndingSlash(string str)
        {
            return string.Format("{0}/", str.TrimEnd('/'));
        }

        public override Stream GetReadStream(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            string target = GetTarget(path, domain);

            if (File.Exists(target))
            {
                return File.OpenRead(target);
            }
            throw new FileNotFoundException("File not found", Path.GetFullPath(target));
        }


        public override Stream GetReadStream(string domain, string path, int offset)
        {
            if (path == null) throw new ArgumentNullException("path");
            string target = GetTarget(path, domain);

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

        public override Uri Save(string domain, string path, Stream stream, string contentType,
                                 string contentDisposition)
        {
            return Save(domain, path, stream);
        }

        public override Uri Save(string domain, string path, Stream stream, string contentEncoding, int cacheDays)
        {
            return Save(domain, path, stream);

        }

        public override Uri UploadWithoutQuota(string domain, string path, Stream stream, string contentType,
                                 string contentDisposition)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (stream == null) throw new ArgumentNullException("stream");

            //Try seek to start
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            //Lookup domain
            string target = GetTarget(path, domain, true);
            CreateDirectory(target);
            //Copy stream

            using (var fs = File.Open(target, FileMode.Create))
            {
                var buffer = new byte[BufferSize];
                int readed;
                while ((readed = stream.Read(buffer, 0, BufferSize)) != 0)
                {
                    fs.Write(buffer, 0, readed);
                }
            }

            return GetUri(domain, path);
        }

        public override Uri Save(string domain, string path, Stream stream)
        {
            bool postWriteCheck = false;
            if (QuotaController != null)
            {
                try
                {
                    QuotaController.QuotaUsedAdd(_modulename, domain, _dataList.GetData(domain), stream.Length);
                }
                catch (TenantQuotaException)
                {
                    //this exception occurs only if tenant has no free space
                    //or if file size larger than allowed by quota
                    //so we can exit this function without stream buffering etc
                    throw;
                }
                catch (Exception)
                {
                    postWriteCheck = true;
                }
            }

            if (path == null) throw new ArgumentNullException("path");
            if (stream == null) throw new ArgumentNullException("stream");

            //Try seek to start
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            //Lookup domain
            string target = GetTarget(path, domain, true);
            CreateDirectory(target);
            //Copy stream

            using (var fs = File.Open(target, FileMode.Create))
            {
                var buffer = new byte[BufferSize];
                int readed;
                while ((readed = stream.Read(buffer, 0, BufferSize)) != 0)
                {
                    fs.Write(buffer, 0, readed);
                }
                if (postWriteCheck)
                {
                    QuotaController.QuotaUsedAdd(_modulename, domain, _dataList.GetData(domain), fs.Length);
                }
            }

            return GetUri(domain, path);
        }

        public override Uri Save(string domain, string path, Stream stream, ACL acl)
        {
            return Save(domain, path, stream);
        }

        #region chunking

        public override string InitiateChunkedUpload(string domain, string path)
        {
            var target = GetTarget(domain, path, true);
            CreateDirectory(target);
            return target;
        }

        public override string UploadChunk(string domain, string path, string uploadId, Stream stream, int chunkNumber, long chunkLength)
        {
            var target = GetTarget(path, domain);
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
                QuotaController.QuotaUsedAdd(_modulename, domain, _dataList.GetData(domain), size);
            }
            return GetUri(domain, path);
        }

        public override void AbortChunkedUpload(string domain, string path, string uploadId)
        {
            var target = GetTarget(path, domain);
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
            string target = GetTarget(path, domain);

            if (File.Exists(target))
            {
                if (QuotaController != null)
                {
                    QuotaController.QuotaUsedDelete(_modulename, domain, _dataList.GetData(domain),
                                                    new FileInfo(target).Length);
                }
                File.Delete(target);
            }
            else
            {
                throw new FileNotFoundException("file not found", target);
            }
        }

        public override void DeleteFiles(string domain, string folderPath, string pattern, bool recursive)
        {
            if (folderPath == null) throw new ArgumentNullException("folderPath");

            //Return dirs
            string targetDir = GetTarget(folderPath, domain);
            if (Directory.Exists(targetDir))
            {
                string[] entries = Directory.GetFiles(targetDir, pattern,
                                                      recursive
                                                          ? SearchOption.AllDirectories
                                                          : SearchOption.TopDirectoryOnly);
                foreach (string entry in entries)
                {
                    if (QuotaController != null)
                    {
                        QuotaController.QuotaUsedDelete(_modulename, domain, _dataList.GetData(domain),
                                                        new FileInfo(entry).Length);
                    }
                    File.Delete(entry);
                }
            }
            else
            {
                throw new DirectoryNotFoundException(string.Format("Directory '{0}' not found", targetDir));
            }
        }

        public override void MoveDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            string target = GetTarget(srcdir, srcdomain);
            string newtarget = GetTarget(newdir, newdomain, true);

            String newtargetSub = newtarget.Remove(newtarget.LastIndexOf("\\"));

            if (!Directory.Exists(newtargetSub))
                Directory.CreateDirectory(newtargetSub);

            Directory.Move(target, newtarget);
        }

        public override Uri Move(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            if (srcpath == null) throw new ArgumentNullException("srcpath");
            if (newpath == null) throw new ArgumentNullException("srcpath");
            string target = GetTarget(srcpath, srcdomain);
            string newtarget = GetTarget(newpath, newdomain);

            if (File.Exists(target))
            {
                if (!Directory.Exists(Path.GetDirectoryName(newtarget)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newtarget));
                }
                if (QuotaController != null)
                {
                    long flength = new FileInfo(target).Length;
                    QuotaController.QuotaUsedDelete(_modulename, srcdomain, _dataList.GetData(srcdomain), flength);
                    QuotaController.QuotaUsedAdd(_modulename, newdomain, _dataList.GetData(newdomain), flength);
                }
                //Delete file if exists
                if (File.Exists(newtarget))
                {
                    File.Delete(newtarget);
                }
                File.Move(target, newtarget);
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
            string targetDir = GetTarget(path, domain);
            if (!string.IsNullOrEmpty(targetDir) && !targetDir.EndsWith("\\"))
            {
                targetDir += "\\";
            }
            return !string.IsNullOrEmpty(targetDir) && Directory.Exists(targetDir);
        }

        public override void DeleteDirectory(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            var targetDir = GetTarget(path, domain);

            if (String.IsNullOrEmpty(targetDir)) throw new Exception("targetDir is null");

            if (!string.IsNullOrEmpty(targetDir) && !targetDir.EndsWith("\\"))
            {
                targetDir += "\\";
            }

            long size = 0;

            if (!Directory.Exists(targetDir)) return;
            
            var entries = Directory.GetFiles(targetDir, "*.*", SearchOption.AllDirectories);
            size = entries.Select(entry => new FileInfo(entry)).Select(info => info.Length).Sum();

            var subDirs = Directory.GetDirectories(targetDir, "*", SearchOption.AllDirectories).ToList();
            subDirs.Reverse();
            subDirs.ForEach(subdir => Directory.Delete(subdir, true));

            Directory.Delete(targetDir, true);

            if (QuotaController != null)
            {
                QuotaController.QuotaUsedDelete(_modulename, domain, _dataList.GetData(domain), size);
            }
        }

        public override long GetFileSize(string domain, string path)
        {
            string target = GetTarget(path, domain);

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
            string targetDir = GetTarget(folderPath, domain);
            if (Directory.Exists(targetDir))
            {
                string[] entries = Directory.GetFiles(targetDir, "*.*", SearchOption.TopDirectoryOnly);
                foreach (string entry in entries)
                {
                    var finfo = new FileInfo(entry);
                    if ((DateTime.UtcNow - finfo.CreationTimeUtc) > oldThreshold)
                    {
                        if (QuotaController != null)
                        {
                            QuotaController.QuotaUsedDelete(_modulename, domain, _dataList.GetData(domain),
                                                            new FileInfo(entry).Length);
                        }
                        File.Delete(entry);
                    }
                }
            }
            else
            {
                throw new DirectoryNotFoundException(string.Format("Directory '{0}' not found", targetDir));
            }
        }

        public override string GetUploadForm(string domain, string directoryPath, string redirectTo, long maxUploadSize,
                                             string contentType, string contentDisposition, string submitLabel)
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

        public override string GetPostParams(string domain, string directoryPath, long maxUploadSize, string contentType,
                                             string contentDisposition)
        {
            throw new NotSupportedException("This operation supported only on s3 storage");
        }


        public override Uri[] List(string domain, string path, bool recursive)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            string targetDir = GetTarget(path, domain);
            if (!string.IsNullOrEmpty(targetDir) && !targetDir.EndsWith("\\")) targetDir += "\\";
            if (Directory.Exists(targetDir))
            {
                string[] entries = Directory.GetDirectories(targetDir, "*",
                                                            recursive
                                                                ? SearchOption.AllDirectories
                                                                : SearchOption.TopDirectoryOnly);
                return Array.ConvertAll(entries,
                                        (x) =>
                                        GetUri(domain,
                                               Path.Combine(path != null ? path.Replace('/', '\\') : "",
                                                            x.Substring(targetDir.Length))));
            }
            return new Uri[0];
        }

        public override Uri[] ListFiles(string domain, string path, string pattern, bool recursive)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            string targetDir = GetTarget(path, domain);
            if (!string.IsNullOrEmpty(targetDir) && !targetDir.EndsWith("\\")) targetDir += "\\";
            if (Directory.Exists(targetDir))
            {
                string[] entries = Directory.GetFiles(targetDir, pattern,
                                                      recursive
                                                          ? SearchOption.AllDirectories
                                                          : SearchOption.TopDirectoryOnly);

                return Array.ConvertAll(entries,
                                        (x) =>
                                        GetUri(domain,
                                               Path.Combine(path != null ? path.Replace('/', '\\') : "",
                                                            x.Substring(targetDir.Length))));
            }
            return new Uri[0];
        }

        public override string[] ListFilesRelative(string domain, string path, string pattern, bool recursive)
        {
            string dirPath = GetUri(domain, path).ToString();
            return
                ListFiles(domain, path, pattern, /*true*/recursive).Select(
                    (x) => x.ToString().Substring(dirPath.Length).TrimStart('/')).ToArray();
        }

        public override bool IsFile(string domain, string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            //Return dirs
            string target = GetTarget(path, domain);
            bool result = File.Exists(target);
            return result;
        }

        public override long ResetQuota(string domain)
        {
            if (QuotaController != null)
            {
                long size = GetUsedQuota(domain);
                QuotaController.QuotaUsedSet(_modulename, domain, _dataList.GetData(domain), size);
            }
            return 0;
        }

        public override long GetUsedQuota(string domain)
        {
            string target = GetTarget("", domain);
            long size = 0;
            if (Directory.Exists(target))
            {
                string[] entries = Directory.GetFiles(target, "*.*", SearchOption.AllDirectories);
                size = entries.Select(entry => new FileInfo(entry)).Select(info => info.Length).Sum();
            }
            return size;
        }

        public override Uri Copy(string srcdomain, string srcpath, string newdomain, string newpath)
        {
            if (srcpath == null) throw new ArgumentNullException("srcpath");
            if (newpath == null) throw new ArgumentNullException("srcpath");
            string target = GetTarget(srcpath, srcdomain);
            string newtarget = GetTarget(newpath, newdomain);

            if (File.Exists(target))
            {
                if (!Directory.Exists(Path.GetDirectoryName(newtarget)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newtarget));
                }
                if (QuotaController != null)
                {
                    long flength = new FileInfo(target).Length;
                    QuotaController.QuotaUsedAdd(_modulename, newdomain, _dataList.GetData(newdomain), flength);
                }
                File.Copy(target, newtarget, true);
            }
            else
            {
                throw new FileNotFoundException("File not found", Path.GetFullPath(target));
            }
            return GetUri(newdomain, newpath);
        }

        public override void CopyDirectory(string srcdomain, string srcdir, string newdomain, string newdir)
        {
            string target = GetTarget(srcdir, srcdomain);
            string newtarget = GetTarget(newdir, newdomain, true);

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
            foreach (FileInfo fi in source.GetFiles())
            {
                if (QuotaController != null)
                {
                    QuotaController.QuotaUsedAdd(_modulename, newdomain, _dataList.GetData(newdomain), fi.Length);
                }
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir, newdomain);
            }
        }

        public override Uri GetUriInternal(string path)
        {
            return GetUri(string.Empty, path);
        }


        public override IDataStore Configure(IDictionary<string, string> props)
        {
            return this; //No config for now
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
            string target = GetTarget(path, domain, true);
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

        private string GetTarget(string path, string domain)
        {
            return GetTarget(path, domain, false);
        }

        private string GetTarget(string path, string domain, bool bOverwriteCheck)
        {
            MappedPath pathMap = GetPath(domain);
            //Build Dir
            string target = pathMap.Path + path.Replace('/', '\\').TrimEnd('\\');
            ValidatePath(target);
            if (!pathMap.Overwrite && bOverwriteCheck)
            {
                //Check existing file
                if (File.Exists(target))
                {
                    throw new ArgumentException("can't overwrite existing file");
                }
            }
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
    }
}