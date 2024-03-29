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
using System.IO;

using ASC.Projects.Engine;
using ASC.Web.Files.Services.DocumentService;
using ASC.Web.Files.Utils;
using ASC.Web.Projects;
using ASC.Web.Projects.Core;

using Autofac;

namespace ASC.Projects.Core.Domain.Reports
{
    public class ReportTemplate
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ReportType ReportType { get; private set; }

        public TaskFilter Filter { get; set; }

        public string Cron { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public bool AutoGenerated { get; set; }

        public int Tenant { get; set; }


        public ReportTemplate(ReportType reportType)
        {
            ReportType = reportType;
            AutoGenerated = false;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var t = obj as ReportTemplate;
            return t != null && Id.Equals(t.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        internal void SaveDocbuilderReport(ReportState state, string url)
        {
            var data = new System.Net.WebClient().DownloadData(url);

            using (var memStream = new MemoryStream(data))
            {
                Action<Stream> action = stream =>
                {
                    var file = FileUploader.Exec(ProjectsCommonSettings.LoadForCurrentUser().FolderId.ToString(), state.FileName, stream.Length, stream, true);
                    state.FileId = (int)file.ID;
                };

                try
                {
                    action(memStream);
                }
                catch (DirectoryNotFoundException)
                {
                    var settings = ProjectsCommonSettings.LoadForCurrentUser();
                    settings.FolderId = Web.Files.Classes.Global.FolderMy;
                    settings.SaveForCurrentUser();

                    action(memStream);
                }
            }

            using (var scope = DIHelper.Resolve())
            {
                scope.Resolve<EngineFactory>().ReportEngine.Save(new ReportFile
                {
                    FileId = state.FileId,
                    Name = Name,
                    ReportType = ReportType
                });
            }
        }
    }
}
