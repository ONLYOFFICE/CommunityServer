/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Core.Caching;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;

namespace ASC.Web.Projects.Classes
{
    public class SecurityAdapter : IFileSecurity
    {
        private readonly ASC.Projects.Core.DataInterfaces.IProjectDao dao;
        private readonly int projectId;

        private Project project;
        private readonly TrustInterval interval = new TrustInterval();
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(10);

        private Project Project
        {
            get
            {
                if (interval.Expired)
                {
                    project = dao.GetById(projectId);
                    interval.Start(timeout);
                }
                return project;
            }
        }

        public SecurityAdapter(ASC.Projects.Core.DataInterfaces.IDaoFactory factory, int projectId)
        {
            dao = factory.GetProjectDao();
            this.projectId = projectId;
        }

        public bool CanRead(FileEntry file, Guid userId)
        {
            return Can(file, userId, SecurityAction.Read);
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
            if (!ProjectSecurity.CanReadFiles(Project, userId)) return false;

            if (ProjectSecurity.IsAdministrator(userId)) return true;
            if (fileEntry == null || Project == null) return false;
            if (fileEntry is Folder && ((Folder) fileEntry).FolderType == FolderType.DEFAULT && fileEntry.CreateBy == userId) return true;
            if (fileEntry is File && fileEntry.CreateBy == userId) return true;

            switch (action)
            {
                case SecurityAction.Read:
                    return !Project.Private || dao.IsInTeam(Project.ID, userId);
                case SecurityAction.Create:
                case SecurityAction.Edit:
                    return dao.IsInTeam(Project.ID, userId)
                           && (!ProjectSecurity.IsVisitor(userId) || fileEntry is Folder && ((Folder) fileEntry).FolderType == FolderType.BUNCH);
                case SecurityAction.Delete:
                    return !ProjectSecurity.IsVisitor(userId) && Project.Responsible == userId;
                default:
                    return false;
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