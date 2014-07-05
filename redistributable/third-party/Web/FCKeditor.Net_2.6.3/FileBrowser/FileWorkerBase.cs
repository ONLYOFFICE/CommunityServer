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
 * Base class used by the FileBrowserConnector and Uploader.
 */

using System;
using System.Web;
using System.Text.RegularExpressions;

namespace FredCK.FCKeditorV2.FileBrowser
{
	public abstract class FileWorkerBase : System.Web.UI.Page
	{
		public Config Config;

		protected void FileUpload( string resourceType, string currentFolder, bool isQuickUpload )
		{
			HttpPostedFile oFile = Request.Files[ "NewFile" ];

			string sFileName = "";

			if ( oFile == null )
			{
				this.SendFileUploadResponse( 202, isQuickUpload );
				return;
			}

			// Map the virtual path to the local server path.
			string sServerDir = this.ServerMapFolder( resourceType, currentFolder, isQuickUpload );

			// Get the uploaded file name.
			sFileName = System.IO.Path.GetFileName( oFile.FileName );
			sFileName = this.SanitizeFileName( sFileName );

			string sExtension = System.IO.Path.GetExtension( oFile.FileName );
			sExtension = sExtension.TrimStart( '.' );

			if ( !this.Config.TypeConfig[ resourceType ].CheckIsAllowedExtension( sExtension ) )
			{
				this.SendFileUploadResponse( 202, isQuickUpload );
				return;
			}

			if ( this.Config.CheckIsNonHtmlExtension( sExtension ) && !this.CheckNonHtmlFile( oFile ) )
			{
				this.SendFileUploadResponse( 202, isQuickUpload );
				return;
			}

			int iErrorNumber = 0;
			int iCounter = 0;

			while ( true )
			{
				string sFilePath = System.IO.Path.Combine( sServerDir, sFileName );

				if ( System.IO.File.Exists( sFilePath ) )
				{
					iCounter++;
					sFileName =
						System.IO.Path.GetFileNameWithoutExtension( oFile.FileName ) +
						"(" + iCounter + ")." +
						sExtension;

					iErrorNumber = 201;
				}
				else
				{
					oFile.SaveAs( sFilePath );
					break;
				}
			}

			TypeConfig typeConfig = this.Config.TypeConfig[resourceType] ;

			string sFileUrl = isQuickUpload ? typeConfig.GetQuickUploadPath() : typeConfig.GetFilesPath() ;
			sFileUrl += sFileName;

			this.SendFileUploadResponse( iErrorNumber, isQuickUpload, sFileUrl, sFileName );
		}

		private void SendFileUploadResponse( int errorNumber, bool isQuickUpload )
		{
			this.SendFileUploadResponse( errorNumber, isQuickUpload, "", "", "" );
		}

		private void SendFileUploadResponse( int errorNumber, bool isQuickUpload, string fileUrl, string fileName )
		{
			this.SendFileUploadResponse( errorNumber, isQuickUpload, fileUrl, fileName, "" );
		}

		protected void SendFileUploadResponse( int errorNumber, bool isQuickUpload, string fileUrl, string fileName, string customMsg )
		{
			Response.Clear();

			Response.Write( "<script type=\"text/javascript\">" );

			// Minified version of the document.domain automatic fix script.
			// The original script can be found at _dev/domain_fix_template.js
			Response.Write( @"(function(){var d=document.domain;while (true){try{var A=window.top.opener.document.domain;break;}catch(e) {};d=d.replace(/.*?(?:\.|$)/,'');if (d.length==0) break;try{document.domain=d;}catch (e){break;}}})();" );

			if ( isQuickUpload )
				Response.Write( "window.parent.OnUploadCompleted(" + errorNumber + ",'" + fileUrl.Replace( "'", "\\'" ) + "','" + fileName.Replace( "'", "\\'" ) + "','" + customMsg.Replace( "'", "\\'" ) + "') ;" );
			else
				Response.Write( "window.parent.frames['frmUpload'].OnUploadCompleted(" + errorNumber + ",'" + fileName.Replace( "'", "\\'" ) + "') ;" );

			Response.Write( "</script>" );

			Response.End();
		}

		protected string ServerMapFolder( string resourceType, string folderPath, bool isQuickUpload )
		{
			TypeConfig typeConfig = this.Config.TypeConfig[ resourceType ];

			// Get the resource type directory.
			string sResourceTypePath = isQuickUpload ? typeConfig.GetQuickUploadDirectory() : typeConfig.GetFilesDirectory();

			// Ensure that the directory exists.
			Util.CreateDirectory( sResourceTypePath );

			// Return the resource type directory combined with the required path.
			return System.IO.Path.Combine( sResourceTypePath, folderPath.TrimStart( '/' ) );
		}


		// Do a cleanup of the folder name to avoid possible problems
		protected string SanitizeFolderName( string folderName )
		{
			// Remove . \ / | : ? * " < >
			return Regex.Replace( folderName, @"[.\\/|:?*""<>\p{C}]", "_", RegexOptions.None );
		}

		// Do a cleanup of the file name to avoid possible problems
		private string SanitizeFileName( string fileName )
		{
			// Replace dots in the name with underscores (only one dot can be there... security issue).
			if ( Config.ForceSingleExtension )
				fileName = Regex.Replace( fileName, @"\.(?![^.]*$)", "_", RegexOptions.None );

			// Remove \ / | : ? * " < >
			return Regex.Replace( fileName, @"[\\/|:?*""<>\p{C}]", "_", RegexOptions.None );
		}

		private bool CheckNonHtmlFile( HttpPostedFile file )
		{
			byte[] buffer = new byte[ 1024 ];
			file.InputStream.Read( buffer, 0, 1024 );

			string firstKB = System.Text.ASCIIEncoding.ASCII.GetString( buffer );

			if ( Regex.IsMatch( firstKB, @"<!DOCTYPE\W*X?HTML", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			if ( Regex.IsMatch( firstKB, @"<(?:body|head|html|img|pre|script|table|title)", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			//type = javascript
			if ( Regex.IsMatch( firstKB, @"type\s*=\s*[\'""]?\s*(?:\w*/)?(?:ecma|java)", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			//href = javascript
			//src = javascript
			//data = javascript
			if ( Regex.IsMatch( firstKB, @"(?:href|src|data)\s*=\s*[\'""]?\s*(?:ecma|java)script:", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			//url(javascript
			if ( Regex.IsMatch( firstKB, @"url\s*\(\s*[\'""]?\s*(?:ecma|java)script:", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			return true;
		}
	}
}
