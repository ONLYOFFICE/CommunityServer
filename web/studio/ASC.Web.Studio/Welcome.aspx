<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" MasterPageFile="~/Masters/basetemplate.master" CodeBehind="Welcome.aspx.cs" Inherits="ASC.Web.Studio.Welcome" %>

<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div id="chooseDefaultModule" class="choose-default-module <% = IsVideoPage() ? "display-none" : "" %>">
        <div class="choose-module-title"><%=String.Format(Resources.Resource.WizardChooseStartModule, "<br />", "<b>", "</b>", "<span class='gray-text'>","</span>")%></div>
        <div class="clearFix choose-module-block">
            <%if (showProjects)
              { %>
            <div class="item-module projects">
                <a class="default-module" href="javascript:void(0);" data-name="projects" data-url="<%= VirtualPathUtility.ToAbsolute("~/products/projects/") %>"></a>
                <div class="text-module"><%= Resources.Resource.WizardProjectsTitle %></div>
                <div class="forwhat-module"><%= Resources.Resource.WizardProjectsReason %></div>
            </div>
            <% } %>
            <div class="item-module documents">
                <a class="default-module" href="javascript:void(0);" data-name="documents" data-url="<%= VirtualPathUtility.ToAbsolute("~/products/files/") %>"></a>
                <div class="text-module"><%=Resources.Resource.WizardDocsTitle %></div>
                <div class="forwhat-module"><%=Resources.Resource.WizardDocsReason %></div>
            </div>
            <%if (showCRM)
              {%>
            <div class="item-module crm">
                <a class="default-module" href="javascript:void(0);" data-name="crm" data-url="<%= VirtualPathUtility.ToAbsolute("~/products/crm/") %>"></a>
                <div class="text-module"><%= Resources.Resource.WizardCrmTitle %></div>
                <div class="forwhat-module"><%= Resources.Resource.WizardCrmReason %></div>
            </div>
            <% } %>
        </div>

    </div>
    <div id="chooseVideoModule" class="choose-default-module display-none">
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

</asp:Content>
