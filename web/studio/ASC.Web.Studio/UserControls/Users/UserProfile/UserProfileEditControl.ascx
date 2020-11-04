<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProfileEditControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.UserProfileEditControl" %>
<%@ Import Namespace="ASC.ActiveDirectory.Base.Settings" %>
<%@ Import Namespace="ASC.Web.Core.Sms" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>

<div id="userProfileEditPage" class="containerBodyBlock">
    <div class="profile-action-content clearFix">
        <div class="profile-photo-block">
            <div class="profile-user-photo">
                <div id="userProfilePhoto" class="profile-action-usericon">
                    <img src="<%= PhotoPath %>" alt="" />
                </div>
                <% if (!ProfileIsLdap || ((ProfileHasAvatar || !LdapSettings.GetImportedFields.Contains(LdapSettings.MappingFields.AvatarAttribute))))
                    { %>
                <div id="loadPhotoImage" class="action-block grey-phone">
                    <span class="bold"><%= Resource.EditPhoto %></span>
                </div>
                <% } %>
                <% if (IsPageEditProfileFlag && ProfileRole != null) { %>
                <div class="profile-role <%= ProfileRole.Class %>" title="<%= ProfileRole.Title%>"></div>
                <% } %>
            </div>
        </div>
        <table class="profile-action-userdata">
            <%--Type--%>
            <% if (!IsPersonal) { %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text">
                    <%= Resource.UserType %>
                    <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForProjectTeam'});"></div>
                    <div class="popup_helper" id="AnswerForProjectTeam">
                        <p><%= Resource.ProfileTypePopupHelper %></p>
                        <table class="moduleInfo">
                        <tr>
                            <td class="moduleName tableHead"><%= Resource.ProductsAndInstruments_Products %></td>
                            <td class="userType tableHead"><%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %></td>
                            <td class="userType tableHead"><%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %></td>
                        </tr>
                        </table>
                        <% if (!string.IsNullOrEmpty(HelpLink)) { %>
                        <a class="link underline blue" href="<%= HelpLink + "/gettingstarted/people.aspx#ManagingAccessRights_block" %>" target="_blank"><%= Resource.TermsOfUsePopupHelperLink %></a>
                        <% } %>
                    </div>
                </td>
                <td class="userdata-value user-type">
                    <div id="advancedUserType" class="buttonGroup <%= UserTypeSelectorClass %>">
                        <span class="<%= UserTypeSelectorGuestItemClass %>"><%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %></span>
                        <span class="<%= UserTypeSelectorUserItemClass %>"><%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %></span>
                    </div>
                </td>
            </tr>
            <% } %>
            <%--FirstName--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text requiredTitle"><%= Resource.FirstName %></td>
                <td class="userdata-value requiredField">
                    <div class="validationField">
                        <input type="text" id="profileFirstName" class="textEdit" value="<%= FirstName %>" autocomplete="off" <%= IsPageEditProfileFlag && (IsLdapField(LdapSettings.MappingFields.FirstNameAttribute) || ProfileIsSso) ? "disabled" : "" %> <%= IsLdapField(LdapSettings.MappingFields.FirstNameAttribute) ? " title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : ( ProfileIsSso ? " title=\"" + Resource.SsoUserEditCanOnlyAdminTitle + "\"" : " title=\"" + Resource.FirstName + "\"") %>/>
                        <span class="validationText"></span>
                    </div>
                </td>
            </tr>
            <%--LastName--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text requiredTitle"><%= Resource.LastName %></td>
                <td class="userdata-value requiredField">
                    <div class="validationField">
                        <input type="text" id="profileSecondName" class="textEdit" value="<%= LastName %>" autocomplete="off" <%= IsPageEditProfileFlag && (IsLdapField(LdapSettings.MappingFields.SecondNameAttribute) || ProfileIsSso) ? "disabled" : "" %> <%= IsLdapField(LdapSettings.MappingFields.FirstNameAttribute) ? " title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : ( ProfileIsSso ? " title=\"" + Resource.SsoUserEditCanOnlyAdminTitle + "\"" : " title=\"" + Resource.LastName + "\"") %>/>
                        <span class="validationText"></span>
                    </div>
                </td>
            </tr>
            <%--Email--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text">
                    <span class="requiredTitle"><%= Resource.Email %></span>
                    <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForEmail'});"></div>
                    <div class="popup_helper" id="AnswerForEmail"><%= String.Format(Resource.EmailPopupHelper.HtmlEncode(), "<p>", "</p><p>", "</p><p>", "</p>") %></div>
                </td>
                <td class="userdata-value requiredField">
                    <div id="inputUserEmail">
                        <input type="text" id="profileEmail" value="<%= Email %>" autocomplete="off" class="textEdit" <%= IsPageEditProfileFlag ? "disabled" : "" %> <%= ProfileIsLdap ? " title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : (ProfileIsSso ? " title=\"" + Resource.SsoUserEditCanOnlyAdminTitle + "\"" : " title=\"" + Resource.Email + "\"") %>/>
                        <span class="emailInfo"></span>
                        <% if (!IsPageEditProfileFlag  && !IsTrial) { %>
                        <a id="createEmailOnDomain" class="link dotline" style="display:none;"><%= Resource.CreateEmailOnDomain %></a>
                        <% } %>
                    </div>
                    <% if (!IsPageEditProfileFlag && CurrentUserIsMailAdmin  && !IsTrial) { %>
                    <div id="inputPortalEmail" style="display:none;">
                        <input type="text" autocomplete="off" class="textEdit portalEmail" maxlength="30" size="30" />
                        @<select id="domainSelector"></select>
                        <span class="emailInfo"></span>
                        <br />
                        <a id="setExistingEmail" class="link dotline"><%= Resource.SpecifyAnExistingEmail %></a>
                    </div>
                    <% } %>
                </td>
            </tr>
            <%--Phone--%>
            <% if (IsPageEditProfileFlag && !String.IsNullOrEmpty(Phone)) { %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.MobilePhone %></td>
                <td class="userdata-value">
                    <div class="text-alignment"><%= IsLdapField(LdapSettings.MappingFields.MobilePhoneAttribute) ? "" : "+" %><%= SmsSender.GetPhoneValueDigits(Phone) %></div>
                </td>
            </tr>
            <% } %>
            <%--Login--%>
            <% if (ProfileIsLdap && IsPageEditProfileFlag) { %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text">
                    <span class="requiredTitle"><%= Resource.Login %></span> 
                </td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profileLogin" value="<%= Login %>" autocomplete="off" class="textEdit" disabled="disabled" title="<%= Resource.LdapUserEditCanOnlyAdminTitle %>" />
                </td>
            </tr>
            <% } %>
            <%--Password--%>
            <% if (!IsPageEditProfileFlag) { %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text">
                    <span id="titlePassword" class=""><%= Resource.Password %></span>
                </td>
                <td id="tablePassword" class="userdata-value">
                    <div id="generatedPassword">
                        <a id="setPassword" class="link dotline"><%= Resource.SetPassword %></a>
                        <p class="gray-text" style="margin-top: 2px;"><%= Resource.TemporaryPasswordToAccess %></p>
                    </div>
                    <div class="validationBlock">
                        <input id="password" autocomplete="off" class="textEdit" type="password" maxlength="<%= PasswordSettings.MaxLength %> " size="10" title="<%= Resource.Password %>"/>
                        <a class="infoChecking" id="passwordGen">&nbsp;</a>
                        <div id="bubleBlock">
                        <div id="passwordInfo" style="display:none;"><%= Resource.ErrorPasswordMessage %>:
                            <br /><span id="passMinLength" class="infoItem"><%= String.Format(Resource.ErrorPasswordLength.HtmlEncode(), UserPasswordMinLength, PasswordSettings.MaxLength) %></span>
                            <% if (UserPasswordDigits) { %>
                            <br /><span id="passDigits" class="infoItem"><%= Resource.ErrorPasswordNoDigits %></span>
                            <% } if (UserPasswordUpperCase) { %>
                            <br /><span id="passUpper" class="infoItem"><%= Resource.ErrorPasswordNoUpperCase %></span>
                            <% } if (UserPasswordSpecSymbols) { %>
                            <br /><span id="passSpecial" class="infoItem"><%= Resource.ErrorPasswordNoSpecialSymbols %> (!@#$%^&*_\-()=)</span>
                            <% } %>
                        </div>
                        </div>
                        <input type="checkbox" id="passwordShow"/><label for="passwordShow" class="hide" id="passwordShowLabel"></label>
                        <div class="validationProgress">&nbsp;</div><br />
                        <textarea id="clip" tabindex="-1"></textarea>
                        <a id="copyValues" class="link dotline disabled"><%= Resource.CopyEmailAndPassword %></a>
                    </div>
                </td>
            </tr>
            <% } %>

            <tr><td><br/></td></tr>

            <%--Birth Date--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.Birthdate %></td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profileBirthDate" class="textCalendar textEditCalendar" value="<%= BirthDate %>" data-value="<%= BirthDate %>"  <%= IsLdapField(LdapSettings.MappingFields.BirthDayAttribute) ? "disabled" : "" %> <%= IsLdapField(LdapSettings.MappingFields.BirthDayAttribute) ? " title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : " title=\"" + Resource.Birthdate + "\"" %>  autocomplete="off"/>
                    <span class="requiredErrorText"><%= Resource.ErrorNotCorrectDate %></span>
                </td>
            </tr>
            <%--Sex--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.Sex %></td>
                <td class="userdata-value">
                    <div id="advancedSexType" class="buttonGroup <%= IsLdapField(LdapSettings.MappingFields.GenderAttribute) ? "disabled" : "" %>" <%= IsLdapField(LdapSettings.MappingFields.GenderAttribute) ? " title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : "" %> >
                        <span><%= Resource.MaleSexStatus %></span>
                        <span><%= Resource.FemaleSexStatus %></span>
                    </div>
                </td>
            </tr>
            <tr><td><br/></td></tr>
            <%--Department--%>
            <% if ((CurrentUserIsPeopleAdmin || Departments.Length != 0) && !IsPersonal) { %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text">
                    <%= CustomNamingPeople.Substitute<Resource>("Department").HtmlEncode() %>
                </td>
                <td id="departmentsField" class="userdata-value">
                    <% if (CurrentUserIsPeopleAdmin) { %>
                    <ul class="departments-list advanced-selector-list-results"></ul>
                    <div><span id="chooseGroupsSelector" class="link dotline plus"><%= CustomNamingPeople.Substitute<Resource>("BindDepartmentButton").HtmlEncode() %></span></div>
                    <% } else { %>
                    <% foreach (var department in Departments) { %>
                    <div class="field-value"><%= department.Name.HtmlEncode() %></div>
                    <% } %>
                    <% } %>
                </td>
            </tr>
            <% } %>
            <%--Position--%>
            <% if (!IsPersonal) { %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= CustomNamingPeople.Substitute<Resource>("UserPost").HtmlEncode() %></td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profilePosition" class="textEdit" value="<%= Position %>" autocomplete="off" <%= IsPageEditProfileFlag && (!CurrentUserIsPeopleAdmin || IsLdapField(LdapSettings.MappingFields.TitleAttribute) || ProfileIsSso) ? "disabled" : "" %> <%= IsLdapField(LdapSettings.MappingFields.TitleAttribute) ? " title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : ( ProfileIsSso ? " title=\"" + Resource.SsoUserEditCanOnlyAdminTitle + "\"" : " title=\"" + CustomNamingPeople.Substitute<Resource>("UserPost").HtmlEncode() + "\"") %>/>
                    <span class="requiredErrorText"><%= Resource.ErrorMessageLongField64 %></span>
                </td>
            </tr>
            <% } %>
            <%--Location--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.Location %></td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profilePlace" class="textEdit" value="<%= Place %>" autocomplete="off" <%= IsPageEditProfileFlag && (IsLdapField(LdapSettings.MappingFields.LocationAttribute) || ProfileIsSso) ? "disabled" : "" %> <%= IsLdapField(LdapSettings.MappingFields.LocationAttribute) ? " title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : (ProfileIsSso ? " title=\"" + Resource.SsoUserEditCanOnlyAdminTitle + "\""  : " title=\"" + Resource.Location + "\"") %> />
                    <span class="requiredErrorText"><%= Resource.ErrorMessageLongField255 %></span>
                </td>
            </tr>
            <%--Registration Date--%>
            <% if (!IsPersonal) { %>
                <tr class="userdata-field">
                    <td class="userdata-title describe-text"><%= CustomNamingPeople.Substitute<Resource>("WorkFromDate").HtmlEncode() %></td>
                    <td class="userdata-value requiredField">
                        <input type="text" id="profileRegistrationDate" class="textCalendar textEditCalendar" value="<%= WorkFromDate %>" data-value="<%= WorkFromDate %>" <%= IsPageEditProfileFlag && !CurrentUserIsPeopleAdmin ? "disabled" : "" %> title="<%= CustomNamingPeople.Substitute<Resource>("WorkFromDate").HtmlEncode() %>"  autocomplete="off"/>
                        <span class="requiredErrorText"><%= Resource.ErrorNotCorrectDate %></span>
                    </td>
                </tr>
            <% } %>
        </table>
    </div>
    <%--Comment--%>
     <% if (!IsPersonal) { %>
    <div id="commentTab" class="tabs-section">
        <span class="header-base"><%= Resource.Comments %></span>
        <span id="switcherCommentButton" class="toggle-button" data-switcher="0"
            data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <div id="commentContainer" class="tabs-content">
        <textarea id="profileComment" class="textEdit" rows="4" title="<%= Resource.Comments %>"><%= Comment %></textarea>
    </div>
    <% } %>
    <%--Contacts--%>
    <div id="contactInfoTab" class="tabs-section">
        <span class="header-base"><%= Resource.ContactInformation %></span>
        <span id="switcherContactInfoButton" class="toggle-button" data-switcher="0"
            data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <div id="contactInfoContainer" class="tabs-content contacts-group">
        <div class="field-with-actions default">
            <select class="group-field top-align">
                <option class="optionItem mail" value="mail" selected="selected"><%= Resource.TitleEmail %></option>
                <option class="optionItem phone" value="phone"><%= Resource.TitlePhone %></option>
                <option class="optionItem mobphone" value="mobphone"><%= Resource.TitleMobphone %></option>
                <option class="optionItem gmail" value="gmail"><%= Resource.TitleGooglemail %></option>
                <option class="optionItem skype" value="skype"><%= Resource.TitleSkype %></option>
                <option class="optionItem msn" value="msn"><%= Resource.TitleMsn %></option>
                <option class="optionItem icq" value="icq"><%= Resource.TitleIcq %></option>
                <option class="optionItem jabber" value="jabber"><%= Resource.TitleJabber %></option>
                <option class="optionItem aim" value="aim"><%= Resource.TitleAim %></option>
            </select>
            <a class="delete-field icon-link trash"></a>
            <input type="text" class="textEdit" value="" autocomplete="off" />
        </div>
        <a class="add-new-field link dotline plus"><%= Resource.BtnAddNewContact %></a>
    </div>
    <%--Social Nets--%>
    <div id="SocialTab" class="tabs-section">
        <span class="header-base"><%= Resource.SocialProfiles %></span>
        <span id="switcherSocialButton" class="toggle-button" data-switcher="0"
            data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <div id="socialContainer" class="tabs-content contacts-group">
        <div class="field-with-actions default">
            <select class="group-field top-align">
                <option class="optionItem facebook" value="facebook"><%= Resource.TitleFacebook %></option>
                <option class="optionItem livejournal" value="livejournal"><%= Resource.TitleLiveJournal %></option>
                <option class="optionItem myspace" value="myspace"><%= Resource.TitleMyspace %></option>
                <option class="optionItem twitter" value="twitter"><%= Resource.TitleTwitter %></option>
                <option class="optionItem blogger" value="blogger"><%= Resource.TitleBlogger %></option>
                <option class="optionItem yahoo" value="yahoo"><%= Resource.TitleYahoo %></option>
            </select>
            <a class="delete-field icon-link trash"></a>
            <input type="text" class="textEdit" value="" placeholder="<%= Resource.HintForSocialAccounts %>" autocomplete="off" />
        </div>
        <a class="add-new-field link dotline plus"><%= UserControlsCommonResource.AddNewSocialProfile %></a>
    </div>
    <div id="profileActionsContainer" class="big-button-container">
        <a id="profileActionButton" class="button blue big"><%= ButtonText %></a>
        <span class="splitter-buttons"></span>
        <a id="cancelProfileAction" data-url="<%= ProfilePath %>" class="button gray big"><%= UserControlsCommonResource.CancelButton %></a>
    </div>
</div>

<asp:PlaceHolder ID="loadPhotoWindow" runat="server"></asp:PlaceHolder>
