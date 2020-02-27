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


if (typeof window.ASC === 'undefined') {
    window.ASC = {};
}
if (typeof window.ASC.Controls == 'undefined') {
    ASC.Controls = {};
}

window.ASC.Controls.PageNavigator = new function () {
    this.init = function (
                    objName,
                    navigatorParent,
                    entryCountOnPage,
                    visiblePageCount,
                    currentPageNumber,
                    previousButton,
                    nextButton) {
        this.ObjName = objName;
        this.EntryCountOnPage = entryCountOnPage;
        this.VisiblePageCount = visiblePageCount;
        this.CurrentPageNumber = currentPageNumber;
        this.PreviousButton = previousButton;
        this.NextButton = nextButton;

        this.NavigatorParent = navigatorParent;
        this.TotalPageCount = 0;

        this.changePageCallback = function () {
        };

        this.drawPageNavigator = function (currentPageNumber, countTotal) {
            this.CurrentPageNumber = currentPageNumber;
            var amountPage = (countTotal / this.EntryCountOnPage).toFixed(0) * 1;
            if (amountPage - (countTotal / this.EntryCountOnPage) < 0) {
                amountPage++;
            }
            this.TotalPageCount = amountPage;
            if (amountPage == 0) {
                jq(this.NavigatorParent).html("");
                return;
            }

            if (currentPageNumber < 1) {
                currentPageNumber = 1;
            }
            if (currentPageNumber > amountPage) {
                currentPageNumber = amountPage;
            }

            //Navigator
            var startPage = currentPageNumber - (this.VisiblePageCount / 2).toFixed(0) * 1;

            if (startPage + this.VisiblePageCount > amountPage) {
                startPage = amountPage - this.VisiblePageCount;
            }
            if (startPage < 0) {
                startPage = 0;
            }
            var endPage = startPage + this.VisiblePageCount;

            if (endPage > amountPage) {
                endPage = amountPage;
            }
            var sb = new String("<div class='pagerNavigationLinkBox'>");

            //button Prev
            if (currentPageNumber > 1)
                sb += "<a class='pagerPrevButtonCSSClass' href='javascript:void(0)' onclick='" + this.ObjName + ".changePageCallback(" + (currentPageNumber - 1) + "); " + this.ObjName + ".drawPageNavigator(" + (currentPageNumber - 1) + "," + countTotal + ");return false;'>" + this.PreviousButton + "</a>";

            for (var i = startPage; i < endPage && endPage - startPage > 1; i++) {
                //to 1 page
                if (i == startPage && i != 0) {
                    sb += "<a class='pagerNavigationLinkCSSClass' href='javascript:void(0)' onclick='" + this.ObjName + ".changePageCallback(1); " + this.ObjName + ".drawPageNavigator(1, " + countTotal + ");return false;'>1</a>";
                    if (i != 1) {
                        sb += "<span class='splitter'>...</span>";
                    }
                }
                if ((currentPageNumber - 1) == i) {
                    sb += "<span class='pagerCurrentPosition'>" + currentPageNumber + "</span>";
                } else {
                    sb += "<a class='pagerNavigationLinkCSSClass' href='javascript:void(0)' onclick='" + this.ObjName + ".changePageCallback(" + (i + 1) + "); " + this.ObjName + ".drawPageNavigator(" + (i + 1) + "," + countTotal + ");return false;'>" + (i + 1) + "</a>";
                }
                //button to the end
                if (i == endPage - 1 && i != amountPage - 1) {
                    if (i != amountPage - 2) {
                        sb += "<span class='splitter'>...</span>";
                    }
                    sb += "<a class='pagerNavigationLinkCSSClass' href='javascript:void(0)' onclick='" + this.ObjName + ".changePageCallback(" + amountPage + "); " + this.ObjName + ".drawPageNavigator(" + amountPage + "," + countTotal + ");return false;'>" + amountPage + "</a>";
                }
            }

            //button Next
            if (currentPageNumber != amountPage && amountPage != 1) {
                sb += "<a class='pagerNextButtonCSSClass' href='javascript:void(0)' onclick='" + this.ObjName + ".changePageCallback(" + (currentPageNumber + 1) + "); " + this.ObjName + ".drawPageNavigator(" + (currentPageNumber + 1) + "," + countTotal + ");return false;'>" + this.NextButton + "</a>";
            }

            sb += "</div>";

            jq(this.NavigatorParent).html(sb);
        };
    };
};