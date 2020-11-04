<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
    CodeBehind="Reports.aspx.cs" Inherits="ASC.Web.Projects.Reports" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Projects.Core.Domain.Reports" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>


<asp:Content ID="ListReports" runat="server" ContentPlaceHolderID="AdditionalColumns">
    <div class="reports-menu-container">
        <span class="reports-category"><%= ReportResource.Reports %></span>
        <ul>
            <% var repType = Request["reportType"];
               var currentTmpl = Request["tmplId"];
                
               foreach (var report in ListReports)
               {
                   if (repType == ((int)report.ReportType).ToString(CultureInfo.InvariantCulture) && string.IsNullOrEmpty(currentTmpl))
                   {%>
                <li><a class="menu-report-name active"><%= report.ReportInfo.Title %></a></li>
              <% }
                   else
                   {%>
                <li><a class="menu-report-name" href="Reports.aspx?reportType=<%= (int)report.ReportType %>"><%= report.ReportInfo.Title %></a></li>  
              <% }
               } %>
        </ul>
        <% if (ListTemplates.Count > 0){%>
        <span class="reports-category templates"><%= ReportResource.Templates %></span>
        <ul id="reportsTemplates">
            <% foreach (ReportTemplate tmpl in ListTemplates)
               {
                   if (currentTmpl == tmpl.Id.ToString())
                   {%>
                <li id="<%=tmpl.Id %>"><a class="menu-report-name active" title="<%= tmpl.Name %>"><%= tmpl.Name %></a></li>
               <% }
                   else
                   { %>
                  <li id="<%=tmpl.Id %>"><a class="menu-report-name" href="Reports.aspx?tmplId=<%= tmpl.Id %>&reportType=<%=(int) tmpl.ReportType %>" title="<%= tmpl.Name %>"><%= tmpl.Name %></a></li> 
               <% }
               } %>
        </ul>
        <% } else {%>
        <span class="reports-category templates display-none"><%= ReportResource.Templates %></span>
        <ul id="reportsTemplates">
        </ul>
        <%}%>
        <% if (ReportsCount > 0)
           { %>
        <a class="reports-category generated <%= string.IsNullOrEmpty(repType) ? "active" : "" %>" href="Reports.aspx" title="<%= ReportResource.GeneratedReports %>"><%= ReportResource.GeneratedReports %></a>
        <% } %>
     </div>
</asp:Content>


<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server"> 
    <asp:PlaceHolder ID="_content" runat="server"/>
</asp:Content>