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
using System.Linq;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Core;
using Autofac;

namespace ASC.Web.Projects.Classes
{
    public class SecurityAdapter : IFileSecurity
    {
        private readonly Project project;

        public SecurityAdapter(int projectId)
        {
            using (var scope = DIHelper.Resolve())
            {
                project = scope.Resolve<EngineFactory>().ProjectEngine.GetByID(projectId);
            }
        }

        public SecurityAdapter(Project project)
        {
            this.project = project;
        }

        public bool CanRead(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, SecurityAction.Read);
        }

        public bool CanCustomFilterEdit(FileEntry file, Guid userId)
        {
            return CanEdit(file, userId);
        }

        public bool CanComment(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, SecurityAction.Edit);
        }

        public bool CanFillForms(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, SecurityAction.Edit);
        }

        public bool CanReview(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, SecurityAction.Edit);
        }

        public bool CanCreate(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, SecurityAction.Create);
        }

        public bool CanDelete(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, SecurityAction.Delete);
        }

        public bool CanEdit(FileEntry entry, Guid userId)
        {
            return Can(entry, userId, SecurityAction.Edit);
        }

        private bool Can(FileEntry entry, Guid userId, SecurityAction action)
        {
            if (entry == null || project == null) return false;

            using (var scope = DIHelper.Resolve())
            {
                var projectSecurity = scope.Resolve<ProjectSecurity>();
                if (!projectSecurity.CanReadFiles(project, userId)) return false;

                if (project.Status == ProjectStatus.Closed
                    && action != SecurityAction.Read)
                    return false;

                if (projectSecurity.IsAdministrator(userId)) return true;

                var projectEngine = scope.Resolve<EngineFactory>().ProjectEngine;

                var inTeam = projectEngine.IsInTeam(project.ID, userId);

                switch (action)
                {
                    case SecurityAction.Read:
                        return !project.Private || inTeam;
                    case SecurityAction.Create:
                    case SecurityAction.Edit:
                        Folder folder;
                        return inTeam
                               && (!projectSecurity.IsVisitor(userId)
                                   || (folder = entry as Folder) != null && folder.FolderType == FolderType.BUNCH);
                    case SecurityAction.Delete:
                        return inTeam
                               && !projectSecurity.IsVisitor(userId)
                               && (project.Responsible == userId ||
                                   (entry.CreateBy == userId
                                    && ((folder = entry as Folder) == null || folder.FolderType == FolderType.DEFAULT)));
                    default:
                        return false;
                }
            }
        }

        public IEnumerable<Guid> WhoCanRead(FileEntry entry)
        {
            using (var scope = DIHelper.Resolve())
            {
                return scope.Resolve<EngineFactory>().ProjectEngine.GetTeam(project.ID).Select(p => p.ID).ToList();
            }
        }

        private enum SecurityAction
        {
            Read,
            Create,
            Edit,
            Delete,
        };
    }
}