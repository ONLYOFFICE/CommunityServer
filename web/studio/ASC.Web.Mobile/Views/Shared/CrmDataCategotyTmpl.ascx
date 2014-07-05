<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-crm-datacategory" type="text/x-jquery-tmpl" async="true">
    {{each categories}}        
            <option value="${id}" {{if $data.commonDataCategoryId == id}}selected{{/if}}>${title}</option>        
    {{/each}}               
</script>