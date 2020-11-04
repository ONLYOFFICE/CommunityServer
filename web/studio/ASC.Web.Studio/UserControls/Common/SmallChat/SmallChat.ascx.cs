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


using ASC.Data.Storage;
using System;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Common.SmallChat
{
    public partial class SmallChat : UserControl
    {
        public static string Location
        {
            get
            {
                return "~/UserControls/Common/SmallChat/SmallChat.ascx";
            }
        }

        public static string SoundPath { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            SoundPath = WebPath.GetPath("UserControls/Common/SmallChat/css/sounds/chat");

            Page
                .RegisterStyle(
                    "~/UserControls/Common/SmallChat/css/smallchat.css",
                    "~/UserControls/Common/SmallChat/css/jquery.cssemoticons.css")
                .RegisterBodyScripts(
                    "~/js/third-party/jquery/jquery.linkify.js",
                    "~/js/third-party/jquery/noty/jquery.noty.js",
                    "~/js/third-party/jquery/jquery.cssemoticons.js",
                    "~/js/third-party/autosize.js",
                    "~/UserControls/Common/SmallChat/js/smallchat.js");
        }
    }
}