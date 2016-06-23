<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<script id="projects_statusChangePanel" type="text/x-jquery-tmpl">
    <div id="${listId}" class="studio-action-panel">
        <ul id="statusList" class="dropdown-content">
            {{each statuses}}
            <li class="${cssClass} dropdown-item">${text}</li>
            {{/each}}
        </ul>
    </div>
</script>

<script id="projects_panelFrame" type="text/x-jquery-tmpl">
    <div id="${panelId}" class="studio-action-panel" objid="">
        <div class="panel-content">
        </div>
    </div>
</script>

<script id="projects_actionMenuContent" type="text/x-jquery-tmpl">
    <ul class="dropdown-content">
        {{each menuItems}}
            <li id="${id}" class="dropdown-item"><span title="${text}">${text}</span></li>
        {{/each}}
    </ul>
</script>

<script id="projects_linedListWithCheckbox" type="text/x-jquery-tmpl">
    <ul class="dropdown-content lined">
        {{each menuItems}}
            <li class="dropdown-item {{if classname}}${classname}{{/if}}"><input id="${id}" type="checkbox" /><label for="${id}" title="${title}">${title}</label></li>
        {{/each}}
    </ul>
</script>

<script id="projects_descriptionPanelContent" type="text/x-jquery-tmpl">
    {{if creationDate}} 
    <div class="date">
        <div class="param"><%= TaskResource.CreatingDate%>:</div>
        <div class="value">${creationDate}</div>
    </div>
    {{/if}}
    {{if createdBy}}
    <div class="createdby">
        <div class="param"><%= TaskResource.CreatedBy%>:</div>
        <div class="value">${createdBy}</div>
    </div>
    {{/if}}
    {{if startDate}}
    <div class="startdate">
        <div class="param"><%= TaskResource.TaskStartDate%>:</div>
        <div class="value">${startDate}</div>
    </div>
    {{/if}}
    {{if closedDate}}
    <div class="closed">
        <div class="param"><%= TaskResource.Closed%>:</div>
        <div class="value">${closedDate}</div>
    </div>
    {{/if}}
    {{if closedBy}}
    <div class="closedby">
        <div class="param"><%= TaskResource.ClosedBy%>:</div>
        <div class="value">${closedBy}</div>
    </div>
    {{/if}}
    {{if project}}
    <div class="project">
        <div class="param"><%= TaskResource.Project%>:</div>
        <div class="value" projectid="${projectId}">{{html project}}</div>
    </div>
    {{/if}}
    {{if milestone}}
    <div class="milestone">
        <div class="param"><%= TaskResource.Milestone%>:</div>
        <div class="value" projectid="${projectId}" milestone="${milestoneId}">{{html milestone}}</div>
    </div>
    {{/if}}
    {{if description}}
    <div class="descr">
        <div class="param"><%= TaskResource.Description%>:</div>
        <div class="value">
            <div class="descrValue">{{html jq.linksParser(jq.htmlEncodeLight(description))}}</div>
            {{if readMore}}
            <a class="readMore" target="_blank" href="${readMore}"><%=ProjectsCommonResource.ReadMore %></a>
            {{/if}}
        </div>
    </div>
    {{/if}}
</script>

<script id="projects_timeTrakingGroupActionMenu" type="text/x-jquery-tmpl">
    <ul id="timeTrakingGroupActionMenu" class="clearFix contentMenu contentMenuDisplayAll display-none">
    <li class="menuAction menuActionSelectAll menuActionSelectLonely">
        <div class="menuActionSelect">
            <input id="selectAllTimers" type="checkbox" title="<%= TimeTrackingResource.GroupMenuSelectAll %>"/>
                    </div>
        </li>
        <li class="menuAction" data-status="billed" title="<%= TimeTrackingResource.PaymentStatusBilled %>">
            <span><%= TimeTrackingResource.PaymentStatusBilled%></span>
        </li>
        <li class="menuAction" data-status="not-billed" title="<%= TimeTrackingResource.PaymentStatusNotBilled %>">
            <span><%= TimeTrackingResource.PaymentStatusNotBilled%></span>
        </li>
        <li class="menuAction" data-status="not-chargeable" title="<%=TimeTrackingResource.PaymentStatusNotChargeable%>">
            <span><%= TimeTrackingResource.PaymentStatusNotChargeable%></span>
        </li>
        <li class="menuAction" data-status="delete" title="<%= ProjectsCommonResource.Delete%>">
            <span><%= ProjectsCommonResource.Delete%></span>
        </li>

        <li class="menu-action-checked-count">
            <span></span>
            <a id="deselectAllTimers" class="link dotline small">
                <%= TimeTrackingResource.GroupMenuDeselectAll%>
            </a>
        </li>
        <li class="menu-action-simple-pagenav" style="display: list-item;"></li>
        <li class="menu-action-on-top">
            <a class="on-top-link" onclick="javascript:window.scrollTo(0, 0);">
                <%= TimeTrackingResource.GroupMenuOnTop%>
            </a>
        </li>

    </ul>
</script>

<script id="projects_totalTimeText" type="text/x-jquery-tmpl">
    
        <span data-tasktext="<%= TimeTrackingResource.TimeSpentForTask%>" data-listtext="<%= TimeTrackingResource.TotalTimeNote%>"></span>
        <span class="total-count">
            <%=TimeTrackingResource.TotalTimeCommon %>
            <b><span class="hours"></span> <%= TimeTrackingResource.Hours%>
            <span class="minutes"></span> <%= TimeTrackingResource.Minutes%></b>
        </span>
        <span class="billed-count">
            <%=TimeTrackingResource.TotalTimeBilled %> 
            <b><span class="hours"></span> <%= TimeTrackingResource.Hours%>
            <span class="minutes"></span> <%= TimeTrackingResource.Minutes%></b>
        </span>
     
</script>