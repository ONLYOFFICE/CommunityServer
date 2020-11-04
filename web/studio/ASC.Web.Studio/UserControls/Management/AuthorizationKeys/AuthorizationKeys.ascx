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
        <div id="<%= service.Title %>" class="auth-service-item">
            <div class="auth-service-name clearFix">
                <img src="<%= WebPath.GetPath("UserControls/Management/AuthorizationKeys/img/" + service.Name.ToLower() + ".svg") %>" alt="<%= service.Title %>" class="auth-service-img" />
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

                    <img src="<%= WebPath.GetPath("UserControls/Management/AuthorizationKeys/img/" + service.Name.ToLower() + ".svg") %>" alt="<%= service.Title %>" class="auth-service-img" />

                    <% if (!string.IsNullOrEmpty(service.Instruction))
                       { %>
                    <div class="popup-info-block"><%= string.Format(service.Instruction.HtmlEncode(), "<div class=\"popup-info-separator\"></div>") %></div>
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

                     <% if (!string.IsNullOrEmpty(SupportLink) || !string.IsNullOrEmpty(SupportLink))
                       { %>
                    <div class="popup-info-block">
                        <% if (!string.IsNullOrEmpty(HelpLink))
                           { %>
                        <div><%= string.Format(Resource.AuthorizationKeysHelpTextV11, "<a href=\"" + HelpLink + "/server/windows/community/authorization-keys.aspx#" + service.Name + "\" target=\"_blank\">", "</a>") %></div>
                        <div class="popup-info-separator"></div>
                        <% } %>
                        <% if (!string.IsNullOrEmpty(SupportLink))
                           { %>
                        <div><%= string.Format(Resource.AuthorizationKeysSupportTextV11, "<a href=\""+ SupportLink +"\" target=\"_blank\">", "</a>") %></div>
                        <% } %>
                    </div>
                    <% } %>
                </div>
            </div>
            <% } %>
        </div>
        <% } %>
    </div>
</div>
