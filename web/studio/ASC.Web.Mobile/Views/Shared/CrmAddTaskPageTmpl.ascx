<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-crm-addtask" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-crm-addtask ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-tasks ui-btn-left ui-btn-row target-back" href="#crm/tasks"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addtask-content">
      <div class="item-container task-item-container item-tasktitle">
        <label><%=Resources.MobileResource.LblTitle%>:</label>
        <input class="task-title" type="text" maxlength = 100/>
      </div>
      <div class="item-container task-item-container item-taskcatype">
        <label><%=Resources.MobileResource.LblTaskCategory%>:</label>
        <select class="task-type" type="text">
            <option value="-1"><%=Resources.MobileResource.LblSelectCategory%></option>
            {{each categories}}
                <option value="${id}" >${title}</option>
            {{/each}}                   
        </select>
      </div>      
      <div class="item-container task-item-container item-taskduedate">
        <label><%=Resources.MobileResource.LblDatapicCrmDueData%>:</label>
        <input class="task-duedate" type="datepick" />
      </div>
      <div class="item-container task-item-container item-taskrespons">
        <label>Assign to group:</label>
        <select class="group-responsible" type="text">
            <option value="-1">Select responsible group</option>
            <option value="0">Out of groups</option>
            {{each groups}}
                <option value="${id}" >${name}</option>
            {{/each}}         
        </select>        
      </div>
      <div class="item-container task-item-container item-description">
        <label><%=Resources.MobileResource.LblDescription%>:</label>
        <textarea class="task-description"></textarea>
      </div>
      
      <div class="item-container task-item-container add-crm-task">
        {{if ($data.id)}}
            <button class="create-item add-crm-task" data-id="${id}"><%=Resources.MobileResource.BtnCreateTask%></button>
        {{else}}
            <button class="create-item add-crm-task"><%=Resources.MobileResource.BtnCreateTask%></button>
        {{/if}}        
        <button class="create-item cansel-crm-task"><%=Resources.MobileResource.BtnCancel%></button>
      </div>
    </div>
  </div>
</div>
</script>