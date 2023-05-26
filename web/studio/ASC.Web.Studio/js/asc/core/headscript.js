/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


function setCustomVh() {
    var vh = window.innerHeight * 0.01 + 'px';
    document.documentElement.style.setProperty('--vh', vh);
}

function setContentFocus(e) {
    var keyCodes = [33, 34, 35, 36, 38, 40];
    if (keyCodes.indexOf(e.keyCode) == -1) return;
    var content = document.querySelector('#studioPageContent .mainPageContent')
    if (e.target == content) return;
    if (e.target instanceof HTMLTextAreaElement) return;
    if (e.target instanceof HTMLInputElement) {
        var inputTypes = ['text', 'password', 'number', 'email'];
        if (inputTypes.indexOf(e.target.type) >= 0) return;
    }
    if (jq && jq('.popupContainerClass:visible, .advanced-selector-container:visible').length) return;
    content.setAttribute("tabindex", -1);
    content.focus();
}

window.addEventListener('resize', setCustomVh);

window.addEventListener('keydown', setContentFocus);

setCustomVh();