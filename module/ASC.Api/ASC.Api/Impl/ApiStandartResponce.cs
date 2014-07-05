/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

#region usings

using System.Runtime.Serialization;
using ASC.Api.Enums;
using ASC.Api.Interfaces;

#endregion

namespace ASC.Api.Impl
{
    [DataContract(Name = "result", Namespace = "")]
    internal class ApiStandartResponce : IApiStandartResponce
    {
        #region IApiStandartResponce Members

        [DataMember(Name = "response", EmitDefaultValue = false, Order = 200)]
        public object Response { get; set; }

        [DataMember(Name = "error", EmitDefaultValue = false, Order = 210)]
        public ErrorWrapper Error { get; set; }

        [DataMember(Name = "status", EmitDefaultValue = true, Order = 100)]
        public ApiStatus Status { get; set; }

        [DataMember(Name = "statusCode", EmitDefaultValue = false, Order = 101)]
        public long Code { get; set; }

        [DataMember(Name = "count", EmitDefaultValue = false, Order = 10)]
        public long Count { get; set; }

        [DataMember(Name = "startIndex", EmitDefaultValue = false, Order = 11)]
        public long StartIndex { get; set; }

        [DataMember(Name = "nextIndex", EmitDefaultValue = false, Order = 12)]
        public long? NextPage { get; set; }

        [DataMember(Name = "total", EmitDefaultValue = false, Order = 13)]
        public long? TotalCount { get; set; }


        public ApiContext ApiContext { get; set; }
        #endregion
    }
}