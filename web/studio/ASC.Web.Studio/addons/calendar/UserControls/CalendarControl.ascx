<%@ Assembly Name="ASC.Web.Calendar" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CalendarControl.ascx.cs" Inherits="ASC.Web.Calendar.UserControls.CalendarControl" %>


<%@ Register Src="~/addons/calendar/UserControls/CalendarResources.ascx" TagName="CalendarResources" TagPrefix="ascwc" %>
<%@ Register Src="~/addons/calendar/UserControls/CalendarTemplates.ascx" TagName="CalendarTemplates" TagPrefix="ascwc" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<ascwc:CalendarResources runat="server"></ascwc:CalendarResources>
<ascwc:CalendarTemplates runat="server"></ascwc:CalendarTemplates>

<asp:PlaceHolder runat="server" ID="_sharingContainer"></asp:PlaceHolder>

<script type="text/javascript" src="<%=VirtualPathUtility.ToAbsolute("~/js/third-party/ical.js") %>" type="text/javascript"></script>
<script type="text/javascript" src="<%=VirtualPathUtility.ToAbsolute("~/js/third-party/moment.min.js") %>" type="text/javascript"></script>
<script type="text/javascript" src="<%=VirtualPathUtility.ToAbsolute("~/js/third-party/moment-timezone.min.js") %>" type="text/javascript"></script>
<script type="text/javascript" src="<%=VirtualPathUtility.ToAbsolute("~/js/third-party/rrule.js") %>" type="text/javascript"></script>
<script type="text/javascript" src="<%=VirtualPathUtility.ToAbsolute("~/js/third-party/nlp.js") %>" type="text/javascript"></script>
<script type="text/javascript" src="<%=VirtualPathUtility.ToAbsolute("~/UserControls/Common/ckeditor/ckeditor.js?ver=4.16.1") %>" type="text/javascript"></script>

<div id="asc_calendar"></div>
<div id="asc_event" style="display: none;"></div>

<div id="popupDocumentUploader">
    <asp:PlaceHolder ID="_phDocUploader" runat="server"></asp:PlaceHolder>
</div>

<div id="commonPopup" class="hidden" style="display: none;">
    <sc:Container ID="_commonPopup" runat="server">
    </sc:Container>
</div>