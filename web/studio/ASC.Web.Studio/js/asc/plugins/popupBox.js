/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


PopupBoxManager = new function() {
    this.Collection = new Array();
    this.TimerCollection = new Array();

    this.RegistryPopupBox = function(popupBox) {
        this.Collection[popupBox.ID] = popupBox;
    };

    this.GetPopupBoxByID = function(id) {
        return this.Collection[id];
    };

    this.AjaxCallback = function(result) { };
};

var PopupBoxContainerElementID = 'container';

PopupBox = function(id, width, height, backgroundCSSClass, borderCSSClass, serverMethodFullName, params) {
    this.Height = height;
    this.Width = width;

    this.BorderCSSClass = borderCSSClass;
    this.BackgroundCSSClass = backgroundCSSClass;

    this.ID = id;
    this.ContentID = '_' + id;
    this.IsVisible = false;

    this.TimerHandler = null;
    this.CurrentElementID = '';
    this.PreviousElementID = '';

    this.VisibleWidth = -1;
    this.VisibleHeight = -1;

    this.ServerMethodFullName = serverMethodFullName;
    if (typeof (params) === "object") {
        if (params.hasOwnProperty("apiMethodName")) {
            this.ApiMethodName = params.apiMethodName;
        }
        if (params.hasOwnProperty("tmplName")) {
            this.TmplName = params.tmplName;
        }
        if (params.hasOwnProperty("customFactory")) {
            this.CustomFactory = params.customFactory;
        }
    }

    this.BeginBox = '<div class="' + this.BorderCSSClass + ' ' + this.BackgroundCSSClass + '" style="z-index:9999999; position:absolute; display:none; width:' + this.Width + 'px;" id=' + this.ID + '>';
    this.EndBox = '</div>';
    this.Content = '<div id=' + this.ContentID + '></div>';

    this.Init = function() {
        jq('#' + PopupBoxContainerElementID).append(this.BeginBox + this.Content + this.EndBox);
    };


    PopupBoxManager.RegistryPopupBox(this);
    if (jq('#' + PopupBoxContainerElementID).length > 0) {
        this.Init();
    } else {
        var popupBoxID = this.ID;
        jq(document).ready(function() {
            PopupBoxManager.GetPopupBoxByID(popupBoxID).Init();
        });
    };

    this.SetContent = function(content) {
        jq('#' + this.ContentID).html(content);
        jq('#' + this.ID).css({
            'width': this.Width + 'px'
        });

    };

    this.CheckVisible = function () {
        var self = this;

        if (self.IsVisible == false) {//popup should be hidden
            clearInterval(self.checkVisibleHandler);
            self.checkVisibleHandler = null;

            if (jq('#' + self.ID).is(':visible')) {
                jq("#" + self.ID).fadeOut("slow");
            }
        } else {
            self.checkVisibleHandler = setTimeout(function () {
                self.CheckVisible();
            }, 313);
        }
    };

    this.RegistryElement = function(idElement, methodParams) {
        var popupBoxID = this.ID;
        jq('#' + idElement)
            .on("mouseover", function () {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).ExecAndShow(idElement, methodParams);
            })
            .on("mouseout", function () {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
            })
            .on("click", function () {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
            });

        jq(document).on("keypress", function (evt) {
            if (!(evt.hasOwnProperty("ctrlKey") && evt.ctrlKey === true && (evt.which === 99 || evt.which === 97))) {//ctrl + c and ctrl + a
                PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
            }
        });
    };

    this.RegistryElementOnLive = function(idElement, methodParams) {
        var popupBoxID = this.ID;
        jq(document)
            .on('mouseover', '#' + idElement, function () {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).ExecAndShow(idElement, methodParams);
            })
            .on('mouseout', '#' + idElement, function () {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
            })
            .on('click', '#' + idElement, function () {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
            });

        jq(document).on("keypress", function (evt) {
            if (!(evt.hasOwnProperty("ctrlKey") && evt.ctrlKey === true && (evt.which === 99 || evt.which === 97))) {//ctrl + c and ctrl + a
                PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
            }
        });
    };

    this.ExecAndShow = function(idElement, methodParams) {
        this.CurrentElementID = idElement;
        if (this.CurrentElementID == this.PreviousElementID && 1 == 2) {
            setTimeout("PopupBoxManager.GetPopupBoxByID('" + this.ID + "').Show('" + idElement + "')", 850);
            return;
        } else {
            if (this.TimerHandler)
                clearInterval(this.TimerHandler);

            this.PreviousElementID = this.CurrentElementID;

            if (typeof (this.ApiMethodName) != "undefined" && this.ApiMethodName != "") {
                this.TimerHandler = setTimeout("PopupBoxManager.GetPopupBoxByID('" + this.ID + "').ExecApiCode('" + idElement + "','" + methodParams + "')", 850);
            } else {
                this.TimerHandler = setTimeout("PopupBoxManager.GetPopupBoxByID('" + this.ID + "').ExecAjaxCode('" + idElement + "','" + methodParams + "')", 850);
            }
        }
    };

    this.ExecApiCode = function(idElement, methodParams) {
        var popupBoxID = this.ID;
        eval(this.ApiMethodName + "({popupBoxID: \"" + this.ID + "\"}," + methodParams + "," +
        "{" +
        "success:" + this.ApiCallback + "," +
        "error:" + function () {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).StopTimer();
            } + "," +
        "before:" + function() {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).SetContent('<img class="popupbox-loader" src="' + StudioManager.GetImage('loader_32.gif') + '" alt="">');
                PopupBoxManager.GetPopupBoxByID(popupBoxID).Show(idElement);
            }
        +"})");
    };

    this.ExecAjaxCode = function(idElement, methodParams) {
        var popupBoxID = this.ID;
        AjaxPro.onLoading = function(b) {
            if (b) {
                PopupBoxManager.GetPopupBoxByID(popupBoxID).SetContent('<img class="popupbox-loader" src="' + StudioManager.GetImage('loader_32.gif') + '" alt="">');
                PopupBoxManager.GetPopupBoxByID(popupBoxID).Show(idElement);
            }
        };
        eval(this.ServerMethodFullName + "(" + methodParams + ",\"" + this.ID + "\",this.AjaxCallback);");
    };

    this.AjaxCallback = function(result) {
        AjaxPro.onLoading = function(b) { };
        var ppb = PopupBoxManager.GetPopupBoxByID(result.value.rs1);
        ppb.SetContent(result.value.rs2);
        ppb.SetPopupPosition(ppb.CurrentElementID);
    };

    this.ApiCallback = function(params, data) {
        var ppb = PopupBoxManager.GetPopupBoxByID(params.popupBoxID);
        if (typeof (ppb.CustomFactory) === "function") {
            ppb.CustomFactory.apply(this, [data]);
        }
        data.isPrivateForMe = data.isPrivate;
        if (data.isPrivate && data.accessList) {
            for (var i = 0; i < data.accessList.length; i++) {
                if (data.accessList[i].id == Teamlab.profile.id) {
                    data.isPrivateForMe = false;
                }
            }
        }
        var $newContent = jq.tmpl(ppb.TmplName, data);
        ppb.SetContent($newContent);
        ppb.SetPopupPosition(ppb.CurrentElementID);
    };

    this.SetPopupPosition = function (idElement) {
        if (jq('#' + idElement).length == 0) return;
        var ppbHeight = jq('#' + this.ID).height(),

            w = jq(window),
            TopPadding = w.scrollTop(),
            LeftPadding = w.scrollLeft(),
            ScrWidth = w.width(),
            ScrHeight = w.height(),

            idBox = this.ID,
            pos = jq('#' + idElement).offset(),
            elHeight = jq('#' + idElement).height(),

            left = "",
            top = "";

        if ((pos.left + this.Width) > (LeftPadding + ScrWidth)) {
            left = pos.left - this.Width + 'px';
        } else {
            left = pos.left + 'px';
        }

        if ((pos.top + ppbHeight + elHeight) > (TopPadding + ScrHeight)) {
            top = pos.top - 5 - ppbHeight + 'px';
        } else {
            top = pos.top + 3 + elHeight + 'px'
        }
        jq('#' + idBox).css({
            'left': left,
            'top': top
        });
    };

    this.Show = function(idElement) {
        this.SetPopupPosition(idElement);

        var idBox = this.ID,
            $box = jq('#' + idBox);
        this.VisibleWidth = -1;
        this.VisibleHeight = -1;

        if ($box.is(':visible') == false) {
            this.IsVisible = true;
            $box.fadeIn("slow");

            $box.on("mouseout", function () {
                PopupBoxManager.GetPopupBoxByID(idBox).SetVisible(false);
            });

            $box.on("mouseover", function () {
                PopupBoxManager.GetPopupBoxByID(idBox).SetVisible(true);
            });
        }

        this.CheckVisible();
    };

    this.SetVisible = function(state) {
        if (jq('#' + this.ID).is(':visible')) {
            this.IsVisible = state;
        }
    };

    this.StopTimer = function() {
        try {
            this.IsVisible = false;
            clearInterval(this.TimerHandler);
            this.TimerHandler = null;
        }
        catch (e) { };
    };
};