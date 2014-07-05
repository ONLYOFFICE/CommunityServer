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
 * This is the FCKeditor Asp.Net control.
 */

using System ;
using System.Web.UI ;
using System.Web.UI.WebControls ;
using System.ComponentModel ;
using System.Text.RegularExpressions ;
using System.Globalization ;
using System.Security.Permissions ;

namespace FredCK.FCKeditorV2
{
	public enum LanguageDirection
	{
		LeftToRight,
		RightToLeft
	}

	[ DefaultProperty("Value") ]
	[ ValidationProperty("Value") ]
	[ ToolboxData("<{0}:FCKeditor runat=server></{0}:FCKeditor>") ]
	[ Designer("FredCK.FCKeditorV2.FCKeditorDesigner") ]
	[ ParseChildren(false) ]
	public class FCKeditor : System.Web.UI.Control, IPostBackDataHandler
	{
		private bool _IsCompatible ;

		public FCKeditor()
		{}

		#region Base Configurations Properties
		
		[ Browsable( false ) ]
		public FCKeditorConfigurations Config
		{
			get 
			{ 
				if ( ViewState["Config"] == null )
					ViewState["Config"] = new FCKeditorConfigurations() ; 
				return (FCKeditorConfigurations)ViewState["Config"] ;
			}
		}

		[ DefaultValue( "" ) ]
		public string Value
		{
			get { object o = ViewState["Value"] ; return ( o == null ? "" : (string)o ) ; }
			set { ViewState["Value"] = value ; }
		}

		/// <summary>
		/// <p>
		///		Sets or gets the virtual path to the editor's directory. It is
		///		relative to the current page.
		/// </p>
		/// <p>
		///		The default value is "/fckeditor/".
		/// </p>
		/// <p>
		///		The base path can be also set in the Web.config file using the 
		///		appSettings section. Just set the "FCKeditor:BasePath" for that. 
		///		For example:
		///		<code>
		///		&lt;configuration&gt;
		///			&lt;appSettings&gt;
		///				&lt;add key="FCKeditor:BasePath" value="/scripts/fckeditor/" /&gt;
		///			&lt;/appSettings&gt;
		///		&lt;/configuration&gt;
		///		</code>
		/// </p>
		/// </summary>
		[ DefaultValue( "/fckeditor/" ) ]
		public string BasePath
		{
			get 
			{ 
				object o = ViewState["BasePath"] ; 

				if ( o == null )
					o = System.Configuration.ConfigurationSettings.AppSettings["FCKeditor:BasePath"] ;

				return ( o == null ? "/fckeditor/" : (string)o ) ;
			}
			set { ViewState["BasePath"] = value ; }
		}

		[ DefaultValue( "Default" ) ]
		public string ToolbarSet
		{
			get { object o = ViewState["ToolbarSet"] ; return ( o == null ? "Default" : (string)o ) ; }
			set { ViewState["ToolbarSet"] = value ; }
		}

		#endregion

		#region Appearence Properties

		[ Category( "Appearence" ) ]
		[ DefaultValue( "100%" ) ]
		public Unit Width
		{
			get { object o = ViewState["Width"] ; return ( o == null ? Unit.Percentage(100) : (Unit)o ) ; }
			set { ViewState["Width"] = value ; }
		}

		[ Category("Appearence") ]
		[ DefaultValue( "200px" ) ]
		public Unit Height
		{
			get { object o = ViewState["Height"] ; return ( o == null ? Unit.Pixel( 200 ) : (Unit)o ) ; }
			set { ViewState["Height"] = value ; }
		}

		#endregion

		#region Configurations Properties

		[ Category("Configurations") ]
		public string CustomConfigurationsPath
		{
			set { this.Config["CustomConfigurationsPath"] = value ; }
		}

		[ Category("Configurations") ]
		public string EditorAreaCSS
		{
			set { this.Config["EditorAreaCSS"] = value ; }
		}

		[ Category("Configurations") ]
		public string BaseHref
		{
			set { this.Config["BaseHref"] = value ; }
		}

		[ Category("Configurations") ]
		public string SkinPath
		{
			set { this.Config["SkinPath"] = value ; }
		}

		[ Category("Configurations") ]
		public string PluginsPath
		{
			set { this.Config["PluginsPath"] = value ; }
		}

