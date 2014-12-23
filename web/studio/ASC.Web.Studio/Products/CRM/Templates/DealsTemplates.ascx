<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<%--Opportunities List--%>
<script id="dealsListBaseTmpl" type="text/x-jquery-tmpl">
    <div id="dealFilterContainer">
        <div id="dealsAdvansedFilter"></div>
    </div>

    <div id="dealList" class="clearFix" style="display:none; min-height: 400px;">

        <table id="tableForDealNavigation" class="crm-navigationPanel" cellpadding="4" cellspacing="0" border="0">
            <tbody>
            <tr>
                <td>
                    <div id="divForDealPager">
                    </div>
                </td>
                <td style="text-align:right;">
                    <a style="margin-right: 25px;" class="baseLinkAction showTotalAmount"
                        onclick="ASC.CRM.ListDealView.showExchangeRatePopUp();" href="javascript:void(0)">
                            <%=CRMDealResource.ShowTotalAmount %>
                    </a>
                    <span class="gray-text"><%= CRMDealResource.TotalDeals %>:</span>
                    <span class="gray-text" id="totalDealsOnPage"></span>

                    <span class="gray-text"><%= CRMCommonResource.ShowOnPage %>:&nbsp;</span>
                    <select class="top-align">
                        <option value="25">25</option>
                        <option value="50">50</option>
                        <option value="75">75</option>
                        <option value="100">100</option>
                    </select>
                </td>
            </tr>
            </tbody>
        </table>
    </div>

    <div id="files_hintStagesPanel" class="hintDescriptionPanel">
        <%=CRMDealResource.TooltipStages%>
        <a href="http://www.onlyoffice.com/help/tipstricks/opportunity-stages.aspx" target="_blank"><%=CRMCommonResource.ButtonLearnMore%></a>
    </div>

    <div id="hiddenBlockForContactSelector" style="display:none;">
        <span id="contactSelectorForFilter" class="custom-value">
            <span class="inner-text">
                <span class="value"><%= CRMCommonResource.Select %></span>
            </span>
        </span>
    </div>

    <div id="addTagDealsDialog" class="studio-action-panel addTagDialog">
        <ul class="dropdown-content mobile-overflow"></ul>
        <div class="h_line">&nbsp;</div>
        <div style="padding: 0 12px;">
            <div style="margin-bottom: 5px;"><%= CRMCommonResource.CreateNewTag%>:</div>
            <input type="text" maxlength="50" class="textEdit" />
            <a onclick="ASC.CRM.ListDealView.addNewTag();" class="button blue" id="addThisTag">
                <%= CRMCommonResource.OK%>
            </a>
        </div>
    </div>

    <div id="permissionsDealsPanelInnerHtml" class="display-none">
        {{if IsCRMAdmin !== true}}
        <div style="margin-top:10px">
            <b><%= CRMDealResource.DealAccessRightsLimit %></b>
        </div>
        {{/if}}
    </div>

    <div id="dealActionMenu" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a class="showProfileLink dropdown-item"><%= CRMDealResource.ShowDealProfile %></a></li>
            <li><a class="setPermissionsLink dropdown-item"><%= CRMCommonResource.SetPermissions %></a></li>
            {{if CanCreateProjects}}
            <li><a class="createProject dropdown-item" target="_blank"><%= CRMCommonResource.CreateNewProject %></a></li>
            {{/if}}
            <li><a class="editDealLink dropdown-item"><%= CRMDealResource.EditThisDealButton %></a></li>
            <li><a class="deleteDealLink dropdown-item"><%= CRMDealResource.DeleteDeal %></a></li>
        </ul>
    </div>

</script>

