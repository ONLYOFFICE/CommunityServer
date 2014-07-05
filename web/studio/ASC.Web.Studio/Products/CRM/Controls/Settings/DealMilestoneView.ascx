<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DealMilestoneView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.DealMilestoneView" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>

<p style="margin-bottom: 10px;"><%= CRMSettingResource.DescriptionTextDealMilestone %></p>
<p style="margin-bottom: 20px;"><%= CRMSettingResource.DescriptionTextDealMilestoneEditDelete %></p>

<div class="clearFix" style="margin-bottom: 8px;">
    <a id="createNewDealMilestone" class="gray button">
        <span class="plus"><%= CRMSettingResource.CreateNewDealMilestoneListButton%></span>
    </a>
</div>

<ul id="dealMilestoneList">
</ul>

<div id="dealMilestoneActionMenu" class="studio-action-panel" dealmilestoneid="">
    <div class="corner-top right"></div>
    <ul class="dropdown-content">
        <li><a class="dropdown-item" onclick="ASC.CRM.DealMilestoneView.showEditDealMilestonePanel();"><%= CRMSettingResource.EditDealMilestone%></a></li>
        <li><a class="dropdown-item" onclick="ASC.CRM.DealMilestoneView.deleteDealMilestone();"><%= CRMSettingResource.DeleteDealMilestone%></a></li>
    </ul>
</div>

<div id="colorsPanel" class="studio-action-panel colorsPanelSettings">
    <div class="corner-top left"></div>
    <span class="style1" colorstyle="1"></span><span class="style2" colorstyle="2"></span><span class="style3" colorstyle="3"></span><span class="style4" colorstyle="4"></span><span class="style5" colorstyle="5"></span><span class="style6" colorstyle="6"></span><span class="style7" colorstyle="7"></span><span class="style8" colorstyle="8"></span>
    <span class="style9" colorstyle="9"></span><span class="style10" colorstyle="10"></span><span class="style11" colorstyle="11"></span><span class="style12" colorstyle="12"></span><span class="style13" colorstyle="13"></span><span class="style14" colorstyle="14"></span><span class="style15" colorstyle="15"></span><span class="style16" colorstyle="16"></span>
</div>