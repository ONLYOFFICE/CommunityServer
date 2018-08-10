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

        public bool CanRead(FileEntry file, Guid userId)
        {
            return Can(file, userId, SecurityAction.Read);
        }

        public bool CanReview(FileEntry file, Guid userId)
        {
            return Can(file, userId, SecurityAction.Edit);
        }

        public bool CanCreate(FileEntry file, Guid userId)
        {
            return Can(file, userId, SecurityAction.Create);
        }

        public bool CanDelete(FileEntry file, Guid userId)
        {
            return Can(file, userId, SecurityAction.Delete);
        }

        public bool CanEdit(FileEntry file, Guid userId)
        {
            return Can(file, userId, SecurityAction.Edit);
        }

        private bool Can(FileEntry fileEntry, Guid userId, SecurityAction action)
        {
            if (fileEntry == null || project == null) return false;

            if (!ProjectSecurity.CanReadFiles(project, userId)) return false;

            if (project.Status == ProjectStatus.Closed
                && action != SecurityAction.Read)
                return false;

            if (ProjectSecurity.IsAdministrator(userId)) return true;

            using (var scope = DIHelper.Resolve())
            {
                var projectEngine = scope.Resolve<EngineFactory>().ProjectEngine;

                var folder = fileEntry as Folder;
                if (folder != null && folder.FolderType == FolderType.DEFAULT && folder.CreateBy == userId) return true;

                var file = fileEntry as File;
                if (file != null && file.CreateBy == userId) return true;

                switch (action)
                {
                    case SecurityAction.Read:
                        return !project.Private || projectEngine.IsInTeam(project.ID, userId);
                    case SecurityAction.Create:
                    case SecurityAction.Edit:
                        return projectEngine.IsInTeam(project.ID, userId) &&
                               (!ProjectSecurity.IsVisitor(userId) ||
                                folder != null && folder.FolderType == FolderType.BUNCH);
                    case SecurityAction.Delete:
                        return !ProjectSecurity.IsVisitor(userId) && project.Responsible == userId;
                    default:
                        return false;
                }
            }
        }

        public IEnumerable<Guid> WhoCanRead(FileEntry fileEntry)
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