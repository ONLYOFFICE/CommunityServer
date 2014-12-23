/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace MultiLanguage
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("C04D65CF-B70D-11D0-B188-00AA0038C969"), ComConversionLoss, ClassInterface((short) 0), TypeLibType((short) 2)]
    public class CMLangStringClass : IMLangString, CMLangString, IMLangStringWStr, IMLangStringAStr
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetAStr([In] int lSrcPos, [In] int lSrcLen, [In] uint uCodePageIn, out uint puCodePageOut, out sbyte pszDest, [In] int cchDest, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern int GetLength();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetLocale([In] int lSrcPos, [In] int lSrcMaxLen, out uint plocale, out int plLocalePos, out int plLocaleLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetMLStr([In] int lSrcPos, [In] int lSrcLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] uint dwClsContext, [In] ref Guid piid, [MarshalAs(UnmanagedType.IUnknown)] out object ppDestMLStr, out int plDestPos, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetStrBufA([In] int lSrcPos, [In] int lSrcMaxLen, out uint puDestCodePage, [MarshalAs(UnmanagedType.Interface)] out IMLangStringBufA ppDestBuf, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetStrBufW([In] int lSrcPos, [In] int lSrcMaxLen, [MarshalAs(UnmanagedType.Interface)] out IMLangStringBufW ppDestBuf, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void GetWStr([In] int lSrcPos, [In] int lSrcLen, out ushort pszDest, [In] int cchDest, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern int IMLangStringAStr_GetLength();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void IMLangStringAStr_GetLocale([In] int lSrcPos, [In] int lSrcMaxLen, out uint plocale, out int plLocalePos, out int plLocaleLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void IMLangStringAStr_GetMLStr([In] int lSrcPos, [In] int lSrcLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] uint dwClsContext, [In] ref Guid piid, [MarshalAs(UnmanagedType.IUnknown)] out object ppDestMLStr, out int plDestPos, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void IMLangStringAStr_SetLocale([In] int lDestPos, [In] int lDestLen, [In] uint locale);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void IMLangStringAStr_SetMLStr([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pSrcMLStr, [In] int lSrcPos, [In] int lSrcLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void IMLangStringAStr_Sync([In] int fNoAccess);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern int IMLangStringWStr_GetLength();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void IMLangStringWStr_GetMLStr([In] int lSrcPos, [In] int lSrcLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] uint dwClsContext, [In] ref Guid piid, [MarshalAs(UnmanagedType.IUnknown)] out object ppDestMLStr, out int plDestPos, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void IMLangStringWStr_SetMLStr([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pSrcMLStr, [In] int lSrcPos, [In] int lSrcLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void IMLangStringWStr_Sync([In] int fNoAccess);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void LockAStr([In] int lSrcPos, [In] int lSrcLen, [In] int lFlags, [In] uint uCodePageIn, [In] int cchRequest, out uint puCodePageOut, [Out] IntPtr ppszDest, out int pcchDest, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void LockWStr([In] int lSrcPos, [In] int lSrcLen, [In] int lFlags, [In] int cchRequest, [Out] IntPtr ppszDest, out int pcchDest, out int plDestLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void SetAStr([In] int lDestPos, [In] int lDestLen, [In] uint uCodePage, [In] ref sbyte pszSrc, [In] int cchSrc, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void SetLocale([In] int lDestPos, [In] int lDestLen, [In] uint locale);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void SetMLStr([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pSrcMLStr, [In] int lSrcPos, [In] int lSrcLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void SetStrBufA([In] int lDestPos, [In] int lDestLen, [In] uint uCodePage, [In, MarshalAs(UnmanagedType.Interface)] IMLangStringBufA pSrcBuf, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void SetStrBufW([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.Interface)] IMLangStringBufW pSrcBuf, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void SetWStr([In] int lDestPos, [In] int lDestLen, [In] ref ushort pszSrc, [In] int cchSrc, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void Sync([In] int fNoAccess);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void UnlockAStr([In] ref sbyte pszSrc, [In] int cchSrc, out int pcchActual, out int plActualLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        public virtual extern void UnlockWStr([In] ref ushort pszSrc, [In] int cchSrc, out int pcchActual, out int plActualLen);
    }
}
