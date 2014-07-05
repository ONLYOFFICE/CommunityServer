<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-comm-timeline" type="text/x-jquery-tmpl" async="true">
{{each items}}
  <li class="item ${$value.classname}">
    <div class="item-state"><div class="product-icon"></div></div>
    <a class="ui-item-link title target-self" href="#${$value.href}">
      <span class="inner-text">${$value.title}</span>
    </a>
    {{if $value.type === 'forum'}}
      <div class="addition-info">
        <span class="inner-text">${$value.threadTitle}</span>
      </div>
    {{/if}}
    <div class="sub-info">
      <span class="timestamp">
        <span class="date">${$value.displayDateUptdate || $value.displayDateCrtdate}</span>
      </span>
      {{if $value.createdBy}}
        <span class="author">${$value.createdBy.displayName}</span>
      {{else $value.updatedBy}}
        <span class="author">${$value.updatedBy.displayName}</span>
      {{/if}}
    </div>
  </li>
{{/each}}
</script>