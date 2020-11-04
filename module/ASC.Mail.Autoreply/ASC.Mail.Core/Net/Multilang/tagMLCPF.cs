/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Text;

namespace MultiLanguage
{
    [Flags]
    public enum MLCPF
    {
        // Not currently supported.
        MLDETECTF_MAILNEWS = 0x0001,

        // Not currently supported.
        MLDETECTF_BROWSER = 0x0002,
        
        // Detection result must be valid for conversion and text rendering.
        MLDETECTF_VALID = 0x0004,
        
        // Detection result must be valid for conversion.
        MLDETECTF_VALID_NLS = 0x0008,

        //Preserve preferred code page order. 
        //This is meaningful only if you have set the puiPreferredCodePages parameter in IMultiLanguage3::DetectOutboundCodePage or IMultiLanguage3::DetectOutboundCodePageInIStream.
        MLDETECTF_PRESERVE_ORDER = 0x0010,

        // Only return one of the preferred code pages as the detection result. 
        // This is meaningful only if you have set the puiPreferredCodePages parameter in IMultiLanguage3::DetectOutboundCodePage or IMultiLanguage3::DetectOutboundCodePageInIStream.
        MLDETECTF_PREFERRED_ONLY = 0x0020,

        // Filter out graphical symbols and punctuation.
        MLDETECTF_FILTER_SPECIALCHAR = 0x0040,
        
        // Return only Unicode codepages if the euro character is detected. 
        MLDETECTF_EURO_UTF8 = 0x0080
    }    

}
