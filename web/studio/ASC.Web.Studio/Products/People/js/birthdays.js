/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.People === "undefined")
    ASC.People = (function () { return {} })();

ASC.People.Birthdays = (function() {

    var openContact = function(obj) {
        var name = jq(obj).attr("username"),
            tcExist = false;
        try {
            tcExist = !!ASC.Controls.JabberClient;
        } catch (err) {
            tcExist = false;
        }
        if (tcExist === true) {
            try {
                ASC.Controls.JabberClient.open(name);
            } catch (err) {
            }
        }
    };

    var remind = function(obj, onRemind) {
        var userCard = jq(obj).parents(".small-user-card"),
            userId = jq(userCard).find("input[type=hidden]").val();

        Teamlab.subscribePeopleBirthday(
            { userCard: userCard },
            { userId: userId, onRemind: onRemind },
            {
                error: function(params, errors) {
                    toastr.error(errors[0]);
                    return false;
                },
                success: function(params, data){
                    jq(params.userCard).toggleClass("active", data);
                }
            });
    };

    return {
        openContact: openContact,
        remind: remind
    };
})(jQuery);