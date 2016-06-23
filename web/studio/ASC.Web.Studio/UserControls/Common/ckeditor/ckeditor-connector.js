new function () {
    if (window.CKEDITOR && window.CKEDITOR.dom)
        return;

    window.ckeditorConnector = (function () {
        var isReady = false;
        var listFun = new Array();

        var load = function () {
            ckeditorConnector.isReady = true;
            for (var i = 0; i < ckeditorConnector.listFun.length; i++) {
                ckeditorConnector.listFun[i]();
            }
        };

        var onReady = function (fun) {
            if (typeof fun !== "function") {
                return false;
            }
            if (ckeditorConnector.isReady) {
                fun();
            } else {
                ckeditorConnector.listFun.push(fun);
            }
            return ckeditorConnector.isReady;
        };

        return {
            isReady: isReady,
            listFun: listFun,
            load: load,
            onReady: onReady,
        };
    })();

    window.CKEDITOR_BASEPATH = ASC.Resources.Master.CKEDITOR_BASEPATH;
    var getPath = function (scriptName) {
        return window.CKEDITOR_BASEPATH + scriptName;
    };

    var loadScript = function (src) {
        var newScript = document.createElement("script");
        newScript.type = "text/javascript";
        newScript.src = src;
        return document.getElementsByTagName("head")[0].appendChild(newScript);
    };

    var ckeditorScript = loadScript(getPath("ckeditor.js"));
    ckeditorScript.onload = function () {
        var adapterScript = loadScript(getPath("adapters/jquery.js"));
        adapterScript.onload = function () {
            ckeditorConnector.load();
        };
    };
};