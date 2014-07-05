// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

namespace ActiveUp.Net.Mail
{
#region MimeTypesHelper Object
    /// <summary>
    /// Provides static method to help manipulate MIME parts.
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class MimeTypesHelper
    {
        /// <summary>
        /// Returns the know MIME type string based on the extension.
        /// </summary>
        /// <param name="extension">The extension of the file.</param>
        /// <returns>A string that define the MIME type</returns>
        public static string GetMimeqType(string extension)
        {
            switch (extension.TrimStart('.').ToUpper())
            {
                case "DWG":
                    return "application/acad";
                case "ARJ":
                    return "application/arj";
                case "ASD":
                case "ASN":
                    return "application/astound";
                case "CCAD":
                    return "application/clariscad";
                case "DRW":
                    return "application/drafting";
                case "DXF":
                    return "application/dxf";
                case "UNV":
                    return "application/i-deas";
                case "IGES":
                case "IGS":
                    return "application/iges";
                case "JAR":
                    return "application/java-archive";
                case "HQX":
                    return "application/mac-binhex40";
                case "MDB":
                    return "application/msaccess";
                case "XLA":
                case "XLS":
                case "XLT":
                case "XLW":
                    return "application/msexcel";
                case "POT":
                case "PPS":
                case "PPT":
                    return "application/mspowerpoint";
                case "MPP":
                    return "application/msproject";
                case "DOC":
                case "WORD":
                case "W6W":
                    return "application/msword";
                case "WRI":
                    return "application/mswrite";
                case "ODA":
                    return "application/oda";
                case "PDF":
                    return "application/pdf";
                case "AI":
                case "EPS":
                case "PS":
                    return "application/postscript";
                case "PART":
                case "PRT":
                    return "application/pro_eng";
                case "RTF":
                    return "application/rtf";
                case "SET":
                    return "application/set";
                case "STL":
                    return "application/sla";
                case "SOL":
                    return "application/solids";
                case "ST":
                case "STEP":
                case "STP":
                    return "application/STEP";
                case "VDA":
                    return "application/vda";
                case "BCPIO":
                    return "application/x-bcpio";
                case "CPIO":
                    return "application/x-cpio";
                case "CSH":
                    return "application/x-csh";
                case "DCR":
                case "DIR":
                case "DXR":
                    return "application/x-director";
                case "DVI":
                    return "application/x-dvi";
                case "DWF":
                    return "application/x-dwf";
                case "GTAR":
                    return "application/x-gtar";
                case "GZ":
                case "GZIP":
                    return "application/x-gzip";
                case "HDF":
                    return "application/x-hdf";
                case "JS":
                    return "application/x-javascript";
                case "LATEX":
                    return "application/x-latex";
                case "MIF":
                    return "application/x-mif";
                case "CDF":
                case "NC":
                    return "application/x-netcdf";
                case "SH":
                    return "application/x-sh";
                case "SHAR":
                    return "application/x-shar";
                case "SWF":
                    return "application/x-shockwave-flash";
                case "SIT":
                    return "application/x-stuffit";
                case "SV4CPIO":
                    return "application/x-sv4cpio";
                case "SV4CRC":
                    return "application/x-sv4crc";
                case "TAR":
                    return "application/x-tar";
                case "TCL":
                    return "application/x-tcl";
                case "TEX":
                    return "application/x-tex";
                case "TEXI":
                case "TEXINFO":
                    return "application/x-texinfo";
                case "ROFF":
                case "T":
                case "TR":
                    return "application/x-troff";
                case "MAN":
                    return "application/x-troff-man";
                case "ME":
                    return "application/x-troff-me";
                case "MS":
                    return "application/x-troff-ms";
                case "USTAR":
                    return "application/x-ustar";
                case "SRC":
                    return "application/x-wais-source";
                case "HELP":
                case "HLP":
                    return "application/x-winhelp";
                case "ZIP":
                    return "application/zip";
                case "AU":
                case "SND":
                    return "audio/basic";
                case "MID":
                    return "audio/midi";
                case "AIF":
                case "AIFC":
                case "AIFF":
                    return "audio/x-aiff";
                case "MP3":
                case "MP4":
                    return "audio/x-mpeg";
                case "RA":
                case "RAM":
                    return "audio/x-pn-realaudio";
                case "RPM":
                    return "audio/x-pn-realaudio-plugin";
                case "VOC":
                    return "audio/x-voice";
                case "WAV":
                    return "audio/x-wav";
                case "BMP":
                    return "image/bmp";
                case "GIF":
                    return "image/gif";
                case "IEF":
                    return "image/ief";
                case "JPE":
                case "JPEG":
                case "JPG":
                    return "image/jpeg";
                case "PICT":
                    return "image/pict";
                case "PNG":
                    return "image/png";
                case "TIF":
                case "TIFF":
                    return "image/tiff";
                case "RAS":
                    return "image/x-cmu-raster";
                case "PNM":
                    return "image/x-portable-anymap";
                case "PBM":
                    return "image/x-portable-bitmap";
                case "PGM":
                    return "image/x-portable-graymap";
                case "PPM":
                    return "image/x-portable-pixmap";
                case "RGB":
                    return "image/x-rgb";
                case "XBM":
                    return "image/x-xbitmap";
                case "XPM":
                    return "image/x-xpixmap";
                case "XWD":
                    return "image/x-xwindowdump";
                case "HTM":
                case "HTML":
                    return "text/html";
                case "C":
                case "CC":
                case "H":
                case "TXT":
                case "CPP":
                case "CS":
                case "VB":
                case "ASP":
                case "INI":
                    return "text/plain";
                case "RTX":
                    return "text/richtext";
                case "TSV":
                    return "text/tab-separated-values";
                case "ETX":
                    return "text/x-setext";
                case "SGM":
                case "SGML":
                    return "text/x-sgml";
                case "MPE":
                case "MPEG":
                case "MPG":
                    return "video/mpeg";
                case "AVI":
                    return "video/msvideo";
                case "MOV":
                case "QT":
                    return "video/quicktime";
                case "VDO":
                    return "video/vdo";
                case "VIV":
                case "VIVO":
                    return "video/vivo";
                case "MOVIE":
                    return "video/x-sgi-movie";
                case "ICE":
                    return "x-conference/x-cooltalk";
                case "SVR":
                    return "x-world/x-svr";
                case "WRL":
                    return "x-world/x-vrml";
                case "VRT":
                    return "x-world/x-vrt";
                    //case "BIN":
                default:
                    return "application/octet-stream";
            }

        }
    }
    #endregion
}