<%@ Assembly Name="ASC.Web.Community" %>
<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" CodeBehind="Default.aspx.cs" MasterPageFile="~/Products/Community/Master/Community.master" Inherits="ASC.Web.Community._Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CommunityPageContent" runat="server">
    <asp:PlaceHolder ID="NavigationPanelContent" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="AddContent" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="WidgetsContent" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="emptyModuleCommunity" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="ImportUsers" runat="server" />
</asp:Content>