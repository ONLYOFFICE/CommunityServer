<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Community.Bookmarking.Bookmarking" MasterPageFile="~/Products/Community/Master/Community.Master" %>


<asp:Content ContentPlaceHolderID="CommunityPageContent" runat="server">
    <asp:PlaceHolder ID="BookmarkingPageContent" runat="server"/>
</asp:Content>

<asp:Content ContentPlaceHolderID="CommunityPagingContent" runat="server">
    <asp:PlaceHolder ID="BookmarkingPagingContent" runat="server"/>
</asp:Content>