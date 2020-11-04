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

    [ComImport, ClassInterface((short) 0), TypeLibType((short) 2), Guid("D66D6F99-CDAA-11D0-B822-00C04FC9B31F")]
    public class CMLangConvertCharsetClass : IMLangConvertCharset, CMLangConvertCharset
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void DoConversion([In] ref byte pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref byte pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void DoConversionFromUnicode([In] ref ushort pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref sbyte pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void DoConversionToUnicode([In] ref sbyte pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref ushort pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetDestinationCodePage(out uint puiDstCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetProperty(out uint pdwProperty);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetSourceCodePage(out uint puiSrcCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void Initialize([In] uint uiSrcCodePage, [In] uint uiDstCodePage, [In] uint dwProperty);
    }
}
