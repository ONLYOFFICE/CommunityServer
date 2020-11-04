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
using System.Globalization;
using System.Web;
using System.Web.UI;
using ASC.Core.Users;
using ASC.Web.Core.Users;

namespace ASC.Web.Studio.UserControls.Users.UserProfile
{
    public partial class LoadPhotoControl : UserControl
    {
        public UserInfo User { get; set; }

        protected bool HasAvatar { get; set; }
        protected string MainImgUrl { get; set; }
        protected UserPhotoThumbnailSettings ThumbnailSettings { get; set; }

        protected bool IsLdap { get; set; }

        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/LoadPhotoControl.ascx"; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var defuaultPhoto = UserPhotoManager.GetDefaultPhotoAbsoluteWebPath();

            if (User == null)
            {
                MainImgUrl = defuaultPhoto;
                ThumbnailSettings = UserPhotoThumbnailSettings.LoadForDefaultTenant();
            }
            else
            {
                IsLdap = User.IsLDAP();
                HasAvatar = User.HasAvatar();
                MainImgUrl = UserPhotoManager.GetPhotoAbsoluteWebPath(User.ID);
                ThumbnailSettings = UserPhotoThumbnailSettings.LoadForUser(User.ID);
            }

            Page.RegisterStyle("~/UserControls/Users/UserProfile/css/loadphoto_style.less",
                               "~/UserControls/Users/UserProfile/css/jquery.jcrop.less")
                .RegisterBodyScripts("~/js/uploader/ajaxupload.js",
                                    "~/UserControls/Users/UserProfile/js/loadphoto.js",
                                     "~/UserControls/Users/UserProfile/js/jquery.jcrop.js");

            var script =
                string.Format(
                    "window.ASC.Controls.LoadPhotoImage.init('{0}',[{1},{2}],{{point:{{x:{3},y:{4}}},size:{{width:{5},height:{6}}}}},'{7}', '{8}');",
                    User == null ? "" : User.ID.ToString(),
                    UserPhotoManager.SmallFotoSize.Width,
                    UserPhotoManager.SmallFotoSize.Height,
                    ThumbnailSettings.Point.X,
                    ThumbnailSettings.Point.Y,
                    ThumbnailSettings.Size.Width,
                    ThumbnailSettings.Size.Height,
                    HasAvatar ? MainImgUrl : "",
                    defuaultPhoto);

            Page.RegisterInlineScript(script);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}