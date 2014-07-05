<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-project-addmilestone" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-additem page-projects-additem page-projects-additem-milestone ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-row ui-btn-milestones ui-btn-left ui-btn-row target-back none-shift-back" href="#projects"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.BtnBack%></span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-item-content ui-addmilestone-content">
      <div class="item-container milestone-item-container item-milestonetitle">
        <label><%=Resources.MobileResource.LblTitle%>:</label>
        <input class="milestone-title" type="text" />
      </div>
      <%-- <div class="item-container milestone-item-container item-description">
        <label><%=Resources.MobileResource.LblDescription%>:</label>
        <textarea class="milestone-description"></textarea>
      </div> --%>
      <div class="item-container milestone-item-container item-deadline">
        <input class="milestone-deadline" type="datepick" readonly="readonly" value="<%=Resources.MobileResource.LblDeadline%>" />
      </div>
      <div class="item-container milestone-item-container item-projectid">
        {{tmpl({items : projects, classname : 'milestone-projectid', selectedid : projid}) '#template-projprojects'}}
      </div>
      <div class="item-container milestone-item-container create-milestone">
        <button class="create-item create-milestone add-projects-milestone"><%=Resources.MobileResource.BtnCreateMilestone%></button>
      </div>
    </div>
  </div>
</div>
</script>