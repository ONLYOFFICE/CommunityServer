<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-dialog-crm-navigate" type="text/x-jquery-tmpl" async="true">
<div class="ui-dialog dialog-crmnavigate">
  <div class="ui-content">
    <div class="ui-listbox">
        <ul class="ui-listbox-list ui-timeline">
          <li class="ui-lisbox-item item person">
            <a class="ui-item-link title target-self navigate-dialog" href="/"><span class="inner-text"><%=Resources.MobileResource.PageTitle%></span></a>
          </li>
        </ul>
      </div>
    <div class="ui-item-content">
      <div class="ui-item-content-header">
        <span class="inner-text"><%=Resources.MobileResource.LblCrmTitle%></span>
      </div>
      <div class="ui-listbox">
        <ul class="ui-listbox-list ui-timeline">
          <li class="ui-lisbox-item item person">
            <a class="ui-item-link title navigate-dialog target-self" href="#crm"><span class="inner-text"><%=Resources.MobileResource.ContactsListTitle%></span></a>
          </li>
          <li class="ui-lisbox-item item company">
            <a class="ui-item-link title navigate-dialog target-self" href="#crm/tasks/today"><span class="inner-text"><%=Resources.MobileResource.TasksListTitle%></span></a>
          </li>
        </ul>
      </div>
    </div>
  </div>
</div>
</script>