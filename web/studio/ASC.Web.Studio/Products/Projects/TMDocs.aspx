<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" AutoEventWireup="true" CodeBehind="TMDocs.aspx.cs" Inherits="ASC.Web.Projects.TMDocs" Title="Untitled Page" %>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="FilterContent">
    <asp:PlaceHolder runat="server" ID="FilterHolder"></asp:PlaceHolder>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <asp:PlaceHolder runat="server" ID="CommonContainerHolder"></asp:PlaceHolder>
</asp:Content>
