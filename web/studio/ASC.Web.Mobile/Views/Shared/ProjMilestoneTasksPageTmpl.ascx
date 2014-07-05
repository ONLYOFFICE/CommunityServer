<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-milestone-tasks" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects page-projects-items page-projects-milestone-tasks ui-header{{if item.comments}} loaded-comments{{/if}}">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-project ui-btn-left target-self" href="#projects/project/${item.projectId}/milestones"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    {{if item.project && item.project.security.canCreateTask === true}}
      <a class="ui-btn ui-btn-additem ui-btn-addtask ui-btn-right ui-btn-no-text target-self" href="#projects/${item.projectId}/milestone/${item.id}/tasks/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}}
  </div>
  <div class="ui-content">
    <div class="ui-scroller none-iscroll">
      <div class="ui-page-title">
        <span class="title-text no-icon"><span><%=Resources.MobileResource.LblMilestoneTasks%></span></span>
      </div>
      {{if item.tasks.length === 0 && item.closedtasks.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrMilestoneNoTasks%></span>
        </div>
      {{else}}
        <ul class="ui-timeline tasks-timeline{{if item.tasks.length === 0 || item.closedtasks.length === 0}} one-type{{/if}}">
          {{tmpl({items : item.tasks}) '#template-proj-tasks'}}
          <li class="item-separator"></li>
          {{tmpl({items : item.closedtasks}) '#template-proj-tasks'}}
        </ul>
      {{/if}}
      {{tmpl({item : item, classname : 'add-projects-milestone-comment', label : '<%=Resources.MobileResource.BtnAddComment%>'}) '#template-addcomment-block'}}
      <ul class="ui-item-comments">
        {{if item.comments}}
          {{tmpl({comments : item.comments}) '#template-comments'}}
        {{/if}}
      </ul>
      <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
      <span class="ui-btn load-comments target-update" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnLoadComments%></span></span></span>
    </div>
  </div>
</div>
</script>