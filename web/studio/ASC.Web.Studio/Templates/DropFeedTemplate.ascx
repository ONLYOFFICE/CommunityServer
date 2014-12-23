<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Import Namespace="Resources" %>

<script id="dropFeedTmpl" type="text/x-jquery-tmpl">
    <div class="item">
        <div class="avatar">
            {{if isGuest}}
            <img src="${author.avatarBig}" title="${author.displayName}"/>
            {{else}}
            <a href="${author.profileUrl}" title="${author.displayName}" target="_blank"><img src="${author.avatarBig}"/></a>
            {{/if}}
        </div>
        <div class="content-box">
            <div class="description">
                <span class="menu-item-icon ${itemClass}" ></span>
                <span class="product">${productText}.</span>
                {{if location}}
                <span class="location">${location}.</span>
                {{/if}}
                <span class="action">${actionText}</span>
                {{if groupedFeeds.length}}
                <span class="grouped-feeds-count">
                    ${ASC.Resources.Master.FeedResource.OtherFeedsCountMsg.format(groupedFeeds.length)}
                </span>
                {{/if}}
            </div>
            <div class="header">
                <a class="title" href="${itemUrl}" title="${title}" target="_blank">${title}</a>
            </div>
            <div class="date">
                {{if isToday}}
                <span><%= FeedResource.TodayAt + " " %>${displayCreatedTime}</span>
                {{else isYesterday}}
                <span><%= FeedResource.YesterdayAt + " " %>${displayCreatedTime}</span>
                {{else}}
                <span>${displayCreatedDatetime}</span>
                {{/if}}
            </div>
        </div>
    </div>
</script>