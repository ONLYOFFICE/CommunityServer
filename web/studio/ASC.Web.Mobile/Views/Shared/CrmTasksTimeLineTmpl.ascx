<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-crm-tasks-timeline" type="text/x-jquery-tmpl" async="true">
{{each items}}  
  <li class="item ${$value.classname}" {{if $value.isClosed}}data-isclosed = '1'{{else}}data-isclosed = '0'{{/if}} data-itemid = ${$value.id}>    
    <div class="ui-update-indicator update-status"></div>
    <input class="input-checkbox item-status{{if $value.canedit === false}} disabled{{/if}}" type="checkbox" data-itemid="${$value.id}"{{if $value.isClosed}} checked="checked"{{/if}} />
    <a class="ui-item-link title target-self" {{if $data.contactId != null}}href="#crm/contact/${contactId}/task/${id}"{{else}}href="#${$value.href}"{{/if}}>
        <span class="inner-text {{if $value.isClosed}}closed {{else}} {{if $value.contact && $value.isOverdue}}overdue{{/if}}{{/if}}">${$value.title}</span>
        {{if $value.contact}}
            <div class="{{if $value.contact.isCompany}}item-company{{else}}item-person{{/if}}"><span class="contact-name">${$value.contact.displayName}</span></div>
        {{/if}}          
      <span class="item-count">
        {{if $value.deadlineDate}}<span class="count-text{{if $value.isClosed}} closed{{/if}}">${$value.deadlineDate}</span>{{/if}}
        {{if $value.deadlineTime}}<span class="count-text{{if $value.isClosed}} closed{{/if}}">${$value.deadlineTime}</span>{{/if}}
      </span>
    </a>
  </li>  
{{/each}}
</script>