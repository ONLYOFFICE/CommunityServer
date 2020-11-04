<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceActionView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Invoices.InvoiceActionView" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>

<div id="crm_invoiceMakerDialog">
    <div class="invoice_info clearFix">
        
        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"><%= CRMInvoiceResource.NumberRequiredErrorMsg %></span>
            <div class="header-base-small  headerPanelSmall"><%= CRMInvoiceResource.InvoiceNumber %>:</div>
            <input type="text" id="invoiceNumber" class="textEdit" name="invoiceNumber" value="<%=InvoicesNumber%>"/>
            <% if(ActionType != InvoiceActionType.Edit && CRMSecurity.IsAdmin) { %>
            <span class="splitter"></span>
            <a id="changeFormatBtn" class="gray link dotline"><%= CRMInvoiceResource.ChangeFormat %></a>
            <% } %>
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"><%= CRMInvoiceResource.IssueDateRequiredErrorMsg %></span>
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.IssueDate %>:</div>
            <input type="text" id="invoiceIssueDate" class="textEdit textEditCalendar" name="invoiceIssueDate" autocomplete="off"/>
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"><%= CRMInvoiceResource.InvoiceClientReqiuredErrorMsg %></span>
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.InvoiceClient %>:</div>
            <div id="invoiceContactSelectorContainer"></div>
            <div id="invoiceContactInfoContainer" class="display-none">
                <span class="billing-address">
                    <a class="link plus dotline add-billing-address display-none"><%= CRMInvoiceResource.AddBillingAddress %></a>
                    <a class="link edit dotline edit-billing-address display-none"><%= CRMInvoiceResource.EditBillingAddress %></a>
                </span>
                <span class="splitter"></span>
                <span class="delivery-address">
                    <a class="link plus dotline add-delivery-address display-none"><%= CRMInvoiceResource.AddDeliveryAddress %></a>
                    <a class="link edit dotline edit-delivery-address display-none"><%= CRMInvoiceResource.EditDeliveryAddress %></a>
                </span>
            </div>
            <input type="hidden" id="invoiceContactID" name="invoiceContactID" />
        </div>

        <div class="headerPanelSmall-splitter" id="consigneeBox">
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.InvoiceConsignee %>:</div>
            <div class="header-bottom-splitter">
                <input type="checkbox" id="consigneeEqualClientCbx"/>
                <label for="consigneeEqualClientCbx"><%= CRMInvoiceResource.ConsigneeEqualClient %></label>
            </div>
            <div id="consigneeContainer" class="display-none">
                <div id="invoiceConsigneeSelectorContainer"></div>
                <div id="invoiceConsigneeInfoContainer" class="display-none">
                    <span class="delivery-address">
                        <a class="link plus dotline add-delivery-address display-none"><%= CRMInvoiceResource.AddDeliveryAddress %></a>
                        <a class="link edit dotline edit-delivery-address display-none"><%= CRMInvoiceResource.EditDeliveryAddress %></a>
                    </span>
                </div>
                <input type="hidden" id="invoiceConsigneeID" name="invoiceConsigneeID" />
            </div>
        </div>

        <div id="invoiceOpportunityBlock" class="headerPanelSmall-splitter" style="display:none;">
            <div class="header-base-small headerPanelSmall"><%= CRMDealResource.Deal %>:</div>
            <div id="invoiceOpportunitySelectorContainer">
                <div id="opportunitySelector" class="display-none">
                    <span id="linkOpportunityButton">
                        <a class="link plus dotline"><%= CRMInvoiceResource.LinkOpportunityButton%></a>
                        <span class="sort-down-black"></span>
                    </span>
                    <div id="linkOpportunityDialog" class="studio-action-panel">
                        <ul class="dropdown-content mobile-overflow"></ul>
                    </div>
                </div>
                <div id="opportunityContainer" class="display-none">
                    <span id="selectedOpportunity"></span>
                    <a class="crm-removeLink"></a>
                    <input type="hidden" id="invoiceOpportunityID" name="invoiceOpportunityID" />
                </div>
            </div>
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"></span>
            <div class="header-bottom-splitter">
                <span class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.DueDate %>:</span>
                <span class="HelpCenterSwitcher" id="duedatePresetHelpSwitcher"></span>
                <div class="popup_helper" id="duedatePresetHelpInfo"><%: CRMInvoiceResource.DueDateHelpInfoText %></div>
            </div>
            <input type="text" id="invoiceDueDate" class="textEdit textEditCalendar" name="invoiceDueDate" autocomplete="off"/>
            <span class="splitter"></span>
            <a id="duedate_0" class="link dotline duedate-link"><%= CRMInvoiceResource.DueDatePresetInfoText %></a>
            <span class="splitter"></span>
            <a id="duedate_15" class="link dotline duedate-link"><%= string.Format(CRMInvoiceResource.DaysCount, 15) %></a>
            <span class="splitter"></span>
            <a id="duedate_30" class="link dotline duedate-link"><%= string.Format(CRMInvoiceResource.DaysCount, 30) %></a>
            <span class="splitter"></span>
            <a id="duedate_45" class="link dotline duedate-link"><%= string.Format(CRMInvoiceResource.DaysCount, 45) %></a>
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"><%= CRMInvoiceResource.InvoiceLanguageRequiredErrorMsgs %></span>
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.InvoiceLanguage %>:</div>
            <select id="invoiceLanguage" class="comboBox" name="invoiceLanguage">
                <% foreach (var culture in SetupInfo.EnabledCultures)%>
                <% { %>
                <option value="<%=culture.Name%>"><%=culture.NativeName%></option>
                <% } %>
            </select>
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"><%= CRMInvoiceResource.CurrencyRequiredErrorMsg %></span>
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.Currency %>:</div>
            <select id="invoiceCurrency" class="comboBox" name="invoiceCurrency">
                <optgroup label="<%= CRMCommonResource.Currency_Basic %>">
                    <% foreach (var keyValuePair in CurrencyProvider.GetBasic())%>
                    <% { %>
                    <option value="<%=keyValuePair.Abbreviation%>">
                        <%=string.Format("{0} - {1}", keyValuePair.Abbreviation, keyValuePair.Title)%>
                    </option>
                    <% } %>
                </optgroup>
                <optgroup label="<%= CRMCommonResource.Currency_Other %>">
                    <% foreach (var keyValuePair in CurrencyProvider.GetOther())%>
                    <% { %>
                    <option value="<%=keyValuePair.Abbreviation%>">
                        <%=string.Format("{0} - {1}", keyValuePair.Abbreviation, keyValuePair.Title)%>
                    </option>
                    <% } %>
                </optgroup>
            </select>
        </div>

        <div class="headerPanelSmall-splitter requiredField display-none" id="exchangeRateContainer">
            <span class="requiredErrorText"><%= CRMInvoiceResource.ExchangeRateRequiredErrorMsg %></span>
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.ExchangeRate %>:</div>
            1&nbsp;<span id="celectedCurrency"></span>&nbsp;=&nbsp;
            <input type="text" id="invoiceExchangeRate" class="textEdit" name="invoiceExchangeRate"/>&nbsp;
            <span><%= Global.TenantSettings.DefaultCurrency.Abbreviation %></span>
            <% if (CRMSecurity.IsAdmin) { %>
            <span class="HelpCenterSwitcher" id="currencyHelpSwitcher"></span>
            <% } %>
            <span class="splitter-buttons"></span>
            <a id="exchangeRateSaveBtn" class="button gray middle"><%= CRMCommonResource.Save %></a>
            <span class="splitter-buttons"></span>
            <span id="exchangeRateSavedText" class="gray display-none"><%= CRMSettingResource.SaveCompleted %></span>
            <div class="popup_helper" id="currencyHelpInfo"><%= CRMInvoiceResource.InvoiceCurrencyHelpInfo %></div>
        </div>

        <div class="headerPanelSmall-splitter">
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.PONumber %>:</div>
            <input type="text" id="invoicePurchaseOrderNumber" class="textEdit" name="invoicePurchaseOrderNumber"/>
            <!--CRMInvoiceResource.AttachDocumentButton-->
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"><%= CRMInvoiceResource.TermsRequiredErrorMsg %></span>
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.Terms %>:</div>
            <textarea id="invoiceTerms" name="invoiceTerms"></textarea>
            <a id="setDefaultTermsBtn" class="gray link dotline"><%= CRMInvoiceResource.SetDefault %></a>
        </div>

        <div class="headerPanelSmall-splitter">
            <div class="header-bottom-splitter">
                <span class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.ClientNotes %>:</span>
                <span class="HelpCenterSwitcher" id="clientNotesHelpSwitcher"></span>
                <div class="popup_helper" id="clientNotesHelpInfo"><%: CRMInvoiceResource.ClientNotessHelpInfoText %></div>
            </div>
            <textarea id="invoiceDescription" name="invoiceDescription"></textarea>
        </div>

    </div>

    <div class="invoice-lines clearFix">
        <div class="requiredField headerPanelSmall">
            <span class="requiredErrorText"><%= CRMInvoiceResource.ProductsAndServicesRequiredErrorMsg %></span>
            <div id="invoiceLineTableHeader" class="header-base headerPanelSmall"><%= CRMInvoiceResource.ProductsAndServices%></div>
        </div>
        <div id="invoiceLineTableContainer"></div>
    </div>

    <div class="middle-button-container">
        <asp:LinkButton runat="server" ID="saveButton" CommandName="SaveInvoice" CommandArgument="0"
            OnCommand="SaveOrUpdateInvoice" CssClass="button blue big" />

        <% if (ActionType != InvoiceActionType.Edit) %>
        <% { %>
        <span class="splitter-buttons"></span>
        <asp:LinkButton runat="server" ID="saveAndCreateNewButton"  CommandName="SaveInvoice" CommandArgument="1"
            OnCommand="SaveOrUpdateInvoice" CssClass="button gray big" />
        <% } %>

        <span class="splitter-buttons"></span>
        <asp:HyperLink ID="cancelButton" runat="server" CssClass="button gray big cancelSbmtFormBtn"><%= CRMCommonResource.Cancel%></asp:HyperLink>

        <% if (ActionType == InvoiceActionType.Edit) %>
        <% { %>
        <span class="splitter-buttons"></span>
        <a id="deleteButton" class="button gray big"><%= CRMInvoiceResource.DeleteThisInvoice %></a>
        <% } %>
    </div>

    <div id="addressDialog" class="studio-action-panel">
         <div class="headerPanel-splitter" style="display: none;">
             <select class="address_type_select"></select>
             <input type="hidden" name="billingAddressID" value="0"/>
             <input type="hidden" name="deliveryAddressID" value="0"/>
        </div>
        <table class="address-tbl" cellpadding="0" cellspacing="0">
            <colgroup>
                <col style="width: 50%;"/>
                <col style="width: 50%;"/>
            </colgroup>
            <tbody>
                <tr>
                    <td class="cell select-cell">
                        <select class="address_category comboBox" disabled="disabled">
                            <option value="<%=(int)AddressCategory.Billing%>"><%=AddressCategory.Billing.ToLocalizedString()%></option>
                            <option value="<%=(int)AddressCategory.Postal%>"><%=AddressCategory.Postal.ToLocalizedString()%></option>
                        </select>
                    </td>
                    <td class="cell input-cell textarea-cell" rowspan="4">
                        <textarea class="contact_street" maxlength="255" placeholder="<%= CRMJSResource.AddressWatermark %>"></textarea>
                    </td>
                </tr>
                <tr>
                    <td class="cell input-cell">
                        <input type="text" class="contact_city textEdit" maxlength="255" placeholder="<%= CRMJSResource.CityWatermark %>"/>
                    </td>
                </tr>
                <tr>
                    <td class="cell input-cell">
                        <input type="text" class="contact_state textEdit" maxlength="255" placeholder="<%= CRMJSResource.StateWatermark %>"/>
                    </td>
                </tr>
                <tr>
                    <td class="cell input-cell">
                        <input type="text" class="contact_zip textEdit" maxlength="255" placeholder="<%= CRMJSResource.ZipCodeWatermark %>"/>
                    </td>
                </tr>
                <tr>
                    <td class="input-cell" colspan="2">
                        <input type="text" class="contact_country textEdit" maxlength="255" placeholder="<%= CRMJSResource.CountryWatermark %>"/>
                    </td>
                </tr>
            </tbody>
        </table>
        <div class="small-button-container">
            <a class="button blue middle"><%= CRMCommonResource.Save%></a>
            <span class="splitter-buttons"></span>
            <a class="button gray middle"><%= CRMCommonResource.Cancel%></a>
        </div>
    </div>
</div>