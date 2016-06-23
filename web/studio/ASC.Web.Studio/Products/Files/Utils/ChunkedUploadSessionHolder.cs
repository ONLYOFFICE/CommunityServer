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


using ASC.Files.Core;
using ASC.Web.Files.Classes;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ASC.Web.Files.Utils
{
    static class ChunkedUploadSessionHolder
    {
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(12);

        static ChunkedUploadSessionHolder()
        {
            // clear old sessions
            try
            {
                Global.GetStore(false).DeleteExpired(FileConstant.StorageDomainTmp, "sessions", SlidingExpiration);
            }
            catch (Exception err)
            {
                Global.Logger.Error(err);
            }
        }

        public static void StoreSession(ChunkedUploadSession s)
        {
            using (var stream = Serialize(s))
            {
                Global.GetStore(false).SavePrivate(FileConstant.StorageDomainTmp, Path.Combine("sessions", s.Id + ".session"), stream, s.Expired);
            }
        }

        public static void RemoveSession(ChunkedUploadSession s)
        {
            Global.GetStore(false).Delete(FileConstant.StorageDomainTmp, Path.Combine("sessions", s.Id + ".session"));
        }

        public static ChunkedUploadSession GetSession(string sessionId)
        {
            using (var stream = Global.GetStore(false).GetReadStream(FileConstant.StorageDomainTmp, Path.Combine("sessions", sessionId + ".session")))
            {
                return Deserialize(stream);
            }
        }

        private static Stream Serialize(ChunkedUploadSession s)
        {
            var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, s);
            return stream;
        }

        private static ChunkedUploadSession Deserialize(Stream stream)
        {
            return (ChunkedUploadSession) new BinaryFormatter().Deserialize(stream);
        }
    }
}