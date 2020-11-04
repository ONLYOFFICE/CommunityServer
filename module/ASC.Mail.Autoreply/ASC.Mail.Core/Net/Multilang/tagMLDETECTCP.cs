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
    public enum MLDETECTCP {
        // Default setting will be used. 
        MLDETECTCP_NONE = 0,

        // Input stream consists of 7-bit data. 
        MLDETECTCP_7BIT = 1,

        // Input stream consists of 8-bit data. 
        MLDETECTCP_8BIT = 2,

        // Input stream consists of double-byte data. 
        MLDETECTCP_DBCS = 4,

        // Input stream is an HTML page. 
        MLDETECTCP_HTML = 8,

        //Not currently supported. 
        MLDETECTCP_NUMBER = 16
    } 
}
