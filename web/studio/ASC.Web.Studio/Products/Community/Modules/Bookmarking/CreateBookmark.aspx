<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateBookmark.aspx.cs" Inherits="ASC.Web.Community.Bookmarking.CreateBookmark" MasterPageFile="~/Products/Community/Master/Community.master" %>
<%@ Import Namespace="ASC.Web.UserControls.Bookmarking.Resources" %>

<asp:Content ID="BookmarkingPageContent" ContentPlaceHolderID="CommunityPageContent" runat="server">
    <div class="header-base-big"><%= BookmarkingUCResource.AddBookmarkLink %></div>
    <asp:PlaceHolder ID="BookmarkingPageContent" runat="server"></asp:PlaceHolder>
</asp:Content>

<asp:Content ID="BookmarkingSidePanel" ContentPlaceHolderID="CommunitySidePanel" runat="server">
    <asp:PlaceHolder ID="BookmarkingSideHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
