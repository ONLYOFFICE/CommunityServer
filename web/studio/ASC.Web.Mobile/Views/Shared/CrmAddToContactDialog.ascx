<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-dialog-crm-addtocontact" type="text/x-jquery-tmpl" async="true">
<div class="ui-dialog dialog-additem addtocontact">
  <div class="ui-content">
    <div class="ui-item-content ui-adddocument-content">
      <div class="ui-item-content-header">
        <span class="inner-text"><%=Resources.MobileResource.LblCreateDialog%></span>
      </div>
      <div class="ui-listbox">
        <ul class="ui-listbox-list ui-timeline">
          <li class="ui-lisbox-item item historyevent">
            <a class="ui-item-link title target-self" href="#crm/contact/${id}/add/historyevent"><span class="inner-text"><%=Resources.MobileResource.AddCrmHistory%></span></a>
          </li>
          <li class="ui-lisbox-item item task">
            <a class="ui-item-link title target-self" href="#crm/contact/${id}/add/task"><span class="inner-text"><%=Resources.MobileResource.AddCrmTask%></span></a>
          </li>
          <li class="ui-lisbox-item item note">
            <a class="ui-item-link title target-self" href="#crm/contact/${id}/add/file"><span class="inner-text"><%=Resources.MobileResource.AddCrmDoc%></span></a>
          </li>
        </ul>
      </div>
    </div>
  </div>
</div>
</script>