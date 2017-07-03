/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.IO;
using System.Net;
using ASC.Web.Core.Users;
using log4net;

namespace ASC.SingleSignOn.Common
{
    public class UserPhotoLoader
    {
        private readonly ILog log = LogManager.GetLogger(typeof(UserPhotoLoader));
        private const int OneMegabyte = 1048576;

        public void SaveOrUpdatePhoto(string photoUrl, Guid userId)
        {
            if (photoUrl != null)
            {
                if (CheckUri(photoUrl))
                {
                    log.DebugFormat("Loading image: {0}", photoUrl);
                    var data = GetUserPhoto(photoUrl);
                    if (data != null)
                    {
                        UserPhotoManager.SaveOrUpdatePhoto(userId, data);
                    }
                    else
                    {
                        log.DebugFormat("Can't load image: {0}. Image size is more than 1Mb", photoUrl);
                    }
                }
                else
                {
                    log.ErrorFormat("Wrong photo url: {0}", photoUrl);
                }
            }
        }

        private byte[] GetUserPhoto(string photoUrl)
        {
            try
            {
                var req = HttpWebRequest.Create(photoUrl);
                req.Method = "HEAD";
                using (var resp = req.GetResponse())
                {
                    int ContentLength;
                    if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength) && ContentLength <= OneMegabyte)
                    {
                        var request = (HttpWebRequest)WebRequest.Create(photoUrl);
                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            var resStream = response.GetResponseStream();
                            var data = ReadFully(resStream, ContentLength);
                            resStream.Dispose();
                            return data;
                        }
                    }
                    else
                    {
                        log.ErrorFormat("Can't parse ContentLength {0}", ContentLength);
                        return null;
                    }
                }
            }
            catch(Exception e)
            {
                log.ErrorFormat("Unexpected error: {0}", e);
                return null;
            }
        }

        private static byte[] ReadFully(Stream input, int contentLength)
        {
            var buffer = new byte[contentLength];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private static bool CheckUri(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}