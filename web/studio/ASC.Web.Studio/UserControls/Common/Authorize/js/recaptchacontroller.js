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