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


if (!ASC.Controls.TalkNavigationItem) {
    ASC.Controls.TalkNavigationItem = {
        checkMessagesTimeout: 1000,
        currentValue: -1,
        init: function (timeout) {

            if (jq("#studioPageContent li.talk").length == 0)
                return;

            if (isFinite(+timeout)) {
                this.checkMessagesTimeout = +timeout * 1000;

                StudioManager.addPendingRequest(function () {
                    ASC.Controls.TalkNavigationItem.checkNewMessages();
                    if (+timeout > 0) {
                        setInterval(ASC.Controls.TalkNavigationItem.checkNewMessages, ASC.Controls.TalkNavigationItem.checkMessagesTimeout);
                    }
                });
            }
            
            Teamlab.bind(Teamlab.events.getTalkUnreadMessages, ASC.Controls.TalkNavigationItem.getResponce);

            
        },
        updateValue: function (value) {
            if (this.currentValue === value) {
                return undefined;
            }
            this.currentValue = value;
            var o = document.getElementById('talkMsgCount');
            if (o) {
                o.innerHTML = value > 100 ? ">100" : value;
                jQuery('#talkMsgLabel')[value > 0 ? 'addClass' : 'removeClass']('has-led');
            }
        },
        getResponce: function (params, count) {
            if (count > 0) {
                ASC.Controls.TalkNavigationItem.updateValue(count);
            }
        },
        checkNewMessages: function () {
            if (jQuery('#talkMsgLabel').length > 0) {
                Teamlab.getTalkUnreadMessages();
            }
        }
    };
}

jq(document).ready(function () {
    if (ASC.SocketIO && !ASC.SocketIO.disabled()) {
        ASC.SocketIO.Factory.counters
            .on('getNewMessagesCount',
                function(counts) {
                    ASC.Controls.TalkNavigationItem.updateValue(counts.me);
                })
            .on('sendMessagesCount',
                function(counts) {
                    ASC.Controls.TalkNavigationItem.updateValue(counts);
                });
    }
});

if (!ASC.Controls.JabberClient) {
    ASC.Controls.JabberClient = {
        username: null,
        jid: null,
        winName: 'ASCJabberClient' + location.hostname,
        helperId: 'JabberClientHelper-' + Math.floor(Math.random() * 1000000),
        params: '', //in new tab
        //params: 'ontouchend' in document ? '' : 'width=900,height=600,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no,target=_blank', //in new window
        pathCmdHandler: '',
        pathWebTalk: '',
        init: function (name, talkpath, cmdpath) {

            if (jq("#studioPageContent li.talk").length == 0)
                return;

            if (typeof name === 'string' && name.length > 0) {
                this.username = name.toLowerCase();
            }
            if (typeof talkpath === 'string' && talkpath.length > 0) {
                this.pathWebTalk = talkpath;
            }
            if (typeof cmdpath === 'string' && cmdpath.length > 0) {
                this.pathCmdHandler = cmdpath;
            }
            var a = this.winName.match(/\w+/g);
            this.winName = a ? a.join('') : this.winName;
        },
        extendChat: function () {
            ASC.Controls.JabberClient.open();
        },
        redirectMain: function () {
            try {
                window.open('/');
            } catch (err) {
            }
        },
        open: function (jid) {
            var hWnd = null,
                isExist = false,
                name = jid ? jid.substring(0, jid.indexOf('@')) : '';

            name = name || jid || '';

            if (this.username === name.toLowerCase()) {
                jid = null;
            }
            if (typeof jid === 'string') {
                jid = jid.toLowerCase();
            }

            try {
                hWnd = window.open('', this.winName, this.params);
            } catch (err) {
            }

            try {
                isExist = !hWnd || typeof hWnd.ASC === 'undefined' ? false : true;
            } catch (err) {
                isExist = true;
            }

            if (!isExist && typeof this.pathWebTalk) {
                hWnd = window.open(this.pathWebTalk + (jid && typeof jid === 'string' && jid.length > 0 ? '#' + jid : ''), this.winName, this.params);
                isExist = true;
            }

            if (!isExist) {
                return undefined;
            }

            try {
                if (hWnd)
                    hWnd.focus();
            } catch (err) {
            }

            try {
                ASC.Controls.TalkNavigationItem.updateValue(0);
                if (ASC.SocketIO && !ASC.SocketIO.disabled()) {
                    ASC.SocketIO.Factory.counters.emit('sendMessagesCount', 0);
                }
            } catch (e) {
                console.error(e.message);
            }

            if (typeof jid === 'string' && jid.length > 0) {
                this.openContact(jid);
            }
            return hWnd;
        },
        openContact: function (jid) {
            if (typeof this.username === 'string' && this.username.length > 0) {
                var name = jid.substring(0, jid.indexOf('@'));
                name = name || jid || '';
                this.jid = jid;

                if (this.username === name.toLowerCase()) {
                    name = null;
                }

                if (name && typeof name === 'string') {
                    jQuery.ajax({
                        async: true,
                        type: 'get',
                        contentType: 'text/xml',
                        cache: false,
                        url: this.pathCmdHandler,
                        data: { from: name.toLowerCase(), to: this.username },
                        complete: null
                    });
                }
            }
        },
        openTenant: function (tenant) {
            if (tenant && typeof tenant === 'string') {
                jQuery.ajax({
                    async: true,
                    type: 'get',
                    contentType: 'text/xml',
                    cache: false,
                    url: this.pathCmdHandler,
                    data: { from: tenant.toLowerCase(), to: this.username, fromtenant: true },
                    complete: null
                });
            }
        },
        terminate: function(data, service) {
            jQuery.ajax({
                async: true,
                type: 'post',
                contentType: 'text/xml',
                cache: false,
                url: service,
                data: data,
                complete: null
            });
        }
    };
}

if (!ASC.Controls.SoundManager) {
    ASC.Controls.SoundManager = {
        isEnabled: null,
        soundContainer: null,
        soundPath: null,
        installPath: null,
        init: function (sound, install) {
            if (typeof sound === 'string') {
                this.soundPath = sound;
            }
            if (typeof install === 'string') {
                this.installPath = install;
            }
        },
        onLoad: function (o) {
            if (typeof o === 'string') {
                o = document.getElementById(o);
            }
            if (!o || typeof o === 'undefined') {
                return null;
            }
            this.soundContainer = o;
            if (this.isEnabled === null) {
                this.isEnabled = true;
            }
            this.play('incmsg');
        },
        alarm: function () {
            if (this.soundContainer === null && this.swfPath !== null) {
                if (this.soundPath !== null) {
                    try {
                        var soundContainerId = 'talkSoundContainer-' + Math.floor(Math.random() * 1000000);
                        var o = document.createElement('div');
                        o.setAttribute('id', soundContainerId);
                        document.body.appendChild(o);
                        swfobject.embedSWF(
                            this.soundPath,
                            soundContainerId,
                            1,
                            1,
                            '9.0.0',
                            this.installPath,
                            {
                                apiInit: function (id) {
                                    ASC.Controls.SoundManager.onLoad(id);
                                },
                                apiId: soundContainerId
                            },
                            { allowScriptAccess: 'always', wmode: 'transparent' },
                            { styleclass: 'sound-container', wmode: 'transparent' }
                        );
                    } catch (err) {
                    }
                }
                this.soundContainer = undefined;
                return undefined;
            }
            this.play('incmsg');
        },
        play: function (soundname) {
            if (this.soundContainer !== null && this.isEnabled === true && typeof soundname === 'string') {
                try {
                    this.soundContainer.playSound(soundname)
                } catch (err) {
                }
            }
        }
    };
}