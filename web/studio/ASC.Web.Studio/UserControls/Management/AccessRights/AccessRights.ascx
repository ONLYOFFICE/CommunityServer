<%@ Control Language="C#" AutoEventWireup="true" Inherits="ASC.Web.Studio.UserControls.Management.AccessRights" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>

<script id="adminTmpl" type="text/x-jquery-tmpl">
    <tr id="adminItem_${id}" class="adminItem">
        <td class="borderBase adminImg">
            <img src="{{if $item.isRetina}}${bigFotoUrl}{{else}}${smallFotoUrl}{{/if}}" />
        </td>
        <td class="borderBase">
            <a class="link bold" href="${userUrl}">
                ${displayName}
            </a>
            {{if ldap}}<span class="ldap-lock" title=""></span>{{/if}}
            <div>
                ${title}
            </div>
        </td>
        {{each(i, item) accessList}}
        <td class="borderBase cbxCell">
            <input type="checkbox" id="check_${item.pName}_${id}"
            {{if item.pAccess}}checked="checked"{{/if}}
            {{if item.disabled}}disabled="disabled"{{/if}}
                   onclick=" ASC.Settings.AccessRights.setAdmin(this, '${item.pId}') " >
        </td>
        {{/each}}
    </tr>
</script>

<div class="header-base owner">
    <%= Resource.PortalOwner %>
</div>

<div class="clearFix">
    <div class="accessRights-ownerCard borderBaseShadow float-left">
        <asp:PlaceHolder runat="server" ID="_phOwnerCard" />
    </div>
    <div class="possibilitiesAdmin">
        <div><%= Resource.AccessRightsOwnerCan %>:</div>
        <% foreach (var item in Resource.AccessRightsOwnerOpportunities.Split('|')) %>
        <% { %>
            <div class="simple-marker-list"><%= item.Trim() %>;</div>
        <% } %>
    </div>
</div>

<% if (CanOwnerEdit) %>
<% { %>
    <div id="ownerSelectorContent" class="clearFix">
        <div class="changeOwnerText">
            <%= Resource.AccessRightsChangeOwnerText %>
        </div>
        <span class="link dotline" data-id="" id="ownerSelector">
            <%= Resource.ChooseOwner %>
        </span>
        <div class="changeOwnerTextBlock">
            <a class="button blue disable" id="changeOwnerBtn"><%= Resource.AccessRightsChangeOwnerButtonText %></a>
            <span class="splitter"></span>
            <span class="describe-text"><%= Resource.AccessRightsChangeOwnerConfirmText %></span>
        </div>
    </div>
<% } %>

<div class="tabs-section">
    <span class="header-base">
        <span><%= Resource.AdminSettings %></span>
    </span> 
    <span id="switcherAccessRights_Admin" data-id="Admin" class="toggle-button"
          data-switcher="0" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
        <%= Resource.Hide %>
    </span>
</div>

<div id="accessRightsContainer_Admin" class="accessRights-content accessRightsTable">
    <table id="adminTable" class="tableBase display-none" cellpadding="4" cellspacing="0">
        <thead>
            <tr>
                <th></th>
                <th></th>
                <th class="cbxHeader">
                    <%= Resource.AccessRightsFullAccess %>
                    <div class="HelpCenterSwitcher" onclick=" jq(this).helper({ BlockHelperID: 'full_panelQuestion' }); "></div>
                </th>
                <% foreach (var p in Products) %>
                <% { %>
                    <th class="cbxHeader">
                        <%= p.Name %>
                        <% if (p.GetAdminOpportunities().Count > 0) %>
                        <% { %>
                            <div class="HelpCenterSwitcher" onclick=" jq(this).helper({ BlockHelperID: '<%= p.GetSysName() %>_panelQuestion' }); "></div>
                        <% } %>
                    </th>
                <% } %>
            </tr>
        </thead>
        <tbody></tbody>
    </table>
    <div id="adminAdvancedSelector" class="advanced-selector-select">
          <%= CustomNamingPeople.Substitute<Resource>("ChooseUser").HtmlEncode() %>
     </div>
    <div>
        <div id="full_panelQuestion" class="popup_helper">
            <% for (var i = 0; i < FullAccessOpportunities.Length; i++) %>
            <% { %>
                <% if (i == 0) %>
                <% { %>
                    <div><%= FullAccessOpportunities[i] %>:</div>
                <% }  else { %>
                    <div class="simple-marker-list"><%= FullAccessOpportunities[i].TrimEnd(' ', ';', '.') + (i == FullAccessOpportunities.Length - 1 ? "." : ";") %></div>
                <% } %>
            <% } %>
        </div>
        <% foreach (var p in Products) %>
        <% { %>
            <% if (p.GetAdminOpportunities().Count > 0) %>
            <% { %>
                <div id="<%= p.GetSysName() %>_panelQuestion" class="popup_helper">
                    <div><%= String.Format(Resource.AccessRightsProductAdminsCan, p.Name) %>:</div>
                    <% var last = p.GetAdminOpportunities().Last();
                       foreach (var oprtunity in p.GetAdminOpportunities()) %>
                    <% { %>
                        <div class="simple-marker-list"><%= oprtunity.TrimEnd(' ', ';', '.') + (oprtunity == last ? "." : ";") %></div>
                    <% } %>
                </div>
            <% } %>
        <% } %>
    </div>
</div>