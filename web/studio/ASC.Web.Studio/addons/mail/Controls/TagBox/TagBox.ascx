<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="TagBox.ascx.cs" Inherits="ASC.Web.Mail.Controls.TagBox" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="tags_panel" class="expandable top-margin-menu left-margin hidden">
  <div class="content">
    <div id="id_tags_panel_content">
    </div>
  </div>
  <div class="more hidden">
    <div class="shadow">
    </div>
    <div class="text link dotline">
      <%=MailResource.ShowMoreTags%>
    </div>
  </div>
</div>


<div id="tagWnd" style="display: none"   savetag="<%=MailResource.TagEdit%>"
                                         newtag="<%=MailResource.NewTag%>"
                                         deletetag="<%=MailResource.DeleteTag%>">
    <sc:Container ID="tagFieldPopup" runat="server">
        <header>
        </header>
        <body>
            <div id="mail_CreateTag_Name" class="save requiredField">
                <span class="requiredErrorText required-hint"><%= MailScriptResource.ErrorEmptyField %></span>
                <table>
                    <tr>
                        <td>
                            <div class="tag color save">
                                <div class="outer">
                                    <div class="inner tag1" colorstyle="1" />
                                </div>
                            </div>
                        </td>
                        <td>
                            <input id="mail_tag_name" type="text" class="textEdit" maxlength="20" placeholder="<%=MailResource.TagNamePlaceholder%>">
                        </td>
                    </tr>
                </table>
            </div>
            <div class="linked_addresses save">
                <div id="mail_CreateTag_Email" class="requiredField clearFix">
                   <span class="requiredErrorText required-hint"><%= MailScriptResource.ErrorEmptyField %></span>
                    <div class="headerPanelSmall" ><%: MailResource.MarkFrom %></div>
                   <input id="mail_tag_email" type="email" value="" class="textEdit addemail" placeholder="<%=MailResource.AddEmailPlaceholder%>"/>
                   <a class="plus plusmail" title="<%=MailResource.AddEmailAddressHint%>"></a>
                </div>
            </div>
            <div class="tagEditEmailList save" style="display: none;">
                <table id="mail_EmailsContainer"></table>
            </div>
            <div class="del">
                <p><%=MailResource.DeleteTagAttention%></p>
                <p id="deleteTagShure"></p>
            </div>
            <div class="clearFix save">
                <div class="addemail_error"></div>
                <div class="progressContainer">
                    <div class="loader" style="display: none;"></div>
                </div>
            </div>
            <div class="buttons new-tag">
                <button class="button middle blue save" type="button"><%=MailResource.SaveBtnLabel%></button>
                <button class="button middle blue del" type="button"><%=MailResource.DeleteBtnLabel%></button>
                <button class="button middle gray cancel" type="button"><%=MailScriptResource.CancelBtnLabel%></button>
            </div>
        </body>
    </sc:Container>
</div>

<div id="addTagsPanel" class="actionPanel stick-over">
    <div id="tagsPanelContent" style="display: block;">
        <div class="actionPanelSection">
            <label for="markallrecipients" class="mark_all_checkbox">
                <input type="checkbox" id="markallrecipients"/>
                <span  id="markallrecipientsLabel"><%=MailScriptResource.MarkAllSendersLabel%></span>
            </label>
        </div>
        <div class="existsTags">
        </div>
        <div class="h_line"></div>
        <div class="actionPanelSection" style="width: 293px;">
            <div id="mail_CreateNewTag" class="requiredField">
               <span class="requiredErrorText" style="padding-right: 49px;"><%: MailScriptResource.ErrorEmptyField %></span>
                <div class="headerPanelSmall" ><%: MailResource.CreateNewTagLabel %></div>
                <div class="entertagname">
                    <div title="Select color" class="square"></div>
                    <input id="createnewtag" type="text" class="textEdit tagname" placeholder="<%=MailResource.TagNamePlaceholder%>" maxlength="20"/>
                </div>
                <button class="entertag_button button blue"><%=MailScriptResource.OkBtnLabel%></button>
            </div>
        </div>
    </div>
</div>