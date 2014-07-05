<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-community-addblog" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-community-additem page-community-additem-blog ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-blogs ui-btn-left ui-btn-row target-back none-shift-back" href="#community/blogs"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addblog-content">
      <div class="item-container blog-item-container item-blogtitle">
        <label><%=Resources.MobileResource.LblTitle%>:</label>
        <input class="blog-title" type="text" />
      </div>
      <div class="item-container blog-item-container item-blogdescription">
        <label><%=Resources.MobileResource.LblDescription%>:</label>
        <textarea class="blog-description"></textarea>
      </div>
      <div class="item-container blog-item-container item-blogttags">
        <label><%=Resources.MobileResource.LblTags%>:</label>
        <input class="blog-tags" type="text" />
      </div>
      <div class="item-container blog-item-container create-blog">
        <button class="create-item create-blog create-community-blog"><%=Resources.MobileResource.BtnCreatePost%></button>
      </div>
    </div>
  </div>
</div>
</script>