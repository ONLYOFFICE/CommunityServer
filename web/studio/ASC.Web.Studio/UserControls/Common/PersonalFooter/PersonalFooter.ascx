<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PersonalFooter.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.PersonalFooter.PersonalFooter" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="personal-footer">
    <div class="personal-footer_w clearFix">
        <ul class="personal-footer-links">
            <li><a href="<%= CommonLinkUtility.ToAbsolute("~/Terms.aspx?lang=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) %>" target="_blank"><%= Resource.AuthTermsService %></a></li>
            <li><a href="https://www.onlyoffice.com" target="blank"><%= Resource.CorporateUse %></a></li>
        </ul>
        <div class="personal-footer_rights">
            © Ascensio System SIA <%= DateTime.UtcNow.Year.ToString() %>. <%= Resource.AllRightsReserved %>
            <a href="https://www.instagram.com/the_onlyoffice/" class="personal-footer_inst"></a>
        </div>
    </div>
</div>