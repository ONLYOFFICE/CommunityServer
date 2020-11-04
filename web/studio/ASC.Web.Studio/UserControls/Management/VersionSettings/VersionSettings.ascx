<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VersionSettings.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Management.VersionSettings.VersionSettings" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
  <div id="studio_versionSetting" class="settings-block ">    
    <div class="header-base clearFix" id="versionSettingsTitle">
            <%= Resource.StudioVersionSettings %>
    </div>
    <div class="clearFix" >
        <div class="clearFix versionSettingBox">
            <div class="clearFix">
                <div class="versionSettingName" id="versionSelector">
                    <% foreach (var tenantVersion in CoreContext.TenantManager.GetTenantVersions())
                       {%>
                    <div class="clearFix">
                        <input type="radio" name="version" id="radio<%=tenantVersion.Id%>" value="<%=tenantVersion.Id%>"
                            <%= CoreContext.TenantManager.GetCurrentTenant(false).Version==tenantVersion.Id?"checked=\"checked\"":"" %>
                             />
                        <%if (CoreContext.TenantManager.GetCurrentTenant(false).Version == tenantVersion.Id)
                          {%>
                        <label for="radio<%= tenantVersion.Id %>">
                            <strong>
                                <%= GetLocalizedName(tenantVersion.Name) %>
                            </strong>
                        </label>
                        <% }
                          else
                          {%>
                        <label for="radio<%= tenantVersion.Id %>">
                            <%= GetLocalizedName(tenantVersion.Name)%>
                        </label>
                        <%} %>
                    </div>
                    <%} %>
                </div>
            </div>
            <div class="middle-button-container">
                <a class="button blue" onclick="StudioVersionManagement.SwitchVersion();"
                    href="javascript:void(0);">
                    <%= Resource.SaveButton %></a>
            </div>
        </div>
    </div>
 </div>
 <div class="settings-help-block">
        <p><%=String.Format(Resource.HelpAnswerPortalVersion.HtmlEncode(), "<br /> ", " <b>", "</b>")%></p>
     <% if (!string.IsNullOrEmpty(HelpLink))
             { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#CustomizingPortal_block" %>" target="_blank"><%= Resource.LearnMore %></a>
     <% } %>
 </div>  
</div>