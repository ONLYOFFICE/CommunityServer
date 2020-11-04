<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceItemActionView.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.InvoiceItemActionView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div id="crm_invoiceItemMakerDialog">
    <div class="item_info clearFix">
        <div class="headerPanelSmall-splitter">
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.StockKeepingUnit %>:</div>
            <input type="text" class="textEdit invoiceItemSKU"  maxlength="255" value="" /> 
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"><%= CRMSettingResource.EmptyTitleError %></span>
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.FormInvoiceItemName %>:</div>
            <input type="text" class="textEdit invoiceItemTitle" maxlength="255" value=""/>
        </div>

        <div class="headerPanelSmall-splitter">
            <div class="header-base-small headerPanelSmall"><%= CRMSettingResource.Description %>:</div>
            <textarea rows="4" class="invoiceItemDescr"></textarea>
        </div>

        <div class="headerPanelSmall-splitter requiredField">
            <span class="requiredErrorText"></span>
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.FormInvoiceItemPrice %>:</div>
            <input type="text" class="textEdit invoiceItemPrice"  maxlength="11" value="" />
            <span class="splitter"></span>
            <span class="invoiceItemCurrency"></span>
            <div class="HelpCenterSwitcher" id="itemCurrencyHelpSwitcher"></div>
            <div class="popup_helper" id="itemCurrencyHelpInfo">
                <%= CRMInvoiceResource.ItemCurrencyHelpInfo %>
            </div>
        </div>

        <div class="headerPanelSmall-splitter">
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.FormInvoiceItemTaxes %>:</div>
            <table cellspacing="0" cellpadding="0" class="invoiceItemTaxesTable">
                <colgroup>
                    <col style="width: 50%" />
                    <col style="width: 50%" />
                </colgroup>
                <tbody>
                <tr>
                    <td style="white-space:nowrap;padding-right: 4px;">
                        <select id="tax1Select"></select>
                    </td>
                    <td style="white-space:nowrap; padding-left: 4px;">
                        <select id="tax2Select"></select>
                    </td>
                </tr>
                </tbody>
            </table>
        </div>

        <div class="headerPanelSmall-splitter">
            <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.FormInvoiceItemInventoryStock %>:
                <div class="HelpCenterSwitcher" id="iventoryStockHelpSwitcher"></div>
                <div class="popup_helper" id="iventoryStockHelpInfo">
                    <%= CRMInvoiceResource.IventoryStockHelpInfo %>
                </div>
            </div>
            <input type="checkbox" id="invoiceItemInventoryStock" />
            <label for="invoiceItemInventoryStock"><%= CRMInvoiceResource.TrackInventory %></label>
            <div class="currentQuantity display-none">
                <span class="requiredErrorText"><%= CRMInvoiceResource.ErrorIncorrectQuantity %></span>
                <div class="header-base-small headerPanelSmall"><%= CRMInvoiceResource.FormInvoiceItemStockQuantity %>:</div>
                <input type="text" class="textEdit invoiceItemStockQuantity"  maxlength="10" value="" /> 
            </div>
        </div>
    </div>

    <div class="middle-button-container">
        <a id="saveItemButton" class="button blue big"><%= CRMCommonResource.Save %></a>
        <span class="splitter-buttons"></span>

        <% if (TargetInvoiceItem == null) %>
        <% { %>
        <a id="saveAndCreateItemButton" class="button gray big"><%= CRMInvoiceResource.AddThisAndCreateInvoiceItemButton %></a>
        <span class="splitter-buttons"></span>
        <% } %>
        <a id="cancelButton" class="button gray big" href="Settings.aspx?type=invoice_items"> <%= CRMCommonResource.Cancel%></a>
    </div>
</div>