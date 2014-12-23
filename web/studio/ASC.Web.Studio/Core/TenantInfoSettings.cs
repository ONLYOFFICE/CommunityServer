/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core
{
    [Serializable]
    [DataContract]
    public class TenantInfoSettings : ISettings
    {
        [DataMember(Name = "LogoSize")]
        public Size CompanyLogoSize { get; private set; }

        [DataMember(Name = "LogoFileName")] private string _companyLogoFileName;

        [DataMember(Name = "Default")]
        private bool _isDefault { get; set; }

        #region ISettings Members

        public ISettings GetDefault()
        {
            return new TenantInfoSettings
                       {
                           _isDefault = true
                       };
        }

        public void RestoreDefault()
        {
            _isDefault = true;

            var currentTenant = CoreContext.TenantManager.GetCurrentTenant();
            currentTenant.Name = ConfigurationManager.AppSettings["web.portal-name"] ?? "Cloud Office Applications";
            CoreContext.TenantManager.SaveTenant(currentTenant);

            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");
            try
            {
                store.DeleteFiles("", "*", false);
            }
            catch
            {
            }
            CompanyLogoSize = default(Size);
        }

        public void SetCompanyLogo(string companyLogoFileName, byte[] data)
        {
            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");

            if (!_isDefault)
            {
                try
                {
                    store.DeleteFiles("", "*", false);
                }
                catch
                {
                }
            }
            using (var memory = new MemoryStream(data))
            using (var image = Image.FromStream(memory))
            {
                CompanyLogoSize = image.Size;
                memory.Seek(0, SeekOrigin.Begin);
                store.Save(companyLogoFileName, memory);
                _companyLogoFileName = companyLogoFileName;
            }
            _isDefault = false;
        }

        public string GetAbsoluteCompanyLogoPath()
        {
            if (_isDefault)
            {
                return WebImageSupplier.GetAbsoluteWebPath("logo/logo.png");
            }

            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");
            return store.GetUri(_companyLogoFileName ?? "").ToString();
        }

        public Guid ID
        {
            get { return new Guid("{5116B892-CCDD-4406-98CD-4F18297C0C0A}"); }
        }

        #endregion
    }
}