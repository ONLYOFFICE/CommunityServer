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


var ForumManager = new function() {
    this.All = "All";
    this.QuestionEmptyMessage = "Enter question";
    this.SubjectEmptyMessage = "Enter topic title";
    this.ApproveTopicButton = "Aloow";
    this.OpenTopicButton = "Open";
    this.CloseTopicButton = "Close";
    this.StickyTopicButton = "Stick";
    this.ClearStickyTopicButton = "Unstick";
    this.DeleteTopicButton = "Remove";
    this.EditTopicButton = "Edit";
    this.ConfirmMessage = "Are you sure?";
    this.NameEmptyString = "Enter title";
    this.SaveButton = "Save";
    this.CancelButton = "Cancel";
    this.ModuleID = '853B6EB9-73EE-438d-9B09-8FFEEDF36234';
    this.SettingsID = '';
    this.TextEmptyMessage = "Enter a message";
    this.forumEditor = null;

    this.SendMessage = function() {
        if (jq("#createPostBth").hasClass("disable"))
            return;

        var subject = '';

        if (jq('#forum_postType').val() != 0)
            subject = jq.trim(jq('#forum_subject').val());

        if (subject == '' && jq('#forum_postType').val() != 0) {
            if (jq('#forum_postType').val() == 2) {
                AddRequiredErrorText(jq("input[name$='poll_question']"), this.QuestionEmptyMessage);
                ShowRequiredError(jq("input[name$='poll_question']"));
            }
            else {
                AddRequiredErrorText(jq("input[name$='forum_subject']"), this.SubjectEmptyMessage);
                ShowRequiredError(jq("input[name$='forum_subject']"));
            }

            jq('#forum_subject').focus();
            return;
        }

        if (jq('#forum_postType').val() != 0)
            jq('#forum_subject').val(subject);

        var html = ForumManager.forumEditor.getData();
        if (jq.trim(html) == "") {
            toastr.error(this.TextEmptyMessage);
            return;
        }

        if (jq('#forum_topSubscription').length > 0) {
            if (jq('#forum_topSubscription').is(':checked'))
                jq('#forum_topSubscriptionState').val('1');
            else
                jq('#forum_topSubscriptionState').val('0');
        }

        this.BlockButtons();

        __doPostBack('', '');
    };

    this.CancelPost = function(itemID) {
        AjaxPro.onLoading = function(b) { };
        PostCreator.CancelPost(this.SettingsID, itemID, function(result) {
            window.open(result.value, '_self');
        });
    };

    this.CancelForum = function(itemID) {
        window.location.href = "Default.aspx";
    };

    this.BlockButtons = function() {
        LoadingBanner.showLoaderBtn("#post_container");
    };

    this.UnblockButtons = function() {
        LoadingBanner.hideLoaderBtn("#post_container");
    };

    this.ShowInfoMessage = function(text) {
        jq('#' + ForumContainer_PanelInfoID).html(text);
        jq('#' + ForumContainer_PanelInfoID).show();
    };

    this.PreviewEditor = function () {
        if (jq("#btnPreview").hasClass("disable")) return;
        jq('#forum_previewBoxFCK').show();
        jq('#forum_message_previewfck').html(ForumManager.forumEditor.getData());
        jq.scrollTo(jq('#forum_previewBoxFCK').position().top, { speed: 500 });
    };

    this.HidePreview = function() {
        jq('#forum_previewBoxFCK').hide();
        var scroll_to = jq('#post_container').position();
        jq.scrollTo(scroll_to.top, { speed: 500 });
    }

    this.SaveEditTopic = function() {
        var subject = jq.trim(jq('#forum_subject').val());

        if (subject == '') {
            if (jq('#forum_topicType').val() == 1)
                jq('#forum_errorMessage').html('<div class="errorBox">' + this.QuestionEmptyMessage + '</div>');
            else
                jq('#forum_errorMessage').html('<div class="errorBox">' + this.SubjectEmptyMessage + '</div>');
            jq('#forum_subject').focus();
            jq('#forum_errorMessage').scrollTo();
            return;
        }

        jq('#forum_subject').val(subject);
        document.forms[0].submit();
    };

    //---------------Tags--------------------------    

    this.SaveSearchTags = function(elementId, tagValue, tagHelp) {
        if (tagValue == '' || tagHelp == '')
            return;

        var valuesStr = jq("#" + elementId).val();

        var values = JSON.parse(valuesStr);

        values.push([tagValue, tagHelp]);

        jq("#" + elementId).val(JSON.stringify(values));
    };

    //----------------------------------------------------------  

    this.ApproveTopic = function(idTopic) {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_topic_' + idTopic).block();
            else
                jq('#forum_topic_' + idTopic).unblock();
        }
        jq('#forum_mf').hide();

        TopicManager.DoApprovedTopic(idTopic, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1')
                jq('#forum_topic_' + res.rs2).removeClass('tintDangerous');
            else {
                jq('#forum_topic_' + res.rs2).attr('class', 'errorBox');
                jq('#forum_topic_' + res.rs2).html(res.rs3);
            }
        });
    };


    this.CloseTopic = function(idTopic) {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_topic_' + idTopic).block();
            else
                jq('#forum_topic_' + idTopic).unblock();

        }
        jq('#forum_mf').hide();

        TopicManager.DoCloseTopic(idTopic, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                toastr.success(res.rs3);
                location.reload();
            }
            else {
                toastr.error(res.rs3);
            }
        });

    };

    this.StickyTopic = function(idTopic) {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_topic_' + idTopic).block();
            else
                jq('#forum_topic_' + idTopic).unblock();
        }
        jq('#forum_mf').hide();

        TopicManager.DoStickyTopic(idTopic, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                toastr.success(res.rs3);
                location.reload();
            }
            else {
                toastr.error(res.rs3);
            }
        });
    };

    this.DeleteTopic = function(idTopic) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_topic_' + idTopic).block();
            else
                jq('#forum_topic_' + idTopic).unblock();
        }
        jq('#forum_mf').hide();

        TopicManager.DoDeleteTopic(idTopic, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                toastr.success(res.rs3);
                if (jq(".forums-row").length > 1) {
                    jq('#forum_topic_' + res.rs2).remove();
                } else {
                    location.reload();
                }
            }
            else {
                toastr.error(res.rs3);
            }
        });

    };
    this.CloseTopicPost = function(idTopic) {

        TopicManager.DoCloseTopic(idTopic, this.SettingsID, function(result) {
            var res = result.value;
            //console.log(res);
            jq("#forumsActionsMenuPanel").hide();
            if (res.rs1 == '1') {
                location.reload();
            }
            else {
                jq('#post_errorMessage').html('<div class="errorBox">' + res.rs3 + '</div>');
                setTimeout("jq('#post_errorMessage').html('');", 3000);
            }
        });

    };
    this.StickyTopicPost = function(idTopic) {
        TopicManager.DoStickyTopic(idTopic, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#StickyTopic').html(res.rs4);
            }
            else {
                jq('#errorMessageTopic').html('<div class="errorBox">' + res.rs3 + '</div>');
            }
        });
    };

    this.DeleteTopicPost = function(idTopic) {
        if (!confirm(this.ConfirmMessage))
            return;

        TopicManager.DoDeleteTopic(idTopic, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                window.location.href = "topics.aspx?f=" + res.rs4;
            }
            else {
                jq('#errorMessageTopic').html('<div class="errorBox">' + res.rs3 + '</div>');
            }
        });

    };

    this.ModeratorFunctionState = 0;

    this.ShowTopicModeratorFunctions = function(idTopic, mask, status, editURL) {
        if (mask == 0)
            return;

        if (jq('#forum_mf').length == 0)
            jq('#container').append('<div style="display:none;" id="forum_mf"></div>');

        var sb = '';
        // 1 - aprroved
        // 2 - delete
        // 3 - stiky
        // 4 - close
        // 5 - move
        // 6 - edit
        if (mask & 1)
            sb += '<div class="clearFix describe-text" style="margin:5px 0px;"><a href="javascript:ForumManager.ApproveTopic(\'' + idTopic + '\')">' + this.ApproveTopicButton + '</a></div>';
        if (mask & 8) {
            //1 - close / open
            sb += '<div class="clearFix" style="margin:5px 0px;">';
            if (status & 1)
                sb += '<a class = "link" href="javascript:ForumManager.CloseTopic(\'' + idTopic + '\')">' + this.OpenTopicButton + '</a>';
            else
                sb += '<a class = "link" href="javascript:ForumManager.CloseTopic(\'' + idTopic + '\')">' + this.CloseTopicButton + '</a>';

            sb += '</div>';
        }
        if (mask & 4) {
            //2 - sticky / clearsticky
            sb += '<div class="clearFix describe-text" style="margin:5px 0px;">';
            if (status & 2)
                sb += '<a class = "link" href="javascript:ForumManager.StickyTopic(\'' + idTopic + '\')">' + this.ClearStickyTopicButton + '</a>';
            else
                sb += '<a class = "link" href="javascript:ForumManager.StickyTopic(\'' + idTopic + '\')">' + this.StickyTopicButton + '</a>';

            sb += '</div>';
        }

        if (mask & 2) {
            //delete
            sb += '<div class="clearFix describe-text" style="margin:5px 0px;">';
            sb += '<a class = "link" href="javascript:ForumManager.DeleteTopic(\'' + idTopic + '\')">' + this.DeleteTopicButton + '</a>';
            sb += '</div>';
        }
        if (mask & 16) {
            //sb+='<div class="clearFix" style="height:2px;"></div>';     
            //sb+='<a class="button.blue" style="width:110px;" href="javascript:showMoveTopic(\''+id+'\',\''+alMask+'\',\''+status+'\')">'+moveTopicButton+'</a>';
        }
        if (mask & 32) {
            sb += '<div class="clearFix describe-text" style="margin:5px 0px;">';
            sb += '<a class = "link" href="' + editURL + '">' + this.EditTopicButton + '</a>';
            sb += '</div>';
        }
        jq('#forum_mf').html(sb);

        var pos = jq('#forum_mf_' + idTopic).offset();
        var top = pos.top + jq('#forum_mf_' + idTopic).outerHeight() - 1;
        jq('#forum_mf').attr('style', 'display:none;');

        jq('#forum_mf').css({
            'left': pos.left + "px",
            'top': +top + 'px',
            'display': 'block',
            'position': 'absolute',
            'width': '100px',
            'padding': '10px'
        });

        jq('#forum_mf').attr('class', 'tintLight cornerAll borderBase');

        jq('body').click(function(event) {

            var elt = (event.target) ? event.target : event.srcElement;
            var isHide = true;
            if (jq(elt).is('[id^="forum_mf"]') || jq(elt).is('[id^="forum_mf"]'))
                isHide = false;

            if (isHide)
                jq(elt).parents().each(function() {
                    if (jq(this).is('[id^="forum_mf"]') || jq(this).is('[id^="forum_mf"]')) {
                        isHide = false;
                        return false;
                    }
                });

            if (isHide) {
                jq('#forum_mf').hide();
            }
        });
    };

    this.ShowModeratorFunctions = function(idTopic, mask, status, editURL) {
        if (mask == 0)
            return;

        var sb = '';
        var $combox = jq('#forumsActions');
        
        if($combox.html()!="")
            return;

        if (mask & 1)
            sb += '<li><a class = "dropdown-item" href="javascript:ForumManager.ApproveTopic(\'' + idTopic + '\')">' + this.ApproveTopicButton + '</a></li>';
        
        if (mask & 32) {
            sb += '<li><a class = "dropdown-item" href="' + editURL + '">' + this.EditTopicButton + '</a>';
        }

        if (mask & 8) {
            //1 - close / open
            if (status & 1)
                sb += '<li><a class = "dropdown-item" id="OpenCloseTopic" href="javascript:ForumManager.CloseTopicPost(\'' + idTopic + '\')">' + this.OpenTopicButton + '</a></li>';
            else
                sb += '<li><a class = "dropdown-item" id="OpenCloseTopic" href="javascript:ForumManager.CloseTopicPost(\'' + idTopic + '\')">' + this.CloseTopicButton + '</a></li>';

        }
        if (mask & 4) {
            //2 - sticky / clearsticky
            if (status & 2)
                sb += '<li><a class = "dropdown-item" id="StickyTopic" href="javascript:ForumManager.StickyTopicPost(\'' + idTopic + '\')">' + this.ClearStickyTopicButton + '</a></li>';
            else
                sb += '<li><a class = "dropdown-item" id="StickyTopic" href="javascript:ForumManager.StickyTopicPost(\'' + idTopic + '\')">' + this.StickyTopicButton + '</a></li>';

        }

        if (mask & 2) {
            //delete
            sb += '<li><a class = "dropdown-item" href="javascript:ForumManager.DeleteTopicPost(\'' + idTopic + '\')">' + this.DeleteTopicButton + '</a></li>';
        }
        $combox.html(sb);
    };

    //----------- -------------------------------------

    this.ApprovePost = function(idPost) {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_post_' + idPost).block();
            else
                jq('#forum_post_' + idPost).unblock();
        }
        PostManager.DoApprovedPost(idPost, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#forum_message_' + res.rs2).removeClass("tintDangerous");
                jq('#forum_btap_' + res.rs2).remove();
                return;
            }
            else {
                jq('#forum_post_' + res.rs2).attr('class', "errorBox").css("padding", "10px 0 10px 35px");
                jq('#forum_post_' + res.rs2).html(res.rs3);
            }
        });
    };

    this.DeletePost = function(idPost) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_post_' + idPost).block();
            else
                jq('#forum_post_' + idPost).unblock();
        }
        PostManager.DoDeletePost(idPost, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#forum_post_' + res.rs2).remove();
                toastr.success(res.rs3);
                disableLastPostDelete();
            }
            else {
                toastr.error(res.rs3);
            }
        });


    };

    this.DeleteAttachment = function(idAttachment, idPost) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_attach_' + idAttachment).block();
            else
                jq('#forum_attach_' + idAttachment).unblock();
        };
        
        PostManager.DoDeleteAttachment(idAttachment, idPost, this.SettingsID, function(result) {
            var res = result.value;
            if (res.rs1 == '1') {
                jq('#forum_attach_' + res.rs2).remove();
                toastr.success(res.rs3);
            }
            else {
                toastr.error(res.rs3);
            }
        });
    };
}

