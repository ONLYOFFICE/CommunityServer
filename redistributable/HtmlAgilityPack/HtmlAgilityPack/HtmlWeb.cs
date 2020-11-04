// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: http://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright © ZZZ Projects Inc. 2014 - 2017. All rights reserved.

#if !METRO
#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
#if !NETSTANDARD
using System.Security.Permissions;
#else
using System.Linq;
#endif
using System.Text;
using System.Xml;
using Microsoft.Win32;
#if NET45 || NETSTANDARD
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
#endif

#if FX40 || FX45
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
#endif

#endregion

namespace HtmlAgilityPack
{
    /// <summary>
    /// A utility class to get HTML document from HTTP.
    /// </summary>
    public partial class HtmlWeb
    {
        #region Delegates

#if !NETSTANDARD
/// <summary>
/// Represents the method that will handle the PostResponse event.
/// </summary>
        public delegate void PostResponseHandler(HttpWebRequest request, HttpWebResponse response);
#endif

#if NET45 || NETSTANDARD
        /// <summary>
        /// Represents the method that will handle the PostResponse event.
        /// </summary>
        public delegate void PostResponseHandler(HttpRequestMessage request, HttpResponseMessage response);
#endif
        /// <summary>
        /// Represents the method that will handle the PreHandleDocument event.
        /// </summary>
        public delegate void PreHandleDocumentHandler(HtmlDocument document);

#if !NETSTANDARD
/// <summary>
/// Represents the method that will handle the PreRequest event.
/// </summary>
        public delegate bool PreRequestHandler(HttpWebRequest request);
#endif
#if NET45 || NETSTANDARD
        /// <summary>
        /// Represents the method that will handle the PostResponse event.
        /// </summary>
        public delegate bool PreRequestHandler(HttpClientHandler handler, HttpRequestMessage request);
#endif

        #endregion

        #region Fields

        private bool _autoDetectEncoding = true;
        private bool _cacheOnly;

        private string _cachePath;
        private bool _fromCache;
        private int _requestDuration;
        private Uri _responseUri;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private int _streamBufferSize = 1024;
        private bool _useCookies;
        private bool _usingCache;
        private bool _usingCacheAndLoad;
        private string _userAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:x.x.x) Gecko/20041107 Firefox/x.x";

        /// <summary>
        /// Occurs after an HTTP request has been executed.
        /// </summary>
        public PostResponseHandler PostResponse;

        /// <summary>
        /// Occurs before an HTML document is handled.
        /// </summary>
        public PreHandleDocumentHandler PreHandleDocument;

        /// <summary>
        /// Occurs before an HTTP request is executed.
        /// </summary>
        public PreRequestHandler PreRequest;


        #endregion

        #region Static Members

        private static Dictionary<string, string> _mimeTypes;

