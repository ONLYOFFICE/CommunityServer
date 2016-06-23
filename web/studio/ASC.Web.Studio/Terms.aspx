<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Terms.aspx.cs" Inherits="ASC.Web.Studio.Terms" %>
<%@ Import Namespace="System.Globalization" %>

    <%
        var termsLinks = (new Dictionary<string, string>
            {
                {"de", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543238&doc=Z0UxWDZ4SHE3eDZEUGxCYzErU1NjeHJuOSswcm0xYTBZQVk4OUJvdEoyRT0_IjQ1NDMyMzgi0"},
                {"en", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543205&doc=cCs2di9Vd211WG1BVWMzZ3ZDSkFzSGRFbXB2UHNjSHB6djNDTWR1YThqST0_IjQ1NDMyMDUi0"},
                {"es", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543242&doc=bDZYYUx6WE44Yk4zNlp5djNiNDg4UW5Za2VGanZHUTFYbzdoTG1UTXR5az0_IjQ1NDMyNDIi0"},
                {"fr", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543271&doc=QUQ3alVkR0hLS2Q0c1BSNUt1RjdqZXEwc3gvUVZVZlp1ay9veWFoMW4xOD0_IjQ1NDMyNzEi0"},
                {"pt", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543279&doc=N0NuYkFHdEk2c2xSU1pEZWxEK2xFSTlXM3NwL2RPSzlxQUlnTFBYeXQ5VT0_IjQ1NDMyNzki0"},
                {"ru", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543231&doc=eDFDYzZOa0F1NjhERGQ0eHQ5QThlRnBheFFxSSt2dkZoZkxzZ3htditobz0_IjQ1NDMyMzEi0"},
                {"sl", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543286&doc=eEpoK1dGSmJXcVViMU4xTXJmalBidWc1S0wzNTJ1TVJkdGJ5UHZZTmdSMD0_IjQ1NDMyODYi0"},
                {"tr", "https://help.onlyoffice.com/products/files/doceditor.aspx?fileid=4543292&doc=c09kSWdVeVN5eUdRNzArbUtsYmtoc2JmbndISS9tRzZ2MnJxcjNOUW9UOD0_IjQ1NDMyOTIi0"},
            });
        var termsLink = termsLinks.ContainsKey(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                            ? termsLinks[CultureInfo.CurrentUICulture.TwoLetterISOLanguageName]
                            : termsLinks["en"];
        Response.Redirect(termsLink, true);
    %>
