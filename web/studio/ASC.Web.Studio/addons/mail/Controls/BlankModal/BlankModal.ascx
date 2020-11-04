<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BlankModal.ascx.cs" Inherits="ASC.Web.Mail.Controls.BlankModal" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>
<%@ Import Namespace="ASC.Web.Mail.Configuration" %>

<div class="backdrop hidden" blank-page=""></div>

<div id="content" blank-page="" class="dashboard-center-box mail hidden">
    <a class="close">&times;</a>
    <div class="content">
        <div class="slick-carousel">
            <% if (IsAdmin() && Settings.IsAdministrationPageAvailable())
                { %>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img use-mail-server"></div>
                <div class="text">
                    <div class="title"><%= MailResource.DashboardUseMailServer %></div>
                    <p><%= MailResource.DashboardUseMailServerFirstLine %></p>
                    <p><%= MailResource.DashboardUseMailServerSecondLine %></p>
                    <p><%= MailResource.DashboardUseMailServerThirdLine %></p>
                </div>
            </div>
            <% } %>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img collect-all-emails-in-one-place"></div>
                <div class="text">
                    <div class="title"><%= MailResource.DashboardCollectAllEmailsInOnePlace %></div>
                    <p><%= MailResource.DashboardCollectAllEmailsInOnePlaceFirstLine %></p>
                    <p><%= MailResource.DashboardCollectAllEmailsInOnePlaceSecondLine %></p>
                    <p><%= MailResource.DashboardCollectAllEmailsInOnePlaceThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img structure-correspondence"></div>
                <div class="text">
                    <div class="title"><%= MailResource.DashboardStructureCorrespondence %></div>
                    <p><%= MailResource.DashboardStructureCorrespondenceFirstLine %></p>
                    <p><%= MailResource.DashboardStructureCorrespondenceSecondLine %></p>
                    <p><%= MailResource.DashboardStructureCorrespondenceThirdLine %></p>
                </div>
            </div>
            <div class="module-block slick-carousel-item clearFix">
                <div class="img communicate-easily"></div>
                <div class="text">
                    <div class="title"><%= MailResource.DashboardCommunicateEasily %></div>
                    <p><%= MailResource.DashboardCommunicateEasilyFirstLine %></p>
                    <p><%= MailResource.DashboardCommunicateEasilySecondLine %></p>
                    <p><%= MailResource.DashboardCommunicateEasilyThirdLine %></p>
                </div>
            </div>
        </div>
    </div>
    <div class="dashboard-buttons">
        <a class="button huge create-button" href="javascript:void(0)" onclick=" blankModal.addAccount(); "><%= MailResource.BlankModalCreateBtn %></a>
        <% if (IsAdmin())
           { %>
            <% if (Settings.IsAdministrationPageAvailable())
               { %>
            <span class="or-split"><%= MailResource.Or %></span>
            <a class="link create-button-link" href="javascript:void(0)" onclick=" blankModal.setUpDomain(); "><%= MailResource.BlankModalSetUpDomainBtn %></a>
            <% } %>
        <% }
           else
           { %>
            <% if (Settings.IsMailCommonDomainAvailable())
               { %>
            <span class="or-split"><%= MailResource.Or %></span>
            <a class="link create-button-link" href="javascript:void(0)" onclick=" blankModal.addMailbox(); "><%= MailResource.BlankModalCreateMailboxBtn %></a>
            <% } %>
        <% } %>
    </div>
</div>