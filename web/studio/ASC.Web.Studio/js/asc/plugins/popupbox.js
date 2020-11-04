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
        if (params.hasOwnProperty("tmplParams")) {
            this.tmplParams = params.tmplParams;
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
        data.isPrivateForMe = data.isPrivate && !ASC.CRM.Data.IsCRMAdmin;
        if (data.isPrivate && data.accessList) {
            for (var i = 0; i < data.accessList.length; i++) {
                if (data.accessList[i].id == Teamlab.profile.id) {
                    data.isPrivateForMe = false;
                }
            }
        }

        if (ppb.tmplParams) {
            data = jq.extend(data, ppb.tmplParams);
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