        internal static Dictionary<string, string> MimeTypes
        {
            get
            {
                if (_mimeTypes != null)
                    return _mimeTypes;
                //agentsmith spellcheck disable
                _mimeTypes = new Dictionary<string, string>();
                _mimeTypes.Add(".3dm", "x-world/x-3dmf");
                _mimeTypes.Add(".3dmf", "x-world/x-3dmf");
                _mimeTypes.Add(".a", "application/octet-stream");
                _mimeTypes.Add(".aab", "application/x-authorware-bin");
                _mimeTypes.Add(".aam", "application/x-authorware-map");
                _mimeTypes.Add(".aas", "application/x-authorware-seg");
                _mimeTypes.Add(".abc", "text/vnd.abc");
                _mimeTypes.Add(".acgi", "text/html");
                _mimeTypes.Add(".afl", "video/animaflex");
                _mimeTypes.Add(".ai", "application/postscript");
                _mimeTypes.Add(".aif", "audio/aiff");
                _mimeTypes.Add(".aif", "audio/x-aiff");
                _mimeTypes.Add(".aifc", "audio/aiff");
                _mimeTypes.Add(".aifc", "audio/x-aiff");
                _mimeTypes.Add(".aiff", "audio/aiff");
                _mimeTypes.Add(".aiff", "audio/x-aiff");
                _mimeTypes.Add(".aim", "application/x-aim");
                _mimeTypes.Add(".aip", "text/x-audiosoft-intra");
                _mimeTypes.Add(".ani", "application/x-navi-animation");
                _mimeTypes.Add(".aos", "application/x-nokia-9000-communicator-add-on-software");
                _mimeTypes.Add(".aps", "application/mime");
                _mimeTypes.Add(".arc", "application/octet-stream");
                _mimeTypes.Add(".arj", "application/arj");
                _mimeTypes.Add(".arj", "application/octet-stream");
                _mimeTypes.Add(".art", "image/x-jg");
                _mimeTypes.Add(".asf", "video/x-ms-asf");
                _mimeTypes.Add(".asm", "text/x-asm");
                _mimeTypes.Add(".asp", "text/asp");
                _mimeTypes.Add(".asx", "application/x-mplayer2");
                _mimeTypes.Add(".asx", "video/x-ms-asf");
                _mimeTypes.Add(".asx", "video/x-ms-asf-plugin");
                _mimeTypes.Add(".au", "audio/basic");
                _mimeTypes.Add(".au", "audio/x-au");
                _mimeTypes.Add(".avi", "application/x-troff-msvideo");
                _mimeTypes.Add(".avi", "video/avi");
                _mimeTypes.Add(".avi", "video/msvideo");
                _mimeTypes.Add(".avi", "video/x-msvideo");
                _mimeTypes.Add(".avs", "video/avs-video");
                _mimeTypes.Add(".bcpio", "application/x-bcpio");
                _mimeTypes.Add(".bin", "application/mac-binary");
                _mimeTypes.Add(".bin", "application/macbinary");
                _mimeTypes.Add(".bin", "application/octet-stream");
                _mimeTypes.Add(".bin", "application/x-binary");
                _mimeTypes.Add(".bin", "application/x-macbinary");
                _mimeTypes.Add(".bm", "image/bmp");
                _mimeTypes.Add(".bmp", "image/bmp");
                _mimeTypes.Add(".bmp", "image/x-windows-bmp");
                _mimeTypes.Add(".boo", "application/book");
                _mimeTypes.Add(".book", "application/book");
                _mimeTypes.Add(".boz", "application/x-bzip2");
                _mimeTypes.Add(".bsh", "application/x-bsh");
                _mimeTypes.Add(".bz", "application/x-bzip");
                _mimeTypes.Add(".bz2", "application/x-bzip2");
                _mimeTypes.Add(".c", "text/plain");
                _mimeTypes.Add(".c", "text/x-c");
                _mimeTypes.Add(".c++", "text/plain");
                _mimeTypes.Add(".cat", "application/vnd.ms-pki.seccat");
                _mimeTypes.Add(".cc", "text/plain");
                _mimeTypes.Add(".cc", "text/x-c");
                _mimeTypes.Add(".ccad", "application/clariscad");
                _mimeTypes.Add(".cco", "application/x-cocoa");
                _mimeTypes.Add(".cdf", "application/cdf");
                _mimeTypes.Add(".cdf", "application/x-cdf");
                _mimeTypes.Add(".cdf", "application/x-netcdf");
                _mimeTypes.Add(".cer", "application/pkix-cert");
                _mimeTypes.Add(".cer", "application/x-x509-ca-cert");
                _mimeTypes.Add(".cha", "application/x-chat");
                _mimeTypes.Add(".chat", "application/x-chat");
                _mimeTypes.Add(".class", "application/java");
                _mimeTypes.Add(".class", "application/java-byte-code");
                _mimeTypes.Add(".class", "application/x-java-class");
                _mimeTypes.Add(".com", "application/octet-stream");
                _mimeTypes.Add(".com", "text/plain");
                _mimeTypes.Add(".conf", "text/plain");
                _mimeTypes.Add(".cpio", "application/x-cpio");
                _mimeTypes.Add(".cpp", "text/x-c");
                _mimeTypes.Add(".cpt", "application/mac-compactpro");
                _mimeTypes.Add(".cpt", "application/x-compactpro");
                _mimeTypes.Add(".cpt", "application/x-cpt");
                _mimeTypes.Add(".crl", "application/pkcs-crl");
                _mimeTypes.Add(".crl", "application/pkix-crl");
                _mimeTypes.Add(".crt", "application/pkix-cert");
                _mimeTypes.Add(".crt", "application/x-x509-ca-cert");
                _mimeTypes.Add(".crt", "application/x-x509-user-cert");
                _mimeTypes.Add(".csh", "application/x-csh");
                _mimeTypes.Add(".csh", "text/x-script.csh");
                _mimeTypes.Add(".css", "application/x-pointplus");
                _mimeTypes.Add(".css", "text/css");
                _mimeTypes.Add(".cxx", "text/plain");
                _mimeTypes.Add(".dcr", "application/x-director");
                _mimeTypes.Add(".deepv", "application/x-deepv");
                _mimeTypes.Add(".def", "text/plain");
                _mimeTypes.Add(".der", "application/x-x509-ca-cert");
                _mimeTypes.Add(".dif", "video/x-dv");
                _mimeTypes.Add(".dir", "application/x-director");
                _mimeTypes.Add(".dl", "video/dl");
                _mimeTypes.Add(".dl", "video/x-dl");
                _mimeTypes.Add(".doc", "application/msword");
                _mimeTypes.Add(".dot", "application/msword");
                _mimeTypes.Add(".dp", "application/commonground");
                _mimeTypes.Add(".drw", "application/drafting");
                _mimeTypes.Add(".dump", "application/octet-stream");
                _mimeTypes.Add(".dv", "video/x-dv");
                _mimeTypes.Add(".dvi", "application/x-dvi");
                _mimeTypes.Add(".dwf", "model/vnd.dwf");
                _mimeTypes.Add(".dwg", "application/acad");
                _mimeTypes.Add(".dwg", "image/vnd.dwg");
                _mimeTypes.Add(".dwg", "image/x-dwg");
                _mimeTypes.Add(".dxf", "application/dxf");
                _mimeTypes.Add(".dxf", "image/vnd.dwg");
                _mimeTypes.Add(".dxf", "image/x-dwg");
                _mimeTypes.Add(".dxr", "application/x-director");
                _mimeTypes.Add(".el", "text/x-script.elisp");
                _mimeTypes.Add(".elc", "application/x-bytecode.elisp");
                _mimeTypes.Add(".elc", "application/x-elc");
                _mimeTypes.Add(".env", "application/x-envoy");
                _mimeTypes.Add(".eps", "application/postscript");
                _mimeTypes.Add(".es", "application/x-esrehber");
                _mimeTypes.Add(".etx", "text/x-setext");
                _mimeTypes.Add(".evy", "application/envoy");
                _mimeTypes.Add(".evy", "application/x-envoy");
                _mimeTypes.Add(".exe", "application/octet-stream");
                _mimeTypes.Add(".f", "text/plain");
                _mimeTypes.Add(".f", "text/x-fortran");
                _mimeTypes.Add(".f77", "text/x-fortran");
                _mimeTypes.Add(".f90", "text/plain");
                _mimeTypes.Add(".f90", "text/x-fortran");
                _mimeTypes.Add(".fdf", "application/vnd.fdf");
                _mimeTypes.Add(".fif", "application/fractals");
                _mimeTypes.Add(".fif", "image/fif");
                _mimeTypes.Add(".fli", "video/fli");
                _mimeTypes.Add(".fli", "video/x-fli");
                _mimeTypes.Add(".flo", "image/florian");
                _mimeTypes.Add(".flx", "text/vnd.fmi.flexstor");
                _mimeTypes.Add(".fmf", "video/x-atomic3d-feature");
                _mimeTypes.Add(".for", "text/plain");
                _mimeTypes.Add(".for", "text/x-fortran");
                _mimeTypes.Add(".fpx", "image/vnd.fpx");
                _mimeTypes.Add(".fpx", "image/vnd.net-fpx");
                _mimeTypes.Add(".frl", "application/freeloader");
                _mimeTypes.Add(".funk", "audio/make");
                _mimeTypes.Add(".g", "text/plain");
                _mimeTypes.Add(".g3", "image/g3fax");
                _mimeTypes.Add(".gif", "image/gif");
                _mimeTypes.Add(".gl", "video/gl");
                _mimeTypes.Add(".gl", "video/x-gl");
                _mimeTypes.Add(".gsd", "audio/x-gsm");
                _mimeTypes.Add(".gsm", "audio/x-gsm");
                _mimeTypes.Add(".gsp", "application/x-gsp");
                _mimeTypes.Add(".gss", "application/x-gss");
                _mimeTypes.Add(".gtar", "application/x-gtar");
                _mimeTypes.Add(".gz", "application/x-compressed");
                _mimeTypes.Add(".gz", "application/x-gzip");
                _mimeTypes.Add(".gzip", "application/x-gzip");
                _mimeTypes.Add(".gzip", "multipart/x-gzip");
                _mimeTypes.Add(".h", "text/plain");
                _mimeTypes.Add(".h", "text/x-h");
                _mimeTypes.Add(".hdf", "application/x-hdf");
                _mimeTypes.Add(".help", "application/x-helpfile");
                _mimeTypes.Add(".hgl", "application/vnd.hp-hpgl");
                _mimeTypes.Add(".hh", "text/plain");
                _mimeTypes.Add(".hh", "text/x-h");
                _mimeTypes.Add(".hlb", "text/x-script");
                _mimeTypes.Add(".hlp", "application/hlp");
                _mimeTypes.Add(".hlp", "application/x-helpfile");
                _mimeTypes.Add(".hlp", "application/x-winhelp");
                _mimeTypes.Add(".hpg", "application/vnd.hp-hpgl");
                _mimeTypes.Add(".hpgl", "application/vnd.hp-hpgl");
                _mimeTypes.Add(".hqx", "application/binhex");
                _mimeTypes.Add(".hqx", "application/binhex4");
                _mimeTypes.Add(".hqx", "application/mac-binhex");
                _mimeTypes.Add(".hqx", "application/mac-binhex40");
                _mimeTypes.Add(".hqx", "application/x-binhex40");
                _mimeTypes.Add(".hqx", "application/x-mac-binhex40");
                _mimeTypes.Add(".hta", "application/hta");
                _mimeTypes.Add(".htc", "text/x-component");
                _mimeTypes.Add(".htm", "text/html");
                _mimeTypes.Add(".html", "text/html");
                _mimeTypes.Add(".htmls", "text/html");
                _mimeTypes.Add(".htt", "text/webviewhtml");
                _mimeTypes.Add(".htx", "text/html");
                _mimeTypes.Add(".ice", "x-conference/x-cooltalk");
                _mimeTypes.Add(".ico", "image/x-icon");
                _mimeTypes.Add(".idc", "text/plain");
                _mimeTypes.Add(".ief", "image/ief");
                _mimeTypes.Add(".iefs", "image/ief");
                _mimeTypes.Add(".iges", "application/iges");
                _mimeTypes.Add(".iges", "model/iges");
                _mimeTypes.Add(".igs", "application/iges");
                _mimeTypes.Add(".igs", "model/iges");
                _mimeTypes.Add(".ima", "application/x-ima");
                _mimeTypes.Add(".imap", "application/x-httpd-imap");
                _mimeTypes.Add(".inf", "application/inf");
                _mimeTypes.Add(".ins", "application/x-internett-signup");
                _mimeTypes.Add(".ip", "application/x-ip2");
                _mimeTypes.Add(".isu", "video/x-isvideo");
                _mimeTypes.Add(".it", "audio/it");
                _mimeTypes.Add(".iv", "application/x-inventor");
                _mimeTypes.Add(".ivr", "i-world/i-vrml");
                _mimeTypes.Add(".ivy", "application/x-livescreen");
                _mimeTypes.Add(".jam", "audio/x-jam");
                _mimeTypes.Add(".jav", "text/plain");
                _mimeTypes.Add(".jav", "text/x-java-source");
                _mimeTypes.Add(".java", "text/plain");
                _mimeTypes.Add(".java", "text/x-java-source");
                _mimeTypes.Add(".jcm", "application/x-java-commerce");
                _mimeTypes.Add(".jfif", "image/jpeg");
                _mimeTypes.Add(".jfif", "image/pjpeg");
                _mimeTypes.Add(".jfif-tbnl", "image/jpeg");
                _mimeTypes.Add(".jpe", "image/jpeg");
                _mimeTypes.Add(".jpe", "image/pjpeg");
                _mimeTypes.Add(".jpeg", "image/jpeg");
                _mimeTypes.Add(".jpeg", "image/pjpeg");
                _mimeTypes.Add(".jpg", "image/jpeg");
                _mimeTypes.Add(".jpg", "image/pjpeg");
                _mimeTypes.Add(".jps", "image/x-jps");
                _mimeTypes.Add(".js", "application/x-javascript");
                _mimeTypes.Add(".js", "application/javascript");
                _mimeTypes.Add(".js", "application/ecmascript");
                _mimeTypes.Add(".js", "text/javascript");
                _mimeTypes.Add(".js", "text/ecmascript");
                _mimeTypes.Add(".jut", "image/jutvision");
                _mimeTypes.Add(".kar", "audio/midi");
                _mimeTypes.Add(".kar", "music/x-karaoke");
                _mimeTypes.Add(".ksh", "application/x-ksh");
                _mimeTypes.Add(".ksh", "text/x-script.ksh");
                _mimeTypes.Add(".la", "audio/nspaudio");
                _mimeTypes.Add(".la", "audio/x-nspaudio");
                _mimeTypes.Add(".lam", "audio/x-liveaudio");
                _mimeTypes.Add(".latex", "application/x-latex");
                _mimeTypes.Add(".lha", "application/lha");
                _mimeTypes.Add(".lha", "application/octet-stream");
                _mimeTypes.Add(".lha", "application/x-lha");
                _mimeTypes.Add(".lhx", "application/octet-stream");
                _mimeTypes.Add(".list", "text/plain");
                _mimeTypes.Add(".lma", "audio/nspaudio");
                _mimeTypes.Add(".lma", "audio/x-nspaudio");
                _mimeTypes.Add(".log", "text/plain");
                _mimeTypes.Add(".lsp", "application/x-lisp");
                _mimeTypes.Add(".lsp", "text/x-script.lisp");
                _mimeTypes.Add(".lst", "text/plain");
                _mimeTypes.Add(".lsx", "text/x-la-asf");
                _mimeTypes.Add(".ltx", "application/x-latex");
                _mimeTypes.Add(".lzh", "application/octet-stream");
                _mimeTypes.Add(".lzh", "application/x-lzh");
                _mimeTypes.Add(".lzx", "application/lzx");
                _mimeTypes.Add(".lzx", "application/octet-stream");
                _mimeTypes.Add(".lzx", "application/x-lzx");
                _mimeTypes.Add(".m", "text/plain");
                _mimeTypes.Add(".m", "text/x-m");
                _mimeTypes.Add(".m1v", "video/mpeg");
                _mimeTypes.Add(".m2a", "audio/mpeg");
                _mimeTypes.Add(".m2v", "video/mpeg");
                _mimeTypes.Add(".m3u", "audio/x-mpequrl");
                _mimeTypes.Add(".man", "application/x-troff-man");
                _mimeTypes.Add(".map", "application/x-navimap");
                _mimeTypes.Add(".mar", "text/plain");
                _mimeTypes.Add(".mbd", "application/mbedlet");
                _mimeTypes.Add(".mc$", "application/x-magic-cap-package-1.0");
                _mimeTypes.Add(".mcd", "application/mcad");
                _mimeTypes.Add(".mcd", "application/x-mathcad");
                _mimeTypes.Add(".mcf", "image/vasa");
                _mimeTypes.Add(".mcf", "text/mcf");
                _mimeTypes.Add(".mcp", "application/netmc");
                _mimeTypes.Add(".me", "application/x-troff-me");
                _mimeTypes.Add(".mht", "message/rfc822");
                _mimeTypes.Add(".mhtml", "message/rfc822");
                _mimeTypes.Add(".mid", "application/x-midi");
                _mimeTypes.Add(".mid", "audio/midi");
                _mimeTypes.Add(".mid", "audio/x-mid");
                _mimeTypes.Add(".mid", "audio/x-midi");
                _mimeTypes.Add(".mid", "music/crescendo");
                _mimeTypes.Add(".mid", "x-music/x-midi");
                _mimeTypes.Add(".midi", "application/x-midi");
                _mimeTypes.Add(".midi", "audio/midi");
                _mimeTypes.Add(".midi", "audio/x-mid");
                _mimeTypes.Add(".midi", "audio/x-midi");
                _mimeTypes.Add(".midi", "music/crescendo");
                _mimeTypes.Add(".midi", "x-music/x-midi");
                _mimeTypes.Add(".mif", "application/x-frame");
                _mimeTypes.Add(".mif", "application/x-mif");
                _mimeTypes.Add(".mime", "message/rfc822");
                _mimeTypes.Add(".mime", "www/mime");
                _mimeTypes.Add(".mjf", "audio/x-vnd.audioexplosion.mjuicemediafile");
                _mimeTypes.Add(".mjpg", "video/x-motion-jpeg");
                _mimeTypes.Add(".mm", "application/base64");
                _mimeTypes.Add(".mm", "application/x-meme");
                _mimeTypes.Add(".mme", "application/base64");
                _mimeTypes.Add(".mod", "audio/mod");
                _mimeTypes.Add(".mod", "audio/x-mod");
                _mimeTypes.Add(".moov", "video/quicktime");
                _mimeTypes.Add(".mov", "video/quicktime");
                _mimeTypes.Add(".movie", "video/x-sgi-movie");
                _mimeTypes.Add(".mp2", "audio/mpeg");
                _mimeTypes.Add(".mp2", "audio/x-mpeg");
                _mimeTypes.Add(".mp2", "video/mpeg");
                _mimeTypes.Add(".mp2", "video/x-mpeg");
                _mimeTypes.Add(".mp2", "video/x-mpeq2a");
                _mimeTypes.Add(".mp3", "audio/mpeg3");
                _mimeTypes.Add(".mp3", "audio/x-mpeg-3");
                _mimeTypes.Add(".mp3", "video/mpeg");
                _mimeTypes.Add(".mp3", "video/x-mpeg");
                _mimeTypes.Add(".mpa", "audio/mpeg");
                _mimeTypes.Add(".mpa", "video/mpeg");
                _mimeTypes.Add(".mpc", "application/x-project");
                _mimeTypes.Add(".mpe", "video/mpeg");
                _mimeTypes.Add(".mpeg", "video/mpeg");
                _mimeTypes.Add(".mpg", "audio/mpeg");
                _mimeTypes.Add(".mpg", "video/mpeg");
                _mimeTypes.Add(".mpga", "audio/mpeg");
                _mimeTypes.Add(".mpp", "application/vnd.ms-project");
                _mimeTypes.Add(".mpt", "application/x-project");
                _mimeTypes.Add(".mpv", "application/x-project");
                _mimeTypes.Add(".mpx", "application/x-project");
                _mimeTypes.Add(".mrc", "application/marc");
                _mimeTypes.Add(".ms", "application/x-troff-ms");
                _mimeTypes.Add(".mv", "video/x-sgi-movie");
                _mimeTypes.Add(".my", "audio/make");
                _mimeTypes.Add(".mzz", "application/x-vnd.audioexplosion.mzz");
                _mimeTypes.Add(".nap", "image/naplps");
                _mimeTypes.Add(".naplps", "image/naplps");
                _mimeTypes.Add(".nc", "application/x-netcdf");
                _mimeTypes.Add(".ncm", "application/vnd.nokia.configuration-message");
                _mimeTypes.Add(".nif", "image/x-niff");
                _mimeTypes.Add(".niff", "image/x-niff");
                _mimeTypes.Add(".nix", "application/x-mix-transfer");
                _mimeTypes.Add(".nsc", "application/x-conference");
                _mimeTypes.Add(".nvd", "application/x-navidoc");
                _mimeTypes.Add(".o", "application/octet-stream");
                _mimeTypes.Add(".oda", "application/oda");
                _mimeTypes.Add(".omc", "application/x-omc");
                _mimeTypes.Add(".omcd", "application/x-omcdatamaker");
                _mimeTypes.Add(".omcr", "application/x-omcregerator");
                _mimeTypes.Add(".p", "text/x-pascal");
                _mimeTypes.Add(".p10", "application/pkcs10");
                _mimeTypes.Add(".p10", "application/x-pkcs10");
                _mimeTypes.Add(".p12", "application/pkcs-12");
                _mimeTypes.Add(".p12", "application/x-pkcs12");
                _mimeTypes.Add(".p7a", "application/x-pkcs7-signature");
                _mimeTypes.Add(".p7c", "application/pkcs7-mime");
                _mimeTypes.Add(".p7c", "application/x-pkcs7-mime");
                _mimeTypes.Add(".p7m", "application/pkcs7-mime");
                _mimeTypes.Add(".p7m", "application/x-pkcs7-mime");
                _mimeTypes.Add(".p7r", "application/x-pkcs7-certreqresp");
                _mimeTypes.Add(".p7s", "application/pkcs7-signature");
                _mimeTypes.Add(".part", "application/pro_eng");
                _mimeTypes.Add(".pas", "text/pascal");
                _mimeTypes.Add(".pbm", "image/x-portable-bitmap");
                _mimeTypes.Add(".pcl", "application/vnd.hp-pcl");
                _mimeTypes.Add(".pcl", "application/x-pcl");
                _mimeTypes.Add(".pct", "image/x-pict");
                _mimeTypes.Add(".pcx", "image/x-pcx");
                _mimeTypes.Add(".pdb", "chemical/x-pdb");
                _mimeTypes.Add(".pdf", "application/pdf");
                _mimeTypes.Add(".pfunk", "audio/make");
                _mimeTypes.Add(".pfunk", "audio/make.my.funk");
                _mimeTypes.Add(".pgm", "image/x-portable-graymap");
                _mimeTypes.Add(".pgm", "image/x-portable-greymap");
                _mimeTypes.Add(".pic", "image/pict");
                _mimeTypes.Add(".pict", "image/pict");
                _mimeTypes.Add(".pkg", "application/x-newton-compatible-pkg");
                _mimeTypes.Add(".pko", "application/vnd.ms-pki.pko");
                _mimeTypes.Add(".pl", "text/plain");
                _mimeTypes.Add(".pl", "text/x-script.perl");
                _mimeTypes.Add(".plx", "application/x-pixclscript");
                _mimeTypes.Add(".pm", "image/x-xpixmap");
                _mimeTypes.Add(".pm", "text/x-script.perl-module");
                _mimeTypes.Add(".pm4", "application/x-pagemaker");
                _mimeTypes.Add(".pm5", "application/x-pagemaker");
                _mimeTypes.Add(".png", "image/png");
                _mimeTypes.Add(".pnm", "application/x-portable-anymap");
                _mimeTypes.Add(".pnm", "image/x-portable-anymap");
                _mimeTypes.Add(".pot", "application/mspowerpoint");
                _mimeTypes.Add(".pot", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".pov", "model/x-pov");
                _mimeTypes.Add(".ppa", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".ppm", "image/x-portable-pixmap");
                _mimeTypes.Add(".pps", "application/mspowerpoint");
                _mimeTypes.Add(".pps", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".ppt", "application/mspowerpoint");
                _mimeTypes.Add(".ppt", "application/powerpoint");
                _mimeTypes.Add(".ppt", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".ppt", "application/x-mspowerpoint");
                _mimeTypes.Add(".ppz", "application/mspowerpoint");
                _mimeTypes.Add(".pre", "application/x-freelance");
                _mimeTypes.Add(".prt", "application/pro_eng");
                _mimeTypes.Add(".ps", "application/postscript");
                _mimeTypes.Add(".psd", "application/octet-stream");
                _mimeTypes.Add(".pvu", "paleovu/x-pv");
                _mimeTypes.Add(".pwz", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".py", "text/x-script.phyton");
                _mimeTypes.Add(".pyc", "applicaiton/x-bytecode.python");
                _mimeTypes.Add(".qcp", "audio/vnd.qcelp");
                _mimeTypes.Add(".qd3", "x-world/x-3dmf");
                _mimeTypes.Add(".qd3d", "x-world/x-3dmf");
                _mimeTypes.Add(".qif", "image/x-quicktime");
                _mimeTypes.Add(".qt", "video/quicktime");
                _mimeTypes.Add(".qtc", "video/x-qtc");
                _mimeTypes.Add(".qti", "image/x-quicktime");
                _mimeTypes.Add(".qtif", "image/x-quicktime");
                _mimeTypes.Add(".ra", "audio/x-pn-realaudio");
                _mimeTypes.Add(".ra", "audio/x-pn-realaudio-plugin");
                _mimeTypes.Add(".ra", "audio/x-realaudio");
                _mimeTypes.Add(".ram", "audio/x-pn-realaudio");
                _mimeTypes.Add(".ras", "application/x-cmu-raster");
                _mimeTypes.Add(".ras", "image/cmu-raster");
                _mimeTypes.Add(".ras", "image/x-cmu-raster");
                _mimeTypes.Add(".rast", "image/cmu-raster");
                _mimeTypes.Add(".rexx", "text/x-script.rexx");
                _mimeTypes.Add(".rf", "image/vnd.rn-realflash");
                _mimeTypes.Add(".rgb", "image/x-rgb");
                _mimeTypes.Add(".rm", "application/vnd.rn-realmedia");
                _mimeTypes.Add(".rm", "audio/x-pn-realaudio");
                _mimeTypes.Add(".rmi", "audio/mid");
                _mimeTypes.Add(".rmm", "audio/x-pn-realaudio");
                _mimeTypes.Add(".rmp", "audio/x-pn-realaudio");
                _mimeTypes.Add(".rmp", "audio/x-pn-realaudio-plugin");
                _mimeTypes.Add(".rng", "application/ringing-tones");
                _mimeTypes.Add(".rng", "application/vnd.nokia.ringing-tone");
                _mimeTypes.Add(".rnx", "application/vnd.rn-realplayer");
                _mimeTypes.Add(".roff", "application/x-troff");
                _mimeTypes.Add(".rp", "image/vnd.rn-realpix");
                _mimeTypes.Add(".rpm", "audio/x-pn-realaudio-plugin");
                _mimeTypes.Add(".rt", "text/richtext");
                _mimeTypes.Add(".rt", "text/vnd.rn-realtext");
                _mimeTypes.Add(".rtf", "application/rtf");
                _mimeTypes.Add(".rtf", "application/x-rtf");
                _mimeTypes.Add(".rtf", "text/richtext");
                _mimeTypes.Add(".rtx", "application/rtf");
                _mimeTypes.Add(".rtx", "text/richtext");
                _mimeTypes.Add(".rv", "video/vnd.rn-realvideo");
                _mimeTypes.Add(".s", "text/x-asm");
                _mimeTypes.Add(".s3m", "audio/s3m");
                _mimeTypes.Add(".saveme", "application/octet-stream");
                _mimeTypes.Add(".sbk", "application/x-tbook");
                _mimeTypes.Add(".scm", "application/x-lotusscreencam");
                _mimeTypes.Add(".scm", "text/x-script.guile");
                _mimeTypes.Add(".scm", "text/x-script.scheme");
                _mimeTypes.Add(".scm", "video/x-scm");
                _mimeTypes.Add(".sdml", "text/plain");
                _mimeTypes.Add(".sdp", "application/sdp");
                _mimeTypes.Add(".sdp", "application/x-sdp");
                _mimeTypes.Add(".sdr", "application/sounder");
                _mimeTypes.Add(".sea", "application/sea");
                _mimeTypes.Add(".sea", "application/x-sea");
                _mimeTypes.Add(".set", "application/set");
                _mimeTypes.Add(".sgm", "text/sgml");
                _mimeTypes.Add(".sgm", "text/x-sgml");
                _mimeTypes.Add(".sgml", "text/sgml");
                _mimeTypes.Add(".sgml", "text/x-sgml");
                _mimeTypes.Add(".sh", "application/x-bsh");
                _mimeTypes.Add(".sh", "application/x-sh");
                _mimeTypes.Add(".sh", "application/x-shar");
                _mimeTypes.Add(".sh", "text/x-script.sh");
                _mimeTypes.Add(".shar", "application/x-bsh");
                _mimeTypes.Add(".shar", "application/x-shar");
                _mimeTypes.Add(".shtml", "text/html");
                _mimeTypes.Add(".shtml", "text/x-server-parsed-html");
                _mimeTypes.Add(".sid", "audio/x-psid");
                _mimeTypes.Add(".sit", "application/x-sit");
                _mimeTypes.Add(".sit", "application/x-stuffit");
                _mimeTypes.Add(".skd", "application/x-koan");
                _mimeTypes.Add(".skm", "application/x-koan");
                _mimeTypes.Add(".skp", "application/x-koan");
                _mimeTypes.Add(".skt", "application/x-koan");
                _mimeTypes.Add(".sl", "application/x-seelogo");
                _mimeTypes.Add(".smi", "application/smil");
                _mimeTypes.Add(".smil", "application/smil");
                _mimeTypes.Add(".snd", "audio/basic");
                _mimeTypes.Add(".snd", "audio/x-adpcm");
                _mimeTypes.Add(".sol", "application/solids");
                _mimeTypes.Add(".spc", "application/x-pkcs7-certificates");
                _mimeTypes.Add(".spc", "text/x-speech");
                _mimeTypes.Add(".spl", "application/futuresplash");
                _mimeTypes.Add(".spr", "application/x-sprite");
                _mimeTypes.Add(".sprite", "application/x-sprite");
                _mimeTypes.Add(".src", "application/x-wais-source");
                _mimeTypes.Add(".ssi", "text/x-server-parsed-html");
                _mimeTypes.Add(".ssm", "application/streamingmedia");
                _mimeTypes.Add(".sst", "application/vnd.ms-pki.certstore");
                _mimeTypes.Add(".step", "application/step");
                _mimeTypes.Add(".stl", "application/sla");
                _mimeTypes.Add(".stl", "application/vnd.ms-pki.stl");
                _mimeTypes.Add(".stl", "application/x-navistyle");
                _mimeTypes.Add(".stp", "application/step");
                _mimeTypes.Add(".sv4cpio", "application/x-sv4cpio");
                _mimeTypes.Add(".sv4crc", "application/x-sv4crc");
                _mimeTypes.Add(".svf", "image/vnd.dwg");
                _mimeTypes.Add(".svf", "image/x-dwg");
                _mimeTypes.Add(".svr", "application/x-world");
                _mimeTypes.Add(".svr", "x-world/x-svr");
                _mimeTypes.Add(".swf", "application/x-shockwave-flash");
                _mimeTypes.Add(".t", "application/x-troff");
                _mimeTypes.Add(".talk", "text/x-speech");
                _mimeTypes.Add(".tar", "application/x-tar");
                _mimeTypes.Add(".tbk", "application/toolbook");
                _mimeTypes.Add(".tbk", "application/x-tbook");
                _mimeTypes.Add(".tcl", "application/x-tcl");
                _mimeTypes.Add(".tcl", "text/x-script.tcl");
                _mimeTypes.Add(".tcsh", "text/x-script.tcsh");
                _mimeTypes.Add(".tex", "application/x-tex");
                _mimeTypes.Add(".texi", "application/x-texinfo");
                _mimeTypes.Add(".texinfo", "application/x-texinfo");
                _mimeTypes.Add(".text", "application/plain");
                _mimeTypes.Add(".text", "text/plain");
                _mimeTypes.Add(".tgz", "application/gnutar");
                _mimeTypes.Add(".tgz", "application/x-compressed");
                _mimeTypes.Add(".tif", "image/tiff");
                _mimeTypes.Add(".tif", "image/x-tiff");
                _mimeTypes.Add(".tiff", "image/tiff");
                _mimeTypes.Add(".tiff", "image/x-tiff");
                _mimeTypes.Add(".tr", "application/x-troff");
                _mimeTypes.Add(".tsi", "audio/tsp-audio");
                _mimeTypes.Add(".tsp", "application/dsptype");
                _mimeTypes.Add(".tsp", "audio/tsplayer");
                _mimeTypes.Add(".tsv", "text/tab-separated-values");
                _mimeTypes.Add(".turbot", "image/florian");
                _mimeTypes.Add(".txt", "text/plain");
                _mimeTypes.Add(".uil", "text/x-uil");
                _mimeTypes.Add(".uni", "text/uri-list");
                _mimeTypes.Add(".unis", "text/uri-list");
                _mimeTypes.Add(".unv", "application/i-deas");
                _mimeTypes.Add(".uri", "text/uri-list");
                _mimeTypes.Add(".uris", "text/uri-list");
                _mimeTypes.Add(".ustar", "application/x-ustar");
                _mimeTypes.Add(".ustar", "multipart/x-ustar");
                _mimeTypes.Add(".uu", "application/octet-stream");
                _mimeTypes.Add(".uu", "text/x-uuencode");
                _mimeTypes.Add(".uue", "text/x-uuencode");
                _mimeTypes.Add(".vcd", "application/x-cdlink");
                _mimeTypes.Add(".vcs", "text/x-vcalendar");
                _mimeTypes.Add(".vda", "application/vda");
                _mimeTypes.Add(".vdo", "video/vdo");
                _mimeTypes.Add(".vew", "application/groupwise");
                _mimeTypes.Add(".viv", "video/vivo");
                _mimeTypes.Add(".viv", "video/vnd.vivo");
                _mimeTypes.Add(".vivo", "video/vivo");
                _mimeTypes.Add(".vivo", "video/vnd.vivo");
                _mimeTypes.Add(".vmd", "application/vocaltec-media-desc");
                _mimeTypes.Add(".vmf", "application/vocaltec-media-file");
                _mimeTypes.Add(".voc", "audio/voc");
                _mimeTypes.Add(".voc", "audio/x-voc");
                _mimeTypes.Add(".vos", "video/vosaic");
                _mimeTypes.Add(".vox", "audio/voxware");
                _mimeTypes.Add(".vqe", "audio/x-twinvq-plugin");
                _mimeTypes.Add(".vqf", "audio/x-twinvq");
                _mimeTypes.Add(".vql", "audio/x-twinvq-plugin");
                _mimeTypes.Add(".vrml", "application/x-vrml");
                _mimeTypes.Add(".vrml", "model/vrml");
                _mimeTypes.Add(".vrml", "x-world/x-vrml");
                _mimeTypes.Add(".vrt", "x-world/x-vrt");
                _mimeTypes.Add(".vsd", "application/x-visio");
                _mimeTypes.Add(".vst", "application/x-visio");
                _mimeTypes.Add(".vsw", "application/x-visio");
                _mimeTypes.Add(".w60", "application/wordperfect6.0");
                _mimeTypes.Add(".w61", "application/wordperfect6.1");
                _mimeTypes.Add(".w6w", "application/msword");
                _mimeTypes.Add(".wav", "audio/wav");
                _mimeTypes.Add(".wav", "audio/x-wav");
                _mimeTypes.Add(".wb1", "application/x-qpro");
                _mimeTypes.Add(".wbmp", "image/vnd.wap.wbmp");
                _mimeTypes.Add(".web", "application/vnd.xara");
                _mimeTypes.Add(".wiz", "application/msword");
                _mimeTypes.Add(".wk1", "application/x-123");
                _mimeTypes.Add(".wmf", "windows/metafile");
                _mimeTypes.Add(".wml", "text/vnd.wap.wml");
                _mimeTypes.Add(".wmlc", "application/vnd.wap.wmlc");
                _mimeTypes.Add(".wmls", "text/vnd.wap.wmlscript");
                _mimeTypes.Add(".wmlsc", "application/vnd.wap.wmlscriptc");
                _mimeTypes.Add(".word", "application/msword");
                _mimeTypes.Add(".wp", "application/wordperfect");
                _mimeTypes.Add(".wp5", "application/wordperfect");
                _mimeTypes.Add(".wp5", "application/wordperfect6.0");
                _mimeTypes.Add(".wp6", "application/wordperfect");
                _mimeTypes.Add(".wpd", "application/wordperfect");
                _mimeTypes.Add(".wpd", "application/x-wpwin");
                _mimeTypes.Add(".wq1", "application/x-lotus");
                _mimeTypes.Add(".wri", "application/mswrite");
                _mimeTypes.Add(".wri", "application/x-wri");
                _mimeTypes.Add(".wrl", "application/x-world");
                _mimeTypes.Add(".wrl", "model/vrml");
                _mimeTypes.Add(".wrl", "x-world/x-vrml");
                _mimeTypes.Add(".wrz", "model/vrml");
                _mimeTypes.Add(".wrz", "x-world/x-vrml");
                _mimeTypes.Add(".wsc", "text/scriplet");
                _mimeTypes.Add(".wsrc", "application/x-wais-source");
                _mimeTypes.Add(".wtk", "application/x-wintalk");
                _mimeTypes.Add(".xbm", "image/x-xbitmap");
                _mimeTypes.Add(".xbm", "image/x-xbm");
                _mimeTypes.Add(".xbm", "image/xbm");
                _mimeTypes.Add(".xdr", "video/x-amt-demorun");
                _mimeTypes.Add(".xgz", "xgl/drawing");
                _mimeTypes.Add(".xif", "image/vnd.xiff");
                _mimeTypes.Add(".xl", "application/excel");
                _mimeTypes.Add(".xla", "application/excel");
                _mimeTypes.Add(".xla", "application/x-excel");
                _mimeTypes.Add(".xla", "application/x-msexcel");
                _mimeTypes.Add(".xlb", "application/excel");
                _mimeTypes.Add(".xlb", "application/vnd.ms-excel");
                _mimeTypes.Add(".xlb", "application/x-excel");
                _mimeTypes.Add(".xlc", "application/excel");
                _mimeTypes.Add(".xlc", "application/vnd.ms-excel");
                _mimeTypes.Add(".xlc", "application/x-excel");
                _mimeTypes.Add(".xld", "application/excel");
                _mimeTypes.Add(".xld", "application/x-excel");
                _mimeTypes.Add(".xlk", "application/excel");
                _mimeTypes.Add(".xlk", "application/x-excel");
                _mimeTypes.Add(".xll", "application/excel");
                _mimeTypes.Add(".xll", "application/vnd.ms-excel");
                _mimeTypes.Add(".xll", "application/x-excel");
                _mimeTypes.Add(".xlm", "application/excel");
                _mimeTypes.Add(".xlm", "application/vnd.ms-excel");
                _mimeTypes.Add(".xlm", "application/x-excel");
                _mimeTypes.Add(".xls", "application/excel");
                _mimeTypes.Add(".xls", "application/vnd.ms-excel");
                _mimeTypes.Add(".xls", "application/x-excel");
                _mimeTypes.Add(".xls", "application/x-msexcel");
                _mimeTypes.Add(".xlt", "application/excel");
                _mimeTypes.Add(".xlt", "application/x-excel");
                _mimeTypes.Add(".xlv", "application/excel");
                _mimeTypes.Add(".xlv", "application/x-excel");
                _mimeTypes.Add(".xlw", "application/excel");
                _mimeTypes.Add(".xlw", "application/vnd.ms-excel");
                _mimeTypes.Add(".xlw", "application/x-excel");
                _mimeTypes.Add(".xlw", "application/x-msexcel");
                _mimeTypes.Add(".xm", "audio/xm");
                _mimeTypes.Add(".xml", "application/xml");
                _mimeTypes.Add(".xml", "text/xml");
                _mimeTypes.Add(".xmz", "xgl/movie");
                _mimeTypes.Add(".xpix", "application/x-vnd.ls-xpix");
                _mimeTypes.Add(".xpm", "image/x-xpixmap");
                _mimeTypes.Add(".xpm", "image/xpm");
                _mimeTypes.Add(".x-png", "image/png");
                _mimeTypes.Add(".xsr", "video/x-amt-showrun");
                _mimeTypes.Add(".xwd", "image/x-xwd");
                _mimeTypes.Add(".xwd", "image/x-xwindowdump");
                _mimeTypes.Add(".xyz", "chemical/x-pdb");
                _mimeTypes.Add(".z", "application/x-compress");
                _mimeTypes.Add(".z", "application/x-compressed");
                _mimeTypes.Add(".zip", "application/x-compressed");
                _mimeTypes.Add(".zip", "application/x-zip-compressed");
                _mimeTypes.Add(".zip", "application/zip");
                _mimeTypes.Add(".zip", "multipart/x-zip");
                _mimeTypes.Add(".zoo", "application/octet-stream");
                _mimeTypes.Add(".zsh", "text/x-script.zsh");
                //agentsmith spellcheck enable
                return _mimeTypes;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets a value indicating if document encoding must be automatically detected.
        /// </summary>
        public bool AutoDetectEncoding
        {
            get { return _autoDetectEncoding; }
            set { _autoDetectEncoding = value; }
        }

        private Encoding _encoding;
        /// <summary>
        /// Gets or sets the Encoding used to override the response stream from any web request
        /// </summary>
        public Encoding OverrideEncoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        /// <summary>
        /// Gets or Sets a value indicating whether to get document only from the cache.
        /// If this is set to true and document is not found in the cache, nothing will be loaded.
        /// </summary>
        public bool CacheOnly
        {
            get { return _cacheOnly; }
            set
            {
                if ((value) && !UsingCache)
                {
                    throw new HtmlWebException("Cache is not enabled. Set UsingCache to true first.");
                }
                _cacheOnly = value;
            }
        }

        /// <summary>
        /// Gets or Sets the cache path. If null, no caching mechanism will be used.
        /// </summary>
        public string CachePath
        {
            get { return _cachePath; }
            set { _cachePath = value; }
        }

        /// <summary>
        /// Gets a value indicating if the last document was retrieved from the cache.
        /// </summary>
        public bool FromCache
        {
            get { return _fromCache; }
        }

        /// <summary>
        /// Gets the last request duration in milliseconds.
        /// </summary>
        public int RequestDuration
        {
            get { return _requestDuration; }
        }

        /// <summary>
        /// Gets the URI of the Internet resource that actually responded to the request.
        /// </summary>
        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        /// <summary>
        /// Gets the last request status.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        /// <summary>
        /// Gets or Sets the size of the buffer used for memory operations.
        /// </summary>
        public int StreamBufferSize
        {
            get { return _streamBufferSize; }
            set
            {
                if (_streamBufferSize <= 0)
                {
                    throw new ArgumentException("Size must be greater than zero.");
                }
                _streamBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or Sets a value indicating if cookies will be stored.
        /// </summary>
        public bool UseCookies
        {
            get { return _useCookies; }
            set { _useCookies = value; }
        }

        /// <summary>Gets or sets a value indicating whether redirect should be captured instead of the current location.</summary>
        /// <value>True if capture redirect, false if not.</value>
        public bool CaptureRedirect { get; set; }

        /// <summary>
        /// Gets or Sets the User Agent HTTP 1.1 header sent on any webrequest
        /// </summary>
        public string UserAgent { get { return _userAgent; } set { _userAgent = value; } }

        /// <summary>
        /// Gets or Sets a value indicating whether the caching mechanisms should be used or not.
        /// </summary>
        public bool UsingCache
        {
            get { return _cachePath != null && _usingCache; }
            set
            {
                if ((value) && (_cachePath == null))
                {
                    throw new HtmlWebException("You need to define a CachePath first.");
                }
                _usingCache = value;
            }
        }

        #endregion

        #region Public Methods
#if !NETSTANDARD
/// <summary>
/// Gets the MIME content type for a given path extension.
/// </summary>
/// <param name="extension">The input path extension.</param>
/// <param name="def">The default content type to return if any error occurs.</param>
/// <returns>The path extension's MIME content type.</returns>
        public static string GetContentTypeForExtension(string extension, string def)
        {
            var helper = new PermissionHelper();
            if (string.IsNullOrEmpty(extension))
            {
                return def;
            }
            string contentType = "";
            if (!helper.GetIsRegistryAvailable())
            {
                if (MimeTypes.ContainsKey(extension))
                    contentType = MimeTypes[extension];
                else
                    contentType = def;
            }

            if (!helper.GetIsDnsAvailable())
            {
                //do something.... not at full trust
                try
                {
                    RegistryKey reg = Registry.ClassesRoot;
                    reg = reg.OpenSubKey(extension, false);
                    if (reg != null) contentType = (string)reg.GetValue("", def);
                }
                catch (Exception)
                {
                    contentType = def;
                }
            }
            return contentType;
        }

        /// <summary>
        /// Gets the path extension for a given MIME content type.
        /// </summary>
        /// <param name="contentType">The input MIME content type.</param>
        /// <param name="def">The default path extension to return if any error occurs.</param>
        /// <returns>The MIME content type's path extension.</returns>
        public static string GetExtensionForContentType(string contentType, string def)
        {
            var helper = new PermissionHelper();

            if (string.IsNullOrEmpty(contentType))
            {
                return def;
            }
            string ext = "";
            if (!helper.GetIsRegistryAvailable())
            {
                if (MimeTypes.ContainsValue(contentType))
                {
                    foreach (KeyValuePair<string, string> pair in MimeTypes)
                        if (pair.Value == contentType)
                            return pair.Value;
                }
                return def;
            }

            if (helper.GetIsRegistryAvailable())
            {
                try
                {
                    RegistryKey reg = Registry.ClassesRoot;
                    reg = reg.OpenSubKey(@"MIME\Database\Content Type\" + contentType, false);
                    if (reg != null) ext = (string)reg.GetValue("Extension", def);
                }
                catch (Exception)
                {
                    ext = def;
                }
            }
            return ext;
        }

        /// <summary>
        /// Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="type">The requested type.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string url, Type type)
        {
            return CreateInstance(url, null, null, type);
        }
#endif


        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        public void Get(string url, string path)
        {
            Get(url, path, "GET");
        }

#if !NETSTANDARD
/// <summary>
/// Gets an HTML document from an Internet resource and saves it to the specified file. - Proxy aware
/// </summary>
/// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="path">The location of the file where you want to save the document.</param>
/// <param name="proxy"></param>
/// <param name="credentials"></param>
        public void Get(string url, string path, WebProxy proxy, NetworkCredential credentials)
        {
            Get(url, path, proxy, credentials, "GET");
        }
#endif
#if NET45 || NETSTANDARD
        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file. - Proxy aware
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="proxy"></param>
        /// <param name="credentials"></param>
        public void Get(string url, string path, IWebProxy proxy, ICredentials credentials)
        {
            Get(url, path, proxy, credentials, "GET");
        }
#endif

        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        public void Get(string url, string path, string method)
        {
            Uri uri = new Uri(url);
#if !NETSTANDARD
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
#else
            // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
            if ((uri.Scheme == "https") ||
                (uri.Scheme == "http"))
#endif
            {
                Get(uri, method, path, null, null, null);
            }
            else
            {
                throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
            }
        }

#if !NETSTANDARD
/// <summary>
/// Gets an HTML document from an Internet resource and saves it to the specified file.  Understands Proxies
/// </summary>
/// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="path">The location of the file where you want to save the document.</param>
/// <param name="credentials"></param>
/// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
/// <param name="proxy"></param>
        public void Get(string url, string path, WebProxy proxy, NetworkCredential credentials, string method)
        {
            Uri uri = new Uri(url);
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
            {
                Get(uri, method, path, null, proxy, credentials);
            }
            else
            {
                throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
            }
        }
#endif

#if NET45 || NETSTANDARD
        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.  Understands Proxies
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="credentials"></param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy"></param>
        public void Get(string url, string path, IWebProxy proxy, ICredentials credentials, string method)
        {
            Uri uri = new Uri(url);
#if !NETSTANDARD
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
#else
            // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
            if ((uri.Scheme == "https") ||
                (uri.Scheme == "http"))
#endif
            {
                Get(uri, method, path, null, proxy, credentials);
            }
            else
            {
                throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
            }
        }
#endif

        /// <summary>
        /// Gets the cache file path for a specified url.
        /// </summary>
        /// <param name="uri">The url fo which to retrieve the cache path. May not be null.</param>
        /// <returns>The cache file path.</returns>
        public string GetCachePath(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (!UsingCache)
            {
                throw new HtmlWebException("Cache is not enabled. Set UsingCache to true first.");
            }
            string cachePath;
            if (uri.AbsolutePath == "/")
            {
                cachePath = Path.Combine(_cachePath, ".htm");
            }
            else
            {
                if (uri.AbsolutePath[uri.AbsolutePath.Length - 1] == Path.AltDirectorySeparatorChar)
                {
                    cachePath = Path.Combine(_cachePath, (uri.Host + uri.AbsolutePath.TrimEnd(Path.AltDirectorySeparatorChar)).Replace('/', '\\') + ".htm");
                }
                else
                {
                    cachePath = Path.Combine(_cachePath, (uri.Host + uri.AbsolutePath.Replace('/', '\\')));
                }
            }
            return cachePath;
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url)
        {
            return Load(url, "GET");
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="uri">The requested Uri, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri)
        {
            return Load(uri, "GET");
        }

#if !NETSTANDARD
/// <summary>
/// Gets an HTML document from an Internet resource.
/// </summary>
/// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="proxyHost">Host to use for Proxy</param>
/// <param name="proxyPort">Port the Proxy is on</param>
/// <param name="userId">User Id for Authentication</param>
/// <param name="password">Password for Authentication</param>
/// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string proxyHost, int proxyPort, string userId, string password)
        {
            //Create my proxy
            WebProxy myProxy = new WebProxy(proxyHost, proxyPort);
            myProxy.BypassProxyOnLocal = true;

            //Create my credentials
            NetworkCredential myCreds = null;
            if ((userId != null) && (password != null))
            {
                myCreds = new NetworkCredential(userId, password);
                CredentialCache credCache = new CredentialCache();
                //Add the creds
                credCache.Add(myProxy.Address, "Basic", myCreds);
                credCache.Add(myProxy.Address, "Digest", myCreds);
            }

            return Load(url, "GET", myProxy, myCreds);
        }
#endif

#if !NETSTANDARD
/// <summary>
/// Gets an HTML document from an Internet resource.
/// </summary>
/// <param name="uri">The requested Uri, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
/// <param name="proxyHost">Host to use for Proxy</param>
/// <param name="proxyPort">Port the Proxy is on</param>
/// <param name="userId">User Id for Authentication</param>
/// <param name="password">Password for Authentication</param>
/// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri, string proxyHost, int proxyPort, string userId, string password)
        {
            //Create my proxy
            WebProxy myProxy = new WebProxy(proxyHost, proxyPort);
            myProxy.BypassProxyOnLocal = true;

            //Create my credentials
            NetworkCredential myCreds = null;
            if ((userId != null) && (password != null))
            {
                myCreds = new NetworkCredential(userId, password);
                CredentialCache credCache = new CredentialCache();
                //Add the creds
                credCache.Add(myProxy.Address, "Basic", myCreds);
                credCache.Add(myProxy.Address, "Digest", myCreds);
            }

            return Load(uri, "GET", myProxy, myCreds);
        }
#endif

        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method)
        {
            Uri uri = new Uri(url);

            return Load(uri, method);
        }
        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="uri">The requested URL, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri, string method)
        {
            if (UsingCache)
            {
                _usingCacheAndLoad = true;
            }

            HtmlDocument doc;
#if !NETSTANDARD
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
#else
            // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
            if ((uri.Scheme == "https") ||
                (uri.Scheme == "http"))
#endif
            {
                doc = LoadUrl(uri, method, null, null);
            }
            else
            {
#if !NETSTANDARD
                if (uri.Scheme == Uri.UriSchemeFile)
#else
                // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
                if (uri.Scheme == "file")
#endif
                {
                    doc = new HtmlDocument();
                    doc.OptionAutoCloseOnEnd = false;
                    doc.OptionAutoCloseOnEnd = true;
                    if (OverrideEncoding != null)
                        doc.Load(uri.OriginalString, OverrideEncoding);
                    else
                        doc.DetectEncodingAndLoad(uri.OriginalString, _autoDetectEncoding);
                }
                else
                {
                    throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
                }
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }

#if !NETSTANDARD
/// <summary>
/// Loads an HTML document from an Internet resource.
/// </summary>
/// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
/// <param name="proxy">Proxy to use with this request</param>
/// <param name="credentials">Credentials to use when authenticating</param>
/// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method, WebProxy proxy, NetworkCredential credentials)
        {
            Uri uri = new Uri(url);

            return Load(uri, method, proxy, credentials);
        }
#endif

#if !NETSTANDARD
/// <summary>
/// Loads an HTML document from an Internet resource.
/// </summary>
/// <param name="uri">The requested Uri, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
/// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
/// <param name="proxy">Proxy to use with this request</param>
/// <param name="credentials">Credentials to use when authenticating</param>
/// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri, string method, WebProxy proxy, NetworkCredential credentials)
        {
            if (UsingCache)
            {
                _usingCacheAndLoad = true;
            }

            HtmlDocument doc;
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
            {
                doc = LoadUrl(uri, method, proxy, credentials);
            }
            else
            {
                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    doc = new HtmlDocument();
                    doc.OptionAutoCloseOnEnd = false;
                    doc.OptionAutoCloseOnEnd = true;
                    doc.DetectEncodingAndLoad(uri.OriginalString, _autoDetectEncoding);
                }
                else
                {
                    throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
                }
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }
#endif
#if NET45 || NETSTANDARD
        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy">Proxy to use with this request</param>
        /// <param name="credentials">Credentials to use when authenticating</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method, IWebProxy proxy, ICredentials credentials)
        {
            Uri uri = new Uri(url);
            return Load(uri, method, proxy, credentials);
        }
#endif

#if NET45 || NETSTANDARD
        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="uri">The requested Uri, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy">Proxy to use with this request</param>
        /// <param name="credentials">Credentials to use when authenticating</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri, string method, IWebProxy proxy, ICredentials credentials)
        {
            if (UsingCache)
            {
                _usingCacheAndLoad = true;
            }

            HtmlDocument doc;
#if !NETSTANDARD
            if (uri.Scheme == Uri.UriSchemeFile)
#else
            // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
            if (uri.Scheme == "file")
#endif
            {
                doc = LoadUrl(uri, method, proxy, credentials);
            }
            else
            {
#if !NETSTANDARD
                if (uri.Scheme == Uri.UriSchemeFile)
#else
                // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
                if (uri.Scheme == "file")
#endif
                {
                    doc = new HtmlDocument();
                    doc.OptionAutoCloseOnEnd = false;
                    doc.OptionAutoCloseOnEnd = true;
                    doc.DetectEncodingAndLoad(uri.OriginalString, _autoDetectEncoding);
                }
                else
                {
                    throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
                }
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }
#endif
#if !NETSTANDARD
/// <summary>
/// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter.
/// </summary>
/// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="writer">The XmlTextWriter to which you want to save to.</param>
        public void LoadHtmlAsXml(string htmlUrl, XmlTextWriter writer)
        {
            HtmlDocument doc = Load(htmlUrl);
            doc.Save(writer);
        }
#endif
#if NET45 || NETSTANDARD
        /// <summary>
        /// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="writer">The XmlTextWriter to which you want to save to.</param>
        public void LoadHtmlAsXml(string htmlUrl, XmlWriter writer)
        {
            HtmlDocument doc = Load(htmlUrl);
            doc.Save(writer);
        }
#endif



        #endregion

        #region Private Methods

        private static void FilePreparePath(string target)
        {
            if (File.Exists(target))
            {
                FileAttributes atts = File.GetAttributes(target);
                File.SetAttributes(target, atts & ~FileAttributes.ReadOnly);
            }
            else
            {
                string dir = Path.GetDirectoryName(target);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }


        private static DateTime RemoveMilliseconds(DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, 0);
        }

        private static DateTime RemoveMilliseconds(DateTimeOffset? offset)
        {
            var t = offset ?? DateTimeOffset.Now;
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, 0);
        }

        // ReSharper disable UnusedMethodReturnValue.Local
        private static long SaveStream(Stream stream, string path, DateTime touchDate, int streamBufferSize)
            // ReSharper restore UnusedMethodReturnValue.Local
        {
            FilePreparePath(path);

            long len = 0;

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        byte[] buffer;
                        do
                        {
                            buffer = br.ReadBytes(streamBufferSize);
                            len += buffer.Length;
                            if (buffer.Length > 0)
                            {
                                bw.Write(buffer);
                            }
                        } while (buffer.Length > 0);
                        bw.Flush();
                    }
                }
            }

            File.SetLastWriteTime(path, touchDate);
            return len;
        }

#if !NETSTANDARD
        private HttpStatusCode Get(Uri uri, string method, string path, HtmlDocument doc, IWebProxy proxy,
            ICredentials creds)
        {
            string cachePath = null;
            HttpWebRequest req;
            bool oldFile = false;

            req = WebRequest.Create(uri) as HttpWebRequest;
            req.Method = method;
            req.UserAgent = UserAgent;
            if (CaptureRedirect)
            {
                req.AllowAutoRedirect = false;
            }
            if (proxy != null)
            {
                if (creds != null)
                {
                    proxy.Credentials = creds;
                    req.Credentials = creds;
                }
                else
                {
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    req.Credentials = CredentialCache.DefaultCredentials;
                }
                req.Proxy = proxy;
            }

            _fromCache = false;
            _requestDuration = 0;
            int tc = Environment.TickCount;
            if (UsingCache)
            {
                cachePath = GetCachePath(req.RequestUri);
                if (File.Exists(cachePath))
                {
                    req.IfModifiedSince = File.GetLastAccessTime(cachePath);
                    oldFile = true;
                }
            }

            if (_cacheOnly)
            {
                if (!File.Exists(cachePath))
                {
                    throw new HtmlWebException("File was not found at cache path: '" + cachePath + "'");
                }

                if (path != null)
                {
                    IOLibrary.CopyAlways(cachePath, path);
                    // touch the file
                    if (cachePath != null) File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                }
                _fromCache = true;
                return HttpStatusCode.NotModified;
            }

            if (_useCookies)
            {
                req.CookieContainer = new CookieContainer();
            }

            if (PreRequest != null)
            {
                // allow our user to change the request at will
                if (!PreRequest(req))
                {
                    return HttpStatusCode.ResetContent;
                }

                // dump cookie
                //				if (_useCookies)
                //				{
                //					foreach(Cookie cookie in req.CookieContainer.GetCookies(req.RequestUri))
                //					{
                //						HtmlLibrary.Trace("Cookie " + cookie.Name + "=" + cookie.Value + " path=" + cookie.Path + " domain=" + cookie.Domain);
                //					}
                //				}
            }

            HttpWebResponse resp;

            try
            {
                resp = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException we)
            {
                _requestDuration = Environment.TickCount - tc;
                resp = (HttpWebResponse)we.Response;
                if (resp == null)
                {
                    if (oldFile)
                    {
                        if (path != null)
                        {
                            IOLibrary.CopyAlways(cachePath, path);
                            // touch the file
                            File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                        }
                        return HttpStatusCode.NotModified;
                    }
                    throw;
                }
            }
            catch (Exception)
            {
                _requestDuration = Environment.TickCount - tc;
                throw;
            }

            // allow our user to get some info from the response
            if (PostResponse != null)
            {
                PostResponse(req, resp);
            }

            _requestDuration = Environment.TickCount - tc;
            _responseUri = resp.ResponseUri;

            bool html = IsHtmlContent(resp.ContentType);
            bool isUnknown = string.IsNullOrEmpty(resp.ContentType);

            Encoding respenc = !string.IsNullOrEmpty(resp.ContentEncoding)
                ? Encoding.GetEncoding(resp.ContentEncoding)
                : null;
            if (OverrideEncoding != null)
                respenc = OverrideEncoding;

            if (CaptureRedirect)
            {
                // Found == 302
                if (resp.StatusCode == HttpStatusCode.Found)
                {
                    var location = resp.Headers["Location"];
                    Uri locationUri;

                    // Do the redirection after we've eaten all the cookies...
                    if (!Uri.TryCreate(location, UriKind.Absolute, out locationUri))
                    {
                        locationUri = new Uri(uri, location);
                    }
                    return Get(locationUri, "GET", path, doc, proxy, creds);
                }
            }



            if (resp.StatusCode == HttpStatusCode.NotModified)
            {
                if (UsingCache)
                {
                    _fromCache = true;
                    if (path != null)
                    {
                        IOLibrary.CopyAlways(cachePath, path);
                        // touch the file
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }
                    return resp.StatusCode;
                }
                // this should *never* happen...
                throw new HtmlWebException("Server has send a NotModifed code, without cache enabled.");
            }
            Stream s = resp.GetResponseStream();
            if (s != null)
            {
                if (UsingCache)
                {
                    // NOTE: LastModified does not contain milliseconds, so we remove them to the file
                    SaveStream(s, cachePath, RemoveMilliseconds(resp.LastModified), _streamBufferSize);

                    // save headers
                    SaveCacheHeaders(req.RequestUri, resp);

                    if (path != null)
                    {
                        // copy and touch the file
                        IOLibrary.CopyAlways(cachePath, path);
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }

                    if (_usingCacheAndLoad)
                    {
                        doc.Load(cachePath);
                    }
                }
                else
                {
                    // try to work in-memory
                    if (doc != null && html)
                    {
                        if (respenc == null)
                        {
                            doc.Load(s, true);
                        }
                        else
                        {
                            doc.Load(s, respenc);
                        }
                    }

                    if (doc != null && isUnknown)
                    {
                        try
                        {
                            if (respenc == null)
                            {
                                doc.Load(s, true);
                            }
                            else
                            {
                                doc.Load(s, respenc);
                            }
                        }
                        catch
                        {
                            // That’s fine, the content type was unknown so probably not HTML
                            // Perhaps trying to figure if the content contains some HTML before would be a better idea.
                        }
                    }
                }
                resp.Close();
            }
            return resp.StatusCode;
        }
#else
        private HttpStatusCode Get(Uri uri, string method, string path, HtmlDocument doc, IWebProxy proxy,
            ICredentials creds)
        {
            string cachePath = null;
            bool oldFile = false;
            HttpStatusCode status;
            using (var request = new HttpRequestMessage(new HttpMethod(method), uri))
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                if(CaptureRedirect)
                {
                    handler.AllowAutoRedirect = false;
                }

                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

                if (proxy != null)
                {
                    if (creds != null)
                    {
                        proxy.Credentials = creds;
                        handler.Credentials = creds;
                    }
                    else
                    {
                        proxy.Credentials = CredentialCache.DefaultCredentials;
                        handler.Credentials = CredentialCache.DefaultCredentials;
                    }
                    handler.Proxy = proxy;
                    handler.UseProxy = true;
                }

                _fromCache = false;
                _requestDuration = 0;
                int tc = Environment.TickCount;
                if (UsingCache)
                {
                    cachePath = GetCachePath(request.RequestUri);
                    if (File.Exists(cachePath))
                    {
                        client.DefaultRequestHeaders.IfModifiedSince = File.GetLastAccessTime(cachePath);
                        oldFile = true;
                    }
                }

                if (_cacheOnly)
                {
                    if (!File.Exists(cachePath))
                    {
                        throw new HtmlWebException("File was not found at cache path: '" + cachePath + "'");
                    }

                    if (path != null)
                    {
                        IOLibrary.CopyAlways(cachePath, path);
                        // touch the file
                        if (cachePath != null) File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }
                    _fromCache = true;
                    return HttpStatusCode.NotModified;
                }

                if (_useCookies)
                {
                    handler.CookieContainer = new CookieContainer();
                }

                if (PreRequest != null)
                {
                    // allow our user to change the request at will
                    if (!PreRequest(handler, request))
                    {
                        return HttpStatusCode.ResetContent;
                    }

                    // dump cookie
                    //				if (_useCookies)
                    //				{
                    //					foreach(Cookie cookie in req.CookieContainer.GetCookies(req.RequestUri))
                    //					{
                    //						HtmlLibrary.Trace("Cookie " + cookie.Name + "=" + cookie.Value + " path=" + cookie.Path + " domain=" + cookie.Domain);
                    //					}
                    //				}
                }

                HttpResponseMessage response;
                try
                {
                    response = client.SendAsync(request).Result;
                }
                catch (HttpRequestException)
                {
                    _requestDuration = Environment.TickCount - tc;
                    if (oldFile)
                    {
                        if (path != null)
                        {
                            IOLibrary.CopyAlways(cachePath, path);
                            // touch the file
                            File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                        }
                        return HttpStatusCode.NotModified;
                    }
                    throw;
                }
                catch (Exception)
                {
                    _requestDuration = Environment.TickCount - tc;
                    throw;
                }

                // allow our user to get some info from the response
                if (PostResponse != null)
                {
                    PostResponse(request, response);
                }

                _requestDuration = Environment.TickCount - tc;
                _responseUri = response.RequestMessage.RequestUri;

                bool isUnknown = response.Content.Headers.ContentType == null;
                bool html = !isUnknown && IsHtmlContent(response.Content.Headers.ContentType.MediaType);

                var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
                Encoding respenc = !string.IsNullOrEmpty(encoding)
                    ? Encoding.GetEncoding(encoding)
                    : null;

                if(CaptureRedirect)
                {
                    // Found == 302
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        // Do the redirection after we've eaten all the cookies...
                        return Get(response.Headers.Location, "GET", path, doc, proxy, creds);
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    if (UsingCache)
                    {
                        _fromCache = true;
                        if (path != null)
                        {
                            IOLibrary.CopyAlways(cachePath, path);
                            // touch the file
                            File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                        }
                        return response.StatusCode;
                    }
                    // this should *never* happen...
                    throw new HtmlWebException("Server has send a NotModifed code, without cache enabled.");
                }
                Stream s = response.Content.ReadAsStreamAsync().Result;
                if (s != null)
                {
                    if (UsingCache)
                    {
                        // NOTE: LastModified does not contain milliseconds, so we remove them to the file
                        SaveStream(s, cachePath, RemoveMilliseconds(response.Content.Headers.LastModified), _streamBufferSize);

                        // save headers
                        SaveCacheHeaders(request.RequestUri, response);

                        if (path != null)
                        {
                            // copy and touch the file
                            IOLibrary.CopyAlways(cachePath, path);
                            File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                        }

                        if (_usingCacheAndLoad)
                        {
                            doc.Load(cachePath);
                        }
                    }
                    else
                    {
                        // try to work in-memory
                        if ((doc != null) && (html))
                        {
                            if (respenc != null)
                            {
                                doc.Load(s, respenc);
                            }
                            else
                            {
                                doc.Load(s, true);
                            }
                        }

                        else if (doc != null && isUnknown)
                        {
                            try
                            {
                                if (respenc == null)
                                {
                                    doc.Load(s, true);
                                }
                                else
                                {
                                    doc.Load(s, respenc);
                                }
                            }
                            catch
                            {
                                // That’s fine, the content type was unknown so probably not HTML
                                // Perhaps trying to figure if the content contains some HTML before would be a better idea.
                            }
                        }
                    }
                }
                status = response.StatusCode;
            }

            return status;
        }
#endif

