<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmPortalActivity.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ConfirmPortalActivity" %>

<div class="header-base"><%=_title%></div>        


<asp:PlaceHolder ID="_confirmContentHolder" runat="server">
    <div class="big-button-container">
        <a class="button blue big" onclick="document.forms[0].submit(); return false;" href="javascript:void(0);"><%=_buttonTitle%></a>
        <span class="splitter-buttons"></span>
        <a class="button gray big" href="./" ><%=Resources.Resource.CancelButton %></a>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="_messageHolder" runat="server">
    <script type="text/javascript" >
        var link = jq("#successMessageCnt").find("a").attr("href");
        setTimeout("window.open(link)", 10000);
    </script>        
    <div id="successMessageCnt" style="margin-top:50px;">
        <%=_successMessage%>
    </div>
</asp:PlaceHolder>