function disableLastPostDelete() {
    try {
        var postsCount = jq('div[id^="forum_post_"]').length;
        if (postsCount == 1) {
            var postID = jq('div[id^="forum_post_"]').attr('id').split('forum_post_')[1];

            jq('#PostDeleteLink' + postID).hide();
            jq('#PostDeleteSplitter' + postID).hide();
        }
    } catch (err) { }
}

jq(document).ready(function() {
    disableLastPostDelete();
});


jQuery.extend({
    /*
    var result = $.format("Hello, {0}.", "world");
    //result -> "Hello, world."
    */
    format: function jQuery_dotnet_string_format(text) {
        //check if there are two arguments in the arguments list
        if (arguments.length <= 1) {
            //if there are not 2 or more arguments there's nothing to replace
            //just return the text
            return text;
        }
        //decrement to move to the second argument in the array
        var tokenCount = arguments.length - 2;
        for (var token = 0; token <= tokenCount; ++token) {
            //iterate through the tokens and replace their placeholders from the text in order
            text = text.replace(new RegExp("\\{" + token + "\\}", "gi"), arguments[token + 1]);
        }
        return text;
    }
}
);

function getURLParam(strParamName) {

        strParamName = strParamName.toLowerCase();

        var strReturn = "";
        var strHref = window.location.href.toLowerCase();
        var bFound = false;

        var cmpstring = strParamName + "=";
        var cmplen = cmpstring.length;

        if (strHref.indexOf("?") > -1) {
            var strQueryString = strHref.substr(strHref.indexOf("?") + 1);
            var aQueryString = strQueryString.split("&");
            for (var iParam = 0; iParam < aQueryString.length; iParam++) {
                if (aQueryString[iParam].substr(0, cmplen) == cmpstring) {
                    var aParam = aQueryString[iParam].split("=");
                    strReturn = aParam[1];
                    bFound = true;
                    break;
                }

            }
        }
        if (bFound == false) return null;

        if (strReturn.indexOf("#") > -1)
            return strReturn.split("#")[0];

        return strReturn;
}

