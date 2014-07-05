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
 * Class that holds all editor configurations.
 */

using System ;
using System.Collections ;
using System.Runtime.Serialization ;

namespace FredCK.FCKeditorV2
{
	[ Serializable() ]
	public class FCKeditorConfigurations : ISerializable
	{
		private Hashtable _Configs ;

		internal FCKeditorConfigurations()
		{
			_Configs = new Hashtable() ;
		}

		protected FCKeditorConfigurations( SerializationInfo info, StreamingContext context )
		{
			_Configs = (Hashtable)info.GetValue( "ConfigTable", typeof( Hashtable ) ) ;
		}

		public string this[ string configurationName ]
		{
			get
			{
				if ( _Configs.ContainsKey( configurationName ) )
					return (string)_Configs[ configurationName ] ;
				else
					return null ;
			}
			set
			{
				_Configs[ configurationName ] = value ;
			}
		}

		internal string GetHiddenFieldString()
		{
			System.Text.StringBuilder osParams = new System.Text.StringBuilder() ;

			foreach ( DictionaryEntry oEntry in _Configs )
			{
				if ( osParams.Length > 0 )
					osParams.Append( "&amp;" ) ;

				osParams.AppendFormat( "{0}={1}", EncodeConfig( oEntry.Key.ToString() ), EncodeConfig( oEntry.Value.ToString() ) ) ;
			}

			// To avoid the "A potentially dangerous Request.Form value was
			// detected from the client" error, forcing developers to set
			// validateRequest=false in their pages, we are forcing
			// HtmlEncodeOutput to "true", if not defined. (#294)
			if ( !_Configs.Contains( "HtmlEncodeOutput" ) )
			{
				if ( osParams.Length > 0 )
					osParams.Append( "&amp;" ) ;

				osParams.Append( "HtmlEncodeOutput=true" ) ;
			}

			return osParams.ToString() ;
		}
		
		private string EncodeConfig( string valueToEncode )
		{
			string sEncoded = valueToEncode.Replace( "&", "%26" ) ;
			sEncoded = sEncoded.Replace( "=", "%3D" ) ;
			sEncoded = sEncoded.Replace( "\"", "%22" ) ;
			
			return sEncoded ;
		}

		#region ISerializable Members

		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue( "ConfigTable", _Configs ) ;
		}

		#endregion
	}
}
