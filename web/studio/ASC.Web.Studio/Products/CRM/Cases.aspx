<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master" CodeBehind="Cases.aspx.cs" Inherits="ASC.Web.CRM.Cases" %>

<%@ MasterType TypeName="ASC.Web.CRM.BasicTemplate" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <asp:PlaceHolder ID="TabsHolder" runat="server" Visible="False">
        <div id="CaseTabs"></div>
    </asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
