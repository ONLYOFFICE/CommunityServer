<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Import Namespace="Resources" %>

<script id="feed-inner-comment-tmpl" type="text/x-jquery-tmpl">
    <div class="comment">
        <a href="${authorUrl}" target="_blank"><img class="comment-avatar" src="${authorAvatar}"/></a>
        <div class="comment-content-box">
            <div class="comment-author">
                <a href="${authorUrl}" class="title" target="_blank">${author.DisplayName}</a>,
                {{if author.UserInfo.Title}}
                <span>${author.UserInfo.Title},</span>
                {{/if}}
                <span>${formattedDate}</span>
            </div>
            <div class="comment-body">{{html description}}</div>
            <div class="reply-comment-btn" data-commentid="${id}"><%= FeedResource.ReplyCommentBtn %></div>
        </div>
    </div>
</script>

<script id="feedTmpl" type="text/x-jquery-tmpl">
    <div id="feed_${id}" class="item clearFix">
        <div class="avatar">
            {{if byGuest}}
            <img src="${authorAvatar}" />
            {{else}}
            <a href="${authorUrl}" target="_blank"><img src="${authorAvatar}"/></a>
            {{/if}}
        </div>
        <div class="content-box">
            <div class="feed-item">
                <div class="header">
                    <span class="action">${actionText}.</span>
                    <a href="${itemUrl}" class="title" target="_blank"
                       data-extra="${extra}" data-hintName="${hintName}"
                       data-extra2="${extra2}" data-hintName2="${hintName2}"
                       data-extra3="${extra3}" data-hintName3="${hintName3}"
                       data-extra4="${extra4}" data-hintName4="${hintName4}">${title}
                    </a>
                    {{if groupedFeeds.length}}
                    <span class="grouped-feeds-count">
                        ${ASC.Resources.Master.FeedResource.OtherFeedsCountMsg.format(groupedFeeds.length)}
                    </span>
                    {{/if}}
                    {{if isNew}}
                    <span class="new-indicator" title="<%= FeedResource.NewFeedIndicator %>"><%= FeedResource.NewFeedIndicator %></span>
                    {{/if}}
                </div>
                <div class="description">
                    <span class="menu-item-icon ${itemClass}" />
                    <span class="product">${productText}.</span>
                    {{if location}}
                    <span class="location">${location}.</span>
                    {{/if}}
                    {{if extraLocation}}
                    <span class="extra-location">
                        <a href="${extraLocationUrl}" class="title" title="${extraLocation}" target="_blank">
                            ${extraLocation}</a>.
                    </span>
                    {{/if}}
                </div>
                <div class="date">
                    {{if isToday}}
                    <span><%= FeedResource.TodayAt + " " %>${displayCreatedTime}</span>
                    {{else isYesterday}}
                    <span><%= FeedResource.YesterdayAt + " " %>${displayCreatedTime}</span>
                    {{else}}
                    <span>${displayCreatedDate}</span>
                    <span class="time">${displayCreatedTime}</span>
                    {{/if}}
                </div>
                {{if author}}
                <div class="author">
                    <span class="label"><%= FeedResource.Author %>:</span>
                    {{if byGuest}}
                    <span class="guest">${author.DisplayName}</span>
                    {{else}}
                    <a href="${authorUrl}" class="title" target="_blank">${author.DisplayName}</a>{{if authorTitle}},
                    <span class="author-title">
                        ${authorTitle}</span>
                    {{/if}}
                    {{/if}}
                </div>
                {{/if}}
                <div class="body">
                    {{html description}}
                    {{if hasPreview}}
                    <div class="show-all-btn control-btn"><%= FeedResource.ShowAll %></div>
                    {{/if}}
                </div>
                {{if groupedFeeds.length}}
                <div class="control-btn show-grouped-feeds-btn">
                    <%= FeedResource.ShowGroupedFeedsBtn %>
                </div>
                <div class="control-btn hide-grouped-feeds-btn">
                    <%= FeedResource.HideGroupedFeedsBtn %>
                </div>
                <div class="grouped-feeds-box">
                    {{each groupedFeeds}}
                    <div>
                        <a href="${ItemUrl}" class="title" target="_blank"
                           data-extra="${AdditionalInfo}" data-hintName="${hintName}"
                           data-extra2="${AdditionalInfo2}" data-hintName2="${hintName2}"
                           data-extra3="${AdditionalInfo3}" data-hintName3="${hintName3}"
                           data-extra4="${AdditionalInfo4}" data-hintName4="${hintName4}">
                            ${Title}
                        </a>
                    </div>
                    {{/each}}
                </div>
                {{/if}}
                {{if canComment}}
                <div id="write-comment-btn-${id}" class="control-btn write-comment-btn">
                    <%= FeedResource.WriteComment %>
                </div>
                {{if comments && comments.length}}
                <div style="margin-top: 5px;"></div>
                {{/if}}
            </div>
            <div class="comments-box">
                {{if comments && comments.length > 3}}
                <div class="comments-show-panel">
                    <span class="control-btn" data-show-text="<%= FeedResource.ShowOthersCommentsBtn %>" data-hide-text="<%= FeedResource.HideAdditionalCommentsBtn %>" data-state="0">
                        <%= FeedResource.ShowOthersCommentsBtn %>
                    </span>
                </div>
                {{/if}}
                <div id="comment-form-${id}" class="comment-form">
                    <textarea></textarea>
                    <div class="comment-error-msg-box">
                        <span class="red-text"><%= FeedResource.CommentErrorMsg %></span>
                    </div>
                    <div class="comment-empty-error-msg-box">
                        <span class="red-text"><%= FeedResource.CommentEmptyErrorMsg %></span>
                    </div>
                    <a id="publish-comment-btn-${id}" class="publish-comment-btn button" href="#" 
                       data-id="${id}" 
                       data-entity="${item}" 
                       data-entityid="${itemId}" 
                       data-commentapiurl="${commentApiUrl}"><%= FeedResource.PublishCommentBtn %></a>
                    <a id="cancel-comment-btn-${id}" class="cancel-comment-btn button gray" href="#" data-id="${id}">
                        <%= FeedResource.CancelCommentBtn %>
                    </a>
                </div>
                <div class="main-comments-box">
                    {{each comments.slice(0, 3)}}
                    {{tmpl($value) 'feed-inner-comment-tmpl'}}
                    {{/each}}
                </div>
                <div class="extra-comments-box">
                    {{each comments.slice(3)}}
                    {{tmpl($value) 'feed-inner-comment-tmpl'}}
                    {{/each}}
                </div>
            </div>
            {{/if}}
        </div>
    </div>
</script>

<script id="feedCommentTmpl" type="text/x-jquery-tmpl">
    <div class="comment">
        <a href="${authorUrl}" target="_blank"><img class="comment-avatar" src="${authorAvatar}"/></a>
        <div class="comment-content-box">
            <div class="comment-author">
                <a href="${authorUrl}" class="title" target="_blank">${authorName}</a>,
                {{if authorTitle}}
                <span>${authorTitle},</span>
                {{/if}}
                <span>${date}</span>
            </div>
            <div class="comment-body">{{html description}}</div>
            <div class="reply-comment-btn" data-commentid="${id}"><%= FeedResource.ReplyCommentBtn %></div>
        </div>
    </div>
</script>