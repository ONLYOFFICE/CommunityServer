<%@ Assembly Name="ASC.Web.Calendar" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Masters/basetemplate.master" CodeBehind="~/addons/calendar/default.aspx.cs" Inherits="ASC.Web.Calendar._default" %>

<asp:Content runat="server" ContentPlaceHolderID="SidePanel">
    <asp:PlaceHolder ID="CalendarSidePanel" runat="server"></asp:PlaceHolder>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <asp:PlaceHolder ID="CalendarPageContent" runat="server"></asp:PlaceHolder>
</asp:Content>
