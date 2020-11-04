<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master"
                CodeBehind="Tasks.aspx.cs" Inherits="ASC.Web.CRM.Tasks" %>
<%@ MasterType  TypeName="ASC.Web.CRM.BasicTemplate" %>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
