<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PersonalFooter.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.PersonalFooter.PersonalFooter" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="personal-footer">
    <div class="personal-footer_w clearFix">
        <% if (!SecurityContext.IsAuthenticated)
           { %>
        <div class="personal-languages">
            <div class="personal-languages_select <%= CultureInfo.CurrentUICulture.Name %>" data-lang="<%= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName %>">
                <span><%= CultureInfo.CurrentUICulture.DisplayName %></span>
            </div>
            <div id="AuthFormLanguagesPanel" class="studio-action-panel">
                <ul class="personal-languages_list dropdown-content">
                    <% foreach (var ci in SetupInfo.EnabledCulturesPersonal)
                       { %>
                    <li class="dropdown-item <%= ci.Name %>">
                        <a href="<%= Request.Path %>?lang=<%= ci.TwoLetterISOLanguageName %>"><%= ci.DisplayName %></a>
                    </li>
                    <% } %>
                </ul>
            </div>
        </div>
        <% } %>
        <ul class="personal-footer-links">
            <li><a href="<%= CommonLinkUtility.ToAbsolute("~/terms.aspx?lang=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) %>" target="_blank"><%=Resource.AuthTermsService %></a></li>
            <li><a href="<%= CommonLinkUtility.ToAbsolute("~/about.aspx?lang=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) %>"><%=Resource.AboutTitle %></a></li>
            <li><a href="https://www.onlyoffice.com" target="blank"><%=Resource.CorporateUse %></a></li>
        </ul>
        <div class="personal-footer_rights">© Ascensio System SIA <%= DateTime.UtcNow.Year.ToString() %>. <%= Resource.AllRightsReserved %></div>
    </div>
</div>