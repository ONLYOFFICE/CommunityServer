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


using System;
using System.Collections.Generic;
using System.Web;
using ASC.Api.Interfaces;
using ASC.Api.Interfaces.ResponseTypes;
using ASC.Api.Utils;

namespace ASC.Api.Impl.Responders
{
    public class ContentResponder : IApiResponder
    {
        #region IApiResponder Members

        public string Name
        {
            get { return "content"; }
        }

        public IEnumerable<string> GetSupportedExtensions()
        {
            return new string[0];
        }

        public bool CanSerializeType(Type type)
        {
            return false;
        }

        public string Serialize(IApiStandartResponce obj, ApiContext context)
        {
            throw new NotSupportedException();
        }

        public bool CanRespondTo(IApiStandartResponce responce, HttpContextBase context)
        {
            return responce.Response is IApiContentResponce;
        }

        public void RespondTo(IApiStandartResponce responce, HttpContextBase context)
        {
            var contentResponce = (IApiContentResponce) responce.Response;
            if (contentResponce.ContentDisposition != null)
            {
                context.Response.AddHeader("Content-Disposition", contentResponce.ContentDisposition.ToString());
            }
            if (contentResponce.ContentType != null)
            {
                context.Response.ContentType = contentResponce.ContentType.ToString();
            }
            if (contentResponce.ContentEncoding != null)
            {
                context.Response.ContentEncoding = contentResponce.ContentEncoding;
            }
            context.Response.WriteStreamToResponce(contentResponce.ContentStream);
        }

        #endregion
    }
}