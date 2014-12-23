/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
