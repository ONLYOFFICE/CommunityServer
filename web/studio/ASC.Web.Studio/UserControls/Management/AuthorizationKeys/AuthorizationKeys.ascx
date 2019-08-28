<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthorizationKeys.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.AuthorizationKeys" %>
<%@ Import Namespace="ASC.Data.Storage" %>
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
                <img src="<%= WebPath.GetPath("UserControls/Management/AuthorizationKeys/img/" + service.Name.ToLower() + ".svg") %>" alt="<%= service.Title %>" />
                <span class="sub-button">
                    <a id="switcherBtn<%= service.Name %>" class="on_off_button <%= service.CanSet ? "" : "disable" %> <%= (service.Props.All(r=> string.IsNullOrEmpty(r.Value))) ? "off" : "on" %>"></a>
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

                    <img src="<%= WebPath.GetPath("UserControls/Management/AuthorizationKeys/img/" + service.Name.ToLower() + ".svg") %>" alt="<%= service.Title %>" />

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
                        <%foreach (var prop in service.Props){%>
                        <div class="bold headerPanelSmall"><%= prop.Title %>:</div>
                        <input id="<%= prop.Name %>" type="text" class="auth-service-key textEdit" placeholder="<%= prop.Title %>" value="<%= prop.Value %>" />
                        <%}%>
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
