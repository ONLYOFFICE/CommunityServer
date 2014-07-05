<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GreetingSettingsContent.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.GreetingSettingsContent" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
       
<div class="clearFix">
    <div class="header-base-small greetingContentTitle">
        <%=Resources.Resource.GreetingTitle%>:
    </div>
    <div>
        <input type="text" class="textEdit" maxlength="150" id="studio_greetingHeader" value="<%=HttpUtility.HtmlEncode(ASC.Core.CoreContext.TenantManager.GetCurrentTenant().Name)%>" />
    </div>
</div>
<div class="greetingContentLogo clearFix">
    <div class="header-base-small greetingContentLogoText">
        <%=Resources.Resource.GreetingLogo%>:
    </div>
    <div >
        <div class="clearFix">
            <div class="greetingContentLogoImg">
                <img id="studio_greetingLogo" class="borderBase" src="<%=_tenantInfoSettings.GetAbsoluteCompanyLogoPath()%>" />
            </div>
            <%if (!MobileDetector.IsMobile)
              { %>
            <div class="greetingContentChangeLogo">
                <input type="hidden" id="studio_greetingLogoPath" value="" />
                <a id="studio_logoUploader" class="link dotline" href="javascript:void(0);">
                    <%=Resources.Resource.ChangeLogoButton%></a>
            </div>
            <% } %>
        </div>
    </div>
</div>
