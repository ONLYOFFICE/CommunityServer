<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ASC.Web.Mobile.Models.HomeModel>" %>

<asp:Content ID="Script" ContentPlaceHolderID="Script" runat="server">
<script type="text/javascript">
  (function () {
    window.PHONEGAP = false;
    var params = {}, item = null, search = '', searchInd = 0;
    search = (location.search.charAt(0) === '?' ? location.search.substring(1) : location.search).split('&');
    searchInd = search.length;
    while (searchInd--) {
      item = search[searchInd].split('=');
      if (item.length === 2) {
        params[item[0]] = decodeURIComponent(item[1]);
      }
    }
    window.PHONEGAP = params.hasOwnProperty('app');

    try {TeamlabMobile.update(params)} catch (err) {}
  })();
</script>

<script type="text/javascript" async="true">
  (function () {
    ServiceManager.init('<%=Model.ApiBaseUrl%>', true);
    ServiceFactory.init({
      names : {
        months : '<%=Model.MonthNames%>',
        shortmonths : '<%=Model.ShortMonthNames%>',
        days : '<%=Model.DayNames%>',
        shortdays : '<%=Model.ShortDayNames%>'
      },
      formats : {
        datetime : '<%=Model.DateTimePattern%>',
        time : '<%=Model.TimePattern%>',
        date : '<%=Model.DatePattern%>'
      },
      avatars : {
        small : '<%=Url.Content("~/content/images/default-user-photo-size-32x32.gif")%>',
        medium : '<%=Url.Content("~/content/images/default-user-photo-size-48x48.gif")%>',
        large : '<%=Url.Content("~/content/images/default-user-photo-size-82x82.gif")%>'
      },
      supportedfiles : {
        imgs : '<%=Model.PreviewedImageExtensions%>',
        docs : '<%=Model.PreviewedDocExtensions%>'
      },
      contacttitles : {
        phone     : '<%=Resources.MobileResource.LblPhone%>',
        mobphone  : '<%=Resources.MobileResource.LblMobilePhone%>',
        mail      : '<%=Resources.MobileResource.LblEmail%>'
      }
    });
    Teamlab.init();

    if (!ASC) ASC = {};
    ASC.DefaultParams = {
      srchhelper  : [],
      teamlab : [
        {
          community : ['<%=Url.Content("~/content/images/icon-community.svg")%>', '<%=Url.Content("~/content/images/icon-community-disabled.svg")%>'],
          people    : ['<%=Url.Content("~/content/images/icon-people.svg")%>', '<%=Url.Content("~/content/images/icon-people-disabled.svg")%>'],
          projects  : ['<%=Url.Content("~/content/images/icon-projects.svg")%>', '<%=Url.Content("~/content/images/icon-projects-disabled.svg")%>'],
          documents : ['<%=Url.Content("~/content/images/icon-documents.svg")%>', '<%=Url.Content("~/content/images/icon-documents-disabled.svg")%>'],
          crm       : ['<%=Url.Content("~/content/images/icon-crm.svg")%>', '<%=Url.Content("~/content/images/icon-crm-disabled.svg")%>']
        },
        {
          fileview : '<%=Url.Content("~/fileview/")%>'
        }
      ]
    };
  })();
</script>
</asp:Content>

