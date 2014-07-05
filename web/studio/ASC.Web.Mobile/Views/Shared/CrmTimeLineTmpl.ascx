<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-crm-timeline" type="text/x-jquery-tmpl" async="true">
{{each items}}
  <li class="item ${$value.contactclass}{{if $value.hasdelayedtasks}} has-delayed{{/if}}" data-itemname="${$value.displayName}">
    <div class="item-state white"><div class="item-icon"></div></div>
    <a class="ui-item-link title target-self" href="#${$value.href}">
      {{if $value.company}}
          <span class="inner-text">${$value.displayName}</span>
          <div class="item-company"><div class="white"><div class="company-icon"></div></div><span class="company-name">${$value.company.displayName}</span></div>
      {{else}}
        <span class="inner-text center">${$value.displayName}</span>
      {{/if}}
      </a>
      {{if $value.haveLateTasks}}
          <a class = "count" href = "#${$value.href}/tasks">
              <span class="item-count-late">
                <span class="count-value-late"><span class="count-text-late">${$value.taskCount}</span></span>
              </span>
          </a>
      {{else}}
          <a class = "count" href = "#${$value.href}/tasks">
              <span class="item-count">
                <span class="count-value"><span class="count-text">${$value.taskCount}</span></span>
              </span>
          </a>
      {{/if}}  
  </li>
{{/each}}
</script>