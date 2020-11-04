<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="TagBox.ascx.cs" Inherits="ASC.Web.Mail.Controls.TagBox" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="tags_panel" class="expandable top-margin-menu left-margin hidden">
  <div id="id_tags_panel_content"></div>
  <div class="more" style="display: none">
    <div class="shadow">
    </div>
    <a class="more_link text link dotline">
      <%=MailResource.ShowMoreTags%>
    </a>
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
                            <input id="mail_tag_name" type="text" class="textEdit" maxlength="255" placeholder="<%=MailResource.TagNamePlaceholder%>">
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
            <div class="tagEditEmailList save">
                <table id="mail_EmailsContainer" style="display: none;"></table>
            </div>
            <div class="del">
                <p><%=MailResource.DeleteTagAttention%></p>
                <p id="deleteTagShure"></p>
            </div>
            <div class="clearFix save">
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