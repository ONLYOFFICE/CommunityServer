<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control EnableViewState="true" Language="C#" AutoEventWireup="true" CodeBehind="DealActionView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Deals.DealActionView" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div id="crm_dealMakerDialog">
    <div class="deal_info clearFix">
        <div class="requiredField headerPanelSmall-splitter" style="padding-right:2px;">
            <span class="requiredErrorText"><%=CRMDealResource.EmptyDealName%></span>
            <div class="headerPanelSmall header-base-small" style="margin-bottom: 5px;"><%=CRMDealResource.NameDeal%>:</div>
            <div>
                <input type="text" id="nameDeal" style="width: 100%" name="nameDeal" class="textEdit" maxlength="255" />
            </div>
        </div>

        <dl class="headerPanelSmall-splitter">
            <dt class="header-base-small"><%= CRMDealResource.ClientDeal %>:</dt>
            <dd id="dealClientContainer"></dd>

            <dt id="dealMembersHeader" class="header-base-small" style="display:none;"><%= CRMDealResource.OtherMembersDeal %>:</dt>
            <dd id="dealMembersBody" style="display:none;"></dd>  

            <dt style="margin:20px 0;" class="assignedDealContacts hiddenFields">
                <div id="dealsContactListBox">
                    <table id="contactTable" class="table-list padding4" cellpadding="0" cellspacing="0">
                        <tbody>
                        </tbody>
                    </table>
                </div>
            </dt>
            <dd>
                <input type="hidden" name="memberID" value="" />
            </dd>

            <dt class="header-base-small"><%=CRMDealResource.DescriptionDeal%>:</dt>
            <dd style="padding-right:2px;">
                <textarea name="descriptionDeal" id="descriptionDeal" style="width:100%;height:150px;resize:none;"
                    class="textEdit"></textarea>
            </dd>

            <dt class="header-base-small" id="bidCurrencyHeader"><%=CRMDealResource.ExpectedValue%>:</dt>
            <dd>
                <select id="bidCurrency" name="bidCurrency" class="comboBox">
                    <optgroup label="<%= CRMCommonResource.Currency_Basic %>">
                        <% foreach (var keyValuePair in CurrencyProvider.GetBasic())%>
                        <% { %>
                        <option value="<%=keyValuePair.Abbreviation%>" <%=IsSelectedBidCurrency(keyValuePair.Abbreviation) ? "selected=selected" : String.Empty%>>
                            <%=String.Format("{0} - {1}", keyValuePair.Abbreviation, keyValuePair.Title)%></option>
                        <% } %>
                    </optgroup>
                    <optgroup label="<%= CRMCommonResource.Currency_Other %>">
                        <% foreach (var keyValuePair in CurrencyProvider.GetOther())%>
                        <% { %>
                        <option value="<%=keyValuePair.Abbreviation%>" <%=IsSelectedBidCurrency(keyValuePair.Abbreviation) ? "selected=selected" : String.Empty%>>
                            <%=String.Format("{0} - {1}", keyValuePair.Abbreviation, keyValuePair.Title)%></option>
                        <% } %>
                    </optgroup>
                </select>
                <span class="splitter"></span>
                <input type="text" style="width: 100px" name="bidValue" id="bidValue" class="textEdit" value="0" maxlength="15"/>
                <span class="splitter"></span>
                <select id="bidType" name="bidType" onchange="ASC.CRM.DealActionView.selectBidTypeEvent(this)" class="comboBox">
                    <option value="<%=BidType.FixedBid%>">
                        <%=CRMDealResource.BidType_FixedBid%>
                    </option>
                    <option value="<%=(int) BidType.PerHour%>">
                        <%=CRMDealResource.BidType_PerHour%></option>
                    <option value="<%=(int) BidType.PerDay%>">
                        <%=CRMDealResource.BidType_PerDay%></option>
                    <option value="<%=(int) BidType.PerWeek%>">
                        <%=CRMDealResource.BidType_PerWeek%></option>
                    <option value="<%=(int) BidType.PerMonth%>">
                        <%=CRMDealResource.BidType_PerMonth%></option>
                    <option value="<%=(int) BidType.PerYear%>">
                        <%=CRMDealResource.BidType_PerYear%></option>
                </select>

                <span class="splitter"></span>
                <input type="text" style="width: 50px; display:none;" id="perPeriodValue"
                    name="perPeriodValue" value="0" class="textEdit" />
                <span class="splitter"></span>
            </dd>

            <dt class="header-base-small"><%=CRMJSResource.ExpectedCloseDate%>:</dt>
            <dd>
                <input type="text" id="expectedCloseDate" name="expectedCloseDate" class="textEdit textEditCalendar" autocomplete="off"/>
            </dd>
        </dl>

        <div id="AdvUserSelectorContainer" class="requiredField headerPanelSmall-splitter">
            <span class="requiredErrorText"><%=CRMDealResource.EmptyDealResponsible%></span>
            <div class="headerPanelSmall header-base-small" style="margin-bottom:5px;"><%=CRMDealResource.ResponsibleDeal%>:</div>
            <div id="advUserSelectorResponsible" data-responsible-id="" style="position: relative;">
                <span>
                    <a class="link dotline dealResponsibleLabel"></a>
                    <span class="sort-down-black"></span>
                </span>
            </div>
        </div>

        <dl class="headerPanelSmall-splitter">
            <dt class="header-base-small"><%=CRMDealResource.CurrentDealMilestone%>:</dt>
            <dd>
                <select id="dealMilestone" name="dealMilestone" onchange="javascript: jq('#probability').val(window.dealMilestones[this.selectedIndex].probability);" class="comboBox">
                </select>
            </dd>
            <dt class="header-base-small"><%=CRMDealResource.ProbabilityOfWinning%>:</dt>
            <dd>
                <input type="text" id="probability" name="probability" class="textEdit" style="width: 30px;"
                    maxlength="3" value="0" />&nbsp;(%)
            </dd>
        </dl>

        <% if (CRMSecurity.IsAdmin) %>
        <% { %>
        <div id="otherDealsCustomFieldPanel">
            <div class="bold" style="margin: 16px 0 10px;"><%= CRMSettingResource.OtherFields %></div>
            <a onclick="ASC.CRM.DealActionView.showGotoAddSettingsPanel();" style="text-decoration: underline" class="linkMedium">
                <%= CRMSettingResource.SettingCustomFields %>
            </a>
        </div>
        <% } %>

        <% if (HavePermission) %>
        <% { %>
        <div class="dealPrivatePanel">
            <asp:PlaceHolder ID="phPrivatePanel" runat="server"></asp:PlaceHolder>
        </div>
        <% } %>
    </div>


    <div style="display: none" id="autoCompleteBlock"></div>

    <div class="middle-button-container">
        <asp:LinkButton runat="server" ID="saveDealButton" CommandName="SaveDeal" CommandArgument="0"
            OnClientClick="return ASC.CRM.DealActionView.submitForm();" OnCommand="SaveOrUpdateDeal" CssClass="button blue big" />
         <span class="splitter-buttons"></span>
         <% if (TargetDeal == null)%>
         <% { %>
         <asp:LinkButton runat="server" ID="saveAndCreateDealButton"  CommandName="SaveDeal" CommandArgument="1"
            OnClientClick="return ASC.CRM.DealActionView.submitForm();"
            OnCommand="SaveOrUpdateDeal" CssClass="button gray big" />
         <span class="splitter-buttons"></span>
         <% } %>
        <asp:HyperLink runat="server" CssClass="button gray big cancelSbmtFormBtn" ID="cancelButton"> <%= CRMCommonResource.Cancel%></asp:HyperLink>

        <% if (TargetDeal != null) %>
        <% { %>
        <span class="splitter-buttons"></span>
        <a id="deleteDealButton" class="button gray big"><%= CRMDealResource.DeleteDeal %></a>
        <% } %>
    </div>
</div>

<input type="hidden" id="responsibleID" name="responsibleID" />
<input type="hidden" id="selectedContactID" name="selectedContactID" />
<input type="hidden" id="selectedMembersID" name="selectedMembersID" />
<input type="hidden" id="selectedPrivateUsers" name="selectedPrivateUsers" value="" />
<input type="hidden" id="isPrivateDeal" name="isPrivateDeal" value="" />
<input type="hidden" id="notifyPrivateUsers" name="notifyPrivateUsers" value="" />