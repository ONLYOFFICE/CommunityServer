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


using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

using ASC.Common.Security;

namespace ASC.Security.Cryptography
{
    public static class MachinePseudoKeys
    {
        private static readonly byte[] confkey = null;


        static MachinePseudoKeys()
        {
            var key = ConfigurationManagerExtension.AppSettings["core.machinekey"];

            if (string.IsNullOrEmpty(key))
            {
                key = ConfigurationManagerExtension.AppSettings["asc.common.machinekey"];
            }
            if (!string.IsNullOrEmpty(key))
            {
                confkey = Encoding.UTF8.GetBytes(key);
            }
        }


        public static byte[] GetMachineConstant()
        {
            if (confkey != null)
            {
                return confkey;
            }

            var path = typeof(MachinePseudoKeys).Assembly.Location;
            var fi = new FileInfo(path);
            return BitConverter.GetBytes(fi.CreationTime.ToOADate());
        }

        public static byte[] GetMachineConstant(int bytesCount)
        {
            var cnst = Enumerable.Repeat<byte>(0, sizeof(int)).Concat(GetMachineConstant()).ToArray();
            var icnst = BitConverter.ToInt32(cnst, cnst.Length - sizeof(int));
            var rnd = new AscRandom(icnst);
            var buff = new byte[bytesCount];
            rnd.NextBytes(buff);
            return buff;
        }
    }
}