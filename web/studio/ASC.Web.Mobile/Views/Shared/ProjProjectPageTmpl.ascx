<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-project" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects-item page-projects-project ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-projects ui-btn-left" href="#projects"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.LblProjects%></span></span></span></a>
    {{if item.security.canCreateTask === true}}
      <a class="ui-btn ui-btn-additem ui-btn-addtask ui-btn-right ui-btn-no-text target-self" href="#projects/${item.id}/tasks/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}}
  </div>
  <div class="ui-content">
    <div class="ui-item-title ui-project-title">
      <span class="item-title project-title">
        <span class="inner-text">${item.description}</span>
      </span>
      <div class="sub-info">
        <a class="responsible full-length" href="#people/${item.responsible.id}"><%=Resources.MobileResource.LblManager%><span class="name">${item.responsible.displayName}</span></a>
      </div>
    </div>
    <div class="ui-item-content ui-project-content">
      <ul class="ui-timeline">
        {{if item.security.canReadMilestones === true}}
          <li class="item ui-dscr-item item-milestones">
            <a class="ui-item-link title target-self" href="#projects/project/${item.id}/milestones">
              <span class="item-label"><%=Resources.MobileResource.LblMilestones%></span>
              <span class="item-count">
                <span class="count-value"><span class="count-text">${milestonescount}</span></span>
              </span>
            </a>
          </li>
        {{/if}}
        {{if item.security.canReadTasks === true}}
          <li class="item ui-dscr-item item-tasks">
            <a class="ui-item-link title target-self" href="#projects/project/${item.id}/tasks">
              <span class="item-label"><%=Resources.MobileResource.LblTasks%></span>
              <span class="item-count">
                <span class="count-value"><span class="count-text">${taskscount}</span></span>
              </span>
            </a>
          </li>
        {{/if}}
        {{if item.security.canReadMessages === true}}
          <li class="item ui-dscr-item item-discussions">
            <a class="ui-item-link title target-self" href="#projects/project/${item.id}/discussions">
              <span class="item-label"><%=Resources.MobileResource.LblDiscussions%></span>
              <span class="item-count">
                <span class="count-value"><span class="count-text">${discussionscount}</span></span>
              </span>
            </a>
          </li>
        {{/if}}
        <li class="item ui-dscr-item item-team">
          <a class="ui-item-link title target-self" href="#projects/project/${item.id}/team">
            <span class="item-label"><%=Resources.MobileResource.LblTeam%></span>
            <span class="item-count">
              <span class="count-value"><span class="count-text">${personscount}</span></span>
            </span>
          </a>
        </li>
        {{if item.security.canReadFiles === true}}
          <li class="item ui-dscr-item item-documents">
            <a class="ui-item-link title target-self" href="#projects/project/${item.id}/documents/${item.projectFolder}">
              <span class="item-label"><%=Resources.MobileResource.LblDocuments%></span>
              <span class="item-count">
                <span class="count-value"><span class="count-text">${documentscount}</span></span>
              </span>
            </a>
          </li>
        {{/if}}
      </ul>
    </div>
  </div>
</div>
</script>