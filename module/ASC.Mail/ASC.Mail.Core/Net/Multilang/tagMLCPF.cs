/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
