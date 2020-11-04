<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportView.ascx.cs" Inherits="ASC.Web.Projects.Controls.Reports.ReportView" %>
<%@ Import Namespace="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="report-name"><%=Report.ReportInfo.Title %></div>
<div class="report-description">
    <%=Report.ReportInfo.Description%>
    <asp:PlaceHolder ID="_filter" runat="server"/>
</div>

<div class="middle-button-container">
    <a id="generateReport" class="button blue middle" href="javascript:void(0)"><%=ReportResource.GenerateReport%></a>
    <span class="splitter-buttons"></span>
    <a id="createTemplate" class="button gray middle" href="javascript:void(0)"><%= ReportResource.CrteateTemplate%></a>
</div>

<div id="reportTemplatePopup" class="display-none">
    <sc:Container id="reportTemplateContainer" runat="server">
        <header>    
            <%= ReportResource.CrteateTemplate%>
        </header>
        <body>
            <div class="block-cnt-splitter requiredField">
                <span class="requiredErrorText title" error="<%=ReportResource.EmptyTemplateTitle %>"><%=ReportResource.EmptyTemplateTitle %></span>
                <div class="headerPanelSmall">
                    <b><%= ReportResource.TemplateTitle%></b>
                </div>
                <input id="templateTitle" type="text" class="textEdit" style="width:99%;"/>
            </div>
            
            <div class="popup-checkbox-container">
                <input id="autoGeneration" type="checkbox" />
                <label for="autoGeneration"><%=ReportResource.AutomaticallyGenerate%></label>
            </div>
            
            <div class="template-params">
                <select id="generatedPeriods" class="comboBox period-cbox"  disabled="disabled">
                    <option value="day"><%=ReportResource.EveryDay %></option>
                    <option value="week"><%=ReportResource.EveryWeek %></option>
                    <option value="month"><%=ReportResource.EveryMonth %></option>
                </select>
                <div class="variant-conteiner">
                    <select id="week" class="comboBox display-none"  disabled="disabled">
                    <%=TemplateParamInitialiser.InitDaysOfWeek()%>
                    </select>
                    <select id="month" class="comboBox display-none"  disabled="disabled">
                    <%=TemplateParamInitialiser.InitDaysOfMonth()%>
                    </select>
                    <select id="hours" class="comboBox"  disabled="disabled">
                    <%=TemplateParamInitialiser.InitHoursCombobox()%>
                    </select>
               </div>

            </div>

                <div class="middle-button-container">
                    <a id="saveTemplate" class="button blue middle">
                        <%= ProjectsCommonResource.Save%>
                    </a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle">
                        <%= ProjectsCommonResource.Cancel%>
                    </a>
                </div>
        </body>
    </sc:Container>
</div>