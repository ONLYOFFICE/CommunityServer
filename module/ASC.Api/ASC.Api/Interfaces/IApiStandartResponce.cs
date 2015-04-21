/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


#region usings

using System;
using System.Collections;
using System.Runtime.Serialization;
using ASC.Api.Enums;
using ASC.Api.Impl;

#endregion

namespace ASC.Api.Interfaces
{
    public interface IApiStandartResponce
    {
        object Response { get; set; }
        ErrorWrapper Error { get; set; }
        ApiStatus Status { get; set; }
        long Code { get; set; }
        long Count { get; set; }
        long StartIndex { get; set; }
        long? NextPage { get; set; }
        long? TotalCount { get; set; }
        ApiContext ApiContext { get; set; }
    }

    [DataContract(Name = "error", Namespace = "")]
    public class ErrorWrapper
    {
        public ErrorWrapper()
        {
        }

        public ErrorWrapper(Exception exception)
        {
            //Unwrap
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            Message = exception.Message;
#if (DEBUG)
            Type = exception.GetType().ToString();
            Stack = exception.StackTrace;
#endif

            HResult = exception.HResult;
            Data = exception.Data;

        }

        [DataMember(Name = "message", EmitDefaultValue = false, Order = 2)]
        public string Message { get; set; }

        [DataMember(Name = "type", EmitDefaultValue = false, Order = 3)]
        public string Type { get; set; }

        [DataMember(Name = "stack", EmitDefaultValue = false, Order = 3)]
        public string Stack { get; set; }

        [DataMember(Name = "hresult", EmitDefaultValue = false, Order = 3)]
        public int HResult { get; set; }

        [DataMember(Name = "data", EmitDefaultValue = false, Order = 3)]
        public IDictionary Data { get; set; }
    }
}