<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/People/Masters/PeopleBaseTemplate.Master" CodeBehind="Help.aspx.cs" Inherits="ASC.Web.People.Help" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <div class="clearFix profile-title header-with-menu">
        <span class="header text-overflow" title="<%= PageTitle.HtmlEncode() %>"><%= PageTitle.HtmlEncode() %></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="PeoplePageContent" runat="server">
    <asp:PlaceHolder ID="HelpHolder" runat="server"/>
</asp:Content>