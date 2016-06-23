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