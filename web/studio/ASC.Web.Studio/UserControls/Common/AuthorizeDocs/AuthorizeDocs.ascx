<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthorizeDocs.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.AuthorizeDocs.AuthorizeDocs" %>
<%@ Import Namespace="System.Globalization" %>

<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="System.Threading" %>

<link href="https://fonts.googleapis.com/css?family=Roboto:500" rel="stylesheet">

<script id="personalReviewTmpl" type="text/x-jquery-tmpl">
    <div class="auth-form-with_review_i item">
        <div class="auth-form-with_review_photo">
            {{if photoUrl }}
            <img src="${photoUrl}" alt="${author}" />
            {{/if}}
        </div>
        <div class="auth-form-with_review_info">
            <span class="auth-form-with_review_author">${author}</span>
            <span class="auth-form-with_review_date">${date}</span>
        </div>
        <div class="auth-form-with_review_text">${value}</div>
    </div>
</script>

<div class="cookieMess">
    <div class="cookieMess_container">
        <p><%= String.Format(Resource.CookieMessText.HtmlEncode(), "<a target=\"_blank\" href=\"https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=5048502&doc=SXhWMEVzSEYxNlVVaXJJeUVtS0kyYk14YWdXTEFUQmRWL250NllHNUFGbz0_IjUwNDg1MDIi0&_ga=2.239950403.1196593722.1525950411-169631771.1404734630\">", "</span>") %></p>
        <div id="personalcookie" class="personalcookie">
            <a class="gotItBtn"><%= Resource.CookieMessButton %></a>
        </div>
    </div>
    <div class="closer_container">
        <div class="cookieMess_closer">&times;</div>
    </div>

</div>

