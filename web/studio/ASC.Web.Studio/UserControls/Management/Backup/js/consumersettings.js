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


window.ConsumerStorageSettings = new function () {
    var encryptionTypes = [
        { id: "none", title: ASC.Resources.Master.TemplateResource.ConsumersS3EncryprionNone },
        { id: "server", title: ASC.Resources.Master.TemplateResource.ConsumersS3EncryprionSSE },
        { id: "client", title: ASC.Resources.Master.TemplateResource.ConsumersS3EncryprionCSE }
    ];

    var encryptionMethods = [
        { id: "", title: ASC.Resources.Master.TemplateResource.ConsumersS3EncryprionNone, type: "none", visible: false, key: false, kms: false },
        { id: "aes256", title: ASC.Resources.Master.TemplateResource.ConsumersS3EncryprionSSES3, type: "server", visible: true, key: false, kms: false, description: ASC.Resources.Master.TemplateResource.ConsumersS3EncryprionSSES3Help },
        { id: "awskms", title: ASC.Resources.Master.TemplateResource.ConsumersS3EncryprionSSEKMS, type: "server", visible: true, key: false, kms: true, description: ASC.Resources.Master.TemplateResource.ConsumersS3EncryprionSSEKMSHelp },
        { id: "clientawskms", title: "Client Kms", type: "client", visible: false, key: true, kms: false },
        //{ id: "clientaes", title: "Client Aes", type: "client", visible: false, key: true, kms: false },
        //{ id: "clientrsa", title: "Client Rsa", type: "client", visible: false, key: true, kms: false }
    ];

    var regionsS3 = [];

    var displayNoneClass = "display-none",
        withErrorClass = "with-error",
        disableClass = "disable",
        textBoxClass = ".textBox",
        comboBoxStorageClass = ".comboBoxStorage",
        comboBoxEncryptionClass = ".comboBoxEncryption",
        encryptionMethodClass = ".encryption-method",
        encryptionKmsClass = ".encryption-kms",
        encryptionKeyClass = ".encryption-key";

    function initS3Regions(regions) {
        regionsS3 = regions;
    }

    function bindEvents($box, $btn, consumer) {

        $box.off("change" + comboBoxStorageClass).on("change" + comboBoxStorageClass, comboBoxStorageClass, function () {
            $box.find(textBoxClass).removeClass(withErrorClass).val("");
            $box.find(".storage").addClass(displayNoneClass);
            $box.find(".storage[data-id='" + this.value + "']").removeClass(displayNoneClass);
            $btn.addClass(disableClass);
        });

        $box.find(comboBoxStorageClass).val(consumer.id).trigger("change");

        $box.off("input" + textBoxClass).on("input" + textBoxClass, textBoxClass, function () {
            var $self = jq(this).removeClass(withErrorClass);
            var $storage = $self.parents(".storage");
            var $siblings = Array.from($storage.find("input.requiredField" + textBoxClass + ":visible"));

            function notEmpty(item) {
                return item.value && item.value.length > 0;
            }

            if ($siblings.every(notEmpty)) {
                $btn.removeClass(disableClass);
            } else {
                $btn.addClass(disableClass);
            }
        });

        $box.off("change" + comboBoxEncryptionClass).on("change" + comboBoxEncryptionClass, comboBoxEncryptionClass, function () {
            $box.find(encryptionMethodClass).addClass(displayNoneClass);
            $box.find(encryptionKmsClass).addClass(displayNoneClass);
            $box.find(encryptionKeyClass).addClass(displayNoneClass);

            var type = this.value;
            var $firstOfType = null;

            for (var i = 0; i < encryptionMethods.length; i++) {
                var method = encryptionMethods[i];
                var $element = $box.find(encryptionMethodClass + " input[value='" + method.id + "']");

                if ($firstOfType == null && method.type == type) {
                    $firstOfType = $element;
                }

                if (!method.visible || method.type != type) {
                    $element.parent().hide();
                } else {
                    $element.parent().show();
                    $box.find(encryptionMethodClass).removeClass(displayNoneClass);
                }
            }

            $firstOfType.prop("checked", true).trigger("change");
        });

        $box.off("change" + encryptionMethodClass).on("change" + encryptionMethodClass, encryptionMethodClass + " input", function () {
            var val = this.value;
            var method = encryptionMethods.find(item => item.id == val);
            $box.find(encryptionKeyClass).toggleClass(displayNoneClass, !method.key);

            if (method.kms) {
                $box.find(encryptionKmsClass).removeClass(displayNoneClass);
                $box.find(encryptionKmsClass + " select").val(0).trigger("change");
            } else {
                $box.find(encryptionKmsClass).addClass(displayNoneClass);
            }
        });

        $box.off("change" + encryptionKmsClass).on("change" + encryptionKmsClass, encryptionKmsClass + " select", function () {
            $box.find(encryptionKeyClass).toggleClass(displayNoneClass, this.value == 0);
        });

        $box.find('.HelpCenterSwitcher').off('click').on('click', function (e) {
            var switcher = jq(this);
            switcher.helper({ BlockHelper: switcher.next() });
            e.preventDefault();
        });
    }

    function setProps($box, consumer) {
        for (var i = 0; i < consumer.properties.length; i++) {
            var prop = consumer.properties[i];
            var $element = $box.find("[data-id='" + prop.name + "']");

            switch ($element.get(0).type) {
                case "checkbox":
                    $element.prop("checked", prop.value == "true");
                    break;
                case "radio":
                    var method = encryptionMethods.find(item => item.id == prop.value) || encryptionMethods[0];
                    $box.find(".comboBoxEncryption").val(method.type).trigger("change");
                    $box.find("[data-id='" + prop.name + "'][value='" + prop.value + "']").prop("checked", true).trigger("change");
                    if (method.kms) {
                        var key = consumer.properties.find(item => item.name == "ssekey");
                        $box.find(".encryption-kms select").val(key && key.value ? 1 : 0).trigger("change");
                    }
                    break;
                case "select-one":
                    $element.val(prop.value || $element.find("option:first").val());
                    break;
                default:
                    $element.val(prop.value);
                    break;
            }
        }
    }

    function getProps($box) {
        var storageProps = [];
        var hasError = false;

        var $settings = $box.find(".textBox[data-id]:visible, select[data-id], input[type=checkbox][data-id], input[type=radio][data-id]:checked");

        $settings.each(function () {
            switch (this.type) {
                case "checkbox":
                    storageProps.push({ key: this.dataset.id, value: this.checked });
                    break;
                default:
                    if (this.value) {
                        storageProps.push({ key: this.dataset.id, value: this.value });
                    }
                    if (this.classList.contains("requiredField") && !this.value) {
                        this.classList.add(withErrorClass);
                        hasError = true;
                    }
                    break;
            }
        });

        return hasError ? null : storageProps;
    }

    function getTmplData(data, name) {
        return Object.assign({}, data, {
            regions: regionsS3,
            encryptionTypes: encryptionTypes,
            encryptionMethods: encryptionMethods,
            settingName: name || ""
        });
    }

    return {
        initS3Regions: initS3Regions,
        bindEvents: bindEvents,
        setProps: setProps,
        getProps: getProps,
        getTmplData: getTmplData
    }
};