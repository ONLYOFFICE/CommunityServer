<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportFilters.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Reports.ReportFilters" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Projects.Core.Domain.Reports" %>

<% if (Report.ReportType == ReportType.MilestonesExpired || Report.ReportType == ReportType.TasksExpired) %>
<% { %>
<div id="reportFilters" class="report-filter-container display-none">
        <div class="filter-item-container">
            <span class="describe-text"><%=ProjectResource.Tags%>:</span>
            <span class="splitter"></span>
            <select id="Tags" class="comboBox pm-report-select">
                    <%=InitTagsDdl()%>
            </select>
        </div>
        <div class="filter-item-container">
            <span class="describe-text"><%=ProjectResource.Project%>:</span>
            <span class="splitter"></span>
            <select id="Projects" class="comboBox pm-report-select">
                <%=InitProjectsDdl()%>
            </select>
        </div>
</div>
<% } %>
<% if (Report.ReportType == ReportType.MilestonesNearest) %>
<% { %>
<div id="reportFilters" class="report-filter-container display-none">
        <div class="filter-item-container">
            <span class="describe-text"><%=ProjectResource.Tags%>:</span>
            <span class="splitter"></span>
            <select id="Tags" class="comboBox pm-report-select">
                    <%=InitTagsDdl()%>
            </select>
        </div>
        <div class="filter-item-container">
            <span class="describe-text"><%=ProjectResource.Project%>:</span>
            <span class="splitter"></span>
            <select id="Projects" class="comboBox pm-report-select">
                <%=InitProjectsDdl()%>
            </select>
        </div>
        <div class="filter-item-container">
            <span class="describe-text"><%=ReportResource.ChooseNearestMilestonesTimePeriod%>:</span>
            <span class="splitter"></span>
            <select id="UpcomingIntervals" class="comboBox pm-report-select">
                <%=InitUpcomingIntervalsDdl(false)%>
            </select>
        </div>
</div>
<% } %>
<% if (Report.ReportType == ReportType.UsersWithoutActiveTasks || Report.ReportType == ReportType.UsersWorkload) %>
<% { %>
<div id="reportFilters" class="report-filter-container display-none">
    <div class="filter-option-container">
        <input id="departmentReport" name="reportType" type="radio"  <% if(Report.Filter.ViewType == 0) {%> checked="checked" <%} %>/>
        <label for="departmentReport"><%=HttpUtility.HtmlEncode(ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<ReportResource>("ViewByDepartments"))%></label>
        <span class="splitter"></span>
        <input id="projectReport" name="reportType" type="radio"  <% if(Report.Filter.ViewType == 1) {%> checked="checked" <%} %>/>
        <label for="projectReport"><%=ReportResource.ViewByProjects%></label>
    </div>
    <div id="departmentFilterContainer" class="report-inline-container" <% if(Report.Filter.ViewType == 1) {%> style="display:none;" <%} %>>
        <div class="filter-item-container">
            <span class="describe-text"><%=HttpUtility.HtmlEncode(ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<ReportResource>("ViewByDepartments"))%>:</span>
            <span class="splitter"></span>
            <select id="Departments" class="comboBox pm-report-select">
                <%=InitDepartmentsDdl()%>
            </select>
        </div>
    </div>
    <div id="projectFiltersContainer"  <% if(Report.Filter.ViewType == 0) {%> style="display:none;" <%} %> >
        <div class="filter-item-container">
            <span class="describe-text"><%=ProjectResource.Tags%>:</span>
            <span class="splitter"></span>
            <select id="Tags" class="comboBox pm-report-select">
                    <%=InitTagsDdl()%>
            </select>
        </div>
        <div class="filter-item-container">
            <span class="describe-text"><%=ProjectResource.Project%>:</span>
            <span class="splitter"></span>
            <select id="Projects" class="comboBox pm-report-select">
                <%=InitProjectsDdl()%>
            </select>
        </div>
    </div>
    <% if (Report.ReportType == ReportType.UsersWorkload){ %>
        <div id="userFilterContainer" class="filter-item-container">
            <span class="describe-text"><%=ReportResource.User%>:</span>
            <span class="splitter"></span>
            <select id="Users" class="comboBox pm-report-select">
                <%=InitUsersDdl()%>
            </select>
        </div>
    <% } %>
</div>
<% } %>
<% if (Report.ReportType == ReportType.ProjectsWithoutActiveMilestones || Report.ReportType == ReportType.ProjectsWithoutActiveTasks || Report.ReportType == ReportType.ProjectsList) %>
<% { %>
<div id="reportFilters" class="report-filter-container display-none">
   <div class="filter-item-container">
        <span class="describe-text"><%=HttpUtility.HtmlEncode(ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<ReportResource>("ViewByDepartments"))%>:</span>
        <span class="splitter"></span>
        <select id="Departments" class="comboBox pm-report-select">
            <%=InitDepartmentsDdl()%>
        </select>
   </div>
   <div class="filter-item-container">
        <span class="describe-text"><%=ReportResource.User%>:</span>
        <span class="splitter"></span>
        <select id="Users" class="comboBox pm-report-select">
            <%=InitUsersDdl()%>
        </select>
   </div>
    <% if (Report.ReportType == ReportType.ProjectsList) %>
    <% { %>
    <div class="filter-item-container">
        <input id="cbxViewClosedProjects" type="checkbox" <% if(Report.Filter.ProjectStatuses.Count == 0) {%> checked="checked" <%} %> />
        <label for="cbxViewClosedProjects">
            <%=ReportResource.ViewClosedProjects%></label>
    </div>
    <% } %>
</div>
<% } %>
<% if (Report.ReportType == ReportType.UsersActivity || Report.ReportType == ReportType.TimeSpend) %>
<% { %>
<div id="reportFilters" class="report-filter-container display-none">
    <div class="filter-item-container">
        <span class="describe-text"><%=HttpUtility.HtmlEncode(ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<ReportResource>("ViewByDepartments"))%>:</span>
        <span class="splitter"></span>
        <select id="Departments" class="comboBox pm-report-select">
            <%=InitDepartmentsDdl()%>
        </select>
    </div>
    <div class="filter-item-container">
        <span class="describe-text"><%=ReportResource.User%>:</span>
        <span class="splitter"></span>
        <select id="Users" class="comboBox pm-report-select">
            <%=InitUsersDdl()%>
        </select>
    </div>
    <% if (Report.ReportType == ReportType.TimeSpend) %>
    <% { %>
        <div class="filter-item-container">
            <span class="describe-text"><%= ReportResource.PaymentStatuses%>:</span>
            <span class="splitter"></span>
            <select id="PaymentsStatuses" class="comboBox pm-report-select">
                <%=InitPaymentsStatusesDdl(true)%>
            </select>           
        </div>    
    <% } %>
    <div class="filter-item-container">
        <span class="describe-text"><%= ReportResource.ChooseTimeInterval%>:</span>
        <span class="splitter"></span>
        <select id="TimeIntervals" class="comboBox pm-report-select">
            <%=InitTimeIntervalsDdl()%>
        </select>
        <span id="otherInterval" <%if(Report.Filter.TimeInterval != ReportTimeInterval.Absolute || Report.Filter.FromDate.Equals(DateTime.MinValue)){%> style="display:none;" <%} %> >
                <asp:TextBox runat="server" ID="fromDate" CssClass="textEditCalendar" Width="75px" />
                <div class="splitter-input"></div>
                <asp:TextBox runat="server" ID="toDate" CssClass="textEditCalendar" Width="75px" />
            <div class="errorText display-none float-left" style="margin:3px 0 0 10px;"><%=ReportResource.IncorrectDateRage%></div>
        </span>             
    </div>
        <% if (Report.ReportType == ReportType.TimeSpend) %>
    <% { %>
        <div class="clearFix"></div>
        <div class="filter-item-container">
            <input name="type_rbl" id="byUsers" value="0" type="radio" <% if(Report.Filter.ViewType == 0) {%> checked="checked" <%} %> />
            <label for="byUsers">
                <%= ReportResource.ViewByUsers %></label>
        </div>
        <div class="filter-item-container">
            <input name="type_rbl" id="byTasks" value="1" type="radio" <% if(Report.Filter.ViewType == 1) {%> checked="checked" <%} %> />
            <label for="byTasks">
                <%= ReportResource.ViewByUserTasks %></label>
        </div> 
    <% } %>
</div>
<% } %>
<% if (Report.ReportType == ReportType.TasksByProjects || Report.ReportType == ReportType.TasksByUsers) %>
<% { %>
<div id="reportFilters" class="report-filter-container display-none">
    <% if (Report.ReportType == ReportType.TasksByProjects) %>
    <% { %>
    <div class="filter-item-container">
        <span class="describe-text"><%=ProjectResource.Tags%>:</span>
        <span class="splitter"></span>
        <select id="Tags" class="comboBox pm-report-select">
                <%=InitTagsDdl()%>
        </select>
    </div>
    <div class="filter-item-container">
        <span class="describe-text"><%=ProjectResource.Project%>:</span>
        <span class="splitter"></span>
        <select id="Projects" class="comboBox pm-report-select">
            <%=InitProjectsDdl()%>
        </select>
    </div>
    <% }else{%>
    <div class="filter-item-container">
        <span class="describe-text"><%=HttpUtility.HtmlEncode(ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<ReportResource>("ViewByDepartments"))%>:</span>
        <span class="splitter"></span>
        <select id="Departments" class="comboBox pm-report-select">
            <%=InitDepartmentsDdl()%>
        </select>
    </div>
    <% } %>
    <div class="filter-item-container">
        <span class="describe-text"><%=ReportResource.User%>:</span>
        <span class="splitter"></span>
        <select id="Users" class="comboBox pm-report-select">
            <%=InitUsersDdl()%>
        </select>
    </div>
    <div class="filter-item-container">
        <span class="describe-text"><%= ReportResource.ChooseTasksTimePeriod%>:</span>
        <span class="splitter"></span>
        <select id="UpcomingIntervals" class="comboBox pm-report-select">
            <%=InitUpcomingIntervalsDdl(true)%>
        </select>           
    </div>            
    <div class="filter-item-container">
        <span class="describe-text"><%= ReportResource.ShowTasks%>:</span>
        <span class="splitter"></span>
        <select id="TaskStatuses" class="comboBox pm-report-select">
            <%=InitTaskStatusesDdl(true)%>
        </select>           
    </div>

    <% if (Report.ReportType == ReportType.TasksByProjects)
       {%>
        <div class="option-container filter-item-container">
            <input id="cbxShowTasksWithoutResponsible" type="checkbox" <% if(Report.Filter.NoResponsible) {%> checked="checked" <%} %> <% if(Report.Filter.ParticipantId.HasValue && !Report.Filter.ParticipantId.Equals(Guid.Empty)) {%> disabled="disabled" <%} %>/>
            <label for="cbxShowTasksWithoutResponsible"><%= TaskResource.ShowTasksWithoutResponsible %></label>
        </div>
    <% } %>

</div>
<% } %>
    <div class="clearFix"></div>