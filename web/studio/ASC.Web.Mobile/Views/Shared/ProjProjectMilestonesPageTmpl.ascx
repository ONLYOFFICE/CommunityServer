<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-project-milestones" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects page-projects-items page-projects-project-milestones ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-project ui-btn-left target-self" href="#projects/project/${projid}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    {{if security.canCreateMilestone === true}}
      <a class="ui-btn ui-btn-additem ui-btn-addmilestone ui-btn-right ui-btn-no-text target-self" href="#projects{{if projid && projid !== -1}}/${projid}{{/if}}{{if milsid}}/milestone/${milsid}{{/if}}/milestones/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}}
  </div>
  <div class="ui-content">
    <div class="ui-scroller none-iscroll">
      <div class="ui-page-title">
        <span class="title-text no-icon"><span><%=Resources.MobileResource.LblProjectMilestones%></span></span>
      </div>
      {{if items.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrProjectNoMilestones%></span>
        </div>
      {{else}}
        <ul class="ui-timeline">
          {{each items}}
            <li class="item milestone-item">
              <a class="ui-item-link title target-self" href="#${$value.href}">
                <span class="inner-text"><span class="text">${$value.title}</span></span>
                <span class="item-count opened-tasks">
                  <span class="count-value"><span class="count-text">${$value.tasks.length}</span></span>
                </span>
                <span class="item-count closed-tasks">
                  <span class="count-value"><span class="count-text">${$value.closedtasks.length}</span></span>
                </span>
              </a>
              <div class="sub-info">
                {{if $value.deadline}}
                  <span class="timestamp{{if $value.isExpired}} is-expired{{/if}}{{if $value.deadlineToday}} deadline-today{{/if}}">
                    {{if $value.deadlineToday}}
                      <span class="timestamp-title"><%=Resources.MobileResource.LblToday%></span>
                    {{else $value.isExpired}}
                      <span class="timestamp-title"><%=Resources.MobileResource.LblTo%></span>
                      <span class="date">${$value.displayDateDeadline}</span>
                    {{/if}}
                  </span>
                {{/if}}
              </div>
            </li>
          {{/each}}
          {{if $data.items.length > 0 && $data.closeditems.length > 0}}
            <li class="item-separator"></li>
          {{/if}}
          {{each closeditems}}
            <li class="item milestone-item closed-item">
              <a class="ui-item-link title target-self" href="#${$value.href}">
                <span class="inner-text"><span class="text">${$value.title}</span></span>
                <span class="item-count closed-tasks">
                  <span class="count-value"><span class="count-text">${$value.tasks.length + $value.closedtasks.length}</span></span>
                </span>
              </a>
              <div class="sub-info">
                {{if $value.deadline}}
                  <span class="timestamp">
                    <span class="timestamp-title"><%=Resources.MobileResource.LblClosed%></span>
                    <span class="date">${$value.displayDateDeadline}</span>
                  </span>
                {{/if}}
              </div>
            </li>
          {{/each}}
        </ul>
      {{/if}}
    </div>
  </div>
</div>
</script>