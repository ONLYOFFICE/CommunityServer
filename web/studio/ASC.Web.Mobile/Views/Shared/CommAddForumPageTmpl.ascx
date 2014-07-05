<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-community-addforum" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-community-additem page-community-additem-forum ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-forums ui-btn-left ui-btn-row target-back none-shift-back" href="#community/forums"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addforum-content">
      <div class="item-container forum-item-container item-forumtitle">
        <label><%=Resources.MobileResource.LblTitle%>:</label>
        <input class="forum-title" type="text" />
      </div>
      <div class="item-container forum-item-container item-description">
        <label><%=Resources.MobileResource.LblDescription%>:</label>
        <textarea class="forum-description"></textarea>
      </div>
      <div class="item-container forum-item-container item-threadid">
        <div class="loading-indicator">&nbsp;</div>
        <select class="forum-threadid disabled" disabled="disabled">
          <option value="-1"><%=Resources.MobileResource.LblSelectThread%></option>
        </select>
      </div>
      <div class="item-container blog-item-container create-forum">
        <button class="create-item create-forum create-community-forum"><%=Resources.MobileResource.BtnCreateTopic%></button>
      </div>
    </div>
  </div>
</div>
</script>