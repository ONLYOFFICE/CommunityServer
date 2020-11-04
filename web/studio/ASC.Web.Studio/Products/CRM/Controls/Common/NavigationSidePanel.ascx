<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavigationSidePanel.ascx.cs" Inherits="ASC.Web.CRM.Controls.Common.NavigationSidePanel" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Data.Storage" %>


<div class="page-menu">

    <ul class="menu-list">
        <li id="nav-menu-contacts" class="menu-item sub-list<% if (CurrentPage == "contacts")
                                                               { %> active currentCategory<% }
                                                               else if (CurrentPage == "companies" || CurrentPage == "persons")
                                                               { %> currentCategory<% } %>">
            <div class="category-wrapper">
                <span class="expander"></span>
                <a class="menu-item-label outer-text text-overflow" href=".#" title="<%= CRMContactResource.Contacts %>">
                    <span class="menu-item-icon group"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/crm-icons.svg#crmIconsgroup"></use></svg></span>
                    <span class="menu-item-label inner-text"><%= CRMContactResource.Contacts %></span>
                </a>
                <span id="feed-new-contacts-count" class="feed-new-count"></span>
            </div>
            <ul class="menu-sub-list">
                <li class="menu-sub-item<% if (CurrentPage == "companies")
                                           { %> active<% } %>">
                    <a class="menu-item-label outer-text text-overflow companies-menu-item" title="<%= CRMContactResource.Companies %>">
                        <span class="menu-item-label inner-text"><%= CRMContactResource.Companies %></span>
                    </a>
                </li>
                <li class="menu-sub-item<% if (CurrentPage == "persons")
                                           { %> active<% } %>">
                    <a class="menu-item-label outer-text text-overflow persons-menu-item" title="<%= CRMContactResource.Persons %>">
                        <span class="menu-item-label inner-text"><%= CRMContactResource.Persons %></span>
                    </a>
                </li>
            </ul>
        </li>
        <li id="nav-menu-tasks" class="menu-item none-sub-list<% if (CurrentPage == "tasks")
                                                                 { %> active<% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Tasks.aspx#" title="<%= CRMCommonResource.TaskModuleName %>">
                <span class="menu-item-icon tasks"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/crm-icons.svg#crmIconstasks"></use></svg></span>
                <span class="menu-item-label inner-text"><%= CRMCommonResource.TaskModuleName %></span>
            </a>
            <span id="feed-new-crmTasks-count" class="feed-new-count"></span>
        </li>
        <li id="nav-menu-deals" class="menu-item  none-sub-list<% if (CurrentPage == "deals")
                                                                  { %> active<% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Deals.aspx#" title="<%= CRMCommonResource.DealModuleName %>">
                <span class="menu-item-icon opportunities"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/crm-icons.svg#crmIconscase"></use></svg></span>
                <span class="menu-item-label inner-text"><%= CRMCommonResource.DealModuleName %></span>
            </a>
            <span id="feed-new-deals-count" class="feed-new-count"></span>
        </li>

        <li id="nav-menu-invoices" class="menu-item  sub-list<% if (CurrentPage == "invoices")
                                                                { %> active currentCategory<% } %>">
            <div class="category-wrapper">
                <span class="expander"></span>
                <a class="menu-item-label outer-text text-overflow" href="Invoices.aspx" title="<%= CRMCommonResource.InvoiceModuleName %>">
                    <span class="menu-item-icon documents"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/crm-icons.svg#crmIconsdocuments"></use></svg></span>
                    <span class="menu-item-label inner-text"><%= CRMCommonResource.InvoiceModuleName %></span>
                </a>
                <span id="feed-new-invoices-count" class="feed-new-count"></span>
            </div>
            <ul class="menu-sub-list">
                <li class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow drafts-menu-item" href="Invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJakVpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCRWNtRm1kQ0FnSUNBZ0lDSXNJbDlmYVdRaU9qRXdNelV3TTMwPSJ9" title="<%= CRMEnumResource.InvoiceStatus_Draft %>">
                        <span class="menu-item-label inner-text"><%= CRMEnumResource.InvoiceStatus_Draft %></span>
                    </a>
                </li>
                <li class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow sent-menu-item" href="Invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJaklpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCVFpXNTBJQ0FnSUNBZ0lpd2lYMTlwWkNJNk1UQXpOVEF6ZlE9PSJ9" title="<%= CRMEnumResource.InvoiceStatus_Sent %>">
                        <span class="menu-item-label inner-text"><%= CRMEnumResource.InvoiceStatus_Sent %></span>
                    </a>
                </li>
                <li class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow paid-menu-item" href="Invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJalFpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCUVlXbGtJQ0FnSUNBZ0lpd2lYMTlwWkNJNk1UQXpOVEF6ZlE9PSJ9" title="<%= CRMEnumResource.InvoiceStatus_Paid %>">
                        <span class="menu-item-label inner-text"><%= CRMEnumResource.InvoiceStatus_Paid %></span>
                    </a>
                </li>
                <li class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow rejected-menu-item" href="Invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJak1pTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCU1pXcGxZM1JsWkNBZ0lDQWdJQ0lzSWw5ZmFXUWlPakV3TXpVd00zMD0ifQ==" title="<%= CRMEnumResource.InvoiceStatus_Rejected %>">
                        <span class="menu-item-label inner-text"><%= CRMEnumResource.InvoiceStatus_Rejected %></span>
                    </a>
                </li>
            </ul>
        </li>

        <li id="nav-menu-cases" class="menu-item  none-sub-list<% if (CurrentPage == "cases")
                                                                  { %> active<% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Cases.aspx#" title="<%= CRMCommonResource.CasesModuleName %>">
                <span class="menu-item-icon cases"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/crm-icons.svg#crmIconscalendar"></use></svg></span>
                <span class="menu-item-label inner-text"><%= CRMCommonResource.CasesModuleName %></span>
            </a>
            <span id="feed-new-cases-count" class="feed-new-count"></span>
        </li>
        <% if ((CRMSecurity.IsAdmin || VoipNumberData.CanMakeOrReceiveCall) && VoipNumberData.Allowed) %>
        <% { %>
            <li id="nav-menu-voip-calls" class="menu-item  none-sub-list<% if (CurrentPage == "calls")
                                                                            { %> active<% } %>">
                <a class="menu-item-label outer-text text-overflow" href="Calls.aspx" title="<%= CRMCommonResource.VoIPCallsSettings %>">
                    <span class="menu-item-icon calls"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/crm-icons.svg#crmIconscalls"></use></svg></span>
                    <span class="menu-item-label inner-text"><%= CRMCommonResource.VoIPCallsSettings %></span>
                </a>
                <span id="feed-new-voip-calls-count" class="feed-new-count"></span>
            </li>
        <% } %>

        <% if (ASC.Web.CRM.Classes.Global.CanCreateReports)
           { %>
        <li id="nav-menu-reports" class="menu-item  none-sub-list <% if (CurrentPage == "reports") { %> active <% } %>">
            <a class="menu-item-label outer-text text-overflow" href="Reports.aspx" title="<%= CRMCommonResource.ReportsModuleName %>">
                <span class="menu-item-icon reports"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/crm-icons.svg#crmIconsstatistic"></use></svg></span>
                <span class="menu-item-label inner-text"><%= CRMCommonResource.ReportsModuleName %></span>
            </a>
        </li>
        <% } %> 

        <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>

        <% if (CRMSecurity.IsAdmin) %>
        <% { %>
            <li id="menuSettings" class="menu-item add-block sub-list<% if (CurrentPage.IndexOf("settings_", StringComparison.Ordinal) > -1)
                                                                        { %> currentCategory<% } %>">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow<% if (CurrentPage.IndexOf("settings_", StringComparison.Ordinal) == -1)
                                                                         { %> gray-text<% } %>" href="Settings.aspx" title="<%= CRMCommonResource.SettingModuleName %>">
                        <span class="menu-item-icon settings"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusettings"></use></svg></span>
                        <span class="menu-item-label inner-text"><%= CRMCommonResource.SettingModuleName %></span>
                    </a>
                </div>
                <ul class="menu-sub-list">
                    <li class="menu-sub-item<% if (CurrentPage == "settings_common")
                                               { %> active<% } %>">
                        <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=common" title="<%= CRMSettingResource.CommonSettings %>">
                            <span class="menu-item-label inner-text"><%= CRMSettingResource.CommonSettings %></span>
                        </a>
                    </li>

                    <li class="menu-sub-item<% if (CurrentPage == "settings_currency")
                                               { %> active<% } %>">
                        <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=currency" title="<%= CRMSettingResource.CurrencySettings %>">
                            <span class="menu-item-label inner-text"><%= CRMSettingResource.CurrencySettings %></span>
                        </a>
                    </li>

                    <li id="contactSettingsConteiner" class="menu-sub-item menu-item<% if (CurrentPage == "settings_contact_stage" || CurrentPage == "settings_contact_type")
                                                                                       { %> open<% } %>">
                        <div class="sub-list">
                            <span class="expander" id="contactSettingsExpander"></span>
                            <a href="Settings.aspx?type=contact_stage" class="menu-item-label outer-text text-overflow" id="menuMyProjects" title="<%= CRMCommonResource.ContactSettings %>"><%= CRMCommonResource.ContactSettings %></a>
                        </div>
                        <ul class="menu-sub-list">
                            <li class="menu-sub-item<% if (CurrentPage == "settings_contact_stage")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=contact_stage" title="<%= CRMContactResource.ContactStages %>">
                                    <span class="menu-item-label inner-text"><%= CRMContactResource.ContactStages %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_contact_type")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=contact_type" title="<%= CRMSettingResource.ContactTypes %>">
                                    <span class="menu-item-label inner-text"><%= CRMSettingResource.ContactTypes %></span>
                                </a>
                            </li>
                        </ul>
                    </li>

                    <li class="menu-sub-item menu-item<% if (CurrentPage == "settings_invoice_items" || CurrentPage == "settings_invoice_tax" || CurrentPage == "settings_organisation_profile")
                                                         { %> open<% } %>">
                        <div class="sub-list">
                            <span class="expander"></span>
                            <a href="Settings.aspx?type=invoice_items" class="menu-item-label outer-text text-overflow" title="<%= CRMCommonResource.InvoiceSettings %>"><%= CRMCommonResource.InvoiceSettings %></a>
                        </div>
                        <ul class="menu-sub-list">
                            <li class="menu-sub-item<% if (CurrentPage == "settings_invoice_items")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=invoice_items" title="<%= CRMCommonResource.ProductsAndServices %>">
                                    <span class="menu-item-label inner-text"><%= CRMCommonResource.ProductsAndServices %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_invoice_tax")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=invoice_tax" title="<%= CRMCommonResource.InvoiceTaxes %>">
                                    <span class="menu-item-label inner-text"><%= CRMCommonResource.InvoiceTaxes %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_organisation_profile")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=organisation_profile" title="<%= CRMCommonResource.OrganisationProfile %>">
                                    <span class="menu-item-label inner-text"><%= CRMCommonResource.OrganisationProfile %></span>
                                </a>
                            </li>

                        </ul>
                    </li>

                    <% if (VoipNumberData.Allowed)
                       { %>

                        <li class="menu-sub-item menu-item<% if (CurrentPage == "settings_voip.common" || CurrentPage == "settings_voip.numbers")
                                                             { %> open<% } %>">
                            <div class="sub-list">
                                <span class="expander"></span>
                                <a href="Settings.aspx?type=voip.numbers" class="menu-item-label outer-text text-overflow" title="<%= CRMCommonResource.VoIPSettings %>"><%= CRMCommonResource.VoIPSettings %></a>
                            </div>
                            <ul class="menu-sub-list">
                                <li class="menu-sub-item<% if (CurrentPage == "settings_voip.common")
                                                           { %> active<% } %>">
                                    <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=voip.common" title="<%= CRMCommonResource.VoIPCommonSettings %>">
                                        <span class="menu-item-label inner-text"><%= CRMCommonResource.VoIPCommonSettings %></span>
                                    </a>
                                </li>
                                <li class="menu-sub-item<% if (CurrentPage == "settings_voip.numbers")
                                                           { %> active<% } %>">
                                    <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=voip.numbers" title="<%= CRMCommonResource.VoIPNumbersSettings %>">
                                        <span class="menu-item-label inner-text"><%= CRMCommonResource.VoIPNumbersSettings %></span>
                                    </a>
                                </li>
                            </ul>
                        </li>
                
                    <% } %>

                    <li class="menu-sub-item menu-item<% if (CurrentPage == "settings_custom_field" || CurrentPage == "settings_history_category" || CurrentPage == "settings_task_category" || CurrentPage == "settings_deal_milestone" || CurrentPage == "settings_tag")
                                                         { %> open<% } %>">
                        <div class="sub-list">
                            <span class="expander"></span>
                            <a href="Settings.aspx?type=custom_field" class="menu-item-label outer-text text-overflow" title="<%= CRMCommonResource.OtherSettings %>"><%= CRMCommonResource.OtherSettings %></a>
                        </div>
                        <ul class="menu-sub-list">
                            <li class="menu-sub-item<% if (CurrentPage == "settings_custom_field")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=custom_field" title="<%= CRMSettingResource.CustomFields %>">
                                    <span class="menu-item-label inner-text"><%= CRMSettingResource.CustomFields %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_history_category")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=history_category" title="<%= CRMSettingResource.HistoryCategories %>">
                                    <span class="menu-item-label inner-text"><%= CRMSettingResource.HistoryCategories %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_task_category")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=task_category" title="<%= CRMTaskResource.TaskCategories %>">
                                    <span class="menu-item-label inner-text"><%= CRMTaskResource.TaskCategories %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_deal_milestone")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=deal_milestone" title="<%= CRMDealResource.DealMilestone %>">
                                    <span class="menu-item-label inner-text"><%= CRMDealResource.DealMilestone %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_tag")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=tag" title="<%= CRMCommonResource.Tags %>">
                                    <span class="menu-item-label inner-text"><%= CRMCommonResource.Tags %></span>
                                </a>
                            </li>

                        </ul>
                    </li>

                    <li id="menuCreateWebsite" class="menu-sub-item<% if (CurrentPage == "settings_web_to_lead_form")
                                                                      { %> active<% } %>">
                        <a class="menu-item-label outer-text text-overflow" href="Settings.aspx?type=web_to_lead_form" title="<%= CRMSettingResource.WebToLeadsForm %>">
                            <span class="menu-item-label inner-text"><%= CRMSettingResource.WebToLeadsForm %></span>
                        </a>
                    </li>

                    <% if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) { %>
                    <li id="menuAccessRights" class="menu-sub-item">
                        <a class="menu-item-label outer-text text-overflow" href="<%= CommonLinkUtility.GetAdministration(ManagementType.AccessRights) + "#crm" %>" title="<%= CRMSettingResource.AccessRightsSettings %>">
                            <span class="menu-item-label inner-text"><%= CRMSettingResource.AccessRightsSettings %></span>
                        </a>
                    </li>
                    <% } %>   

                </ul>
            </li>
        <% } %> 
        <asp:PlaceHolder ID="HelpHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="UserForumHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
    </ul>

</div>