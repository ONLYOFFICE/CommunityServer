<%@ Assembly Name="ASC.Web.Calendar" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/BaseTemplate.master" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Calendar.Default" %>

<asp:Content ContentPlaceHolderID="CreateButtonContent" runat="server">
    <asp:PlaceHolder ID="CreateButtonContent" runat="server"/>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidePanel" runat="server">
    <asp:PlaceHolder ID="SidePanelContent" runat="server"/>
</asp:Content>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <asp:PlaceHolder ID="CalendarPageContent" runat="server"/>
</asp:Content>
