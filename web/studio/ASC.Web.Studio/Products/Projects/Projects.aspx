<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="Projects.aspx.cs" Inherits="ASC.Web.Projects.Projects" %>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="_content"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="_listProjects"></asp:PlaceHolder>

</asp:Content>

