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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using ASC.Common.Security.Authorizing;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using log4net;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    internal abstract class FileOperation : IProgressItem
    {
        private readonly IPrincipal _principal;
        private readonly string _culture;
        private double _step;
        private int _processed;
        private List<object> _completeFiles;
        private List<object> _completeFolders;

        protected string SplitCharacter = ":";


        public object Id { get; set; }

        public bool IsCompleted { get; set; }

        public double Percentage { get; set; }

        public object Status { get; set; }

        public object Error { get; set; }

        public object Source { get; set; }

        public Guid Owner { get; private set; }

        public bool CountWithoutSubitems { get; protected set; }


        protected List<object> Folders { get; private set; }

        protected List<object> Files { get; private set; }

        protected Tenant CurrentTenant { get; private set; }

        protected FileSecurity FilesSecurity { get; private set; }

        protected IFolderDao FolderDao { get; private set; }

        protected IFileDao FileDao { get; private set; }

        protected ITagDao TagDao { get; private set; }

        protected IProviderDao ProviderDao { get; private set; }

        protected ILog Logger { get; private set; }

        protected bool Canceled { get; private set; }

        protected FileOperation(Tenant tenant, List<object> folders, List<object> files)
        {
            if (tenant == null) throw new ArgumentNullException("tenant");

            Id = Guid.NewGuid().ToString();
            CurrentTenant = tenant;
            Owner = ASC.Core.SecurityContext.CurrentAccount.ID;
            _principal = Thread.CurrentPrincipal;
            _culture = Thread.CurrentThread.CurrentCulture.Name;

            Folders = folders ?? new List<object>();
            Files = files ?? new List<object>();
            Source = string.Join(SplitCharacter, Folders.Select(f => "folder_" + f).Concat(Files.Select(f => "file_" + f)).ToArray());
            _completeFiles = new List<object>();
            _completeFolders = new List<object>();
        }

        public void RunJob()
        {
            IPrincipal oldPrincipal = null;
            try
            {
                oldPrincipal = Thread.CurrentPrincipal;
            }
            catch
            {
            }
            try
            {
                if (_principal != null)
                {
                    Thread.CurrentPrincipal = _principal;
                }
                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(_culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(_culture);
                FolderDao = Global.DaoFactory.GetFolderDao();
                FileDao = Global.DaoFactory.GetFileDao();
                TagDao = Global.DaoFactory.GetTagDao();
                Logger = Global.Logger;
                ProviderDao = Global.DaoFactory.GetProviderDao();
                FilesSecurity = new FileSecurity(Global.DaoFactory);

                try
                {
                    _step = InitProgressStep();
                }
                catch
                {
                }

                Do();
            }
            catch(AuthorizingException authError)
            {
                Error = FilesCommonResource.ErrorMassage_SecurityException;
                Logger.Error(Error, new SecurityException(Error.ToString(), authError));
            }
            catch(Exception error)
            {
                Error = error.Message;
                Logger.Error(error, error);
            }
            finally
            {
                IsCompleted = true;
                Percentage = 100;
                try
                {
                    if (oldPrincipal != null) Thread.CurrentPrincipal = oldPrincipal;
                    FolderDao.Dispose();
                    FileDao.Dispose();
                    TagDao.Dispose();
                    ProviderDao.Dispose();
                }
                catch
                {
                }
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var op = obj as FileOperation;
            return op != null && op.Id == Id;
        }


        public FileOperationResult GetResult()
        {
            var r = new FileOperationResult
                {
                    Id = Id.ToString(),
                    OperationType = OperationType,
                    Progress = (int) Percentage,
                    Source = Source != null ? Source.ToString().Trim() : null,
                    Result = Status != null ? Status.ToString().Trim() : null,
                    Error = Error != null ? Error.ToString() : null,
                    Processed = _processed.ToString(),
                    FileIds = _completeFiles.ToArray(),
                    FolderIds = _completeFolders.ToArray()
                };
#if !DEBUG
            var error = Error as Exception;
            if (error != null)
            {
                if (error is System.IO.IOException)
                {
                    r.Error = FilesCommonResource.ErrorMassage_FileNotFound;
                }
                else
                {
                    r.Error = error.Message;
                }
            }
#endif
            return r;
        }

        public void Terminate()
        {
            Canceled = true;
        }


        protected virtual double InitProgressStep()
        {
            var count = Files.Count;

            if (CountWithoutSubitems)
                count += Folders.Count;
            else
                Folders.ForEach(f => count += FolderDao.GetItemsCount(f, true));

            return count == 0 ? 100d : 100d / count;
        }

        protected void ProgressStep()
        {
            Percentage = Percentage + _step < 100d ? Percentage + _step : 99d;
        }

        protected bool ProcessedFolder(object folderId)
        {
            _processed++;
            if (Folders.Contains(folderId))
            {
                Status += string.Format("folder_{0}{1}", folderId, SplitCharacter);
                return true;
            }
            return false;
        }

        protected void ResultedFolder(object folderId)
        {
            _completeFolders.Add(folderId);
        }

        protected bool ProcessedFile(object fileId)
        {
            _processed++;
            if (Files.Contains(fileId))
            {
                Status += string.Format("file_{0}{1}", fileId, SplitCharacter);
                return true;
            }
            return false;
        }

        protected void ResultedFile(object fileId)
        {
            _completeFiles.Add(fileId);
        }


        protected abstract FileOperationType OperationType { get; }

        protected abstract void Do();
    }
}