<script id="dealExtendedListTmpl" type="text/x-jquery-tmpl">
    {{if contactID == 0}}
    <ul id="dealHeaderMenu" class="clearFix contentMenu contentMenuDisplayAll">
        <li class="menuAction menuActionSelectAll menuActionSelectLonely">
            <div class="menuActionSelect">
                <input type="checkbox" id="mainSelectAllDeals" title="<%= CRMCommonResource.SelectAll %>" onclick="ASC.CRM.ListDealView.selectAll(this);" />
            </div>
        </li>
        <li class="menuAction menuActionAddTag" title="<%= CRMCommonResource.AddNewTag %>">
            <span><%= CRMCommonResource.AddNewTag %></span>
            <div class="down_arrow"></div>
        </li>
        <li class="menuAction menuActionPermissions" title="<%= CRMCommonResource.SetPermissions %>">
            <span><%= CRMCommonResource.SetPermissions %></span>
        </li>
        <li class="menuAction menuActionDelete" title="<%= CRMCommonResource.Delete %>">
            <span><%= CRMCommonResource.Delete %></span>
        </li>
        <li class="menu-action-simple-pagenav">
        </li>
        <li class="menu-action-checked-count">
            <span></span>
            <a class="linkDescribe baseLinkAction" style="margin-left:10px;" onclick="ASC.CRM.ListDealView.deselectAll();">
                <%= CRMCommonResource.DeselectAll %>
            </a>
        </li>
        <li class="menu-action-on-top">
            <a class="on-top-link" onclick="javascript:window.scrollTo(0, 0);">
                <%= CRMCommonResource.OnTop %>
            </a>
        </li>
    </ul>
    <div class="header-menu-spacer" style="display: none;">&nbsp;</div>
    {{/if}}

    <table id="dealTable" class="tableBase" cellpadding="4" cellspacing="0">
        <colgroup>
            {{if contactID == 0}}
            <col style="width: 26px;"/>
            {{/if}}
            <col/>
            <col style="width: 150px;"/>
            {{if contactID == 0}}
            <col style="width: 180px;"/>
            {{/if}}
            <col style="width: 180px;"/>
            {{if contactID == 0}}
            <col style="width: 40px;"/>
            {{/if}}
        </colgroup>
        <tbody>
        </tbody>
    </table>

    {{if contactID != 0}}
    <div id="showMoreDealsButtons">
        <a class="crm-showMoreLink" style="display:none;">
            <%= CRMJSResource.ShowMoreButtonText %>
        </a>
        <a class="loading-link" style="display:none;">
            <%= CRMJSResource.LoadingProcessing %>
        </a>
    </div>
    <a style="float: right;margin-top: 20px;margin-right: 8px;" class="baseLinkAction showTotalAmount"
        onclick="ASC.CRM.ListDealView.showExchangeRatePopUp();" href="javascript:void(0)">
            <%= CRMDealResource.ShowTotalAmount %>
    </a>
    {{/if}}
</script>

<script id="dealListTmpl" type="text/x-jquery-tmpl">
    <tbody>
        {{tmpl(opportunities) "dealTmpl"}}
    </tbody>
</script>

<script id="dealTmpl" type="text/x-jquery-tmpl">
    <tr id="dealItem_${id}" {{if ASC.CRM.ListDealView.contactID == 0}}class="with-entity-menu"{{/if}}>
        {{if ASC.CRM.DealTabView.contactID == 0}}
        <td class="borderBase" style="padding: 0 0 0 6px;">
            <input type="checkbox" id="checkDeal_${id}" style="margin-left:2px;"
                 onclick="ASC.CRM.ListDealView.selectItem(this);"
                 {{if isChecked == true}}checked="checked"{{/if}} />
            <div id="loaderImg_${id}" class="loader-middle baseList_loaderImg"></div>
        </td>
        {{/if}}

        <td class="borderBase dealTitle">
            <div>
                {{if isPrivate == true}}
                    <label class="crm-private-lock"></label>
                {{/if}}

                <a id="dealTitle_${id}" href="deals.aspx?id=${id}"
                    class="${classForTitle}"
                    dscr_label="<%=CRMCommonResource.Description%>" dscr_value="${description}"
                    resp_label="<%=CRMCommonResource.Responsible%>" resp_value="${responsible.displayName}">
                    ${title}
                </a>

                <div style="height:4px;">&nbsp;</div>

                {{if isOverdue == true}}
                    <span class='red-text'>
                        <%= CRMJSResource.ExpectedCloseDate %>: ${expectedCloseDateString}
                    </span>
                {{else closedStatusString != ""}}
                    <span class='gray-text'>
                        ${closedStatusString}
                    </span>
                {{else expectedCloseDateString != ""}}
                    <span>
                        <%= CRMJSResource.ExpectedCloseDate %>: ${expectedCloseDateString}
                    </span>
                {{/if}}
            </div>
        </td>
        <td class="borderBase dealCategory">
            <div>
                ${stage.title}
            </div>
        </td>
        {{if ASC.CRM.DealTabView.contactID == 0}}
        <td class="borderBase dealContact">
            {{if contact != null}}
                <div>
                {{if contact.isCompany == true}}
                    <a href="default.aspx?id=${contact.id}" data-id="${contact.id}"
                            id="deal_${id}_company_${contact.id}" class="linkMedium crm-companyInfoCardLink">
                        ${contact.displayName}
                    </a>
                {{else}}
                    <a href="default.aspx?id=${contact.id}&type=people" data-id="${contact.id}"
                            id="deal_${id}_person_${contact.id}" class="linkMedium crm-peopleInfoCardLink">
                        ${contact.displayName}
                    </a>
                {{/if}}
                </div>
            {{/if}}
        </td>
        {{/if}}

        <td class="borderBase dealBidValue">
            <div>
            {{if typeof bidValue != "undefined" && bidValue != 0}}
                <span {{if closedStatusString != ""}} class="gray-text" {{/if}}>{{html bidNumberFormat}}</span><span class='describe-text'> ${bidCurrency.abbreviation}</span>
                {{if typeof bidType != "undefined" && bidType != 0}}
                    <div style="height:4px;">&nbsp;</div>
                    <span class='text-medium-describe'>${ASC.CRM.ListDealView.expectedValue(bidType, perPeriodValue)}</span>
                {{/if}}
            {{/if}}
            </div>
        </td>

        {{if ASC.CRM.DealTabView.contactID == 0}}
        <td class="borderBase">
            <div id="dealMenu_${id}" class="entity-menu" title="<%= CRMCommonResource.Actions %>"></div>
        </td>
        {{/if}}
    </tr>
