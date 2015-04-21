<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/basetemplate.master" CodeBehind="Feed.aspx.cs" Inherits="ASC.Web.Studio.Feed" %>

<asp:Content ID="SidePanel" ContentPlaceHolderID="SidePanel" runat="server">
    <asp:PlaceHolder ID="navigationHolder" runat="server"/>
</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="PageContent" runat="server">
    <div id="feed-view">
        <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="controlsHolder" runat="server">
            <div id="feed-filter"></div>
        </asp:PlaceHolder>
        
        <div id="empty-screens-box">
           <asp:PlaceHolder ID="emptyScreensHolder" runat="server"></asp:PlaceHolder> 
        </div>
    </div>
</asp:Content>