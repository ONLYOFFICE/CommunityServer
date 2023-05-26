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


namespace ASC.Thrdparty.Twitter
{
    public class TwitterUserInfo
    {
        ///<example type="dobule">1,1</example>
        public decimal UserID { get; set; }

        ///<example>Screen name</example>
        public string ScreenName { get; set; }

        ///<example>User Name</example>
        public string UserName { get; set; }

        ///<example>url to img</example>
        public string SmallImageUrl { get; set; }

        ///<example>Description</example>
        public string Description { get; set; }
    }
}
