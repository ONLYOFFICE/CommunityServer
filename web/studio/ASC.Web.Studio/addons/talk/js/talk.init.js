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


window.onload = function () {
    jq('meta[name=viewport]').remove();
    jq('head').append('<meta name="viewport" content="width=device-width, initial-scale=0.5, minimum-scale=0.3, maximum-scale=1">');
};

jq(document).ready(function () {
    TMTalk.init();
    var tmTalk = ASC.TMTalk,
        config = tmTalk.Config,
        resource = tmTalk.Resources;

    tmTalk.properties.init("2.0");
    tmTalk.iconManager.init();
    tmTalk.notifications.init("userphoto.ashx", "notification.html");
    tmTalk.msManager.init(config.validSymbols);
    tmTalk.mucManager.init(config.validSymbols);
    tmTalk.roomsManager.init();
    tmTalk.contactsManager.init();
    tmTalk.messagesManager.init(resource.ShortDateFormat, resource.FullDateFormat, tmTalk.abbreviatedMonthNames, config.historyLength, tmTalk.abbreviatedDayNames);
    tmTalk.connectionManager.init(config.boshUri, config.jabberAccount, config.resourcePriority, config.clientInactivity);
    tmTalk.properties.item("addonID", config.addonID);
    tmTalk.properties.item("enabledMassend", config.enabledMassend);
    tmTalk.properties.item("enabledConferences", config.enabledConferences);
    tmTalk.properties.item("requestTransportType", config.requestTransportType);
    tmTalk.properties.item("fileTransportType", config.fileTransportType);
    tmTalk.properties.item("maxUploadSize", config.maxUploadSize);
    tmTalk.properties.item("maxUploadSizeError", tmTalk.maxUploadSizeError);
    tmTalk.properties.item("sounds", config.sounds);
    tmTalk.properties.item("soundsHtml", config.soundsHtml);

    tmTalk.properties.item("expressInstall", config.expressInstall);
    //hack for artifact css
    jq('#talkWrapper').removeClass('hide');
    
    window.addEventListener('storage', function (event) {
        if (event.key == 'onlyoffice') {
            try {
                setTimeout(function () {
                    window.name = null;
                    location = event.url;
                    localStorage.removeItem('openedRooms');
                }, 300);
            } catch(e) {}
        }
    });
    var isConnected = true;
    var isTalkConnected = true;
    var isNeedReload = true;
    var countRetry = 0;
    var talkDate = null;

    var updateTalkDate = function () {
        var now = new Date();

        var yyyy = now.getFullYear().toString();
        var mm = (now.getMonth() + 1).toString();
        var dd = now.getDate().toString();
        var nowStr = [
            yyyy,
            (mm[1] ? mm : "0" + mm[0]),
            (dd[1] ? dd : "0" + dd[0])
        ].join('-');

        talkDate = talkDate != null ? talkDate : nowStr;
       
        if (talkDate != nowStr) {
            talkDate = nowStr;
            ASC.TMTalk.messagesManager.updateOpenRoomsHistory();
        }
    };
    function reconnect() {
        setTimeout(function () {
            ASC.TMTalk.connectionManager.conflict = false;
            if (ASC.TMTalk.connectionManager.status().id === 0) {
                ASC.TMTalk.connectionManager.terminate();
                setTimeout(function () {
                    ASC.TMTalk.connectionManager.status(1);
                }, 1000);
            }
        }, 1000);
    }
    jQuery(window).focus(function () { reconnect();});
    jQuery(window).click(function () { reconnect();});

    setInterval(function () {
        if (!isConnected && navigator.onLine && !ASC.TMTalk.connectionManager.conflict && ASC.TMTalk.connectionManager.status().id == 0) {
            isNeedReload = false;
            setTimeout(function () {
               ASC.TMTalk.connectionManager.terminate();
               location.reload();
            }, 10000);
        } else if (ASC.TMTalk.connectionManager.connected() && jQuery('#talkStatusMenu').hasClass('processing')) {
            if (isTalkConnected) {
                isTalkConnected = false;
               
                setTimeout(function () {
                    ASC.TMTalk.connectionManager.status(0);
                    isTalkConnected = true;
                }, 1000);
            }
        } else if (!ASC.TMTalk.connectionManager.conflict && navigator.onLine && ASC.TMTalk.connectionManager.status().id == 0 && !ASC.TMTalk.connectionManager.connected()) {
            if (countRetry == 10 && isNeedReload) {
                ASC.TMTalk.connectionManager.terminate();
                location.reload();
            }
            countRetry++;
        }
        isConnected = navigator.onLine;
        updateTalkDate();
    }, 1000);
});