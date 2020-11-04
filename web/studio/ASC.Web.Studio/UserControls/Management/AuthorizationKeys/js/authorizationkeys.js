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


if (typeof ASC === "undefined")
    ASC = {};

ASC.AuthorizationKeysManager = (function () {
    function init() {
        jq("#authKeysContainer .on_off_button:not(.disable)").click(function () {
            var switcherBtn = jq(this);
            var itemName = switcherBtn.attr("id").replace("switcherBtn", "");
            if (switcherBtn.hasClass("off")) {
                var popupDialog = jq("#popupDialog" + itemName);
                window.StudioBlockUIManager.blockUI(popupDialog, 600);
            } else {
                save(itemName, false);
            }
        });
        
        jq(".popupContainerClass .cancelButton").click(function () {
            PopupKeyUpActionProvider.CloseDialog();
        });
        
        jq(".popupContainerClass .saveButton").click(function () {
            var saveButton = jq(this);
            var itemName = saveButton.attr("id").replace("saveBtn", "");
            save(itemName, true);
        });

        jq(".popupContainerClass input.textEdit").keyup(function (key) {
            var inputObj = jq(this);
            var popupObj = inputObj.parents(".popupContainerClass");
            var saveBtn = popupObj.find(".saveButton");
            var itemName = saveBtn.attr("id").replace("saveBtn", "");
            var inputs = jq("#popupDialog" + itemName + " .auth-service-key");

            //checkParams(saveBtn, inputs); todo: need to create not required fields
            
            if ((key.keyCode || key.which) == 13) {
                var inputList = popupObj.find(".textEdit");
                
                jq.each(inputList, function (index, obj) {
                    if (inputObj.is(obj)) {
                        if (index == inputList.length - 1) {
                            saveBtn.click();
                        } else {
                            jq(inputList[index + 1]).focus();
                        }
                        return false;
                    }
                    return true;
                });
            }
        });
    };

    function save(itemName, enable) {
        var props = [];

        var keys = jq("#popupDialog" + itemName + " .auth-service-key");
        for (var i = 0; i < keys.length; i++) {
            //if (keys[i].value == "") return; //todo: need to create not required fields
            props.push({ Name: keys[i].id, Value: enable ? keys[i].value.trim() : null });
        }

        jq("#popupDialog" + itemName).block();

        window.AuthorizationKeys.SaveAuthKeys(itemName, props,
            function(result) {
                jq("#popupDialog" + itemName).unblock();
                PopupKeyUpActionProvider.CloseDialog();

                if (result.error != null) {
                    toastr.error(result.error.Message);
                } else {
                    if (result.value) {
                        toastr.success(ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage);
                        jq("#switcherBtn" + itemName).toggleClass("on off");
                    }
                }
            });
    };

function checkParams(saveBtn, keys) {
    
    var disabled = false;
    for (var i = 0; i < keys.length; i++) {
        if (keys[i].value == '') {
           disabled = true;
           break;
        }
    }

    if (!disabled) {
        saveBtn.removeClass('disabled');
    } else {
        saveBtn.addClass('disabled');
    } 
}

    return {
        init: init
    };
})();

jq(function() {
    ASC.AuthorizationKeysManager.init();
});