<div class="auth-form-container">
    <div class="auth-form-head_w_background"></div>
    <div class="auth-form-head_w <%= Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName %>">
        <div class="auth-form-head">
            <h1 class="auth-form-header"><%= String.Format(Resource.AuthDocsTitle.HtmlEncode(), " ") %></h1>
            <div class="auth-form-with_try clearFix">
                <p class="auth-form-with_try_formats">
                    <%= Resource.AuthDocsCompatibleText %>
                <p class="auth-form-with_try_formats">
                    <%= Resource.AuthDocsRightBrowserText %>
                <p class="auth-form-with_try_formats">
                    <%= Resource.AuthDocsFreeChargeText %>
                </p>
            </div>
            <div class="default-personal-popup_btn open-form"><%= Resource.RegistryButtonStartEditing %></div>
            <div class="auth-form-with_form_w_anchor"></div>

            <div class="cloud-image"></div>
        </div>

    </div>
    <div class="auth-form-with auth-form-create-doc">
        <div class="create-doc-text-header-container">
            <p class="create-doc-text-header"><%= Resource.AuthDocsCreate %></p>
        </div>
        <div class="create-carousel">
            <div class="create-carousel-item">
                <p class="create-doc-text-title"><%= Resource.AuthDocsCreateSterlingDocuments %></p>
                <p class="create-doc-text-content"><%= Resource.AuthDocsCreateSterlingDocumentsText %></p>
            </div>
            <div class="create-carousel-item">
                <p class="create-doc-text-title"><%= Resource.AuthDocsCreateProfSpreadsheets %></p>
                <p class="create-doc-text-content"><%= Resource.AuthDocsCreateProfSpreadsheetsText %></p>
            </div>
            <div class="create-carousel-item">
                <p class="create-doc-text-title"><%= Resource.AuthDocsCreateCaptivatingPres %></p>
                <p class="create-doc-text-content"><%= Resource.AuthDocsCreateCaptivatingPresText %></p>
            </div>
        </div>
        <div class="slick-carousel">
            <div class="create-doc-picture doc"></div>
            <div class="create-doc-picture presentations"></div>
            <div class="create-doc-picture spreadsheets"></div>
        </div>
    </div>
    <div class="auth-form-with auth-form-capabilities">
        <div class="auth-form-capabilities-one">
            <div class="auth-form-capabilities-picture"></div>
            <p class="auth-form-capabilities-text">
                <%= Resource.AuthDocsCapabilitiesTextOne %>
            </p>
        </div>
        <div class="auth-form-capabilities-two">
            <div class="auth-form-capabilities-picture"></div>
            <p class="auth-form-capabilities-text">
                <%= Resource.AuthDocsCapabilitiesTextTwo %>
            </p>
        </div>
        <div class="auth-form-capabilities-three">
            <div class="auth-form-capabilities-picture"></div>
            <p class="auth-form-capabilities-text">
                <%= Resource.AuthDocsCapabilitiesTextThree %>
            </p>
        </div>
    </div>
    <div class="auth-form-with auth-form-share-collaborate">
        <p class="share-collaborate-header"><%= Resource.AuthDocsShareСollaborate %></p>

        <div class="share-collaborate-picture-carousel">
            <div class="scpc-item">

                <div class="share-collaborate-text-content active" id="Div1">
                    <%= Resource.AuthDocsShareСollaborateInviteUsers %>
                    <p class="share-collaborate-text-small-content">
                        <%= Resource.AuthDocsShareСollaborateInviteUsersText %>
                    </p>
                </div>
                <div class="collaborate1"></div>
            </div>
            <div class="scpc-item">

                <div class="share-collaborate-text-content" id="collaborate2">
                    <%= Resource.AuthDocsShareСollaborateSelectRight %>
                    <p class="share-collaborate-text-small-content">
                        <%= Resource.AuthDocsShareСollaborateSelectRightText %>
                    </p>
                </div>
                <div class="collaborate2"></div>
            </div>
            <div class="scpc-item">

                <div class="share-collaborate-text-content" id="collaborate3">
                    <%= Resource.AuthDocsShareСollaborateDiscuss %>
                    <p class="share-collaborate-text-small-content">
                        <%= Resource.AuthDocsShareСollaborateDiscussText %>
                    </p>
                </div>
                <div class="collaborate3"></div>
            </div>
            <div class="scpc-item">

                <div class="share-collaborate-text-content" id="collaborate4">
                    <%= Resource.AuthDocsShareСollaborateReviewResults %>
                    <p class="share-collaborate-text-small-content">
                        <%= Resource.AuthDocsShareСollaborateReviewResultsText %>
                    </p>
                </div>
                <div class="collaborate4"></div>
            </div>
            <div class="scpc-item">

                <div class="share-collaborate-text-content" id="collaborate5">
                    <%= Resource.AuthDocsShareСollaborateNavigateHistory %>
                    <p class="share-collaborate-text-small-content">
                        <%= Resource.AuthDocsShareСollaborateNavigateHistoryText %>
                    </p>
                </div>
                <div class="collaborate5"></div>
            </div>
            <div class="scpc-item">

                <div class="share-collaborate-text-content" id="collaborate6">
                    <%= Resource.AuthDocsShareСollaborateEmberDocuments %>
                    <p class="share-collaborate-text-small-content">
                        <%= Resource.AuthDocsShareСollaborateEmberDocumentsText %>
                    </p>
                </div>
                <div class="collaborate6"></div>
            </div>
        </div>


    </div>
    <div class="auth-form-with auth-form-extend">
        <div class="auth-form-extend-content">
            <p class="extend-header"><%= Resource.AuthDocsExtend %></p>
            <div class="extend-keeping">
                <%= String.Format(Resource.AuthDocsExtendKeeping, "<span class=\"extend-href\">DropBox</span>", "<span class=\"extend-href\">Google Drive</span>") %>
            </div>
            <p class="extend-text">
                <%= Resource.AuthDocsExtendKeepingText %>
            </p>
            <div class="third-party-container">
                <div class="third-party google"></div>
                <div class="third-party dropbox"></div>
                <div class="third-party owncloud"></div>
                <div class="third-party nextcloud"></div>
                <div class="third-party yandex"></div>
                <div class="third-party onedrive"></div>
            </div>
        </div>
    </div>
    <div class="auth-form-with auth-form-access">
        <div class="auth-form-access-container">
            <p class="access-header"><%= Resource.AuthDocsAccess %></p>
            <div class="access-text">
                <p class="access-text-header"><%= Resource.AuthDocsAccessOnline %></p>
                <p class="access-text-content">
                    <%= Resource.AuthDocsAccessOnlineText %>
                </p>
            </div>
            <div class="access-text">
                <p class="access-text-header"><%= Resource.AuthDocsAccessOffline %></p>
                <p class="access-text-content">
                    <%= String.Format(Resource.AuthDocsAccessOfflineText, "<a class=\"access-href\" href=\"https://www.onlyoffice.com/download-desktop.aspx\">" + Resource.AuthDocsAccessOnlyofficeDesktop + "</a>") %>
                </p>
            </div>
            <div class="access-text">
                <p class="access-text-header"><%= Resource.AuthDocsAccessOnMobile %></p>
                <p class="access-text-content">
                    <%= Resource.AuthDocsAccessAndroidIOSText %>
                </p>
                <a href="https://itunes.apple.com/app/onlyoffice-documents/id944896972?mt=8" class="access-apple"></a>
                <a href="https://play.google.com/store/apps/details?id=com.onlyoffice.documents" class="access-android"></a>

            </div>
        </div>
    </div>
    <div class="auth-form-with auth-form-customer-experience clearFix">
        <div class="auth-form-customer-experience-container">
            <div class="customer-experience-text">
                <p class="customer-experience-header"><%= Resource.AuthDocsCustomerExperience %></p>
            </div>
            <div class="customer-experience-recall">
                <div class="auth-form-with_review">
                    <div class="carousel-wrapper">
                        <div id="reviewsContainer" class="carousel-items carousel" data-lng="<%= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName %>"></div>
                    </div>
                </div>
            </div>
        </div>

    </div>
