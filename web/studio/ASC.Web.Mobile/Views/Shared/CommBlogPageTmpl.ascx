<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-community-blog" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-community-item page-community-blog ui-header{{if item.comments}} loaded-comments{{/if}}">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-blogs ui-btn-left ui-btn-row target-back" href="#{{if $data.back}}${$data.back}{{else}}community/blogs{{/if}}"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-addblog ui-btn-right ui-btn-no-text target-self" href="#community/blogs/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-title ui-blog-title">
      <img src="${item.createdBy.avatar}" alt="${item.createdBy.displayName}" />
      <span class="item-title blog-title">
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
    <div class="ui-item-content ui-blog-content">{{html item.text}}</div>
    {{tmpl({item : item, classname : 'add-community-blog-comment', label : '<%=Resources.MobileResource.BtnAddComment%>'}) '#template-addcomment-block'}}
    <ul class="ui-item-comments">
      {{if item.comments}}
        {{tmpl({comments : item.comments}) '#template-comments'}}
      {{/if}}
    </ul>
    <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
    <span class="ui-btn load-comments target-update" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnLoadComments%></span></span></span>
    <a class="ui-btn add-comment target-self" href="#community/blog/${item.id}/comment/add"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnAddComment%></span></span></a>
  </div>
</div>
</script>