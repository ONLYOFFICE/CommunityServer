/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


window.contactsPanel = (function($) {
    var isInit = false,
        panelContent;

    var init = function() {
        if (isInit === false) {
            isInit = true;
            panelContent = $('#customContactPanel');
        }
    };

    var unmarkContacts = function() {
        var $contacts = panelContent.children();

        for (var i = 0, n = $contacts.length; i < n; i++) {
            var $contact = $($contacts[i]);
            if ($contact.hasClass('active')) {
                $contact.toggleClass('active', false);
            }
        }
    };

    var selectContact = function(id) {
        var $account = (panelContent.find('[id="' + id + '"]')).parent();
        if ($account != undefined) {
            $account.toggleClass('active', true);
        }
    };

    return {
        init: init,
        unmarkContacts: unmarkContacts,
        selectContact: selectContact
    };
})(jQuery);