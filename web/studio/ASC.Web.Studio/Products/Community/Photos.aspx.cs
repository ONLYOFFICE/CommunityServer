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


#region Import

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Api;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;

#endregion

namespace ASC.Web.Community
{
    public partial class Photos : MainPage
    {
        protected ApiServer apiServer = new ApiServer();

        protected void Page_Load(object sender, EventArgs e)
        {
            PublishRecentUploadPhotos("recentPhotos");
            PublishAllAlbumInfo("");
            PublishAlbumsPhotos("",Enumerable.Empty<int>());
            
           // Page.JsonPublisher(contactsForFirstRequest, "contactsForFirstRequest");

        }

        protected String PublishRecentUploadPhotos(String publishByName)
        {
            var apiResponse = apiServer.GetApiResponse(
                String.Format("{0}crm/contact/filter.json?{1}", SetupInfo.WebApiBaseUrl, ""), "GET");

            JsonPublisher(apiResponse, publishByName);

            return apiResponse;
        }

        protected String PublishAllAlbumInfo(String publishByName)
        {
            var apiResponse = apiServer.GetApiResponse(String.Format("{0}files/@photos", SetupInfo.WebApiBaseUrl), "GET");

            JsonPublisher(apiResponse, publishByName);

            return apiResponse;
        }

        protected String PublishAlbumsPhotos(String publishByName, IEnumerable<int> albumIDs)
        {

            throw new NotImplementedException();
        }

        protected void JsonPublisher<T>(T data, String jsonClassName) where T : class
        {

            String json;

            using (var stream = new MemoryStream())
            {

                var serializer = new DataContractJsonSerializer(typeof(T));

                serializer.WriteObject(stream, data);

                json = Encoding.UTF8.GetString(stream.ToArray());
            }

            Page.ClientScript.RegisterClientScriptBlock(GetType(),
                                                       Guid.NewGuid().ToString(),
                                                        String.Format(" var {1} = {0};", json, jsonClassName),
                                                        true);
        }
    }
}
