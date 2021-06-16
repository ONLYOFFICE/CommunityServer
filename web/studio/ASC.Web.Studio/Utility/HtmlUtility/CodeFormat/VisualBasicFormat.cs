/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
    /// Generates color-coded HTML 4.01 from Visual Basic source code.
    /// </summary>
    internal class VisualBasicFormat : CodeFormat
    {
        /// <summary>
        /// Determines if the language is case sensitive.
        /// </summary>
        /// <value>Always <b>true</b>, since VB is not case sensitive.</value>
        public override bool CaseSensitive
        {
            get { return false; }
        }

        /// <summary>
        /// Regular expression string to match comments (' and REM). 
        /// </summary>
        protected override string CommentRegEx
        {
            get { return @"(?:'|REM\s).*?(?=\r|\n)"; }
        }

        /// <summary>
        /// Regular expression string to match string and character literals. 
        /// </summary>
        protected override string StringRegEx
        {
            get { return @"""""|"".*?"""; }
        }

        /// <summary>
        /// The list of VB keywords.
        /// </summary>
        protected override string Keywords
        {
            get
            {
                return "AddHandler AddressOf AndAlso Alias And Ansi As Assembly "
                       + "Auto Boolean ByRef Byte ByVal Call Case Catch "
                       + "CBool CByte CChar CDate CDec CDbl Char CInt "
                       + "Class CLng CObj Const CShort CSng CStr CType "
                       + "Date Decimal Declare Default Delegate Dim DirectCast Do "
                       + "Double Each Else ElseIf End EndIf Enum Erase Error "
                       + "Event Exit False Finally For Friend Function Get "
                       + "GetType Global GoSub GoTo Handles If Implements Imports In Inherits "
                       + "Integer Interface Is Let Lib Like Long Loop "
                       + "Me Mod Module MustInherit MustOverride MyBase MyClass Namespace "
                       + "Narrowing New Next Not Nothing NotInheritable NotOverridable Object On "
                       + "Operator Option Optional Or OrElse Overloads Overridable Overrides ParamArray "
                       + "Preserve Private Property Protected Public RaiseEvent ReadOnly ReDim "
                       + "REM RemoveHandler Resume Return Select Set Shadows Shared "
                       + "Short Single Static Step Stop String Structure Sub "
                       + "SyncLock Then Throw To True Try TryCast TypeOf Unicode "
                       + "Until Variant Wend When While With Widening WithEvents WriteOnly Xor";
            }
        }

        /// <summary>
        /// The list of VB preprocessors.
        /// </summary>
        protected override string Preprocessors
        {
            get
            {
                return @"#\s*Const #\s*If #\s*Else #\s*ElseIf #\s*End\s+If "
                       + @"#\s*ExternalSource #\s*End\s+ExternalSource "
                       + @"#\s*Region #\s*End\s+Region";
            }
        }
    }
}