/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Diagnostics;
using ASC.Thrdparty.Configuration;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    public class AuthService
    {
        public string Name { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string Instruction { get; private set; }

        public bool CanSet = false;

        public int? Order;

        public AuthKey Key { get; private set; }

        public AuthKey Secret { get; private set; }

        public AuthKey KeyDefault { get; private set; }

        public AuthKey SecretDefault { get; private set; }

        public AuthService(string name)
        {
            Name = name;
            Title = GetResourceString(name) ?? name;
            Description = GetResourceString(name + "Description");
            Instruction = GetResourceString(name + "Instruction");
        }

        public AuthService WithKey(string keyName, string keyValue)
        {
            if (keyName != null)
                Key = new AuthKey { Name = keyName, Value = keyValue, Title = GetResourceString(Name + "Key") ?? keyName };
            return this;
        }

        public AuthService WithSecret(string keyName, string keyValue)
        {
            if (keyName != null)
                Secret = new AuthKey { Name = keyName, Value = keyValue, Title = GetResourceString(Name + "Secret") ?? keyName };
            return this;
        }

        public AuthService WithKeyDefault(string keyName, string keyValue)
        {
            if (keyName != null)
                KeyDefault = new AuthKey { Name = keyName, Value = keyValue, Title = GetResourceString(Name + "KeyDefault") ?? keyName };
            return this;
        }

        public AuthService WithSecretDefault(string keyName, string keyValue)
        {
            if (keyName != null)
                SecretDefault = new AuthKey { Name = keyName, Value = keyValue, Title = GetResourceString(Name + "SecretDefault") ?? keyName };
            return this;
        }

        private static string GetResourceString(string resourceKey)
        {
            try
            {
                return Resource.ResourceManager.GetString("Consumers" + resourceKey);
            }
            catch
            {
                return null;
            }
        }
    }

    [DebuggerDisplay("({Name},{Value})")]
    public class AuthKey : KeyElement
    {
        public string Title { get; set; }
    }
}