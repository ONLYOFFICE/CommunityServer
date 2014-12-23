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

using ASC.Core.Configuration;
using ASC.Core.Tenants;

namespace ASC.Core
{
    public interface IConfigurationClient
    {
        SmtpSettings SmtpSettings { get; set; }

        bool Standalone { get; }

        bool Personal { get; }

        bool PartnerHosted { get; }

        string SKey { get; set; }


        string GetSetting(string key, int tenant = Tenant.DEFAULT_TENANT);

        void SaveSetting(string key, string value, int tenant = Tenant.DEFAULT_TENANT);

        string GetKey(int tenant);

        T GetSection<T>() where T : class;
        T GetSection<T>(int tenantId) where T : class;
        T GetSection<T>(string sectionName) where T : class;
        T GetSection<T>(int tenantId, string sectionName) where T : class;
        void SaveSection<T>(string sectionName, T section) where T : class;
        void SaveSection<T>(T section) where T : class;
        void SaveSection<T>(int tenantId, T section) where T : class;
        void SaveSection<T>(int tenantId, string sectionName, T section) where T : class;
    }
}