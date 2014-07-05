<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-proj-tasks" type="text/x-jquery-tmpl" async="true">
{{each items}}
  <li class="item ${$value.classname}" data-itemid="${$value.id}" data-isclosed="{{if $value.statusname == 'closed'}}1{{else}}0{{/if}}" data-lowtitle="${$value.lowTitle}">
    <div class="ui-update-indicator update-status"></div>
    <input class="input-checkbox item-status update-projects-task-status{{if $value.canEdit === false}} disabled{{/if}}" type="checkbox" data-itemid="${$value.id}"{{if $value.statusname == 'closed'}} checked="checked"{{/if}} />
    <a class="ui-item-link title target-self" href="#${$value.href}">
      <span class="inner-text">${$value.title}</span>
    </a>
    <div class="sub-info">
      {{if $data.projectTitle === true}}
        {{if $value.projectTitle}}
          <span class="project-title">${$value.projectTitle}</span>
        {{/if}}
      {{else}}
        {{if $value.responsible}}
          <span class="project-responsible">${$value.responsible.displayName}</span>
        {{/if}}
      {{/if}}
      {{if $value.deadline}}
        <span class="timestamp{{if $value.isExpired}} is-expired{{/if}}{{if $value.deadlineToday}} deadline-today{{/if}}">
          {{if $value.deadlineToday}}
            <span class="timestamp-title"><%=Resources.MobileResource.LblToday%></span>
          {{else}}
            <span class="timestamp-title"><%=Resources.MobileResource.LblTo%></span>
            <span class="date">${$value.displayDateDeadline}</span>
          {{/if}}
        </span>
      {{/if}}
    </div>
  </li>
{{/each}}
</script>