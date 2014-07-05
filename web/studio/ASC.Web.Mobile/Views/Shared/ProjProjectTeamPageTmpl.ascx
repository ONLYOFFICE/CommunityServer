<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-project-team" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects page-projects-items page-projects-project-team ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-project ui-btn-left target-self" href="#projects/project/${projid}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-page-title">
      <span class="title-text no-icon"><span><%=Resources.MobileResource.LblProjectTeam%></span></span>
    </div>
    <ul class="ui-timeline ui-people-items">
      {{each items}}
        <li class="item item-persone{{if $value.isManager}} manager{{/if}}{{if $data.isSinglePersone === true}} single-persone{{/if}}" data-itemname="${$value.displayName}">
          <a class="ui-item-link item-persone-data target-self" href="#people/${$value.id}">
            <span class="item-persone-displayname">${$value.displayName}</span>
          </a>
          <div class="sub-info">
            <span class="title full-length">${$value.title}</span>
          </div>
        </li>
      {{/each}}
    </ul>
  </div>
</div>
</script>