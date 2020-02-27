/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


jq(document).on("keyup",
    "#studio_confirm_Email," +
        "#studio_confirm_FirstName," +
        "#studio_confirm_LastName," +
        "#studio_confirm_pwd",
    function (event) {
        var code;
        if (!e) {
            var e = event;
        }
        if (e.keyCode) {
            code = e.keyCode;
        } else if (e.which) {
            code = e.which;
        }

        if (code != 13) {
            return;
        }

        if (jq(this).is("#studio_confirm_pwd")) {
            //do postback
            jq("#buttonConfirmInvite").click();
            return;
        }

        var input = jq(this).parents(".property").next().find(".value input");
        input.focus();
        //set focus to end of text
        var tmpStr = input.val();
        input.val("");
        input.val(tmpStr);
    });

jq(document).on("click", "#buttonConfirmInvite", function () {
    var requireFields = {
            email: jq("#studio_confirm_Email"),
            firstname: jq("#studio_confirm_FirstName"),
            lastname: jq("#studio_confirm_LastName"),
            psw: jq("#studio_confirm_pwd")
        },
        error = 0;

    jq("#registrationForm input").removeClass("with-error");

    for (var item in requireFields) {
        
        if (requireFields[item].is(":visible")) requireFields[item].val(requireFields[item].val().trim())

        if (requireFields[item].is(":visible") && !requireFields[item].val()) {
            requireFields[item].addClass("with-error");
            error++;
        }
    }

    if (requireFields.email.is(":visible") && !jq.isValidEmail(requireFields.email.val())) {
        requireFields.email.addClass("with-error");
        error++;
    }

    var regexp = new XRegExp(ASC.Resources.Master.UserNameRegExpr.Pattern);

    if (requireFields.firstname.is(":visible") && !regexp.test(requireFields.firstname.val())) {
        requireFields.firstname.addClass("with-error");
        error++;
    }

    if (requireFields.lastname.is(":visible") && !regexp.test(requireFields.lastname.val())) {
        requireFields.lastname.addClass("with-error");
        error++;
    }

    if (error == 0) {
        window.submitForm("confirmInvite", "");
    }
});