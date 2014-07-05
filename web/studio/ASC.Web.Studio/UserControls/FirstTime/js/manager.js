/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
if (typeof (ASC) == 'undefined')
    ASC = {};
if (typeof (ASC.Controls) == 'undefined')
    ASC.Controls = {};

ASC.Controls.FirstTimeManager = function() {

    this.OnAfterSaveRequiredData = null;
    this.OnAfterSaveData = null;

    this.SaveRequiredData = function(parentCalllback) {
        ASC.Controls.EmailAndPasswordManager.SaveRequiredData(parentCalllback);
    };
};

ASC.Controls.EmailAndPasswordManager = new function() {

    this.PassText = '';
    this.changeIt = '';
    this.ok = '';
    this.wrongPass = '';
    this.emptyPass = '';
    this.wrongEmail = '';

    this.init = function (changeItText, okText, wrongPassText, emptyPassText, wrongEmailText) {
        ASC.Controls.EmailAndPasswordManager.changeIt = changeItText;
        ASC.Controls.EmailAndPasswordManager.ok = okText;
        ASC.Controls.EmailAndPasswordManager.wrongPass = wrongPassText;
        ASC.Controls.EmailAndPasswordManager.emptyPass = emptyPassText;
        ASC.Controls.EmailAndPasswordManager.wrongEmail = wrongEmailText;
    };

    this.ShowChangeEmailAddress = function() {
        var email = jQuery.trim(jq('.emailAddress').html());
        jq('.emailAddress').html('');
        jq('.emailAddress').append('<input type="textbox" id="newEmailAddress" maxlength="64" class="textEdit newEmail">');
        jq('.emailAddress #newEmailAddress').val(email);
        jq('.changeEmail').html('');
    };

    this.AcceptNewEmailAddress = function() {
        var email = jq('.changeEmail #dvChangeMail #newEmailAddress').val();

        if (email == '')
            return;

        jq('#requiredStep .emailBlock .email .emailAddress').html(email);
        jq('.changeEmail #dvChangeMail').html('');
        jq('.changeEmail #dvChangeMail').append('<a class="info baseLinkAction" onclick="ASC.Controls.EmailAndPasswordManager.ShowChangeEmailAddress();">' + ASC.Controls.EmailAndPasswordManager.changeIt + '</a>');
    };

    this.SaveRequiredData = function(parentCallback) {

        var email = jQuery.trim(jq('#requiredStep .emailBlock .email .emailAddress #newEmailAddress').val()); //
        if (email == '' || email == null)
            email = jQuery.trim(jq('#requiredStep .emailBlock .email .emailAddress').html());
        var pwd = jq('.passwordBlock .pwd #newPwd').val();
        var cpwd = jq('.passwordBlock .pwd #confPwd').val();
        var promocode = jq('.passwordBlock .promocode #promocode_input').val();

        if (email == '' || !jq.isValidEmail(email)) {
            var res = { "Status": 0, "Message": ASC.Controls.EmailAndPasswordManager.wrongEmail };
            if (parentCallback != null)
                parentCallback(res);
            return;
        }

        if (pwd != cpwd || pwd == '') {

            if (pwd != cpwd) {
                jq(".passwordBlock .pwd #newPwd ,.passwordBlock .pwd #confPwd").css("border-color", "#DF1B1B");
            }

            if (pwd == '') {
                jq(".passwordBlock .pwd #newPwd").css("border-color", "#DF1B1B");
            }

            var res = { "Status": 0, "Message": pwd == '' ? ASC.Controls.EmailAndPasswordManager.emptyPass : ASC.Controls.EmailAndPasswordManager.wrongPass };
            if (parentCallback != null)
                parentCallback(res);
            return;
        }
        window.onbeforeunload = null;
        AjaxPro.timeoutPeriod = 1800000;
        EmailAndPasswordController.SaveData(email, pwd, jq('#studio_lng').val(), promocode, function(result) {

            if (parentCallback != null)
                parentCallback(result.value);
        });
    };
};

jq(function() {
    if (jQuery.trim(jq('.emailAddress').html()) == '') {
        ASC.Controls.EmailAndPasswordManager.ShowChangeEmailAddress();
    }
});