        private string GetCacheHeader(Uri requestUri, string name, string def)
        {
            // note: some headers are collection (ex: www-authenticate)
            // we don't handle that here
            XmlDocument doc = new XmlDocument();
#if NETSTANDARD
            doc.Load(File.OpenRead(GetCacheHeadersPath(requestUri)));
#else
            doc.Load(GetCacheHeadersPath(requestUri));
#endif
            XmlNode node =
                doc.SelectSingleNode("//h[translate(@n, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='" +
                                     name.ToUpper() + "']");
            if (node == null)
            {
                return def;
            }
            // attribute should exist
            return node.Attributes[name].Value;
        }

        private string GetCacheHeadersPath(Uri uri)
        {
            //return Path.Combine(GetCachePath(uri), ".h.xml");
            return GetCachePath(uri) + ".h.xml";
        }

#if !NETSTANDARD
        private bool IsCacheHtmlContent(string path)
        {
            string ct = GetContentTypeForExtension(Path.GetExtension(path), null);
            return IsHtmlContent(ct);
        }
#endif

        private bool IsHtmlContent(string contentType)
        {
            return contentType.ToLower().StartsWith("text/html");
        }

        private bool IsGZipEncoding(string contentEncoding)
        {
            return contentEncoding.ToLower().StartsWith("gzip");
        }

#if !NETSTANDARD
        private HtmlDocument LoadUrl(Uri uri, string method, WebProxy proxy, NetworkCredential creds)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionAutoCloseOnEnd = false;
            doc.OptionFixNestedTags = true;
            _statusCode = Get(uri, method, null, doc, proxy, creds);
            if (_statusCode == HttpStatusCode.NotModified)
            {
                // read cached encoding
                doc.DetectEncodingAndLoad(GetCachePath(uri));
            }
            return doc;
        }
