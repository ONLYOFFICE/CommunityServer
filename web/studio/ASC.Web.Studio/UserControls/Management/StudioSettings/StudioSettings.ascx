<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StudioSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.StudioSettings" %>
<%@ Import Namespace="Resources" %>

<%--timezone & language--%>
<div class="clearFix">
    <div id="studio_lngTimeSettings" class="settings-block">
        <div class="header-base clearFix">
            <%= Resource.StudioTimeLanguageSettings %>
        </div>
           <asp:PlaceHolder ID="_timelngHolder" runat="server"></asp:PlaceHolder>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerLngTimeSettings.HtmlEncode(), "<br />", "<b>", "</b>") %></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#CustomizingPortal_block" %>" target="_blank"><%= Resource.LearnMore %></a>
        <% } %>
    </div>
</div>

<%--greeting settings--%>
<asp:PlaceHolder ID="_greetingSettings" runat="server"></asp:PlaceHolder>

<%--DNS settings--%>
<asp:PlaceHolder ID="_dnsSettings" runat="server"></asp:PlaceHolder>

<%--portal rename control--%>
<asp:PlaceHolder ID="_portalRename" runat="server"></asp:PlaceHolder>

<%--version settings--%>
<asp:PlaceHolder ID="_portalVersionSettings" runat="server"></asp:PlaceHolder>

<%-- Promo code --%>
<asp:PlaceHolder ID="promoCodeSettings" runat="server" />