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
using System.Text;

namespace ASC.Web.Studio.Controls.FileUploader.HttpModule
{
    internal class EntityBodyChunkStateWaiter
    {
        private readonly bool _collectInput;
        private StringBuilder _input;
        private EntityBodyChunkReader _reader;

        private readonly int _guardSize;
        private readonly char[] _guard;

        internal event EventHandler<EventArgs> MeetGuard;

        internal EntityBodyChunkStateWaiter(string waitFor, bool collectInput)
        {
            _collectInput = collectInput;

            if (collectInput) _input = new StringBuilder();

            _guardSize = waitFor.Length;
            _guard = waitFor.ToCharArray();

            CharFound = 0;
        }

        internal string Value
        {
            get { return !_collectInput ? string.Empty : _input.ToString(); }
        }

        public int Index
        {
            get { return _reader.Index; }
        }

        public int CharFound { get; private set; }

        internal void Reset()
        {
            _input = new StringBuilder();
            CharFound = 0;
        }

        internal void Wait(byte[] buffer, int offset)
        {
            _reader = new EntityBodyChunkReader(buffer, offset);
            Wait();
        }

        internal void Wait(EntityBodyChunkStateWaiter waiter)
        {
            _reader = waiter._reader;
            Wait();
        }

        internal void Wait()
        {
            while (_reader.Read())
            {
                var c = _reader.Current;

                if (_collectInput)
                    _input.Append(c);

                if (c != _guard[CharFound])
                {
                    CharFound = 0;
                }
                else
                {
                    CharFound++;
                    if (CharFound == _guard.Length)
                    {
                        if (MeetGuard != null)
                        {
                            if (_collectInput && _input.Length >= _guardSize)
                            {
                                _input.Remove(_input.Length - _guardSize, _guardSize);
                            }
                            MeetGuard(this, new EventArgs());
                        }
                        break;
                    }
                }
            }
        }
    }
}