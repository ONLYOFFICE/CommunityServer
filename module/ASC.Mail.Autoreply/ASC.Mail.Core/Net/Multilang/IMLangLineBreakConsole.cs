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


namespace MultiLanguage
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType((short) 1), Guid("F5BE2EE1-BFD7-11D0-B188-00AA0038C969")]
    public interface IMLangLineBreakConsole
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void BreakLineML([In, MarshalAs(UnmanagedType.Interface)] CMLangString pSrcMLStr, [In] int lSrcPos, [In] int lSrcLen, [In] int cMinColumns, [In] int cMaxColumns, out int plLineLen, out int plSkipLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void BreakLineW([In] uint locale, [In] ref ushort pszSrc, [In] int cchSrc, [In] int cMaxColumns, out int pcchLine, out int pcchSkip);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void BreakLineA([In] uint locale, [In] uint uCodePage, [In] ref sbyte pszSrc, [In] int cchSrc, [In] int cMaxColumns, out int pcchLine, out int pcchSkip);
    }
}
