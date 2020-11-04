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


#pragma warning disable 0108

namespace MultiLanguage
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType((short) 1), ComConversionLoss, Guid("359F3441-BD4A-11D0-B188-00AA0038C969")]
    public interface IMLangFontLink : IMLangCodePages
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCharCodePages([In] ushort chSrc, out uint pdwCodePages);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetStrCodePages([In] ref ushort pszSrc, [In] int cchSrc, [In] uint dwPriorityCodePages, out uint pdwCodePages, out int pcchCodePages);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CodePageToCodePages([In] uint uCodePage, out uint pdwCodePages);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CodePagesToCodePage([In] uint dwCodePages, [In] uint uDefaultCodePage, out uint puCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFontCodePages([In, ComAliasName("MultiLanguage.wireHDC")] ref _RemotableHandle hDC, [In, ComAliasName("MultiLanguage.wireHFONT")] ref _RemotableHandle hFont, out uint pdwCodePages);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void MapFont([In, ComAliasName("MultiLanguage.wireHDC")] ref _RemotableHandle hDC, [In] uint dwCodePages, [In, ComAliasName("MultiLanguage.wireHFONT")] ref _RemotableHandle hSrcFont, [Out, ComAliasName("MultiLanguage.wireHFONT")] IntPtr phDestFont);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReleaseFont([In, ComAliasName("MultiLanguage.wireHFONT")] ref _RemotableHandle hFont);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ResetFontMapping();
    }
}
