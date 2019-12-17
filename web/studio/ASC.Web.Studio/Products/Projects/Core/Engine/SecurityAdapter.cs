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