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

window.administrationManager = (function ($) {
    var is_init = false,
        domains = [],
        mailboxes = [],
        mailgroups = [],
        server_info,
        events = $({});

    var init = function () {
        if (is_init === false) {
            is_init = true;

            serviceManager.bind(window.Teamlab.events.getMailServerFreeDns, onGetMailServerFreeDns);
            serviceManager.bind(window.Teamlab.events.addMailDomain, onAddMailDomain);
            serviceManager.bind(window.Teamlab.events.getMailServerFullInfo, onGetMailServerFullInfo);
            serviceManager.bind(window.Teamlab.events.addMailbox, onAddMailbox);
            serviceManager.bind(window.Teamlab.events.removeMailbox, onRemoveMailbox);
            serviceManager.bind(window.Teamlab.events.addMailGroup, onAddMailGroup);
            serviceManager.bind(window.Teamlab.events.removeMailGroup, onRemoveMailGroup);
            serviceManager.bind(window.Teamlab.events.removeMailDomain, onRemoveMailDomain);
            serviceManager.bind(window.Teamlab.events.addMailBoxAlias, onAddMailAlias);
            serviceManager.bind(window.Teamlab.events.removeMailBoxAlias, onRemoveMailAlias);
            serviceManager.bind(window.Teamlab.events.addMailGroupAddress, onAddMailgroupAddress);
            serviceManager.bind(window.Teamlab.events.removeMailGroupAddress, onRemoveMailgroupAddress);

            administrationPage.init();
        }
    };

    function onGetMailServerFullInfo(params, serverFullInfo) {
        messagePage.hide();
        mailBox.hidePages();
        mailBox.hideContentDivs();
        mailBox.hideLoadingMask();

        server_info = serverFullInfo.server;
        domains = serverFullInfo.domains;
        mailboxes = $.map(serverFullInfo.mailboxes, convertServerMailbox);
        mailgroups = $.map(serverFullInfo.mailgroups, convertServerGroup);
        events.trigger('ongetfullinformation', { domains: domains, mailgroups: mailgroups, mailboxes: mailboxes });

        administrationPage.bindCreationLinks();
    }

    function onGetMailServerFreeDns(params, dns) {
        server_info.dns = dns;
    }


    function convertServerMailbox(serverMailbox) {
        var mailbox = {};
        mailbox.id = serverMailbox.id;
        mailbox.address = serverMailbox.address;
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
        var group_mailboxes = [];
        for (var i = 0; i < serverGroup.addresses.length; i++) {
            var mailbox = getMailboxByEmail(serverGroup.addresses[i].email);
            group_mailboxes.push(mailbox);
        }
        group.mailboxes = group_mailboxes;

        return group;
    }

    function onAddMailDomain(params, serverDomain) {
        domains.push(serverDomain);
        events.trigger('onadddomain', serverDomain);
    }

    function onAddMailbox(params, serverMailbox) {
        var mailbox = convertServerMailbox(serverMailbox);
        mailboxes.push(mailbox);
        if (mailbox.user.id == window.Teamlab.profile.id) {
            serviceManager.getAccounts();
        }
        events.trigger('onaddmailbox', mailbox);
    }

    function onRemoveMailbox(params, id) {
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
        if (!mailbox) return;

        for (var i = 0; i < mailgroups.length; i++) {
            var index = mailgroups[i].mailboxes.indexOf(mailbox);
            if (index > -1) {
                removeMailboxFromGroup(mailgroups[i], mailbox, false);
            }
        }

        clearEmptyGroups();
    }

    function clearEmptyGroups() {
        var temp_collection = mailgroups.slice(); // clone array
        var i, len = temp_collection.length;
        for (i = 0; i < len; i++) {
            var mailgroup = temp_collection[i];
            if (mailgroup.mailboxes.length == 0) {
                removeMailGroup(mailgroup, false);
            }
        }
    }

    function onAddMailAlias(params, alias) {
        var mailbox = getMailbox(params.mailbox_id);
        mailbox.aliases.push(alias);
        events.trigger('onaddalias', { mailbox: mailbox, alias: alias });
        if (mailbox.user.id == window.Teamlab.profile.id) {
            serviceManager.getAccounts();
        }
    }

    function onRemoveMailAlias(params, mailboxId) {
        var mailbox = getMailbox(mailboxId);
        var index = mailbox.aliases.indexOf(params.alias);
        if (index > -1)
            mailbox.aliases.splice(index, 1);
        events.trigger('onremovealias', { mailbox: mailbox, alias: params.alias });
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
        if (!mailgroup) return;
        
        var index = mailgroups.indexOf(mailgroup);
        if (index > -1) {
            mailgroups.splice(index, 1);
            events.trigger('onremovemailgroup', { group: mailgroup, showToastr: showToastr });
        }
    }

    function onAddMailgroupAddress(params, serverGroup) {
        var new_group = convertServerGroup(serverGroup);
        var mailgroup = getMailGroup(new_group.id);
        mailgroup.mailboxes = new_group.mailboxes;
        events.trigger('onaddgroupaddress', { group: mailgroup, address: params.address });

        var mailbox = getMailboxByEmail(params.address.email);
        if (mailbox.user.id == window.Teamlab.profile.id) {
            serviceManager.getAccounts();
        }
    }
    
    function onRemoveMailgroupAddress(params, addressId) {
        var mailbox = getMailboxByEmail(params.address.email);
        var group = getMailGroup(params.group.id);

        removeMailboxFromGroup(group, mailbox, true);

        if (mailbox.user.id == window.Teamlab.profile.id) {
            serviceManager.getAccounts();
        }
    }

    function removeMailboxFromGroup(group, mailbox, showToastr) {
        if (!group || !group.mailboxes) return;

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
        return server_info;
    }

    function getMailboxesByDomain(domainId) {
        var domain_mailboxes = $.map(mailboxes, function (mailbox) {
            return mailbox.address.domainId == domainId ? mailbox : null;
        });
        return domain_mailboxes;
    }

    function getFreeMailboxesByDomain(domainId) {
        var groups = getMailGroupsByDomain(domainId);
        var mailbox_array = getMailboxesByDomain(domainId);

        for (var k = 0; k < groups.length; k++) {
            for (var l = 0; l < groups[k].mailboxes.length; l++) {
                var index = mailbox_array.indexOf(groups[k].mailboxes[l]);
                if (index > -1)
                    mailbox_array.splice(index, 1);
            }
        }

        return mailbox_array;
    }

    function getMailGroupsByDomain(domainId) {
        var domain_mailgroups = $.map(mailgroups, function (mailgroup) {
            return mailgroup.address.domainId == domainId ? mailgroup : null;
        });
        return domain_mailgroups;
    }

    function getAddressesByDomain(domainId) {
        var domain_addresses = $.map(mailboxes, function (mailbox) {
            return mailbox.address.domainId == domainId ? mailbox.address : null;
        });
        return domain_addresses;
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

        events: events
    };

})(jQuery);