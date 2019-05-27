<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportFile.ascx.cs" Inherits="ASC.Web.Projects.Controls.Reports.ReportFile" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<div class="report-name"><%= ReportResource.GeneratedReports %></div>
<div class="report-description">
    <ul>
        <li>
            <%=ReportResource.GeneratedReportsDescription%>
        </li>
    </ul>
</div>
<div>
    <asp:PlaceHolder runat="server" ID="phAttachmentsControl" />
</div>