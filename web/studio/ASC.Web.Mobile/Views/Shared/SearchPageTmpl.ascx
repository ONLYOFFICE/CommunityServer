<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-search" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-search ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-self" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-page-title">
      <div class="text-field-wrapper">
        <form class="search-form" action="/" onsubmit="setTimeout(TeamlabMobile.resetFocus, 0); return false;">
          <label class="search-label"></label>
          <label class="disable-search-label"></label>
          <input class="input-text search-field top-search-field" type="search" autocapitalize="off" autocomplete="off" autocorrect="off" value="${query}" />
        </form>
      </div>
    </div>
    {{if items.length === 0}}
      <div class="ui-no-content">
        <span class="inner"><%=Resources.MobileResource.ErrSearchNoResults%></span>
      </div>
    {{else}}
      <ul class="ui-timeline">
        {{each items}}
          <li class="product-item ${$value.classname}">
            <div class="item-title product-item-title">
              <span class="text">${$value.title}</span>
            </div>
            <ul class="ui-timeline timeline-search-results">
              {{each $value.items}}
                <li class="item ${$value.classname}">
                  {{if $value.type === 'person'}}
                    <div class="item-state">
                      <img class="product-icon item-persone-avatar" src="${$value.avatar}" alt="${$value.displayName}" />
                    </div>
                    <a class="ui-item-link title item-persone-data{{if $value.href}} target-self{{else}} target-none{{/if}}" href="#${$value.href}" data-back="${anchor}">
                      <span class="inner-text item-persone-displayname">${$value.displayName}</span>
                    </a>
                  {{else}}
                    <div class="item-state">
                      <div class="item-icon"></div>
                    </div>
                    <a class="ui-item-link title{{if $value.href}} target-self{{else}} target-none{{/if}}" href="#${$value.href}" data-back="${anchor}">
                      <span class="inner-text">${$value.title}</span>
                    </a>
                    {{if $value.additioninfo}}
                      <div class="addition-info">
                        <span class="inner-text">${$value.additioninfo}</span>
                      </div>
                    {{/if}}
                    {{if $value.producttype === 'commitems'}}
                      <div class="sub-info">
                        <span class="timestamp">
                          <span class="date">${$value.displayCrtdate}</span>
                        </span>
                        {{if $value.createdBy}}
                          <span class="author">${$value.createdBy.displayName}</span>
                        {{/if}}
                      </div>
                    {{/if}}
                  {{/if}}
                </li>
                {{if $value.type === 'project' && $value.items && $value.items.length > 0}}
                  <li class="item-projsearch-results">
                    {{tmpl({items : $value.items, anchor : anchor}) '#template-projsearch-items'}}
                  </li>
                {{/if}}
              {{/each}}
            </ul>
          </li>
        {{/each}}
      </ul>
    {{/if}}
  </div>
</div>
</script>