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


window.PagesNavigation = (function($) {
    var visiblePageCount = 3;

    var changeSysfolderPageCallback = function(number) {

        var folderId = MailFilter.getFolder();
        var anchor = "#" + (folderId !== TMMail.sysfolders.userfolder.id
            ? TMMail.getSysFolderNameById(folderId)
            : TMMail.getSysFolderNameById(folderId) + "=" + MailFilter.getUserFolder());
        var prevFlag, el;
        var messagesRows = $('.messages:visible .row[data_id]');

        if (1 !== number) {
            // next button was pressed
            el = messagesRows.last();
            prevFlag = false;
        } else {
            //previous btn was pressed
            el = messagesRows.first();
            prevFlag = true;
        }

        if (commonSettingsPage.isConversationsEnabled()) {
            var date = el.attr('chain_date');
            var message = el.attr('data_id');

            anchor += MailFilter.toAnchor(
                true,
                {
                    from_date: new Date(date),
                    from_message: +message,
                    prev_flag: prevFlag
                });
        } else {
            anchor += MailFilter.toAnchor(
                true,
                {
                    page: number
                });
        }


        mailBox.keepSelection(false);
        ASC.Controls.AnchorController.move(anchor);
    };

    var redrawFolderNavigationBar = function(pagerNavigator, pageSize, changePageSizeCallback, hasNext, hasPrev) {
        var page = 1;
        var pageCount = 1;

        if (commonSettingsPage.isConversationsEnabled()) {
            // fake second page for previos button if has prev
            if (true === hasPrev) {
                pageCount++;
                page = 2;
            }
            if (true === hasNext) {
                pageCount++;
            }
        } else {
            page = +MailFilter.getPage();
            pageCount = hasNext ? page + 1 : page;
        }

        var total = pageCount * pageSize;

        return redrawNavigationBar(pagerNavigator,
            page,
            pageSize,
            total,
            changeSysfolderPageCallback,
            changePageSizeCallback,
            "",
            true);
    };


    var redrawNavigationBar = function(pagerNavigator, page, pageSize, totalItemsCount,
                                       changePageCallback, changePageSizeCallback, totalItemsText, sysfolderFlag) {

        var $navigationBarDiv = $('#bottomNavigationBar');

        if (true === sysfolderFlag) {
            !$navigationBarDiv.is('.sysfolder') && $navigationBarDiv.addClass('sysfolder');
        } else {
            $navigationBarDiv.removeClass('sysfolder');
        }

        pagerNavigator.changePageCallback = changePageCallback;
        pagerNavigator.NavigatorParent = $navigationBarDiv.find('#divForMessagesPager');
        if (commonSettingsPage.isConversationsEnabled())
            pagerNavigator.VisiblePageCount = +visiblePageCount;
        pagerNavigator.EntryCountOnPage = +pageSize;

        pagerNavigator.drawPageNavigator(+page, +totalItemsCount);

        if (!sysfolderFlag) {
            $navigationBarDiv.find('#TotalItems').show();
            $navigationBarDiv.find('#totalItemsOnAllPages').show();
            $navigationBarDiv.find('#TotalItems').text(totalItemsText);
            $navigationBarDiv.find('#totalItemsOnAllPages').text(totalItemsCount);
        } else {
            // dirty hack
            var regex = /;[^\.]+.drawPageNavigator\([^\)]+\);/i;
            var prev = $navigationBarDiv.find('.pagerPrevButtonCSSClass').attr('onclick');
            prev && $navigationBarDiv.find('.pagerPrevButtonCSSClass').attr('onclick', prev.replace(regex, ';'));
            var next = $navigationBarDiv.find('.pagerNextButtonCSSClass').attr('onclick');
            next && $navigationBarDiv.find('.pagerNextButtonCSSClass').attr('onclick', next.replace(regex, ';'));
            $navigationBarDiv.find('#TotalItems').hide();
            $navigationBarDiv.find('#totalItemsOnAllPages').hide();
        }

        var $select = $navigationBarDiv.find('select');
        $select.val(pageSize).tlCombobox();

        $select.unbind('change');
        $select.change(function() { changePageSizeCallback(this.value); });

        $navigationBarDiv.show();
        decideComboUpOrDown($navigationBarDiv);

    };

    var redrawPrevNextControl = function() {
        var $prevNextDiv = $('.menu-action-simple-pagenav');

        var $navigationBarSourceDiv = $('#bottomNavigationBar');
        $prevNextDiv.html("");

        var nav = [];

        var $prevSource = $navigationBarSourceDiv.find(".pagerPrevButtonCSSClass");
        var $nextSource = $navigationBarSourceDiv.find(".pagerNextButtonCSSClass");

        if ($prevSource.length != 0) {
            nav.push($prevSource.clone());
        }

        if ($nextSource.length != 0) {
            if ($prevSource.length != 0) {
                nav.push($("<span style='padding: 0 8px;'>&nbsp;</span>").clone());
            }
            nav.push($nextSource.clone());
        }

        if (nav.length != 0) {
            $prevNextDiv.append(nav);
            $prevNextDiv.show();
        } else {
            $prevNextDiv.hide();
        }

        if ($nextSource.length != 0) {
            $nextSource.css('margin-left', $prevSource.length == 0 ? '0' : '');
        }

    };

    var decideComboUpOrDown = function($navigationBarDiv) {
        var $combo = $navigationBarDiv.find('.tl-combobox');
        var $comboDropList = $combo.find('.combobox-container');
        var directionIsUp = $('.page-menu').height() < $('.mainContainerClass').height() + $comboDropList.height();
        $combo.attr('direction_is_up', directionIsUp);
    };

    var fixAnchorPageNumberIfNecessary = function(page) {
        var anchor = ASC.Controls.AnchorController.getAnchor();

        var newAnchor = anchor.replace(/\/page=(\d+)/, "\/page=" + page);
        if (newAnchor != anchor) {
            ASC.Controls.AnchorController.safemove(newAnchor);
        }
    };

    var fixAnchorPageSizeIfNecessary = function(pageSize) {
        if (25 == pageSize || 50 == pageSize || 75 == pageSize || 100 == pageSize) {
            return false;
        }

        var anchor = ASC.Controls.AnchorController.getAnchor();
        var newAnchor = anchor.replace(/\/page_size=(\d+)/, "\/page_size=" + 25);
        ASC.Controls.AnchorController.move(newAnchor);

        return true;
    };

    return {
        RedrawNavigationBar: redrawNavigationBar,
        FixAnchorPageNumberIfNecessary: fixAnchorPageNumberIfNecessary,
        FixAnchorPageSizeIfNecessary: fixAnchorPageSizeIfNecessary,
        RedrawPrevNextControl: redrawPrevNextControl,
        RedrawFolderNavigationBar: redrawFolderNavigationBar
    };
})(jQuery);