#endif

#if NET45 || NETSTANDARD
        private HtmlDocument LoadUrl(Uri uri, string method, IWebProxy proxy, ICredentials creds)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionAutoCloseOnEnd = false;
            doc.OptionFixNestedTags = true;
            _statusCode = Get(uri, method, null, doc, proxy, creds);
            if (_statusCode == HttpStatusCode.NotModified)
            {
                // read cached encoding
                doc.DetectEncodingAndLoad(GetCachePath(uri));
            }
            return doc;
        }
#endif
#if !NETSTANDARD
        private void SaveCacheHeaders(Uri requestUri, HttpWebResponse resp)
        {
            // we cache the original headers aside the cached document.
            string file = GetCacheHeadersPath(requestUri);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<c></c>");
            XmlNode cache = doc.FirstChild;
            foreach (string header in resp.Headers)
            {
                XmlNode entry = doc.CreateElement("h");
                XmlAttribute att = doc.CreateAttribute("n");
                att.Value = header;
                entry.Attributes.Append(att);

                att = doc.CreateAttribute("v");
                att.Value = resp.Headers[header];
                entry.Attributes.Append(att);

                cache.AppendChild(entry);
            }
            doc.Save(file);
        }
#endif

#if NET45 || NETSTANDARD
        private void SaveCacheHeaders(Uri requestUri, HttpResponseMessage resp)
        {
            // we cache the original headers aside the cached document.
            string file = GetCacheHeadersPath(requestUri);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<c></c>");
            XmlNode cache = doc.FirstChild;
            foreach (var header in resp.Headers)
            {
                XmlNode entry = doc.CreateElement("h");
                XmlAttribute att = doc.CreateAttribute("n");
                att.Value = header.Key;
                entry.Attributes.Append(att);

                att = doc.CreateAttribute("v");
                att.Value = string.Join(";", header.Value);
                entry.Attributes.Append(att);

                cache.AppendChild(entry);
            }
            doc.Save(File.OpenWrite(file));
        }
