<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DefaultPageSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.DefaultPageSettings" %>
<%@ Import Namespace="Resources" %>

<div class="clearFix">
  <div id="studio_defaultPageSettings" class="settings-block">
    <div class="header-base clearFix">
        <div class="title">
            <%= Resource.DefaultPageSettingsTitle %>
        </div>
    </div>
        <div class="clearFix">
            <% foreach (var defaultPage in DefaultPages) %>
            <% { %>
            <div class="clearFix">
                <input id="chk_studio_default_<%= defaultPage.ProductName %>"
                    value="<%= defaultPage.ProductID%>" type="radio" name="defaultPage"
                    <%=(defaultPage.IsSelected?"checked=\"checked\"":"")%>/>
                <label for="chk_studio_default_<%= defaultPage.ProductName %>">
                    <%= defaultPage.DisplayName %>
                </label>
            </div>
            <% } %>
        </div>
        <div class="middle-button-container">
            <a class="button blue" onclick="DefaultPage.SaveSettings(); return false;" href="javascript:void(0);">
                <%= Resource.SaveButton %>
            </a>
        </div>

  </div>
  <div class="settings-help-block">
        <p>
            <%= String.Format(Resource.HelpAnswerDefaultPageSettings.HtmlEncode(), "<b>", "</b>") %>
        </p>
      <% if (!string.IsNullOrEmpty(HelpLink))
           { %>
        <a href="<%= HelpLink + "/gettingstarted/configuration.aspx#ManagingPortalModules_block" %>" target="_blank">
            <%= Resource.LearnMore%>
        </a>
       <% } %>
 </div>  
</div>