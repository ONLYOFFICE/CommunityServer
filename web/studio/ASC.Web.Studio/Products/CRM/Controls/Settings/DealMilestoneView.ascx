<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DealMilestoneView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.DealMilestoneView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<ul id="dealMilestoneList"></ul>

<div id="dealMilestoneActionMenu" class="studio-action-panel" dealmilestoneid="">
    <ul class="dropdown-content">
        <li><a class="dropdown-item with-icon edit" onclick="ASC.CRM.DealMilestoneView.showEditDealMilestonePanel();"><%= CRMSettingResource.EditDealMilestone%></a></li>
        <li><a class="dropdown-item with-icon delete" onclick="ASC.CRM.DealMilestoneView.deleteDealMilestone();"><%= CRMSettingResource.DeleteDealMilestone%></a></li>
    </ul>
</div>

<div id="colorsPanel" class="studio-action-panel colorsPanelSettings">
    <span class="style1" colorstyle="1"></span><span class="style2" colorstyle="2"></span><span class="style3" colorstyle="3"></span><span class="style4" colorstyle="4"></span><span class="style5" colorstyle="5"></span><span class="style6" colorstyle="6"></span><span class="style7" colorstyle="7"></span><span class="style8" colorstyle="8"></span>
    <span class="style9" colorstyle="9"></span><span class="style10" colorstyle="10"></span><span class="style11" colorstyle="11"></span><span class="style12" colorstyle="12"></span><span class="style13" colorstyle="13"></span><span class="style14" colorstyle="14"></span><span class="style15" colorstyle="15"></span><span class="style16" colorstyle="16"></span>
</div>