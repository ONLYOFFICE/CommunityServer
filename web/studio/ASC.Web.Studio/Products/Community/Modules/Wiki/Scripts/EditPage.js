/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
