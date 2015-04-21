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
using System.Web;

namespace ASC.Web.Studio.Controls.FileUploader.HttpModule
{
    internal class EntityBodyInspector
    {
        private EntityBodyChunkStateWaiter _current;
        private string _lastCdName;
        private readonly EntityBodyChunkStateWaiter _boundaryWaiter;
        private readonly EntityBodyChunkStateWaiter _boundaryInfoWaiter;
        private readonly EntityBodyChunkStateWaiter _formValueWaiter;
        private readonly UploadProgressStatistic _statistic;

        internal EntityBodyInspector(HttpUploadWorkerRequest request)
        {
            _statistic = new UploadProgressStatistic
                {
                    TotalBytes = request.GetTotalEntityBodyLength()
                };

            var contentType = request.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentType);

            var boundary = string.Format("--{0}\r\n", UploadProgressUtils.GetBoundary(contentType));

            _boundaryWaiter = new EntityBodyChunkStateWaiter(boundary, false);
            _boundaryWaiter.MeetGuard += BoundaryWaiterMeetGuard;
            _current = _boundaryWaiter;

            _boundaryInfoWaiter = new EntityBodyChunkStateWaiter("\r\n\r\n", true);
            _boundaryInfoWaiter.MeetGuard += BoundaryInfoWaiterMeetGuard;

            _formValueWaiter = new EntityBodyChunkStateWaiter("\r\n", true);
            _formValueWaiter.MeetGuard += FormValueWaiterMeetGuard;

            _lastCdName = string.Empty;
        }

        internal void EndRequest()
        {
            _statistic.EndUpload();
        }

        internal void Inspect(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
                return;

            _statistic.AddUploadedBytes(buffer.Length);
            Inspect(buffer, offset);
        }

        private void Inspect(byte[] buffer, int offset)
        {
            if (buffer == null)
                return;

            _current.Wait(buffer, offset);
        }

        private void BoundaryWaiterMeetGuard(object sender, EventArgs e)
        {
            var sw = sender as EntityBodyChunkStateWaiter;
            sw.Reset();
            _current = _boundaryInfoWaiter;
            _current.Wait(sw);
        }

        private void BoundaryInfoWaiterMeetGuard(object sender, EventArgs e)
        {
            var sw = sender as EntityBodyChunkStateWaiter;
            var cdi = UploadProgressUtils.GetContentDisposition(sw.Value);
            sw.Reset();
            if (!cdi.IsFile)
            {
                _lastCdName = cdi.name;
                _current = _formValueWaiter;
                _current.Wait(sw);
            }
            else
            {
                _statistic.BeginFileUpload(cdi.filename);
                _current = _boundaryWaiter;
                _current.Wait(sw);
            }
        }

        private void FormValueWaiterMeetGuard(object sender, EventArgs e)
        {
            var sw = sender as EntityBodyChunkStateWaiter;

            var fieldValue = sw.Value;
            _statistic.AddFormField(_lastCdName, fieldValue);

            sw.Reset();

            _current = _boundaryWaiter;
            _current.Wait();
        }
    }
}