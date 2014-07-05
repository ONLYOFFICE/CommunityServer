<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-community-forum" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-community-item page-community-forum ui-header loaded-comments">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-forum ui-btn-left ui-btn-row" href="#{{if $data.back}}${$data.back}{{else}}community/forums{{/if}}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-addforum ui-btn-right ui-btn-no-text target-self" href="#community/forums/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-title ui-forum-title">
      <img src="${item.createdBy.avatar}" alt="${item.createdBy.displayName}" />
      <span class="item-title forum-title">
        <span class="inner-text">${item.title}</span>
      </span>
      <div class="sub-info">
        <span class="timestamp">
          <span class="date">${item.displayDatetimeCrtdate}</span>
        </span>
        {{if item.createdBy}}
          <a class="author" href="#people/${item.createdBy.id}">${item.createdBy.displayName}</a>
        {{/if}}
      </div>
    </div>
    <div class="ui-item-content ui-forum-content">{{html item.text}}</div>
    {{tmpl({item : item, classname : 'add-community-forum-post', label : '<%=Resources.MobileResource.BtnAddPost%>'}) '#template-addcomment-block'}}
    <ul class="ui-item-comments">{{tmpl({comments : item.posts}) '#template-comments'}}</ul>
    <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
    <span class="ui-btn load-comments target-update" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnLoadComments%></span></span></span>
    <a class="ui-btn add-comment target-self" href="#community/forum/${item.id}/comment/add"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnAddPost%></span></span></a>
  </div>
</div>
</script>