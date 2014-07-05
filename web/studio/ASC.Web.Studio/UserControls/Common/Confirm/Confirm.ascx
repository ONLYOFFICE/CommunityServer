<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Confirm.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Common.Confirm" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_confirmDialog<%=AdditionalID%>" class="display-none">
    <sc:Container runat="server" ID="_studioConfirm">
        <Header>
            <%= Title.HtmlEncode()%>
        </Header>
        <Body>
            <asp:HiddenField runat="server" ID="_confirmEnterCode" />
            <div id="studio_confirmMessage"></div>
            <div class="clearFix">
                <div>
                    <%=SelectTitle%>:
                </div>
                <div style="margin: 3px 0;">
                    <input id="studio_confirmInput<%=AdditionalID%>" class="textEdit" type="text" value="<%=Value %>"  style="width: 350px;" maxlength="100" />
                </div>
            </div>
            <div class="clearFix" style="margin-top: 16px;">
                    <a class="button blue float-left" href="javascript:void(0);" onclick="StudioConfirm.Select('<%=AdditionalID%>',<%=SelectJSCallback%>);"><%=Resources.Resource.SaveButton%></a> 
                    <a class="button gray" href="javascript:void(0);" onclick="StudioConfirm.Cancel();" style="float: left;margin-left: 8px;"><%=Resources.Resource.CancelButton%></a>
            </div>
        </Body>
    </sc:Container>
</div>
