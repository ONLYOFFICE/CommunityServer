<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-task" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects-item page-projects-task ui-header{{if item.comments}} loaded-comments{{/if}}">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-tasks ui-btn-left ui-btn-row target-back" href="#projects/tasks"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-title ui-task-title">
      <div class="ui-update-indicator update-status"></div>
      <input class="input-checkbox item-status update-projects-task-status{{if item.canEdit === false}} disabled{{/if}}" type="checkbox" data-itemid="${item.id}"{{if item.statusname === 'closed'}} checked="checked"{{/if}} />
      <span class="item-title task-title">
        <span class="inner-text">${item.title}</span>
      </span>
      <div class="sub-info">
        <span class="timestamp">
          <span class="date">${item.displayCrtdate}</span>
        </span>
        <span class="project-title">${item.projectTitle}</span>
      </div>
    </div>
    <div class="ui-item-content ui-task-content">{{html item.description}}</div>
    {{tmpl({item : item, classname : 'add-projects-task-comment', label : '<%=Resources.MobileResource.BtnAddComment%>'}) '#template-addcomment-block'}}
    <ul class="ui-item-comments">
      {{if item.comments}}
        {{tmpl({comments : item.comments}) '#template-comments'}}
      {{/if}}
    </ul>
    <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
    <span class="ui-btn load-comments target-update" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnLoadComments%></span></span></span>
    <a class="ui-btn add-comment target-self" href="#projects/task/${item.id}/comment/add"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnAddComment%></span></span></a>
  </div>
</div>
</script>