</script>

<script id="bidFormat" type="text/x-jquery-tmpl">
    {{html number}}<span class='describe-text'> ${abbreviation}</span><br/>
</script>


<script id="ratesTableTmpl" type="text/x-jquery-tmpl">
    <tbody>
    <tr>
        <th style="width:30px;"></th>
        <th style="width:22px;"></th>
        <th></th>
        <th style="width:30px;"></th>
        <th style="width:22px;"></th>
        <th></th>
    </tr>
    {{each currencyRates}}
        {{if $index%2 == 0}}
        <tr>
        {{/if}}
            <td class="borderBase">
                <i class="b-fg b-fg_${this.cultureName}">
                    <img src="<%= WebImageSupplier.GetAbsoluteWebPath("fg.png", ProductEntryPoint.ID) %>"
                        alt="${this.abbreviation}" title="${this.title}"/>
                </i>
            </td>
            <td class="borderBase header-base-small" title="${this.title}">
                ${this.abbreviation}
            </td>
            <td class="borderBase rateValue" id="${this.abbreviation}" style="{{if $index%2 == 1}}padding-right:20px;{{else}}padding-right:35px;{{/if}}">
                ${this.rate}
            </td>
        {{if $index%2 == 1}}
        </tr>
        {{/if}}
    {{/each}}
    </tbody>
</script>

<%-- ExchangeRateView --%>

<script id="exchangeRateViewTmpl" type="text/x-jquery-tmpl">
    <div id="ExchangeRateTabs"></div>

    <div id="totalAmountTab">
        <div id="totalAmountContent">
            <table cellpadding="0" cellspacing="0" id="totalOnPage">
                <tr>
                    <td class="header-base-medium" style="width: 100%;"><%= CRMDealResource.TotalOnPage%>:</td>
                    <td style="white-space: nowrap;padding-left: 22px">
                        <div class="diferrentBids">
                        </div>
                        <div class="totalBidAndExchangeRateLink" style="display:none;">
                            <div class="h_line" style="margin-top: 5px; margin-bottom: 5px;">&nbsp;</div>
                            <div class="totalBid">
                            </div>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <div id="converterTab">
        <div id="convertRateContent">
            <dl>
                <dt class="header-base-small"><%= CRMCommonResource.EnterAmount%>:</dt>
                <dd>
                    <input class="textEdit" type="text" id="amount"/>
                </dd>

                <dt class="header-base-small"><%= CRMCommonResource.From %>:</dt>
                <dd>
                    <select id="fromCurrency" onchange="ASC.CRM.ExchangeRateView.changeCurrency();" class="comboBox">
                    </select>
                </dd>

                <dt class="header-base-small"><%= CRMCommonResource.To %>:</dt>
                <dd>
                    <select id="toCurrency" onchange="ASC.CRM.ExchangeRateView.changeCurrency();" class="comboBox">
                    </select>
                    <div class="h_line">&nbsp;</div>
                </dd>
            </dl>

            <table cellpadding="2" cellspacing="0" border="0">
                <tr>
                    <td class="header-base-small" style="text-align: right;" id="introducedFromCurrency"></td>
                    <td class="header-base-medium" id="introducedAmount"></td>
                </tr>
                <tr>
                    <td class="header-base-small" style="text-align: right;"><%= CRMCommonResource.ConversionRate %>:</td>
                    <td class="header-base-medium" id="conversionRate"></td>
                </tr>
                <tr>
                    <td class="header-base-small" id="introducedToCurrency" style="text-align: right;"></td>
                    <td class="header-base-medium" id="conversionResult"></td>
                </tr>
            </table>
        </div>
    </div>
    <div id="exchangeTab">
        <div id="exchangeRateContent">
            <select onchange="ASC.CRM.ExchangeRateView.updateSummaryTable(this.value);" style="width: 100%;" class="comboBox">
            </select>
            <div class="ratesTableContainer">
                <table class="tableBase" cellpadding="5" cellspacing="0" id="ratesTable"></table>
            </div>
        </div>
    </div>

    <div class="action_block clearFix" style="margin-top: 15px;">
        <div style="display:inline-block;">
            <span class="header-base-small"><%= CRMCommonResource.ConversionDate%></span>:&nbsp;   ${ratesPublisherDisplayDate}
        </div>
        <div style="float: right;">
            <span class="text-medium-describe"><%= CRMCommonResource.InformationProvidedBy%></span> <a class="link blue bold" href="http://themoneyconverter.com/" target="_blank">The Money Converter.com</a>
        </div>
        <div class="middle-button-container">
            <a class="button gray" href="javascript:void(0)" onclick="PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI();">
                <%= CRMCommonResource.CloseThisWindow%>
            </a>
        </div>
    </div>
</script>