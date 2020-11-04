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


var MarkupControl = "xEditingArea";


function WrapSelectedMarkup(preTag, postTag) {
    var objTextArea = document.getElementById(MarkupControl).firstChild;
    if (objTextArea) {
        if (document.selection && document.selection.createRange) {

            objTextArea.focus();
            var objSelectedTextRange = document.selection.createRange();
            var strSelectedText = objSelectedTextRange.text;
            if (strSelectedText.substring(0, preTag.length) == preTag && strSelectedText.substring(strSelectedText.length - postTag.length, strSelectedText.length) == postTag) {
                objSelectedTextRange.text = strSelectedText.substring(preTag.length, strSelectedText.length - postTag.length);
            }
            else {
                objSelectedTextRange.text = preTag + strSelectedText + postTag;
            }
        }
        else {
            objTextArea.focus();
            var scrollPos = objTextArea.scrollTop;
            var selStart = objTextArea.selectionStart;
            var strFirst = objTextArea.value.substring(0, objTextArea.selectionStart);
            var strSelected = objTextArea.value.substring(objTextArea.selectionStart, objTextArea.selectionEnd);
            var strSecond = objTextArea.value.substring(objTextArea.selectionEnd);
            if (strSelected.substring(0, preTag.length) == preTag && strSelected.substring(strSelected.length - postTag.length, strSelected.length) == postTag) {
                // Remove tags
                strSelected = strSelected.substring(preTag.length, strSelected.length - postTag.length);
                objTextArea.value = strFirst + strSelected + strSecond;
                objTextArea.selectionStart = selStart;
                objTextArea.selectionEnd = selStart + strSelected.length;
            }
            else {
                objTextArea.value = strFirst + preTag + strSelected + postTag + strSecond;
                objTextArea.selectionStart = selStart;
                objTextArea.selectionEnd = selStart + preTag.length + strSelected.length + postTag.length;
            }
            objTextArea.scrollTop = scrollPos;
        }
    }
    return false;
}


/*function WrapSelectedMarkupWYSIWYG(preTag, postTag) {
    insertHTML(preTag + getSelectedText() + postTag);
    return false;
}*/


function InsertSelection(type) {

}


function InsertMarkup(tag) {


    var objTextArea = document.getElementById(MarkupControl).firstChild;
    if (objTextArea) {
        if (document.selection && document.selection.createRange) {
            objTextArea.focus();
            var objSelectedTextRange = document.selection.createRange();
            var strSelectedText = objSelectedTextRange.text;
            objSelectedTextRange.text = tag + strSelectedText;
        }
        else {
            objTextArea.focus();
            var scrollPos = objTextArea.scrollTop;
            var selStart = objTextArea.selectionStart;
            var strFirst = objTextArea.value.substring(0, objTextArea.selectionStart);
            var strSecond = objTextArea.value.substring(objTextArea.selectionStart);
            objTextArea.value = strFirst + tag + strSecond;
            objTextArea.selectionStart = selStart + tag.length;
            objTextArea.selectionEnd = selStart + tag.length;
            objTextArea.scrollTop = scrollPos;
        }
    }
    return false;
}
