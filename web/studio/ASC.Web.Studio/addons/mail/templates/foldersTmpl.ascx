<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="foldersTmpl" type="text/x-jquery-tmpl">
    <li class="menu-item none-sub-list{{if $item.marked==id}} active{{/if}}" folderid="${id}" unread="${unread}">
        <span class="menu-item-icon {{if id==1}}inbox{{else id==2}}sent{{else id==3}}drafts{{else id==4}}trash{{else id==5}}spam{{/if}}"></span>
        <a class="menu-item-label outer-text text-overflow" href="#" folderid="${id}">
            <span class="menu-item-label inner-text">
              {{if id==1}}<%: MailResource.FolderNameInbox %>{{else id==2}}<%: MailResource.FolderNameSent %>{{else id==3}}<%: MailResource.FolderNameDrafts %>{{else id==4}}<%: MailResource.FolderNameTrash %>{{else id==5}}<%: MailResource.FolderNameSpam %>{{/if}}
            </span>
        </a>
        <div class="lattersCount counter">
          {{if id==4 || id==5}}
            {{if id==4 ||unread==0 }}
                <div style="padding-top: 3px;"></div>
            {{/if}}
            {{if total_count!=0}}
                <span class="delete_mails" folderid="${id}" title="<%: MailResource.ClearBtnLabel %>">
                </span>
            {{/if}}
          {{/if}}
          {{if unread!=0 }}
            {{if id==1 || id==5}}
                <span class="unread">${unread}</span>
            {{/if}}
          {{/if}}
          {{if id==3 && total_count!=0}}
               <span class="unread">${total_count}</span>
          {{/if}}
        </div>
    </li>
</script>