<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SingleSignOnSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.SingleSignOnSettings.SingleSignOnSettings" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="Newtonsoft.Json" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="clearFix">
    <div class="sso-settings-main-container <%= !isAvailable ? "disable" : "" %>">
        <div class="sso-main-container settings-block">
            <div class="clearFix">
                <div class="header-base"><%= Resource.SingleSignOnSettings %></div>
                <label class="sso-settings-label-checkbox">
                    <a id="ssoEnableBtn" class="on_off_button 
                        <% if (!Settings.EnableSso)
                        { %> off <% }
                        else
                        { %> on <% }
                        %>"></a>
                    <span class="settings-checkbox-text"><%= Resource.SsoEnable %></span>

                </label>
                <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoEnableBtnHelper'});"></span>
                <div class="popup_helper" id="ssoEnableBtnHelper">
                    <p><%: Resource.SsoEnableBtnHelper %></p>
                </div>
            </div>


            <div class="top-indent-big">
                <div class="header-base sso-settings-title"><%= Resource.SsoSPSettings.HtmlEncode() %></div>
                <a id="ssoSPSettingsSpoilerLink" class="link dotted sso-settings-spoiler-link" title="<%= Resource.Show%>"><%= Resource.Show%></a>
            </div>

            <div id="ssoSPSettingsSpoiler" class="display-none">
                <div class="sso-settings-block requiredField">
                    <div class="header-base-small"><%= Resource.SsoLoadMetadataHdr %></div>
                    <div class="ssoUploadMetadataContainer">
                        <div>
                            <div>
                                <div class="inputWithBtn withUploadBtn">
                                    <input class="textEdit" id="ssoUploadMetadataInput" type="text" placeholder="<%= Resource.SsoLoadMetadataPlaceholder%>" value=""
                                        <% if (!Settings.EnableSso)
                                        { %> disabled <% } %>>
                                    <a class="button gray <% if (!Settings.EnableSso)
                                        { %> disable <% } %>" id="ssoUploadMetadataBtn">
                                        <svg class="upload-svg">
                                            <use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/documents-icons.svg?ver=<%= HttpUtility.UrlEncode(ASC.Web.Core.Client.ClientSettings.ResetCacheKey) %>#documentsIconsupload"></use>
                                        </svg>
                                    </a>
                                </div>
                            </div>
                            <div>
                                <div class="sso-btn-splitter"><%= Resource.Or%></div>
                            </div>
                            <div class="ssoSelectMetadataBtnContainer">
                                <div class="button gray uploadBtn <% if (!Settings.EnableSso)
                                    { %> disable <% } %>" id="ssoSelectMetadataBtn"><%= Resource.SelectFile%><input id="ssoSelectMetadataInput" type="file" name="metadata" accept=".xml" <% if (!Settings.EnableSso)
                            { %> disabled <% } %>></div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoSpLoginLabelError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoSPLoginLabel %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoSpLoginLabelHelper'});"></span>
                        <div class="popup_helper" id="ssoSpLoginLabelHelper">
                            <p><%: Resource.SsoSpLoginLabelHelper %></p>
                        </div>
                    </div>
                    <input id="ssoSpLoginLabel" class="textEdit" placeholder="<%= Resource.SsoSPLoginLabelPlaceholder%>" type="text" value="<%=Settings.SpLoginLabel %>" <% if (!Settings.EnableSso)
                        { %> disabled <% } %>>
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoEntityIdError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoEntityId %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoEntityIdHelper'});"></span>
                        <div class="popup_helper" id="ssoEntityIdHelper">
                            <p><%: Resource.SsoEntityIdHelper %></p>
                        </div>
                    </div>
                    <input id="ssoEntityId" class="textEdit" type="text" placeholder="<%= Resource.SsoEntityIdPlaceholder%>" value="<%=Settings.IdpSettings.EntityId %>" <% if (!Settings.EnableSso)
                        { %> disabled <% } %>>
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoSignRedirectUrlError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoSignUrl %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoSignUrlHelper'});"></span>
                        <div class="popup_helper" id="ssoSignUrlHelper">
                            <p><%: Resource.SsoSignUrlHelper %></p>
                        </div>
                        <div class="radio-container">
                            <span><%= Resource.SsoBinding %>:</span>
                            <span>
                                <input id="ssoSignPostRbx" type="radio" name="ssoSignRbx" data-value='ssoSignPostUrl' <% if (!Settings.EnableSso)
                                    { %> disabled <% } %>
                                    <% if (isSsoPost)
                                    {%> checked <%}%>>
                                <label for="ssoSignPostRbx"><%= Resource.SsoBindingPost %></label>
                            </span>
                            <span>
                                <input id="ssoSignRedirectRbx" type="radio" name="ssoSignRbx" data-value='ssoSignRedirectUrl' <% if (!Settings.EnableSso)
                                    { %> disabled <% } %>
                                    <% if (isSsoRedirect)
                                    {%> checked <%}%>>
                                <label for="ssoSignRedirectRbx"><%= Resource.SsoBindingRedirect %></label>
                            </span>
                        </div>
                    </div>
                    <input id="ssoSignPostUrl" class="textEdit
                        <% if (!isSsoPost)
                        {%> display-none <%}%>"
                        type="text" value="<%=isSsoPost ? Settings.IdpSettings.SsoUrl : "" %>" <% if (!Settings.EnableSso)
                        { %> disabled <% } %> placeholder="<%= Resource.SsoSignPostUrlPlaceholder %>">
                    <input id="ssoSignRedirectUrl" class="textEdit
                         <% if (!isSsoRedirect)
                        {%> display-none <%}%>"
                        type="text" value="<%=isSsoRedirect ? Settings.IdpSettings.SsoUrl : "" %>" <% if (!Settings.EnableSso)
                        { %> disabled <% } %> placeholder="<%= Resource.SsoSignRedirectUrlPlaceholder %>">
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoLogoutPostUrlError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoLogoutUrl %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoLogoutUrlHelper'});"></span>
                        <div class="popup_helper" id="ssoLogoutUrlHelper">
                            <p><%: Resource.SsoLogoutUrlHelper %></p>
                        </div>
                        <div class="radio-container">
                            <span><%= Resource.SsoBinding %>:</span>
                            <span>
                                <input id="ssoLogoutPostRbx" type="radio" name="ssoLogoutRbx" data-value='ssoLogoutPostUrl' <% if (!Settings.EnableSso)
                                    { %> disabled <% } %>
                                    <% if (isSloPost)
                                    {%> checked <%}%>>
                                <label for="ssoLogoutPostRbx"><%= Resource.SsoBindingPost %></label>
                            </span>
                            <span>
                                <input id="ssoLogoutRedirectRbx" type="radio" name="ssoLogoutRbx" data-value='ssoLogoutRedirectUrl' <% if (!Settings.EnableSso)
                                    { %> disabled <% } %>
                                    <% if (isSloRedirect)
                                    {%> checked <%}%>>
                                <label for="ssoLogoutRedirectRbx"><%= Resource.SsoBindingRedirect %></label>
                            </span>
                        </div>
                    </div>
                    <input id="ssoLogoutPostUrl" class="textEdit
                        <% if (!isSloPost)
                        {%> display-none <%}%>"
                        type="text" value="<%=isSloPost ? Settings.IdpSettings.SloUrl : "" %>" <% if (!Settings.EnableSso)
                        { %> disabled <% } %> placeholder="<%= Resource.SsoLogoutPostUrlPlaceholder %>">
                    <input id="ssoLogoutRedirectUrl" class="textEdit
                        <% if (!isSloRedirect)
                        {%> display-none <%}%>"
                        type="text" value="<%=isSloRedirect ? Settings.IdpSettings.SloUrl : "" %>" <% if (!Settings.EnableSso)
                        { %> disabled <% } %> placeholder="<%= Resource.SsoLogoutRedirectUrlPlaceholder %>">
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoNameIdFormatError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoNameIdFormat %>:</span>                        
                    </div>
                    <%=RenderSsoNameIdFormatSelector(Settings.EnableSso, Settings)%>
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoIdPCertificatesError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoIdPCertificates %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoIdPCertificatesHelper'});"></span>
                        <div class="popup_helper" id="ssoIdPCertificatesHelper">
                            <p><%: Resource.SsoIdPCertificatesHelper %></p>
                        </div>
                    </div>
                    <div id="ssoIdPCertificateContainer" class="<% if (!Settings.EnableSso)
                        { %> disable <% } %>"></div>
                    <div class="certificate-btn-container">
                        <a class="button gray <% if (!Settings.EnableSso)
                            { %> disable <% } %>" id="ssoAddIdPCertificateBtn" href="javascript:void(0);"><%= Resource.SsoAddCertificate %></a>
                        <a class="link dotted sso-spoiler-link" id="ssoIdpCertificateSpoilerLink" title="<%= Resource.SsoHideAdvancedSettings %>"><%= Resource.SsoHideAdvancedSettings %></a>
                    </div>

                    <div class="certificate-spoiler display-none" id="ssoIdpCertificateSpoiler">
                        <div class="clearFix">
                            <input type="checkbox" id="ssoVerifyAuthResponsesSignCbx" <% if (!Settings.EnableSso)
                                { %> disabled <% } %> <% if (Settings.IdpCertificateAdvanced.VerifyAuthResponsesSign) {  %> checked <% } %> />
                            <label for="ssoVerifyAuthResponsesSignCbx" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                                <%= Resource.SsoVerifyAuthResponsesSign %>
                            </label>
                        </div>
                        <div class="clearFix">
                            <input type="checkbox" id="ssoVerifyLogoutRequestsSignCbx" <% if (!Settings.EnableSso)
                                { %> disabled <% } %> <% if (Settings.IdpCertificateAdvanced.VerifyLogoutRequestsSign) {  %> checked <% } %>>
                            <label for="ssoVerifyLogoutRequestsSignCbx" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                                <%= Resource.SsoVerifyLogoutRequestsSign %>
                            </label>
                        </div>
                        <div class="clearFix">
                            <input type="checkbox" id="ssoVerifyLogoutResponsesSignCbx" <% if (!Settings.EnableSso)
                                { %> disabled <% } %> <% if (Settings.IdpCertificateAdvanced.VerifyLogoutResponsesSign) {  %> checked <% } %> >
                            <label for="ssoVerifyLogoutResponsesSignCbx" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                                <%= Resource.SsoVerifyLogoutResponsesSign %>
                            </label>
                        </div>
                        <span class=""><%= Resource.SsoDefaultSignVerifyingAlgorithm %>:</span>
                        <div class="clearFix algorithm-select-container">
                            <%=RenderSignVerifyingAlgorithmSelector("ssoDefaultSignVerifyingAlgorithm", Settings)%>
                        </div>
                        <div class="display-none">
                            <span class=""><%= Resource.SsoEncryptAlgorithm %>:</span>
                            <div class="clearFix algorithm-select-container">
                                <%=RenderEncryptAlgorithmTypeSelector("ssoDefaultDecryptAlgorithm", Settings)%>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoSPCertificatesError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoSPCertificates %></span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoSPCertificatesHelper'});"></span>
                        <div class="popup_helper" id="ssoSPCertificatesHelper">
                            <p><%: Resource.SsoSPCertificatesHelper %></p>
                        </div>
                    </div>
                    <div id="ssoSPCertificateContainer" class="<% if (!Settings.EnableSso)
                        { %> disable <% } %>"></div>
                    <div class="certificate-btn-container">
                        <a class="button gray <% if (!Settings.EnableSso)
                            { %> disable <% } %>" id="ssoAddSPCertificateBtn" href="javascript:void(0);"><%= Resource.SsoAddCertificate %></a>
                        <a class="link dotted sso-spoiler-link" id="ssoSpCertificateSpoilerLink" title="<%= Resource.SsoShowAdvancedSettings %>"><%= Resource.SsoShowAdvancedSettings %></a>
                    </div>

                    <div class="certificate-spoiler display-none" id="ssoSpCertificateSpoiler">
                        <div class="clearFix">
                            <input type="checkbox" id="ssoSignAuthRequestsCbx" <% if (!Settings.EnableSso)
                                { %> disabled <% } %> <% if (Settings.SpCertificateAdvanced.SignAuthRequests) {  %> checked <% } %>>
                            <label for="ssoSignAuthRequestsCbx" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                                <%= Resource.SsoSignAuthRequests %>
                            </label>
                        </div>
                        <div class="clearFix">
                            <input type="checkbox" id="ssoSignLogoutRequestsCbx" <% if (!Settings.EnableSso)
                                { %> disabled <% } %> <% if (Settings.SpCertificateAdvanced.SignLogoutRequests) {  %> checked <% } %> >
                            <label for="ssoSignLogoutRequestsCbx" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                                <%= Resource.SsoSignLogoutRequests %>
                            </label>
                        </div>
                        <div class="clearFix">
                            <input type="checkbox" id="ssoSignLogoutResponsesCbx" <% if (!Settings.EnableSso)
                                { %> disabled <% } %> <% if (Settings.SpCertificateAdvanced.SignLogoutResponses) {  %> checked <% } %> >
                            <label for="ssoSignLogoutResponsesCbx" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                                <%= Resource.SsoSignLogoutResponses %>
                            </label>
                        </div>
                        <div class="clearFix">
                            <input type="checkbox" id="ssoEncryptAssertionsCbx" <% if (!Settings.EnableSso)
                                { %> disabled <% } %> <% if (Settings.SpCertificateAdvanced.EncryptAssertions) {  %> checked <% } %> >
                            <label for="ssoEncryptAssertionsCbx" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
                                <%= Resource.SsoDecryptAssertions %>
                            </label>
                        </div>
                        <div class="clearFix">
                            <span class=""><%= Resource.SsoSigningAlgorithm %>:</span>
                            <div class="clearFix algorithm-select-container">
                                <%=RenderSignVerifyingAlgorithmSelector("ssoSigningAlgorithm", Settings)%>
                            </div>
                        </div>
                        <div class="clearFix">
                            <span class=""><%= Resource.SsoDefaultDecryptAlgorithm %>:</span>
                            <div class="clearFix algorithm-select-container">
                                <%=RenderEncryptAlgorithmTypeSelector("ssoEncryptAlgorithm", Settings)%>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="sso-settings-block">
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoAttributeMapping %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoAttributeMappingHelper'});"></span>
                        <div class="popup_helper" id="ssoAttributeMappingHelper">
                            <p><%: Resource.SsoAttributeMappingHelper %></p>
                        </div>
                    </div>
                    <div>
                        <div class="sso-attribute-block requiredField">
                            <span id="ssoFirstNameError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                            <div>
                                <span class="sso-settings-text-title"><%= Resource.SsoFirstName %></span>
                            </div>
                            <input id="ssoFirstName" class="textEdit" type="text" value="<%=Settings.FieldMapping.FirstName %>" <% if (!Settings.EnableSso)
                                { %> disabled <% } %>>
                        </div>
                        <div class="sso-attribute-block requiredField">
                            <span id="ssoLastNameError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                            <div>
                                <span class="sso-settings-text-title"><%= Resource.SsoLastName %></span>
                            </div>
                            <input id="ssoLastName" class="textEdit" type="text" value="<%=Settings.FieldMapping.LastName %>" <% if (!Settings.EnableSso)
                                { %> disabled <% } %>>
                        </div>
                        <div class="sso-attribute-block requiredField">
                            <span id="ssoEmailError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                            <div>
                                <span class="sso-settings-text-title"><%= Resource.SsoEmail %></span>
                            </div>
                            <input id="ssoEmail" class="textEdit" type="text" value="<%=Settings.FieldMapping.Email %>" <% if (!Settings.EnableSso)
                                { %> disabled <% } %>>
                        </div>
                        <div class="sso-attribute-block requiredField">
                            <span id="ssoLocationError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                            <div>
                                <span class="sso-settings-text-title"><%= Resource.SsoLocation %></span>
                            </div>
                            <input id="ssoLocation" class="textEdit" type="text" value="<%=Settings.FieldMapping.Location %>" <% if (!Settings.EnableSso)
                                { %> disabled <% } %>>
                        </div>
                        <div class="sso-attribute-block requiredField">
                            <span id="ssoTitleError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                            <div>
                                <span class="sso-settings-text-title"><%= Resource.SsoTitle %></span>
                            </div>
                            <input id="ssoTitle" class="textEdit" type="text" value="<%=Settings.FieldMapping.Title %>" <% if (!Settings.EnableSso)
                                { %> disabled <% } %>>
                        </div>
                        <div class="sso-attribute-block requiredField">
                            <span id="ssoPhoneError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                            <div>
                                <span class="sso-settings-text-title"><%= Resource.SsoPhone %></span>
                            </div>
                            <input id="ssoPhone" class="textEdit" type="text" value="<%=Settings.FieldMapping.Phone %>" <% if (!Settings.EnableSso)
                                { %> disabled <% } %>>
                        </div>
                    </div>
                </div>

                <div class="sso-settings-advanced-container clearFix">
                    <div class="sso-settings-block">
                        <div class="sso-settings-text">
                            <span class="sso-settings-text-title">
                                <%= Resource.LdapAdvancedSettings %>
                            </span>
                        </div>
                        <div class="clearFix">
                            <label class="sso-settings-label-checkbox">
                                <input id="ssoHideAuthPage" type="checkbox" <% if (!Settings.EnableSso)
                                { %> disabled <% } %> <% if (Settings.HideAuthPage)
                                    {%> checked <%}%>><%= Resource.ssoHideAuthPage %>
                            </label>
                            <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoHideAuthPageHelper'});"></span>
                        </div>
                        <div class="popup_helper" id="ssoHideAuthPageHelper">
                             <p><%: Resource.SsoHideAuthPageHelper %></p>
                        </div>
                    </div>
                </div>

                <div class="middle-button-container">
                    <a class="button blue <% if (!Settings.EnableSso)
                        { %> disable <% } %>" id="ssoSaveBtn" title="<%= Resource.SaveButton %>"><%= Resource.SaveButton %></a>
                    <span class="splitter-buttons"></span>
                    <a id="ssoResetBtn" class="button gray <% if (!Settings.EnableSso)
                        { %> disable <% } %>" title="<%= Resource.SsoSettingsRestoreDefault %>"><%= Resource.SsoSettingsRestoreDefault %></a>

                </div>
            </div>


            <div class="top-indent-big">
                <div class="header-base sso-settings-title"><%= Resource.SsoSPMetadata.HtmlEncode() %></div>
                <a id="ssoSPMetadataSpoilerLink" class="link dotted sso-settings-spoiler-link" title="<%= !Settings.EnableSso ? Resource.Show : Resource.Hide %>"><%= !Settings.EnableSso ? Resource.Show : Resource.Hide %></a>
            </div>

            <div class=" <% if (!Settings.EnableSso)
                        { %> display-none <% } %>>" id="ssoSPMetadataSpoiler">
                <div class="sso-settings-block requiredField">
                    <span id="ssoSPEntityIdError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoSPEntityId %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoSPEntityIdHelper'});"></span>
                        <div class="popup_helper" id="ssoSPEntityIdHelper">
                            <p><%: Resource.SsoSPEntityIdHelper %></p>
                        </div>
                    </div>
                    <input id="ssoSPEntityId" class="textEdit blocked" type="text" value="" disabled="disabled">
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoSPConsumerUrlError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoSPConsumerUrl %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoSPConsumerUrlHelper'});"></span>
                        <div class="popup_helper" id="ssoSPConsumerUrlHelper">
                            <p><%: Resource.SsoSPConsumerUrlHelper %></p>
                        </div>
                    </div>
                    <input id="ssoSPConsumerUrl" class="textEdit blocked" type="text" value="" disabled="disabled">
                </div>

                <div class="sso-settings-block requiredField">
                    <span id="ssoSPLogoutUrlError" class="requiredErrorText"><%= Resource.SsoSettingsEmptyField %></span>
                    <div class="sso-settings-text">
                        <span class="sso-settings-text-title"><%= Resource.SsoSPLogoutUrl %>:</span>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'ssoSPLogoutUrlHelper'});"></span>
                        <div class="popup_helper" id="ssoSPLogoutUrlHelper">
                            <p><%: Resource.SsoSPLogoutUrlHelper %></p>
                        </div>
                    </div>
                    <input id="ssoSPLogoutUrl" class="textEdit blocked" type="text" value="" disabled="disabled">
                </div>
                <div class="middle-button-container">
                    <a id="ssoDownloadSPMetadataBtn" class="button blue <% if (!Settings.EnableSso)
                        { %> disable <% } %>" title="<%= Resource.SsoDownloadSPMetadata %>">
                        <%= Resource.SsoDownloadSPMetadata %></a>
                </div>
            </div>
        </div>

        <div class="settings-help-block">
            <% if (!isAvailable)
                { %>
                    <p>
                        <%= Resource.ErrorNotAllowedOption %>
                    </p>
                    <% if (TenantExtra.EnableTariffSettings)
                        { %>
                    <a href="<%= TenantExtra.GetTariffPageLink() %>" target="_blank">
                        <%= Resource.ViewTariffPlans %></a>
                    <% }
                }
                else
                { %>
                   <p><%= String.Format(Resource.SsoSettingsHelpNew.HtmlEncode(), "<b>", "</b>", String.Empty) %></p>
                <% if (!string.IsNullOrEmpty(HelpLink))
                    { %>
                        <p>
                            <a href="<%= HelpLink + "/administration/sso-settings-saas.aspx" %>"
                                target="_blank"><%= Resource.LearnMore %></a>
                        </p>
                <% } %>
                <%} %>

            
        </div>
    </div>
