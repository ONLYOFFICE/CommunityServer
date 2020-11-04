new function () {
    if (window.CKEDITOR && window.CKEDITOR.dom)
        return;

    window.ckeditorConnector = (function () {
        var isReady = false;
        var listFun = new Array();

        var load = function (fun) {
            onReady(fun);

            var ckeditorScript = loadScript("ckeditor.js");
            ckeditorScript.onload = function () {
                var adapterScript = loadScript("adapters/jquery.js");
                adapterScript.onload = function () {
                    isReady = true;
                    for (var i = 0; i < listFun.length; i++) {
                        listFun[i]();
                    }
                };
            };
        };

        function onReady(fun) {
            if (typeof fun !== "function") {
                return false;
            }
            if (isReady) {
                fun();
            } else {
                listFun.push(fun);
            }
            return isReady;
        };

        function loadScript(scriptName) {
            var newScript = document.createElement("script");
            newScript.type = "text/javascript";
            newScript.src = window.CKEDITOR_BASEPATH + scriptName;
            return document.getElementsByTagName("head")[0].appendChild(newScript);
        };

        return {
            load: load
        };
    })();

    window.CKEDITOR_BASEPATH = ASC.Resources.Master.CKEDITOR_BASEPATH;
};