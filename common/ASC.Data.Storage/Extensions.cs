/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Threading;
using ASC.Data.Storage;

public static class Extensions
{
    private const int BufferSize = 2048;//NOTE: set to 2048 to fit in minimum tcp window

    public static Stream IronReadStream(this IDataStore store, string domain, string path, int tryCount)
    {
        var ms = TempStream.Create();
        IronReadToStream(store, domain, path, tryCount, ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static void IronReadToStream(this IDataStore store, string domain, string path, int tryCount, Stream readTo)
    {
        if (tryCount < 1) throw new ArgumentOutOfRangeException("tryCount", "Must be greater or equal 1.");
        if (!readTo.CanWrite) throw new ArgumentException("stream cannot be written", "readTo");

        var tryCurrent = 0;
        var offset = 0;

        while (tryCurrent < tryCount)
        {
            try
            {
                tryCurrent++;
                using (var stream = store.GetReadStream(domain, path, offset))
                {
                    var buffer = new byte[BufferSize];
                    var readed = 0;
                    while ((readed = stream.Read(buffer, 0, BufferSize)) > 0)
                    {
                        readTo.Write(buffer, 0, readed);
                        offset += readed;
                    }
                }
                break;
            }
            catch (Exception ex)
            {
                if (tryCurrent >= tryCount)
                {
                    throw new IOException("Can not read stream. Tries count: " + tryCurrent + ".", ex);
                }
                Thread.Sleep(tryCount * 50);
            }
        }
    }
}