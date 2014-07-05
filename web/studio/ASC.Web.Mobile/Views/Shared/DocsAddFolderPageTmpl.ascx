<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-documents-addfolder" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-documents-additem page-documents-additem-folder ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-documents ui-btn-left ui-btn-row target-self" href="#{{if $data.back}}${back}{{else}}docs/${folderid}{{/if}}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addfolder-content">
      <input class="folder-folderid" type="hidden" value="${folderid}" />
      <div class="item-container document-item-container item-foldertitle">
        <label><%=Resources.MobileResource.LblTitle%>:</label>
        <input class="folder-title" type="text" />
      </div>
      <div class="item-container document-item-container create-folder">
        <button class="create-item create-folder create-documents-folder"><%=Resources.MobileResource.BtnCreateFolder%></button>
      </div>
    </div>
  </div>
</div>
</script>