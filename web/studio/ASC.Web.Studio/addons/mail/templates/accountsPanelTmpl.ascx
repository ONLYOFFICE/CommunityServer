<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="accountsPanelTmpl" type="text/x-jquery-tmpl">
  <li class="menu-item none-sub-list">
    <a id="${id}"{{if marked}} class="tag tagArrow"{{/if}}><span class="link dotted {{if !enabled}} gray {{/if}}"
        {{if !enabled}} title="<%= MailResource.MessageFromWarning %>"{{/if}}>${email}</span></a>
  </li>
</script>