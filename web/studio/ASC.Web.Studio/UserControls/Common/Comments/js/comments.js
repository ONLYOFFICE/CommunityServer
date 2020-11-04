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


var CommentsManagerObj = new function() {
    this.obj = null;
    this.editorInstance = null;

    this.javaScriptCallBackAddComment = "";

    this.OnEditedCommentJS = "";
    this.OnRemovedCommentJS = "";
    this.OnCanceledCommentJS = "";


    this.FckDomainName = "";
    this.currentCommentID = "";

    this.CkUploadHandlerPath = "";

    this.mainLoader = "";

    this.maxLevel = 8;

    this._jsObjName = "";

    this.onLoadComplete = null;

    function hideAddButton() {
        jq('#add_comment_btn').addClass("display-none");
    };

    function showAddButton() {
        setTimeout(function () { jq('#add_comment_btn').removeClass("display-none"); }, 500);
    };

    function previewClick() {
        if (jq("#btnPreview").hasClass("disable")) return;

        var commentId = jq('#hdnCommentID').val(),
            commentContent = CommentsManagerObj.editorInstance.getData(),
            ajax_opts = {
                before: function () {
                    CommentsManagerObj.BlockCommentsBox();
                },
                after: function () {
                    CommentsManagerObj.UnblockCommentsBox();
                },
                error: function (params, errors) {
                    toastr.error(errors[0]);
                },
                success: callBackPreview
            };


        switch (CommentsManagerObj.moduleName) {
            case "projects_Message":
            case "projects_Task":
                Teamlab.getPrjCommentPreview({}, commentId, commentContent, ajax_opts);
                break;
            case "wiki":
                Teamlab.getWikiCommentPreview({}, commentId, commentContent, ajax_opts);
                break;
            case "blogs":
                Teamlab.getBlogCommentPreview({}, commentId, commentContent, ajax_opts);
                break;
            case "news":
                Teamlab.getNewsCommentPreview({}, commentId, commentContent, ajax_opts);
                break;
            case "bookmarks":
                Teamlab.getBookmarksCommentPreview({}, commentId, commentContent, ajax_opts);
                break;
        }
    };

    function cancelClick() {
        if (CommentsManagerObj.editorInstance.mode == "source") {
            CommentsManagerObj.editorInstance.commands.source.exec();
        }

        CommentsManagerObj.CallActionHandlerJS('cancel', 'CommentsManagerObj.UnblockCommentsBox');

        jq('#commentBox').hide();
        showAddButton();
    };

    function addCommentClick() {

        var text = CommentsManagerObj.editorInstance.getData();
        if (text.trim().length == 0) {
            toastr.error(ASC.Resources.Master.Resource.EmptyCommentErrorMessage);
            return false;
        }

        if (text != "") {

            var ajax_opts =
                {
                    before: function () {
                        CommentsManagerObj.BlockCommentsBox();
                    },
                    after: function () {
                        CommentsManagerObj.UnblockCommentsBox();
                    },
                    error: function (params, errors) {
                        toastr.error(errors[0]);
                    },
                    success: (jq("#hdnAction").val() == "add" ? callBackAddComment : callBackUpdateComment)
                };


            if (jq("#hdnAction").val() == "add") {
                var data = {
                    parentcommentid: jq('#hdnParentComment').val(),
                    entityid: jq('#hdnObjectID').val(),
                    content: CommentsManagerObj.editorInstance.getData()
                };

                switch (CommentsManagerObj.moduleName) {
                    case "projects_Message":
                    case "projects_Task":
                        data = jq.extend(data, { type: CommentsManagerObj.moduleName .split('_')[1]});
                        Teamlab.addPrjComment(data, data, ajax_opts);
                        break;
                    case "wiki":
                        data.entityid = jq("#hdnPageName").val();
                        Teamlab.addWikiComment(data, data, ajax_opts);
                        break;
                    case "blogs":
                        Teamlab.addBlogComment(data, data, ajax_opts);
                        break;
                    case "news":
                        Teamlab.addNewsComment(data, data, ajax_opts);
                        break;
                    case "bookmarks":
                        Teamlab.addBookmarksComment(data, data, ajax_opts);
                        break;
                }
            } else if (jq('#hdnAction').val() == "update") {

                var data = {
                    commentid: jq('#hdnCommentID').val(),
                    content: CommentsManagerObj.editorInstance.getData()
                };

                switch (CommentsManagerObj.moduleName) {
                    case "projects_Message":
                    case "projects_Task":
                        Teamlab.updatePrjComment(data, data.commentid, data, ajax_opts);
                        break;
                    case "wiki":
                        Teamlab.updateWikiComment(data, data.commentid, data, ajax_opts);
                        break;
                    case "blogs":
                        Teamlab.updateBlogComment(data, data.commentid, data, ajax_opts);
                        break;
                    case "news":
                        Teamlab.updateNewsComment(data, data.commentid, data, ajax_opts);
                        break;
                    case "bookmarks":
                        Teamlab.updateBookmarksComment(data, data.commentid, data, ajax_opts);
                        break;
                }
            }
        }
        jq('#noComments').hide();
    };

    function removeCommentClick(elt) {
        var id = jq(elt).attr("id").replace("remove_", ""),
                ajax_opts = {
                    before: function () {
                        CommentsManagerObj.BlockCommentsBox();
                    },
                    after: function () {
                        CommentsManagerObj.UnblockCommentsBox();
                    },
                    error: function (params, errors) {
                        toastr.error(errors[0]);
                    },
                    success: callbackRemove
                },
                fn = null;
        switch (CommentsManagerObj.moduleName) {
            case "projects_Message":
            case "projects_Task":
                fn = function () { Teamlab.removePrjComment({ commentid: id }, id, ajax_opts); };
                break;
            case "wiki":
                fn = function () { Teamlab.removeWikiComment({ commentid: id }, id, ajax_opts); };
                break;
            case "blogs":
                fn = function () { Teamlab.removeBlogComment({ commentid: id }, id, ajax_opts); };
                break;
            case "news":
                fn = function () { Teamlab.removeNewsComment({ commentid: id }, id, ajax_opts); };
                break;
            case "bookmarks":
                fn = function () { Teamlab.removeBookmarksComment({ commentid: id }, id, ajax_opts); };
                break;
        }

        StudioConfirm.OpenDialog('', fn);
    };

    function changeUploadHandlerPath(commentId) {
        var uploadUrl = CommentsManagerObj.CkUploadHandlerPath;

        if (commentId) {
            var url = new URL(CommentsManagerObj.CkUploadHandlerPath);
            var searchParams = url.searchParams;
            searchParams.set("iid", commentId);
            url.search = searchParams.toString();
            uploadUrl = url.toString();
        }

        if (CommentsManagerObj.editorInstance &&
            CommentsManagerObj.editorInstance.status == "ready" &&
            uploadUrl != CommentsManagerObj.editorInstance.config.filebrowserUploadUrl) {
            CommentsManagerObj.editorInstance.destroy();
            buildEditorInstance(uploadUrl);
        }
    }

    function editCommentClick(elt) {
        var id = jq(elt).attr("id").replace("edit_", "");

        CommentsManagerObj.obj = id;
        changeUploadHandlerPath(id);
        setParentComment("");
        setAction("update", id);
        showCommentBox(id);
        CommentsManagerObj.currentCommentID = id;
        return false;
    };

    function responseCommentClick(elt) {
        var id = jq(elt).attr("id").replace("response_", "");

        CommentsManagerObj.obj = id;
        changeUploadHandlerPath(null);
        setParentComment(id);
        setAction("add", null);
        showCommentBox("", elt);

        return false;
    };

    function copyToClipboard(elt) {
        var id = jq(elt).attr("id").replace("clip_", "");
        var link = window.location.href.replace(window.location.hash, '') + '#comment_' + id
        jq('#commentsClip').val(link).select();
        document.execCommand('copy');
        window.toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
    };

    function redraw() {
        oddcnt = jq('#mainCommentsContainer div[id^=comment_]:even')
                    .css({ 'border-top': '1px solid #DDD', 'border-bottom': '1px solid #DDD' })
                    .length;
        evencnt = jq('#mainCommentsContainer div[id^=comment_]:odd')
                    .css({ 'border-top': '', 'border-bottom': '' })
                    .length;

        if (oddcnt == evencnt) {
            jq('#mainCommentsContainer').css('border-bottom', '1px solid #DDD');
        } else {
            jq('#mainCommentsContainer').css('border-bottom', '');
        }
    };

    function scrollToElement(target) {
        var parent = jq(jq("body").css("overflow") == "hidden" ? '.mainPageContent' : 'body');
        parent.scrollTo(target, 500);
    }

    function hidePreview() {
        scrollToElement('#commentBox', 500);
        jq('#previewBox').hide("slow");
    };

    function addNewComment() {
        if (CommentsManagerObj.editorInstance) {
            CommentsManagerObj.obj = null;
            changeUploadHandlerPath(null);
            setParentComment("");
            setAction("add", null);
            showCommentBox("", null);

            CommentsManagerObj.currentCommentID = "";
            jq('#hdnCommentID').val('');
        } else {
            setTimeout(function() { addNewComment(); }, 100);
        }
    };

    function callBackPreview(params, comment) {
        var html = jq.tmpl('template-comment',
        {
            comment: comment,
            index: 0,
            commentLevel: 1,
            maxDepthLevel: CommentsManagerObj.maxDepthLevel
        });

        jq('#previewBoxBody').html(html);
        scrollToElement(jq('#previewBox').show(), 500);
    };

    function callBackAddComment(params, response) {
        if (response == null) return;

        var comment = response;

        if (params.parentcommentid === "") {

            var html = jq.tmpl('template-comment',
            {
                comment: comment,
                index: 0,
                commentLevel: 1,
                maxDepthLevel: CommentsManagerObj.maxDepthLevel
            });

            jq('#mainCommentsContainer').append(html);

        } else {
            var level = 1,
                obj = jq('#container_' + params.parentcommentid);

            while (obj.attr("id") != "mainCommentsContainer") {
                level = level + 1;
                obj = obj.parent();
            }

            var html = jq.tmpl("template-comment",
            {
                comment: comment,
                index: 0,
                commentLevel: level,
                maxDepthLevel: CommentsManagerObj.maxDepthLevel
            });

            jq("#container_" + params.parentcommentid).append(html);
        }

        if (CommentsManagerObj.javaScriptCallBackAddComment != "")
            eval(CommentsManagerObj.javaScriptCallBackAddComment);

        jq('#commentBox').hide();

        redraw();

        jq("#mainCommentsContainer").show();
        jq("#commentsTitle").show();
        showAddButton();

        var obj;
        if (params.parentcommentid == "") {
            obj = jq('#mainCommentsContainer > div:last-child').children();
        } else {
            obj = jq('#container_' + params.parentcommentid + ' > div:last-child').children();
        }

        CommentsManagerObj.currentCommentID = obj.attr("id").replace("comment_", "");


        scrollToElement(obj, 500);

        obj.css({ "background-color": "#ffffcc" });
        obj.animate({ backgroundColor: '#ffffff' }, 1000);

        CommentsManagerObj.CallFCKComplete();
    };

    function callBackUpdateComment(params, response) {
        if (response == null) return;

        jq('#content_' + params.commentid).html(response);

        jq('#commentBox').hide("slow");
        showAddButton();

        var obj = jq('#comment_' + params.commentid);

        CommentsManagerObj.currentCommentID = params.commentid;

        scrollToElement(obj, 500);

        obj.css({ "background-color": "#ffffcc" });
        obj.animate({ backgroundColor: '#ffffff' }, 1000);

        CommentsManagerObj.CallFCKComplete();
    };

    function callbackRemove(params, response) {
        var html = jq.tmpl("template-comment-inactive",{});
        jq('#comment_' + params.commentid).html(html);
        CommentsManagerObj.currentCommentID = params.commentid;
        CommentsManagerObj.CallActionHandlerJS('remove', 'CommentsManagerObj.UnblockCommentsBox');

        redraw();
        cancelClick();
    };

    function showCommentBox(id) {
        hideAddButton();

        var ContentDiv = document.getElementById('content_' + id),
            timeOut = 500;

        if (CommentsManagerObj.editorInstance.mode == "source") {
            CommentsManagerObj.editorInstance.commands.source.exec();
        }

        if (ContentDiv != null) {
            CommentsManagerObj.editorInstance.setData(ContentDiv.innerHTML);
        } else {
            CommentsManagerObj.editorInstance.setData('');
            timeOut = 600;
        }

        jq('#commentBox').show();

        scrollToElement(jq('#commentBox .middle-button-container'), 500);
        jq('#previewBox').hide();

        setTimeout(function () {
            CommentsManagerObj.editorInstance.focus();
        }, timeOut);
    };

    function setParentComment(value) {
        jq('#hdnParentComment').val(value);
    };

    function setAction(action, comment_id) {
        jq('#hdnAction').val(action);

        if (comment_id != null)
            jq('#hdnCommentID').val(comment_id);
    };

    function buildEditorInstance(uploadUrl) {
        CommentsManagerObj.editorInstance =
            jq("#commentEditor" + CommentsManagerObj._jsObjName)
                .ckeditor(
                    {
                        toolbar: "Comment",
                        extraPlugins: "teamlabquote,codemirror",
                        filebrowserUploadUrl: uploadUrl || CommentsManagerObj.CkUploadHandlerPath,
                        height: "200"
                    })
                .editor;

        CommentsManagerObj.editorInstance.keystrokeHandler.keystrokes[window.CKEDITOR.CTRL + 13] = "ctrlEnter";
        CommentsManagerObj.editorInstance.addCommand("ctrlEnter", {
            exec: function (editor, data) {
                addCommentClick();
            }
        });

        CommentsManagerObj.editorInstance.on("change", function () {
            if (this.getData() == "") {
                jq("#btnPreview").addClass("disable");
            } else {
                jq("#btnPreview").removeClass("disable");
            }
        });
    };

    this.Init = function () {
        try {
            CommentsManagerObj.comments = jq.parseJSON(jq.base64.decode(CommentsManagerObj.comments));
        } catch (e) {
            console.log(e);
            CommentsManagerObj.comments = [];
        }

        jq("#commentsTempContainer_" + CommentsManagerObj.objectID).html(jq.tmpl("template-commentsList", CommentsManagerObj));

        jq("code").each(function () { hljs.highlightBlock(jq(this).get(0)); });

        redraw();

        jq("#btnPreview").on("click", function () { previewClick(); return false; });
        jq("#btnCancel").on("click", function () { cancelClick(); });
        jq("#btnAddComment").on("click", function () { addCommentClick(); return false; });

        jq("#add_comment_btn").on("click", function () { addNewComment(); });

        jq("#previewBox .middle-button-container > .button.blue").on("click", function () { hidePreview(); return false; });

        jq("#mainCommentsContainer").on("click", "[id^=remove_]", function () { removeCommentClick(this); });

        jq("#mainCommentsContainer").on("click", "[id^=edit_]", function () { editCommentClick(this); });

        jq("#mainCommentsContainer").on("click", "[id^=response_]", function () { responseCommentClick(this); });

        jq("#mainCommentsContainer").on("click", "[id^=clip_]", function () { copyToClipboard(this); });

        ckeditorConnector.load(buildEditorInstance);

        if (CommentsManagerObj.onLoadComplete && typeof CommentsManagerObj.onLoadComplete == "function") {
            CommentsManagerObj.onLoadComplete();
        }

        var hash = window.location.hash;
        if (hash) {
            setTimeout(function () { window.scrollTo(0, 0); }, 0);
            if (hash.indexOf("#comment_") == 0) {
                setTimeout(function () {
                    scrollToElement(hash, 500);
                    jq(hash).css({ "background-color": "#ffffcc" }).animate({ backgroundColor: '#ffffff' }, 1000);
                }, 1000);
            }
        }
    };

    this.BlockCommentsBox = function() {
        LoadingBanner.showLoaderBtn("#commentBoxContainer");
        CommentsManagerObj.mainLoader = jq.blockUI.defaults.message;
        jq.blockUI.defaults.message = '';
        jq.blockUI();
    };

    this.UnblockCommentsBox = function () {
        LoadingBanner.hideLoaderBtn("#commentBoxContainer");
        jq.blockUI.defaults.message = CommentsManagerObj.mainLoader;
        jq.unblockUI();
    };

    this.CallActionHandlerJS = function(action, callBack) {
        switch (action) {
            case "add":
                if (CommentsManagerObj.OnEditedCommentJS != "")
                    eval(CommentsManagerObj.OnEditedCommentJS)(CommentsManagerObj.currentCommentID, CommentsManagerObj.editorInstance.getData() , CommentsManagerObj.FckDomainName, false, callBack);
                return;

            case "edit":
                if (CommentsManagerObj.OnEditedCommentJS != "")
                    eval(CommentsManagerObj.OnEditedCommentJS)(CommentsManagerObj.currentCommentID, CommentsManagerObj.editorInstance.getData(), CommentsManagerObj.FckDomainName, true, callBack);
                return;

            case "remove":
                if (CommentsManagerObj.OnRemovedCommentJS != "")
                    eval(CommentsManagerObj.OnRemovedCommentJS)(CommentsManagerObj.currentCommentID, CommentsManagerObj.FckDomainName, callBack);
                return;

            case "cancel":
                if (CommentsManagerObj.OnCanceledCommentJS != "")
                    eval(CommentsManagerObj.OnCanceledCommentJS)(CommentsManagerObj.currentCommentID, CommentsManagerObj.FckDomainName, (CommentsManagerObj.currentCommentID != ''), callBack);
                return;
        }

        eval(callBack)();
    };

    this.CallFCKComplete = function() {
        if (jq('#hdnAction').val() == "update") {
            CommentsManagerObj.CallActionHandlerJS('edit', 'CommentsManagerObj.UnblockCommentsBox');
        } else {
            CommentsManagerObj.CallActionHandlerJS('add', 'CommentsManagerObj.UnblockCommentsBox');
        }
    };

    this.AddNewComment = addNewComment;
};