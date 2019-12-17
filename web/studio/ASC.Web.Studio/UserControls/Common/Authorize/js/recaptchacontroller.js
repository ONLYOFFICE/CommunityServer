;
var RecaptchaController = new function () {
    this.InitRecaptcha = function (datahl) {
        if (!datahl || typeof  datahl !== "string") {
            datahl = "en";
        }

        var script = document.createElement("script");
        script.type = "text/javascript";
        script.async = false;
        script.src = "https://www.google.com/recaptcha/api.js?hl=" + datahl;

        var s = document.getElementsByTagName("script")[0];
        s.parentNode.insertBefore(script, s);
    };
};