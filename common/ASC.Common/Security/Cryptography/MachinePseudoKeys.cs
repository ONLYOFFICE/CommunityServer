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
            var key = ConfigurationManager.AppSettings["core.machinekey"];
            if (string.IsNullOrEmpty(key))
            {
                key = ConfigurationManager.AppSettings["asc.common.machinekey"];
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
            var cnst = Enumerable.Repeat<byte>(0, sizeof (int)).Concat(GetMachineConstant()).ToArray();
            var icnst = BitConverter.ToInt32(cnst, cnst.Length - sizeof (int));
            var rnd = new AscRandom(icnst);
            var buff = new byte[bytesCount];
            rnd.NextBytes(buff);
            return buff;
        }
    }
}