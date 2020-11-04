<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommunityDashboardEmptyScreen.ascx.cs" Inherits="ASC.Web.Studio.UserControls.EmptyScreens.CommunityDashboardEmptyScreen" %>
<%@ Import Namespace="ASC.Web.Community.Resources" %>


<div id="dashboardBackdrop" class="backdrop display-none" blank-page=""></div>

<div id="dashboardContent" blank-page="" class="dashboard-center-box community display-none">
    <a href="<%= ProductStartUrl %>" class="close">&times;</a>
    <div class="content">
        <div class="slick-carousel">
            <div class="module-block slick-carousel-item clearFix">
                <div class="img share-news-and-knowledge"></div>
                <div class="text">
                    <div class="title"><%= CommunityResource.DashboardShareNewsAndKnowledge %></div>
                    <p><%= CommunityResource.DashboardShareNewsAndKnowledgeFirstLine %></p>
                    <p><%= CommunityResource.DashboardShareNewsAndKnowledgeSecondLine %></p>
                    <p><%= CommunityResource.DashboardShareNewsAndKnowledgeThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img discuss-with-team"></div>
                <div class="text">
                    <div class="title"><%= CommunityResource.DashboardDiscussWithTeam %></div>
                    <p><%= CommunityResource.DashboardDiscussWithTeamFirstLine %></p>
                    <p><%= CommunityResource.DashboardDiscussWithTeamSecondLine %></p>
                    <p><%= CommunityResource.DashboardDiscussWithTeamThirdLine %></p>
                </div>
            </div>
            <div class="module-block wiki slick-carousel-item clearFix">
                <div class="img keep-your-team-posted"></div>
                <div class="text">
                    <div class="title"><%= CommunityResource.DashboardKeepYourTeamPosted %></div>
                    <p><%= CommunityResource.DashboardKeepYourTeamPostedFirstLine %></p>
                    <p><%= CommunityResource.DashboardKeepYourTeamPostedSecondLine %></p>
                    <p><%= CommunityResource.DashboardKeepYourTeamPostedThirdLine %></p>
                </div>
            </div>
        </div>
    </div>
    <% if (IsBlogsAvailable) %>
    <% { %>
    <div class="dashboard-buttons">
        <a class="button huge create-button" href="<%= VirtualPathUtility.ToAbsolute("~/Products/Community/Modules/Blogs/AddBlog.aspx") %>">
            <%= CommunityResource.DashboardCreateWelcomePost %>
        </a>
    </div>
    <% } %>
</div>