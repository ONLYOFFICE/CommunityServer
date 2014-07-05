<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-documents-addfile" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-documents-additem page-documents-additem-file ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-documents ui-btn-left ui-btn-row target-self" href="#{{if $data.back}}${back}{{else}}docs/${folderid}{{/if}}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addfile-content">
      <input class="document-folderid" type="hidden" value="${folderid}" />
      <div class="item-container document-item-container item-fileinput">
        <div class="loading-indicator"></div>
        <label><%=Resources.MobileResource.LblFile%>:</label>
        <div class="file-wrapper"><input class="document-file" type="file" /></div>
      </div>
      <div class="item-container document-item-container upload-file">
        <button class="create-item upload-file upload-documents-file"><%=Resources.MobileResource.BtnUploadFile%></button>
      </div>
    </div>
  </div>
</div>
</script>