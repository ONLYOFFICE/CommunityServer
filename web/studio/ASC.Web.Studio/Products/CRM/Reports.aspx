<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master" CodeBehind="Reports.aspx.cs" Inherits="ASC.Web.CRM.Reports" %>
<%@ MasterType TypeName="ASC.Web.CRM.BasicTemplate" %>

<asp:Content ID="AdditionalContainer" runat="server" ContentPlaceHolderID="AdditionalColumns" Visible="False">
    <asp:PlaceHolder ID="AdditionalContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
