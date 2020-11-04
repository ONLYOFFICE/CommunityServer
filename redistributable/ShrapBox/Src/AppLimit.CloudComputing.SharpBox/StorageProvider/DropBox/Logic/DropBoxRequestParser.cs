using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using AppLimit.CloudComputing.SharpBox.Common.Cache;
using AppLimit.CloudComputing.SharpBox.Common.Net.Json;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox.Logic
{
    internal class DropBoxRequestParser
    {
        public static String RequestResourceByUrl(String url, IStorageProviderService service, IStorageProviderSession session, out int netErrorCode)
        {
            return RequestResourceByUrl(url, null, service, session, out netErrorCode);
        }

        private static readonly CachedDictionary<KeyValuePair<string, string>> SessionHashStorage
            = new CachedDictionary<KeyValuePair<string, string>>("dropbox-requests-hash", TimeSpan.Zero, TimeSpan.FromMinutes(30), x => true);

        public static void Addhash(string url, string hash, string response, IStorageProviderSession session)
        {
            SessionHashStorage.Add(session.SessionToken.ToString(), url, new KeyValuePair<string, string>(hash, response));
        }

        public static String RequestResourceByUrl(String url, Dictionary<String, String> parameters, IStorageProviderService service, IStorageProviderSession session, out int netErrorCode)
        {
            // cast the dropbox session
            var dropBoxSession = session as DropBoxStorageProviderSession;

            // instance the oAuthServer
            var svc = new OAuthService();

            var urlhash = new KeyValuePair<string, string>();
            if (!string.IsNullOrEmpty(url) && url.Contains("/metadata/"))
            {
                //Add the hash attr if any
                urlhash = SessionHashStorage.Get(session.SessionToken.ToString(), url, () => new KeyValuePair<string, string>());
            }

            if (!string.IsNullOrEmpty(urlhash.Key) && !string.IsNullOrEmpty(urlhash.Value))
            {
                //Add params
                if (parameters == null)
                    parameters = new Dictionary<string, string>();
                parameters.Add("hash", urlhash.Key);
            }

            // build the webrequest to protected resource
            var request = svc.CreateWebRequest(url, WebRequestMethodsEx.Http.Get, null, null, dropBoxSession.Context, (DropBoxToken)dropBoxSession.SessionToken, parameters);

            // get the error code
            WebException ex;

            // perform a simple webrequest 
            using (Stream s = svc.PerformWebRequest(request, null, out netErrorCode, out ex,
                                                    code => !string.IsNullOrEmpty(urlhash.Key) && !string.IsNullOrEmpty(urlhash.Value) && code == 304) /*to check code without downloading*/)
            {
                if (!string.IsNullOrEmpty(urlhash.Key) && !string.IsNullOrEmpty(urlhash.Value) && netErrorCode == 304)
                {
                    return urlhash.Value;
                }
                if (s == null)
                    return "";

                // read the memory stream and convert to string
                using (var response = new StreamReader(s))
                {
                    return response.ReadToEnd();
                }
            }
        }

        public static BaseFileEntry CreateObjectsFromJsonString(String jsonMessage, IStorageProviderService service, IStorageProviderSession session)
        {
            return UpdateObjectFromJsonString(jsonMessage, null, service, session);
        }

        public static BaseFileEntry UpdateObjectFromJsonString(String jsonMessage, BaseFileEntry objectToUpdate, IStorageProviderService service, IStorageProviderSession session)
        {
            // verify if we have a directory or a file
            var jc = new JsonHelper();
            if (!jc.ParseJsonMessage(jsonMessage))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);

            var isDir = jc.GetBooleanProperty("is_dir");

            // create the entry
            BaseFileEntry dbentry;
            Boolean bEntryOk;

            if (isDir)
            {
                if (objectToUpdate == null)
                    dbentry = new BaseDirectoryEntry("Name", 0, DateTime.Now, service, session);
                else
                    dbentry = objectToUpdate as BaseDirectoryEntry;

                bEntryOk = BuildDirectyEntry(dbentry as BaseDirectoryEntry, jc, service, session);
            }
            else
            {
                if (objectToUpdate == null)
                    dbentry = new BaseFileEntry("Name", 0, DateTime.Now, service, session);
                else
                    dbentry = objectToUpdate;

                bEntryOk = BuildFileEntry(dbentry, jc);
            }

            // parse the childs and fill the entry as self
            if (!bEntryOk)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCouldNotContactStorageService);

            // set the is deleted flag
            try
            {
                // try to read the is_deleted property
                dbentry.IsDeleted = jc.GetBooleanProperty("is_deleted");
            }
            catch (Exception)
            {
                // the is_deleted proprty is missing (so it's not a deleted file or folder)
                dbentry.IsDeleted = false;
            }

            // return the child
            return dbentry;
        }

        private static Boolean BuildFileEntry(BaseFileEntry fileEntry, JsonHelper jh)
        {
            /*
             *  "revision": 29251,
                "thumb_exists": false,
                "bytes": 37941660,
                "modified": "Tue, 01 Jun 2010 14:45:09 +0000",
                "path": "/Public/2010_06_01 15_53_48_336.nvl",
                "is_dir": false,
                "icon": "page_white",
                "mime_type": "application/octet-stream",
                "size": "36.2MB"
             * */

            // set the size
            fileEntry.Length = Convert.ToInt64(jh.GetProperty("bytes"));

            // set the modified time
            fileEntry.Modified = jh.GetDateTimeProperty("modified");

            // build the displayname
            var DropBoxPath = jh.GetProperty("path");
            var arr = DropBoxPath.Split('/');
            fileEntry.Name = arr.Length > 0 ? arr[arr.Length - 1] : DropBoxPath;

            if (DropBoxPath.Equals("/"))
            {
                fileEntry.Id = "/";
                fileEntry.ParentID = null;
            }
            else
            {
                fileEntry.Id = DropBoxPath.Trim('/');
                fileEntry.ParentID = DropBoxResourceIDHelpers.GetParentID(DropBoxPath);
            }


            // set the hash property if possible
            var hashValue = jh.GetProperty("hash");
            if (hashValue.Length > 0)
                fileEntry.SetPropertyValue("hash", hashValue);

            // set the path property            
            fileEntry.SetPropertyValue("path", DropBoxPath.Equals("/") ? "" : DropBoxPath);

            // set the revision value if possible
            var revValue = jh.GetProperty("rev");
            if (revValue.Length > 0)
            {
                fileEntry.SetPropertyValue("rev", revValue);
            }

            // go ahead
            return true;
        }

        private static Boolean BuildDirectyEntry(BaseDirectoryEntry dirEntry, JsonHelper jh, IStorageProviderService service, IStorageProviderSession session)
        {
            // build the file entry part 
            if (!BuildFileEntry(dirEntry, jh))
                return false;

            // now take the content 
            var content = jh.GetListProperty("contents");

            if (content.Count == 0)
                return true;

            // remove all childs
            dirEntry.ClearChilds();

            // add the childs
            foreach (var jsonContent in content)
            {
                // parse the item
                var jc = new JsonHelper();
                if (!jc.ParseJsonMessage(jsonContent))
                    continue;

                // check if we have a directory
                var isDir = jc.GetBooleanProperty("is_dir");

                BaseFileEntry fentry;

                if (isDir)
                {
                    fentry = new BaseDirectoryEntry("Name", 0, DateTime.Now, service, session);
                }
                else
                {
                    fentry = new BaseFileEntry("Name", 0, DateTime.Now, service, session);
                }

                // build the file attributes
                BuildFileEntry(fentry, jc);

                // establish parent child realtionship
                dirEntry.AddChild(fentry);
            }

            // set the length
            dirEntry.Length = dirEntry.Count;

            // go ahead
            return true;
        }
    }
}