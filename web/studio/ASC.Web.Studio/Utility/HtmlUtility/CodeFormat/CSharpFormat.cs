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


#region Copyright � 2001-2003 Jean-Claude Manoli [jc@manoli.net]
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author(s) be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *   1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 * 
 *   2. Altered source versions must be plainly marked as such, and must not
 *      be misrepresented as being the original software.
 * 
 *   3. This notice may not be removed or altered from any source distribution.
 */
#endregion

namespace ASC.Web.Studio.Utility.HtmlUtility.CodeFormat
{
    /// <summary>
    /// Generates color-coded HTML 4.01 from C# source code.
    /// </summary>
    internal class CSharpFormat : CLikeFormat
    {
        /// <summary>
        /// The list of C# keywords.
        /// </summary>
        protected override string Keywords
        {
            get
            {
                return "abstract add alias as base bool break byte case catch char "
                       + "checked class const continue decimal default delegate do double else "
                       + "enum event explicit extern false finally fixed float for foreach goto "
                       + "get global if implicit in int interface internal is lock long namespace new null "
                       + "object operator out override partial params private protected public readonly "
                       + "ref remove return sbyte sealed set short sizeof stackalloc static string struct "
                       + "switch this throw true try typeof uint ulong unchecked unsafe ushort "
                       + "var using value virtual void volatile where while yield";
            }
        }

        /// <summary>
        /// The list of C# preprocessors.
        /// </summary>
        protected override string Preprocessors
        {
            get
            {
                return "#if #else #elif #endif #define #undef #warning "
                       + "#error #line #region #endregion #pragma";
            }
        }
    }
}