<%@ Assembly Name="ASC.Web.Calendar" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CalendarControl.ascx.cs" Inherits="ASC.Web.Calendar.UserControls.CalendarControl" %>
<%@ Import Namespace="ASC.Data.Storage" %>


<%@Register Src="~/addons/calendar/usercontrols/CalendarResources.ascx" TagName="CalendarResources" TagPrefix="ascwc"  %>



<ascwc:CalendarResources runat="server"></ascwc:CalendarResources>
<asp:PlaceHolder runat="server" ID="_sharingContainer"></asp:PlaceHolder>
<div id="asc_calendar"></div>