<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="import.aspx.cs" Inherits="ASC.Web.Projects.Import" %>

<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins"%>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    <div id="pageHeader">
        <div class="pageTitle"><%=ImportResource.ImportFromBasecamp%></div>
        <div style="clear: both"></div>
    </div>
    <div id="importTools">
        <div id="companyUrl" class="block-cnt-splitter field-container requiredField">
            <span class="requiredErrorText"></span>
            <div class="headerPanelSmall">
                <%= ImportResource.CompanyURL%>
            </div>
            <input type="text" id="tbxURL" class="textEdit" placeholder="<%=ImportResource.ExampleCompanyName%>"/>
        </div>        
        <div id="companyEmail" class="block-cnt-splitter field-container requiredField">
            <span class="requiredErrorText"></span>
            <div class="headerPanelSmall">
                <%= ImportResource.Email%>
            </div>
            <input type="text" ID="tbxUserName" class="textEdit"/>
        </div>
        <div id="companyPassword" class="block-cnt-splitter field-container requiredField">
            <span class="requiredErrorText"><%= ImportResource.EmptyPassword%></span>
            <div class="headerPanelSmall">
                <%= ImportResource.UserPassword%>
            </div>
            <input type="password" ID="tbxPassword" class="textEdit"/>
        </div>
        <div class="checkbox-container">
            <input type="checkbox" id="chooseProjects" autocomplete="off"/>
            <label for="chooseProjects"><%=ImportResource.ChooseBasecampProjects%></label>
        </div>
        <div class="checkbox-container">     
            <input type="checkbox" id="importClosed" value="false" autocomplete="off"/>
            <label for="importClosed"><%=ImportResource.ImportClosedTasks%></label>
        </div>
        <div class="checkbox-container"> 
            <input type="checkbox" id="sendInvitations" autocomplete="off"/>
            <label for="sendInvitations"><%=ImportResource.SendInvitations%></label>    
        </div>
        <div class="checkbox-container"> 
            <input type="checkbox" id="importAsCollaborators" <%if(QuotaEndFlag){ %> checked="checked" disabled="disabled"<%} %> />
            <label for="importAsCollaborators"><%= Resources.Resource.InviteUsersAsCollaborators%></label><div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'answerForHelpInviteGuests',position: 'fixed'});"></div>
            <div class="popup_helper" id="answerForHelpInviteGuests">
                <p><%=string.Format(Resources.Resource.NoteForInviteCollaborator, "<b>","</b>")%> 
                     <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
                       { %>
                    <a href="<%= CommonLinkUtility.GetHelpLink() %>" target="_blank"><%=Resources.Resource.LearnMore%></a>
                    <% } %>
                </p>
            </div> 
        </div>
        
        <div class="block-cnt-splitter big-margin-top">
            <div class="header-base pm-projectSettings-container red-text">
                <%=ImportResource.ImportAttantionPanelTitle%>
            </div>
            <div class="pm-projectSettings-container min-margin-top">
                <%=ImportResource.ImportAttantionPanelBody%>
            </div>
            <div class="checkbox-container middle-margin-top">
                <input type="checkbox" id="agreement" autocomplete="off"/>
                <label for="agreement">
                    <%=ImportResource.ImportAttantionPanelAgreement%>
                </label>
            </div>
        </div>
        
        <div class="middle-button-container">
            <a id="startImportButton" class="button blue big disable"><%=ImportResource.StartImport%></a>
        </div>
        <div id="viewDetailsImport" class="display-none">           
            <a class="link underline bold view-import" style="font-size: 11px !important; margin-left: 10px;">
                <%= ImportResource.ViewDetails%>
            </a>
        </div>
    </div>
    
    <div id="import_info_popup" style="display: none;">
        <sc:Container id="import_info_container" runat="server">
            <header>    
                <%= ImportResource.PopupPanelHeader%>
            </header>
            <body>
                <div class="header-base block-cnt-splitter">
                    <%= ImportResource.PleaseWait%>
                </div>
                <div class="block-cnt-splitter">
                    <table cellspacing="0" cellpadding="0">
                        <tr style="height: 30px;">
                            <td>
                                <b>
                                    <%= ImportResource.People%></b>
                            </td>
                            <td style="width: 100%;">
                                <div style="border-bottom: 1px dotted rgb(0, 0, 0);">
                                    &nbsp;</div>
                            </td>
                            <td id="importPeopleStatus"  class="importStatus">
                            </td>
                        </tr>
                    </table>
                    <table cellspacing="0" cellpadding="0">
                        <tr style="height: 30px;">
                            <td>
                                <b>
                                    <%= ImportResource.Projects%>
                                </b>
                            </td>
                            <td style="width: 100%;">
                                <div style="border-bottom: 1px dotted rgb(0, 0, 0);">
                                    &nbsp;</div>
                            </td>
                            <td id="importProjectsStatus" class="importStatus">
                            </td>
                        </tr>
                    </table>
                    <table cellspacing="0" cellpadding="0">
                        <tr style="height: 30px;">
                            <td>
                                <b>
                                    <%= ImportResource.Files%></b>
                            </td>
                            <td style="width: 100%;">
                                <div style="border-bottom: 1px dotted rgb(0, 0, 0);">
                                    &nbsp;</div>
                            </td>
                            <td id="importFilesStatus" class="importStatus">
                            </td>
                        </tr>
                    </table>
                    <div style="text-align: right; padding-top: 5px;">
                        <%= ImportResource.ImportCompleted%>: <span id="importProgress">0</span>
                        <%= ImportResource.OutOfThree%>
                    </div>
                </div>
                <div class="block-cnt-splitter">
                    <%=String.Format(ImportResource.PopupPanelBody,"<br/>")%>
                    <div class="import-statuses-container">
                        <div id="popupPanelBodyError" style="display: none;">
                        </div>
                    </div>
                </div>
                <div class="small-button-container">
                    <a class="button blue middle" href="javascript:void(0)" onclick="javascript: jq.unblockUI();">
                        <%= ImportResource.Close%>
                    </a>
                </div>
            </body>
        </sc:Container>
    </div>
    
    <div id="chooseProjectsPopup" style="display: none;">
        <sc:Container id="import_projects_container" runat="server">
            <header>    
                <%= ImportResource.PopupPanelHeader%>
            </header>
            <body>
                <p><%=ImportResource.ChooseBasecampProjectsNote%></p>
                <p class="active-proj-note"><input id="checkActiveProj" type="checkbox"/><label for="checkActiveProj"><%=ImportResource.ActiveProjects%></label></p>
                <ul id="activeProjects" class="basecamp-projects-container">
                    
                </ul>
                <p class="archived-proj-note"><input id="checkArchivedProj" type="checkbox"/><label for="checkArchivedProj"><%=ImportResource.ArchivedProjects%></label></p>
                <ul id="archivedProjects" class="basecamp-projects-container">
                    
                </ul>
                
                <div class="small-button-container">
                    <a id="importCheckedProjects" class="button blue middle" href="javascript:void(0)" onclick="javascript: jq.unblockUI();">
                        <%= ImportResource.Import%>
                    </a>
                </div>
            </body>
        </sc:Container>
    </div>
    
     <div id="popupImportErrorContainer" style="display: none;">
        <sc:Container id="import_popup_Error" runat="server">
            <header>    
                <p class="popup-header"><%= ImportResource.ImportFailed%></p>
            </header>
            <body>
                <p class="error-message"><%=ImportResource.ImportFailedMessage%></p>
               
                <div class="small-button-container">
                    <a class="button gray middle"  onclick="javascript: jq.unblockUI();">
                        <%= ProjectResource.OkButton%>
                    </a>
                </div>
            </body>
        </sc:Container>
    </div>
    
    <div id="popupUsersQuotaEnds" style="display: none;">
        <sc:Container id="users_quota_ends" runat="server">
            <header><%=Resources.Resource.ImportUserLimitTitle%></header>
            <body>
                <div class="tariff-limitexceed-users">
                    <div class="header-base-medium" data-zero-users-text="<%=Resources.Resource.ImportUserOverlimitHeader %>">
                        <%=String.Format(Resources.Resource.ImportUserLimitHeader, "<span id='userLimit'></span>")%>
                    </div>
                    <br/>
                    <br/>
                    <span id="limitReasonText" data-zero-users-text="<%=Resources.Resource.ImportUserOverlimitReason %>"><%=Resources.Resource.ImportUserLimitReason%></span>
                    <br/>
                    <br/>
                    <%=Resources.Resource.ImportUserLimitDecision%>
                </div>
                <div class="middle-button-container">
                    <a id="continueImport" class="blue button">
                        <%=Resources.Resource.ImportUserLimitOkButtons%>
                    </a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray" onclick="javascript: jq.unblockUI();">
                        <%= Resources.Resource.ImportContactsCancelButton %>
                    </a>
                </div>
            </body>
        </sc:Container>
    </div>

    <asp:HiddenField ID="HiddenFieldForPermission" runat="server" />
</asp:Content>

<asp:Content ID="projectsClientTemplatesResourcesPlaceHolder" ContentPlaceHolderID="projectsClientTemplatesResourcesPlaceHolder" runat="server">
</asp:Content>