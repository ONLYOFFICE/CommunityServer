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
 * This is the code behind of the uploader.aspx page used for Quick Uploads.
 */

using System ;
using System.Globalization ;
using System.Xml ;
using System.Web ;

namespace FredCK.FCKeditorV2.FileBrowser
{
	public class Uploader : FileWorkerBase
	{
		protected override void OnLoad(EventArgs e)
		{
			this.Config.LoadConfig();

			if ( !Config.Enabled )
			{
				this.SendFileUploadResponse( 1, true, "", "", "This connector is disabled. Please check the \"editor/filemanager/connectors/aspx/config.aspx\" file." );
				return;
			}

			string sResourceType = Request.QueryString[ "Type" ];

			if ( sResourceType == null )
			{
				this.SendFileUploadResponse( 1, true, "", "", "Invalid request." );
				return;
			}

			// Check if it is an allowed type.
			if ( !Config.CheckIsTypeAllowed( sResourceType ) )
			{
				this.SendFileUploadResponse( 1, true, "", "", "Invalid resource type specified." );
				return;
			}

			this.FileUpload( sResourceType, "/", true );
		}
	}
}
