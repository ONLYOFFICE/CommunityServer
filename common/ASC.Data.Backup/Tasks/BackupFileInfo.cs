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
using System.Xml.Linq;
using ASC.Data.Backup.Extensions;

namespace ASC.Data.Backup.Tasks
{
    public class BackupFileInfo
    {
        public string Domain { get; set; }
        public string Module { get; set; }
        public string Path { get; set; }
        public int Tenant { get; set; }

        public BackupFileInfo()
        {
        }

        public BackupFileInfo(string domain, string module, string path, int tenant = -1)
        {
            Domain = domain;
            Module = module;
            Path = path;
            Tenant = tenant;
        }

        public XElement ToXElement()
        {
            var xElement =  new XElement("file",
                                new XElement("domain", Domain),
                                new XElement("module", Module),
                                new XElement("path", Path));

            if (Tenant != -1)
            {
                xElement.Add(new XElement("tenant", Tenant));
            }

            return xElement;
        }

        public static BackupFileInfo FromXElement(XElement el)
        {
            return new BackupFileInfo
                {
                    Domain = el.Element("domain").ValueOrDefault(),
                    Module = el.Element("module").ValueOrDefault(),
                    Path = el.Element("path").ValueOrDefault(),
                    Tenant = Convert.ToInt32(el.Element("tenant").ValueOrDefault() ?? "-1")
                };
        }
    }
}
