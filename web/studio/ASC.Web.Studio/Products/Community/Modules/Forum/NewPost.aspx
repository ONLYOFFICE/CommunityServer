<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" AutoEventWireup="true" CodeBehind="NewPost.aspx.cs" Inherits="ASC.Web.Community.Forum.NewPost" %>


<asp:Content ContentPlaceHolderID="ForumTitleContent" runat="server">
    <div class="forumsHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon forums"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(PageTitle)%></span>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ForumPageContent" runat="server">
    <asp:PlaceHolder ID="_newPostHolder" runat="server"/>
</asp:Content>
