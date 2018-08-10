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


AnswerVariantPrototype = function (id, name, voteCount) {
    this.ID = id;
    this.Name = name;
    this.VoteCount = voteCount;
};

VotingPollPrototype = function (varName, voteHandlerType, pollID, emptySelectText,
                                statBarCSSClass, liderBarCSSClass, variantNameCSSClass, voteCountCSSClass,
                                additionalParams) {
    this.VarName = varName;
    this.VoteHandlerType = voteHandlerType;
    this.PollID = pollID;
    this.EmptySelect = emptySelectText;
    this.AnswerVariants = new Array();
    this.AdditionalParams = additionalParams;

    this.StatBarCSSClass = statBarCSSClass;
    this.LiderBarCSSClass = liderBarCSSClass;
    this.VariantNameCSSClass = variantNameCSSClass;
    this.VoteCountCSSClass = voteCountCSSClass;

    this.RegistryVariant = function (variant) {
        for (var i = 0; i < this.AnswerVariants.length; i++) {
            if (this.AnswerVariants[i].ID == variant.ID)
                return;
        }
        this.AnswerVariants.push(variant);
    };

    this.Vote = function () {
        var ids = new String();
        jq.each(jq("input:checked[name='__pollForm_" + this.PollID + "_av']"), function (i, n) {
            if (i != 0)
                ids += ",";

            ids += jq(this).val();
        });

        jq('#__pollForm_' + this.PollID + '_result').html('');

        if (ids == '') {
            jq('#__pollForm_' + this.PollID + '_result').html('<div class="errorBox">' + this.EmptySelect + '</div>');
            return;
        }

        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.showLoaderBtn('#__pollForm_' + this.PollID + '_voteBox');
            else
                LoadingBanner.hideLoaderBtn('#__pollForm_' + this.PollID + '_voteBox');
        };

        PollFormControl.Vote(this.VoteHandlerType, this.PollID, ids, this.AnswerVariants,
            this.StatBarCSSClass, this.LiderBarCSSClass, this.VariantNameCSSClass, this.VoteCountCSSClass,
            this.AdditionalParams, function (result) {
                var res = result.value;
                if (res.Success == '1') {
                    jq('#__pollForm_' + res.PollID + '_answButtonBox').hide();
                    jq('#__pollForm_' + res.PollID + '_voteBox').html(res.HTML);
                } else {
                    jq('#__pollForm_' + res.PollID + '_result').html('<div class="errorBox">' + res.Message + '</div>');
                }
            });
    };
};
PollMaster = new function () {
    this.AddAnswerVariant = function (uniqueID, variantClass, labelClass, editClass) {
        var items = jq('#__poll_' + uniqueID + '_qbox').children();
        var numb = items.length + 1;
        var leftMargin = 10;
        if (numb > 9)
            leftMargin = 3;
        var answerId = "__poll_" + uniqueID + "_q" + numb;
        var sb = '<div class="' + variantClass + '"><label for="__poll_' + uniqueID + '_q' + numb + '" style="float:left;" class="' + labelClass + '">' +
            jq('#__poll_' + uniqueID + '_variantCaption').val() + ' ' + numb +
            ':</label><span class="poll_remove_span_' + numb + '" onclick="PollMaster.RemoveAnswerVariant(this)"></span><input id="' + answerId + '" name="q' + numb + '" type="text" maxlength="100" class="' + editClass + '" style="margin:2px 0 0 ' + leftMargin + 'px;  float:left; width:500px"/>' +
            '<input id="__poll_' + uniqueID + '_qid' + numb + '" name="qid' + numb + '" type="hidden" value=""/>' +
            '</div>';

        jq('#__poll_' + uniqueID + '_qbox').append(sb);
        jq("input[name='q" + numb + "']").focus();
        if (numb > 2)
            jq("span[class^='poll_remove_span']").show();
        if (numb >= 15)
            jq('#__poll_' + uniqueID + '_addButton').hide();
    };

    this.RemoveAnswerVariant = function (item) {
        var itemName = jq(item).attr("class");
        var number = parseInt(itemName.substring(17));
        jq(item).parent("div").remove();
        var massVariants = jq("label[class='poll_variantLabel']");
        if (massVariants.length < 3) {
            jq("span[class^='poll_remove_span']").hide();
        }
        var text = massVariants[0].innerHTML;
        var variant = text.substring(0, text.length - 2);
        for (var i = number; i <= massVariants.length; i++) {
            massVariants[i - 1].innerHTML = variant + i + ":";
            jq(massVariants[i - 1]).parent("div").children("span[class^='poll_remove_span']").attr("class", "poll_remove_span_" + i);
            if (i < 10) {
                var textboxs = jq(massVariants[i - 1]).parent("div").children("input");
                jq(textboxs[0]).css("margin-left", "10px");

            }
        }
        var lastIndex = massVariants.length;

        if (lastIndex < 15)
            jq("div[id$='_addButton']").show();
    };
};