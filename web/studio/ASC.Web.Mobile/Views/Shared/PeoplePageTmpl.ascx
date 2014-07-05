<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-people" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-people ui-header">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-self" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-page-title">
      <div class="text-field-wrapper">
        <form class="search-form" action="/" onsubmit="setTimeout(TeamlabMobile.resetFocus, 0);return false;">
          <label class="search-label"></label>
          <label class="disable-search-label"></label>
          <input class="input-text search-field top-search-field" type="search" autocapitalize="off" autocomplete="off" autocorrect="off" />
        </form>
      </div>
      <%-- <span class="title-text"><img class="product-icon" src="<%=Url.Content("~/content/images/icon-people.svg")%>" alt="${title}" /><span>${title}</span></span> --%>
    </div>
    <ul class="ui-people-indexes">
      {{each indexes}}
        <li class="item-index" data-index="${$value.index}">
          <div class="ui-index-head">
            <span class="inner-text">${$value.index}</span>
          </div>
          <ul class="ui-people-items">
            {{each $value.items}}
              <li class="item-persone" data-itemname="${$value.displayName}">
                <%-- <img class="item-persone-avatar" src="${$value.avatar}" alt="${$value.displayName}" /> --%>
                <a class="ui-item-link item-persone-data target-self" href="#people/${$value.id}">
                  <span class="item-persone-displayName">${$value.displayName}</span>
                  <%-- <span class="item-persone-title">${$value.title}</span> --%>
                  <%-- {{if $value.groups.length > 0}}
                    <span class="item-persone-groups">
                      {{each groups}}
                        {{if $index > 0}}<span class="group-separator">,</span>{{/if}}
                        ${$value.name}
                      {{/each}}
                    </span>
                  {{/if}} --%>
                </a>
                <div class="item-persone-contacts">
                  {{if $value.tel != ''}}
                    <a class="item-persone-contact item-persone-tel target-top change-page-none" href="tel:${$value.tel}">&nbsp;</a>
                  {{/if}}
                  {{if $value.email != ''}}
                    <a class="item-persone-contact item-persone-email target-top change-page-none" href="mailto:${$value.email}">&nbsp;</a>
                  {{/if}}
                </div>
              </li>
            {{/each}}
          </ul>
        </li>
      {{/each}}
    </ul>
    <%-- <div class="ui-navbar ui-navbar-people">
      {{each filters}}
        <span class="people-filter-item" href="#people/${$value}" data-index="${$value}">${$value}</span>
      {{/each}}
    </div> --%>
  </div>
</div>
</script>