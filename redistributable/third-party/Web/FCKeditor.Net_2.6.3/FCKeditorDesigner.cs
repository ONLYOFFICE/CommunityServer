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
 * The EditorDesigner class defines the editor visualization at design 
 * time. 
 */

using System ;
using System.Globalization ;

namespace FredCK.FCKeditorV2
{
	public class FCKeditorDesigner : System.Web.UI.Design.ControlDesigner
	{
		public FCKeditorDesigner()
		{
		}

		public override string GetDesignTimeHtml() 
		{
			FCKeditor control = (FCKeditor)Component ;
			return String.Format( CultureInfo.InvariantCulture,
				"<div><table width=\"{0}\" height=\"{1}\" bgcolor=\"#f5f5f5\" bordercolor=\"#c7c7c7\" cellpadding=\"0\" cellspacing=\"0\" border=\"1\"><tr><td valign=\"middle\" align=\"center\">FCKeditor V2 - <b>{2}</b></td></tr></table></div>",
					control.Width,
					control.Height,
					control.ID ) ;
		}
	}
}
