<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserConnections.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserConnections" %>
<%@ Import Namespace="ASC.FederatedLogin.LoginProviders" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<script id="contentConnectionsTemplate" type="text/x-jquery-tmpl">
{{if items.length > 0}}
<div class="logout-all-link">
    <span class="baseLinkAction" onclick="CommonConnectionsManager.OpenLogoutAllPopupDialog();return false;">
        <%= UserControlsCommonResource.LogoutFromAllActiveConnectionsButton %>
    </span>
    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'logoutFromAllConnections'});"></span>
    <div class="popup_helper" id="logoutFromAllConnections"><%= string.Format(Resource.LogoutFromAllActiveConnectionsInfo.HtmlEncode()) %></div>
</div>
<div class="connections-content">
    <table class="tableBase">
        <colgroup>
            <col style="width: 26%">
            <col style="width: 20%">
            <col style="width: 20%">
            <col style="width: 30%">
            <col style="width: 4%">
        </colgroup>
        <tbody>
        {{each(i, item) items}} 
            <tr id="connection_content_${item.id}">
            <td class="borderBase">
                <div class="platform">
                    {{if item.platform}}
                        {{if loginEvent == item.id}}<div class="this-connection">${item.platform}
                            <div class="popup_helper popup-this-connection"><%= UserControlsCommonResource.CurrentConnection %></div>
                        </div>
                        {{else}}
                            ${item.platform}
                        {{/if}}
                    {{else}}
                        <%= ResourceJS.Unknown %>
                    {{/if}}
                </div>
                <div class="browser">
                {{if item.browser}}${item.browser}{{else}}<%= ResourceJS.Unknown %>{{/if}}
                </div>
            </td>
            <td class="borderBase date">${CommonConnectionsManager.GetDisplayDateTime(item.date)}</td>
            <td class="borderBase ip">${item.ip}</td>
            <td class="borderBase">
                <div class="city">
                    ${item.city}
                </div>
                <div class="country">
                    ${item.country}
                </div>
            </td>
            <td class="borderBase logout">
                {{if loginEvent != item.id}}
                <span class="reset-icon" onclick="CommonConnectionsManager.OpenLogoutActiveConnectionPopupDialog(${item.id}, '${item.ip}', '${item.platform}', '${item.browser}');return false;"></span>
                {{/if}}
                </td>
            </tr>
        {{/each}}
        </tbody>
    </table>
</div>
{{else}}
    <div class="gray-text"><%= UserControlsCommonResource.NoData %></div>
{{/if}}
</script>

<a name="connections"></a>

<div class="connections-tabs" >
    <div id="connections_list"></div>
</div>  


<%--popup window--%>
<div id="confirmLogout" style="display: none;">
    <sc:Container runat="server" ID="confirmLogoutDialog">
        <Header><%= UserControlsCommonResource.ConfirmLogoutFromAllActiveConnections %></Header>
        <Body>
            <div><%= UserControlsCommonResource.ConfirmLogoutFromAllActiveConnectionsNote %></div><br />
            <div><%= UserControlsCommonResource.ConfirmLogoutFromAllActiveConnectionsSecurity %></div>
            <div class="middle-button-container">
                <a class="button blue middle" onclick="CommonConnectionsManager.CloseLogOutAllActiveConnectionsChangePassword();return false;">
                    <%= UserControlsCommonResource.ButtonLogoutAndChangePassword %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="CommonConnectionsManager.CloseLogoutFromAllActiveConnectionsExceptThis();return false;">
                    <%= UserControlsCommonResource.ButtonLogout %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= UserControlsCommonResource.ButtonCancel %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>


<div id="confirmLogoutConnection" style="display: none;">
    <sc:Container runat="server" ID="confirmLogoutConnectionDialog">
        <Header><%= UserControlsCommonResource.ConfirmLogoutFromActiveConnection %></Header>
        <Body>
            <div id="confirmLogoutFromConnectionText"></div>
            <div class="middle-button-container">
                <a class="button blue middle" onclick="CommonConnectionsManager.CloseLogoutActiveConnection();return false;">
                    <%= UserControlsCommonResource.ButtonConfirmLogout %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= UserControlsCommonResource.ButtonCancel %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>