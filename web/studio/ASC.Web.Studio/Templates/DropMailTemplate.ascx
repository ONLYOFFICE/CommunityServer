<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Import Namespace="Resources" %>

<script id="dropMailTmpl" type="text/x-jquery-tmpl">
    <div class="item" data-id="${id}">
      
        <div class="content-box">
            <div class="description">
                <span>${from}</span>
            </div>
            <div class="header">
                <a class="title" href="${itemUrl}" title="${subject}" target="_blank">${subject}</a>
            </div>
            <div class="date">
                {{if isToday}}
                <span><%= FeedResource.TodayAt + ", " %>${displayTime}</span>
                {{else isYesterday}}
                <span><%= FeedResource.YesterdayAt + ", " %>${displayTime}</span>
                {{else}}
                <span>${displayDate}, ${displayTime}</span>
                {{/if}}
            </div>
        </div>
    </div>
</script>