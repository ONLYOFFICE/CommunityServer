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
 *
 * This is the code behind of the connector.aspx page used by the 
 * File Browser.
 */

using System;
using System.Text;
using System.Web;

namespace FredCK.FCKeditorV2.FileBrowser
{
	public class Config : System.Web.UI.UserControl
	{
		private const string DEFAULT_USER_FILES_PATH = "/userfiles/";

		private string sUserFilesDirectory;

		public bool Enabled;
		public string UserFilesPath;
		public string UserFilesAbsolutePath;
		public bool ForceSingleExtension;
		public string[] AllowedTypes;
		public string[] HtmlExtensions;
		public TypeConfigList TypeConfig;

		public Config()
		{}

		private void DefaultSettings() 
		{
			// Initialize all default settings.

			Enabled = false;
			UserFilesPath = "/userfiles/";
			UserFilesAbsolutePath = "";
			ForceSingleExtension = true;
			AllowedTypes = new string[] { "File", "Image", "Flash", "Media" };
			HtmlExtensions = new string[] { "html", "htm", "xml", "xsd", "txt", "js" };

			TypeConfig = new TypeConfigList( (FileWorkerBase)this.Page );

			TypeConfig[ "File" ].AllowedExtensions = new string[] { "7z", "aiff", "asf", "avi", "bmp", "csv", "doc", "fla", "flv", "gif", "gz", "gzip", "jpeg", "jpg", "mid", "mov", "mp3", "mp4", "mpc", "mpeg", "mpg", "ods", "odt", "pdf", "png", "ppt", "pxd", "qt", "ram", "rar", "rm", "rmi", "rmvb", "rtf", "sdc", "sitd", "swf", "sxc", "sxw", "tar", "tgz", "tif", "tiff", "txt", "vsd", "wav", "wma", "wmv", "xls", "xml", "zip" };
			TypeConfig[ "File" ].DeniedExtensions = new string[] { };
			TypeConfig[ "File" ].FilesPath = "%UserFilesPath%file/";
			TypeConfig[ "File" ].FilesAbsolutePath = ( UserFilesAbsolutePath == "" ? "" : "%UserFilesAbsolutePath%file/" );
			TypeConfig[ "File" ].QuickUploadPath = "%UserFilesPath%";
			TypeConfig[ "File" ].QuickUploadAbsolutePath = "%UserFilesAbsolutePath%";

			TypeConfig[ "Image" ].AllowedExtensions = new string[] { "bmp", "gif", "jpeg", "jpg", "png" };
			TypeConfig[ "Image" ].DeniedExtensions = new string[] { };
			TypeConfig[ "Image" ].FilesPath = "%UserFilesPath%image/";
			TypeConfig[ "Image" ].FilesAbsolutePath = ( UserFilesAbsolutePath == "" ? "" : "%UserFilesAbsolutePath%image/" );
			TypeConfig[ "Image" ].QuickUploadPath = "%UserFilesPath%";
			TypeConfig[ "Image" ].QuickUploadAbsolutePath = "%UserFilesAbsolutePath%";

			TypeConfig[ "Flash" ].AllowedExtensions = new string[] { "swf", "flv" };
			TypeConfig[ "Flash" ].DeniedExtensions = new string[] { };
			TypeConfig[ "Flash" ].FilesPath = "%UserFilesPath%flash/";
			TypeConfig[ "Flash" ].FilesAbsolutePath = ( UserFilesAbsolutePath == "" ? "" : "%UserFilesAbsolutePath%flash/" );
			TypeConfig[ "Flash" ].QuickUploadPath = "%UserFilesPath%";
			TypeConfig[ "Flash" ].QuickUploadAbsolutePath = "%UserFilesAbsolutePath%";

			TypeConfig[ "Media" ].AllowedExtensions = new string[] { "aiff", "asf", "avi", "bmp", "fla", "flv", "gif", "jpeg", "jpg", "mid", "mov", "mp3", "mp4", "mpc", "mpeg", "mpg", "png", "qt", "ram", "rm", "rmi", "rmvb", "swf", "tif", "tiff", "wav", "wma", "wmv" };
			TypeConfig[ "Media" ].DeniedExtensions = new string[] { };
			TypeConfig[ "Media" ].FilesPath = "%UserFilesPath%media/";
			TypeConfig[ "Media" ].FilesAbsolutePath = ( UserFilesAbsolutePath == "" ? "" : "%UserFilesAbsolutePath%media/" );
			TypeConfig[ "Media" ].QuickUploadPath = "%UserFilesPath%";
			TypeConfig[ "Media" ].QuickUploadAbsolutePath = "%UserFilesAbsolutePath%";
		}

		internal void LoadConfig()
		{
			DefaultSettings();

			// Call the setConfig() function for the configuration file (config.ascx).
			SetConfig();

			// Look for possible UserFilesPath override options.

			// Session
			string userFilesPath = Session[ "FCKeditor:UserFilesPath" ] as string;

			// Application
			if ( userFilesPath == null || userFilesPath.Length == 0 )
				userFilesPath = Application[ "FCKeditor:UserFilesPath" ] as string;

			// Web.config file.
			if ( userFilesPath == null || userFilesPath.Length == 0 )
				userFilesPath = System.Configuration.ConfigurationSettings.AppSettings[ "FCKeditor:UserFilesPath" ];

			// config.asxc
			if ( userFilesPath == null || userFilesPath.Length == 0 )
				userFilesPath = this.UserFilesPath;

			if ( userFilesPath == null || userFilesPath.Length == 0 )
				userFilesPath = DEFAULT_USER_FILES_PATH;

			// Check that the user path ends with slash ("/")
			if ( !userFilesPath.EndsWith( "/" ) )
				userFilesPath += "/";

			userFilesPath = this.ResolveUrl( userFilesPath );

			this.UserFilesPath = userFilesPath;
		}

		/// <summary>
		/// The absolution path (server side) of the user files directory. It 
		/// is based on the <see cref="FileWorkerBase.UserFilesPath"/>.
		/// </summary>
		internal string UserFilesDirectory
		{
			get
			{
				if ( sUserFilesDirectory == null )
				{
					if ( this.UserFilesAbsolutePath.Length > 0 )
					{
						sUserFilesDirectory = this.UserFilesAbsolutePath;
						sUserFilesDirectory = sUserFilesDirectory.TrimEnd( '\\', '/' ) + '/';
					}
					else
					{
						// Get the local (server) directory path translation.
						sUserFilesDirectory = Server.MapPath( this.UserFilesPath );
					}
				}
				return sUserFilesDirectory;
			}
		}

		public virtual void SetConfig()
		{ }

		internal bool CheckIsTypeAllowed( string typeName )
		{
			return ( System.Array.IndexOf( this.AllowedTypes, typeName ) >= 0 );
		}

		internal bool CheckIsNonHtmlExtension( string extension )
		{
			return ( this.HtmlExtensions.Length == 0 || !Util.ArrayContains( this.HtmlExtensions, extension, System.Collections.CaseInsensitiveComparer.DefaultInvariant ) );
		}
	}
}
