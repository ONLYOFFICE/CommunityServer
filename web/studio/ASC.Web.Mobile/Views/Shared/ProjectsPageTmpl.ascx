<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-projects" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-projects ui-header ui-footer ui-fixed-footer">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-self" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-scroller">
      <%-- <div class="ui-page-title">
        <div class="text-field-wrapper"><label class="search-label" for="txtSearchValue"></label><input class="input-text search-field top-search-field" type="text" value="<%=Resources.MobileResource.LblSearch%>" title="<%=Resources.MobileResource.LblSearch%>" onblur="if(this.value=='')this.value = this.title" onfocus="if(this.value==this.title)this.value=''" /></div>
        <span class="title-text"><img class="product-icon" src="<%=Url.Content("~/content/images/icon-projects.svg")%>" alt="${title}" /><span>${title}</span></span>
      </div> --%>
      {{if mineitems.length === 0 && otheritems.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrNoProjects%></span>
        </div>
      {{else}}
        <ul class="ui-timeline{{if onetype}} one-type{{/if}}">
          {{each mineitems}}
            <li class="item ${$value.classname} is-self" data-itemid="${$value.id}">
              <a class="ui-item-link title target-self" href="#${$value.href}">
                <span class="inner-text">${$value.title}</span>
              </a>
              <div class="sub-info">
                <span class="author">${$value.responsible.displayName}</span>
              </div>
            </li>
          {{/each}}
          <li class="item-separator"></li>
          {{each otheritems}}
            <li class="item ${$value.classname} is-other" data-itemid="${$value.id}">
              <a class="ui-item-link title target-self" href="#${$value.href}">
                <span class="inner-text">${$value.title}</span>
              </a>
              <div class="sub-info">
                <span class="author">${$value.responsible.displayName}</span>
              </div>
            </li>
          {{/each}}
        </ul>
      {{/if}}
    </div>
  </div>
  <div class="ui-footer">
    <div class="ui-navbar">
      <ul class="ui-grid ui-grid-2 nav-menu main-menu">
        <li class="ui-block filter-item projects{{if $data.type == 'projects-page'}} current-filter{{/if}}">
          <%-- <img class="item-icon" src="<%=Url.Content("~/content/images/icon-filter-all.svg")%>" alt="<%=Resources.MobileResource.BtnProjects%>" /> --%>
          <a class="nav-menu-item target-self" href="#projects">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnProjects%></span>
          </a>
        </li>
        <li class="ui-block filter-item tasks{{if $data.type == 'projects-page-tasks'}} current-filter{{/if}}">
          <%-- <img class="item-icon" src="<%=Url.Content("~/content/images/icon-filter-tasks.svg")%>" alt="<%=Resources.MobileResource.BtnMyTasks%>" /> --%>
          <a class="nav-menu-item target-self" href="#projects/tasks">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnMyTasks%></span>
          </a>
        </li>
      </ul>
    </div>
  </div>
</div>
</script>