<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CRMDashboardEmptyScreen.ascx.cs" Inherits="ASC.Web.Studio.UserControls.EmptyScreens.CRMDashboardEmptyScreen" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>


<div id="dashboardBackdrop" class="backdrop display-none" blank-page=""></div>

<div id="dashboardContent" blank-page="" class="dashboard-center-box crm display-none">
    <a class="close">&times;</a>
    <div class="content">
        <div class="slick-carousel">
            <div class="module-block slick-carousel-item clearFix">
                <div class="img create-clients-database"></div>
                <div class="text">
                    <div class="title"><%= CRMCommonResource.DashboardCreateClientsDatabase %></div>
                    <p><%= CRMCommonResource.DashboardCreateClientsDatabaseFirstLine %></p>
                    <p><%= CRMCommonResource.DashboardCreateClientsDatabaseSecondLine %></p>
                    <p><%= CRMCommonResource.DashboardCreateClientsDatabaseThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img track-sales"></div>
                <div class="text">
                    <div class="title"><%= CRMCommonResource.DashboardTrackSales %></div>
                    <p><%= CRMCommonResource.DashboardTrackSalesFirstLine %></p>
                    <p><%= CRMCommonResource.DashboardTrackSalesSecondLine %></p>
                    <p><%= CRMCommonResource.DashboardTrackSalesThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img communicate-with-clients"></div>
                <div class="text">
                    <div class="title"><%= CRMCommonResource.DashboardCommunicateWithClients %></div>
                    <p><%= CRMCommonResource.DashboardCommunicateWithClientsFirstLine %></p>
                    <p><%= CRMCommonResource.DashboardCommunicateWithClientsSecondLine %></p>
                    <p><%= CRMCommonResource.DashboardCommunicateWithClientsThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img customize-your-crm"></div>
                <div class="text">
                    <div class="title"><%= CRMCommonResource.DashboardCustomizeYourCrm %></div>
                    <p><%= CRMCommonResource.DashboardCustomizeYourCrmFirstLine %></p>
                    <p><%= CRMCommonResource.DashboardCustomizeYourCrmSecondLine %></p>
                    <p><%= CRMCommonResource.DashboardCustomizeYourCrmThirdLine %></p>
                </div>
            </div>
        </div>
    </div>
    <div class="dashboard-buttons">
        <a class="button huge create-button" href="<%= VirtualPathUtility.ToAbsolute("~/Products/CRM/Default.aspx?action=manage&type=people") %>">
            <%= CRMCommonResource.CreateNewContact %>
        </a>
    </div>
</div>