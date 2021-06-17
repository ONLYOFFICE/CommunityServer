<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VisitorsChart.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Statistics.VisitorsChart" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div class="chartBlock">
  <div class="header-base header">
    <%=Resource.VisitorsChartTitle%>
  </div>
  <div class="content">
    <ul id="visitorsFilter">
      <li id="filterByWeek" class="filter"><%=Resource.FilterLastWeek%></li>
      <li id="filterByMonth" class="filter"><%=Resource.FilterLastMonth%></li>
      <li id="filterBy3Months" class="filter selected"><%=Resource.FilterLast3Months%></li>
      <li id="filterByPeriod" class="filter"><%=Resource.FilterInPeriod%></li>
    </ul>
    <div id="periodSelection" class="date disabled">
      <div id="startDate" class="date">
        <span class="label"><%=Resource.FilterInPeriodFromLabel%></span>
        <input id="studio_chart_FromDate" type="text" class="textCalendar textEditCalendar" disabled="disabled" autocomplete="off" />
      </div>
      <div id="endDate" class="date">
        <span class="label"><%=Resource.FilterInPeriodToLabel%></span>
        <input id="studio_chart_ToDate" type="text" class="textCalendar textEditCalendar" disabled="disabled" autocomplete="off" />
      </div>
    </div>
    <br class="clear" />
    <div id="visitorsChartCanvas"></div>
    <ul id="chartLegend">
      <li class="label default">
        <div class="color"></div>
        <div class="title"></div>
      </li>
    </ul>
  </div>
</div>
