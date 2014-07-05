<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-community" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-community ui-header ui-footer ui-fixed-footer{{if $data.allLoaded === true}} loaded-items{{/if}}">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-self" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-disabled ui-btn-addblog ui-btn-right ui-btn-no-text target-self" href="#community/blogs/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-disabled ui-btn-addforum ui-btn-right ui-btn-no-text target-self" href="#community/forums/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    <%-- <a class="ui-btn ui-btn-additem ui-btn-disabled ui-btn-addevent ui-btn-right ui-btn-no-text target-self" href="#community/events/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a> --%>
    <a class="ui-btn ui-btn-additem ui-btn-disabled ui-btn-addbookmark ui-btn-right ui-btn-no-text target-self" href="#community/bookmarks/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-scroller">
      <%-- <div class="ui-page-title">
        <div class="text-field-wrapper"><label class="search-label" for="txtSearchValue"></label><input class="input-text search-field top-search-field" type="text" value="<%=Resources.MobileResource.LblSearch%>" title="<%=Resources.MobileResource.LblSearch%>" onblur="if(this.value=='')this.value = this.title" onfocus="if(this.value==this.title)this.value=''" /></div>
        <span class="title-text"><img class="product-icon" src="<%=Url.Content("~/content/images/icon-community.svg")%>" alt="${title}" /><span>${title}</span></span>
      </div> --%>
      {{if items.length === 0}}
        <div class="ui-no-content">
          {{if $data.type == 'community-page-blogs'}}
            <span class="inner"><%=Resources.MobileResource.ErrNoBlogs%></span>
          {{else $data.type == 'community-page-forums'}}
            <span class="inner"><%=Resources.MobileResource.ErrNoForums%></span>
          {{else $data.type == 'community-page-bookmarks'}}
            <span class="inner"><%=Resources.MobileResource.ErrNoBookmarks%></span>
          {{else $data.type == 'community-page-events'}}
            <span class="inner"><%=Resources.MobileResource.ErrNoEvents%></span>
          {{else $data.type == 'community-page-polls'}}
            <span class="inner"><%=Resources.MobileResource.ErrNoPolls%></span>
          {{else}}
            <span class="inner"><%=Resources.MobileResource.ErrNoCommItems%></span>
          {{/if}}
        </div>
      {{else}}
        <ul class="ui-timeline">
          {{tmpl '#template-comm-timeline'}}
        </ul>
        <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
        <span class="ui-btn load-more-items load-more-community-items target-update" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnMore%></span></span></span>
      {{/if}}
    </div>
  </div>
  <div class="ui-footer">
    <div class="ui-navbar">
      <ul class="ui-grid ui-grid-${modules.length} nav-menu main-menu">
        {{each modules}}
          <li class="ui-block ${$value.classname}{{if $data.type == $value.type}} current-filter{{/if}}">
            <a class="nav-menu-item target-self" href="#${$value.link}">
              <span class="item-icon"></span>
              <span class="inner-text">${$value.title}</span>
            </a>
          </li>
        {{/each}}
      </ul>
    </div>
  </div>
</div>
</script>