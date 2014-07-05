<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-projteam" type="text/x-jquery-tmpl" async="true">
<select class="${classname}{{if $data.disabled === true}} disabled{{/if}}"{{if $data.disabled === true}} disabled="disabled"{{/if}}>
  <option value="00000000-0000-0000-0000-000000000000"><%=Resources.MobileResource.LblSelectResponsible%></option>
  {{each items}}
    <option value="${$value.id}"{{if $value.id == $data.selectedid}} selected="selected"{{/if}}>${$value.displayName}</option>
  {{/each}}
</select>
</script>