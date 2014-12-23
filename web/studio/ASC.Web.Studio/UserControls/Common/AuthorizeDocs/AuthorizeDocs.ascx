<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthorizeDocs.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.AuthorizeDocs.AuthorizeDocs" %>
<%@ Import Namespace="System.Globalization" %>

<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="System.Threading" %>

<div>
    <div class="auth-form-head_w <%= Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName %>">
        <div class="auth-form-head">
            <h1 class="auth-form-header"><%= String.Format(Resource.AuthDocsTitle, "<br />") %></h1>
            <div class="auth-form-with_try clearFix">
                <p class="auth-form-with_try_formats"><%= Resource.AuthDocsEditText %>
                    <span id="chromebookText"><%= String.Format(Resource.AuthDocsChromebook, "<span class=\"chromebook\">", "</span>") %></span>
                </p>
            </div>
            <div class="auth-form-with_sign-up">
                <div class="auth-form-with_hint"><%= Resource.RegistryButton %></div>
                <div class="auth-form-with_hint-add"><%= Resource.AuthDocsItsFree %></div>
                <div class="auth-form-with_lines"></div>
            </div>
            <div class="auth-form-with_form_w">
                <div class="auth-form-with_form clearFix">
                    <div id="confirmEmailError" class="auth-form-with_form_error"></div>
                    <input id="confirmEmail" class="auth-form-with_field" type="text" placeholder="<%= Resource.AuthDocsEmailHint %>" />
                    <a id="confirmEmailBtn" class="auth-form-with_form_btn" data-campaign="<%= Session["campaign"] %>" ><%=Resource.RegistryButton %></a>
                     <% if (SetupInfo.ThirdPartyAuthEnabled) {%>
                    <span class="auth-form-with_form_or"><%= Resource.AuthDocsOr %></span>
                    <span class="auth-form-with_btns_social auth-docs_btns_social">
                        <asp:PlaceHolder runat="server" ID="HolderLoginWithThirdParty"></asp:PlaceHolder>
                    </span>
                    <%} %>
                </div>
            </div>
            <div class="cloud-image"></div>
        </div>
        
    </div>
    <div class="auth-form-with clearFix">



        <div class="auth-form-with_review">
            <h3 class="auth-form-with_review_h"><%= Resource.AuthDocsChromeStore %></h3>
            <span id="cromeStoreRating"></span>
            <div class="carousel-wrapper">
                <div id="reviewsContainer" class="carousel-items" data-lng="<%= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName%>"></div>
            </div>
        </div>

    </div>
</div>

<script id="personalReviewTmpl" type="text/x-jquery-tmpl">
    <div class="auth-form-with_review_i carousel-block">
            <img src="${photoUrl}" class="auth-form-with_review_photo" />
            <div  class="auth-form-with_review_info">
                <span class="auth-form-with_review_author">${author}</span>
                <span class="auth-form-with_review_date">${date}</span>
                <span class="auth-form-with_review_rating">
                    {{each(i, star) stars}}
                    <span class="auth-form-with_review_star"></span>
                    {{/each}}
                </span>
            </div>
            <div class="auth-form-with_review_text">${value}</div>
     </div>
</script>

<div id="sendEmailSuccessPopup" class="default-personal-popup">
    <div class="default-personal-popup_cnt">
        <div class="default-personal-popup_closer">&times</div>
        <div class="default-personal-popup_text"><%=Resource.AuthDocsThanksRegistration %></div>
        <div class="default-personal-popup_text"><%= Resource.AuthDocsSendActivationEmail %> '<span id="activationEmail"></span>'</div>
        <div class="default-personal-popup_strong-text"><%=Resource.AuthDocsCheckEmail %></div>
    </div>
</div>

<div id="loginPopup" class="default-personal-popup login">
    <div class="default-personal-popup_cnt">
        <div class="default-personal-popup_closer">&times</div>
        <h2 class="default-personal-popup_h"><%= Resource.Login %></h2>
        <div class="default-personal-popup_line"></div>
        <div class="default-personal-popup_form-i">
            <label class="default-personal-popup_label"><%= Resource.Email %></label>
            <div class="login_field-cnt">
                <input type="email" id="login" name="login" maxlength="255" class="default-personal-popup_field"
                    placeholder="<%= Resource.RegistrationEmailWatermark %>" 
                    <%= String.IsNullOrEmpty(Login) ? "" : ("value=\"" + Login.HtmlEncode() + "\"") %> />
                <input type="hidden" class="login-message" value="<%= LoginMessage %>" data-type="<%= LoginMessageType %>"/>
            </div>
            <span class="default-personal-popup_check"></span>
            
        </div>
        <div class="default-personal-popup_form-i">
            <label class="default-personal-popup_label"><%= Resource.Password %></label>
            <input class="default-personal-popup_field" id="pwd" name="pwd" maxlength="64" placeholder="<%= Resource.Password %>" type="password" />
            <span class="default-personal-popup_check">
                <label>
                    <input type="checkbox" checked /><%= Resource.Remember %></label>
            </span>
        </div>

        <div class="login_accept-btn-cnt">
            <button class="default-personal-popup_btn" type="submit"><%= Resource.OKButton %></button>
            <span class="login_forget-psw" onclick="PasswordTool.ShowPwdReminderDialog();"><%= Resource.AuthDocsForgotPswd %></span>
        </div>
        <div class="login_not-registered-cnt">
            <span><%= Resource.AuthDocsNotRegistered %></span>
            <span id="loginSignUp" class="login_signup-btn"><%= Resource.AuthDocsSignUp %></span>
        </div>
        <% if (SetupInfo.ThirdPartyAuthEnabled)
           {%>
        <div class="popup_social-nets auth-docs_btns_social">
            <div class="auth-docs_social_text"><%= Resource.AuthDocsEnterViaSocial %></div>
            <asp:PlaceHolder ID="LoginSocialNetworks" runat="server" />
        </div>
        <% } %>
    </div>
    <asp:PlaceHolder ID="pwdReminderHolder" runat="server" />
</div>


<asp:PlaceHolder runat="server" ID="PersonalFooterHolder"></asp:PlaceHolder>


