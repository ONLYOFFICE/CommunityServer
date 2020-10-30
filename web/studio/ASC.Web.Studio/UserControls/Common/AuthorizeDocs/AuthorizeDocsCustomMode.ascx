<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AuthorizeDocs.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.AuthorizeDocs.AuthorizeDocs" %>
<%@ Import Namespace="System.Globalization" %>

<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
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
        <div  class="auth-form-with_review_info">
            <span class="auth-form-with_review_author">${author}</span>
            <span class="auth-form-with_review_date">${date}</span>
        </div>
        <div class="auth-form-with_review_text">${value}</div>
    </div>
</script>

<div class="auth-form-container">
    <div class="auth-form-head_w_background"></div>
    <div class="auth-form-head_w <%= Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName %>">
        <div class="auth-form-head">
            <h1 class="auth-form-header"><%= String.Format(CustomModeResource.TitleCustomMode.HtmlEncode(), " ") %></h1>
            <div class="auth-form-with_try clearFix">
                <p class="auth-form-with_try_formats"><%= CustomModeResource.CompatibleTextCustomMode %></p>
                <p class="auth-form-with_try_formats"><%= CustomModeResource.CollaborativeEditingTextCustomMode %></p>
                <p class="auth-form-with_try_formats"><%= CustomModeResource.BrowserAndDesktopTextCustomMode %></p>
            </div>
            <div class="default-personal-popup_btn open-form"><%= CustomModeResource.CreateOfficeCustomMode %></div>
            <div class="auth-form-with_form_w_anchor"></div>
           
            <div class="cloud-image"></div>
        </div>
    </div>
    <div class="auth-form-with auth-form-create-doc">
        <div class="create-doc-text-header-container">
             <p class="create-doc-text-header"><%= CustomModeResource.OnlineVersionCustomMode %></p>
        </div>
        <div class="create-carousel">
            <div class="create-carousel-item">
                <p class="create-doc-text-title"><%= CustomModeResource.DocumentEditorCustomMode %></p>
                <p class="create-doc-text-content"><%= CustomModeResource.DocumentEditorDescriptionCustomMode %></p>
            </div>
            <div class="create-carousel-item">
                <p class="create-doc-text-title"><%= CustomModeResource.SpreadsheetEditorCustomMode %></p>
                <p class="create-doc-text-content"><%= CustomModeResource.SpreadsheetEditorDescriptionCustomMode %></p>
            </div>
            <div class="create-carousel-item">
                <p class="create-doc-text-title"><%= CustomModeResource.PresentationEditorCustomMode %></p>
                <p class="create-doc-text-content"><%= CustomModeResource.PresentationEditorDescriptionCustomMode %></p>
            </div>
        </div>
        <div class="slick-carousel">
            <div class="create-doc-picture doc"></div>
            <div class="create-doc-picture presentations"></div>
            <div class="create-doc-picture spreadsheets"></div>
        </div>
    </div>
    <div class="auth-form-with auth-form-share-collaborate">
        <p class="share-collaborate-header"><%= CustomModeResource.CollaborativeWorkCustomMode %></p>
       
        <div class="share-collaborate-picture-carousel">
            <div class="scpc-item" >
                <div class="share-collaborate-text-content" id="collaborate2"><%= CustomModeResource.TwoEditingModesCustomMode %>
                    <p class="share-collaborate-text-small-content">
                        <%= CustomModeResource.TwoEditingModesDescriptionCustomMode %>
                    </p>
                </div>
                <div class="collaborate2"></div>
            </div>
            <div class="scpc-item" >
                
                <div class="share-collaborate-text-content" id="collaborate3"><%= CustomModeResource.DiscussionDocumentCustomMode %>
                    <p class="share-collaborate-text-small-content">
                        <%= CustomModeResource.DiscussionDocumentDescriptionCustomMode %>
                    </p>
                </div>
                <div class="collaborate3"></div>
            </div>
            <div class="scpc-item" >
                <div class="share-collaborate-text-content" id="collaborate4"><%= CustomModeResource.TrackingChangesCustomMode %>
                    <p class="share-collaborate-text-small-content">
                        <%= CustomModeResource.TrackingChangesDescriptionCustomMode %>
                    </p>
                </div>
                <div class="collaborate4"></div>
            </div>
            <div class="scpc-item" >
                
                <div class="share-collaborate-text-content" id="collaborate5"><%= CustomModeResource.ReviewingCustomMode %>
                    <p class="share-collaborate-text-small-content">
                        <%= CustomModeResource.ReviewingDescriptionCustomMode %>
                    </p>
                </div>
                <div class="collaborate5"></div>
            </div>
        </div>
    </div>
    
    <div class="auth-form-with auth-form-access">
        <div class="auth-form-access-container">
            <p class="access-header"><%= CustomModeResource.NearAtHandCustomMode %></p>
            <div class="access-text">
                <p class="access-text-header"><%= CustomModeResource.OfficeOnlineCustomMode %></p>
                <p class="access-text-content">
                    <%= CustomModeResource.OfficeOnlineDescriptionCustomMode %><br/><br/>
                    <a class="access-href create-link"> <%= CustomModeResource.CreateOfficeCustomMode %></a>
                </p>
            </div>
            <div class="access-text">
                <p class="access-text-header"><%= CustomModeResource.OfficeDesktopCustomMode %></p>
                <p class="access-text-content">
                    <%= CustomModeResource.OfficeDesktopDescriptionCustomMode %><br/><br/>
                      <a class="access-href" href="<%= HelpLink + "/pdf/Guides/connect_cloud.pdf" %>" target="_blank"><%= CustomModeResource.ConnectionInstructionsCustomMode %></a>
                </p>
            </div>
            
            <div class="clear"></div>
           
        </div>
    </div>
