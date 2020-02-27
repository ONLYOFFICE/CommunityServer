/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


namespace MultiLanguage
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("275C23E1-3747-11D0-9FEA-00AA003F8646"), InterfaceType((short) 1)]
    public interface IMultiLanguage
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberOfCodePageInfo(out uint pcCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCodePageInfo([In] uint uiCodePage, out tagMIMECPINFO pCodePageInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFamilyCodePage([In] uint uiCodePage, out uint puiFamilyCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumCodePages([In] uint grfFlags, [MarshalAs(UnmanagedType.Interface)] out IEnumCodePage ppEnumCodePage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCharsetInfo([In, MarshalAs(UnmanagedType.BStr)] string Charset, out tagMIMECSETINFO pCharsetInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsConvertible([In] uint dwSrcEncoding, [In] uint dwDstEncoding);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConvertString([In, Out] ref uint pdwMode, [In] uint dwSrcEncoding, [In] uint dwDstEncoding, [In] ref byte pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref byte pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConvertStringToUnicode([In, Out] ref uint pdwMode, [In] uint dwEncoding, [In] ref sbyte pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref ushort pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConvertStringFromUnicode([In, Out] ref uint pdwMode, [In] uint dwEncoding, [In] ref ushort pSrcStr, [In, Out] ref uint pcSrcSize, [In] ref sbyte pDstStr, [In, Out] ref uint pcDstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConvertStringReset();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRfc1766FromLcid([In] uint locale, [MarshalAs(UnmanagedType.BStr)] out string pbstrRfc1766);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLcidFromRfc1766(out uint plocale, [In, MarshalAs(UnmanagedType.BStr)] string bstrRfc1766);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumRfc1766([MarshalAs(UnmanagedType.Interface)] out IEnumRfc1766 ppEnumRfc1766);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRfc1766Info([In] uint locale, out tagRFC1766INFO pRfc1766Info);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateConvertCharset([In] uint uiSrcCodePage, [In] uint uiDstCodePage, [In] uint dwProperty, [MarshalAs(UnmanagedType.Interface)] out CMLangConvertCharset ppMLangConvertCharset);
    }
}
