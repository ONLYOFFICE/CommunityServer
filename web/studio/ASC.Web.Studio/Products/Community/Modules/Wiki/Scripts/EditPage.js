/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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