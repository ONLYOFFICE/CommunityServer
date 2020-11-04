<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Help.aspx.cs" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master" Inherits="ASC.Web.CRM.Help" %>
<%@ MasterType  TypeName="ASC.Web.CRM.BasicTemplate" %>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="HelpHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
