using System.Data.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SexiLove.Extensions.Helpers
{
    using System;
    using System.Collections.Generic;

    public class MIMETypeWrapper
    {
        private static readonly Dictionary<string, string> MIMETypesDictionary;

        public static string GetMIMEForData(Binary img)
        {
            if (img.Length == 0)
                return "image/jpeg";
            var ms = new MemoryStream(img.ToArray());
            ms.Position = 0;
            var image = new Bitmap(ms);
            return GetMIMEForImage(image);
        }
        public static string GetMIMEForImage(Image img)
        {
            return GetMIME(img.RawFormat.ToString());
        }

        static MIMETypeWrapper()
        {
            Dictionary<string, string> mimeList = new Dictionary<string, string>();
            mimeList.Add("ai", "application/postscript");
            mimeList.Add("aif", "audio/x-aiff");
            mimeList.Add("aifc", "audio/x-aiff");
            mimeList.Add("aiff", "audio/x-aiff");
            mimeList.Add("asc", "text/plain");
            mimeList.Add("atom", "application/atom+xml");
            mimeList.Add("au", "audio/basic");
            mimeList.Add("avi", "video/x-msvideo");
            mimeList.Add("bcpio", "application/x-bcpio");
            mimeList.Add("bin", "application/octet-stream");
            mimeList.Add("bmp", "image/bmp");
            mimeList.Add("cdf", "application/x-netcdf");
            mimeList.Add("cgm", "image/cgm");
            mimeList.Add("class", "application/octet-stream");
            mimeList.Add("cpio", "application/x-cpio");
            mimeList.Add("cpt", "application/mac-compactpro");
            mimeList.Add("csh", "application/x-csh");
            mimeList.Add("css", "text/css");
            mimeList.Add("dcr", "application/x-director");
            mimeList.Add("dif", "video/x-dv");
            mimeList.Add("dir", "application/x-director");
            mimeList.Add("djv", "image/vnd.djvu");
            mimeList.Add("djvu", "image/vnd.djvu");
            mimeList.Add("dll", "application/octet-stream");
            mimeList.Add("dmg", "application/octet-stream");
            mimeList.Add("dms", "application/octet-stream");
            mimeList.Add("doc", "application/msword");
            mimeList.Add("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            mimeList.Add("dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
            mimeList.Add("docm", "application/vnd.ms-word.document.macroEnabled.12");
            mimeList.Add("dotm", "application/vnd.ms-word.template.macroEnabled.12");
            mimeList.Add("dtd", "application/xml-dtd");
            mimeList.Add("dv", "video/x-dv");
            mimeList.Add("dvi", "application/x-dvi");
            mimeList.Add("dxr", "application/x-director");
            mimeList.Add("eps", "application/postscript");
            mimeList.Add("etx", "text/x-setext");
            mimeList.Add("exe", "application/octet-stream");
            mimeList.Add("ez", "application/andrew-inset");
            mimeList.Add("gif", "image/gif");
            mimeList.Add("gram", "application/srgs");
            mimeList.Add("grxml", "application/srgs+xml");
            mimeList.Add("gtar", "application/x-gtar");
            mimeList.Add("hdf", "application/x-hdf");
            mimeList.Add("hqx", "application/mac-binhex40");
            mimeList.Add("htm", "text/html");
            mimeList.Add("html", "text/html");
            mimeList.Add("ice", "x-conference/x-cooltalk");
            mimeList.Add("ico", "image/x-icon");
            mimeList.Add("ics", "text/calendar");
            mimeList.Add("ief", "image/ief");
            mimeList.Add("ifb", "text/calendar");
            mimeList.Add("iges", "model/iges");
            mimeList.Add("igs", "model/iges");
            mimeList.Add("jnlp", "application/x-java-jnlp-file");
            mimeList.Add("jp2", "image/jp2");
            mimeList.Add("jpe", "image/jpeg");
            mimeList.Add("jpeg", "image/jpeg");
            mimeList.Add("jpg", "image/jpeg");
            mimeList.Add("js", "application/x-javascript");
            mimeList.Add("kar", "audio/midi");
            mimeList.Add("latex", "application/x-latex");
            mimeList.Add("lha", "application/octet-stream");
            mimeList.Add("lzh", "application/octet-stream");
            mimeList.Add("m3u", "audio/x-mpegurl");
            mimeList.Add("m4a", "audio/mp4a-latm");
            mimeList.Add("m4b", "audio/mp4a-latm");
            mimeList.Add("m4p", "audio/mp4a-latm");
            mimeList.Add("m4u", "video/vnd.mpegurl");
            mimeList.Add("m4v", "video/x-m4v");
            mimeList.Add("mac", "image/x-macpaint");
            mimeList.Add("man", "application/x-troff-man");
            mimeList.Add("mathml", "application/mathml+xml");
            mimeList.Add("me", "application/x-troff-me");
            mimeList.Add("mesh", "model/mesh");
            mimeList.Add("mid", "audio/midi");
            mimeList.Add("midi", "audio/midi");
            mimeList.Add("mif", "application/vnd.mif");
            mimeList.Add("mov", "video/quicktime");
            mimeList.Add("movie", "video/x-sgi-movie");
            mimeList.Add("mp2", "audio/mpeg");
            mimeList.Add("mp3", "audio/mpeg");
            mimeList.Add("mp4", "video/mp4");
            mimeList.Add("mpe", "video/mpeg");
            mimeList.Add("mpeg", "video/mpeg");
            mimeList.Add("mpg", "video/mpeg");
            mimeList.Add("mpga", "audio/mpeg");
            mimeList.Add("ms", "application/x-troff-ms");
            mimeList.Add("msh", "model/mesh");
            mimeList.Add("mxu", "video/vnd.mpegurl");
            mimeList.Add("nc", "application/x-netcdf");
            mimeList.Add("oda", "application/oda");
            mimeList.Add("ogg", "application/ogg");
            mimeList.Add("pbm", "image/x-portable-bitmap");
            mimeList.Add("pct", "image/pict");
            mimeList.Add("pdb", "chemical/x-pdb");
            mimeList.Add("pdf", "application/pdf");
            mimeList.Add("pgm", "image/x-portable-graymap");
            mimeList.Add("pgn", "application/x-chess-pgn");
            mimeList.Add("pic", "image/pict");
            mimeList.Add("pict", "image/pict");
            mimeList.Add("png", "image/png");
            mimeList.Add("pnm", "image/x-portable-anymap");
            mimeList.Add("pnt", "image/x-macpaint");
            mimeList.Add("pntg", "image/x-macpaint");
            mimeList.Add("ppm", "image/x-portable-pixmap");
            mimeList.Add("ppt", "application/vnd.ms-powerpoint");
            mimeList.Add("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
            mimeList.Add("potx", "application/vnd.openxmlformats-officedocument.presentationml.template");
            mimeList.Add("ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");
            mimeList.Add("ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12");
            mimeList.Add("pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12");
            mimeList.Add("potm", "application/vnd.ms-powerpoint.template.macroEnabled.12");
            mimeList.Add("ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
            mimeList.Add("ps", "application/postscript");
            mimeList.Add("qt", "video/quicktime");
            mimeList.Add("qti", "image/x-quicktime");
            mimeList.Add("qtif", "image/x-quicktime");
            mimeList.Add("ra", "audio/x-pn-realaudio");
            mimeList.Add("ram", "audio/x-pn-realaudio");
            mimeList.Add("ras", "image/x-cmu-raster");
            mimeList.Add("rdf", "application/rdf+xml");
            mimeList.Add("rgb", "image/x-rgb");
            mimeList.Add("rm", "application/vnd.rn-realmedia");
            mimeList.Add("roff", "application/x-troff");
            mimeList.Add("rtf", "text/rtf");
            mimeList.Add("rtx", "text/richtext");
            mimeList.Add("sgm", "text/sgml");
            mimeList.Add("sgml", "text/sgml");
            mimeList.Add("sh", "application/x-sh");
            mimeList.Add("shar", "application/x-shar");
            mimeList.Add("silo", "model/mesh");
            mimeList.Add("sit", "application/x-stuffit");
            mimeList.Add("skd", "application/x-koan");
            mimeList.Add("skm", "application/x-koan");
            mimeList.Add("skp", "application/x-koan");
            mimeList.Add("skt", "application/x-koan");
            mimeList.Add("smi", "application/smil");
            mimeList.Add("smil", "application/smil");
            mimeList.Add("snd", "audio/basic");
            mimeList.Add("so", "application/octet-stream");
            mimeList.Add("spl", "application/x-futuresplash");
            mimeList.Add("src", "application/x-wais-source");
            mimeList.Add("sv4cpio", "application/x-sv4cpio");
            mimeList.Add("sv4crc", "application/x-sv4crc");
            mimeList.Add("svg", "image/svg+xml");
            mimeList.Add("swf", "application/x-shockwave-flash");
            mimeList.Add("t", "application/x-troff");
            mimeList.Add("tar", "application/x-tar");
            mimeList.Add("tcl", "application/x-tcl");
            mimeList.Add("tex", "application/x-tex");
            mimeList.Add("texi", "application/x-texinfo");
            mimeList.Add("texinfo", "application/x-texinfo");
            mimeList.Add("tif", "image/tiff");
            mimeList.Add("tiff", "image/tiff");
            mimeList.Add("tr", "application/x-troff");
            mimeList.Add("tsv", "text/tab-separated-values");
            mimeList.Add("txt", "text/plain");
            mimeList.Add("ustar", "application/x-ustar");
            mimeList.Add("vcd", "application/x-cdlink");
            mimeList.Add("vrml", "model/vrml");
            mimeList.Add("vxml", "application/voicexml+xml");
            mimeList.Add("wav", "audio/x-wav");
            mimeList.Add("wbmp", "image/vnd.wap.wbmp");
            mimeList.Add("wbmxl", "application/vnd.wap.wbxml");
            mimeList.Add("wml", "text/vnd.wap.wml");
            mimeList.Add("wmlc", "application/vnd.wap.wmlc");
            mimeList.Add("wmls", "text/vnd.wap.wmlscript");
            mimeList.Add("wmlsc", "application/vnd.wap.wmlscriptc");
            mimeList.Add("wrl", "model/vrml");
            mimeList.Add("xbm", "image/x-xbitmap");
            mimeList.Add("xht", "application/xhtml+xml");
            mimeList.Add("xhtml", "application/xhtml+xml");
            mimeList.Add("xls", "application/vnd.ms-excel");
            mimeList.Add("xml", "application/xml");
            mimeList.Add("xpm", "image/x-xpixmap");
            mimeList.Add("xsl", "application/xml");
            mimeList.Add("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            mimeList.Add("xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template");
            mimeList.Add("xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12");
            mimeList.Add("xltm", "application/vnd.ms-excel.template.macroEnabled.12");
            mimeList.Add("xlam", "application/vnd.ms-excel.addin.macroEnabled.12");
            mimeList.Add("xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12");
            mimeList.Add("xslt", "application/xslt+xml");
            mimeList.Add("xul", "application/vnd.mozilla.xul+xml");
            mimeList.Add("xwd", "image/x-xwindowdump");
            mimeList.Add("xyz", "chemical/x-xyz");
            mimeList.Add("zip", "application/zip");
            MIMETypesDictionary = mimeList;
        }

        public static string GetMIME(string extension)
        {
            if (MIMETypesDictionary.ContainsKey(extension))
            {
                return MIMETypesDictionary[extension];
            }
            return "application/octet-stream";
        }
    }
}

