<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-project-discussions" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects page-projects-items page-projects-project-discussions ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-project ui-btn-left target-self" href="#projects/project/${projid}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    {{if security.canCreateMessage === true}}
      <a class="ui-btn ui-btn-additem ui-btn-adddiscussion ui-btn-right ui-btn-no-text target-self" href="#projects/${projid}/discussions/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    {{/if}}
  </div>
  <div class="ui-content">
    <div class="ui-scroller none-iscroll">
      <div class="ui-page-title">
        <span class="title-text no-icon"><span><%=Resources.MobileResource.LblProjectDiscussions%></span></span>
      </div>
      {{if items.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrProjectNoDiscussions%></span>
        </div>
      {{else}}
        <ul class="ui-timeline">
          {{each items}}
            <li class="item disussion-item">
              <a class="ui-item-link title target-self" href="#${$value.href}">
                <span class="inner-text"><span class="text">${$value.title}</span></span>
              </a>
              <div class="sub-info">
                <span class="author">${$value.createdBy.displayName}</span>
                <span class="timestamp"><%=Resources.MobileResource.LblUpdate%>&nbsp;<span class="date">${$value.displayDateUptdate}</span>
                </span>
              </div>
            </li>
          {{/each}}
        </ul>
      {{/if}}
    </div>
  </div>
</div>
</script>