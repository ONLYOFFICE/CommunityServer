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