</div>


<div id="ssoSettingsTurnOffDialog" class="display-none">
    <sc:Container runat="server" ID="ssoSettingsTurnOffContainer">
        <Header>
            <div><%= Resource.ConfirmationTitle %></div>
        </Header>
        <Body>
            <%: Resource.ShortCancelConfirmMessage %>
            <div class="big-button-container">
                <a class="button blue middle sso-settings-ok"><%= Resource.OKButton %></a>
                <a class="button gray middle sso-settings-cancel"><%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<div id="ssoSettingsInviteDialog" class="display-none">
    <sc:Container runat="server" ID="ssoSettingsInviteContainer">
        <Header>
            <div><%= Resource.ConfirmationTitle %></div>
        </Header>
        <Body>
            <%: Resource.CancelConfirmMessage %>
            <div class="big-button-container">
                <a class="button blue middle sso-settings-ok"><%= Resource.OKButton %></a>
                <a class="button gray middle sso-settings-cancel"><%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<div id="ssoIdpCertificateDialog" class="display-none">
    <sc:Container runat="server" ID="ssoIdpCertificatePopupContainer">
        <Header>
            <div class="create-caption"><%= Resource.SsoNewCertificate %></div>
            <div class="edit-caption display-none"><%= Resource.SsoEditCertificate %></div>
        </Header>
        <Body>
            <div class="sso-settings-block requiredField">
                <div class="header-base-small">
                    <span class="requiredTitle"><%= Resource.SsoPublicCertificate %></span>
                </div>
                <textarea id="ssoIdpPublicCertificate" rows="3"></textarea>
            </div>
            <div class="sso-settings-block display-none">
                <div class="header-base-small"><%= Resource.SsoUsefor %></div>

                <select id="ssoIdpCertificateActionType" data-value="verification">
                    <option class="optionItem" value="verification"><%= Resource.SsoVerification %></option>
                    <option class="optionItem" value="decrypt"><%= Resource.SsoDecrypt %></option>
                    <option class="optionItem" value="verification and decrypt"><%= Resource.SsoVerificationAndDecrypt %></option>
                </select>

            </div>
            <div class="big-button-container">
                <a class="button blue middle sso-settings-ok" id="ssoIdpCertificateOkBtn"><%= Resource.OKButton %></a>
                <a class="button gray middle sso-settings-cancel"><%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>
