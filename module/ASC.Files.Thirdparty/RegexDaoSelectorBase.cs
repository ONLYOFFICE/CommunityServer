/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Globalization;
using System.Text.RegularExpressions;
using ASC.Files.Core;
using ASC.Files.Core.Security;

namespace ASC.Files.Thirdparty
{
    internal abstract class RegexDaoSelectorBase<T> : IDaoSelector
    {
        public Regex Selector { get; set; }
        public Func<object, IFileDao> FileDaoActivator { get; set; }
        public Func<object, ISecurityDao> SecurityDaoActivator { get; set; }
        public Func<object, IFolderDao> FolderDaoActivator { get; set; }
        public Func<object, ITagDao> TagDaoActivator { get; set; }
        public Func<object, T> IDConverter { get; set; }

        protected RegexDaoSelectorBase(Regex selector)
            : this(selector, null, null, null, null)
        {
        }

        protected RegexDaoSelectorBase(Regex selector,
                                       Func<object, IFileDao> fileDaoActivator,
                                       Func<object, IFolderDao> folderDaoActivator,
                                       Func<object, ISecurityDao> securityDaoActivator,
                                       Func<object, ITagDao> tagDaoActivator
            )
            : this(selector, fileDaoActivator, folderDaoActivator, securityDaoActivator, tagDaoActivator, null)
        {
        }

        protected RegexDaoSelectorBase(
            Regex selector,
            Func<object, IFileDao> fileDaoActivator,
            Func<object, IFolderDao> folderDaoActivator,
            Func<object, ISecurityDao> securityDaoActivator,
            Func<object, ITagDao> tagDaoActivator,
            Func<object, T> idConverter)
        {
            if (selector == null) throw new ArgumentNullException("selector");

            Selector = selector;
            FileDaoActivator = fileDaoActivator;
            FolderDaoActivator = folderDaoActivator;
            SecurityDaoActivator = securityDaoActivator;
            TagDaoActivator = tagDaoActivator;
            IDConverter = idConverter;
        }

        public virtual ISecurityDao GetSecurityDao(object id)
        {
            return SecurityDaoActivator != null ? SecurityDaoActivator(id) : null;
        }


        public virtual object ConvertId(object id)
        {
            try
            {
                if (id == null) return null;

                return IDConverter != null ? IDConverter(id) : id;
            }
            catch (Exception fe)
            {
                throw new FormatException("Can not convert id: " + id, fe);
            }
        }

        public virtual object GetIdCode(object id)
        {
            return null;
        }

        public virtual bool IsMatch(object id)
        {
            return id != null && Selector.IsMatch(Convert.ToString(id, CultureInfo.InvariantCulture));
        }


        public virtual IFileDao GetFileDao(object id)
        {
            return FileDaoActivator != null ? FileDaoActivator(id) : null;
        }

        public virtual IFolderDao GetFolderDao(object id)
        {
            return FolderDaoActivator != null ? FolderDaoActivator(id) : null;
        }

        public virtual ITagDao GetTagDao(object id)
        {
            return TagDaoActivator != null ? TagDaoActivator(id) : null;
        }
    }
}