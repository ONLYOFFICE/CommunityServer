<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" MasterPageFile="~/Masters/basetemplate.master" CodeBehind="Welcome.aspx.cs" Inherits="ASC.Web.Studio.Welcome" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div id="chooseDefaultModule" class="choose-default-module">
        <div class="choose-module-title"><%=String.Format(Resources.Resource.WizardChooseStartModule, "<br />", "<b>", "</b>", "<span class='gray-text'>","</span>")%></div>
        <div class="clearFix choose-module-block">
            <%if (showProjects)
              { %>
            <div class="item-module projects">
                <a class="default-module" href="<%= VirtualPathUtility.ToAbsolute("~/products/projects/") %>" data-name="projects" data-url="<%= VirtualPathUtility.ToAbsolute("~/products/projects/") %>"></a>
                <div class="text-module"><%= Resources.Resource.WizardProjectsTitle %></div>
                <div class="forwhat-module"><%= Resources.Resource.WizardProjectsReason %></div>
            </div>
            <% } %>
            <div class="item-module documents">
                <a class="default-module" href="<%= VirtualPathUtility.ToAbsolute("~/products/files/") %>" data-name="documents" data-url="<%= VirtualPathUtility.ToAbsolute("~/products/files/") %>"></a>
                <div class="text-module"><%=Resources.Resource.WizardDocsTitle %></div>
                <div class="forwhat-module"><%=Resources.Resource.WizardDocsReason %></div>
            </div>
            <%if (showCRM)
              {%>
            <div class="item-module crm">
                <a class="default-module" href="<%= VirtualPathUtility.ToAbsolute("~/products/crm/") %>" data-name="crm" data-url="<%= VirtualPathUtility.ToAbsolute("~/products/crm/") %>"></a>
                <div class="text-module"><%= Resources.Resource.WizardCrmTitle %></div>
                <div class="forwhat-module"><%= Resources.Resource.WizardCrmReason %></div>
            </div>
            <% } %>
        </div>

    </div>
<%--    <div id="chooseVideoModule" class="choose-default-module display-none">
        <div>
            <div class="choose-module-title"><%=videoTitle%></div>
        </div>
        <div class="choose-module-video">
            <iframe width="640" height="385" frameborder="0" allowfullscreen></iframe>
        </div>
        <div class="related-video">
            <span><%=Resources.Resource.WizardSetUpARM%></span>
            <span id="relatedVideo"><%=Resources.Resource.WizardRelatedVideo%></span>
        </div>
        <a id="continueVideoModule" class="button blue big disable" href="javascript:void(0);" data-value="<% =buttonContinue %>"><%=buttonWait%></a>
    </div>
    <span id="curCulture" class="display-none"><% = culture %></span>
    <span id="listScriptStyle" class="display-none" data-docs="<%=docsScript %>" data-common="<%= CommonLinkUtility.GetFullAbsolutePath("~/startscriptsstyles.aspx") %>"></span>
    --%>
    <% if (!CoreContext.Configuration.Standalone && !CoreContext.Configuration.Personal && SetupInfo.CustomScripts.Length != 0)
       { %>
    <!-- Google Code for Tag for created portal -->
    <!-- Remarketing tags may not be associated with personally identifiable information or placed on pages related to sensitive categories. For instructions on adding this tag and more information on the above requirements, read the setup guide: google.com/ads/remarketingsetup -->
    <script type="text/javascript">
        /* <![CDATA[ */
        var google_conversion_id = 1025072253;
        var google_conversion_label = "l8P4CPOiswgQ_bjl6AM";
        var google_custom_params = window.google_tag_params;
        var google_remarketing_only = true;
        /* ]]> */
    </script>
    <script type="text/javascript" src="//www.googleadservices.com/pagead/conversion.js">
    </script>
    <noscript>
        <div style="display: inline;">
            <img height="1" width="1" style="border-style: none;" alt="" src="//googleads.g.doubleclick.net/pagead/viewthroughconversion/1025072253/?value=0&label=l8P4CPOiswgQ_bjl6AM&guid=ON&script=0" />
        </div>
    </noscript>

<!-- Google Code for PortalReg_042014 Conversion Page -->
<script type="text/javascript">
/* <![CDATA[ */
var google_conversion_id = 1025072253;
var google_conversion_language = "en";
var google_conversion_format = "2";
var google_conversion_color = "ffffff";
var google_conversion_label = "VaHvCPOCkQkQ_bjl6AM";
var google_remarketing_only = false;
/* ]]> */
</script>
<script type="text/javascript" src="//www.googleadservices.com/pagead/conversion.js">
</script>
<noscript>
<div style="display:inline;">
<img height="1" width="1" style="border-style:none;" alt="" src="//www.googleadservices.com/pagead/conversion/1025072253/?label=VaHvCPOCkQkQ_bjl6AM&guid=ON&script=0"/>
</div>
</noscript>

    <% } %>

</asp:Content>
