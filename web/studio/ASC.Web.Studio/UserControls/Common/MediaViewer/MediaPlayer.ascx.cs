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


using System;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class MediaPlayer : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/MediaViewer/MediaPlayer.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Common/MediaViewer/mediaplayer.js",
                "~/UserControls/Common/MediaViewer/imageviewer.js",
                "~/UserControls/Common/MediaViewer/tiff.min.js",
                "~/js/third-party/jquery/jquery.jplayer.js",
                "~/js/third-party/jquery/jquery.mousewheel.js")
                .RegisterStyle("~/UserControls/Common/MediaViewer/mediaplayer.css");
        }
    }
}