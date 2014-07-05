<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-crm-addnote" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-crm-additem page-crm-additem-event ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-events ui-btn-left ui-btn-row target-self" href="#crm/contact/${item.id}/files"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addnote-content">
      <input class="note-contactid" type="hidden" value="${item.id}" />
      <div class="item-container note-item-container item-notetitle">
        <label><%=Resources.MobileResource.LblTitle%></label>
        <input class="note-title" type="text" maxlength = 100/>
      </div>
      <div class="item-container crm-item-container item-notedescription">
        <label><%=Resources.MobileResource.LblDescription%>:</label>
        <textarea class="note-description"></textarea>
      </div>
      <div class="item-container crm-item-container create-note">
        <button class="create-item crm-add-note-to-contact"><%=Resources.MobileResource.BtnCrmAddDoc%></button>
      </div>
    </div>
  </div>
</div>
</script>