#endif

#if NET45 || NETSTANDARD
        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url)
        {
            return LoadFromWebAsync(new Uri(url), null, null);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), null, null, cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding)
        {
            return LoadFromWebAsync(new Uri(url), encoding, null, CancellationToken.None);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), encoding, null, cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName, string password)
        {
            return LoadFromWebAsync(new Uri(url), encoding, new NetworkCredential(userName, password), CancellationToken.None);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName, string password, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), encoding, new NetworkCredential(userName, password), cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName, string password, string domain)
        {
            return LoadFromWebAsync(new Uri(url), encoding, new NetworkCredential(userName, password, domain), CancellationToken.None);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName, string password, string domain, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), encoding, new NetworkCredential(userName, password, domain), cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password, string domain)
        {
            return LoadFromWebAsync(new Uri(url), null, new NetworkCredential(userName, password, domain), CancellationToken.None);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password, string domain, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), null, new NetworkCredential(userName, password, domain), cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password)
        {
            return LoadFromWebAsync(new Uri(url), null, new NetworkCredential(userName, password), CancellationToken.None);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), null, new NetworkCredential(userName, password), cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, NetworkCredential credentials)
        {
            return LoadFromWebAsync(new Uri(url), null, credentials, CancellationToken.None);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, NetworkCredential credentials, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), null, credentials, cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="uri">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(Uri uri, Encoding encoding, NetworkCredential credentials)
        {
            return LoadFromWebAsync(uri, encoding, credentials, CancellationToken.None);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="uri">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<HtmlDocument> LoadFromWebAsync(Uri uri, Encoding encoding, NetworkCredential credentials, CancellationToken cancellationToken)
        {
            var clientHandler = new HttpClientHandler();
            if (credentials == null)
                clientHandler.UseDefaultCredentials = true;
            else
                clientHandler.Credentials = credentials;

            var client = new HttpClient(clientHandler);

            var e = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (e.StatusCode == HttpStatusCode.OK)
            {
                var html = string.Empty;
                if (encoding != null)
                {
                    using (var sr = new StreamReader(await e.Content.ReadAsStreamAsync().ConfigureAwait(false), encoding))
                    {
                        html = sr.ReadToEnd();
                    }
                }
                else
                    html = await e.Content.ReadAsStringAsync().ConfigureAwait(false);
                var doc = new HtmlDocument();
                if (PreHandleDocument != null)
                    PreHandleDocument(doc);
                doc.LoadHtml(html);
                return doc;
            }
            throw new Exception("Error downloading html");
        }
#endif

        #endregion

        #region Browser

#if FX40 || FX45

        private TimeSpan _browserTimeout = TimeSpan.FromSeconds(30);
        private TimeSpan _browserDelay = TimeSpan.FromMilliseconds(100);

        /// <summary>Gets or sets the web browser timeout.</summary>
        /// 
        public TimeSpan BrowserTimeout
        {
            get { return _browserTimeout; }
            set { _browserTimeout = value; }
        }

        /// <summary>Gets or sets the web browser delay.</summary>
        public TimeSpan BrowserDelay
        {
            get { return _browserDelay; }
            set { _browserDelay = value; }
        }

        /// <summary>Loads HTML using a WebBrowser and Application.DoEvents.</summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="url">The requested URL, such as "http://html-agility-pack.net/".</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument LoadFromBrowser(string url)
        {
            return LoadFromBrowser(url, (object browser) => true);
        }
        internal string WebBrowserOuterHtml(object webBrowser)
        {
            var documentProperty = webBrowser.GetType().GetProperty("Document");
            var document = documentProperty.GetValue(webBrowser, null);

            var getElementsByTagNameMethod = document.GetType().GetMethod("GetElementsByTagName", new Type[] { typeof(string) });
            var getElementsByTagName = getElementsByTagNameMethod.Invoke(document, new[] { "HTML" });

            var indexerProperty = getElementsByTagName.GetType().GetProperty("Item", new Type[] { typeof(int) });
            var firstElement = indexerProperty.GetValue(getElementsByTagName, new object[] { 0 });

            var outerHtmlProperty = firstElement.GetType().GetProperty("OuterHtml");
            var outerHtml = outerHtmlProperty.GetValue(firstElement, null);

            return (string)outerHtml;
        }

        /// <summary>Loads HTML using a WebBrowser and Application.DoEvents.</summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="url">The requested URL, such as "http://html-agility-pack.net/".</param>
        /// <param name="isBrowserScriptCompleted">(Optional) Check if the browser script has all been run and completed.</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument LoadFromBrowser(string url, Func<string, bool> isBrowserScriptCompleted = null)
        {
            return LoadFromBrowser(url, browser =>
            {
                if (isBrowserScriptCompleted != null)
                {
                    return isBrowserScriptCompleted(WebBrowserOuterHtml(browser));
                }
                return true;
            });
        }

        /// <summary>Loads HTML using a WebBrowser and Application.DoEvents.</summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="url">The requested URL, such as "http://html-agility-pack.net/".</param>
        /// <param name="isBrowserScriptCompleted">(Optional) Check if the browser script has all been run and completed.</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument LoadFromBrowser(string url, Func<object, bool> isBrowserScriptCompleted = null)
        {
            var system_windows_forms = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "System.Windows.Forms");

            if (system_windows_forms == null)
            {
                // TRY to load id
                try
                {
                    var system = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "System");

                    if (system != null)
                    {
                        var path = system.CodeBase.Replace("System", "System.Windows.Forms").Replace("file:///", "");
                        Assembly.LoadFile(path);
                    }
                }
                catch(Exception ex)
                {
                    // Silence catch
                }

                system_windows_forms = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "System.Windows.Forms");

                if (system_windows_forms == null)
                {
                    throw new Exception("Oops! No reference to System.Windows.Forms have been loaded. Make sure your project load any type from this assembly to make sure the reference is added to the domain assemblies list. Example: `var webBrowserType = typeof(WebBrowser);`");
                }
            }

            var webBrowserType = system_windows_forms.GetType("System.Windows.Forms.WebBrowser");
            var webBrowserConstructor = webBrowserType.GetConstructor(new Type[0]);
            var application = system_windows_forms.GetType("System.Windows.Forms.Application");
            var doEventsMethod = application.GetMethod("DoEvents");

            Uri uri = new Uri(url);
            HtmlDocument doc = new HtmlDocument();
            string timeoutError = "WebBrowser Execution Timeout Expired. The timeout period elapsed prior to completion of the operation. To avoid this error, increase the WebBrowserTimeout value or set it to 0 (unlimited).";

            using (var webBrowser = (IDisposable)webBrowserConstructor.Invoke(new object[0]))
            {
                var scripErrorSuppressedProperty = webBrowserType.GetProperty("ScriptErrorsSuppressed");
                scripErrorSuppressedProperty.SetValue(webBrowser, true, null);

                var navigateMethod = webBrowserType.GetMethod("Navigate", new Type[] {typeof(Uri)});
                navigateMethod.Invoke(webBrowser, new object[] {uri});

                var readyStateProperty = webBrowserType.GetProperty("ReadyState");
                var isBusyProperty = webBrowserType.GetProperty("IsBusy");

                Stopwatch clock = new Stopwatch();
                clock.Start();

                // WAIT until the document is completed
                while ((int)readyStateProperty.GetValue(webBrowser, null) != 4 || (bool)isBusyProperty.GetValue(webBrowser, null))
                {
                    // ENSURE we didn't reach the timeout
                    if (BrowserTimeout.TotalMilliseconds != 0 && clock.ElapsedMilliseconds > BrowserTimeout.TotalMilliseconds)
                    {
                        throw new Exception(timeoutError);
                    }

                    doEventsMethod.Invoke(null, new object[0]);
                    Thread.Sleep(_browserDelay);
                }

                if (isBrowserScriptCompleted != null)
                {
                    // LOOP until the user say script are completed
                    while (!isBrowserScriptCompleted(webBrowser))
                    {
                        // ENSURE we didn't reach the timeout
                        if (BrowserTimeout.TotalMilliseconds != 0 && clock.ElapsedMilliseconds > BrowserTimeout.TotalMilliseconds)
                        {
                            var documentTextError = WebBrowserOuterHtml(webBrowser);
                            throw new Exception(timeoutError);
                        }

                        doEventsMethod.Invoke(null, new object[0]);
                        Thread.Sleep(_browserDelay);
                    }
                }

                var documentText = WebBrowserOuterHtml(webBrowser);

                doc.LoadHtml(documentText);
            }

            return doc;
        }
