/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System.Web;
using System.Web.UI;

namespace ASC.Web.Community.Wiki.Common
{
    public static class ResponseExtention
    {
        public static void RedirectLC(this HttpResponse response, string url, Page page)
        {
            response.Redirect(page.ResolveUrlLC(url));
        }

        public static void RedirectLC(this HttpResponse response, string url, Page page, bool endResponse)
        {
            response.Redirect(page.ResolveUrlLC(url), endResponse);
        }
    }

    public static class ControlsExtention
    {
        public static string ResolveUrlLC(this Control control, string url)
        {
            if (!url.Contains("?"))
            {
                return control.ResolveUrl(url).ToLower();
            }

            string sUrl = url.Split('?')[0];
            string sParams = url.Split('?')[1];

            return string.Format("{0}?{1}", control.ResolveUrl(sUrl).ToLower(), sParams);

        }
    }
}