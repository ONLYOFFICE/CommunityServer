<%@ Page Title="" Language="C#" MasterPageFile="~/Popup.Master" AutoEventWireup="true" CodeBehind="Invite.aspx.cs" Inherits="ASC.Thrdparty.Web.Invite" %>
<asp:Content ID="Content1" ContentPlaceHolderID="bodyContent" runat="server">
    <ul>
        <li><a class="google" href="<%=ResolveUrl("~/Google/GoogleImportContacts.aspx")%>">Import from Google</a></li>
        <li><a class="yahoo" href="<%=ResolveUrl("~/Yahoo/YahooImport.aspx") %>">Import from Yahoo</a></li>
        <li><a class="live" href="<%=ResolveUrl("~/Live/WindowsLive.aspx") %>">Import from Windows Live</a></li>
    </ul>
</asp:Content>
