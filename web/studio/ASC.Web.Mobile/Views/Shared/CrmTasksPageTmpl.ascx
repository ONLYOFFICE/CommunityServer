<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-crm-tasks" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-crm-tasks ui-header ui-footer ui-fixed-footer">
  <div class="ui-header">
    <h1 class="ui-title"><%=Resources.MobileResource.TasksListTitle%></h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-dialog" href="#crm/navigate"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-addcustomer ui-btn-right ui-btn-no-text target-self" href="#crm/add/task"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
    <div class="ui-scroller">
      <div class="ui-page-title">
        <div class="text-field-wrapper">
          <form class="search-form" action="/" onsubmit="setTimeout(TeamlabMobile.resetFocus, 0); return false;">
            <label class="search-label"></label>
            <input class="input-text search-field top-search-field" type="search" autocapitalize="off" autocomplete="off" autocorrect="off" />
          </form>
        </div>
      </div>
      {{if items.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrCrmNoContactTasks%></span>
        </div>
      {{else}}
        <ul class="ui-timeline customers-timeline">
          {{tmpl({items: items}) '#template-crm-tasks-timeline'}}
        </ul>
        {{if nextIndex}}        
            <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
            <span class="ui-btn load-more-items load-more-crm-items target-update" href="/" page="${page}" ><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnMore%></span></span></span>
        {{/if}}
      {{/if}}
    </div>
  </div>
  <div class="ui-footer">
    <div class="ui-navbar">
      <ul class="ui-grid ui-grid-4 nav-menu main-menu">
        <li class="ui-block filter-item today {{if page == "today"}}current-filter{{/if}}">
          <a class="nav-menu-item target-self" href="#crm/tasks/today">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnCrmTodayTasks%></span>
          </a>
        </li>
        <li class="ui-block filter-item nextdays {{if page == "nextdays"}}current-filter{{/if}}">
          <a class="nav-menu-item target-self" href="#crm/tasks/nextdays">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnCrmNextTasks%></span>
          </a>
        </li>
        <li class="ui-block filter-item late {{if page == "late"}}current-filter{{/if}}">
          <a class="nav-menu-item target-self" href="#crm/tasks/late">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnCrmLastTasks%></span>
          </a>
        </li>
        <li class="ui-block filter-item closed {{if page == "closed"}}current-filter{{/if}}">
          <a class="nav-menu-item target-self" href="#crm/tasks/closed">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnCrmClosedTasks%></span>
          </a>
        </li>
      </ul>
    </div>
  </div>
</div>
</script>