		[ Category("Configurations") ]
		public bool FullPage
		{
			set { this.Config["FullPage"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool Debug
		{
			set { this.Config["Debug"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool AutoDetectLanguage
		{
			set { this.Config["AutoDetectLanguage"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public string DefaultLanguage
		{
			set { this.Config["DefaultLanguage"] = value ; }
		}

		[ Category("Configurations") ]
		public LanguageDirection ContentLangDirection
		{
			set { this.Config["ContentLangDirection"] = ( value == LanguageDirection.LeftToRight ? "ltr" : "rtl" )  ; }
		}

		[ Category("Configurations") ]
		public bool EnableXHTML
		{
			set { this.Config["EnableXHTML"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool EnableSourceXHTML
		{
			set { this.Config["EnableSourceXHTML"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool FillEmptyBlocks
		{
			set { this.Config["FillEmptyBlocks"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool FormatSource
		{
			set { this.Config["FormatSource"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool FormatOutput
		{
			set { this.Config["FormatOutput"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public string FormatIndentator
		{
			set { this.Config["FormatIndentator"] = value ; }
		}

		[ Category("Configurations") ]
		public bool GeckoUseSPAN
		{
			set { this.Config["GeckoUseSPAN"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool StartupFocus
		{
			set { this.Config["StartupFocus"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool ForcePasteAsPlainText
		{
			set { this.Config["ForcePasteAsPlainText"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool ForceSimpleAmpersand
		{
			set { this.Config["ForceSimpleAmpersand"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public int TabSpaces
		{
			set { this.Config["TabSpaces"] = value.ToString( CultureInfo.InvariantCulture ) ; }
		}

		[ Category("Configurations") ]
		public bool UseBROnCarriageReturn
		{
			set { this.Config["UseBROnCarriageReturn"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool ToolbarStartExpanded
		{
			set { this.Config["ToolbarStartExpanded"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public bool ToolbarCanCollapse
		{
			set { this.Config["ToolbarCanCollapse"] = ( value ? "true" : "false" ) ; }
		}

		[ Category("Configurations") ]
		public string FontColors
		{
			set { this.Config["FontColors"] = value ; }
		}

		[ Category("Configurations") ]
		public string FontNames
		{
			set { this.Config["FontNames"] = value ; }
		}

		[ Category("Configurations") ]
		public string FontSizes
		{
			set { this.Config["FontSizes"] = value ; }
		}

		[ Category("Configurations") ]
		public string FontFormats
		{
			set { this.Config["FontFormats"] = value ; }
		}

		[ Category("Configurations") ]
		public string StylesXmlPath
		{
			set { this.Config["StylesXmlPath"] = value ; }
		}

		[ Category("Configurations") ]
		public string LinkBrowserURL
		{
			set { this.Config["LinkBrowserURL"] = value ; }
		}

		[ Category("Configurations") ]
		public string ImageBrowserURL
		{
			set { this.Config["ImageBrowserURL"] = value ; }
		}

		[Category( "Configurations" )]
		public bool HtmlEncodeOutput
		{
			set { this.Config[ "HtmlEncodeOutput" ] = ( value ? "true" : "false" ); }
		}

		#endregion

		#region Rendering

		public string CreateHtml()
		{
			System.IO.StringWriter strWriter = new System.IO.StringWriter() ;
			System.Web.UI.HtmlTextWriter writer = new HtmlTextWriter( strWriter );
			this.Render( writer );
			return strWriter.ToString();
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.Write( "<div>" ) ;

			if ( _IsCompatible )
			{
				string sLink = this.BasePath ;
				if ( sLink.StartsWith( "~" ) )
					sLink = this.ResolveUrl( sLink ) ;

				string sFile = 
					System.Web.HttpContext.Current.Request.QueryString["fcksource"] == "true" ? 
						"fckeditor.original.html" : 
						"fckeditor.html" ;

				sLink += "editor/" + sFile + "?InstanceName=" + this.ClientID ;
				if ( this.ToolbarSet.Length > 0 ) sLink += "&amp;Toolbar=" + this.ToolbarSet ;

				// Render the linked hidden field.
				writer.Write( 
					"<input type=\"hidden\" id=\"{0}\" name=\"{1}\" value=\"{2}\" />",
						this.ClientID,
						this.UniqueID,
						System.Web.HttpUtility.HtmlEncode( this.Value ) ) ;

				// Render the configurations hidden field.
				writer.Write( 
					"<input type=\"hidden\" id=\"{0}___Config\" value=\"{1}\" />",
						this.ClientID,
						this.Config.GetHiddenFieldString() ) ;

				// Render the editor IFRAME.
				writer.Write(
					"<iframe id=\"{0}___Frame\" src=\"{1}\" width=\"{2}\" height=\"{3}\" frameborder=\"no\" scrolling=\"no\"></iframe>",
						this.ClientID,
						sLink,
						this.Width,
						this.Height ) ;
			}
			else
			{
				writer.Write(
					"<textarea name=\"{0}\" rows=\"4\" cols=\"40\" style=\"width: {1}; height: {2}\" wrap=\"virtual\">{3}</textarea>",
						this.UniqueID,
						this.Width,
						this.Height,
						System.Web.HttpUtility.HtmlEncode( this.Value ) ) ;
			}

			writer.Write( "</div>" ) ;

		}

		protected override void OnPreRender( EventArgs e )
		{
			base.OnPreRender( e );
			
			_IsCompatible = this.CheckBrowserCompatibility();

			if ( !_IsCompatible )
				return;

			object oScriptManager = null;

			// Search for the ScriptManager control in the page.
			Control oParent = this.Parent;
			while ( oParent != null )
			{
				foreach ( object control in oParent.Controls )
				{
					// Match by type name.
					if ( control.GetType().FullName == "System.Web.UI.ScriptManager" )
					{
						oScriptManager = control;
						break;
					}
				}

				if ( oScriptManager != null )
					break;

				oParent = oParent.Parent;
			}

			// If the ScriptManager control is available.
			if ( oScriptManager != null )
			{
				try
				{
					// Use reflection to check the SupportsPartialRendering
					// property value.
					bool bSupportsPartialRendering = ((bool)(oScriptManager.GetType().GetProperty( "SupportsPartialRendering" ).GetValue( oScriptManager, null )));

					if ( bSupportsPartialRendering )
					{
						string sScript = "(function()\n{\n" +
							"\tvar editor = FCKeditorAPI.GetInstance('" + this.ClientID + "');\n" +
							"\tif (editor)\n" +
							"\t\teditor.UpdateLinkedField();\n" +
							"})();\n";

						// Call the RegisterOnSubmitStatement method through
						// reflection.
						oScriptManager.GetType().GetMethod( "RegisterOnSubmitStatement", new Type[] { typeof( Control ), typeof( Type ), typeof( String ), typeof( String ) } ).Invoke( oScriptManager, new object[] {
							this,
							this.GetType(),
							"FCKeditorAjaxOnSubmit_" + this.ClientID,
							sScript } );

						// Tell the editor that we are handling the submit.
						this.Config[ "PreventSubmitHandler" ] = "true";
					}
				}
				catch { }
			}
		}

		#endregion

		#region Compatibility Check

		public bool CheckBrowserCompatibility()
		{
			return IsCompatibleBrowser();
		}

		/// <summary>
		/// Checks if the current HTTP request comes from a browser compatible
		/// with FCKeditor.
		/// </summary>
		/// <returns>"true" if the browser is compatible.</returns>
		public static bool IsCompatibleBrowser()
		{
			return IsCompatibleBrowser( System.Web.HttpContext.Current.Request );
		}

		/// <summary>
		/// Checks if the provided HttpRequest object comes from a browser
		/// compatible with FCKeditor.
		/// </summary>
		/// <returns>"true" if the browser is compatible.</returns>
		public static bool IsCompatibleBrowser( System.Web.HttpRequest request )
		{
			System.Web.HttpBrowserCapabilities oBrowser = request.Browser;

			// Internet Explorer 5.5+ for Windows
			if ( oBrowser.Browser == "IE" && ( oBrowser.MajorVersion >= 6 || ( oBrowser.MajorVersion == 5 && oBrowser.MinorVersion >= 0.5 ) ) && oBrowser.Win32 )
				return true;

			string sUserAgent = request.UserAgent;

			if ( sUserAgent.IndexOf( "Gecko/" ) >= 0 )
			{
				Match oMatch = Regex.Match( request.UserAgent, @"(?<=Gecko/)\d{8}" );
				if ( oMatch.Success && int.Parse( oMatch.Value, CultureInfo.InvariantCulture ) >= 20030210 )
                return true;

			    oMatch = Regex.Match(request.UserAgent, @"(?<=Gecko/)\d{2}(?=\.\d+)");
			    return (oMatch.Success && int.Parse(oMatch.Value, CultureInfo.InvariantCulture) >= 17);
			}

			if ( sUserAgent.IndexOf( "Opera/" ) >= 0 )
			{
				Match oMatch = Regex.Match( request.UserAgent, @"(?<=Opera/)[\d\.]+" );
				return ( oMatch.Success && float.Parse( oMatch.Value, CultureInfo.InvariantCulture ) >= 9.5 );
			}

			if ( sUserAgent.IndexOf( "AppleWebKit/" ) >= 0 )
			{
				Match oMatch = Regex.Match( request.UserAgent, @"(?<=AppleWebKit/)\d+" );
				return ( oMatch.Success && int.Parse( oMatch.Value, CultureInfo.InvariantCulture ) >= 522 );
			}

			return false;
		}

		#endregion

		#region Postback Handling

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			string postedValue = postCollection[postDataKey] ;

			// Revert the HtmlEncodeOutput changes.
			if ( this.Config["HtmlEncodeOutput"] != "false" )
			{
				postedValue = postedValue.Replace( "&lt;", "<" ) ;
				postedValue = postedValue.Replace( "&gt;", ">" ) ;
				postedValue = postedValue.Replace( "&amp;", "&" ) ;
			}

			if ( postedValue != this.Value )
			{
				this.Value = postedValue ;
				return true ;
			}
			return false ;
		}

		public void RaisePostDataChangedEvent()
		{
			// Do nothing
		}

		#endregion
	}
}
