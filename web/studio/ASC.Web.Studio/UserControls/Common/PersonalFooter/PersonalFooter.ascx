<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PersonalFooter.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.PersonalFooter.PersonalFooter" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div class="personal-footer">
    <div class="personal-footer_w clearFix">
        <ul class="ListSocLink">
            <li><a class="faceBook" href="https://www.facebook.com/pages/OnlyOffice/833032526736775" target="_blank">
                <label title="Facebook" class="social_grey_fb"></label>
            </a></li>
            <li><a class="twitter" href="https://twitter.com/ONLY_OFFICE" target="_blank" rel="nofollow">
                <label title="Twitter" class="social_grey_twi"></label>
            </a></li>
            <li><a class="linkedin" href="https://www.linkedin.com/groups/6726380/" target="_blank" rel="nofollow">
                <label title="LinkedIn" class="social_grey_in"></label>
            </a></li>
            <li><a class="youtube" href="https://www.youtube.com/user/onlyofficeTV" target="_blank" rel="nofollow">
                <label title="YouTube" class="social_grey_tube"></label>
            </a></li>
            <li><a class="blog" href="https://www.onlyoffice.com/blog" target="_blank">
                <label title="<%= PersonalResource.AuthFooterBlog %>" class="social_grey_blog"></label>
            </a></li>
            <li><a class="medium" href="https://medium.com/onlyoffice" target="_blank">
                <label title="Medium" class="social_grey_medium"></label>
            </a></li>
            <li><a class="instagram" href="https://www.instagram.com/the_onlyoffice/" target="_blank">
                <label title="Instagram" class="social_grey_instagram"></label>
            </a></li>
            <li><a class="github" href="https://github.com/ONLYOFFICE/" target="_blank">
                <label title="GitHub" class="social_grey_github"></label>
            </a></li>
            <li><a class="fosstodon" href="https://fosstodon.org/@ONLYOFFICE" target="_blank">
                <label title="Fosstodon" class="social_grey_fosstodon"></label>
            </a></li>
        </ul>
        <div class="partial-personal-footer">
            <ul class="personal-footer-links">
                <li><a href="<%= CommonLinkUtility.ToAbsolute("~/Terms.aspx?lang=" + CultureInfo.CurrentUICulture.Name) %>" target="_blank"><%= Resource.AuthTermsService %></a></li>
                <li><a href="https://www.onlyoffice.com" target="blank"><%= Resource.CorporateUse %></a></li>
            </ul>
            <div class="personal-footer_rights">
                © Ascensio System SIA <%= DateTime.UtcNow.Year.ToString() %>. <%= Resource.AllRightsReserved %>
            </div>
        </div>
    </div>
</div>