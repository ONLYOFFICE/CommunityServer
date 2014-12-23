/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
        Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/uploader/plupload.js"));
        Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/uploader/pluploadManager.js"));

        Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/common/ckeditor/ckeditor-connector.js"));

        AjaxPro.Utility.RegisterTypeForAjax(this.GetType());

        TagsPageHolder.Controls.Add(LoadControl(TagsPage.Location) as TagsPage);
        TagsPageHolder.Controls.Add(LoadControl(AccountsPage.Location) as AccountsPage);
        TagsPageHolder.Controls.Add(LoadControl(ContactsPage.Location) as ContactsPage);

        if (SetupInfo.IsVisibleSettings<AdministrationPage>())
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

        if (!TenantExtra.GetTenantQuota().DocsEdition)
        {
            _phDocUploader.Controls.Add(LoadControl(TariffLimitExceed.Location));
        }

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
        string res = css;
        res = "\r\n" + cssTag.Replace(css, "").Replace("\r\n", "").Replace("}", "}\r\n");
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
  }
}
