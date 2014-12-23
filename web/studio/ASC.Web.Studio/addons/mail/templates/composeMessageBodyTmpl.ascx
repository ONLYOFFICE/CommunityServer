<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="replyMessageHtmlBodyTmpl" type="text/x-jquery-tmpl">
  <div class="reply-text" style="padding: 10px;">
    <p>${message.date}, ${message.from}:</p>
    {{if !visibleQoute}}
        {{tmpl({}, {}) "blockquoteTmpl"}}
        <blockquote style="margin-left:20px; padding-left:20px; border-left:1px solid #D1D1D1; display:none;">
            {{html message.htmlBody}}
        </blockquote>
    {{else}}
         <blockquote style="margin-left:20px; padding-left:20px; border-left:1px solid #D1D1D1;">
             {{html message.htmlBody}}
         </blockquote>
    {{/if}}
        
  </div>
</script>

<script id="forwardMessageHtmlBodyTmpl" type="text/x-jquery-tmpl">
  <div class="forward-text"  style="padding: 10px;">
    <p>-------- <%: MailScriptResource.ForwardTitle %> --------<p/>
    <p><%: MailScriptResource.FromLabel %>: ${from}</p>
    <p><%: MailScriptResource.ToLabel %>: ${to}</p>
    <p><%: MailScriptResource.DateLabel %>: ${date}</p>
    <p><%: MailScriptResource.SubjectLabel %>: ${subject}</p>
    <br/>
    <div>{{html htmlBody}}</div>
  </div>
</script>

<script id="blockquoteTmpl" type="text/x-jquery-tmpl">
    <input class="tl-controll-blockquote" type="button" value="..."/>
</script>