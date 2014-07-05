<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-project-tasks" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects page-projects-items page-projects-project-tasks ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-project ui-btn-left target-self" href="#projects/project/${projid}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    {{if security.canCreateTask === true}}
      <a class="ui-btn ui-btn-additem ui-btn-addtask ui-btn-right ui-btn-no-text target-self" href="#projects/${projid}/tasks/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}}
  </div>
  <div class="ui-content">
    <div class="ui-scroller none-iscroll">
      <div class="ui-page-title">
        <span class="title-text no-icon"><span><%=Resources.MobileResource.LblProjectTasks%></span></span>
      </div>
      {{if items.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrProjectNoTasks%></span>
        </div>
      {{else}}
        <ul class="ui-timeline">
          {{each items}}
            <li class="milestone-item<%--{{if $value.closeditems.length === 0}} no-closed-items{{/if}}--%>">
              <input class="milestone-id" type="hidden" value="${$value.id}" />
              <input class="milestone-projid" type="hidden" value="${$value.projid}" />
              <div class="item-title milestone-item-title"><span class="text">{{if $value.title}}${$value.title}{{else}}<%=Resources.MobileResource.LblUnsortedTasks%>{{/if}}</span></div>
              <ul class="milestone-tasks timeline-tasks one-type">
                {{tmpl({items : $value.openeditems}) '#template-proj-tasks'}}
                <li class="item-separator"></li>
              </ul>
              <div class="milestone-tasks closed-tasks">
                <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
                <span class="ui-btn load-closed-items load-closed-tasks" data-projectid="${$value.projid}" data-milestoneid="${$value.id}"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.LblClosedTasks%></span></span></span>
              </div>
            </li>
          {{/each}}
        </ul>
      {{/if}}
    </div>
  </div>
</div>
</script>