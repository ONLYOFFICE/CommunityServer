<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-addcomment" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-item-addcomment ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-left ui-btn-row target-back" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-title ui-blog-title">
      {{if item.authors}}
        <img src="${item.authors[0].avatar}" alt="${item.authors[0].displayname}" />
      {{/if}}
      {{if item.responsibles}}
        <img src="${item.responsibles[0].avatar}" alt="${item.responsibles[0].displayname}" />
      {{/if}}
      <span class="item-title blog-title">
        <span class="inner-text">${item.title}</span>
      </span>
      <div class="sub-info">
        <span class="timestamp">
          <span class="date">${item.displaycrtdate}</span>
        </span>
        {{if item.authors && item.authors.length > 0}}
          <span class="author{{if item.authors.length > 1}} some{{/if}}">${item.authors[0].displayname}</span>
        {{/if}}
        {{if item.responsibles && item.responsibles.length > 0}}
          <span class="author{{if item.responsibles.length > 1}} some{{/if}}">${item.responsibles[0].displayname}</span>
        {{/if}}
      </div>
    </div>
    <div class="ui-item-content">
      <div class="addcomment-item-container item-textfield">
        <input class="comment-type" type="hidden" value="${item.type}" />
        <input class="comment-id" type="hidden" value="${item.id}" />
        <input class="comment-parentid" type="hidden" value="${parentid}" />
        <textarea class="ui-text-area comment-content"></textarea>
      </div>
      <div class="addcomment-item-container create-comment">
        <button class="create-item create-comment"><%=Resources.MobileResource.BtnAddComment%></button>
      </div>
    </div>
  </div>
</div>
</script>