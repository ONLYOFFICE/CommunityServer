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