function changeCountOfRows (val) {
    var forum = getURLParam("f");
    var search = getURLParam("search");
    var href = window.location.href.split("?")[0]+"?";
    if(forum!=null)
        href += "&f=" + forum;
    if(search!=null)
        href += "&search=" + search;
    window.location.href = href + "&size=" + val;
}

function changePostCountOfRows (val) {
    var post = getURLParam("t");
    var search = getURLParam("search");
    var href = window.location.href.split("?")[0]+"?";
    if(post!=null)
        href += "&t=" + post;
    if(search!=null)
        href += "&search=" + search;        
    window.location.href = href + "&size=" + val;
}

function resizeContent() {
    var windowWidth = jq(window).width() - 24 * 2,
        newWidth = windowWidth,
        mainBlockWidth = parseInt(jq(".mainPageLayout").css("min-width"));
    if (windowWidth < mainBlockWidth) {
        newWidth = mainBlockWidth;
    }
    jq(".forum_mesBox").each(
        function() {
            jq(this).css("max-width", newWidth - jq(".mainPageTableSidePanel").width() - 24 * 2 - 180 + "px"); // 180 - width of the author block
        }
    );
};

jq(document).ready(function() {
    jq.dropdownToggle({
        dropdownID: "forumsActionsMenuPanel",
        switcherSelector: ".forumsHeaderBlock .menu-small",
        addLeft: -11,
        showFunction: function(switcherObj, dropdownItem) {
            jq('.forumsHeaderBlock .menu-small.active').removeClass('active');
            if (dropdownItem.is(":hidden")) {
                switcherObj.addClass('active');
            }
        },
        hideFunction: function() {
            jq('.forumsHeaderBlock .menu-small.active').removeClass('active');
        }
    });
    resizeContent();
    jq(window).resize(function () {
        resizeContent();
    });
    
    var textInput = jq("#forum_subject");
    if (textInput.length)
        textInput.focus();
});
