// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Helpers;

namespace Test.OneDrive.Sdk.Mocks
{
    public class TestChunkedUploadProvider : ChunkedUploadProvider
    {
        public TestChunkedUploadProvider(UploadSession session, IBaseClient client, Stream uploadStream, int maxChunkSize = -1)
            :base(session, client, uploadStream, maxChunkSize)
        { }

        public List<Tuple<long, long>> GetRangesRemainingProxy(UploadSession session)
        {
            return this.GetRangesRemaining(session);
        }
    }
}
