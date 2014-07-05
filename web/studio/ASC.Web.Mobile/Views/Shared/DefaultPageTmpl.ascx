<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-default" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-default ui-header ui-footer">
  <div class="ui-header">
    <h1 class="ui-title"><%=Resources.MobileResource.PageTitle%></h1>
    {{if isapp}}
      <a class="ui-btn ui-btn-logout ui-btn-right target-portals" href="#"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.LblPortals%></span></span></span></a>
    {{else}}
      <a class="ui-btn ui-btn-right target-logout" href="<%=Url.RouteUrl("Default",new { controller = "Account", action = "SignOut" }) %>"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label"><%=Resources.MobileResource.LblLogout%></span></span></span></a>
    {{/if}}
  </div>
  <div class="ui-content">
    <div class="ui-page-title">
      <div class="text-field-wrapper">
        <form class="search-form" action="/" onsubmit="setTimeout(TeamlabMobile.resetFocus, 0); return false;">
          <label class="search-label"></label>
          <label class="disable-search-label"></label>
          <input class="input-text search-field top-search-field" type="search" autocapitalize="off" autocomplete="off" autocorrect="off" />
        </form>
      </div>
    </div>
    <ul class="default-menu">
      {{each products}}
        <li class="item ${$value.classname}{{if $value.unavailable}} coming-soon{{/if}}">
          <img class="product-icon" src="${$value.icon}" alt="${$value.title}" />
          <a class="item-layout target-self" href="#${$value.link}"></a>
          <span class="item-layout"></span>
          <span>${$value.title}</span>
        </li>
        {{if $index % 3 == 2}}<li class="separator{{if $index < $data.products.length - 1}} inner{{/if}}"></li>{{/if}}
      {{/each}}
      {{if products.length % 2}}<li class="separator"></li>{{/if}}
    </ul>
  </div>
  <div class="ui-footer">
    <div class="bottom-menu">
      {{if isapp}}
        <span class="copyrights"><%=Resources.MobileResource.LblCopyrights%></span>
      {{else}}
        <a class="copyrights target-blank" href="http://teamlab.com"><%=Resources.MobileResource.LblCopyrights%></a>
        <span class="sep">|</span>
        <a class="standart-vertion target-standart" href="/"><%=Resources.MobileResource.LblStandartVersion%></a>
      {{/if}}
    </div>
  </div>
</div>
</script>