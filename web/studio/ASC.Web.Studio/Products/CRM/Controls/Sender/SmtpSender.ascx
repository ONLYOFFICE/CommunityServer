<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SmtpSender.ascx.cs" Inherits="ASC.Web.CRM.Controls.Sender.SmtpSender" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Core.WhiteLabel" %>

<div id="sendEmailPanel" style="display:none;">
    <div id="createContent">
        <div class="headerPanel-splitter">
            <%: CRMCommonResource.ComposeMailDescription%>
        </div>
        <div class="headerPanel-splitter">
            <b style="padding-right:5px;"><%= CRMCommonResource.MailFrom%>:</b>
            <span id="emailFromLabel"></span>
        </div>
        <div class="headerPanel-splitter">
            <b style="padding-right:5px;"><%= CRMCommonResource.MailTo%>:</b>
            <span id="emailAddresses"></span>
        </div>
        <div class="headerPanel-splitter">
            <div class="headerPanelSmall-splitter bold">
                <%= CRMCommonResource.Subject%>:
            </div>
            <input type="text" class="textEdit" id="tbxEmailSubject" style="width: 100%" />
        </div>
        <div class="headerPanel-splitter">
            <b style="padding-right:5px;"><%= CRMContactResource.PersonalTags%>:</b>
            <select id="emailTagTypeSelector" onchange="ASC.CRM.SmtpSender.renderTagSelector()" class="comboBox">
                <option value="person" selected="selected"><%=CRMContactResource.Person%></option>
                <option value="company"><%=CRMContactResource.Company%></option>
            </select>
            <select id="emailPersonTagSelector" class="comboBox">
                <%=RenderTagSelector(false)%>
            </select>
            <select id="emailCompanyTagSelector" class="comboBox" style="display: none;">
                <%=RenderTagSelector(true)%>
            </select>
            <a onclick="ASC.CRM.SmtpSender.emailInsertTag()" class="button gray" style="margin: 0 0 3px 5px">
                <%= CRMContactResource.AddToLetterBody%>
            </a>
        </div>
        <div class="headerPanel-splitter requiredField">
            <span class="requiredErrorText"></span>
            <div class="headerPanelSmall headerPanelSmall-splitter bold">
                <%= CRMCommonResource.LetterBody%>:
            </div>
            <input type="hidden" id="requiredMessageBody"/>
            <textarea id="ckEditor" name="ckEditor" style="width: 100%;height: 400px;"></textarea>
        </div>
        <% if (CompanyWhiteLabelSettings.Instance.IsDefault) { %>
        <div id="watermarkInfo" class="headerPanel-splitter">
            <span class="text-medium-describe">*<%: CRMContactResource.TeamlabWatermarkInfo %></span>
        </div>
        <% } %>
        <div class="clearFix">
            <div style="float:right;">
                <a id="attachShowButton" class="attachLink baseLinkAction linkMedium" onclick="javascript:jq('#attachOptions, #attachHideButton').show(); jq('#attachShowButton').hide();" >
                    <%= CRMCommonResource.ShowAttachPanel%>
                </a>
                <a id="attachHideButton" class="attachLink baseLinkAction linkMedium" onclick="javascript:jq('#attachOptions, #attachHideButton').hide(); jq('#attachShowButton').show();" style="display: none;" >
                    <%= CRMCommonResource.HideAttachPanel%>
                </a>
            </div>
            <div>
                <input id="storeInHistory" type="checkbox" style="float: left;"/>
                <label for="storeInHistory" style="float: left; padding: 2px 0 0 4px;">
                    <%= CRMCommonResource.StoreThisLetterInHistory%>
                </label>
            </div>
        </div>
        <div id="attachOptions" style="display:none;margin: 10px 0;">
            <asp:PlaceHolder ID="phFileUploader" runat="server"></asp:PlaceHolder>
        </div>
    </div>
    <div id="previewContent" style="display:none;">
        <div class="headerPanel-splitter">
            <%: CRMCommonResource.PreviewMailDescription%>
        </div>
        <div class="headerPanel-splitter">
            <b style="padding-right:5px;"><%= CRMCommonResource.MailFrom%>:</b>
            <span id="previewEmailFromLabel"></span>
        </div>
        <div class="headerPanel-splitter">
            <b style="padding-right:5px;"><%= CRMCommonResource.MailTo%>:</b>
            <span id="previewEmailAddresses"></span>
        </div>
        <div class="headerPanel-splitter">
            <div class="headerPanelSmall-splitter bold"><%= CRMCommonResource.Subject%>:</div>
            <div id="previewSubject"></div>
        </div>
        <div class="headerPanel-splitter">
            <div class="headerPanelSmall-splitter bold"><%= CRMCommonResource.LetterBody%>:</div>
            <div id="previewMessage" style="max-height: 400px;overflow-y: auto;"></div>
        </div>
        <div class="headerPanel-splitter" id="previewAttachments" style="display:none;">
            <div class="headerPanelSmall-splitter bold"><%= CRMCommonResource.Attachments%>:</div>
            <label class="previewAttachment" align="absmiddle"></label>
            <span></span>
        </div>
    </div>

    <div class="middle-button-container">
        <span id="backButton" style="display:none;">
            <a class="button blue middle">
                <%= CRMCommonResource.Back%>
            </a>
            <span class="splitter-buttons"></span>
        </span>
        <a class="button blue middle" id="sendButton">
            <%= CRMJSResource.NextPreview%>
        </a>
        <span class="splitter-buttons"></span>
        <a class="button gray middle" href="Default.aspx">
            <%= CRMCommonResource.Cancel%>
        </a>
    </div>

