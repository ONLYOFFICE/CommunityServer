<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-project-files" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects page-projects-items page-projects-project-files ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    {{if parent}}
      <a class="ui-btn ui-btn-project-documents ui-btn-left ui-btn-row target-self" href="#${parent.href}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    {{else}}
      <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-self" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}}
  </div>
  <div class="ui-content">
    <div class="ui-page-title">
      <span class="title-text no-icon"><span><%=Resources.MobileResource.LblProjectFiles%></span></span>
    </div>
    {{if items.length === 0}}
      <div class="ui-no-content">
        <span class="inner"><%=Resources.MobileResource.ErrFolderNoFiles%></span>
      </div>
    {{else}}
      <ul class="ui-timeline ui-files-items">
        {{tmpl({items : items}) '#template-files'}}
      </ul>
    {{/if}}
  </div>
</div>
</script>