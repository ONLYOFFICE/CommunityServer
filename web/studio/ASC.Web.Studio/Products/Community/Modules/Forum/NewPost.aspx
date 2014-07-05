<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Community.Forum" %>
<%@ Page Language="C#" MasterPageFile="~/Products/Community/Modules/Forum/Forum.Master" 
    AutoEventWireup="true" CodeBehind="NewPost.aspx.cs" Inherits="ASC.Web.Community.Forum.NewPost"
    Title="Untitled Page" %>
<%@ Import Namespace="ASC.Web.Community.Forum" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ForumPageContent" runat="server">
           <asp:PlaceHolder ID="_newPostHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
