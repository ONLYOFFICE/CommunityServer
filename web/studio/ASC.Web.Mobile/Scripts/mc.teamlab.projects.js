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


;window.TeamlabMobile = (function (TeamlabMobile) {
  if (!TeamlabMobile) {
    console.log('Teamlab.default: has no TeamlabMobile');
    return undefined;
  }

  var
    
    someVariable = {},
    
    templateIds = {
      
      lbprojprojects : 'template-projprojects',
      lbprojmilestones : 'template-projmilestones',
      lbprojteam : 'template-projteam',
      lbtasks : 'template-proj-tasks',
      pgprojects : 'template-page-projects',
      pgprojitems : 'template-page-projects-items',
      pgprojproject : 'template-page-projects-project',
      pgprojprojectteam : 'template-page-projects-project-team',
      pgprojprojectfiles : 'template-page-projects-project-files',
      pgprojprojecttasks : 'template-page-projects-project-tasks',
      pgprojprojectmilestones : 'template-page-projects-project-milestones',
      pgprojprojectdiscussions : 'template-page-projects-project-discussions',
      pgprojmilestonetasks : 'template-page-projects-milestone-tasks',
      pgprojtask : 'template-page-projects-task',
      pgprojdiscussion : 'template-page-projects-discussion',
      pgprojaddtask : 'template-page-project-addtask',
      pgprojaddmilestone : 'template-page-project-addmilestone',
      pgprojadddiscussion : 'template-page-project-adddiscussion'
    },
    staticAnchors = {
      
      projects : 'projects',
      task : 'projects/task/',
      project : 'projects/project/',
      milestone : 'projects/{0}/milestone/{1}',
      discussion : 'projects/discussion/'
    },
    anchorRegExp = {
      
      projects : /^projects[\/]*$/,
      prj_mytasks : /^projects\/tasks[\/]*$/,
      prj_task : /^projects\/task\/([\w\d-]+)$/,
      prj_milestone : /^projects\/([\w\d-]+)\/milestone\/([\w\d-]+)$/,
      prj_discussion : /^projects\/discussion\/([\w\d-]+)$/,
      prj_project : /^projects\/project\/([\w\d-]+)$/,
      prj_milestones : /^projects\/project\/([\w\d-]*)\/milestones[\/]*$/,
      prj_discussions : /^projects\/project\/([\w\d-]*)\/discussions[\/]*$/,
      prj_projtasks : /^projects\/project\/([\w\d-]*)\/tasks[\/]*$/,
      prj_team : /^projects\/project\/([\w\d-]*)\/team[\/]*$/,
      prj_documents : /^projects\/project\/([\w\d-]+)\/documents\/([\w\d-]+)$/,
      prj_document : /^projects\/project\/([\w\d-]+)\/file\/([\w\d-]+)$/,
      prj_addtask : /^projects[\/]*([\w\d-]*)[\/milestone]*([\w\d-]*?)\/tasks\/add[\/]*$/,
      prj_addmilestone : /^projects[\/]*([\w\d-]*)\/milestones\/add[\/]*$/,
      prj_adddiscussion : /^projects[\/]*([\w\d-]*)\/discussions\/add[\/]*$/
    },
    customEvents = {
      changePage : 'onchangepage',
      addComment : 'onaddcomment',
      loadComments : 'onloadcomments',
      
      addProjectsComment : 'onaddprojectscomment',
      projectsPage : 'onprojectspage',
      myTasksPage : 'onmytaskspage',
      projectPage : 'onprojectpage',
      projectTasksPage : 'projecttaskspage',
      taskPage : 'ontaskpage',
      milestonePage : 'onmilestonepage',
      discussionPage : 'ondiscussionpage',
      tasksPage : 'ontaskspage',
      teamPage : 'onteampage',
      filesPage : 'onfilespage',
      milestonesPage : 'onmilestonespage',
      discussionsPage : 'ondiscussionspage',
      addTaskPage : 'onaddtaskpage',
      addMilestonePage : 'onaddmilestonepage',
      addDiscussionPage : 'onadddiscussionpage',
      getProjects : 'ongetprojects',
      getProjectTeam : 'ongetprojectteam',
      getProjectMilestones : 'ongetprojectmilestones',
      addTask : 'onaddtask',
      addMilestone : 'onaddmilestone',
      addDiscussion : 'onadddiscussion',
      projectDocumentsPage : 'onprojectdocumentspage',
      projectDocumentPage : 'ondocumentpage',
      addProjCommentPage : 'onaddprojcommentpage',
      getProject : 'ongetproject',
      updateTask : 'onupdatetask',
      updateTaskForm : 'onupdatetaskform',
      getProjectClosedTasks : 'ongetprojectclosedtasks'
    },
    eventManager = TeamlabMobile.extendEventManager(customEvents),
    dialogMarkCollection = [
      
    ];

  
  TeamlabMobile.extendModule(templateIds, anchorRegExp, staticAnchors, dialogMarkCollection);

  
  ASC.Controls.AnchorController.bind(anchorRegExp.projects, onProjectsAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_mytasks, onMyTasksAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_project, onProjectAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_projtasks, onProjectTasksAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_milestones, onMilestonesAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_milestone, onMilestoneAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_discussions, onDiscussionsAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_discussion, onDiscussionAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_team, onTeamAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_documents, onDocumentsAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_document, onDocumentAnch);

  ASC.Controls.AnchorController.bind(anchorRegExp.prj_task, onTaskAnch);

  ASC.Controls.AnchorController.bind(anchorRegExp.prj_addtask, onAddTaskAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_addmilestone, onAddMilestoneAnch);
  ASC.Controls.AnchorController.bind(anchorRegExp.prj_adddiscussion, onAddDiscussionAnch);

  
  function getFolderItem (item, project) {
  
    items = TeamlabMobile.getFolderItem(item);
    if (item.target === 'self') {
      item.href = item.type === 'file' ? '#projects/project/' + project.id + '/file/' + item.id : '#projects/project/' + project.id + '/documents/' + item.id;      
    }        
    return item;
  }

  
  function onProjectsAnch (params) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getPrjProjects(null, {success : onGetProjects, error : TeamlabMobile.throwException});
  }

  function onProjectAnch (params, id) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    if (id) {
      eventManager.call(customEvents.changePage, window, []);
      Teamlab.joint()
        .getPrjProject(null, id)
        .getPrjMilestones(null, id, null)
        .getPrjTasks(null, id)
        .getPrjDiscussions(null, id, null)
        .getPrjProjectTeamPersons(null, id)
        .getPrjProjectFolder(null, id)
        .start(null, {success : onGetProject, error : TeamlabMobile.throwException});
    }
  }

  function onMyTasksAnch (params) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getPrjTasks(null, '@self', {success : onGetMyTasks, error : TeamlabMobile.throwException});
  }

  function onProjectTasksAnch (params, projid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    Teamlab.joint()
      .getPrjProject(null, projid)
      .getPrjTasks(null, {filter : {projectid : projid}})
      //.getPrjTasks(null, projid, '@all', null, null)
      .start(null, {success : onGetProjectTasks, error : TeamlabMobile.throwException});
  }

  function onTaskAnch (params, id) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    if (id) {
      eventManager.call(customEvents.changePage, window, []);
      Teamlab.joint()
        .getPrjTask(null, id)
        .getPrjTaskComments(null, id)
        .start({}, {success : onGetTask, error : TeamlabMobile.throwException});
    }
  }

  function onMilestonesAnch (params, projid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    eventManager.call(customEvents.changePage, window, []);
    Teamlab.joint()
      .getPrjProject(null, projid)
      .getPrjMilestones(null, projid, null)
      .getPrjTasks(null, projid, '@all')
      .start(null, {success : onGetMilestones, error : TeamlabMobile.throwException});
  }

  function onMilestoneAnch (params, projid, id) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    if (id) {
      eventManager.call(customEvents.changePage, window, []);
      Teamlab.joint()
        .getPrjProject(null, projid)
        .getPrjMilestone(null, id)
        .getPrjTasks(null, null, null, null, {filter : {milestone : id}})
        .getPrjMilestoneComments(null, id)
        .start(null, {success : onGetMilestone, error : TeamlabMobile.throwException});
    }
  }

  function onDiscussionsAnch (params, projid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    eventManager.call(customEvents.changePage, window, []);
    Teamlab.joint()
      .getPrjProject(null, projid)
      .getPrjDiscussions(null, projid, null)
      .start(null, {success : onGetDiscussions, error : TeamlabMobile.throwException});
  }

  function onDiscussionAnch (params, id) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    if (id) {
      eventManager.call(customEvents.changePage, window, []);
      Teamlab.joint()
        .getPrjDiscussion(null, id)
        .getPrjDiscussionComments(null, id)
        .start(null, {success : onGetDiscussion, error : TeamlabMobile.throwException});
    }
  }

  function onTeamAnch (params, projid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    eventManager.call(customEvents.changePage, window, []);
    Teamlab.joint()
      .getPrjProject(null, projid)
      .getPrjTeam(null, projid)
      .start(null, {success : onGetTeam, error : TeamlabMobile.throwException});
  }

  function onDocumentsAnch (params, projid, folderid) {    
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    eventManager.call(customEvents.changePage, window, []);
    Teamlab.joint()
      .getPrjProject(null, projid)
      .getDocFolder(null, folderid)
      .start(null, {success : onGetFolder, error : TeamlabMobile.throwException});
  }

  function onDocumentAnch (params, projid, id) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }

    eventManager.call(customEvents.changePage, window, []);
    Teamlab.joint()
      .getPrjProject(null, projid)
      .getDocFile(null, id)
      .start(null, onGetFile);
  }

  function onAddTaskAnch (params, projid, milsid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.joint();
    Teamlab.getPrjProjects(null, {filter : {status : 'open'}});
    if (projid) {
      Teamlab
        .getPrjMilestones(null, {filter : {projectid : projid, status : 'open'}})
        .getPrjTeam(null, projid);
    }
    Teamlab.start({projid : projid, milsid : milsid}, onGetTaskFormData);
  }

  function onAddMilestoneAnch (params, projid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getPrjProjects({projid : projid}, {filter : {status : 'open'},success : onGetMilestoneFormData});
  }

  function onAddDiscussionAnch (params, projid, milsid) {
    if (!TeamlabMobile.validSession()) {
      ASC.Controls.AnchorController.move(TeamlabMobile.anchors.auth);
      return undefined;
    }
    eventManager.call(customEvents.changePage, window, []);
    Teamlab.getPrjProjects({projid : projid}, {filter : {status : 'open'},success : onGetDiscussionFormData});
  }

  

  function onGetProjects (params, items) {
    var
      item = null,
      itemsInd = items.length,
      mineItems = [], otherItems = [];

    items.sort(TeamlabMobile.ascSortByLowTitle);
    while (itemsInd--) {
      item = items[itemsInd];
      item.classname = item.type;
      item.href = 'projects/' + item.type + '/' + item.id;
      item.forMe === true ? mineItems.unshift(item) : otherItems.unshift(item);
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.projects)) {
      eventManager.call(customEvents.projectsPage, window, [{items : items, mine : mineItems, other : otherItems}]);
    }
  }

  function onGetMyTasks (params, items) {
    if (!items) {
      return undefined;
    }

    var
      item = null,
      itemsInd = items.length,
      openedItems = [], closedItems = [];

    items.sort(TeamlabMobile.ascSortByLowTitle);
    while (itemsInd--) {
      item = items[itemsInd];
      item.classname = item.type;
      item.href = 'projects/' + item.type + '/' + item.id;
      item.isOpened === true ? openedItems.unshift(item) : closedItems.unshift(item);
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_mytasks)) {
      eventManager.call(customEvents.myTasksPage, window, [{items : items, opened : openedItems, closed : closedItems}]);
    }
  }

  function onGetProject (params, project, milestones, tasks, discussions, persons, folder) {
    if (!project) {
      return undefined;
    }

    if (!project) {
      if (params.hasOwnProperty('__errors')) {
        TeamlabMobile.throwException(params, params.__errors);
        ASC.Controls.AnchorController.move(TeamlabMobile.anchors.projects);
      }
      return undefined;
    }

    project.milestones = milestones || [];
    project.tasks = tasks || [];
    project.discussions = discussions || [];
    project.persons = persons || [];
    project.documents = folder ? [].concat(folder.folders, folder.files) : [];
    if (project.security) {
      project.security.canReadTasks = tasks ? true : project.security.canReadTasks;
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_project)) {
      eventManager.call(customEvents.projectPage, window, [project, params]);
    }
  }

  function onGetTask (params, item, comments) {
    if (!item) {
      return undefined;
    }

    item.classname = item.type;
    item.comments = comments;

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_task)) {
      params.back = !TeamlabMobile.regexps.prj_task.test(ASC.Controls.AnchorController.getAnchor()) && !TeamlabMobile.regexps.prj_addtask.test(ASC.Controls.AnchorController.getAnchor()) ? ASC.Controls.AnchorController.getAnchor() : null;
      eventManager.call(customEvents.taskPage, window, [item, params]);
    }
  }

  function onGetProjectTasks (params, project, items) {
    if (!project || !items) {
      return undefined;
    }

    var
      item = null,
      itemsInd = items.length,
      o = {}, milestones = [];

    items.sort(TeamlabMobile.ascSortByLowTitle);
    while (itemsInd--) {
      item = items[itemsInd];
      item.classname = item.type;
      item.href = 'projects/' + item.type + '/' + item.id;

      if (!o.hasOwnProperty(item.milestoneId)) {
        o[item.milestoneId] = {
          id          : item.milestoneId,
          projid      : item.projectId,
          title       : item.milestoneTitle,
          tasks       : [],
          openeditems : [],
          closeditems : []
        };
      }
      o[item.milestoneId].tasks.unshift(item);
      item.isOpened ? o[item.milestoneId].openeditems.unshift(item) : o[item.milestoneId].closeditems.unshift(item);
    }

    for (var fld in o) {
      if (o.hasOwnProperty(fld)) {
        milestones.push(o[fld]);
      }
    }
    milestones.sort(TeamlabMobile.ascSortByLowTitle);

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_projtasks)) {
      eventManager.call(customEvents.projectTasksPage, window, [project, milestones, items, params]);
    }
  }

  function onGetProjectClosedTasks (params, items) {
    if (!items) {
      return undefined;
    }

    var
      projid = params.projid || -1, milsid = params.milsid || -1,
      item = null,
      itemsInd = items.length;

    items.sort(TeamlabMobile.ascSortByLowTitle);
    while (itemsInd--) {
      item = items[itemsInd];
      item.classname = item.type;
      item.href = 'projects/' + item.type + '/' + item.id;
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_projtasks)) {
      eventManager.call(customEvents.getProjectClosedTasks, window, [projid, milsid, items, params]);
    }
  }

  function onGetMilestones (params, project, items, tasks) {
    if (!project || !items) {
      return undefined;
    }

    var
      openedItems = [], closedItems = [],
      task = null,
      tasksInd = 0,
      item = null,
      itemsInd = items.length,
      itemId = null;
    while (itemsInd--) {
      item = items[itemsInd];
      itemId = item.id;
      item.classname = item.type;
      item.href = 'projects/' + project.id + '/' + item.type + '/' + item.id;
      item.tasks = [];
      item.closedtasks = [];

      tasksInd = tasks.length;
      while (tasksInd--) {
        task = tasks[tasksInd];
        if (itemId === task.milestoneId) {
          task.isOpened ? item.tasks.push(task) : item.closedtasks.push(task);
          tasks.splice(tasksInd, 1);
        }
      }

      item.status == 0 ? openedItems.unshift(item) : closedItems.unshift(item);
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_milestones)) {
      eventManager.call(customEvents.milestonesPage, window, [project, [].concat(openedItems, closedItems), params]);
    }
  }

  function onGetMilestone (params, project, item, tasks, comments) {
    if (!project || !item) {
      return undefined;
    }

    var itemId = item.id;
    item.classname = item.type;
    item.comments = comments;
    item.tasks = [];
    item.closedtasks = [];
    item.project = project;

    tasksInd = tasks.length;
    while (tasksInd--) {
      task = tasks[tasksInd];
      task.classname = task.type;
      task.href = 'projects/' + task.type + '/' + task.id;
      if (itemId === task.milestoneId) {
        task.isOpened ? item.tasks.push(task) : item.closedtasks.push(task);
        tasks.splice(tasksInd, 1);
      }
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_milestone)) {
      eventManager.call(customEvents.milestonePage, window, [item, params]);
    }
  }

  function onGetDiscussions (params, project, items) {
    if (!project || !items) {
      return undefined;
    }

    var 
      item = null,
      itemsInd = items.length;

    while (itemsInd--) {
      item = items[itemsInd];
      item.classname = item.type;
      item.href = 'projects/' + item.type + '/' + item.id;
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_discussions)) {
      eventManager.call(customEvents.discussionsPage, window, [project, items, params]);
    }
  }

  function onGetDiscussion (params, item, comments) {
    if (!item) {
      return undefined;
    }

    item.classname = item.type;
    item.comments = comments;

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_discussion)) {
      eventManager.call(customEvents.discussionPage, window, [item, params]);
    }
  }

  function onGetTeam (params, project, items) {
    if (!project || !items) {
      return undefined;
    }

    var item = null, profiles = [], managerId = project.responsible ? project.responsible.id : -1;
    for (var i = 0, n = items.length; i < n; i++) {
      item = items[i];
      item.classname = item.type;
      item.href = 'people/' + item.id;
      if (item.id === managerId) {
        item.isManager = true;
        profiles.unshift(item);
      } else {
        item.isManager = false;
        profiles.push(item)
      }
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_team)) {
      eventManager.call(customEvents.teamPage, window, [project, profiles, params]);
    }
  }

  function onGetFile (params, project, entry) {    
    if (!project || !entry) {
      return undefined;
    }

    if (!project || !entry) {
      return undefined;
    }

    entry.href = TeamlabMobile.getViewUrl(entry);
    switch ((entry.filetype || '').toLowerCase()) {
      case 'txt':
        entry.canEdit = true;
        break;
    }

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_document)) {
      params.back = 'projects/project/' + project.id + '/documents/' + entry.folderId;

      eventManager.call(customEvents.projectDocumentPage, window, [entry, params]);
    }
  }

  function onGetFolder (params, project, entry) { 
    if (!project || !entry) {
      return undefined;
    }
    if (!project || !entry) {
      return undefined;
    }

    var
      item = null,
      itemsInd = 0,
      permissions = {},
      items = null;

    items = [].concat(entry.folders).concat(entry.files);
    
    itemsInd = items ? items.length : 0;
    while (itemsInd--) {
      item = items[itemsInd];
      item = getFolderItem(item, project);
      
    }

    entry.title = !entry.parentId || entry.parentId == -1 ? project.title : entry.title;

    if (entry.parentFolder) {
      entry.parentFolder.classname = entry.parentFolder.type;
      entry.parentFolder.href = 'projects/project/' + project.id + '/documents/' + entry.parentFolder.id;
    } else {
      entry.parentFolder = {
        classname : entry.type,
        href      : 'projects/project/' + project.id
      };
    }

    permissions.canadd = entry.canAddItems;

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_documents)) {
      eventManager.call(customEvents.projectDocumentsPage, window, [items, entry.id, entry.parentFolder, entry.title, entry.rootType, entry.rootfolder, permissions]);
    }
  }

  function onAddComment (params, comment) {
    eventManager.call(customEvents.addProjectsComment, window, [comment, params]);
    eventManager.call(customEvents.addComment, window, [comment, params]);
  }

  function onAddTask (params, item) {
    item.classname = item.type;

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_addtask)) {
      ASC.Controls.AnchorController.lazymove(TeamlabMobile.anchors.task + item.id);
    }
  }

  function onAddMilestone (params, item) {
    item.classname = item.type;

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_addmilestone)) {
      ASC.Controls.AnchorController.lazymove(TeamlabMobile.anchors.milestone.format(item.projectId, item.id));
    }
  }

  function onAddDiscussion (params, item) {
    item.classname = item.type;

    if (ASC.Controls.AnchorController.testAnchor(anchorRegExp.prj_adddiscussion)) {
      ASC.Controls.AnchorController.lazymove(TeamlabMobile.anchors.discussion + item.id);
    }
  }

  function onGetDiscussionFormData (params, projects) {
    var
      myprojects = [], otherprojects = [],
      projid = params.projid || -1;

    var projectsInd = projects.length;
    while (projectsInd--) {
      projects[projectsInd].forMe ? myprojects.unshift(projects[projectsInd]) : otherprojects.unshift(projects[projectsInd]);
    }
    eventManager.call(customEvents.addDiscussionPage, window, [projid, [].concat(myprojects, otherprojects)]);
  }

  function onGetMilestoneFormData (params, projects) {
    var
      myprojects = [], otherprojects = [],
      projid = params.projid || -1;

    var projectsInd = projects.length;
    while (projectsInd--) {
      projects[projectsInd].forMe ? myprojects.unshift(projects[projectsInd]) : otherprojects.unshift(projects[projectsInd]);
    }
    eventManager.call(customEvents.addMilestonePage, window, [projid, [].concat(myprojects, otherprojects)]);
  }

  function onGetTaskFormData (params, projects, milestones, profiles) {
    params = (params && params.length ? params[params.length - 1] : {});

    var
      myprojects = [], otherprojects = [],
      projid = params.projid || -1,
      milsid = params.milsid || -1;

    var projectsInd = projects.length;
    while (projectsInd--) {
      projects[projectsInd].forMe ? myprojects.unshift(projects[projectsInd]) : otherprojects.unshift(projects[projectsInd]);
    }
    eventManager.call(customEvents.addTaskPage, window, [projid, milsid, [].concat(myprojects, otherprojects), milestones || [], profiles || []]);
  }

  function onUpdateTaskFormData (params, milestones, profiles) {
    params = (params && params.length ? params[params.length - 1] : {});

    var projid = params.projid || -1;

    eventManager.call(customEvents.updateTaskForm, window, [projid, milestones, profiles]);
  }

  function onUpdateTaskStatus (params, item) {
    eventManager.call(customEvents.updateTask, window, [item.id, item, item.status, item.statusname]);
  }

  
  Teamlab.getProjectsSearchEntries = function (items) {
    var itemsInd = items ? items.length : 0;
    while (itemsInd--) {
      var item = items[itemsInd];

      if (item.hasOwnProperty('items')) {
        var
          itemItem = null,
          itemItems = item.items,
          itemItemsInd = 0;
        itemItemsInd = itemItems ? itemItems.length : 0;
        while (itemItemsInd--) {
          itemItem = itemItems[itemItemsInd];
          switch (itemItem.type) {
            case 'file' :
              itemItem = getFolderItem(itemItem, item);
              break;
          }
        }
      }
    }
    return items;
  };

  TeamlabMobile.addProjectsItem = function (type, id, data) {
    var fn = null, callback = null;
    switch (type) {
      case 'task' :
        fn = Teamlab.addPrjTask;
        callback = onAddTask;
        break;
      case 'milestone' :
        fn = Teamlab.addPrjMilestone;
        callback = onAddMilestone;
        break;
      case 'discussion' :
        fn = Teamlab.addPrjDiscussion;
        callback = onAddDiscussion;
        break;
    }

    if (fn && callback) {
      return fn(null, id, data, callback);
    }
    return false;
  };

  TeamlabMobile.addProjectsComment = function (type, id, data) {
    var fn = null, callback = null;
    switch (type) {
      case 'task' :
        fn = Teamlab.addPrjTaskComment;
        callback = onAddComment;
        break;
      case 'milestone' :
        fn = Teamlab.addPrjMilestoneComment;
        callback = onAddComment;
        break;
      case 'discussion' :
        fn = Teamlab.addPrjDiscussionComment;
        callback = onAddComment;
        break;
    }

    if (fn && callback) {
      return fn({type : type}, id, data, callback);
    }
    return false;
  };

  TeamlabMobile.updateProjectsTaskFormData = function (projid) {
    if (projid) {
      Teamlab.joint()
        .getPrjMilestones(null,  {filter : {projectid : projid, status : 'open'}})
        .getPrjTeam(null, projid)
        .start({projid : projid}, onUpdateTaskFormData);
    }
  };

  TeamlabMobile.getProjectsClosedTasks = function (projid, milsid) {
    if (projid && milsid) {
      Teamlab.getPrjTasks({projid : projid, milsid : milsid}, {filter : {projectid : projid, milestone : milsid, status : 'closed'}, success : onGetProjectClosedTasks});
      return true;
    }
    return false;
  };

  TeamlabMobile.updateProjectsTaskStatus = function (itemid, newstatus) {
    Teamlab.updatePrjTask(null, itemid, {status : newstatus ? 'open' : 'closed'}, onUpdateTaskStatus);
  };

  return TeamlabMobile;
})(TeamlabMobile);
