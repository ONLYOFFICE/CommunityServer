<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-commthreads" type="text/x-jquery-tmpl" async="true">
<select class="forum-threadid">
  <option value="-1"><%=Resources.MobileResource.LblSelectThread%></option>
  {{each items}}
    <option value="${$value.id}">${$value.title}</option>
  {{/each}}
</select>
</script>