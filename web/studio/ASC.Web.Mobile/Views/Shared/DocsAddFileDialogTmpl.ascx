<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-dialog-documents-additem" type="text/x-jquery-tmpl" async="true">
<div class="ui-dialog dialog-additem dialog-documents-additem dialog-documents-additem-document">
  <div class="ui-content">
    <div class="ui-item-content ui-adddocument-content">
      <div class="ui-item-content-header">
        <span class="inner-text"><%=Resources.MobileResource.LblCreateDialog%></span>
      </div>
      <div class="ui-listbox">
        <ul class="ui-listbox-list ui-timeline">
          <li class="ui-lisbox-item item folder">
            <div class="item-state"><div class="item-icon"></div></div>
            <a class="ui-item-link title target-self" href="#docs/${folderid}/folders/add"><span class="inner-text"><%=Resources.MobileResource.LblCreateFolder%></span></a>
          </li>
          {{if fileupload === true}}
            <li class="ui-lisbox-item item file">
              <div class="item-state"><div class="item-icon"></div></div>
              <a class="ui-item-link title target-self" href="#docs/${folderid}/files/add"><span class="inner-text"><%=Resources.MobileResource.BtnUploadFile%></span></a>
            </li>
          {{/if}}
          <li class="ui-lisbox-item item document">
            <div class="item-state"><div class="item-icon"></div></div>
            <a class="ui-item-link title target-self" href="#docs/${folderid}/documents/add"><span class="inner-text"><%=Resources.MobileResource.LblCreateDocument%></span></a>
          </li>
        </ul>
      </div>
    </div>
  </div>
</div>
</script>
