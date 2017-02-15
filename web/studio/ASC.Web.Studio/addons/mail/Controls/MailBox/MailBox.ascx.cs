/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Web.Mail.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
//using ASC.Web.Studio.UserControls.Common.DocumentsPopup;
using ASC.Web.Studio.UserControls.Management;

namespace ASC.Web.Mail.Controls
{
  [AjaxPro.AjaxNamespace("MailBox")]
  public partial class MailBox : System.Web.UI.UserControl
  {
    public static string Location { get { return "~/addons/mail/Controls/MailBox/MailBox.ascx"; } }
    public const int EntryCountOnPage_def = 25;
    public const int VisiblePageCount_def = 10;

    protected void Page_Load(object sender, EventArgs e)
    {
        Page.RegisterBodyScripts("~/js/uploader/jquery.fileupload.js",
            "~/usercontrols/common/ckeditor/ckeditor-connector.js");

        AjaxPro.Utility.RegisterTypeForAjax(this.GetType());

        TagsPageHolder.Controls.Add(LoadControl(TagsPage.Location) as TagsPage);
        TagsPageHolder.Controls.Add(LoadControl(AccountsPage.Location) as AccountsPage);
        TagsPageHolder.Controls.Add(LoadControl(ContactsPage.Location) as ContactsPage);

        if (Configuration.Settings.IsAdministrationPageAvailable())
            TagsPageHolder.Controls.Add(LoadControl(AdministrationPage.Location) as AdministrationPage);
        //init Page Navigator
        _phPagerContent.Controls.Add(new PageNavigator
        {
            ID = "mailPageNavigator",
            CurrentPageNumber = 1,
            VisibleOnePage = false,
            EntryCount = 0,
            VisiblePageCount = VisiblePageCount_def,
            EntryCountOnPage = EntryCountOnPage_def
        });

        var documentsPopup = (DocumentsPopup)LoadControl(DocumentsPopup.Location);
        _phDocUploader.Controls.Add(documentsPopup);

        QuestionPopup.Options.IsPopup = true;

        var fileSelector = (Files.Controls.FileSelector) LoadControl(Files.Controls.FileSelector.Location);
        fileSelector.DialogTitle = MailResource.SelectFolderDialogTitle;
        fileholder.Controls.Add(fileSelector);
    }

    protected String RenderRedirectUpload()
    {
        return string.Format("{0}://{1}:{2}{3}", Request.GetUrlRewriter().Scheme, Request.GetUrlRewriter().Host, Request.GetUrlRewriter().Port, VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?newEditor=true&esid=mail");
    }

    private static readonly Regex cssBlock = new Regex(@"(\<style(.)*?\>.*?((\r\n)*|.*)*?\<\/style\>)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex cssTag = new Regex(@"(\<style(.)*?\>|<\/style\>)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex cssRow = new Regex(@"((\r\n)(.*){)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private String wrapCSS(string css)
    {
        string res = "\r\n" + cssTag.Replace(css, "").Replace("\r\n", "").Replace("}", "}\r\n");
        MatchCollection mc = cssRow.Matches(res);
        foreach (Match occur in mc)
        {
            string selectors = occur.Value;
            if (!string.IsNullOrEmpty(selectors))
            {
                selectors = selectors.Replace("\r\n", "\r\n#itemContainer .body ").Replace(",", ", #itemContainer .body ");
            }
            res = res.Replace(occur.Value, selectors);

        }
        res = "<style>" + res + "</style>";
        return res;
    }

    private String handleCSS(string html)
    {
        if (cssBlock.IsMatch(html))
        {
            MatchCollection mc = cssBlock.Matches(html);
            foreach (Match occur in mc)
            {
                html = html.Replace(occur.Value, wrapCSS(occur.Value));
            }
        }
        return html;
    }

    [AjaxPro.AjaxMethod(AjaxPro.HttpSessionStateRequirement.ReadWrite)]
    public string getHtmlBody(string url)
    {
        try
        {
            HttpWebRequest loHttp = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse loWebResponse = (HttpWebResponse)loHttp.GetResponse();
            StreamReader loResponseStream = new StreamReader(loWebResponse.GetResponseStream());
            string html = loResponseStream.ReadToEnd();
            loWebResponse.Close();
            loResponseStream.Close();

            html = handleCSS(html);

            return html;
        }
        catch
        {
            return String.Empty;
        }
    }

    public bool IsMailPrintAvailable()
    {
        return SetupInfo.IsVisibleSettings("MailPrint");
    }
  }
}
