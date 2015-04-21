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
using System.Web;
using System.Xml;

namespace FredCK.FCKeditorV2.FileBrowser
{
	class XmlResponseHandler
	{
		private Connector _Connector;
		private HttpResponse _Response;
		private XmlDocument _Xml;

		internal XmlResponseHandler( Connector connector, HttpResponse response )
		{
			_Connector = connector;
			_Response = response;
		}

		private HttpResponse Response
		{
			get { return _Response; }
		}

		private Connector Connector
		{
			get { return _Connector; }
		}

		private XmlDocument Xml
		{
			get
			{
				if ( _Xml == null )
					_Xml = new XmlDocument();
				
				return _Xml;
			}
		}

		public XmlNode CreateBaseXml( string command, string resourceType, string currentFolder )
		{
			// Create the XML document header.
			Xml.AppendChild( Xml.CreateXmlDeclaration( "1.0", "utf-8", null ) );

			// Create the main "Connector" node.
			XmlNode oConnectorNode = XmlUtil.AppendElement( Xml, "Connector" );
			XmlUtil.SetAttribute( oConnectorNode, "command", command );
			XmlUtil.SetAttribute( oConnectorNode, "resourceType", resourceType );

			// Add the current folder node.
			XmlNode oCurrentNode = XmlUtil.AppendElement( oConnectorNode, "CurrentFolder" );
			XmlUtil.SetAttribute( oCurrentNode, "path", currentFolder );
			XmlUtil.SetAttribute( oCurrentNode, "url", Connector.GetUrlFromPath( resourceType, currentFolder ) );

			return oConnectorNode;
		}

		private void SetupResponse()
		{
			XmlResponseHandler.SetupResponse( this.Response );
		}

		private static void SetupResponse( HttpResponse response )
		{
			// Cleans the response buffer.
			response.ClearHeaders();
			response.Clear();

			// Prevent the browser from caching the result.
			response.CacheControl = "no-cache";

			// Set the response format.
			response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
			response.ContentType = "text/xml";
		}

		public void SendResponse()
		{
			SetupResponse();
			Response.Write( Xml.OuterXml );
			Response.End();
		}

		internal static void SendError( HttpResponse response, int errorNumber, string errorText )
		{
			SetupResponse( response );

			response.Write( "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" ) ;
			response.Write( "<Connector>" );
			response.Write( "<Error number=\"" + errorNumber + "\" text=\"" + HttpUtility.HtmlEncode( errorText ) + "\" />" );
			response.Write( "</Connector>" );

			response.End() ;
		}
	}
}
