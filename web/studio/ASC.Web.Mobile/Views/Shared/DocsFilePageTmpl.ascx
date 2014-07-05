<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-documents-item" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-documents-item page-documents-file ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-documents ui-btn-left ui-btn-row target-self" href="#${back}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    <%-- {{if item.canedit === true}}
      <a class="ui-btn ui-btn-edititem ui-btn-editdocument ui-btn-right ui-btn-no-text target-self" href="#docs/${item.id}/files/edit"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}} --%>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-file-content">
      {{if filetype === 'image'}}
        <img class="file-container" src="${item.viewUrl}" alt="${item.filename}" />
      {{else filetype === 'txt'}}
        <iframe class="file-container" src="${item.href}"></iframe>
      {{else filetype === 'unknown'}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrUnknownExtension%></span>
        </div>
      {{else}}
        {{if item.isSupportedFile === true}}
          <iframe class="file-container" src="${item.href}"></iframe>
        {{else}}
          <div class="ui-no-content">
            <span class="inner"><%=Resources.MobileResource.ErrUnknownExtension%></span>
          </div>
        {{/if}}
      {{/if}}
    </div>
  </div>
</div>
</script>