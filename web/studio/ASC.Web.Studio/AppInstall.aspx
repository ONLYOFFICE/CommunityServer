<%@ Page MasterPageFile="~/Masters/BaseTemplate.master" Language="C#" AutoEventWireup="true" CodeBehind="AppInstall.aspx.cs" Inherits="ASC.Web.Studio.AppInstall" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="Resources" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
<div class="Container">
    <div class="Caption">
        <h2><%= Resource.FreeOfficeDesktopDndMobileApps %></h2>
    </div>
        
    <% foreach (var groups in Data)
        { %>
    <div class="Caption">
        <p><%= groups.Caption %></p>
    </div>
    <div class="AppsContainer">
        <% foreach (var item in groups.Items)%>
        <%{%><div class="AppsForPC">
                    <div class="Logo <%= item.Title %>">
                        <h3 class="OS"><%= item.OS %></h3>
                    </div>
                <div align="center" class="AppsLink">
                    <div>
                        <span class="EditorsFor <%= item.Icon %>"></span>
                        <h3 class="EditorsForPCText"><%= item.Text %></h3>
                    </div>
                    <div class="GetIt">
                        <a class="<%= item.ButtonStore %>" target="_blank" href="<%= item.Href %>"><%= item.ButtonText %></a>
                    </div>
                </div>
            </div><%}%>
        </div>
    <% } %>
</div>
</asp:Content>