<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" EnableViewState="false" AutoEventWireup="true" CodeBehind="ManagementCenter.aspx.cs" Inherits="ASC.Web.Community.Forum.ManagementCenter" Title="Untitled Page" %>
<%@ Import Namespace="ASC.Web.Community.Forum.Resources" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ForumPageHeader" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ForumPageContent" runat="server">
<asp:PlaceHolder ID="controlPanel" runat="server"></asp:PlaceHolder>
</asp:Content>