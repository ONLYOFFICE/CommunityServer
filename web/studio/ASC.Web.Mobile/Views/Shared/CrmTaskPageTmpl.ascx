<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-crm-task" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-crm-item page-crm-task ui-header ui-footer ui-fixed-footer" {{if contactId}}data-contactId = "${contactId}"{{/if}}>
  <div class="ui-header">
    <h1 class="ui-title"><%=Resources.MobileResource.AddCrmTask%></h1>
    <a class="ui-btn ui-btn-persons ui-btn-left ui-btn-row{{if $data.back === null}} target-back{{/if}}" href="#{{if $data.back}}${$data.back}{{else}}crm/tasks{{/if}}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-addperson ui-btn-right ui-btn-no-text target-dialog" href="#"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn task-delete">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">  
    <span class="deadline-data">
        <span class="ddata">${item.deadlineDate}</span>
        <span class="dtime">${item.deadlineTime}</span>    
    </span>
    <div class="item ${item.classname}" {{if item.isClosed}}data-isclosed = '1'{{else}}data-isclosed = '0'{{/if}} data-itemid = ${item.id}>
        <div class="ui-update-indicator update-status"></div>
    <input class="input-checkbox item-status{{if item.canedit == false}} disabled{{/if}}" type="checkbox" data-itemid="${item.id}"{{if item.isClosed}} checked="checked"{{/if}} />
        <div class="task-content">
            <span class="inner-text {{if item.isClosed}}closed{{/if}}">${item.title}</span>
              {{if item.description != "" && item.description != null && item.description != "null"}}
                <span class="inner-description {{if item.isClosed}}closed{{/if}}">{{html item.description}}</span>
              {{/if}}
            <div class="item-person"><div class="white"></div><span class="contact-name">${item.responsible.displayName}</span></div>
        </div>
    </div>    
  </div>
</script>