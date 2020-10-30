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
using System.Runtime.Serialization;
using ASC.Web.Core.Users;

namespace ASC.Api.Employee
{
    [DataContract]
    public class ThumbnailsDataWrapper
    {
        public ThumbnailsDataWrapper(Guid userId)
        {
            Original = UserPhotoManager.GetPhotoAbsoluteWebPath(userId);
            Retina = UserPhotoManager.GetRetinaPhotoURL(userId);
            Max = UserPhotoManager.GetMaxPhotoURL(userId);
            Big = UserPhotoManager.GetBigPhotoURL(userId);
            Medium = UserPhotoManager.GetMediumPhotoURL(userId);
            Small = UserPhotoManager.GetSmallPhotoURL(userId);
        }

        private ThumbnailsDataWrapper()
        {
        }

        [DataMember]
        public string Original { get; set; }

        [DataMember]
        public string Retina { get; set; }

        [DataMember]
        public string Max { get; set; }

        [DataMember]
        public string Big { get; set; }

        [DataMember]
        public string Medium { get; set; }

        [DataMember]
        public string Small { get; set; }


        public static ThumbnailsDataWrapper GetSample()
        {
            return new ThumbnailsDataWrapper
                {
                    Original = "default_user_photo_size_1280-1280.png",
                    Retina = "default_user_photo_size_360-360.png",
                    Max = "default_user_photo_size_200-200.png",
                    Big = "default_user_photo_size_82-82.png",
                    Medium = "default_user_photo_size_48-48.png",
                    Small = "default_user_photo_size_32-32.png",
                };
        }
    }
}