</div>

<div id="sendProcessPanel" style="display:none;">
    <table width="100%" cellpadding="0" cellspacing="0">
        <colgroup>
            <col style="width: 190px;"/>
            <col style="width: 250px;">
            <col/>
        </colgroup>
        <tbody>
            <tr>
                <td>
                    <img src="<%=WebImageSupplier.GetAbsoluteWebPath("mail_send.png", ProductEntryPoint.ID)%>" alt=""/>
                </td>
                <td style="color: #787878;font-size: 17px;" colspan="2">
                    <%= String.Format(CRMContactResource.MassSendInfo.HtmlEncode(), "<br/>")%>
                    <div id="sendProcessProgress" class="clearFix progress-container">
                        <div class="percent">0%</div>
                        <div class="progress-wrapper">
                            <div class="progress" style="width: 0%"></div>
                        </div>
                    </div>
                </td>
            </tr>
            <tr class="sendProcessInfo">
                <td></td>
                <td class="describe-text">
                    <%= CRMContactResource.MassSendEmailsTotal%>:
                </td>
                <td id="emailsTotalCount"></td>
            </tr>
            <tr class="sendProcessInfo">
                <td></td>
                <td class="describe-text">
                    <%= CRMContactResource.MassSendAlreadySent%>:
                </td>
                <td id="emailsAlreadySentCount"></td>
            </tr>
            <tr class="sendProcessInfo">
                <td></td>
                <td class="describe-text">
                    <%= CRMContactResource.MassSendEstimatedTime%>:
                </td>
                <td id="emailsEstimatedTime"></td>
            </tr>
            <tr class="sendProcessInfo">
                <td></td>
                <td class="describe-text">
                    <%= CRMContactResource.MassSendErrors%>:
                </td>
                <td id="emailsErrorsCount"></td>
            </tr>
            <tr>
                <td></td>
                <td colspan="2">
                    <div class="middle-button-container">
                        <a id="abortButton" class="button middle gray" onclick="ASC.CRM.SmtpSender.abortMassSend()">
                            <%= CRMContactResource.AbortMassSend%>
                        </a>
                        <a id="okButton" class="button middle gray" href="Default.aspx" style="display: none;">
                            <%= CRMCommonResource.OK%>
                        </a>
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</div>