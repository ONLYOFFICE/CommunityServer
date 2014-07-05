<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-projmilestones" type="text/x-jquery-tmpl" async="true">
<select class="${classname}{{if $data.disabled === true}} disabled{{/if}}"{{if $data.disabled === true}} disabled="disabled"{{/if}}>
  <option value="-1"><%=Resources.MobileResource.LblSelectMilestone%></option>
  {{each items}}
    <option value="${$value.id}"{{if $value.id == $data.selectedid}} selected="selected"{{/if}}>${$value.title}</option>
  {{/each}}
</select>
</script>