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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ASC.Common.Logging;
using ASC.Data.Storage;

namespace ASC.Data.Backup
{
    public class FileBackupProvider : IBackupProvider
    {
        private readonly static ILog log = LogManager.GetLogger("ASC.Data");

        private readonly IEnumerable<string> allowedModules;


        public string Name
        {
            get { return "Files"; }
        }


        public FileBackupProvider()
        {
            allowedModules = new List<string>() { "forum", "photo", "bookmarking", "wiki", "files", "crm", "projects", "logo", "fckuploaders", "talk" };
        }


        public IEnumerable<XElement> GetElements(int tenant, string[] configs, IDataWriteOperator writer)
        {
            InvokeProgressChanged("Saving files...", 0);

            var config = GetWebConfig(configs);
            var files = ComposeFiles(tenant, config);

            var elements = new List<XElement>();
            var backupKeys = new List<string>();

            var counter = 0;
            var totalCount = (double)files.Count();

            foreach (var file in files)
            {
                var backupPath = GetBackupPath(file);
                if (!backupKeys.Contains(backupPath))
                {
                    var storage = StorageFactory.GetStorage(config, tenant.ToString(), file.Module);
                    var errors = 0;
                    while (true)
                    {
                        try
                        {
                            using (var stream = storage.GetReadStream(file.Domain, file.Path))
                            {
                                var tmpPath = Path.GetTempFileName();
                                using (var tmpFile = File.OpenWrite(tmpPath))
                                {
                                    stream.StreamCopyTo(tmpFile);
                                }

                                writer.WriteEntry(backupPath, tmpPath);
                                File.Delete(tmpPath);
                            }

                            break;
                        }
                        catch (Exception error)
                        {
                            errors++;
                            if (20 < errors)
                            {
                                log.ErrorFormat("Can not backup file {0}: {1}", file.Path, error);
                                break;
                            }
                        }
                    }
                    elements.Add(file.ToXElement());
                    backupKeys.Add(backupPath);
                    log.DebugFormat("Backup file {0}", file.Path);
                }
                InvokeProgressChanged("Saving file " + file.Path, counter++ / totalCount * 100);
            }

            return elements;
        }

        private IEnumerable<FileBackupInfo> ComposeFiles(int tenant, string config)
        {
            var files = new List<FileBackupInfo>();
            foreach (var module in StorageFactory.GetModuleList(config))
            {
                if (allowedModules.Contains(module))
                {
                    var store = StorageFactory.GetStorage(config, tenant.ToString(), module);
                    var domainList = StorageFactory.GetDomainList(config, module);
                    foreach (var domain in domainList)
                    {
                        files.AddRange(store
                            .ListFilesRelative(domain, "\\", "*.*", true)
                            .Select(x => new FileBackupInfo(domain, module, x)));
                    }

                    files.AddRange(store
                        .ListFilesRelative(string.Empty, "\\", "*.*", true)
                        .Where(x => domainList.All(domain => x.IndexOf(string.Format("{0}/", domain)) == -1))
                        .Select(x => new FileBackupInfo(string.Empty, module, x)));
                }
            }
            return files.Distinct();
        }

        private string GetBackupPath(FileBackupInfo backupInfo)
        {
            return Path.Combine(backupInfo.Module, Path.Combine(backupInfo.Domain, backupInfo.Path.Replace('/', '\\')));
        }


        public void LoadFrom(IEnumerable<XElement> elements, int tenant, string[] configs, IDataReadOperator dataOperator)
        {
            InvokeProgressChanged("Restoring files...", 0);

            var config = GetWebConfig(configs);
            var files = elements.Where(e => e.Name == "file");
            double current = 0;
            foreach (var file in files)
            {
                var backupInfo = new FileBackupInfo(file);
                if (allowedModules.Contains(backupInfo.Module))
                {
                    using (var entry = dataOperator.GetEntry(GetBackupPath(backupInfo)))
                    {
                        var storage = StorageFactory.GetStorage(config, tenant.ToString(), backupInfo.Module, null);
                        try
                        {
                            storage.Save(backupInfo.Domain, backupInfo.Path, entry);
                        }
                        catch (Exception error)
                        {
                            log.ErrorFormat("Can not restore file {0}: {1}", file, error);
                        }
                    }
                    InvokeProgressChanged("Restoring file " + backupInfo.Path, current++ / files.Count() * 100);
                }
            }
        }

        private string GetWebConfig(string[] configs)
        {
            return configs.Where(c => "Web.config".Equals(Path.GetFileName(c), StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        private void InvokeProgressChanged(string status, double currentProgress)
        {
            try
            {
                var @delegate = ProgressChanged;
                if (@delegate != null) @delegate(this, new ProgressChangedEventArgs(status, (int)currentProgress));
            }
            catch (Exception error)
            {
                log.Error("InvokeProgressChanged", error);
            }
        }



        private class FileBackupInfo
        {
            public string Module { get; set; }
            public string Domain { get; set; }
            public string Path { get; set; }
            public int Errors { get; set; }

            public FileBackupInfo(string domain, string module, string path)
            {
                Domain = domain;
                Module = module;
                Path = path;
            }

            public FileBackupInfo(XElement element)
            {
                Domain = element.Attribute("domain").Value;
                Module = element.Attribute("module").Value;
                Path = element.Attribute("path").Value;
            }

            public XElement ToXElement()
            {
                return new XElement("file",
                    new XAttribute("module", Module),
                    new XAttribute("domain", Domain),
                    new XAttribute("path", Path)
                );
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(FileBackupInfo)) return false;
                return Equals((FileBackupInfo)obj);
            }

            public bool Equals(FileBackupInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.Module, Module) && Equals(other.Domain, Domain) && Equals(other.Path, Path);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = (Module != null ? Module.GetHashCode() : 0);
                    result = (result * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
                    result = (result * 397) ^ (Path != null ? Path.GetHashCode() : 0);
                    return result;
                }
            }
        }
    }
}