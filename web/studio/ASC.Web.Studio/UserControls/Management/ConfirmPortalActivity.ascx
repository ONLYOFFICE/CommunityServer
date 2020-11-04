<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmPortalActivity.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ConfirmPortalActivity" %>

<div class="header-base"><%=_title%></div>        

<asp:PlaceHolder ID="_confirmContentHolder" runat="server">
    <div class="big-button-container">
        <a class="button blue big"
            onclick="<%= _type != ASC.Web.Studio.Utility.ConfirmType.PortalRemove ? "document.forms[0].submit(); return false;" : "javascript:PortalRemove();"  %>"
            href="javascript:void(0);">
            <%=_buttonTitle%>
        </a>
        <span class="splitter-buttons"></span>
        <a class="button gray big" href="./" ><%=Resources.Resource.CancelButton %></a>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="_messageHolderPortalRemove" runat="server">
    <div id="successMessagePortalRemove" style="margin-top:50px;display:none;">
    </div>
    <script type="text/javascript" >
        function PortalRemove() {
            if (jq(".big-button-container .button.blue:first").hasClass("disable")) return;

            jq(".big-button-container .button.blue:first").addClass("disable");

            LoadingBanner.displayLoading();
            AjaxPro.ConfirmPortalActivity.PortalRemove("<%= Request["email"] ?? "" %>", "<%= Request["key"] ?? "" %>", function (response) {
                LoadingBanner.hideLoading();
                if (typeof (response.error) != "undefined" && response.error != null) {
                    toastr.error(response.error.Message);
                    jq(".big-button-container .button.blue:first").removeClass("disable");
                } else {
                    var resp = jq.parseJSON(response.value);

                    jq(".big-button-container").hide();
                    jq("#successMessagePortalRemove").html(resp.successMessage).show();

                    setTimeout("window.location.replace(\"" + resp.redirectLink + "\")", 10000);
                }

            });
        };
    </script>
</asp:PlaceHolder>

<asp:PlaceHolder ID="_messageHolder" runat="server">
    <div id="successMessageCnt" style="margin-top:50px;">
        <%=_successMessage%>
    </div>
    <script type="text/javascript" >
        (function () {
            var smb = document.getElementById("successMessageCnt");
            if (smb == null) return;

            var links = smb.getElementsByTagName("A");
            if (links == null || links.length == 0) return;

            setTimeout("window.location.replace(\"" + links[0].href + "\")", 10000);
        })();
    </script> 
</asp:PlaceHolder>