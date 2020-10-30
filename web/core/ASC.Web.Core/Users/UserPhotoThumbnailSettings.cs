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
using System.Drawing;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.Web.Core.Users
{
    [Serializable]
    [DataContract]
    public class UserPhotoThumbnailSettings : BaseSettings<UserPhotoThumbnailSettings>
    {
        public override Guid ID
        {
            get { return new Guid("{CC3AA821-43CA-421B-BDCD-81FB6D3361CF}"); }
        }

        public UserPhotoThumbnailSettings()
        {
        }

        public UserPhotoThumbnailSettings(Point point, Size size)
        {
            Point = point;
            Size = size;
        }

        public UserPhotoThumbnailSettings(int x, int y, int width, int height)
        {
            Point = new Point(x, y);
            Size = new Size(width, height);
        }

        [DataMember(Name = "Point")]
        public Point Point { get; set; }

        [DataMember(Name = "Size")]
        public Size Size { get; set; }

        [DataMember(Name = "IsDefault")]
        public bool IsDefault { get; private set; }

        public override ISettings GetDefault()
        {
            return new UserPhotoThumbnailSettings
                {
                    Point = new Point(0, 0),
                    Size = new Size(UserPhotoManager.MaxFotoSize.Width, UserPhotoManager.MaxFotoSize.Height),
                    IsDefault = true
                };
        }
    }
}