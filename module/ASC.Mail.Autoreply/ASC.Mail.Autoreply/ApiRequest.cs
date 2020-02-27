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


using System.Collections.Generic;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Mail.Autoreply.ParameterResolvers;

namespace ASC.Mail.Autoreply
{
    internal class ApiRequest
    {
        public string Method { get; set; }

        public string Url { get; set; }

        public List<RequestParameter> Parameters { get; set; } 

        public Tenant Tenant { get; set; }

        public UserInfo User { get; set; }

        public List<RequestFileInfo> FilesToPost { get; set; }

        public ApiRequest(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Url = url.Trim('/');
            }
        }

        public override string ToString()
        {
            return string.Format("t:{0}; u:{1}; {2} {3}", Tenant.TenantId, User.ID, Method, Url);
        }
    }

    internal class RequestParameter
    {
        public string Name { get; private set; }
        public object Value { get; set; }
        public IParameterResolver ValueResolver { get; private set; }

        public RequestParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public RequestParameter(string name, IParameterResolver valueResolver)
        {
            Name = name;
            ValueResolver = valueResolver;
        }
    }

    internal class RequestFileInfo
    {
        public byte[] Body { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
    }
}