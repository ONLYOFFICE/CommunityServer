<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/People/Masters/PeopleBaseTemplate.Master" CodeBehind="Import.aspx.cs" Inherits="ASC.Web.People.Import" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <div class="clearFix profile-title header-with-menu">
        <span class="header text-overflow" title="<%= PageTitle.HtmlEncode() %>"><%= PageTitle.HtmlEncode() %></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="PeoplePageContent" runat="server" >
    <asp:PlaceHolder ID="importUsers" runat="server"/>
</asp:Content>