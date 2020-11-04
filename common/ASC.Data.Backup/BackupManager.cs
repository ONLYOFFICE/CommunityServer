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
                }

                backupWriter.WriteEntry(XML_NAME, doc.ToString(SaveOptions.None));
            }
            ProgressChanged(this, new ProgressChangedEventArgs(string.Empty, 100, true));
        }

        public void Load()
        {
            using (var reader = new ZipReadOperator(backup))
            using (var stream = reader.GetEntry(XML_NAME))
            using (var xml = XmlReader.Create(stream))
            {
                var root = XDocument.Load(xml).Element(ROOT);
                if (root == null) return;

                var tenant = int.Parse(root.Attribute("tenant").Value);
                foreach (var provider in providers.Values)
                {
                    var element = root.Element(provider.Name);
                    provider.LoadFrom(element != null ? element.Elements() : new XElement[0], tenant, configs, reader);
                }
            }
            ProgressChanged(this, new ProgressChangedEventArgs(string.Empty, 100, true));
        }


        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null) ProgressChanged(this, e);
        }
    }
}