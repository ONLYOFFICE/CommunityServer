<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects-discussion" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-projects-item page-projects-discussion ui-header loaded-comments{{if item.comments}} loaded-comments{{/if}}">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-discussion ui-btn-left ui-btn-row target-self" href="#projects/project/${item.projectId}/discussions"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-title ui-discussion-title">
      <img src="${item.createdBy.avatar}" alt="${item.createdBy.displayName}" />
      <span class="item-title discussion-title">
        <span class="inner-text">${item.title}</span>
      </span>
      <div class="sub-info">
        <span class="timestamp">
          <span class="date">${item.displayDateCrtdate}</span>
        </span>
        <a class="author" href="#people/${item.createdBy.id}">${item.createdBy.displayName}</a>
      </div>
    </div>
    <div class="ui-item-content ui-discussion-content">{{html item.text}}</div>
    {{tmpl({item : item, classname : 'add-projects-discussion-comment', label : '<%=Resources.MobileResource.BtnAddComment%>'}) '#template-addcomment-block'}}
    <ul class="ui-item-comments">
      {{if item.comments}}
        {{tmpl({comments : item.comments}) '#template-comments'}}
      {{/if}}
    </ul>
    <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
    <span class="ui-btn load-comments target-update" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnLoadComments%></span></span></span>
  </div>
</div>
</script>