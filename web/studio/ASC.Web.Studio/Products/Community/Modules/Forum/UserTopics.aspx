<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" EnableViewState="false" AutoEventWireup="true" CodeBehind="UserTopics.aspx.cs" Inherits="ASC.Web.Community.Forum.UserTopics" Title="Untitled Page" %>

<asp:Content ContentPlaceHolderID="ForumPageContent" runat="server">
    <div class="clearFix">
        <asp:PlaceHolder ID="topicListHolder" runat="server"/>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ForumPagingContent" runat="server">
    <div class="clearFix" style="padding-top: 20px;">
        <asp:PlaceHolder ID="bottomPageNavigatorHolder" runat="server"/>
    </div>
</asp:Content>
