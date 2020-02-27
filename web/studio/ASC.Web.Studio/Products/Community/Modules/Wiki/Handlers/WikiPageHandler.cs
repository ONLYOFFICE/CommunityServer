/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Collections.Generic;
using System.Web;

namespace ASC.Web.UserControls.Wiki.Handlers
{

    public class WikiPageHandler : IHttpModule
    {

        #region IHttpModule Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += new EventHandler(Application_BeginRequest);
        }

        void Application_BeginRequest(object sender, EventArgs e)
        {
            string wikiView = WikiSection.Section.MainPage.WikiView;
            string wikiEdit = WikiSection.Section.MainPage.WikiEdit;

            HttpApplication app = (HttpApplication)sender;
            string path = app.Request.RawUrl;
            if (path.Contains(wikiView))
            {
                path = path.Substring(path.IndexOf(wikiView) + wikiView.Length).TrimStart('/').Split('?')[0].Trim();
                if(string.IsNullOrEmpty(path))
                {
                    path = WikiSection.Section.MainPage.Url.Split('?')[0];
                }
                else
                {
                    path = string.Format(WikiSection.Section.MainPage.Url, path);
                }

                app.Context.RewritePath(path);
            }
            else if (path.Contains(wikiEdit))
            {
                path = path.Substring(path.IndexOf(wikiEdit) + wikiEdit.Length).TrimStart('/').Trim();
                path = path.Replace("?", "&");
                if(path[0] == '&')
                {
                    path = string.Format("?{0}", path.Substring(1));
                    path = string.Format("{0}{1}", WikiSection.Section.MainPage.Url.Split('?')[0], path);

                }
                else
                {
                    path = string.Format(WikiSection.Section.MainPage.Url, path);
                }
                
                app.Context.RewritePath(path);
            }
            
        }

        #endregion
    }
}
