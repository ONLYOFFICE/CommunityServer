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


using System.Text;

namespace ASC.Web.Studio.Controls.FileUploader.HttpModule
{
    internal class EntityBodyChunkReader
    {
        internal char Current;
        private readonly int _size;
        private readonly byte[] _buffer;
        private readonly Encoding _encoding = Encoding.UTF8;

        internal EntityBodyChunkReader(byte[] buffer, int offset)
        {
            Index = offset - 1;
            _buffer = buffer;
            _size = buffer.Length;
            Current = '0';
        }

        internal bool MoveTo(int pos)
        {
            if (_size == 0 || (pos >= _size && pos != 0))
                return false;

            Index = pos;
            Current = _encoding.GetChars(_buffer, Index, 1)[0];
            return true;
        }

        internal bool Read()
        {
            return MoveTo(++Index);
        }

        public int Index { get; private set; }
    }
}