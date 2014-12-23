<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommonList.ascx.cs" Inherits="ASC.Web.Projects.Controls.Common.CommonList" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<div id="filterContainer">
    <div id="ProjectsAdvansedFilter"></div>
</div>

<div class="simplePageNavigator"></div>

<div id="CommonListContainer">
    <div id="groupActionContainer">
        <div class="header-menu-spacer"> </div>
    </div>
    <div class="taskList"></div>

    <table id="tableListProjects">
        <tbody>
        </tbody>
    </table>

    <table id="milestonesList">
        <thead>
        </thead>
        <tbody>
        </tbody>
    </table>
    <div id="discussionsList">
    </div>
    <table id="timeSpendsList" class="listContainer pm-tablebase">
        <thead>
        </thead>
        <tbody>
        </tbody>
    </table>
    <table id="tableForNavigation" cellpadding="4" cellspacing="0" style="display: none">
        <tbody>
        <tr>
            <td>
                <div id="divForTaskPager" class="divPager">
                </div>
            </td>
            <td style="text-align:right;">
                <span class="gray-text"><%= ProjectsCommonResource.Total%> : </span>
                <span class="gray-text" style="margin-right: 20px;" id="totalCount"></span>
                <span class="gray-text"><%= ProjectsCommonResource.ShowOnPage%> : </span>
                <select id="countOfRows" class="top-align">
                    <option value="10">10</option>
                    <option value="20">20</option>
                    <option value="25">25</option>
                    <option value="30">30</option>
                    <option value="40">40</option>
                    <option value="50">50</option>
                    <option value="75">75</option>
                    <option value="100">100</option>
                </select> 
            </td>
        </tr>
        </tbody>
    </table>
</div>
