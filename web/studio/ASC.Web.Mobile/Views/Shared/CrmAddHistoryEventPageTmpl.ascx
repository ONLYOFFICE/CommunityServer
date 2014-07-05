<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-crm-addhistoryevent" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-crm-additem page-crm-additem-event ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-events ui-btn-left ui-btn-row target-back" href="#crm"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addhistoryevent-content">
      <div class="item-container historyevent-item-container item-eventtype">
        <label><%=Resources.MobileResource.LblSelectEventType%>:</label>
        <select class="historyevent-type" type="text">
        <option value="-1"><%=Resources.MobileResource.LblSelectCategory%></option>
          {{each category}}
            <option value="${id}">${title}</option>
          {{/each}}
        </select>
      </div>
      <div class="item-container historyevent-item-container item-historyeventdate">
        <label><%=Resources.MobileResource.LblDatapicCrmData%>:</label>
        <input class="historyevent-date" type="datepick" />
      </div>
      <div class="item-container crm-item-container item-historyeventdescription">
        <label><%=Resources.MobileResource.LblDescription%>:</label>
        <textarea class="historyevent-description"></textarea>
      </div>
      <div class="item-container crm-item-container add-historyevent">
        <button class="create-item add-crm-history-event" data-id="${id}"><%=Resources.MobileResource.BtnCrmAddHistoryEvent%></button>
      </div>
    </div>
  </div>
</div>
</script>