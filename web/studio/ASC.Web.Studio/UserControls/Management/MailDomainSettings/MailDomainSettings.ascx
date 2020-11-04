<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MailDomainSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.MailDomainSettings" %>

<%@ Import Namespace="ASC.Core.Tenants" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
    <div id="studio_domainSettings" class="settings-block">
        <div class="header-base clearFix">
            <%= Resource.StudioDomainSettings %>
        </div>
        <div class="clearFix">
            <div class="clearFix">
                <input id="offMailDomains" type="radio" value="<%=(int)TenantTrustedDomainsType.None %>" name="signInType" <%=_currentTenant.TrustedDomainsType == TenantTrustedDomainsType.None?"checked=\"checked\"":""%> <%= _tenantAccessAnyone ? "disabled=\"disabled\"" : "" %> />
                <label for="offMailDomains"><%=Resource.OffMailDomains%></label>
            </div>
            <div class="clearFix">
                <input id="allMailDomains" type="radio" value="<%=(int)TenantTrustedDomainsType.All %>" name="signInType" <%=_currentTenant.TrustedDomainsType == TenantTrustedDomainsType.All?"checked=\"checked\"":""%> <%= _tenantAccessAnyone ? "disabled=\"disabled\"" : "" %> />
                <label for="allMailDomains"><%=Resource.AllMailDomains%></label>
            </div>
            <div class="clearFix">
                <input id="trustedMailDomains" type="radio" value="<%= (int)TenantTrustedDomainsType.Custom %>" name="signInType" <%=_currentTenant.TrustedDomainsType == TenantTrustedDomainsType.Custom?"checked=\"checked\"":""%> <%= _tenantAccessAnyone ? "disabled=\"disabled\"" : "" %> />
                <label for="trustedMailDomains"><%= Resource.TrustedDomainSignInTitle %></label>
            </div>
        </div>
        <div id="trustedMailDomainsDescription" class="description" <%= _currentTenant.TrustedDomainsType == TenantTrustedDomainsType.Custom ? "" : "style=\"display:none;\"" %>>
            <div class="clearFix" id="studio_domainListBox">
                <%for (var i = 0; i < _currentTenant.TrustedDomains.Count; i++)
                  {
                      var domain = _currentTenant.TrustedDomains[i];%>
                <div name="<%=i%>" id="studio_domain_box_<%=i%>" class="domainSettingsBlock clearFix">
                    <input id="studio_domain_<%=i%>" type="text" maxlength="60" class="textEdit" value="<%=HttpUtility.HtmlEncode(domain)%>" />
                    <a class="removeDomain" onclick="MailDomainSettingsManager.RemoveTrustedDomain('<%=i%>');" href="javascript:void(0);">
                        <img align="absmiddle" border="0" alt="<%= Resource.DeleteButton %>" src="<%= WebImageSupplier.GetAbsoluteWebPath("trash_16.png") %>" />
                    </a>
                </div>
                <%}%>
            </div>
            <a href="javascript:void(0);" id="addTrustDomainBtn" class="link dotline plus"><%= Resource.AddTrustedDomainButton %></a>
        </div>
        <div id="allMailDomainsDescription" class="description" <%=_currentTenant.TrustedDomainsType == TenantTrustedDomainsType.All?"":"style=\"display:none;\""%>>
        </div>
        <div id="offMailDomainsDescription" class="description" <%=_currentTenant.TrustedDomainsType == TenantTrustedDomainsType.None?"":"style=\"display:none;\""%>>
        </div>
        <div id="domainSettingsCbxContainer" class="clearFix" <%=_currentTenant.TrustedDomainsType == TenantTrustedDomainsType.None?"style=\"display:none;\"":""%>>
            <input type="checkbox" id="cbxInviteUsersAsVisitors" <%= _studioTrustedDomainSettings.InviteUsersAsVisitors ? "checked=\"checked\"" : "" %> <%= _enableInviteUsers && !_tenantAccessAnyone ? "" : "disabled=\"disabled\"" %>>
            <label for="cbxInviteUsersAsVisitors"><%= CustomNamingPeople.Substitute<Resource>("InviteUsersAsCollaborators").HtmlEncode() %></label>
        </div>
        <div class="middle-button-container">
            <a class="button blue <%= _tenantAccessAnyone ? "disable" : "" %>" id="saveMailDomainSettingsBtn">
                <%= Resource.SaveButton %>
            </a>
        </div>
    </div>
    <div class="settings-help-block">
        <p><%= String.Format(Resource.HelpAnswerMailDomainSettings.HtmlEncode(), "<br />","<b>","</b>")%></p>
        <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#ChangingSecuritySettings_block" %>" target="_blank"><%= Resource.LearnMore%></a>
        <% } %>
    </div>
</div>
