/// <reference path="jquery-1.3.2.js" />
/// <reference path="jquery.validate.js" />

// register custom jQuery methods

jQuery.validator.addMethod("regex", function(value, element, params) {
    if (this.optional(element)) {
        return true;
    }

    var match = new RegExp(params).exec(value);
    return (match && (match.index == 0) && (match[0].length == value.length));
});

// glue

function __MVC_ApplyValidator_Range(object, min, max) {
    object["range"] = [min, max];
}

function __MVC_ApplyValidator_RegularExpression(object, pattern) {
    object["regex"] = pattern;
}

function __MVC_ApplyValidator_Required(object) {
    object["required"] = true;
}

function __MVC_ApplyValidator_StringLength(object, maxLength) {
    object["maxlength"] = maxLength;
}

function __MVC_ApplyValidator_Unknown(object, validationType, validationParameters) {
    object[validationType] = validationParameters;
}

function __MVC_CreateFieldToValidationMessageMapping(validationFields) {
    var mapping = {};

    for (var i = 0; i < validationFields.length; i++) {
        var thisField = validationFields[i];
        mapping[thisField.FieldName] = "#" + thisField.ValidationMessageId;
    }

    return mapping;
}

function __MVC_CreateErrorMessagesObject(validationFields) {
    var messagesObj = {};

    for (var i = 0; i < validationFields.length; i++) {
        var thisField = validationFields[i];
        var thisFieldMessages = {};
        messagesObj[thisField.FieldName] = thisFieldMessages;
        var validationRules = thisField.ValidationRules;

        for (var j = 0; j < validationRules.length; j++) {
            var thisRule = validationRules[j];
            if (thisRule.ErrorMessage) {
                var jQueryValidationType = thisRule.ValidationType;
                switch (thisRule.ValidationType) {
                    case "regularExpression":
                        jQueryValidationType = "regex";
                        break;

                    case "stringLength":
                        jQueryValidationType = "maxlength";
                        break;
                }

                thisFieldMessages[jQueryValidationType] = thisRule.ErrorMessage;
            }
        }
    }

    return messagesObj;
}

function __MVC_CreateRulesForField(validationField) {
    var validationRules = validationField.ValidationRules;

    // hook each rule into jquery
    var rulesObj = {};
    for (var i = 0; i < validationRules.length; i++) {
        var thisRule = validationRules[i];
        switch (thisRule.ValidationType) {
            case "range":
                __MVC_ApplyValidator_Range(rulesObj,
                    thisRule.ValidationParameters["minimum"], thisRule.ValidationParameters["maximum"]);
                break;

            case "regularExpression":
                __MVC_ApplyValidator_RegularExpression(rulesObj,
                    thisRule.ValidationParameters["pattern"]);
                break;

            case "required":
                __MVC_ApplyValidator_Required(rulesObj);
                break;

            case "stringLength":
                __MVC_ApplyValidator_StringLength(rulesObj,
                    thisRule.ValidationParameters["maximumLength"]);
                break;

            default:
                __MVC_ApplyValidator_Unknown(rulesObj,
                    thisRule.ValidationType, thisRule.ValidationParameters);
                break;
        }
    }

    return rulesObj;
}

function __MVC_CreateValidationOptions(validationFields) {
    var rulesObj = {};
    for (var i = 0; i < validationFields.length; i++) {
        var validationField = validationFields[i];
        var fieldName = validationField.FieldName;
        rulesObj[fieldName] = __MVC_CreateRulesForField(validationField);
    }

    return rulesObj;
}

function __MVC_EnableClientValidation(validationContext) {
    // this represents the form containing elements to be validated
    var theForm = $("#" + validationContext.FormId);

    var fields = validationContext.Fields;
    var rulesObj = __MVC_CreateValidationOptions(fields);
    var fieldToMessageMappings = __MVC_CreateFieldToValidationMessageMapping(fields);
    var errorMessagesObj = __MVC_CreateErrorMessagesObject(fields);

    var options = {
        errorClass: "input-validation-error",
        errorElement: "span",
        errorPlacement: function(error, element) {
            var messageSpan = fieldToMessageMappings[element.attr("name")];
            $(messageSpan).empty();
            $(messageSpan).removeClass("field-validation-valid");
            $(messageSpan).addClass("field-validation-error");
            error.removeClass("input-validation-error");
            error.attr("_for_validation_message", messageSpan);
            error.appendTo(messageSpan);
        },
        messages: errorMessagesObj,
        rules: rulesObj,
        success: function(label) {
            var messageSpan = $(label.attr("_for_validation_message"));
            $(messageSpan).empty();
            $(messageSpan).addClass("field-validation-valid");
            $(messageSpan).removeClass("field-validation-error");
        }
    };

    // register callbacks with our AJAX system
    var formElement = document.getElementById(validationContext.FormId);
    var registeredValidatorCallbacks = formElement.validationCallbacks;
    if (!registeredValidatorCallbacks) {
        registeredValidatorCallbacks = [];
        formElement.validationCallbacks = registeredValidatorCallbacks;
    }
    registeredValidatorCallbacks.push(function() {
        theForm.validate();
        return theForm.valid();
    });

    theForm.validate(options);
}

// need to wait for the document to signal that it is ready
$(document).ready(function() {
    var allFormOptions = window.mvcClientValidationMetadata;
    if (allFormOptions) {
        while (allFormOptions.length > 0) {
            var thisFormOptions = allFormOptions.pop();
            __MVC_EnableClientValidation(thisFormOptions);
        }
    }
});
