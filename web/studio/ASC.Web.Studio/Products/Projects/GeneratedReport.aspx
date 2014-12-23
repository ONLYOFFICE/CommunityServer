<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="GeneratedReport.aspx.cs" Inherits="ASC.Web.Projects.GeneratedReport" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    <link href="<%= PathProvider.GetFileStaticRelativePath("reportsPrint.css") %>" rel="stylesheet" type="text/css" media="print"/>
    <div class="generated-report-container borderBase">    
       <div class="generated-report-header-container">
           <asp:Literal ID="reportFilter" runat="server"></asp:Literal>
            <% if (HasData) {%>
                <div class="generated-report-buttons">
                    <a id="printReportButton" class="button blue middle"><%=ReportResource.PrintReport%></a>
                    <% if (!string.IsNullOrEmpty(ASC.Web.Core.Files.FilesLinkUtility.DocServiceApiUrl))
                       { %>
                    <span class="splitter-buttons"></span>
                    <a id="exportReportButton" class="button gray middle"><%= ReportResource.ExportToCSV %></a>
                    <% } %>
                </div>
                <div id="exportPopup" style="display: none">
                    <sc:Container ID="exportReportPopup" runat="server">
                    <Header>
                        <%=ReportResource.ExportToCSV%>
                    </Header>
                    <Body>
                        <p><%= ReportResource.ExportToCSVNote%></p>
                        <div class="neverShowContainer clearFix">
                            <input id="neverShowPopup" type="checkbox" />
                            <label for="neverShowPopup"><%=ReportResource.ExportToCSVCheckboxLabel %></label>
                        </div>
                        <div class="middle-button-container">
                            <a id="okExportButton" class="button blue middle"><%= ProjectResource.OkButton%></a>
                        </div>
                    </Body>
                    </sc:Container>
                </div>
            <% } %>
        </div>
        <div class="generated-report-filter-container">
            <asp:PlaceHolder ID="_filter" runat="server"/>
            <div class="middle-button-container">
                <a id="generateNewReport" class="button blue"><%=ReportResource.GenerateReport%></a>
            </div>
        </div>
 
        <asp:Literal ID="reportResult" runat="server"></asp:Literal>
    <div id="emptyScreenContent">
        <asp:PlaceHolder ID="emptyScreenControlPh" runat="server"></asp:PlaceHolder>
    </div>
   </div>
</asp:Content>