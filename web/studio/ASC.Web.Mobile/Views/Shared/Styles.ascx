<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%if(HttpContext.Current.IsDebuggingEnabled){%>
  <%=Html.Style("~/content/all.css", false, 1)
    .With("~/content/mc.style.css")    
    .With("~/content/mc.style.crm.tasks.css")
    .With("~/content/mc.style.crm.contact.css")
    .With("~/content/mc.style.inline-img0.css")
    .With("~/content/mc.style.inline-img1.css")
    .With("~/content/mc.style.inline-img2.css")
    .With("~/content/mc.style.inline-img3.css")
    .With("~/content/mc.style.inline-img4.css")
    .With("~/content/mc.style.inline-img5.css")
    .With("~/content/mc.style.inline-img6.css")
    .With("~/content/mc.style.inline-img7.css")
    .With("~/content/mc.style.inline-img8.css")
    .With("~/content/mc.style.inline-img9.css")
    .With("~/content/jquery.mvc.validation.css")
    .With("~/content/jquery.scroller.css")
    .With("~/content/smart-app-banner.css")
    .Render()
  %>
  <link href="<%=Url.Content("~/content/mc.style.desktop.css")%>" rel="stylesheet" type="text/css" rel="stylesheet" media="only screen and (min-device-width:801px)" />
<%}else{%>
  <%=Html.Style("~/content/all.min.css", true, 1)
    .With("~/content/mc.style.min.css")
    .With("~/content/mc.style.crm.tasks.min.css")
    .With("~/content/mc.style.crm.contact.min.css")
    .With("~/content/mc.style.inline-img0.min.css")
    .With("~/content/mc.style.inline-img1.min.css")
    .With("~/content/mc.style.inline-img2.min.css")
    .With("~/content/mc.style.inline-img3.min.css")
    .With("~/content/mc.style.inline-img4.min.css")
    .With("~/content/mc.style.inline-img5.min.css")
    .With("~/content/mc.style.inline-img6.min.css")
    .With("~/content/mc.style.inline-img7.min.css")
    .With("~/content/mc.style.inline-img8.min.css")
    .With("~/content/mc.style.inline-img9.min.css")
    .With("~/content/jquery.mvc.validation.min.css")
    .With("~/content/jquery.scroller.min.css")
    .With("~/content/smart-app-banner.min.css")
    .Render()
  %>
  <link href="<%=Url.Content("~/content/mc.style.desktop.min.css")%>" rel="stylesheet" type="text/css" media="only screen and (min-device-width:801px)" />
<%}%>
