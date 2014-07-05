<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmPortalOwner.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ConfirmPortalOwner" %> 

    <div class="confirm-block-title header-base"><%=String.Format(Resources.Resource.ConfirmOwnerPortalTitle, _newOwnerName)%></div>


<asp:PlaceHolder ID="_confirmContentHolder" runat="server">
    <div class="big-button-container">
        <a class="button blue big" onclick="document.forms[0].submit(); return false;" href="javascript:void(0);"><%=Resources.Resource.SaveButton%></a>
        <span class="splitter-buttons"></span>
        <a class="button gray big" href="./"><%=Resources.Resource.CancelButton %></a>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="_messageHolder" runat="server">
    <script type="text/javascript">
        setTimeout("window.open('./','_self');",10000);
    </script>
    <div style="margin-top:50px;">
        <%=string.Format(Resources.Resource.ConfirmOwnerPortalSuccessMessage, "<br/>", "<a href=\"" + ASC.Web.Studio.Utility.CommonLinkUtility.GetDefault() + "\">", "</a>")%>
    </div>
</asp:PlaceHolder>