<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthorizationKeys.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.AuthorizationKeys" %>
<%@ Import Namespace="Resources" %>

<div id="authKeysContainer">
    <div class="header-base"><%= Resource.AuthorizationKeys %></div>

    <p class="auth-service-text">
        <%: Resource.AuthorizationKeysText %>
        <br />
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/server/windows/community/authorization-keys.aspx" %>" target="_blank"><% = Resource.LearnMore %></a>
        <% } %>
    </p>

    <div class="auth-service-block clearFix">
        <% foreach (var service in AuthServiceList)
           { %>
        <div class="auth-service-item">
            <div class="auth-service-name clearFix">
                <img src="<%= VirtualPathUtility.ToAbsolute("~/usercontrols/management/authorizationkeys/img/" + service.Name.ToLower() + ".png") %>" alt="<%= service.Title %>" />
                <span class="sub-button">
                    <a id="switcherBtn<%= service.Name %>" class="on_off_button <%= service.CanSet ? "" : "disable" %> <%= (service.Secret == null || string.IsNullOrEmpty(service.Secret.Value)) ? "off" : "on" %>"></a>
                </span>
            </div>
            <div class="auth-service-dscr"><%= service.Description ?? service.Title %></div>
            <% if (service.CanSet)
               { %>
            <div id="popupDialog<%= service.Name %>" class="popupContainerClass display-none">
                <div class="containerHeaderBlock">
                    <table style="width: 100%;">
                        <tbody>
                            <tr>
                                <td><%= service.Title %></td>
                                <td class="popupCancel">
                                    <div class="cancelButton">×</div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="containerBodyBlock clearFix">

                    <img src="<%= VirtualPathUtility.ToAbsolute("~/usercontrols/management/authorizationkeys/img/" + service.Name.ToLower() + ".png") %>" alt="<%= service.Title %>" />

                    <% if (!string.IsNullOrEmpty(service.Instruction))
                       { %>
                    <div class="popup-info-block"><%= service.Instruction.HtmlEncode() %></div>
                    <% } %>

                    <% if (!string.IsNullOrEmpty(HelpLink))
                       { %>
                    <div class="popup-info-block">
                        <div class="bold headerPanelSmall"><%= Resource.AuthorizationKeysHelpHeader %></div>
                        <%= string.Format(Resource.AuthorizationKeysHelpText, "<a href=\"" + HelpLink + "/server/windows/community/authorization-keys.aspx#" + service.Name + "\" target=\"_blank\">", "</a>") %>
                    </div>
                    <% } %>

                    <div>
                        <% if (service.Key != null)
                           { %>
                        <div class="bold headerPanelSmall"><%= service.Key.Title %>:</div>
                        <input id="<%= service.Key.Name %>" type="text" class="auth-service-key textEdit" placeholder="<%= service.Key.Title %>" value="<%= service.Key.Value %>" />
                        <% } %>
                        <% if (service.Secret != null)
                           { %>
                        <div class="bold headerPanelSmall"><%= service.Secret.Title %>:</div>
                        <input id="<%= service.Secret.Name %>" type="text" class="auth-service-key textEdit" placeholder="<%= service.Secret.Title %>" value="<%= service.Secret.Value %>" />
                        <% } %>
                        <% if (service.KeyDefault != null)
                           { %>
                        <div class="bold headerPanelSmall"><%= service.KeyDefault.Title %>:</div>
                        <input id="<%= service.KeyDefault.Name %>" type="text" class="auth-service-key textEdit" placeholder="<%= service.KeyDefault.Title %>" value="<%= service.KeyDefault.Value %>" />
                        <% } %>
                        <% if (service.SecretDefault != null)
                           { %>
                        <div class="bold headerPanelSmall"><%= service.SecretDefault.Title %>:</div>
                        <input id="<%= service.SecretDefault.Name %>" type="text" class="auth-service-key textEdit" placeholder="<%= service.SecretDefault.Title %>" value="<%= service.SecretDefault.Value %>" />
                        <% } %>
                    </div>

                    <div class="small-button-container">
                        <a id="saveBtn<%= service.Name %>" class="button blue middle saveButton"><%= Resource.AuthorizationKeysEnableButton %></a>
                    </div>

                    <% if (!string.IsNullOrEmpty(SupportLink))
                       { %>
                    <div class="popup-info-block">
                        <%= string.Format(Resource.AuthorizationKeysSupportText, "<a href=\""+ SupportLink +"\" target=\"_blank\">", "</a>") %>
                    </div>
                    <% } %>
                </div>
            </div>
            <% } %>
        </div>
        <% } %>
    </div>
</div>
