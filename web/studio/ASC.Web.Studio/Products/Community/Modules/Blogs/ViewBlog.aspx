<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Community/Master/Community.Master" AutoEventWireup="true" EnableViewState="false" CodeBehind="ViewBlog.aspx.cs" Inherits="ASC.Web.Community.Blogs.ViewBlog" Title="Untitled Page" %>

<%@ Register Src="Views/ViewBlogView.ascx" TagPrefix="ctrl" TagName="ViewBlogView" %>
<%@ Import Namespace="ASC.Web.Community.Product" %>
<%@ Register TagPrefix="scl" Namespace="ASC.Web.Studio.UserControls.Common.Comments" Assembly="ASC.Web.Studio" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<asp:Content ContentPlaceHolderID="CommunityTitleContent" runat="server">
    <div class="BlogsHeaderBlock header-with-menu" style="margin-bottom: 16px;">
        <span class="main-title-icon blogs"></span>
        <span class="header"><%=HttpUtility.HtmlEncode(BlogTitle)%></span>
        <% if(!CommunitySecurity.IsOutsider()) { %>
        <asp:Literal ID="SubscribeLinkBlock" runat="server"></asp:Literal>
        <span class="menu-small"></span>
        <% } %>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="CommunityPageContent" runat="server">
    <div class="BlogsBodyBlock">
        <sc:Container id="mainContainer" runat="server">
            <header>
            </header>
            <body>
                <sc:NotFoundControl Visible="false" ID="lblMessage" runat="server" />
                <ctrl:ViewBlogView ID="ctrlViewBlogView" runat="server" />
                <div style="margin-top: 30px;">
                    <scl:CommentsList ID="commentList" runat="server">
                    </scl:CommentsList>
                </div>
            </body>
        </sc:Container>
    </div>
</asp:Content>
