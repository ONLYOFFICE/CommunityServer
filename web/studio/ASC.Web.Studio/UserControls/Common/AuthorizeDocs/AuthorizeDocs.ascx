<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthorizeDocs.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.AuthorizeDocs.AuthorizeDocs" %>
<%@ Import Namespace="System.Globalization" %>

<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="System.Threading" %>

<link href="https://fonts.googleapis.com/css?family=Roboto:500" rel="stylesheet">

<div class="cookieMess">
    <div class="cookieMess_container">
        <p><%= String.Format(PersonalResource.CookieMessText.HtmlEncode(), "<a target=\"_blank\" href=\"https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=5048502&doc=SXhWMEVzSEYxNlVVaXJJeUVtS0kyYk14YWdXTEFUQmRWL250NllHNUFGbz0_IjUwNDg1MDIi0&_ga=2.239950403.1196593722.1525950411-169631771.1404734630\">", "</a>") %></p>
        <div id="personalcookie" class="personalcookie">
            <a class="gotItBtn"><%= PersonalResource.CookieMessButton %></a>
        </div>
    </div>
</div>

<div class="auth-form-container <%= Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName %>" <% if (DontShowMainPage) {%> style="display: none;" <% }%>>
    <div class="auth-form-head_w <%= Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName %>">
        <div class="first-screen-content">
            <div class="auth-form-head">
                <h1 class="auth-form-header"><%= String.Format(PersonalResource.AuthDocsTitle.HtmlEncode(), " ") %></h1>
                <p class="auth-form-subheader"><%= PersonalResource.AuthDocsSubtitle %></p>
            </div>
            <!--Registration form-->
            <div class="auth-form-with_form_w">
                <div class="auth-form-with_form clearFix">
                    <h2 class="auth-form-with_form_title"><%= PersonalResource.CreatePersonalButton %></h2>
                    <div class="auth-input-row">
                        <div class="auth-input-wrapper">
                            <input id="confirmEmail" class="auth-form-with_field_email" type="email" placeholder="<%= PersonalResource.AuthDocsEmailHint %>" />
                            <label for="confirmEmail"><%= PersonalResource.AuthDocsYourEmail %></label>
                        </div>
                        <div id="confirmEmailError" class="auth-form-with_form_error"></div>

                        <a id="confirmEmailBtn" class="auth-form-with_form_btn btn-form disabled"><%= PersonalResource.RegistryButtonCreateNow %></a>

                    </div>

                    <div class="auth-form-settings">
                        <div class="auth-form-setting checkbox-conainer">
                            <label for="">
                                <input type="checkbox" id="spam" name="spam" hidden checked="checked"><span data-span-for="spam"></span>
                                <label for="spam"><%= PersonalResource.RegistrySettingSpam %></label>
                            </label>

                        </div>
                        <div class="auth-form-setting checkbox-conainer">
                            <label for="">
                                <input type="checkbox" id="agree_to_terms" hidden><span data-span-for="agree_to_terms"></span>
                                <label for="agree_to_terms">
                                    <%= String.Format(PersonalResource.RegistrySettingTerms.HtmlEncode(),
                                                                  "<a target=\"_blank\" href=\"" + CommonLinkUtility.ToAbsolute("~/Terms.aspx?lang=" + CultureInfo.CurrentUICulture.Name) + "\">", "</a>",
                                                                  "<a target=\"_blank\" href=\"https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=5048502&doc=SXhWMEVzSEYxNlVVaXJJeUVtS0kyYk14YWdXTEFUQmRWL250NllHNUFGbz0_IjUwNDg1MDIi0&_ga=2.248662407.1867121315.1526272545-169631771.1404734630\">", "</a>") %></label>
                            </label>
                        </div>
                    </div>

                    <% if (RecaptchaEnable)
                       { %>
                    <div id="recaptchaEmail" class="recaptcha-container"></div>
                    <% } %>

                    <% if (ThirdpartyEnable)
                       { %>
                    <span class="or-conainer"><%= PersonalResource.AuthDocsOr %></span>
                    <span class="auth-form-with_btns_social auth-docs_btns_social">
                        <asp:PlaceHolder runat="server" ID="HolderLoginWithThirdParty"></asp:PlaceHolder>
                    </span>
                    <% } %>
                </div>
                <div id="personalAccountLogin">
                    <div class="prebtn"><%= PersonalResource.AuthDocsAlready %></div>
                    <a><%= PersonalResource.AuthDocsLogIn %></a>
                </div>
            </div>
        </div>
    </div>
    <div class="auth-form-with auth-form-create-doc">
        <div class="carousel-container">
            <div class="create-carousel">
                <div class="create-carousel-item">
                    <h3 class="create-doc-text-title"><%= PersonalResource.AuthDocsPowerfulTexts %></h3>
                    <p class="create-doc-text-content"><%= PersonalResource.AuthDocsPowerfulTextSub %></p>
                </div>
                <div class="create-carousel-item">
                    <h3 class="create-doc-text-title"><%= PersonalResource.AuthDocsPowerfulSheets %></h3>
                    <p class="create-doc-text-content"><%= PersonalResource.AuthDocsPowerfulSheetsSub %></p>
                </div>
                <div class="create-carousel-item">
                    <h3 class="create-doc-text-title"><%= PersonalResource.AuthDocsPowerfulSlides %></h3>
                    <p class="create-doc-text-content"><%= PersonalResource.AuthDocsPowerfulSlidesSub %></p>
                </div>
                <div class="create-carousel-item">
                    <h3 class="create-doc-text-title"><%= PersonalResource.AuthDocsPowerfulCollaboration %></h3>
                    <p class="create-doc-text-content"><%= PersonalResource.AuthDocsPowerfulCollaborationSub %></p>
                </div>
            </div>
        </div>
        <div class="slick-carousel">
            <div class="create-doc-picture spreadsheet"></div>
            <div class="create-doc-picture doc"></div>
            <div class="create-doc-picture collaboration"></div>
            <div class="create-doc-picture presentation"></div>
        </div>
    </div>
    <div class="auth-form-with auth-form-extend">
        <h3><%= PersonalResource.AuthDocsConnect %></h3>
        <div class="third-party-container">
            <div>
                <div class="third-party google"></div>
            </div>
            <div>
                <div class="third-party onedrive"></div>
            </div>
            <div>
                <div class="third-party dropbox"></div>
            </div>
            <div>
                <div class="third-party owncloud"></div>
            </div>
            <div>
                <div class="third-party nextcloud"></div>
            </div>
            <div>
                <div class="third-party yandex"></div>
            </div>
        </div>
    </div>
    <div class="auth-downloads-block">
        <h3><%= PersonalResource.AuthDocsOnlineDesktopMobile %></h3>
        <div class="auth-download-block d-chrome-extension">
            <div class="auth-download-txt">
                <b><%= PersonalResource.AuthDocsAccessQuickly %></b> <%= PersonalResource.AuthDocsUsingChrome %>
                <div>
                    <a class="button" href="https://chrome.google.com/webstore/detail/onlyoffice/ohdlcmpahmacjddiaokoogleflddlahc"></a>
                </div>
            </div>
            <div class="auth-download-img"></div>
        </div>
        <div class="auth-download-block d-desktop-app">
            <div class="auth-download-txt">
                <%= String.Format(PersonalResource.AuthDocsFreeDesktopApp.HtmlEncode(),
                    "<b>",
                    "</b>") %>
                <div>
                    <a class="button" href="<%="https://www.onlyoffice.com/" + (CultureInfo.CurrentCulture.TwoLetterISOLanguageName) + "/download-desktop.aspx"%>" target="_blank"><%= PersonalResource.AuthDocsDownload %></a>
                </div>
            </div>
            <div class="auth-download-img"></div>
        </div>
        <div class="auth-download-block d-apps">
            <div class="auth-download-txt">
                <%= String.Format(PersonalResource.AuthDocsWorkWithDocs.HtmlEncode(),
                    "<span class=\"nowrapping\">",
                    "</span>",
                    "<b>",
                    "</b>") %>
                <div>
                    <a href="https://itunes.apple.com/app/onlyoffice-documents/id944896972?mt=8" class="app-store"></a>
                    <a href="https://play.google.com/store/apps/details?id=com.onlyoffice.documents" class="google-play"></a>
                </div>
            </div>
            <div class="auth-download-img"></div>
        </div>
    </div>
    <div class="auth-quote-block">
        <div class="auth-quote-container">
            <p>
                <%= PersonalResource.AuthDocsSoftpediaQuote %>
            </p>
            <a href="https://webapps.softpedia.com/app/OnlyOffice-Personal/" target="_blank"><%= PersonalResource.AuthDocsSoftpedia %></a>
        </div>
    </div>
    <div class="heartheweb-block">
        <div class="heartheweb-container">
            <h3><%= PersonalResource.AuthDocsHearTheWeb %></h3>
            <div class="heartheweb_reviews_list"></div>
        </div>
        <div class="review-disclaimer">
            <%= String.Format(PersonalResource.AuthDocsDisclaimer.HtmlEncode(),
                    "<a href=\"https://www.capterra.com/\" target=\"_blank\">",
                    "</a>") %>
        </div>
    </div>
    <script id="heartheweb-reviews-template-list-item" type="text/template">
        <div class="review_block review_block_{{data-order}}" data-order="{{data-order}}">
            <div class="review_top">
                <div class="review_block_author_name">{{author-name}}</div>
                <div>{{rating}}</div>
            </div>
            <blockquote>
                <div>{{review-heading}}</div>
                <div class="review_block_text">"{{review-text}}"</div>
            </blockquote>
        </div>
    </script>
