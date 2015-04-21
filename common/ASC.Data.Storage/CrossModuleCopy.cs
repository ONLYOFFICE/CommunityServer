/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

namespace ASC.Data.Storage
{
    ///<summary>
    /// Helper for copying from one module to another
    ///</summary>
    public static class CrossModuleCopy
    {
        public static Uri CrossCopy(IDataStore srcStore, string srcFilename, IDataStore dstStore, string dstFilename)
        {
            return CrossCopy(srcStore, string.Empty, srcFilename, dstStore, string.Empty, dstFilename);
        }

        ///<summary>
        /// Copy from one module to another. Can copy from s3 to disk and vice versa
        ///</summary>
        ///<param name="srcStore"></param>
        ///<param name="srcDomain"></param>
        ///<param name="srcFilename"></param>
        ///<param name="dstStore"></param>
        ///<param name="dstDomain"></param>
        ///<param name="dstFilename"></param>
        ///<returns></returns>
        ///<exception cref="ArgumentNullException"></exception>
        public static Uri CrossCopy(IDataStore srcStore, string srcDomain, string srcFilename, IDataStore dstStore,
                                    string dstDomain, string dstFilename)
        {
            if (srcStore == null) throw new ArgumentNullException("srcStore");
            if (srcDomain == null) throw new ArgumentNullException("srcDomain");
            if (srcFilename == null) throw new ArgumentNullException("srcFilename");
            if (dstStore == null) throw new ArgumentNullException("dstStore");
            if (dstDomain == null) throw new ArgumentNullException("dstDomain");
            if (dstFilename == null) throw new ArgumentNullException("dstFilename");
            //Read contents
            using (Stream srcStream = srcStore.GetReadStream(srcDomain, srcFilename))
            {
                using (var memoryStream = TempStream.Create())
                {
                    //Copy
                    var buffer = new byte[4096];
                    int readed;
                    while ((readed = srcStream.Read(buffer, 0, 4096)) != 0)
                    {
                        memoryStream.Write(buffer, 0, readed);
                    }

                    memoryStream.Position = 0;
                    return dstStore.Save(dstDomain, dstFilename, memoryStream);
                }
            }
        }
    }
}