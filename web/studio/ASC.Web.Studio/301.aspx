<%@ Page Language="C#" AutoEventWireup="true"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="ru" lang="ru" dir="ltr">
  <head>
  <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
  <link rel="icon" href="./favicon.ico" type="image/x-icon" />
  <title>Teamlab has been rebranded to ONLYOFFICE</title>
  <style type="text/css">
    html, body, div, span, p{margin:0; padding:0; border:0; vertical-align:baseline; background:transparent;}
      body {
          background: #DFDFDF url('data:image/jpeg;base64,/9j/4AAQSkZJRgABAgAAZABkAAD/7AARRHVja3kAAQAEAAAAUAAA/+4ADkFkb2JlAGTAAAAAAf/bAIQAAgICAgICAgICAgMCAgIDBAMCAgMEBQQEBAQEBQYFBQUFBQUGBgcHCAcHBgkJCgoJCQwMDAwMDAwMDAwMDAwMDAEDAwMFBAUJBgYJDQsJCw0PDg4ODg8PDAwMDAwPDwwMDAwMDA8MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwM/8AAEQgDrgABAwERAAIRAQMRAf/EAaIAAAAHAQEBAQEAAAAAAAAAAAQFAwIGAQAHCAkKCwEAAgIDAQEBAQEAAAAAAAAAAQACAwQFBgcICQoLEAACAQMDAgQCBgcDBAIGAnMBAgMRBAAFIRIxQVEGE2EicYEUMpGhBxWxQiPBUtHhMxZi8CRygvElQzRTkqKyY3PCNUQnk6OzNhdUZHTD0uIIJoMJChgZhJRFRqS0VtNVKBry4/PE1OT0ZXWFlaW1xdXl9WZ2hpamtsbW5vY3R1dnd4eXp7fH1+f3OEhYaHiImKi4yNjo+Ck5SVlpeYmZqbnJ2en5KjpKWmp6ipqqusra6voRAAICAQIDBQUEBQYECAMDbQEAAhEDBCESMUEFURNhIgZxgZEyobHwFMHR4SNCFVJicvEzJDRDghaSUyWiY7LCB3PSNeJEgxdUkwgJChgZJjZFGidkdFU38qOzwygp0+PzhJSktMTU5PRldYWVpbXF1eX1RlZmdoaWprbG1ub2R1dnd4eXp7fH1+f3OEhYaHiImKi4yNjo+DlJWWl5iZmpucnZ6fkqOkpaanqKmqq6ytrq+v/aAAwDAQACEQMRAD8A+vFM3zz1LqYFXYpbwJX0wIpdhSupgSuwKvoMCW6YrS6h8MKV+RVdTFK7Aq+mKV2BK6mKV9MC0voMUrsCV9BgtV1MU0voMbVdgTS+gwJXUOKqlMC03Q4pX4ErqHFK/Aq6hxSvyKV9MVXYpXUwJX4FXUxTa/FV9BgS3gWm6HwxSqYq3Q4Er8Ct4sqX4ELsKV1MCV1MC03Q4pX0wKuwpb4++C1XYFbxTTdD4YqvyKXYVbxSupii3cvbAlbirsVdiqnirsVW8jiq3FWuQxVZirqjxxVTxV2KqeKuqPHFVPFXYqp1PjiqzkcVaxVbyOKrajxxVTxVrkMVWYqsqcVW1GKrMVW8vbFVuKrKnFVtRiqzFVvL2xVbUeOKqeKrKnFWsVU8VaqMVWYqt5e2KrcVU8VW8jiq3FVlT44q1iqnirsVU8VdUeOKqeKrORxVrFVvL2xVSq/8v44q/wD/2Q==') 0 0 repeat-x;
          cursor: default;
          min-height: 100%;
          position: relative;
      }

      html {
          height: 100%;
      }
      * html body {
          height: 100%;
      }
      #title {
          display: block;
          padding-bottom: 20px;
      }
      #wrapper {
          background: url("http://cdn.onlyoffice.com/cloudftlogo.png") center 0 no-repeat;
          width: 100%;
          height: 310px;
          padding: 315px 0 0;
          margin-top: -325px;
          position: absolute;
          left: 0;
          top: 50%;
          z-index: 1;
      }
      #container {
          width: 410px;
          margin: 0 auto;
          padding-left: 38px;
          text-align: center;
          font: normal 14px/22px Tahoma;
          color: #333;
      }
          #container a {
              color: #116d9d;
              text-decoration: none;
          }
              #container a:hover {
                  text-decoration: underline;
              }
  </style>

   <script runat="server">
       protected string Alias { get; set; }
       protected void Page_Load()
       {
           var Tenant = ASC.Core.CoreContext.TenantManager.GetCurrentTenant();
           Alias = Tenant.TenantAlias;
       }
    </script>
</head>

<body>
  <div id="wrapper">
    <div id="container">
      <b id="title">Teamlab has been <a target="_blank" href="http://www.onlyoffice.com/blog/2014/07/teamlab-birthday/">rebranded</a> to ONLYOFFICE</b>
      <p>Please reconfigure your custom DNS</p>
      <p>changing <b>CNAME to <%=Alias %>.onlyoffice.com</b></p>
      <p>or use the <a href="https://<%=Alias %>.onlyoffice.com">direct link</a> to access your portal.</p>
    </div>
  </div>
</body>
</html>