<div id="ssoSpCertificateDialog" class="display-none">
    <sc:Container runat="server" ID="ssoSpCertificatePopupContainer">
        <Header>
            <div class="create-caption"><%= Resource.SsoNewCertificate %></div>
            <div class="edit-caption display-none"><%= Resource.SsoEditCertificate %></div>
        </Header>
        <Body>
            <div class="sso-settings-block requiredField">
                <div class="header-base-small">
                    <span class="requiredTitle"><%= Resource.SsoPublicCertificate %></span>
                    <a class="link dotted" id="ssoSpCertificateGenerateBtn"><%= Resource.SsoGenerateCertificate %></a>
                </div>
                <textarea id="ssoSpPublicCertificate" rows="3"></textarea>
            </div>
            <div class="sso-settings-block requiredField">
                <div class="header-base-small">
                    <span class="requiredTitle"><%= Resource.SsoPrivateKey %></span>
                </div>
                <textarea id="ssoSpPrivateKey" rows="3"></textarea>
            </div>
            <div class="sso-settings-block">
                <div class="header-base-small"><%= Resource.SsoUsefor %></div>

                <select id="ssoSpCertificateActionType" data-value="signing">
                    <option class="optionItem" value="signing"><%= Resource.SsoSigning %></option>
                    <option class="optionItem" value="encrypt"><%= Resource.SsoEncrypt %></option>
                    <option class="optionItem" value="signing and encrypt"><%= Resource.SsoSigningAndEncrypt %></option>
                </select>
            </div>
            <div class="big-button-container">
                <a class="button blue middle sso-settings-ok" id="ssoSpCertificateOkBtn"><%= Resource.OKButton %></a>
                <a class="button gray middle sso-settings-cancel"><%= Resource.CancelButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>

<script id="certificatesTmpl" type="text/x-jquery-tmpl">{{each(i, item) items}}{{tmpl(item) "#certificateItemTmpl"}}{{/each}}</script>
<script id="certificateItemTmpl" type="text/x-jquery-tmpl">
  <div class="crt-data-row clear-fix">{{if selfSigned}}<span class="domain">Self-Signed</span>{{else}}<span class="domain green">${domainName}</span>{{/if}}
    {{if valid}}<span class="expires">${startDateStr}-${expiredDateStr}</span>{{else}}<span class="expires red">${startDateStr}-${expiredDateStr}</span>{{/if}}<span class="action">${action}</span><a class="link dotted delete">delete</a><a class="link dotted edit">edit</a>
  </div>
</script>
<script id="ldapCrtErrorTmpl" type="text/x-jquery-tmpl">
  <div class="ldap-settings-crt-errors">{{each errors}}
    <div class="toast-container">
      <div class="toast toast-error" style="display: block;">
        <div class="toast-message">${message}</div>
      </div>
    </div>{{/each}
  </div>
</script>
