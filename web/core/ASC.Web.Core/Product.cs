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
using System.IO;
using ASC.Data.Storage;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core
{
    [WebZoneAttribute(WebZoneType.TopNavigationProductList | WebZoneType.StartProductList)]
    public abstract class Product : IProduct
    {
        public abstract Guid ProductID { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string StartURL { get; }

        public abstract string ProductClassName { get; }

        public abstract void Init();

        public abstract ProductContext Context { get; }

        public virtual void Shutdown() { }

        public virtual string ExtendedDescription { get { return Description; } }

        WebItemContext IWebItem.Context { get { return ((IProduct)this).Context; } }

        Guid IWebItem.ID { get { return ProductID; } }

        public string GetResourcePath(string relativePath)
        {
            return WebPath.GetPath(Path.Combine(StartURL, relativePath));
        }
    }
}