#endif
        #endregion
    }

#if !NETSTANDARD
/// <summary>
/// Wraps getting AppDomain permissions
/// </summary>
    public class PermissionHelper : IPermissionHelper
    {
        /// <summary>
        /// Checks to see if Registry access is available to the caller
        /// </summary>
        /// <returns></returns>
        public bool GetIsRegistryAvailable()
        {
#if FX40
            var permissionSet = new PermissionSet(PermissionState.None);    
            var writePermission = new RegistryPermission( PermissionState.Unrestricted);
            permissionSet.AddPermission(writePermission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
#else
            return SecurityManager.IsGranted(new RegistryPermission(PermissionState.Unrestricted));
#endif
        }
        /// <summary>
        /// Checks to see if DNS information is available to the caller
        /// </summary>
        /// <returns></returns>
        public bool GetIsDnsAvailable()
        {
#if FX40
            var permissionSet = new PermissionSet(PermissionState.None);    
            var writePermission = new DnsPermission(PermissionState.Unrestricted);
            permissionSet.AddPermission(writePermission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
#else
            return SecurityManager.IsGranted(new DnsPermission(PermissionState.Unrestricted));
#endif
        }
    }
#endif

    /// <summary>
    /// An interface for getting permissions of the running application
    /// </summary>
    public interface IPermissionHelper
    {
        /// <summary>
        /// Checks to see if Registry access is available to the caller
        /// </summary>
        /// <returns></returns>
        bool GetIsRegistryAvailable();

        /// <summary>
        /// Checks to see if DNS information is available to the caller
        /// </summary>
        /// <returns></returns>
        bool GetIsDnsAvailable();
    }
}
#endif