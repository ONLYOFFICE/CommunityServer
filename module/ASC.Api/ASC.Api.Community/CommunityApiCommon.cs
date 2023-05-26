/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System;
using System.Web;

using ASC.Api.Attributes;
using ASC.Core;
using ASC.Web.Studio.Utility.HtmlUtility;


namespace ASC.Api.Community
{
    ///<name>community</name>
    public partial class CommunityApi
    {
        /// <summary>
        /// Returns the preview information about the specified category from the community section.
        /// </summary>
        /// <short>Get preview</short>
        /// <param type="System.String, System" name="title">Category title</param>
        /// <param type="System.String, System" name="content">Category content</param>
        /// <returns>Preview information</returns>
        /// <path>api/2.0/community/preview</path>
        /// <httpMethod>POST</httpMethod>
        [Create("preview")]
        public object GetPreview(string title, string content)
        {
            return new
            {
                title = HttpUtility.HtmlEncode(title),
                content = HtmlUtility.GetFull(content, false)
            };
        }
    }
}
