<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChooseTimePeriod.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.ChooseTimePeriod.ChooseTimePeriod" %>

<%@ Import Namespace="ASC.Web.Studio.UserControls.Common.ChooseTimePeriod" %>
<%@ Import Namespace="Resources" %>

<div class="select-time-period">
    <select id="generatedTimePeriod" class="comboBox select-time-period-every">
        <option value="day"><%= Resource.EveryDay %></option>
        <option value="week"><%= Resource.EveryWeek %></option>
        <option value="month"><%= Resource.EveryMonth %></option>
    </select>
    <select id="week" class="comboBox display-none">
        <%=TimePeriodInitialiser.InitDaysOfWeek()%>
    </select>
    <select id="month" class="comboBox display-none">
        <%=TimePeriodInitialiser.InitDaysOfMonth()%>
    </select>
    <select id="hours" class="comboBox">
        <%=TimePeriodInitialiser.InitHoursCombobox()%>
    </select>
</div>
