<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-documents-editdocument" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-edititem page-documents-edititem page-documents-edititem-document ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-documents ui-btn-left ui-btn-row target-back" href="#docs"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-editdocument-content">
      <input class="document-folderid" type="hidden" value="${folderid}" />
      <div class="ui-menu-item-container">
        <div class="item-container document-item-container item-documenttitle">
          <label><%=Resources.MobileResource.LblTitle%>:</label>
          <input class="document-title" type="text" />
        </div>
        <div class="item-container document-item-container item-content">
          <label><%=Resources.MobileResource.LblDescription%>:</label>
          <textarea class="document-content"></textarea>
        </div>
      </div>
      <div class="item-container document-item-container create-document">
        <button class="create-item create-document"><%=Resources.MobileResource.BtnCreateDocument%></button>
      </div>
    </div>
  </div>
</div>
</script>