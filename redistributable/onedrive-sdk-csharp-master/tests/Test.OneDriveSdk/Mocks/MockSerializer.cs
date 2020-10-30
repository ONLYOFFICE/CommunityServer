// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Test.OneDrive.Sdk.Mocks
{
    using Microsoft.Graph;
    using Moq;

    public class MockSerializer : Mock<ISerializer>
    {
        public MockSerializer()
            : base(MockBehavior.Strict)
        {
        }
    }
}
