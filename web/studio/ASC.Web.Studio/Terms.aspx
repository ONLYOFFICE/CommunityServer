<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Terms.aspx.cs" Inherits="ASC.Web.Studio.Terms" %>

<%@ Import Namespace="System.Globalization" %>

<%
    var termsLinks = (new Dictionary<string, string>
        {
            { "de", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543238&doc=d0pIK2JUdzZwbFhiK3dDK0xPNXZLcHVWWXZiVXR1R2VJWitkdG5rZ2kvZz0_IjQ1NDMyMzgi0" },
            { "en", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543205&doc=VXlOK1NnMVdIYStuSFpMeFR4UVpmNE5VS3VTdENYdU50WjJ5Unh0OERiUT0_IjQ1NDMyMDUi0" },
            { "es", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543242&doc=emhVSzlYK1BzV0tJZVl6M2RoR0tRY3F6ZG9tWWxCWXdoWnYwajRrNGwrST0_IjQ1NDMyNDIi0" },
            { "fr", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543271&doc=R2tBVmd0ZFlsOTNlamFGek52bnBTd0d1bjFVNHhqb2V1b0VmWkZnaFBLcz0_IjQ1NDMyNzEi0" },
            { "pt", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543279&doc=M21zY0xiTjBQd0FTSUxKTGs3N29SQ1RKQWZQNEdPcEdidGNrcEZDdFEvcz0_IjQ1NDMyNzki0" },
            { "ru", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543231&doc=dWRxME5LUGhJNWdNcWlmUklody9HNWRFTC8zQXdoUmJXNGM0eFdrQ016WT0_IjQ1NDMyMzEi0" },
            { "sl", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543286&doc=QllKZHZrTlBKQTNlNXIzSkFMbEp6cXRRdHFsWEtFNHZCZjRNRHpxMGU2MD0_IjQ1NDMyODYi0" },
            { "tr", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543292&doc=S0kyd0llSXBCZGJycHhVVldpYm1MeGg4YWV3eFJYaE1yUkVPVmsyRndpRT0_IjQ1NDMyOTIi0" },
        });
    var termsLink = termsLinks.ContainsKey(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                        ? termsLinks[CultureInfo.CurrentUICulture.TwoLetterISOLanguageName]
                        : termsLinks["en"];
    Response.Redirect(termsLink, true);
%>
