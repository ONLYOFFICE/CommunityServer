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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects
{
    public abstract class CustomStatus
    {
        ///<example type="int">12</example>
        public int Id { get; set; }

        ///<example>Image</example>
        public string Image { get; set; }

        ///<example>ImageType</example>
        public string ImageType { get; set; }

        ///<example>Title</example>
        public string Title { get; set; }

        ///<example>Description</example>
        public string Description { get; set; }

        ///<example>Color</example>
        public string Color { get; set; }

        ///<example type="int">1</example>
        public int Order { get; set; }

        ///<example>true</example>
        public bool IsDefault { get; set; }

        ///<example>true</example>
        public bool? Available { get; set; }

        protected static string GetImageBase64Content(string path)
        {
            var serverPath = HttpContext.Current.Server.MapPath(path);
            if (string.IsNullOrEmpty(serverPath)) return "";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(serverPath)));
        }
    }

    ///<inherited>ASC.Web.Projects.CustomStatus, ASC.Web.Projects</inherited>
    public class CustomTaskStatus : CustomStatus
    {
        ///<example type="int">1</example>
        public TaskStatus StatusType { get; set; }

        ///<example>true</example>
        public bool CanChangeAvailable { get { return StatusType != TaskStatus.Open; } set { } }

        public static List<CustomTaskStatus> GetDefaults()
        {
            return new List<CustomTaskStatus>
            {
                GetDefault(TaskStatus.Open),
                GetDefault(TaskStatus.Closed)
            };
        }

        public static CustomTaskStatus GetDefault(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Open:
                    return GetDefault(status, TaskResource.Open, "inbox.svg");
                case TaskStatus.Closed:
                    return GetDefault(status, TaskResource.Closed, "check_tick.svg");
            }
            return null;
        }

        private static CustomTaskStatus GetDefault(TaskStatus status, string title, string svg, bool available = true)
        {
            return new CustomTaskStatus
            {
                StatusType = status,
                Id = -(int)status,
                Title = title,
                Image = GetImageBase64Content("/skins/default/images/svg/projects/" + svg),
                ImageType = "image/svg+xml",
                Color = "#83888d",
                IsDefault = true,
                Available = available
            };
        }
    }
}