<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-dialog-crm-additem" type="text/x-jquery-tmpl" async="true">
<div class="ui-dialog dialog-additem ui-addcrmcontact dialog-documents-additem dialog-documents-additem-document">
  <div class="ui-content">
    <div class="ui-item-content ui-adddocument-content">
      <div class="ui-item-content-header">
        <span class="inner-text"><%=Resources.MobileResource.LblCreateDialog%></span>
      </div>
      <div class="ui-listbox">
        <ul class="ui-listbox-list ui-timeline">
          <li class="ui-lisbox-item item person">            
            <a class="ui-item-link title target-self" href="#crm/persone/add"><span class="inner-text"><%=Resources.MobileResource.LblCreatePerson%></span></a>
          </li>
          <li class="ui-lisbox-item item company">            
            <a class="ui-item-link title target-self" href="#crm/company/add"><span class="inner-text"><%=Resources.MobileResource.LblCreateCompany%></span></a>
          </li>
        </ul>
      </div>
    </div>
  </div>
</div>
</script>
