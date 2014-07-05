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
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ASC.Data.Backup
{
    public class BackupManager
    {
        private const string ROOT = "backup";
        private const string XML_NAME = "backupinfo.xml";

        private IDictionary<string, IBackupProvider> providers;
        private readonly string backup;
        private readonly string[] configs;

        private int providersProcessed;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;


        public BackupManager(string backup)
            : this(backup, null)
        {
        }

        public BackupManager(string backup, params string[] configs)
        {
            this.backup = backup;
            this.configs = configs ?? new string[0];

            providers = new Dictionary<string, IBackupProvider>();
            AddBackupProvider(new DbBackupProvider());
            AddBackupProvider(new FileBackupProvider());
        }

        public void AddBackupProvider(IBackupProvider provider)
        {
            providers.Add(provider.Name, provider);
            provider.ProgressChanged += OnProgressChanged;
        }

        public void RemoveBackupProvider(string name)
        {
            if (providers.ContainsKey(name))
            {
                providers[name].ProgressChanged -= OnProgressChanged;
                providers.Remove(name);
            }
        }


        public void Save(int tenant)
        {
            using (var backupWriter = new ZipWriteOperator(backup))
            {
                var doc = new XDocument(new XElement(ROOT, new XAttribute("tenant", tenant)));
                foreach (var provider in providers.Values)
                {
                    var elements = provider.GetElements(tenant, configs, backupWriter);
                    if (elements != null)
                    {
                        doc.Root.Add(new XElement(provider.Name, elements));
                    }
                    ++providersProcessed;
                }

                var data = Encoding.UTF8.GetBytes(doc.ToString(SaveOptions.None));
                var stream = backupWriter.BeginWriteEntry(XML_NAME);
                stream.Write(data, 0, data.Length);
                backupWriter.EndWriteEntry();
            }
            ProgressChanged(this, new ProgressChangedEventArgs(string.Empty, 100, true));
        }

        public void Load()
        {
            using (var reader = new ZipReadOperator(backup))
            using (var xml = XmlReader.Create(reader.GetEntry(XML_NAME)))
            {
                var root = XDocument.Load(xml).Element(ROOT);
                if (root == null) return;

                var tenant = int.Parse(root.Attribute("tenant").Value);
                foreach (var provider in providers.Values)
                {
                    var element = root.Element(provider.Name);
                    provider.LoadFrom(element != null ? element.Elements() : new XElement[0], tenant, configs, reader);
                    ++providersProcessed;
                }
            }
            ProgressChanged(this, new ProgressChangedEventArgs(string.Empty, 100, true));
        }


        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var progress = e.Progress;
            if (progress > 0)
            {
                progress = (100*providersProcessed + progress)/(double)providers.Count;
            }
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(e.Status, progress, e.Completed));
            }
        }
    }
}