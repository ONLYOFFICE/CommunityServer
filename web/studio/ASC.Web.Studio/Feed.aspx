<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/BaseTemplate.master" CodeBehind="Feed.aspx.cs" Inherits="ASC.Web.Studio.Feed" %>

<asp:Content ContentPlaceHolderID="SidePanel" runat="server">
    <asp:PlaceHolder ID="navigationHolder" runat="server"/>
</asp:Content>

<asp:Content ContentPlaceHolderID="FilterContent" runat="server">
    <div id="feed-filter"></div>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div id="feed-view">
        <asp:PlaceHolder ID="loaderHolder" runat="server"/>
        <asp:PlaceHolder ID="controlsHolder" runat="server"/>
        <div id="empty-screens-box">
            <asp:PlaceHolder ID="emptyScreensHolder" runat="server"/>
        </div>
    </div>
</asp:Content>