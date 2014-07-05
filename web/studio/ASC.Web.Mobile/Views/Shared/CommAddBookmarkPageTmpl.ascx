<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-community-addbookmark" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-community-additem page-community-additem-bookmark ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-bookmarks ui-btn-left ui-btn-row target-back none-shift-back" href="#community/bookmarks"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addbookmark-content">
      <div class="item-container bookmark-item-container item-bookmarkurl">
        <label><%=Resources.MobileResource.LblUrl%>:</label>
        <input class="bookmark-url" type="text" />
      </div>
      <div class="item-container bookmark-item-container item-bookmarktitle">
        <label><%=Resources.MobileResource.LblTitle%>:</label>
        <input class="bookmark-title" type="text" />
      </div>
      <div class="item-container bookmark-item-container item-bookmarkdescription">
        <label><%=Resources.MobileResource.LblDescription%>:</label>
        <textarea class="bookmark-description"></textarea>
      </div>
      <div class="item-container bookmark-item-container item-bookmarktags">
        <label><%=Resources.MobileResource.LblTags%>:</label>
        <input class="bookmark-tags" type="text" />
      </div>
      <div class="item-container bookmark-item-container create-bookmark">
        <button class="create-item create-bookmark create-community-bookmark"><%=Resources.MobileResource.BtnCreateBookmark%></button>
      </div>
    </div>
  </div>
</div>
</script>