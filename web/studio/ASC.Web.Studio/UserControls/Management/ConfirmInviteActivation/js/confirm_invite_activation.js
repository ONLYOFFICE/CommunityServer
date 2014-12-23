/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
            window.submitForm("confirmInvite", "");
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
        if (requireFields[item].is(":visible") && !requireFields[item].val()) {
            requireFields[item].addClass("with-error");
            error++;
        }
    }

    if (requireFields.email.is(":visible") && !jq.isValidEmail(requireFields.email.val())) {
        requireFields.email.addClass("with-error");
        error++;
    }

    if (error == 0) {
        window.submitForm("confirmInvite", "");
    }
});