/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
    tmTalk.properties.item("expressInstall", config.expressInstall);
    //hack for artifact css
    jq('#talkWrapper').removeClass('hide');
    
    window.addEventListener('storage', function (event) {
        if (event.key == 'onlyoffice') {
            try {
                setTimeout(function () {
                    window.name = null;
                    location = event.url;
                }, 300);
            } catch(e) {}
        }
    });
});