</div>

<% if (RecaptchaEnable)
   { %>
<script src="https://www.recaptcha.net/recaptcha/api.js?hl=<%= CultureInfo.CurrentCulture.TwoLetterISOLanguageName %>"></script>

<input type="hidden" id="recaptchaData" value="<%= SetupInfo.RecaptchaPublicKey %>" />
<% } %>

<div id="sendEmailSuccessPopup" class="default-personal-popup">
    <div class="default-personal-popup_cnt">
        <h2><%: PersonalResource.AuthDocsThanksRegistration %></h2>
        <div class="email-success-text">
            <div><%= PersonalResource.AuthDocsSendActivationEmail %> <span id="activationEmail"></span></div>
            <div><%= PersonalResource.AuthDocsCheckEmail %></div>
        </div>
        <a id="goToMainPage" class="btn-form"><%= PersonalResource.AuthDocsToMainPage %></a>
    </div>
</div>

<div id="passwordRecovery" class="default-personal-popup">
    <label class="email" for="studio_emailPwdReminder"><%= PersonalResource.Email %></label>
    <div class="link-as-btn"><span class="back-to-login"><%= PersonalResource.AuthDocsToLogin %></span></div>
</div>

<div id="loginPopup" class="default-personal-popup login" <% if (DontShowMainPage) {%> style="display: block;" <% }%>>
    <div class="default-personal-popup_cnt">
        <h2 class="default-personal-popup_h"><%= PersonalResource.PersonalLogin %></h2>
        <div class="default-personal-popup_form-i">
            <div class="auth-input-wrapper">
                <input type="email" id="login" name="login" maxlength="255"
                    <%= String.IsNullOrEmpty(Login) ? "" : ("value=\"" + Login.HtmlEncode() + "\"") %> />
                <input type="hidden" class="login-message" value="<%= LoginMessage %>" data-type="<%= LoginMessageType %>" />
                <label class="email"><%= PersonalResource.Email %></label>
            </div>
            <span class="default-personal-popup_check"></span>

        </div>
        <div class="default-personal-popup_form-i">
            <div class="auth-input-wrapper">
                <input class="password" id="pwd" maxlength="<%= PasswordSettings.LimitMaxLength %>" type="password" />
                <input type="hidden" id="passwordHash" name="passwordHash" />
                <label class="password"><%= Resource.Password %></label>
            </div>
        </div>

        <% if (Request.DesktopApp())
           { %>
        <span class="checkbox-conainer">
            <label for="">
                <input type="checkbox" class="checkbox" id="desktop_agree_to_terms" hidden>
                <span class="checkbox-custom" data-span-for="desktop_agree_to_terms"></span>
                <label for="desktop_agree_to_terms">
                    <%= String.Format(PersonalResource.RegistrySettingTerms.HtmlEncode(),
                                                 "<a target=\"_blank\" href=\"" + CommonLinkUtility.ToAbsolute("~/Terms.aspx?lang=" + CultureInfo.CurrentUICulture.Name) + "\">", "</a>",
                                                 "<a target=\"_blank\" href=\"https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=5048502&doc=SXhWMEVzSEYxNlVVaXJJeUVtS0kyYk14YWdXTEFUQmRWL250NllHNUFGbz0_IjUwNDg1MDIi0&_ga=2.248662407.1867121315.1526272545-169631771.1404734630\">", "</a>") %></label>
            </label>
        </span>
        <% } %>
        <span class="checkbox-conainer">
            <label for="">
                <input type="checkbox" id="rememberCheckbox" class="checkbox" hidden checked />
                <span class="checkbox-custom" data-span-for="rememberCheckbox"></span>
                <label class="label" for="rememberCheckbox"><%= Resource.Remember %></label>
            </label>
        </span>

        <% if (RecaptchaEnable && ShowRecaptcha)
           { %>
        <div id="recaptchaLogin" class="recaptcha-container"></div>
        <% } %>

        <div class="login_accept-btn-cnt">
            <button id="loginBtn" class="default-personal-popup_btn btn-form <%= Request.DesktopApp() ? "disabled" : "" %>"
                onclick="return false;">
                <%= PersonalResource.AuthDocsSignIn %>
            </button>
            <div class="link-as-btn"><span class="login_forget-psw"><%= PersonalResource.AuthDocsForgotPswd %></span></div>
        </div>

        <% if (ThirdpartyEnable)
           { %>
        <div class="or-conainer"><%= PersonalResource.AuthDocsEnterViaSocial %></div>
        <div class="popup_social-nets auth-docs_btns_social">
            <asp:PlaceHolder ID="LoginSocialNetworks" runat="server" />
        </div>
        <% } %>
    </div>
    <asp:PlaceHolder ID="pwdReminderHolder" runat="server" />
    <div id="personalCreateNow">
        <div class="prebtn"><%= PersonalResource.AuthDocsDontHave %></div>
        <a><%= PersonalResource.RegistryButtonCreateNow %></a>
    </div>
</div>

<asp:PlaceHolder runat="server" ID="PersonalFooterHolder"></asp:PlaceHolder>
