/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using ASC.Web.Core.Users;
using log4net;
using System;
using System.IO;
using System.Net;

namespace ASC.SingleSignOn.Common
{
    public class UserPhotoLoader
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(UserPhotoLoader));
        private const int ONE_MEGABYTE = 1048576;

        public void SaveOrUpdatePhoto(string photoUrl, Guid userId)
        {
            if (photoUrl != null)
            {
                if (CheckUri(photoUrl))
                {
                    _log.DebugFormat("Loading image: {0}", photoUrl);
                    var data = GetUserPhoto(photoUrl);
                    if (data != null)
                    {
                        UserPhotoManager.SaveOrUpdatePhoto(userId, data);
                    }
                    else
                    {
                        _log.DebugFormat("Can't load image: {0}. Image size is more than 1Mb", photoUrl);
                    }
                }
                else
                {
                    _log.ErrorFormat("Wrong photo url: {0}", photoUrl);
                }
            }
        }

        private byte[] GetUserPhoto(string photoUrl)
        {
            try
            {
                WebRequest req = HttpWebRequest.Create(photoUrl);
                req.Method = "HEAD";
                using (WebResponse resp = req.GetResponse())
                {
                    int ContentLength;
                    if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength) && ContentLength <= ONE_MEGABYTE)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(photoUrl);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream resStream = response.GetResponseStream();
                        var data = ReadFully(resStream, ContentLength);
                        resStream.Dispose();
                        return data;
                    }
                    else
                    {
                        _log.ErrorFormat("Can't parse ContentLength {0}", ContentLength);
                        return null;
                    }
                }
            }
            catch(Exception e)
            {
                _log.ErrorFormat("Unexpected error: {0}", e);
                return null;
            }
        }

        private byte[] ReadFully(Stream input, int contentLength)
        {
            byte[] buffer = new byte[contentLength];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private bool CheckUri(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}