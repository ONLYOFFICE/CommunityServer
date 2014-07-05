<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script id="template-page-crm-contacthistory" type="text/x-jquery-tmpl" async="true">
<div class="ui-page page-crm-item page-crm-history ui-header ui-footer ui-fixed-footer">
  <div class="ui-header">
    <h1 class="ui-title">${pagetitle}</h1>
    <a class="ui-btn ui-btn-index ui-btn-left ui-btn-no-text target-dialog" href="#crm/navigate"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
    <a class="ui-btn ui-btn-additem ui-btn-addperson ui-btn-right ui-btn-no-text target-self" href="#crm/contact/${id}/add/historyevent"><span class="ui-btn-inner"><span class="ui-btn-text"><span class="ui-btn-label">&nbsp;</span></span></span></a>
  </div>
  <div class="ui-content">
  <div class="ui-scroller">      
      <div class="ui-item-title ui-company-title">
        <div class="item-smallavatar"><img src = ${contact.smallFotoUrl}></img></div>
        <div class="item-content">
            <span class="item-name">${contact.displayName}</span>            
        </div>      
      </div>
      {{if items.length === 0}}
        <div class="ui-no-content">
          <span class="inner"><%=Resources.MobileResource.ErrCrmNoContactHistory%></span>
        </div>
      {{else}}
        <ul class="ui-timeline">
          {{each items}}
              <li class="item historyevent">
                 <span class="inner-text">${$value.content}</span>
                 <span class="item-date">
                    <span class="date-text">${$value.displayDateCrtdate}</span>
                    <!--<span class="time date-text">${$value.displayTimeCrtdate}</span>-->
                 </span>
                 <span class="item-delete">
                 {{if $value.canEdit}}
                    <button class="delete-item delete-crm-history-event" data-id="${id}"></button>
                 {{/if}}
                 </span>
              </li>
            {{/each}}
        </ul>
        {{if nextIndex}}
            <div class="loading-indicator"><div class="ui-indicator-inner"></div></div>
            <span class="ui-btn load-more-items load-more-crm-items target-update" href="/" data-id="${id}"><span class="ui-btn-inner"><span class="ui-btn-text"><%=Resources.MobileResource.BtnMore%></span></span></span>
        {{/if}}
      {{/if}}
      </div>
    </div>
    <div class="ui-footer">
    <div class="ui-navbar">
      {{if contact.contactclass == "company"}}
      <ul class="ui-grid ui-grid-5 nav-menu main-menu">
        <li class="ui-block filter-item info">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnInfo%></span>
          </a>
        </li>
        <li class="ui-block filter-item history current-filter">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}/history" data-id="${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnHistory%></span>
          </a>
        </li>
        <li class="ui-block filter-item tasks">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}/tasks" data-id="${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnTasks%></span>
          </a>
        </li>        
        <li class="ui-block filter-item persons">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}/persones" data-id="${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnPersons%></span>
          </a>
        </li>        
        <li class="ui-block filter-item files">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}/files" data-id="${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnFiles%></span>
          </a>
        </li>
      </ul>
      {{else}}
           <ul class="ui-grid ui-grid-4 nav-menu main-menu">
        <li class="ui-block filter-item info">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnInfo%></span>
          </a>
        </li>
        <li class="ui-block filter-item history current-filter">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}/history" data-id="${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnHistory%></span>
          </a>
        </li>
        <li class="ui-block filter-item tasks">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}/tasks" data-id="${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnTasks%></span>
          </a>
        </li>            
        <li class="ui-block filter-item files">
          <a class="nav-menu-item target-self" href="#crm/contact/${id}/files" data-id="${id}">
            <span class="item-icon"></span>
            <span class="inner-text"><%=Resources.MobileResource.BtnFiles%></span>
          </a>
        </li>
      </ul> 
      {{/if}}
    </div>
  </div>
  </div>
</script>