</div>

<!--Registration form-->
<div class="auth-form-with_form_w">
    <div class="auth-form-with_form clearFix">
        <div class="register_form_closer">&times;</div>
        <span class="auth-form-with_form_title"><%=CustomModeResource.CreateAccountCustomMode %></span>
        <div id="confirmEmailError" class="auth-form-with_form_error"></div>
        <input id="confirmEmail" class="auth-form-with_field_email" type="email" placeholder="<%= CustomModeResource.EmailHintCustomMode %>" />
                    
        <div class="auth-form-settings">
            <div class="auth-form-setting">
                <label>
                    <input type="checkbox" id="agree_to_terms" hidden><span></span>
                    <label for="agree_to_terms"><%=String.Format(CustomModeResource.RegistrySettingTermsCustomMode.HtmlEncode(),
                                                "<a target=\"_blank\" href=\"" + HelpLink + "/pdf/Terms/cloud.pdf\">", "</a>")%></label>
                </label>
            </div>
            <div class="auth-form-setting">
                <label>
                    <input type="checkbox" id="spam" name="spam" hidden checked="checked"><span></span>
                    <label for="spam"><%=CustomModeResource.RegistrySettingSpamCustomMode %></label>
                </label>
                            
            </div>
        </div>

        <a id="confirmEmailBtn" class="auth-form-with_form_btn disabled" ><%=CustomModeResource.RegistryButtonCreateNowCustomMode %></a>
        <% if (ThirdpartyEnable)
           { %>
        <span class="auth-form-with_form_or"><%= CustomModeResource.AuthDocsOrCustomMode %></span>
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
        <div class="default-personal-popup_text"><%: CustomModeResource.ThanksRegistrationCustomMode %></div>
        <div class="default-personal-popup_text small-text"><%= CustomModeResource.SendActivationEmailCustomMode %> '<span id="activationEmail"></span>'</div>
        <div class="default-personal-popup_strong-text"><%= CustomModeResource.CheckEmailCustomMode %></div>
    </div>
</div>

<div id="loginPopup" class="default-personal-popup login">
    <div class="default-personal-popup_cnt">
        <div class="default-personal-popup_closer">&times;</div>
        <h2 class="default-personal-popup_h"><%= Request.DesktopApp() ? CustomModeResource.LoginAccountDesktopCustomMode : CustomModeResource.LoginAccountCustomMode %></h2>
       <!-- <div class="default-personal-popup_line"></div> -->
        <div class="default-personal-popup_form-i">
            <label class="default-personal-popup_label email"><%= CustomModeResource.EmailCustomMode %></label>
           <div class="login_field-cnt">
                <input type="email" id="login" name="login" maxlength="255" class="default-personal-popup_field"
                    
                    <%= String.IsNullOrEmpty(Login) ? "" : ("value=\"" + Login.HtmlEncode() + "\"") %>/>
                <input type="hidden" class="login-message" value="<%= LoginMessage %>" data-type="<%= LoginMessageType %>"/>
            </div> 
            <span class="default-personal-popup_check"></span>
            
        </div>
        <div class="default-personal-popup_form-i">
            <label class="default-personal-popup_label password"><%= CustomModeResource.PasswordCustomMode %></label>
            <input class="default-personal-popup_field password" id="pwd" maxlength="<%= PasswordSettings.MaxLength %>" type="password" />
            <input type="hidden" id="passwordHash" name="passwordHash" />
            
            <%if (Request.DesktopApp()){ %>
                <span class="desktop-personal-popup_check">
                    <label>
                        <input type="checkbox" class="checkbox" id="desktop_agree_to_terms" hidden>
                        <span class="checkbox-custom"></span>
                        <label><%= String.Format(CustomModeResource.RegistrySettingTermsCustomMode.HtmlEncode(),
                                                "<a target=\"_blank\" href=\"" + HelpLink + "/pdf/Terms/cloud.pdf\">", "</a>") %></label>
                    </label>
                </span>
            <% } %>
            <span class="default-personal-popup_check">
                <label>
                    <input type="checkbox" class="checkbox" checked />
                    <span class="checkbox-custom"></span>
                    <span class="label"><%= CustomModeResource.RememberCustomMode %></span>
                </label>
            </span>
        </div>

        <div class="login_accept-btn-cnt">
            <button id="loginBtn" class="default-personal-popup_btn <%= Request.DesktopApp() ? "disabled" : "" %>"
                onclick="return false;"><%= CustomModeResource.LoginCustomMode %></button>
            <span class="login_forget-psw" onclick="PasswordTool.ShowPwdReminderDialog();"><%= CustomModeResource.ForgotPswdCustomMode %></span>
        </div>
        
        <% if (ThirdpartyEnable)
           {%>
        <div class="popup_social-nets auth-docs_btns_social">
            <div class="auth-docs_social_text"><%= CustomModeResource.EnterViaSocialCustomMode %></div>
            <asp:PlaceHolder ID="LoginSocialNetworks" runat="server" /> 
        </div>
        <% } %>
        <div class="login_not-registered-cnt">
            <span><%= CustomModeResource.NotRegisteredCustomMode %></span>
            <span id="loginSignUp" class="login_signup-btn"><%= Request.DesktopApp() ? CustomModeResource.SignUpDesktopCustomMode : CustomModeResource.SignUpCustomMode %></span>
        </div>
    </div>
    <asp:PlaceHolder ID="pwdReminderHolder" runat="server" />
</div>


<asp:PlaceHolder runat="server" ID="PersonalFooterHolder"></asp:PlaceHolder>


