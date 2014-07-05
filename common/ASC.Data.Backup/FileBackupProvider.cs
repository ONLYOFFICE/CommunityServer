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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ASC.Data.Storage;
using log4net;

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
                    var zipStream = writer.BeginWriteEntry(backupPath);
                    var errors = 0;
                    var offset = 0;
                    while (true)
                    {
                        try
                        {
                            using (var stream = storage.GetReadStream(file.Domain, file.Path, offset))
                            {
                                var buffer = new byte[2048];
                                var readed = 0;
                                while (0 < (readed = stream.Read(buffer, 0, buffer.Length)))
                                {
                                    zipStream.Write(buffer, 0, readed);
                                    offset += readed;
                                }
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
                    writer.EndWriteEntry();
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
            AppDomain.CurrentDomain.SetData(Constants.CustomDataDirectory, Path.GetDirectoryName(config));
            var files = elements.Where(e => e.Name == "file");
            double current = 0;
            foreach (var file in files)
            {
                var backupInfo = new FileBackupInfo(file);
                if (allowedModules.Contains(backupInfo.Module))
                {
                    var entry = dataOperator.GetEntry(GetBackupPath(backupInfo));
                    var storage = StorageFactory.GetStorage(config, tenant.ToString(), backupInfo.Module, null, null);
                    try
                    {
                        storage.Save(backupInfo.Domain, backupInfo.Path, entry);
                    }
                    catch (Exception error)
                    {
                        log.ErrorFormat("Can not restore file {0}: {1}", file, error);
                    }

                    InvokeProgressChanged("Restoring file " + backupInfo.Path, current++ / files.Count() * 100);
                }
            }
        }

        private string GetWebConfig(string[] configs)
        {
            return configs.Where(c => "web.config".Equals(Path.GetFileName(c), StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
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