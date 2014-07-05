<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-project-adddiscussion" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-projects-additem page-projects-additem-discussion ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-discussions ui-btn-left ui-btn-row target-back none-shift-back" href="#projects"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-adddiscussion-content">
      <div class="item-container discussion-item-container item-discussiontitle">
        <label><%=Resources.MobileResource.LblTitle%>:</label>
        <input class="discussion-title" type="text" />
      </div>
      <div class="item-container discussion-item-container item-description">
        <label><%=Resources.MobileResource.LblText%>:</label>
        <textarea class="discussion-text"></textarea>
      </div>
      <div class="item-container discussion-item-container item-projectid">
        {{tmpl({items : projects, classname : 'discussion-projectid', selectedid : projid}) '#template-projprojects'}}
      </div>
      <div class="item-container discussion-item-container create-discussion">
        <button class="create-item create-discussion add-projects-discussion"><%=Resources.MobileResource.BtnCreateDiscussion%></button>
      </div>
    </div>
  </div>
</div>
</script>