<asp:Content ID="JsTemplate" ContentPlaceHolderID="JsTemplate" runat="server">
  <%Html.RenderPartial("SignInPageTmpl");%>
  <%Html.RenderPartial("DefaultPageTmpl");%>
  <%Html.RenderPartial("RewritePageTmpl");%>
  <%Html.RenderPartial("SearchPageTmpl");%>

  <%Html.RenderPartial("GnrlCommunityThreadsTmpl");%>
  <%Html.RenderPartial("GnrlProjectProjectsSelectTmpl");%>
  <%Html.RenderPartial("GnrlProjectMilestonesSelectTmpl");%>
  <%Html.RenderPartial("GnrlProjectTeamSelectTmpl");%>
  <%Html.RenderPartial("GnrlProjsearchItemsTmpl");%>

  <%Html.RenderPartial("GnrlFilesTmpl");%>
  <%Html.RenderPartial("GnrlCommentsTmpl");%>
  <%Html.RenderPartial("GnrlAddCommentTmpl");%>
  <%Html.RenderPartial("GnrlExceptionPageTmpl");%>
  <%Html.RenderPartial("GnrlAddCommentPageTmpl");%>

  <%Html.RenderPartial("CommunityPageTmpl");%>
  <%Html.RenderPartial("CommBlogPageTmpl");%>
  <%Html.RenderPartial("CommForumPageTmpl");%>
  <%Html.RenderPartial("CommEventPageTmpl");%>
  <%Html.RenderPartial("CommBookmarkPageTmpl");%>
  <%Html.RenderPartial("CommAddBlogPageTmpl");%>
  <%Html.RenderPartial("CommAddForumPageTmpl");%>
  <%Html.RenderPartial("CommAddEventPageTmpl");%>
  <%Html.RenderPartial("CommAddBookmarkPageTmpl");%>
  <%Html.RenderPartial("CommTimeLineTmpl");%>

  <%Html.RenderPartial("ProjectsPageTmpl");%>
  <%Html.RenderPartial("ProjItemsPageTmpl");%>
  <%Html.RenderPartial("ProjProjectPageTmpl");%>
  <%Html.RenderPartial("ProjProjectTeamPageTmpl");%>
  <%Html.RenderPartial("ProjProjectFilesPageTmpl");%>
  <%Html.RenderPartial("ProjProjectTasksPageTmpl");%>
  <%Html.RenderPartial("ProjProjectMilestonesPageTmpl");%>
  <%Html.RenderPartial("ProjProjectDiscussionsPageTmpl");%>
  <%Html.RenderPartial("ProjMilestoneTasksPageTmpl");%>
  <%Html.RenderPartial("ProjTaskPageTmpl");%>
  <%Html.RenderPartial("ProjDiscussionPageTmpl");%>
  <%Html.RenderPartial("ProjAddTaskPageTmpl");%>
  <%Html.RenderPartial("ProjAddMilestonePageTmpl");%>
  <%Html.RenderPartial("ProjAddDiscussionPageTmpl");%>
  <%Html.RenderPartial("ProjTasksTmpl");%>

  <%Html.RenderPartial("PeoplePageTmpl");%>
  <%Html.RenderPartial("PeopPersonPageTmpl");%>

  <%Html.RenderPartial("DocsAddFileDialogTmpl");%>

  <%Html.RenderPartial("DocumentsPageTmpl");%>
  <%Html.RenderPartial("DocsFilePageTmpl");%>
  <%Html.RenderPartial("DocsAddItemPageTmpl");%>
  <%Html.RenderPartial("DocsAddFilePageTmpl");%>
  <%Html.RenderPartial("DocsAddFolderPageTmpl");%>
  <%Html.RenderPartial("DocsAddDocumentPageTmpl");%>

  <%Html.RenderPartial("CrmAddCustomerDialogTmpl");%>
  <%Html.RenderPartial("CrmAddFileDialogTmpl");%>

  <%Html.RenderPartial("CrmPageTmpl");%>
  <%Html.RenderPartial("CrmTasksPageTmpl");%>
  <%Html.RenderPartial("CrmTimeLineTmpl");%>
  <%Html.RenderPartial("CrmTasksTimeLineTmpl");%> 
  <%Html.RenderPartial("CrmTaskPageTmpl");%>
  <%Html.RenderPartial("CrmAddTaskPageTmpl");%> 
  <%Html.RenderPartial("CrmHistoryPageTmpl");%> 
  <%Html.RenderPartial("CrmContactTasksPageTmpl");%> 
  <%Html.RenderPartial("CrmContactFilesPageTmpl");%>
  <%Html.RenderPartial("CrmContactPersonesPageTmpl");%>
  <%Html.RenderPartial("CrmPersonesTimeLineTmpl");%>
  <%Html.RenderPartial("CrmPersonPageTmpl");%>
  <%Html.RenderPartial("CrmCompanyPageTmpl");%>
  <%Html.RenderPartial("CrmNavigateDialogTmpl");%>
  <%Html.RenderPartial("CrmAddToContactDialog");%>
  <%Html.RenderPartial("CrmAddPersonePageTmpl");%>
  <%Html.RenderPartial("CrmAddCompanyPageTmpl");%>
  <%Html.RenderPartial("CrmAddHistoryEventPageTmpl");%>
  <%Html.RenderPartial("CrmAddNotePageTmpl");%>
  <%Html.RenderPartial("CrmDataCategotyTmpl");%>
  <%Html.RenderPartial("DocsEditFilePageTmpl");%>
</asp:Content>

<asp:Content ID="Body" ContentPlaceHolderID="Body" runat="server">
  <div class="ui-page ui-invalid-client-page ui-page-active" id="invalid-client-page">
    <div class="ui-invalid-client-label"><span><%=Resources.MobileResource.ErrInvalidClient%></span></div>
    <div class="ui-background"></div>
  </div>
  <div class="ui-page ui-invalid-client-page ui-page-active" id="disabled-cookie-page">
    <div class="ui-invalid-client-label"><span><%=Resources.MobileResource.ErrDisabledCookie%></span></div>
    <div class="ui-background"></div>
  </div>
  <div class="ui-page ui-change-page" id="change-page">
    <div class="ui-indicator"><span><%=Resources.MobileResource.LblLoading%></span></div>
    <div class="ui-background"></div>
  </div>
  <script type="text/javascript">
    (function () {
      window.INVALIDCLIENT = false;
      document.cookie = document.cookie || 'key=value;';
      if (!document.cookie) {
        window.DISABLEDCOOKIES = true;
        return undefined;
      }
      var cookiepage = document.getElementById('disabled-cookie-page');
      if (cookiepage) {
        cookiepage.style.display = 'none';
        cookiepage.className = cookiepage.className.replace(' ui-page-active', '');
      }
      if (!window.XMLHttpRequest && !window.ActiveXObject) {
        window.INVALIDCLIENT = true;
        return undefined;
      }
      var invalidpage = document.getElementById('invalid-client-page');
      if (invalidpage) {
        invalidpage.style.display = 'none';
        invalidpage.className = invalidpage.className.replace(' ui-page-active', '');
      }
    })();
  </script>
</asp:Content>
