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
using ASC.Data.Storage;

public static class StreamExtension
{
    private const int BufferSize = 2048; //NOTE: set to 2048 to fit in minimum tcp window

    public static Stream GetBuffered(this Stream srcStream)
    {
        if (srcStream == null) throw new ArgumentNullException("srcStream");
        if (!srcStream.CanSeek || srcStream.CanTimeout)
        {
            //Buffer it
            var memStream = TempStream.Create();
            srcStream.StreamCopyTo(memStream);
            memStream.Position = 0;
            return memStream;
        }
        return srcStream;
    }

    public static byte[] GetCorrectBuffer(this Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException("stream");
        }

        using (var mem = stream.GetBuffered())
        {
            var buffer = new byte[mem.Length];
            mem.Position = 0;
            mem.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }

    public static void StreamCopyTo(this Stream srcStream, Stream dstStream)
    {
        if (srcStream == null) throw new ArgumentNullException("srcStream");
        if (dstStream == null) throw new ArgumentNullException("dstStream");

        var buffer = new byte[BufferSize];
        int readed;
        while ((readed = srcStream.Read(buffer, 0, BufferSize)) > 0)
        {
            dstStream.Write(buffer, 0, readed);
        }
    }

    public static void StreamCopyTo(this Stream srcStream, Stream dstStream, int length)
    {
        if (srcStream == null) throw new ArgumentNullException("srcStream");
        if (dstStream == null) throw new ArgumentNullException("dstStream");

        var buffer = new byte[BufferSize];
        int totalRead = 0;
        int readed;
        while ((readed = srcStream.Read(buffer, 0, length - totalRead > BufferSize ? BufferSize : length - totalRead)) > 0 && totalRead < length)
        {
            dstStream.Write(buffer, 0, readed);
            totalRead += readed;
        }
    }
}
