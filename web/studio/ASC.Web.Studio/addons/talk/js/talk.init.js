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