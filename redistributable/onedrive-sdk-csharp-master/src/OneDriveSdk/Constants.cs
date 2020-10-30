// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    public static class Constants
    {
        public const int PollingIntervalInMs = 5000;

        public static class Headers
        {
            public const string SdkVersionHeaderPrefix = "onedrive";
        }

        public static class Url
        {
            public const string Drive = "drive";

            public const string Root = "root";

            public const string AppRoot = "approot";
            
            public const string Documents = "documents";
            
            public const string Photos = "photos";
            
            public const string CameraRoll = "cameraroll";
            
            public const string Music = "music";
        }
    }
}
