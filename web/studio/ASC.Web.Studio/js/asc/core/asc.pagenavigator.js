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
            jq(".mainPageContent").scrollTop(0);
        };
    };
};