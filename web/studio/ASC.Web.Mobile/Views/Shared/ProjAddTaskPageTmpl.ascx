<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-project-addtask" type="text/x-jquery-tmpl" async="true">
<div class="ui-page ui-page-relative page-additem page-projects-additem page-projects-additem-task ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-tasks ui-btn-left ui-btn-row target-back none-shift-back" href="#projects"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addtask-content">
      <div class="item-container task-item-container item-tasktitle">
        <label><%=Resources.MobileResource.LblTitle%>:</label>
        <input class="task-title" type="text" />
      </div>
      <div class="item-container task-item-container item-description">
        <label><%=Resources.MobileResource.LblDescription%>:</label>
        <textarea class="task-description"></textarea>
      </div>
      <div class="item-container task-item-container item-deadline">
        <input class="task-deadline" type="datepick" readonly="readonly" value="<%=Resources.MobileResource.LblDeadline%>" />
      </div>
      <div class="item-container task-item-container item-projectid">
        {{tmpl({items : projects, classname : 'task-projectid taskform-select-projectid', selectedid : projid}) '#template-projprojects'}}
      </div>
      <div class="item-container task-item-container item-milestoneid">
        <div class="loading-indicator">&nbsp;</div>
        {{tmpl({items : milestones, classname : 'task-milestoneid', selectedid : milsid, disabled : projid == -1}) '#template-projmilestones'}}
      </div>
      <div class="item-container task-item-container item-responsibleid">
        <div class="loading-indicator">&nbsp;</div>
        {{tmpl({items : responsibles, classname : 'task-responsibleid', disabled : projid == -1}) '#template-projteam'}}
      </div>
      <div class="item-container task-item-container create-task">
        <button class="create-item create-task add-projects-task"><%=Resources.MobileResource.BtnCreateTask%></button>
      </div>
    </div>
  </div>
</div>
</script>