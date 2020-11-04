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

namespace ASC.Web.Core.Client
{
    public static class ClientCapabilities
    {
        public static long GetMaxEmbeddableImageSize(HttpRequestBase request)
        {
            var ieVersion = GetInternetExplorerVersion(request);
            if (ieVersion<0)
            {
                //Not an IE
                return ClientSettings.MaxImageEmbeddingSize;
            }
            if (ieVersion<8)
            {
                return 0;//Can't embed at all
            }
            if (ieVersion<9)
            {
                //IE8 can embed up to 32kb
                return 32*1024;
            }
            return ClientSettings.MaxImageEmbeddingSize;
        }

        public static double GetInternetExplorerVersion(HttpRequestBase request)
        {
            double rv = -1;
            var browser = request.Browser;
            if (browser.Browser == "IE")
                rv = browser.MajorVersion + browser.MinorVersion;
            return rv;
        }
    }
}