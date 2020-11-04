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


window.administrationManager = (function($) {
    var isInit = false,
        domains = [],
        mailboxes = [],
        mailgroups = [],
        serverInfo,
        events = $({});

    var init = function() {
        if (isInit === false) {
            isInit = true;

            window.Teamlab.bind(window.Teamlab.events.getMailServerFreeDns, onGetMailServerFreeDns);
            window.Teamlab.bind(window.Teamlab.events.addMailDomain, onAddMailDomain);
            window.Teamlab.bind(window.Teamlab.events.getMailServerFullInfo, onGetMailServerFullInfo);
            window.Teamlab.bind(window.Teamlab.events.addMailbox, onAddMailbox);
            window.Teamlab.bind(window.Teamlab.events.addMailGroup, onAddMailGroup);
            window.Teamlab.bind(window.Teamlab.events.removeMailGroup, onRemoveMailGroup);
            window.Teamlab.bind(window.Teamlab.events.removeMailDomain, onRemoveMailDomain);
            editMailboxModal.events.bind('onupdatemailbox', onUpdateMailbox);
            editMailGroupModal.events.bind('onupdategroup', onUpdateMailgroup);

            administrationPage.init();
        }
    };

    function onGetMailServerFullInfo(params, serverFullInfo) {
        mailBox.hidePages();
        mailBox.hideContentDivs();
        mailBox.hideLoadingMask();

        serverInfo = serverFullInfo.server;
        domains = serverFullInfo.domains;
        mailboxes = $.map(serverFullInfo.mailboxes, convertServerMailbox);
        mailgroups = $.map(serverFullInfo.mailgroups, convertServerGroup);
        events.trigger('ongetfullinformation', { domains: domains, mailgroups: mailgroups, mailboxes: mailboxes });

        administrationPage.bindCreationLinks();
    }

    function onGetMailServerFreeDns(params, dns) {
        serverInfo.dns = dns;
    }


    function convertServerMailbox(serverMailbox) {
        var mailbox = {};
        mailbox.id = serverMailbox.id;
        mailbox.address = serverMailbox.address;
        mailbox.name = serverMailbox.name;
        var aliases = [];
        for (var j = 0; j < serverMailbox.aliases.length; j++) {
            var alias = serverMailbox.aliases[j];
            aliases.push(alias);
        }
        mailbox.aliases = aliases;
        mailbox.user = {};
        mailbox.user.id = serverMailbox.userId;
        var contacts = contactsManager.getTLContacts();
        for (var i = 0; i < contacts.length; i++) {
            if (serverMailbox.userId == contacts[i].id) {
                mailbox.user.displayName = contacts[i].displayName;
                break;
            }
        }
        return mailbox;
    }

    function convertServerGroup(serverGroup) {
        var group = {};
        group.id = serverGroup.id;
        group.address = serverGroup.address;
        var groupMailboxes = [];
        for (var i = 0; i < serverGroup.addresses.length; i++) {
            var mailbox = getMailboxByEmail(serverGroup.addresses[i].email);
            groupMailboxes.push(mailbox);
        }
        group.mailboxes = groupMailboxes;

        return group;
    }

    function onAddMailDomain(params, serverDomain) {
        domains.push(serverDomain);
        events.trigger('onadddomain', serverDomain);
    }

    function removeDomain(id) {
        for (var i = 0; i < domains.length; i++) {
            if (domains[i].id == id) {
                var domain = domains[i];
                domains.splice(i, 1);
                events.trigger('onremovedomain', domain);
                break;
            }
        }
    }

    function onAddMailbox(params, serverMailbox) {
        var mailbox = convertServerMailbox(serverMailbox);
        mailboxes.push(mailbox);
        if (mailbox.user.id == window.Teamlab.profile.id) {
            serviceManager.getAccounts();
        }
        events.trigger('onaddmailbox', mailbox);
    }

    function removeMailbox(id) {
        for (var i = 0; i < mailboxes.length; i++) {
            if (mailboxes[i].id == id) {
                var mailbox = mailboxes[i];

                removeMailboxFromGroups(mailbox);

                mailboxes.splice(i, 1);

                if (mailbox.user.id == window.Teamlab.profile.id) {
                    serviceManager.getAccounts();
                }
                events.trigger('onremovemailbox', mailbox);
                break;
            }
        }
    }

    function removeMailboxFromGroups(mailbox) {
        if (!mailbox) {
            return;
        }

        for (var i = 0; i < mailgroups.length; i++) {
            var index = mailgroups[i].mailboxes.indexOf(mailbox);
            if (index > -1) {
                removeMailboxFromGroup(mailgroups[i], mailbox, false);
            }
        }

        clearEmptyGroups();
    }

    function clearEmptyGroups() {
        var tempCollection = mailgroups.slice(); // clone array
        var i, len = tempCollection.length;
        for (i = 0; i < len; i++) {
            var mailgroup = tempCollection[i];
            if (mailgroup.mailboxes.length == 0) {
                removeMailGroup(mailgroup, false);
            }
        }
    }

    function onUpdateMailbox(e, updatedMailbox) {
        var mailbox = getMailbox(updatedMailbox.id);
        var index = mailboxes.indexOf(mailbox);
        if (index > -1) {
            mailboxes[index] = updatedMailbox;
        }
        events.trigger('onupdatemailbox', { mailbox: updatedMailbox });
        if (mailbox.user.id == window.Teamlab.profile.id) {
            serviceManager.getAccounts();
        }
    }

    function onAddMailGroup(params, serverGroup) {
        var mailgroup = convertServerGroup(serverGroup);
        mailgroups.push(mailgroup);
        events.trigger('onaddgroup', mailgroup);

        for (var j = 0; j < mailgroup.mailboxes.length; j++) {
            if (mailgroup.mailboxes[j].user.id == window.Teamlab.profile.id) {
                serviceManager.getAccounts();
                break;
            }
        }
    }

    function onRemoveMailGroup(params, id) {

        var mailgroup = getMailGroup(id);

        removeMailGroup(mailgroup, true);

        for (var j = 0; j < mailgroup.mailboxes.length; j++) {
            if (mailgroup.mailboxes[j].user.id == window.Teamlab.profile.id) {
                serviceManager.getAccounts();
                break;
            }
        }
    }

    function removeMailGroup(mailgroup, showToastr) {
        if (!mailgroup) {
            return;
        }

        var index = mailgroups.indexOf(mailgroup);
        if (index > -1) {
            mailgroups.splice(index, 1);
            events.trigger('onremovemailgroup', { group: mailgroup, showToastr: showToastr });
        }
    }

    function onUpdateMailgroup(e, group) {
        var mailgroup = getMailGroup(group.id);
        mailgroup.mailboxes = group.mailboxes;
        events.trigger('onupdatemailgroup', { group: mailgroup });

        for (var i = 0, len = mailgroup.mailboxes.length; i < len; i++) {
            if (mailgroup.mailboxes[i].user.id == window.Teamlab.profile.id) {
                serviceManager.getAccounts();
                break;
            }
        }
    }

    function removeMailboxFromGroup(group, mailbox, showToastr) {
        if (!group || !group.mailboxes) {
            return;
        }

        var index = group.mailboxes.indexOf(mailbox);
        if (index > -1) {
            group.mailboxes.splice(index, 1);
            events.trigger('onremovegroupaddress', { group: group, address: mailbox.address, showToastr: showToastr });
        }
    }

    function onRemoveMailDomain(params, id) {
        for (var i = 0; i < domains.length; i++) {
            if (domains[i].id == id) {
                var domain = domains[i];
                domains.splice(i, 1);
                events.trigger('onremovemaildomain', domain);
                break;
            }
        }

        serviceManager.getAccounts();
    }

    function getMailDomains() {
        return domains;
    }

    function getMailboxes() {
        return mailboxes;
    }

    function getMailGroups() {
        return mailgroups;
    }

    function getServerInfo() {
        return serverInfo;
    }

    function getMailboxesByDomain(domainId) {
        var domainMailboxes = $.map(mailboxes, function(mailbox) {
            return mailbox.address.domainId == domainId ? mailbox : null;
        });
        return domainMailboxes;
    }

    function getFreeMailboxesByDomain(domainId) {
        var groups = getMailGroupsByDomain(domainId);
        var mailboxArray = getMailboxesByDomain(domainId);

        for (var k = 0; k < groups.length; k++) {
            for (var l = 0; l < groups[k].mailboxes.length; l++) {
                var index = mailboxArray.indexOf(groups[k].mailboxes[l]);
                if (index > -1) {
                    mailboxArray.splice(index, 1);
                }
            }
        }

        return mailboxArray;
    }

    function getMailGroupsByDomain(domainId) {
        var domainMailgroups = $.map(mailgroups, function(mailgroup) {
            return mailgroup.address.domainId == domainId ? mailgroup : null;
        });
        return domainMailgroups;
    }

    function getAddressesByDomain(domainId) {
        var domainAddresses = $.map(mailboxes, function(mailbox) {
            return mailbox.address.domainId == domainId ? mailbox.address : null;
        });
        return domainAddresses;
    }

    function getMailGroup(id) {
        var group = undefined;
        for (var i = 0; i < mailgroups.length; i++) {
            if (mailgroups[i].id == id) {
                group = mailgroups[i];
                break;
            }
        }
        return group;
    }

    function getMailbox(id) {
        id = parseInt(id);
        var mailbox = undefined;
        for (var i = 0; i < mailboxes.length; i++) {
            if (mailboxes[i].id == id) {
                mailbox = mailboxes[i];
                break;
            }
        }
        return mailbox;
    }

    function getMailboxByEmail(email) {
        var mailbox = undefined;
        for (var i = 0; i < mailboxes.length; i++) {
            if (mailboxes[i].address.email == email) {
                mailbox = mailboxes[i];
                break;
            }
        }
        return mailbox;
    }

    function getDomain(id) {
        id = parseInt(id);
        var domain = undefined;
        for (var i = 0; i < domains.length; i++) {
            if (domains[i].id == id) {
                domain = domains[i];
                break;
            }
        }
        return domain;
    }

    function getMailgroupsByDomain(id) {
        id = parseInt(id);
        var groups = [];
        for (var i = 0; i < mailgroups.length; i++) {
            if (mailgroups[i].address.domainId == id) {
                groups.push(mailgroups[i]);
            }
        }
        return groups;
    }


    function loadData() {
        serviceManager.getMailServerFullInfo({}, { error: administrationError.getErrorHandler("getMailServerFullInfo") }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    return {
        init: init,
        loadData: loadData,

        convertServerGroup: convertServerGroup,
        convertServerMailbox: convertServerMailbox,

        getMailDomains: getMailDomains,
        getMailboxes: getMailboxes,
        getMailGroups: getMailGroups,
        getMailboxesByDomain: getMailboxesByDomain,
        getAddressesByDomain: getAddressesByDomain,
        getMailGroupsByDomain: getMailGroupsByDomain,
        getFreeMailboxesByDomain: getFreeMailboxesByDomain,
        getMailboxByEmail: getMailboxByEmail,
        getMailGroup: getMailGroup,
        getMailbox: getMailbox,
        getDomain: getDomain,
        getServerInfo: getServerInfo,
        getMailgroupsByDomain: getMailgroupsByDomain,

        removeMailbox: removeMailbox,
        removeDomain: removeDomain,

        events: events
    };

})(jQuery);