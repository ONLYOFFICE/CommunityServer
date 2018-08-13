/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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