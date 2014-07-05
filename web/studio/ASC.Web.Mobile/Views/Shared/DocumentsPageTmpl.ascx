<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-documents" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-documents ui-header ui-footer ui-fixed-footer">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    {{if parent}}
      <a class="ui-btn ui-btn-documents ui-btn-left ui-btn-row target-self" href="#${parent.href}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    {{else}}
      <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-self" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}}
    {{if $data.rootfoldertype == 'folder-user'}}
      <a class="ui-btn ui-btn-additem ui-btn-adddocument ui-btn-right ui-btn-no-text target-dialog" href="#docs/${id}/items/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}}
  </div>
  <div class="ui-content">
    <div class="ui-scroller">
      {{if items.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrFolderNoFiles%></span>
        </div>
      {{else}}
        <ul class="ui-timeline">
          {{tmpl({items : items}) '#template-files'}}
        </ul>
      {{/if}}
    </div>
  </div>
  <div class="ui-footer">
    <div class="ui-navbar">
      <ul class="ui-grid ui-grid-3 nav-menu main-menu">
        <li class="ui-block filter-item documents{{if $data.rootfoldertype == 'folder-user'}} current-filter{{/if}}">
          <a class="nav-menu-item target-self" href="#docs{{if $data.folderid}}/${$data.folderid}{{/if}}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnMyDocuments%></span>
          </a>
        </li>
        <li class="ui-block filter-item documents-available{{if $data.rootfoldertype == 'folder-shared'}} current-filter{{/if}}">
          <a class="nav-menu-item target-self" href="#docs/c2hhcmVk">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnSharedToMe%></span>
          </a>
        </li>
        <li class="ui-block filter-item documents-shared{{if $data.rootfoldertype == 'folder-common'}} current-filter{{/if}}">
          <a class="nav-menu-item target-self" href="#docs/YXZhaWxhYmxl">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnSharedDocuments%></span>
          </a>
        </li>
      </ul>
    </div>
  </div>
</div>
</script>