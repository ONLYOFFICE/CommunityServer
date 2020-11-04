<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CasesActionView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Cases.CasesActionView" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div id="crm_caseMakerDialog">
    <div class="case_info clearFix">
        <input type="hidden" class="contact_ID" ID="CaseID" runat="server"/>

        <div class="requiredField headerPanelSmall-splitter" style="padding: 0 2px 0 0;">
            <span class="requiredErrorText"></span>
            <div class="bold headerPanelSmall"><%= CRMCasesResource.CaseTitle %>:</div>
            <input type="text" class="textEdit" maxlength="255" style="width:100%" id="caseTitle" name="caseTitle" value="<%= GetCaseTitle() %>"/>
        </div>

        <dl class="clearFix assignedCasesContacts">
            <dt class="header-base-small"><%= CRMCasesResource.MembersCase %></dt>
            <dd id="membersCasesSelectorsContainer"></dd>
            <dt style="margin:20px 0;" class="hiddenFields">
                <div id="casesContactListBox">
                    <table id="contactTable" class="table-list padding4" cellpadding="0" cellspacing="0">
                        <tbody>
                        </tbody>
                    </table>
                </div>
            </dt>
            <dd>
                <input type="hidden" name="memberID" value="" />
            </dd>
        </dl>

        <dl class="headerPanelSmall-splitter customFieldsContainer clearFix">
            <dt class="bold crm-headerHiddenToggledBlock"><%= CRMCommonResource.Tags %></dt>
            <dd id="tagsContainer">
                <div class="display-none">
                    <input type="hidden" name="baseInfo_assignedTags" />
                </div>
            </dd>
        </dl>

        <% if (CRMSecurity.IsAdmin) %>
        <% { %>
        <div id="otherCasesCustomFieldPanel">
            <div class="bold" style="margin-bottom: 10px;"><%= CRMSettingResource.OtherFields %></div>
            <a onclick="ASC.CRM.CasesActionView.showGotoAddSettingsPanel();" style="text-decoration: underline" class="linkMedium">
                <%= CRMSettingResource.SettingCustomFields %>
            </a>
        </div>
        <% } %>

        <% if (HavePermission) %>
        <% { %>
        <div class="casePrivatePanel">
            <asp:PlaceHolder ID="phPrivatePanel" runat="server"></asp:PlaceHolder>
        </div>
        <% } %>
        <input type="hidden" name="isPrivateCase" id="isPrivateCase"/>
        <input type="hidden" name="notifyPrivateUsers" id="notifyPrivateUsers"/>
        <input type="hidden" name="selectedUsersCase" id="selectedUsersCase"/>
    </div>

    <div class="middle-button-container">
        <asp:LinkButton runat="server" ID="saveCaseButton" CommandName="SaveCase" CommandArgument="0"
            OnClientClick="return ASC.CRM.CasesActionView.submitForm();" OnCommand="SaveOrUpdateCase" CssClass="button blue big"/>
            <span class="splitter-buttons"></span>

        <% if (TargetCase == null) %>
        <% { %>
        <asp:LinkButton runat="server" ID="saveAndCreateCaseButton"  CommandName="SaveCase" CommandArgument="1"
            OnClientClick="return ASC.CRM.CasesActionView.submitForm();" OnCommand="SaveOrUpdateCase" CssClass="button gray big" />
        <span class="splitter-buttons"></span>
        <% } %>

        <asp:HyperLink runat="server" CssClass="button gray big cancelSbmtFormBtn" ID="cancelButton"> <%= CRMCommonResource.Cancel%></asp:HyperLink>

        <% if (TargetCase != null) %>
        <% { %>
        <span class="splitter-buttons"></span>
        <a id="deleteCaseButton" class="button gray big" caseTitle="<%= TargetCase.Title.ReplaceSingleQuote().HtmlEncode() %>">
            <%= CRMCasesResource.DeleteThisCase %>
        </a>
        <% } %>
    </div>

    <div id="caseTypePage" class="display-none">       
            <%= TargetCase == null ? CRMCasesResource.CreateCaseProggress : CRMCommonResource.SaveChangesProggress%>
    </div>
</div>