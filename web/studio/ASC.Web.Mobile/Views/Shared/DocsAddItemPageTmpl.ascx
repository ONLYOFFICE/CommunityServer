<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-documents-additem" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-documents-additem page-documents-additem-item ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-documents ui-btn-left ui-btn-row target-back" href="#{{if $data.back}}${back}{{else}}docs{{/if}}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-adddocument-content">
      <input class="document-folderid" type="hidden" value="${folderid}" />
      <ul class="ui-menu-accordion document-type">
        <li class="ui-menu-item item-add-txt active-item">
          <input class="ui-menu-item-type" type="hidden" value="item-add-txt" />
          <h6 class="ui-menu-item-title"><span><%=Resources.MobileResource.LblTextFile%></span></h6>
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
        </li>
        {{if fileupload === true}}
          <li class="ui-menu-item item-add-file">
            <input class="ui-menu-item-type" type="hidden" value="item-add-file" />
            <h6 class="ui-menu-item-title"><span><%=Resources.MobileResource.LblUploadFile%></span></h6>
            <div class="ui-menu-item-container">
              <div class="item-container document-item-container item-documenttitle">
                <label><%=Resources.MobileResource.LblFile%>:</label>
                <input class="document-file" type="file" />
              </div>
            </div>
          </li>
        {{/if}}
      </ul>
      <div class="item-container document-item-container create-document">
        <button class="create-item create-document"><%=Resources.MobileResource.BtnCreateDocument%></button>
      </div>
    </div>
  </div>
</div>
</script>