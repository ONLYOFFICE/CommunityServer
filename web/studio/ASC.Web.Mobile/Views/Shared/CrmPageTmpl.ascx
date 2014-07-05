<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-crm" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-crm ui-header">
  <div class="ui-header">
    <h1 class="ui-title"><%=Resources.MobileResource.ContactsListTitle%></h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-dialog" href="#crm/navigate"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-addcustomer ui-btn-right ui-btn-no-text target-dialog" href="#crm/customers/add"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-scroller">
      <div class="ui-page-title">
        <div class="text-field-wrapper">
          <form class="search-form{{if $data.filtervalue}} active{{/if}}" action="/" onsubmit="setTimeout(TeamlabMobile.resetFocus, 0); return false;">
            <label class="search-label"></label>
            <label class="disable-search-label reset-search-crm-contacts"></label>
            <input class="input-text search-field top-search-field" type="search" autocapitalize="off" autocomplete="off" autocorrect="off"{{if $data.filtervalue}} value="${filtervalue}"{{/if}} />
          </form>
        </div>
      </div>
      {{if items.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrNoCustomers%></span>
        </div>
      {{else}}
        <ul class="ui-timeline customers-timeline">
          {{tmpl '#template-crm-timeline'}}
        </ul>
        {{if nextIndex}}
            <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
            <span class="ui-btn load-more-items load-more-crm-items target-update" href="/"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnMore%></span></span></span>
        {{/if}}
      {{/if}}
    </div>
  </div>
  
</div>
</script>