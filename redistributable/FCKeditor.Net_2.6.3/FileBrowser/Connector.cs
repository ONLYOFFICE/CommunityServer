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

using System ;
using System.Globalization ;
using System.Xml ;
using System.Web ;

namespace FredCK.FCKeditorV2.FileBrowser
{
	public class Connector : FileWorkerBase
	{
		protected override void OnLoad(EventArgs e)
		{
			Config.LoadConfig();

			if ( !Config.Enabled )
			{
				XmlResponseHandler.SendError( Response, 1, "This connector is disabled. Please check the \"editor/filemanager/connectors/aspx/config.ascx\" file." );
				return;
			}

			// Get the main request information.
			string sCommand = Request.QueryString["Command"] ;
			string sResourceType = Request.QueryString[ "Type" ];
			string sCurrentFolder = Request.QueryString[ "CurrentFolder" ];

			if ( sCommand == null || sResourceType == null || sCurrentFolder == null )
			{
				XmlResponseHandler.SendError( Response, 1, "Invalid request." );
				return;
			}

			// Check if it is an allowed type.
			if ( !Config.CheckIsTypeAllowed( sResourceType ) )
			{
				XmlResponseHandler.SendError( Response, 1, "Invalid resource type specified." ) ;
				return ;
			}

			// Check the current folder syntax (must begin and start with a slash).
			if ( ! sCurrentFolder.EndsWith( "/" ) )
				sCurrentFolder += "/" ;
			if ( ! sCurrentFolder.StartsWith( "/" ) )
				sCurrentFolder = "/" + sCurrentFolder ;

			// Check for invalid folder paths (..).
			if ( sCurrentFolder.IndexOf( ".." ) >= 0 || sCurrentFolder.IndexOf( "\\" ) >= 0 )
			{
				XmlResponseHandler.SendError( Response, 102, "" );
				return;
			}

			// File Upload doesn't have to return XML, so it must be intercepted before anything.
			if ( sCommand == "FileUpload" )
			{
				this.FileUpload( sResourceType, sCurrentFolder, false ) ;
				return ;
			}

			XmlResponseHandler oResponseHandler = new XmlResponseHandler( this, Response );
			XmlNode oConnectorNode = oResponseHandler.CreateBaseXml( sCommand, sResourceType, sCurrentFolder );

			// Execute the required command.
			switch( sCommand )
			{
				case "GetFolders" :
					this.GetFolders( oConnectorNode, sResourceType, sCurrentFolder ) ;
					break ;
				case "GetFoldersAndFiles" :
					this.GetFolders( oConnectorNode, sResourceType, sCurrentFolder ) ;
					this.GetFiles( oConnectorNode, sResourceType, sCurrentFolder ) ;
					break ;
				case "CreateFolder" :
					this.CreateFolder( oConnectorNode, sResourceType, sCurrentFolder ) ;
					break ;
			}

			oResponseHandler.SendResponse();
		}

		#region Command Handlers

		private void GetFolders( XmlNode connectorNode, string resourceType, string currentFolder )
		{
			// Map the virtual path to the local server path.
			string sServerDir = this.ServerMapFolder( resourceType, currentFolder, false ) ;

			// Create the "Folders" node.
			XmlNode oFoldersNode = XmlUtil.AppendElement( connectorNode, "Folders" ) ;

			System.IO.DirectoryInfo oDir = new System.IO.DirectoryInfo( sServerDir ) ;
			System.IO.DirectoryInfo[] aSubDirs = oDir.GetDirectories() ;

			for ( int i = 0 ; i < aSubDirs.Length ; i++ )
			{
				// Create the "Folders" node.
				XmlNode oFolderNode = XmlUtil.AppendElement( oFoldersNode, "Folder" ) ;
				XmlUtil.SetAttribute( oFolderNode, "name", aSubDirs[i].Name ) ;
			}
		}

		private void GetFiles( XmlNode connectorNode, string resourceType, string currentFolder )
		{
			// Map the virtual path to the local server path.
			string sServerDir = this.ServerMapFolder( resourceType, currentFolder, false ) ;

			// Create the "Files" node.
			XmlNode oFilesNode = XmlUtil.AppendElement( connectorNode, "Files" ) ;

			System.IO.DirectoryInfo oDir = new System.IO.DirectoryInfo( sServerDir ) ;
			System.IO.FileInfo[] aFiles = oDir.GetFiles() ;

			for ( int i = 0 ; i < aFiles.Length ; i++ )
			{
				Decimal iFileSize = Math.Round( (Decimal)aFiles[i].Length / 1024 ) ;
				if ( iFileSize < 1 && aFiles[i].Length != 0 ) iFileSize = 1 ;

				// Create the "File" node.
				XmlNode oFileNode = XmlUtil.AppendElement( oFilesNode, "File" ) ;
				XmlUtil.SetAttribute( oFileNode, "name", aFiles[i].Name ) ;
				XmlUtil.SetAttribute( oFileNode, "size", iFileSize.ToString( CultureInfo.InvariantCulture ) ) ;
			}
		}

		private void CreateFolder( XmlNode connectorNode, string resourceType, string currentFolder )
		{
			string sErrorNumber = "0" ;

			string sNewFolderName = Request.QueryString["NewFolderName"] ;
			sNewFolderName = this.SanitizeFolderName( sNewFolderName );

			if ( sNewFolderName == null || sNewFolderName.Length == 0 )
				sErrorNumber = "102" ;
			else
			{
				// Map the virtual path to the local server path of the current folder.
				string sServerDir = this.ServerMapFolder( resourceType, currentFolder, false ) ;

				try
				{
					Util.CreateDirectory( System.IO.Path.Combine( sServerDir, sNewFolderName )) ;
				}
				catch ( ArgumentException )
				{
					sErrorNumber = "102" ;
				}
				catch ( System.IO.PathTooLongException )
				{
					sErrorNumber = "102" ;
				}
				catch ( System.IO.IOException )
				{
					sErrorNumber = "101" ;
				}
				catch ( System.Security.SecurityException )
				{
					sErrorNumber = "103" ;
				}
				catch ( Exception )
				{
					sErrorNumber = "110" ;
				}
			}

			// Create the "Error" node.
			XmlNode oErrorNode = XmlUtil.AppendElement( connectorNode, "Error" ) ;
			XmlUtil.SetAttribute( oErrorNode, "number", sErrorNumber ) ;
		}


		#endregion

		#region Directory Mapping

		internal string GetUrlFromPath( string resourceType, string folderPath )
		{
			if ( resourceType == null || resourceType.Length == 0 )
				return this.Config.UserFilesPath.TrimEnd( '/' ) + folderPath;
			else
				return this.Config.TypeConfig[ resourceType ].GetFilesPath().TrimEnd( '/' ) + folderPath;
		}

		#endregion
	}
}
