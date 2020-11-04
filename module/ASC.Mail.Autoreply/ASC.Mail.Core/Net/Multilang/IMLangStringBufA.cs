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

    [ComImport, Guid("D24ACD23-BA72-11D0-B188-00AA0038C969"), InterfaceType((short) 1), ComConversionLoss]
    public interface IMLangStringBufA
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetStatus(out int plFlags, out int pcchBuf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void LockBuf([In] int cchOffset, [In] int cchMaxLock, [Out] IntPtr ppszBuf, out int pcchBuf);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void UnlockBuf([In] ref sbyte pszBuf, [In] int cchOffset, [In] int cchWrite);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Insert([In] int cchOffset, [In] int cchMaxInsert, out int pcchActual);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Delete([In] int cchOffset, [In] int cchDelete);
    }
}
