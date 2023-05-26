/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Files.Core;
using ASC.Files.Core.Security;

namespace ASC.Web.Files.Classes
{
    [Serializable]
    [DataContract]
    public class FilesSettings : BaseSettings<FilesSettings>
    {
        [DataMember(Name = "EnableThirdpartySettings")]
        public bool EnableThirdpartySetting { get; set; }

        [DataMember(Name = "FastDelete")]
        public bool FastDeleteSetting { get; set; }

        [DataMember(Name = "StoreOriginalFiles")]
        public bool StoreOriginalFilesSetting { get; set; }

        [DataMember(Name = "UpdateIfExist")]
        public bool UpdateIfExistSetting { get; set; }

        [DataMember(Name = "ConvertNotify")]
        public bool ConvertNotifySetting { get; set; }

        [DataMember(Name = "SortedBy")]
        public SortedByType DefaultSortedBySetting { get; set; }

        [DataMember(Name = "SortedAsc")]
        public bool DefaultSortedAscSetting { get; set; }

        [DataMember(Name = "HideConfirmConvertSave")]
        public bool HideConfirmConvertSaveSetting { get; set; }

        [DataMember(Name = "HideConfirmConvertOpen")]
        public bool HideConfirmConvertOpenSetting { get; set; }

        [DataMember(Name = "Forcesave")]
        public bool ForcesaveSetting { get; set; }

        [DataMember(Name = "StoreForcesave")]
        public bool StoreForcesaveSetting { get; set; }

        [DataMember(Name = "HideRecent")]
        public bool HideRecentSetting { get; set; }

        [DataMember(Name = "HideFavorites")]
        public bool HideFavoritesSetting { get; set; }

        [DataMember(Name = "HideTemplates")]
        public bool HideTemplatesSetting { get; set; }

        [DataMember(Name = "DownloadZip")]
        private bool DownloadTarGzSetting { get; set; }

        [DataMember(Name = "ShareLink")]
        public bool DisableShareLinkSetting { get; set; }

        [DataMember(Name = "ShareLinkSocialMedia")]
        public bool DisableShareSocialMediaSetting { get; set; }

        [DataMember(Name = "AutomaticallyCleanUp")]
        public AutoCleanUpData AutomaticallyCleanUpSetting { get; set; }

        [DataMember(Name = "DefaultSharingAccessRights")]
        public List<FileShare> DefaultSharingAccessRightsSetting { get; set; }

        public override ISettings GetDefault()
        {
            return new FilesSettings
            {
                FastDeleteSetting = false,
                EnableThirdpartySetting = true,
                StoreOriginalFilesSetting = true,
                UpdateIfExistSetting = false,
                ConvertNotifySetting = true,
                DefaultSortedBySetting = SortedByType.DateAndTime,
                DefaultSortedAscSetting = false,
                HideConfirmConvertSaveSetting = false,
                HideConfirmConvertOpenSetting = false,
                ForcesaveSetting = false,
                StoreForcesaveSetting = false,
                HideFavoritesSetting = false,
                HideRecentSetting = false,
                HideTemplatesSetting = false,
                DownloadTarGzSetting = false,
                DisableShareLinkSetting = CoreContext.Configuration.CustomMode,
                DisableShareSocialMediaSetting = CoreContext.Configuration.CustomMode,
                AutomaticallyCleanUpSetting = null,
                DefaultSharingAccessRightsSetting = null
            };
        }

        public override Guid ID
        {
            get { return new Guid("{03B382BD-3C20-4f03-8AB9-5A33F016316E}"); }
        }

