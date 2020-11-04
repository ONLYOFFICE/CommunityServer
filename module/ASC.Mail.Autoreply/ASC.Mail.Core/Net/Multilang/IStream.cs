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

    [ComImport, Guid("0000000C-0000-0000-C000-000000000046"), InterfaceType((short) 1)]
    public interface IStream : ISequentialStream
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteRead(IntPtr pv, uint cb, ref uint pcbRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteWrite([In] ref byte pv, [In] uint cb, ref uint pcbWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteSeek([In] _LARGE_INTEGER dlibMove, [In] uint dwOrigin, IntPtr plibNewPosition);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSize([In] _ULARGE_INTEGER libNewSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteCopyTo([In, MarshalAs(UnmanagedType.Interface)] IStream pstm, [In] _ULARGE_INTEGER cb, out _ULARGE_INTEGER pcbRead, out _ULARGE_INTEGER pcbWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Commit([In] uint grfCommitFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Revert();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void LockRegion([In] _ULARGE_INTEGER libOffset, [In] _ULARGE_INTEGER cb, [In] uint dwLockType);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void UnlockRegion([In] _ULARGE_INTEGER libOffset, [In] _ULARGE_INTEGER cb, [In] uint dwLockType);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Stat(out tagSTATSTG pstatstg, [In] uint grfStatFlag);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Clone([MarshalAs(UnmanagedType.Interface)] out IStream ppstm);
    }
}

#pragma warning restore 0108