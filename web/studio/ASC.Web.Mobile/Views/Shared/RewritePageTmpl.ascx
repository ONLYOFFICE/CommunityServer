<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-rewrite" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-rewrite ui-header">
  <div class="ui-header">
    <h1 class="ui-title">&nbsp;</h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-self" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-no-content">
      <span class="inner"><%=Resources.MobileResource.ErrNoRewrite%></span>
    </div>
    {{if url}}
      <div class="item-container">
        <button class="move-me" data-href="${url}"><%=Resources.MobileResource.LblMove%></button>
      </div>
    {{/if}}
  </div>
</div>
</script>