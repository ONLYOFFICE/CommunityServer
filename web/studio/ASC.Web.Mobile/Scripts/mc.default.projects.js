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


;window.DefaultMobile = (function (DefaultMobile) {
  if (!DefaultMobile) {
    console.log('Default.default : has no DefaultMobile');
    return DefaultMobile;
  }

  
  function getCommentData ($page) {
    var 
      data = {
        commenttype : $page.find('input.comment-type:first').removeClass('error-field').val(),
        id          : $page.find('input.comment-id:first').removeClass('error-field').val(),
        parentid    : $page.find('input.comment-parentid:first').removeClass('error-field').val(),
        subject     : $page.find('input.comment-subject:first').removeClass('error-field').val(),
        content     : $page.find('textarea.comment-content:first').removeClass('error-field').val()
      };

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        switch (fld) {
          case 'subject':
            continue;
        }
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    data.parentid = data.parentid ? data.parentid : '00000000-0000-0000-0000-000000000000';

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'commenttype'  : errors.push($page.find('input.comment-type:first').addClass('error-field')); break;
          case 'id'           : errors.push($page.find('input.comment-id:first').addClass('error-field')); break;
          case 'content'      : errors.push($page.find('textarea.comment-content:first').addClass('error-field')); break;
        }
      }
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  function getTaskData ($page) {
    var data = {
      projectid   : $page.find('div.taskform-select-projectid:first').removeClass('error-field').find('select').val(),
      milestoneid : $page.find('select.task-milestoneid:first').removeClass('error-field').val(),
      responsible : $page.find('select.task-responsibleid:first').removeClass('error-field').val(),
      description : $page.find('textarea.task-description:first').removeClass('error-field').val() || '',
      deadline    : $page.find('input.task-deadline:first').removeClass('error-field').scroller('getDate'),
      title       : $page.find('input.task-title:first').removeClass('error-field').val() || ''
    };

    data.deadline = TeamlabMobile.verificationDate(data.deadline);

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        switch (fld) {
          case 'deadline':
          case 'description':
            continue;
        }
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    if (data.milestoneid === null) {
      delete data.milestoneid;
    }

    if (data.responsible === null) {
      delete data.responsible;
    }

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'projectid'    : errors.push($page.find('div.taskform-select-projectid:first').addClass('error-field')); break;
          case 'description'  : errors.push($page.find('textarea.task-description:first').addClass('error-field')); break;
          case 'deadline'     : errors.push($page.find('input.task-deadline:first').addClass('error-field')); break;
          case 'title'        : errors.push($page.find('input.task-title:first').addClass('error-field')); break;
        }
      }
    }

    if (errors.length === 0) {
        return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  function getMilestoneData ($page) {
    var data = {
      projectid   : $page.find('select.milestone-projectid:first').removeClass('error-field').val(),
      description : $page.find('textarea.milestone-description:first').removeClass('error-field').val() || '',
      deadline    : $page.find('input.milestone-deadline:first').removeClass('error-field').scroller('getDate'),
      title       : $page.find('input.milestone-title:first').removeClass('error-field').val() || ''
    };

    data.deadline = TeamlabMobile.verificationDate(data.deadline);

    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        switch (fld) {
          case 'description':
            continue;
        }
        data[fld] = TeamlabMobile.verificationValue(data[fld]);
      }
    }

    var errors = [];
    for (var fld in data) {
      if (data.hasOwnProperty(fld)) {
        if (data[fld] !== null) {
          continue;
        }
        switch (fld) {
          case 'projectid'  : errors.push($page.find('select.milestone-projectid:first').addClass('error-field')); break;
          case 'deadline'   : errors.push($page.find('input.milestone-deadline:first').addClass('error-field')); break;
          case 'title'      : errors.push($page.find('input.milestone-title:first').addClass('error-field')); break;
        }
      }
    }

    if (errors.length === 0) {
      return data;
    }

    ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
    return null;
  }

  function getDiscussionData ($page) {
    var data = {
          projectid : $page.find('select.discussion-projectid:first').removeClass('error-field').val(),
          content   : $page.find('textarea.discussion-text:first').removeClass('error-field').val() || '',
          title     : $page.find('input.discussion-title:first').removeClass('error-field').val() || ''
        };

        for (var fld in data) {
          if (data.hasOwnProperty(fld)) {
            data[fld] = TeamlabMobile.verificationValue(data[fld]);
          }
        }

        var errors = [];
        for (var fld in data) {
          if (data.hasOwnProperty(fld)) {
            if (data[fld] !== null) {
              continue;
            }
            switch (fld) {
              case 'projectid'  : errors.push($page.find('select.discussion-projectid:first').addClass('error-field')); break;
              case 'content'    : errors.push($page.find('textarea.discussion-text:first').addClass('error-field')); break;
              case 'title'      : errors.push($page.find('input.discussion-title:first').addClass('error-field')); break;
            }
          }
        }

        if (errors.length === 0) {
          return data;
        }

        ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
        return null;
  }

  
  DefaultMobile.load_closed_tasks = function (evt, $page, $button) {
    var $milestone = $button.parents('li.milestone-item:first');

    if (!$milestone.hasClass('loading-closed-items')) {
      var
        projid = $button.attr('data-projectid'),
        milsid = $button.attr('data-milestoneid');

      if (projid && milsid && TeamlabMobile.getProjectsClosedTasks(projid, milsid)) {
        $milestone.addClass('loading-closed-items');
      }
    }
  };

  DefaultMobile.add_projects_milestone_comment = function (evt, $page, $button) {
    var data = getCommentData($page);
    if (data && TeamlabMobile.addProjectsComment('milestone', data.id, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.add_projects_task_comment = function (evt, $page, $button) {
    var data = getCommentData($page);
    if (data && TeamlabMobile.addProjectsComment('task', data.id, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.add_projects_discussion_comment = function (evt, $page, $button) {
    var data = getCommentData($page);
    if (data && TeamlabMobile.addProjectsComment('discussion', data.id, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.add_projects_task = function (evt, $page, $button) {
    var data = getTaskData($page);
    if (data && TeamlabMobile.addProjectsItem('task', data.projectid, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.add_projects_milestone = function (evt, $page, $button) {
    var data = getMilestoneData($page);
    if (data && TeamlabMobile.addProjectsItem('milestone', data.projectid, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.add_projects_discussion = function (evt, $page, $button) {
    var data = getDiscussionData($page);
    if (data && TeamlabMobile.addProjectsItem('discussion', data.projectid, data)) {
      jQuery(document).trigger('changepage');
      $button.addClass('disabled');
    }
  };

  DefaultMobile.taskform_select_projectid = function (evt, $page, $select) {
    var
      projid = $page.find('select.task-projectid:first').val() || -1;

    if (!projid || projid == -1) {
      $page.find('select.task-responsibleid').addClass('disabled').val(-1).attr('disabled', true);
      $page.find('select.task-milestoneid').addClass('disabled').val(-1).attr('disabled', true);
    } else {
      $page.addClass('projectteam-loading').addClass('projectmilestones-loading');
      $page.find('select.task-responsibleid').parent('div.custom-select:first').remove();
      $page.find('select.task-responsibleid').remove();
      $page.find('select.task-milestoneid').parent('div.custom-select:first').remove();
      $page.find('select.task-milestoneid').remove();
      TeamlabMobile.updateProjectsTaskFormData(projid);
    }
  };

  DefaultMobile.update_projects_task_status = function (evt, $page, $checkbox) {
    var itemid = $checkbox.attr('data-itemid');

    if (itemid) {
      $page.addClass('update-status');
      $checkbox.parents('li.item.:first').addClass('update-status');
      TeamlabMobile.updateProjectsTaskStatus(itemid, !$checkbox.is(':checked'));
    }
  };

  return DefaultMobile;
})(DefaultMobile);

;(function($) {
  
  TeamlabMobile.bind(TeamlabMobile.events.projectsPage, onProjectsPage);
  TeamlabMobile.bind(TeamlabMobile.events.myTasksPage, onMyTasksPage);
  TeamlabMobile.bind(TeamlabMobile.events.projectPage, onProjectPage);
  TeamlabMobile.bind(TeamlabMobile.events.projectTasksPage, onProjectTasksPage);
  TeamlabMobile.bind(TeamlabMobile.events.teamPage, onTeamPage);
  TeamlabMobile.bind(TeamlabMobile.events.projectDocumentsPage, onDocumentsPage);
  TeamlabMobile.bind(TeamlabMobile.events.milestonesPage, onMilestonesPage);
  TeamlabMobile.bind(TeamlabMobile.events.discussionsPage, onDiscussionsPage);
  TeamlabMobile.bind(TeamlabMobile.events.taskPage, onTaskPage);
  TeamlabMobile.bind(TeamlabMobile.events.projectDocumentPage, onDocumentPage);
  TeamlabMobile.bind(TeamlabMobile.events.milestonePage, onMilestonePage);
  TeamlabMobile.bind(TeamlabMobile.events.discussionPage, onDiscussionPage);
  TeamlabMobile.bind(TeamlabMobile.events.addTaskPage, onAddTaskPage);
  TeamlabMobile.bind(TeamlabMobile.events.addMilestonePage, onAddMilestonePage);
  TeamlabMobile.bind(TeamlabMobile.events.addDiscussionPage, onAddDiscussionPage);

  TeamlabMobile.bind(TeamlabMobile.events.addTask, onAddTask);
  TeamlabMobile.bind(TeamlabMobile.events.addMilestone, onAddMilestone);
  TeamlabMobile.bind(TeamlabMobile.events.addDiscussion, onAddDiscussion);

  TeamlabMobile.bind(TeamlabMobile.events.updateTask, onUpdateTask);
  TeamlabMobile.bind(TeamlabMobile.events.updateTaskForm, onUpdateTaskForm);

  TeamlabMobile.bind(TeamlabMobile.events.getProjectClosedTasks, onGetClosedTasks);

  TeamlabMobile.bind(TeamlabMobile.events.addProjectsComment, onAddComment);

  function insertIntoCollection ($item, $items) {
    var itemLowTitle = $item.attr('data-lowtitle');
    var itemsInd = $items.length;
    while (itemsInd--) {
      if (itemLowTitle > $items[itemsInd].getAttribute('data-lowtitle')) {
        $($items[itemsInd]).after($item);
        break;
      }
    }
  }

  
  function onAddComment (data, params) {
    var $page = $();
    var comment = data;

    if (params.hasOwnProperty('type')) {
      switch (params.type) {
        case 'task' :
          $page = $('div.page-projects-task.ui-page-active:first');
          break;
        case 'milestone' :
          $page = $('div.page-projects-milestone-tasks.ui-page-active:first');
          break;
        case 'discussion' :
          $page = $('div.page-projects-discussion.ui-page-active:first');
          break;
      }
    }

    if ($page.length === 0) {
      return undefined;
    }

    data = {comments : [data]};

    var $comments = $page.find('ul.ui-item-comments:first');
    if (comment.parentId) {
      $comments.find('li.item-comment[data-commentid="' + comment.parentId + '"]:first ul.inline-comments:first').append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbcomments, data));
    } else {
      $comments.append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbcomments, data));
    }

    jQuery(document).trigger('updatepage');
  }

  function onProjectsPage (data) {
    data = {pagetitle : ASC.Resources.LblProjectsTitle, type : 'projects-page', onetype : data.mine.length === 0 || data.other.length === 0, mineitems : data.mine, otheritems : data.other};

    var $page = DefaultMobile.renderPage('projects-page', 'page-projects', 'projects', ASC.Resources.LblMyProjects, data).addClass('filter-none');
  }

  function onProjectPage (data, params) {
    var item = null, items = null, itemsInd = 0, documentscount = 0, openedtaskscount = 0;

    openedtaskscount = 0;
    items = data.tasks;
    itemsInd = items ? items.length : 0;
    while (itemsInd--) {
      if (items[itemsInd].isOpened === true) {
        openedtaskscount += 1;
      }
    }

    documentscount = 0;
    items = data.documents;
    itemsInd = items ? items.length  : 0;
    while (itemsInd--) {
      item = items[itemsInd];
      if (item.type === 'file') {
        documentscount++;
      } else if (item.type === 'folder') {
        documentscount += item.filesCount;
      }
    }
    data = {
      item : data,
      pagetitle : data.title,
      milestonescount : data.milestones ? data.milestones.length : 0,
      discussionscount : data.discussions ? data.discussions.length : 0,
      personscount : data.persons ? data.persons.length : 0,
      taskscount : data.tasks ? openedtaskscount : 0,
      documentscount : data.documents ? documentscount : 0
   };

    var $page = DefaultMobile.renderPage('projects-project-page', 'page-projects-project', 'projects-project-' + data.item.id, 'Project', data);
  }

  function onMyTasksPage (data) {
    data = {pagetitle : ASC.Resources.LblMyTasks, type : 'projects-page-tasks', items : data.opened, closeditems : data.closed, projid : -1, milsid : -1};

    var $page = DefaultMobile.renderPage('projects-tasks-page', 'page-projects-tasks', 'projects-project-tasks', ASC.Resources.LblMyTasks, data).addClass('filter-tasks');
  }

  function onProjectTasksPage (project, data, items, params) {
    data = {pagetitle : project.title, type : 'projects-page-tasks', items : data, projid : project.id, security : project.security};

    var $page = DefaultMobile.renderPage('projects-tasks-page', 'page-projects-project-tasks', 'projects-project-tasks', ASC.Resources.LblMyTasks, data).addClass('filter-tasks');
  }

  function onTaskPage (data, params) {
    data = {pagetitle : data.projectTitle, title : ASC.Resources.LblProjectsTitle, back : null, item : data};

    if (params.hasOwnProperty('back') && params.back) {
      data.back = params.back;
    }

    var $page = DefaultMobile.renderPage('projects-task-page', 'page-projects-task', 'projects-task-' + data.item.id, ' ', data);
  }

  function onMilestonesPage (project, data, params) {
    var item = null, items = data, itemsInd = 0, closeditems = [], openeditems = [];
    itemsInd = items.length;
    while (itemsInd--) {
      item = items[itemsInd];
      item.status == 0 ? openeditems.push(item) : closeditems.push(item);
    }
    data = {pagetitle : project.title, type : 'projects-page-milestones', items : openeditems, closeditems : closeditems, projid : project.id, security : project.security};

    var $page = DefaultMobile.renderPage('projects-milestones-page', 'page-projects-project-milestones', 'projects-project-milestones', 'My milestones', data).addClass('filter-milestones');
  }

  function onMilestonePage (data, params) {
    data = {pagetitle : data.title, type : 'projects-page-tasks', item : data};

    var $page = DefaultMobile.renderPage('projects-tasks-page', 'page-projects-milestone-tasks', 'projects-project-tasks', ASC.Resources.LblMyTasks, data).addClass('filter-milestone-tasks');
  }

  function onDiscussionsPage (project, data, params) {
    data = {pagetitle : project.title, type : 'projects-page-discussions', items : data, projid : project.id, security : project.security};

    var $page = DefaultMobile.renderPage('projects-discussions-page', 'page-projects-project-discussions', 'projects-project-discussions', ASC.Resources.LblMyDiscussions, data).addClass('filter-discussions');
  }

  function onDiscussionPage (data, params) {
    data = {pagetitle : data.title, title : ASC.Resources.LblProjectsTitle, item : data};

    var $page = DefaultMobile.renderPage('projects-discussion-page', 'page-projects-discussion', 'projects-discussion-' + data.item.id, ASC.Resources.LblMyDiscussions, data);
  }

  function onTeamPage (project, data, params) {
    data = {pagetitle : project.title, type : 'projects-page-team', items : data, projid : project.id, security : project.security, isSinglePersone : data.length === 1};

    var $page = DefaultMobile.renderPage('projects-team-page', 'page-projects-project-team', 'projects-project-team', ASC.Resources.LblMyTeam, data).addClass('filter-team');
  }

  function onDocumentsPage (data, id, parent, foldertitle, rootfoldertype, rootfolder, permissions) {
    data = {pagetitle : foldertitle, type : 'projects-page-documents', id : id, parent : parent, rootfoldertype : rootfoldertype, permissions : permissions, items : data};

    var $page = DefaultMobile.renderPage('projects-files-page', 'page-projects-project-files', 'projects-project-files', ASC.Resources.LblMyFiles, data).addClass('filter-documents');
  }

  function onDocumentPage (data, params) {
    data = {pagetitle : data.filename, type : 'projects-page-documents-item', item : data, filetype : data.filetype, folderid : data.folderId, back : null};

    if (params.hasOwnProperty('back') && params.back) {
      data.back = params.back;
    }

    var $page = DefaultMobile.renderPage('documents-item-page', 'page-documents-file', 'documents-file', ASC.Resources.LblDocumentsTitle, data);
  }

  function onAddTaskPage (projid, milsid, projects, milestones, responsibles) {
    data = {pagetitle : ASC.Resources.LblProjectsTitle, title : ' ', type : 'projects-addtask', projid : projid, milsid : milsid, projects : projects, milestones : milestones, responsibles : responsibles};

    var $page = DefaultMobile.renderPage('projects-addtask-page', 'page-projects-addtask', 'projects-addtask' + Math.floor(Math.random() * 1000000), ' ', data);
  }

  function onAddMilestonePage (projid, projects) {
    data = {pagetitle : ASC.Resources.LblProjectsTitle, title : ' ', projid : projid, type : 'projects-addmilestone', projects : projects};

    var $page = DefaultMobile.renderPage('projects-addmilestone-page', 'page-projects-addmilestone', 'projects-addmilestone' + Math.floor(Math.random() * 1000000), ' ', data);
  }

  function onAddDiscussionPage (projid, projects) {
    data = {pagetitle : ASC.Resources.LblProjectsTitle, title : ' ', projid : projid, type : 'projects-adddiscussion', projects : projects};

    var $page = DefaultMobile.renderPage('projects-adddiscussion-page', 'page-projects-adddiscussion', 'projects-adddiscussion' + Math.floor(Math.random() * 1000000), ' ', data);
  }

  /* *** */
  function onAddTask (data) {
    var taskid = data.hasOwnProperty('id') ? data.id : null;
    if (taskid) {
      var href = TeamlabMobile.anchors.task + taskid;
      ASC.Controls.AnchorController.lazymove({ back : ASC.Controls.AnchorController.getLastAnchor() }, href.length === 0 || href === '/' ? '#' : href);
    }
  }

  function onAddMilestone (data) {
    var milestoneid = data.hasOwnProperty('id') ? data.id : null;
    if (milestoneid) {
      var href = TeamlabMobile.anchors.milestone + milestoneid;
      ASC.Controls.AnchorController.lazymove(href.length === 0 || href === '/' ? '#' : href);
    }
  }

  function onAddDiscussion (data) {
    var discussionid = data.hasOwnProperty('id') ? data.id : null;
    if (discussionid) {
      var href = TeamlabMobile.anchors.discussion + discussionid;
      ASC.Controls.AnchorController.lazymove(href.length === 0 || href === '/' ? '#' : href);
    }
  }

  function onUpdateTaskForm (projid, milestones, profiles) {
    $page = $('div.page-projects-additem-task.ui-page-active:first').removeClass('projectteam-loading').removeClass('projectmilestones-loading');
    $page.find('div.task-item-container.item-milestoneid:first')
      .append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbprojmilestones, {items : milestones, classname : 'task-milestoneid'}));
    $page.find('div.task-item-container.item-responsibleid:first')
      .append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbprojteam, {items : profiles, classname : 'task-responsibleid'}));
    jQuery(document).trigger('updatepage');
  }

  function onGetClosedTasks (projid, milsid, data, params) {
    data = {items : data};

    var $milestone = $('input[value="' + milsid + '"]:first').parents('li.milestone-item:first');
    if ($milestone.length > 0) {
      var $page = $milestone.parents('div.ui-page.ui-page-active:first');
      $milestone
        .removeClass('loading-closed-items')
        .addClass('loaded-closed-items')
        .find('ul.milestone-tasks:first')
        .append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbtasks, data));

      if ($milestone.find('li[data-isclosed="0"]').length === 0 || $milestone.find('li[data-isclosed="1"]').length === 0) {
        $milestone.find('ul.timeline-tasks:first').addClass('one-type');
      } else {
        $milestone.find('ul.timeline-tasks:first').removeClass('one-type');
      }

      DefaultMobile.renderCheckbox($page);
      
      jQuery(document).trigger('updatepage');
    }
  }

  function onUpdateTask (taskid, task, statusid, statusname) {
    var $page = null, $checkboxes = $(), $item = null, $checkbox = null, $label = null;

    $page = $('div.page-projects-task[data-itemid="' + taskid + '"]').removeClass('update-status');
    if ($page.length > 0) {       
      $checkboxes = $checkboxes.add($page.find('input.input-checkbox.item-status'));
    }

    $page = $('div.page-projects').removeClass('update-status');
    if ($page.length > 0) {       
      $checkboxes = $checkboxes.add($page.find('li.item.task[data-itemid="' + taskid + '"] input.input-checkbox.item-status'));
    }        
    $checkboxes.parents('li.item').removeClass('update-status');

    var 
      $milestoneitem = null,
      $openeditems = null,
      $closeditems = null,
      checkboxesInd = 0;

    checkboxesInd = $checkboxes.length;
    while (checkboxesInd--) {
      $checkbox = $($checkboxes[checkboxesInd]);
      $label = $('label[for="' + $checkboxes[checkboxesInd].id + '"]');

      switch (statusname.toLowerCase()) {
        case 'open' :
          $checkbox.attr('checked', false);
          $label.removeClass('checked');

          $item = $checkbox.parents('li.item:first');
          if ($item.length > 0) {
            $item.attr('data-isclosed', '0')

            $openeditems = $item.siblings('li[data-isclosed="0"]');
            $closeditems = $item.siblings('li[data-isclosed="1"]');
            $item.parent().prepend($item);

            if ($closeditems.length === 0) {
              $item.parent().addClass('one-type');
            } else {
              $item.parent().removeClass('one-type');
            }

            insertIntoCollection($item, $openeditems);
          }
          break;
        case 'closed' :
          $checkbox.attr('checked', true);
          $label.addClass('checked');

          $item = $checkbox.parents('li.item:first');

          if ($item.length > 0) {
            $item.attr('data-isclosed', '1');
            $item.parent().parent().removeClass('no-closed-items');

            if (($milestoneitem = $item.parents('li.milestone-item:first')).length > 0) {
              if (!$milestoneitem.hasClass('loaded-closed-items')) {
                $item.remove();
                continue;
              }
            }

            $openeditems = $item.siblings('li[data-isclosed="0"]');
            $closeditems = $item.siblings('li[data-isclosed="1"]');
            $item.siblings('li.item-separator').after($item);

            if ($openeditems.length === 0) {
              $item.parent().addClass('one-type');
            } else {
              $item.parent().removeClass('one-type');
            }

            insertIntoCollection($item, $closeditems);
          }
          break;
      }
    }
  }
})(jQuery);