        public static bool ConfirmDelete
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.FastDeleteSetting = !value;
                setting.SaveForCurrentUser();
            }
            get { return !LoadForCurrentUser().FastDeleteSetting; }
        }

        public static bool EnableThirdParty
        {
            set
            {
                var setting = Load();
                setting.EnableThirdpartySetting = value;
                setting.Save();
            }
            get { return Load().EnableThirdpartySetting; }
        }

        public static bool ExternalShare
        {
            set
            {
                var setting = Load();
                setting.DisableShareLinkSetting = !value;
                setting.Save();
            }
            get { return !Load().DisableShareLinkSetting; }
        }

        public static bool ExternalShareSocialMedia
        {
            set
            {
                var setting = Load();
                setting.DisableShareSocialMediaSetting = !value;
                setting.Save();
            }
            get
            {
                var setting = Load();
                return !setting.DisableShareLinkSetting && !setting.DisableShareSocialMediaSetting;
            }
        }

        public static bool StoreOriginalFiles
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.StoreOriginalFilesSetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().StoreOriginalFilesSetting; }
        }

        public static bool UpdateIfExist
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.UpdateIfExistSetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().UpdateIfExistSetting; }
        }

        public static bool ConvertNotify
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.ConvertNotifySetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().ConvertNotifySetting; }
        }

        public static bool HideConfirmConvertSave
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.HideConfirmConvertSaveSetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().HideConfirmConvertSaveSetting; }
        }

        public static bool HideConfirmConvertOpen
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.HideConfirmConvertOpenSetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().HideConfirmConvertOpenSetting; }
        }

        public static OrderBy DefaultOrder
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.DefaultSortedBySetting = value.SortedBy;
                setting.DefaultSortedAscSetting = value.IsAsc;
                setting.SaveForCurrentUser();
            }
            get
            {
                var setting = LoadForCurrentUser();
                return new OrderBy(setting.DefaultSortedBySetting, setting.DefaultSortedAscSetting);
            }
        }

        public static bool Forcesave
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.ForcesaveSetting = value;
                setting.SaveForCurrentUser();
            }
            get { return LoadForCurrentUser().ForcesaveSetting; }
        }

        public static bool StoreForcesave
        {
            set
            {
                if (CoreContext.Configuration.Personal) throw new NotSupportedException();
                var setting = Load();
                setting.StoreForcesaveSetting = value;
                setting.Save();
            }
            get { return !CoreContext.Configuration.Personal && Load().StoreForcesaveSetting; }
        }

        public static bool RecentSection
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.HideRecentSetting = !value;
                setting.SaveForCurrentUser();
            }
            get { return !LoadForCurrentUser().HideRecentSetting; }
        }

        public static bool FavoritesSection
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.HideFavoritesSetting = !value;
                setting.SaveForCurrentUser();
            }
            get { return !LoadForCurrentUser().HideFavoritesSetting; }
        }

        public static bool TemplatesSection
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.HideTemplatesSetting = !value;
                setting.SaveForCurrentUser();
            }
            get { return !LoadForCurrentUser().HideTemplatesSetting; }
        }

        public static bool DownloadTarGz
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.DownloadTarGzSetting = value;
                setting.SaveForCurrentUser();
            }
            get => LoadForCurrentUser().DownloadTarGzSetting;
        }

        public static AutoCleanUpData AutomaticallyCleanUp
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.AutomaticallyCleanUpSetting = value;
                setting.SaveForCurrentUser();
            }
            get
            {
                var setting = LoadForCurrentUser().AutomaticallyCleanUpSetting;
                return setting ?? new AutoCleanUpData();
            }
        }

        public static List<FileShare> DefaultSharingAccessRights
        {
            set
            {
                List<FileShare> GetNormalizedList(List<FileShare> src)
                {
                    if (src == null || !src.Any())
                    {
                        return null;
                    }

                    var res = new List<FileShare>();

                    if (src.Contains(FileShare.FillForms))
                    {
                        res.Add(FileShare.FillForms);
                    }

                    if (src.Contains(FileShare.CustomFilter))
                    {
                        res.Add(FileShare.CustomFilter);
                    }

                    if (src.Contains(FileShare.Review))
                    {
                        res.Add(FileShare.Review);
                    }

                    if (src.Contains(FileShare.ReadWrite))
                    {
                        res.Add(FileShare.ReadWrite);
                        return res;
                    }

                    if (src.Contains(FileShare.Comment))
                    {
                        res.Add(FileShare.Comment);
                        return res;
                    }

                    res.Add(FileShare.Read);
                    return res;
                }

                var setting = LoadForCurrentUser();
                setting.DefaultSharingAccessRightsSetting = GetNormalizedList(value);
                setting.SaveForCurrentUser();
            }
            get
            {
                var setting = LoadForCurrentUser().DefaultSharingAccessRightsSetting;
                return setting ?? new List<FileShare>() { FileShare.Read };
            }
        }
    }
}