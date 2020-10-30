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


ASC.VoipNavigationItem = (function () {
    var isInit = false;

    function init() {
        if (!ASC.SocketIO || ASC.SocketIO.disabled()) {
            return;
        }

        jq(".studio-top-panel .voip").removeClass("display-none");
        isInit = true;

        jq(".voipActiveBox").click(function () {
            open();
        });
    }

    function open(contactId) {
        var hWnd = null,
            isExist,
            path = "/VoipClient.aspx",
            winName = 'ASCVoipClient' + location.hostname,
            params = 'ontouchend' in document ? '' : 'width=350,height=500,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no';

        try {
            hWnd = window.open('', winName, params);
        } catch (err) {
        }

        try {
            isExist = !hWnd || typeof hWnd.ASC === 'undefined' ? false : true;
        } catch (err) {
            isExist = true;
        }

        if (!isExist) {
            hWnd = window.open(path, winName, params);
        }

        if (contactId) {
            setTimeout(function () {
                hWnd.ASC.CRM.Voip.PhoneView.makeCallToContact(contactId);
            }, !isExist ? 3000 : 0);
        }

        try {
            if (hWnd) {
                hWnd.focus();
            }
        } catch (err) {
        }

        return hWnd;
    }

    function call(contactId) {
        if (!isInit) return;

        open(contactId);
    }

    return {
        init: init,
        call: call,
        get isInit() {
            return isInit;
        }
    }
})();

jq(document).ready(function () {
    ASC.VoipNavigationItem.init();
});