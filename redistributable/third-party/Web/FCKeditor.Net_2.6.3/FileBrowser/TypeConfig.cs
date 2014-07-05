/*
 * FCKeditor - The text editor for Internet - http://www.fckeditor.net
 * Copyright (C) 2003-2007 Frederico Caldeira Knabben
 *
 * == BEGIN LICENSE ==
 *
 * Licensed under the terms of any of the following licenses at your
 * choice:
 *
 *  - GNU General Public License Version 2 or later (the "GPL")
 *    http://www.gnu.org/licenses/gpl.html
 *
 *  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
 *    http://www.gnu.org/licenses/lgpl.html
 *
 *  - Mozilla Public License Version 1.1 or later (the "MPL")
 *    http://www.mozilla.org/MPL/MPL-1.1.html
 *
 * == END LICENSE ==
 */

using System;

namespace FredCK.FCKeditorV2.FileBrowser
{
	public class TypeConfig
	{
		private FileWorkerBase _FileWorker;

		public string[] AllowedExtensions;
		public string[] DeniedExtensions;
		public string FilesPath;
		public string FilesAbsolutePath;
		public string QuickUploadPath;
		public string QuickUploadAbsolutePath;

		private string _UserFilesPath;
		private string _UserFilesDirectory;

		private string _QuickUploadPath;
		private string _QuickUploadDirectory;

		public TypeConfig( FileWorkerBase fileWorker )
		{
			_FileWorker = fileWorker;

			AllowedExtensions = new string[ 0 ];
			DeniedExtensions = new string[ 0 ];
			FilesPath = "";
			FilesAbsolutePath = "";
			QuickUploadPath = "";
			QuickUploadAbsolutePath = "";
		}

		private FileWorkerBase FileWorker
		{
			get { return _FileWorker; }
		}

		internal string GetFilesPath()
		{
			if ( _UserFilesPath == null )
				_UserFilesPath = FilesPath.Replace( "%UserFilesPath%", this.FileWorker.Config.UserFilesPath );

			return _UserFilesPath;
		}

		internal string GetFilesDirectory()
		{
			if ( _UserFilesDirectory == null )
			{
				if ( this.FilesAbsolutePath.Length == 0 )
					_UserFilesDirectory = System.Web.HttpContext.Current.Server.MapPath( this.GetFilesPath() );
				else
					_UserFilesDirectory = FilesAbsolutePath.Replace( "%UserFilesAbsolutePath%", this.FileWorker.Config.UserFilesDirectory );
			}

			return _UserFilesDirectory;
		}

		internal string GetQuickUploadPath()
		{
			if ( _QuickUploadPath == null )
				_QuickUploadPath = QuickUploadPath.Replace( "%UserFilesPath%", this.FileWorker.Config.UserFilesPath );

			return _QuickUploadPath;
		}

		internal string GetQuickUploadDirectory()
		{
			if ( _QuickUploadDirectory == null )
			{
				if ( this.QuickUploadAbsolutePath.Length == 0 )
					_QuickUploadDirectory = System.Web.HttpContext.Current.Server.MapPath( this.GetQuickUploadPath() );
				else
					_QuickUploadDirectory = QuickUploadAbsolutePath.Replace( "%UserFilesAbsolutePath%", this.FileWorker.Config.UserFilesDirectory );
			}

			return _QuickUploadDirectory;
		}

		internal bool CheckIsAllowedExtension( string extension )
		{
			// Do not accept empty settings.
			if ( AllowedExtensions.Length == 0 && DeniedExtensions.Length == 0 )
				return false;

			if ( DeniedExtensions.Length > 0 && !Util.ArrayContains( DeniedExtensions, extension, System.Collections.CaseInsensitiveComparer.DefaultInvariant ) )
				return false;

			if ( AllowedExtensions.Length > 0 && !Util.ArrayContains( AllowedExtensions, extension, System.Collections.CaseInsensitiveComparer.DefaultInvariant ) )
				return false;

			return true;
		}
	}
}
