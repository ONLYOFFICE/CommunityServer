<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-crm-persones-timeline" type="text/x-jquery-tmpl" async="true">
{{each items}}
  <li class="item ${$value.contactclass}">
    <div class="item-state white"><div class="item-icon"></div></div>
    <a class="ui-item-link title target-self" href="#${$value.href}">
          <span class="inner-text">${$value.displayName}</span>
          <div class="item-company"><div class="white"><div class="company-icon"></div></div><span class="company-name">${$value.company.displayName}</span></div>
    </a>
  </li>
{{/each}}
</script>