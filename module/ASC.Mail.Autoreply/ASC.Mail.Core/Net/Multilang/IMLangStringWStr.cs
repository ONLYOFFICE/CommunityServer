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


#pragma warning disable 0108

namespace MultiLanguage
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType((short) 1), ComConversionLoss, Guid("C04D65D0-B70D-11D0-B188-00AA0038C969")]
    public interface IMLangStringWStr : IMLangString
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Sync([In] int fNoAccess);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetLength();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetMLStr([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pSrcMLStr, [In] int lSrcPos, [In] int lSrcLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetMLStr([In] int lSrcPos, [In] int lSrcLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] uint dwClsContext, [In] ref Guid piid, [MarshalAs(UnmanagedType.IUnknown)] out object ppDestMLStr, out int plDestPos, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetWStr([In] int lDestPos, [In] int lDestLen, [In] ref ushort pszSrc, [In] int cchSrc, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetStrBufW([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.Interface)] IMLangStringBufW pSrcBuf, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetWStr([In] int lSrcPos, [In] int lSrcLen, out ushort pszDest, [In] int cchDest, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetStrBufW([In] int lSrcPos, [In] int lSrcMaxLen, [MarshalAs(UnmanagedType.Interface)] out IMLangStringBufW ppDestBuf, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void LockWStr([In] int lSrcPos, [In] int lSrcLen, [In] int lFlags, [In] int cchRequest, [Out] IntPtr ppszDest, out int pcchDest, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void UnlockWStr([In] ref ushort pszSrc, [In] int cchSrc, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetLocale([In] int lDestPos, [In] int lDestLen, [In] uint locale);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLocale([In] int lSrcPos, [In] int lSrcMaxLen, out uint plocale, out int plLocalePos, out int plLocaleLen);
    }
}

#pragma warning restore 0108