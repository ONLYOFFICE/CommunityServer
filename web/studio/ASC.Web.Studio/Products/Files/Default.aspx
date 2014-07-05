<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Files._Default" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Import" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<asp:Content runat="server" ContentPlaceHolderID="BTSidePanel">
    <div class="page-menu">
        <asp:PlaceHolder ID="CommonSideHolder" runat="server"></asp:PlaceHolder>
    </div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>

    <%--Panels--%>

    <div id="settingCommon">
        <% if (Global.IsAdministrator && !CoreContext.Configuration.Personal && ImportConfiguration.SupportInclusion)%>
        <% { %>
        <span class="header-base"><%= FilesUCResource.ThirdPartyAccounts %></span>
        <br />
        <br />
        <label>
            <input type="checkbox" id="cbxEnableSettings" class="checkbox" <%= FilesSettings.EnableThirdParty ? "checked='checked'" : "" %> />
            <%= FilesUCResource.ThirdPartyEnableSettings %>
        </label>
        <br />
        <br />
        <br />
        <% } %>

        <span class="header-base"><%= FilesUCResource.SettingUpdateIfExist %></span>
        <br />
        <br />
        <label>
            <input type="checkbox" class="update-if-exist float-left checkbox" <%= FilesSettings.UpdateIfExist ? "checked=\"checked\"" : string.Empty %> />
            <%= string.Format(FilesUCResource.ConfirmUpdateIfExist, "<br/><span class=\"text-medium-describe\">", "</span>")%>
        </label>
        <br />
        <br />
        <label>
            <input type="checkbox" class="store-original checkbox" <%= FilesSettings.StoreOriginalFiles ? "checked=\"checked\"" : string.Empty %> />
            <%= FilesUCResource.ConfirmStoreOriginalUploadCbxLabelText %>
        </label>

        <% if (CoreContext.Configuration.Personal)
           { %>
        <br />
        <br />
        <br />
        <asp:PlaceHolder runat="server" ID="UserLanguageHolder"></asp:PlaceHolder>
        <% } %>
    </div>

    <div id="settingThirdPartyPanel">
        <asp:PlaceHolder runat="server" ID="SettingPanelHolder"></asp:PlaceHolder>
    </div>

    <div id="helpPanel"></div>

    <% if (AddCustomScript)
       { %>
    <!-- Google Code for Sign Up Conversion Page -->
    <script type="text/javascript">
        /* <![CDATA[ */
        var google_conversion_id = 1025072253;
        var google_conversion_language = "en";
        var google_conversion_format = "1";
        var google_conversion_color = "ffffff";
        var google_conversion_label = "N4STCIuI3wEQ_bjl6AM";
        var google_conversion_value = 1;
        var google_remarketing_only = false;
        /* ]]> */
    </script>
    <script type="text/javascript" src="//www.googleadservices.com/pagead/conversion.js"> 
    </script>
    <noscript>
        <div style="display: inline;">
            <img height="1" width="1" style="border-style: none;" alt="" src="//www.googleadservices.com/pagead/conversion/1025072253/?value=1&amp;label=N4STCIuI3wEQ_bjl6AM&amp;guid=ON&amp;script=0" />
        </div>
    </noscript>


    <% Page.RegisterInlineScript(@"
        try {
            if (window._gat) {
                _gaq.push(['_setAccount', 'UA-12442749-17']);
                _gaq.push(['_trackPageview', 'Account_Registered']);
            }
        } catch (err) {
        }

        try {
            if ((typeof window.yaCounter23426227 !== 'undefined') && (yaCounter23426227!=null)) {
                yaCounter23426227.reachGoal('Account_Registered'); 
            }
         } catch (e) { }
    "); %>
    <% } %>
</asp:Content>
