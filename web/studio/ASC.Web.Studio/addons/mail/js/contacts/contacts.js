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


window.contactsManager = (function() {
    var initFlag = false,
        teamlabContacts = [],
        crmContacts = [],
        personalContacts = [];

    var init = function() {
        if (!initFlag) {
            initFlag = true;
            updateTlContacts();
            setInterval(function () {
                crmContacts = [];
                personalContacts = [];
                updateTlContacts();
            }, ASC.Mail.Constants.CHECK_NEWS_TIMEOUT * 10);
        }
    };

    var onGetFullTlContacts = function(params, contacts) {
        var count = contacts.length;
        for (var i = 0; i < count; i++) {
            if (contacts[i].email != undefined && contacts[i].email != '') {
                teamlabContacts.push({ 'firstName': contacts[i].firstName, 'lastName': contacts[i].lastName, 'email': contacts[i].email, 'id': contacts[i].id, 'displayName': contacts[i].displayName });
            }
        }
    };

    var updateTlContacts = function() {
        serviceManager.getProfiles({}, { success: onGetFullTlContacts });
    };

    var getTlContacts = function() {
        return teamlabContacts;
    };

    var getTlContactsByEmail = function(email) {
        var result = null;
        var count = teamlabContacts.length;
        for (var i = 0; i < count; i++) {
            if (teamlabContacts[i].email.toLowerCase() == email.toLowerCase()) {
                result = teamlabContacts[i];
                break;
            }
        }
        return result;
    };

    function searchResult(contacts, email) {
        return jq.grep(contacts, function(c) {
            return c.email === email;
        });
    }

    function searchResultIndex(contacts, email) {
        for (var i = 0, n = contacts.length; i < n; i++) {
            if (contacts[i].email === email) {
                return i;
            }
        }

        return -1;
    }

    function inCrmContacts(email) {
        var d = jq.Deferred();
        var result = {
            email: email,
            exists: false
        };

        try {
            if (!ASC.Mail.Constants.CRM_AVAILABLE) {
                d.resolve(result);
                return d.promise(); 
            }

            var found = searchResult(crmContacts, email);
            if (found.length) {
                d.resolve(found[0]);
                return d.promise();
            }

            Teamlab.getContactsByContactInfo({ address: email }, { infoType: 1, data: email }, {
                success: function (params, contacts) {
                    result.email = params.address;
                    result.exists = contacts.length > 0;
                    crmContacts.push(result);
                    d.resolve(result);
                },
                error: function (e) {
                    console.error("Teamlab.getContactsByContactInfo", e);
                },
                async: true
            });


        } catch (e) {
            console.error("inCrmContacts", e);
            d.resolve(result);
        }
        return d.promise();
    }

    function inPersonalContacts(email) {
        var d = jq.Deferred();
        var result = {
            email: email,
            exists: false
        };

        try {
            var found = searchResult(personalContacts, email);
            if (found.length) {
                d.resolve(found[0]);
                return d.promise();
            }

            Teamlab.getMailContactsByInfo({ address: email }, { infoType: 1, data: email }, {
                success: function (params, contacts) {
                    result.email = params.address;
                    result.exists = contacts.length > 0;
                    personalContacts.push(result);
                    d.resolve(result);
                },
                error: function (e) {
                    console.error("Teamlab.getMailContactsByInfo", e);
                    d.resolve(result);
                },
                async: true
            });
        } catch (e) {
            console.error("inPersonalContacts", e);
            d.resolve(result);
        }
        return d.promise();
    }

    function forgetCrmContact(email) {
        var index = searchResultIndex(crmContacts, email);
        if (index > -1) {
            crmContacts.splice(index, 1);
        }
    }

    function forgetPersonalContact(email) {
        var index = searchResultIndex(personalContacts, email);
        if (index > -1) {
            personalContacts.splice(index, 1);
        }
    }

    return {
        init: init,

        updateTlContact: updateTlContacts,
        getTLContacts: getTlContacts,
        getTLContactsByEmail: getTlContactsByEmail,
        inCrmContacts: inCrmContacts,
        inPersonalContacts: inPersonalContacts,
        forgetCrmContact: forgetCrmContact,
        forgetPersonalContact: forgetPersonalContact
    };
})(jQuery);