</div>

<% if (RecaptchaEnable)
   { %>
<script src="https://www.google.com/recaptcha/api.js?hl=<%= CultureInfo.CurrentCulture.TwoLetterISOLanguageName %>" ></script>

<input type="hidden" id="recaptchaData" value="<%= SetupInfo.RecaptchaPublicKey %>" />
<% } %>

<!--Registration form-->
<div class="auth-form-with_form_w">
    <div class="auth-form-with_form clearFix">
        <div class="register_form_closer">&times;</div>
        <span class="auth-form-with_form_title"><%= Resource.CreatePersonalButton %></span>
        <div id="confirmEmailError" class="auth-form-with_form_error"></div>
        <input id="confirmEmail" class="auth-form-with_field_email" type="email" placeholder="<%= Resource.AuthDocsEmailHint %>" />

        <div class="auth-form-settings">
            <div class="auth-form-setting">
                <label>
                    <input type="checkbox" id="agree_to_terms" hidden><span></span>
                    <label for="agree_to_terms">
                        <%= String.Format(Resource.RegistrySettingTerms.HtmlEncode(),
                                                                  "<a target=\"_blank\" href=" + CommonLinkUtility.ToAbsolute("~/Terms.aspx?lang=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) + ">", "</a>",
                                                                  "<a target=\"_blank\" href=\"https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=5048502&doc=SXhWMEVzSEYxNlVVaXJJeUVtS0kyYk14YWdXTEFUQmRWL250NllHNUFGbz0_IjUwNDg1MDIi0&_ga=2.248662407.1867121315.1526272545-169631771.1404734630\">", "</a>") %></label>
                </label>
            </div>
            <div class="auth-form-setting">
                <label>
                    <input type="checkbox" id="spam" name="spam" hidden checked="checked"><span></span>
                    <label for="spam"><%= Resource.RegistrySettingSpam %></label>
                </label>

            </div>
            <div class="auth-form-setting">
                <label>
                    <input type="checkbox" id="analytics" name="analytics" hidden checked="checked"><span></span>
                    <label for="analytics"><%= Resource.RegistrySettingAnalytics %></label>
                </label>

            </div>
        </div>

        <% if (RecaptchaEnable) { %>
        <div id="recaptchaEmail" class="recaptcha-container"></div>
        <% } %>

        <a id="confirmEmailBtn" class="auth-form-with_form_btn disabled"><%= Resource.RegistryButtonCreateNow %></a>

        <% if (ThirdpartyEnable)
           { %>
        <span class="auth-form-with_form_or"><%= Resource.AuthDocsOr %></span>
        <span class="auth-form-with_btns_social auth-docs_btns_social">
            <asp:PlaceHolder runat="server" ID="HolderLoginWithThirdParty"></asp:PlaceHolder>
        </span>
        <% } %>
    </div>
</div>
<!-- ------ -->

<div id="sendEmailSuccessPopup" class="default-personal-popup">
    <div class="default-personal-popup_cnt">
        <div class="default-personal-popup_closer">&times;</div>
        <div class="default-personal-popup_text"><%: Resource.AuthDocsThanksRegistration %></div>
        <div class="default-personal-popup_text small-text"><%= Resource.AuthDocsSendActivationEmail %> '<span id="activationEmail"></span>'</div>
        <div class="default-personal-popup_strong-text"><%= Resource.AuthDocsCheckEmail %></div>
    </div>
</div>

<div id="loginPopup" class="default-personal-popup login">
    <div class="default-personal-popup_cnt">
        <div class="default-personal-popup_closer">&times;</div>
        <h2 class="default-personal-popup_h"><%= Resource.PersonalLogin %></h2>
        <div class="default-personal-popup_form-i">
            <label class="default-personal-popup_label email"><%= Resource.Email %></label>
            <div class="login_field-cnt">
                <input type="email" id="login" name="login" maxlength="255" class="default-personal-popup_field"
                    <%= String.IsNullOrEmpty(Login) ? "" : ("value=\"" + Login.HtmlEncode() + "\"") %> />
                <input type="hidden" class="login-message" value="<%= LoginMessage %>" data-type="<%= LoginMessageType %>" />
            </div>
            <span class="default-personal-popup_check"></span>

        </div>
        <div class="default-personal-popup_form-i">
            <label class="default-personal-popup_label password"><%= Resource.Password %></label>
            <input class="default-personal-popup_field password" id="pwd" maxlength="<%= PasswordSettings.MaxLength %>" type="password" />
            <input type="hidden" id="passwordHash" name="passwordHash" />

            <% if (Request.DesktopApp())
               { %>
            <span class="desktop-personal-popup_check">
                <label>
                    <input type="checkbox" class="checkbox" id="desktop_agree_to_terms" hidden>
                    <span class="checkbox-custom"></span>
                    <label>
                        <%= String.Format(Resource.RegistrySettingTerms.HtmlEncode(),
                                                 "<a target=\"_blank\" href=" + CommonLinkUtility.ToAbsolute("~/Terms.aspx?lang=" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) + ">", "</a>",
                                                 "<a target=\"_blank\" href=\"https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=5048502&doc=SXhWMEVzSEYxNlVVaXJJeUVtS0kyYk14YWdXTEFUQmRWL250NllHNUFGbz0_IjUwNDg1MDIi0&_ga=2.248662407.1867121315.1526272545-169631771.1404734630\">", "</a>") %></label>
                </label>
            </span>
            <% } %>
            <span class="default-personal-popup_check">
                <label>
                    <input type="checkbox" class="checkbox" checked />
                    <span class="checkbox-custom"></span>
                    <span class="label"><%= Resource.Remember %></span>
                </label>
            </span>
        </div>

        <% if (RecaptchaEnable && ShowRecaptcha)
           { %>
        <div id="recaptchaLogin" class="recaptcha-container"></div>
        <% } %>

        <div class="login_accept-btn-cnt">
            <button id="loginBtn" class="default-personal-popup_btn <%= Request.DesktopApp() ? "disabled" : "" %>"
                onclick="return false;"><%= Resource.AuthDocsLogIn %></button>
            <span class="login_forget-psw" onclick="PasswordTool.ShowPwdReminderDialog();"><%= Resource.AuthDocsForgotPswd %></span>
        </div>

        <% if (ThirdpartyEnable)
           { %>
        <div class="popup_social-nets auth-docs_btns_social">
            <div class="auth-docs_social_text"><%= Resource.AuthDocsEnterViaSocial %></div>
            <asp:PlaceHolder ID="LoginSocialNetworks" runat="server" />
        </div>
        <% } %>
        <div class="login_not-registered-cnt">
            <span><%= Resource.AuthDocsNotRegistered %></span>
            <span id="loginSignUp" class="login_signup-btn"><%= Resource.AuthDocsSignUp %></span>
        </div>
    </div>
    <asp:PlaceHolder ID="pwdReminderHolder" runat="server" />
</div>

<asp:PlaceHolder runat="server" ID="PersonalFooterHolder"></asp:PlaceHolder>
