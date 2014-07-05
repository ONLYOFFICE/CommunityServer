<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-projsearch-items" type="text/x-jquery-tmpl" async="true">
<ul class="ui-timeline timeline-projsearch-results">
  {{each items}}
    <li class="item ${$value.classname}">
      <div class="item-state">
        <div class="item-icon"></div>
      </div>
      <a class="ui-item-link title{{if $value.href}} target-self{{else}} target-none{{/if}}" href="${$value.href}" data-back="${anchor}">
        <span class="inner-text">${$value.title}</span>
      </a>
      {{if $value.type === 'task'}}
        <div class="sub-info">
          {{if $value.entryTitle}}
            <span class="project-title">${$value.entryTitle}</span>
          {{/if}}
        </div>
      {{else $value.type === 'milestone'}}
        <div class="sub-info">
          {{if $value.entryTitle}}
            <span class="project-title">${$value.entryTitle}</span>
          {{/if}}
        </div>
      {{else $value.type === 'discussion'}}
        <div class="sub-info">
          {{if $value.entryTitle}}
            <span class="project-title">${$value.entryTitle}</span>
          {{/if}}
        </div>
      {{/if}}
    </li>
  {{/each}}
</ul>
</script>