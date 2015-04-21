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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Web.Files.Classes;
using log4net;

namespace ASC.Projects.Engine
{
    [DebuggerDisplay("SearchItem: EntityType = {EntityType}, ID = {ID}, Title = {Title}")]
    public class SearchItem
    {
        public EntityType EntityType { get; set; }
        public String ID { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public DateTime CreateOn { get; set; }

        public SearchItem()
        {
        }

        public SearchItem(EntityType entityType, int id, string title, string desc, DateTime createon)
        {
            EntityType = entityType;
            ID = id.ToString();
            Title = title;
            Description = desc;
            CreateOn = createon;
        }
    }

    [DebuggerDisplay("SearchGroup: ID = {ProjectID}, Title = {ProjectTitle}")]
    public class SearchGroup
    {
        public string ProjectTitle { get; private set; }

        public int ProjectID { get; private set; }

        public List<SearchItem> Items { get; private set; }

        public SearchGroup(int id, string title)
        {
            ProjectID = id;
            ProjectTitle = title;
            Items = new List<SearchItem>();
        }
    }

    public class SearchEngine
    {
        private readonly ISearchDao _searchDao;
        private readonly IProjectDao _projDao;


        public SearchEngine(IDaoFactory daoFactory)
        {
            _searchDao = daoFactory.GetSearchDao();
            _projDao = daoFactory.GetProjectDao();
        }

        public List<SearchGroup> Search(String searchText, int projectId = 0)
        {
            var queryResult = _searchDao.Search(searchText, projectId);

            var groups = new Dictionary<int, SearchGroup>();
            foreach (var r in queryResult)
            {
                var projId = 0;
                SearchItem item = null;

                if (r is Project)
                {
                    var p = (Project) r;
                    if (ProjectSecurity.CanRead(p))
                    {
                        projId = p.ID;
                        if (!groups.ContainsKey(projId)) groups[projId] = new SearchGroup(projId, p.Title);
                        item = new SearchItem(EntityType.Project, p.ID, p.Title, p.Description, p.CreateOn);
                    }
                }
                else
                {
                    if (r is Milestone)
                    {
                        var m = (Milestone) r;
                        if (ProjectSecurity.CanRead(m))
                        {
                            projId = m.Project.ID;
                            if (!groups.ContainsKey(projId)) groups[projId] = new SearchGroup(projId, m.Project.Title);
                            item = new SearchItem(EntityType.Milestone, m.ID, m.Title, null, m.CreateOn);
                        }
                    }
                    else if (r is Message)
                    {
                        var m = (Message) r;
                        if (ProjectSecurity.CanReadMessages(m.Project))
                        {
                            projId = m.Project.ID;
                            if (!groups.ContainsKey(projId)) groups[projId] = new SearchGroup(projId, m.Project.Title);
                            item = new SearchItem(EntityType.Message, m.ID, m.Title, m.Content, m.CreateOn);
                        }
                    }
                    else if (r is Task)
                    {
                        var t = (Task) r;
                        if (ProjectSecurity.CanRead(t))
                        {
                            projId = t.Project.ID;
                            if (!groups.ContainsKey(projId)) groups[projId] = new SearchGroup(projId, t.Project.Title);
                            item = new SearchItem(EntityType.Task, t.ID, t.Title, t.Description, t.CreateOn);
                        }
                    }
                }
                if (0 < projId && item != null)
                {
                    groups[projId].Items.Add(item);
                }
            }

            try
            {
                // search in files
                var fileEntries = new List<Files.Core.FileEntry>();
                using (var folderDao = FilesIntegration.GetFolderDao())
                using (var fileDao = FilesIntegration.GetFileDao())
                {
                    fileEntries.AddRange(folderDao.Search(searchText, Files.Core.FolderType.BUNCH).Cast<Files.Core.FileEntry>());
                    fileEntries.AddRange(fileDao.Search(searchText, Files.Core.FolderType.BUNCH).Cast<Files.Core.FileEntry>());

                    var projectIds = projectId != 0
                                         ? new List<int> {projectId}
                                         : fileEntries.GroupBy(f => f.RootFolderId)
                                               .Select(g => folderDao.GetFolder(g.Key))
                                               .Select(f => f != null ? folderDao.GetBunchObjectID(f.RootFolderId).Split('/').Last() : null)
                                               .Where(s => !string.IsNullOrEmpty(s))
                                               .Select(s => int.Parse(s));

                    var rootProject = projectIds.ToDictionary(id => FilesIntegration.RegisterBunch("projects", "project", id.ToString()));
                    fileEntries.RemoveAll(f => !rootProject.ContainsKey(f.RootFolderId));

                    var security = FilesIntegration.GetFileSecurity();
                    fileEntries.RemoveAll(f => !security.CanRead(f));

                    foreach (var f in fileEntries)
                    {
                        var id = rootProject[f.RootFolderId];
                        if (!groups.ContainsKey(id))
                        {
                            var project = _projDao.GetById(id);
                            if (project != null && ProjectSecurity.CanRead(project) && ProjectSecurity.CanReadFiles(project))
                            {
                                groups[id] = new SearchGroup(id, project.Title);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        var item = new SearchItem
                                       {
                                           EntityType = EntityType.File,
                                           ID = f is Files.Core.File
                                                    ? FilesLinkUtility.GetFileWebPreviewUrl(f.Title, f.ID)
                                                    : PathProvider.GetFolderUrl((Files.Core.Folder) f),
                                           Title = f.Title,
                                           CreateOn = f.CreateOn,
                                       };
                        groups[id].Items.Add(item);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.Web").Error(err);
            }
            return new List<SearchGroup>(groups.Values);
        }
    }
}