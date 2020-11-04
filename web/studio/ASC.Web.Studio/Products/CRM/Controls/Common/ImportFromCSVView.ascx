<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportFromCSVView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Common.ImportFromCSVView" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>


<br />
<dl id="importFromCSVSteps">
    <dt>1</dt>
    <dd>
        <span class="header-base">
            <%= ImportFromCSVStepOneHeaderLabel %>
        </span>
        <br />
        <span class="describe-text">
            <%=  ASC.Web.Studio.Core.FileSizeComment.GetFileSizeNote(true)%>
        </span>
        <br />
        <br />
        <span class="describe-text">
            <%= String.Format(ImportFromCSVStepOneDescriptionLabel.HtmlEncode(), ASC.Web.CRM.Classes.ImportFromCSV.GetQuotas())%></span>
        <div class="content-info-import">
            <br />
            <span style="display: none; margin-right: 14px;"></span><a id="uploadCSVFile" class="import_button link dotline">
                <%= CRMJSResource.SelectCSVFileButton%></a>
            <br />
            <br />
            <div>
                <div style="margin-bottom: 10px;" class="bold">
                   <%= CRMCommonResource.ImportFromCSV_ReadFileSettings_Header%>:</div>
                <div>
                    <span>
                        <%= CRMCommonResource.ImportFromCSV_ReadFileSettings_DelimiterCharacter%></span>:
                    <select id="delimiterCharacterSelect">
                        <option selected="selected" value="<%=(int)","[0] %>"> <%= CRMCommonResource.Comma%></option>
                        <option value="<%= (int)";"[0] %>"> <%= CRMCommonResource.Semicolon%></option>
                        <option value="<%= (int)":"[0] %>"> <%= CRMCommonResource.Colon%></option>
                        <option value="<%= (int)"\t"[0] %>" ><%= CRMCommonResource.Tabulation%></option>
                        <option value="<%= (int)" "[0] %>"><%= CRMCommonResource.Space%></option>
                    </select>
                </div>
                <div>
                    <span>
                        <%= CRMCommonResource.ImportFromCSV_ReadFileSettings_Encoding%></span>:
                    <select id="encodingSelect">
                        <% foreach (var encoding in GetEncodings()) %>
                        <% { %>

                        <% if (encoding.CodePage == 65001) %>
                        <% { %>
                         <option selected="selected" value='<%=encoding.CodePage%>'>
                            <%=String.Concat(encoding.DisplayName, " - ",encoding.Name) %></option>
                        <%} else {%>
                         <option value='<%=encoding.CodePage%>'>
                            <%=String.Concat(encoding.DisplayName, " - ",encoding.Name) %></option>
                        <% } %>

                        <% } %>
                    </select>
                </div>
                <div>
                    <span><%= CRMCommonResource.ImportFromCSV_ReadFileSettings_QuoteCharacter%></span>:
                    <select id="quoteCharacterSelect">
                        <option selected="selected" value="<%= (int)"\""[0] %>"> <%= CRMCommonResource.DoubleQuote%></option>
                        <option value="<%= (int)"'"[0] %>"> <%= CRMCommonResource.SingleQuote%></option>
                    </select>
                </div>
            </div>
            <div id="removingDuplicatesBehaviorPanel" style="display: none;">
                 <br />
                <span style="font-weight: bold;"><%= CRMCommonResource.DuplicateRecords %>:</span>
                <br />
                <span class="describe-text"><%: CRMCommonResource.ImportFromCSV_DublicateBehavior_Description%></span>
                <br />
                <br />
                <label>
                    <input type="radio" name="removingDuplicatesBehavior" value="1" />
                    <%= CRMCommonResource.ImportFromCSV_DublicateBehavior_Skip%>
                </label>
                <div title="<%= CRMCommonResource.ShowInformation %>" onclick="jq(this).helper({ BlockHelperID: 'SkipDescription'});"
                    class="HelpCenterSwitcher">
                </div>
                <div id="SkipDescription" class="popup_helper">
                    <%: CRMCommonResource.ImportFromCSV_DublicateBehavior_SkipDescription%>
                    <div class="pos_top">
                    </div>
                </div>
                <br />
                <label>
                    <input type="radio" name="removingDuplicatesBehavior" value="2" />
                    <%= CRMCommonResource.ImportFromCSV_DublicateBehavior_Overwrite%>
                </label>
                <div title="<%= CRMCommonResource.ShowInformation %>" onclick="jq(this).helper({ BlockHelperID: 'OverwriteDescription'});"
                    class="HelpCenterSwitcher">
                </div>
                <div id="OverwriteDescription" class="popup_helper">
                    <%: CRMCommonResource.ImportFromCSV_DublicateBehavior_OverwriteDescription%>
                    <div class="pos_top">
                    </div>
                </div>
                <br />
                <label>
                    <input type="radio" name="removingDuplicatesBehavior" checked="checked" value="3" />
                    <%= CRMCommonResource.ImportFromCSV_DublicateBehavior_Clone%>
                </label>
                <div title="<%= CRMCommonResource.ShowInformation %>" onclick="jq(this).helper({ BlockHelperID: 'CloneDescription'});"
                    class="HelpCenterSwitcher">
                </div>
                <div id="CloneDescription" class="popup_helper">
                    <%: CRMCommonResource.ImportFromCSV_DublicateBehavior_CloneDescription%>
                    <div class="pos_top">
                    </div>
                </div>
            </div>
            <br />
            <label>
                <input type="checkbox" id="ignoreFirstRow" checked="checked" />
                <%= CRMCommonResource.IgnoreFirstRow%>
            </label>

            <div style="margin-top:24px;">
                <asp:PlaceHolder runat="server" ID="_phPrivatePanel"></asp:PlaceHolder>
            </div>

            <% if (EntityType == ASC.CRM.Core.EntityType.Contact) %>
            <% { %>
                <div class="accessRightsPanel">
                    <div class="header-base"><%= CRMCommonResource.PrivatePanelHeader %></div>
                    <div class="contactManager-selectorContent">
                            <div class="headerPanelSmall"><%= CRMContactResource.AssignContactManager %></div>
                            <div id="importContactsManager"></div>
                    </div>
                    <div style="margin-top:23px;" id="makePublicPanel"></div>
                </div>
            <% } %>
            <% if (EntityType != ASC.CRM.Core.EntityType.Task) { %>
                <div style="margin: 24px 0 10px 0; font-weight: bold"><%=CRMCommonResource.Tags%>:</div>
                <div id="importFromCSVTags"></div>
            <% } %>



            <div class="middle-button-container">
                <a class="button blue middle disable" href="javascript:void(0)" onclick="ASC.CRM.ImportEntities.startUploadCSVFile(this)">
                    <%= CRMContactResource.Continue%>
                </a>
                <span class="splitter-buttons"></span>
                <a href="Default.aspx" class="button gray middle">
                    <%= CRMCommonResource.Cancel%>
                </a>
            </div>
        </div>
    </dd>
    <dt style="display: none">2</dt>
    <dd style="display: none">
        <span class="header-base">
            <%: ImportFromCSVStepTwoHeaderLabel %></span><br />
        <span class="describe-text">
            <%= String.Format(ImportFromCSVStepTwoDescriptionLabel.HtmlEncode(), System.DateTimeExtension.DateFormatPattern)%></span>
        <table id="columnMapping" cellspacing="0" cellpadding="0" class="table-list padding10">
            <thead>
                <tr>
                    <td style="width: 20%">
                        <%: CRMCommonResource.Column %>:
                    </td>
                    <td style="width: 20%">
                        <%: CRMCommonResource.AssignedField %>:
                    </td>
                    <td style="width: 60%">
                       <div>
                        <span style="float: left;">
                            <%: CRMCommonResource.SampleValues %>:
                        </span>
                           <span style="float: right;">
                               <a id="prevSample" class="link dotline" href="javascript:void(0)" onclick="javascript:ASC.CRM.ImportEntities.getPrevSampleRow();"
                                style="display: none">
                                    <%= CRMCommonResource.PrevSample %>
                               </a>
                            <span class="splitter" style="display: none;">|</span>
                               <a id="nextSample" class="link dotline" href="javascript:void(0)" onclick="javascript:ASC.CRM.ImportEntities.getNextSampleRow();">
                                   <%= CRMCommonResource.NextSample %>
                               </a>
                           </span>
                       </div>
                    </td>
                </tr>
            </thead>
            <tbody>
            </tbody>
        </table>
        <div class="middle-button-container">
            <a class="button blue middle" href="javascript:void(0)" onclick="ASC.CRM.ImportEntities.startImport()">
                <%: StartImportLabel %>
            </a>
            <span class="splitter-buttons"></span>
            <a onclick="ASC.CRM.ImportEntities.prevStep(0)" class="button gray middle">
                <%= CRMCommonResource.PrevStep%>
            </a>
            <span class="splitter-buttons"></span>
            <a href="Default.aspx" class="button gray middle">
                <%= CRMCommonResource.Cancel%>
            </a>
        </div>
    </dd>
</dl>
<select id="columnSelectorBase" name="columnSelectorBase" style="display: none;"
    class="comboBox">
</select>
<div id="importStartedFinalMessage" style="display: none;">
    <table width="100%" cellpadding="0" cellspacing="0">
        <colgroup>
            <col style="width: 160px;"/>
            <col/>
        </colgroup>
        <tbody>
            <tr valign="top">
                <td>
                    <img src="<%=ImportImgSrc%>" />
                </td>
                <td>
                    <span class="header-base">
                        <%: ImportStartingPanelHeaderLabel%>
                    </span>
                    <p class="header-base-small">
                        <%: ImportStartingPanelDescriptionLabel %>
                    </p>
                    <div class="clearFix progress-container">
                        <div class="percent">0%</div>
                        <div class="progress-wrapper">
                            <div class="progress"></div>
                        </div>
                    </div>
                    <div id="importErrorBox" class="clearFix">
                        <div><%= CRMContactResource.MassSendErrors %>:</div>
                        <div class="progressErrorBox">
                        </div>
                    </div>
                    <div id="importLinkBox" class="middle-button-container">
                        <a href="<%= GoToRedirectURL %>" class="button middle blue">
                            <%= ImportStartingPanelButtonLabel%>
                        </a>
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</div>