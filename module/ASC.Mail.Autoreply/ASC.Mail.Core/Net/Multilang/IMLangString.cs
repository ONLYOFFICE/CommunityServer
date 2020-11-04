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

    [ComImport, Guid("C04D65CE-B70D-11D0-B188-00AA0038C969"), InterfaceType((short) 1)]
    public interface IMLangString
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Sync([In] int fNoAccess);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetLength();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetMLStr([In] int lDestPos, [In] int lDestLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pSrcMLStr, [In] int lSrcPos, [In] int lSrcLen);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetMLStr([In] int lSrcPos, [In] int lSrcLen, [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] uint dwClsContext, [In] ref Guid piid, [MarshalAs(UnmanagedType.IUnknown)] out object ppDestMLStr, out int plDestPos, out int plDestLen);
    }
}
