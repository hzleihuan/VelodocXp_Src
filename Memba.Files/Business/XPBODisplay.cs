/*
 * Copyright © 2005-2008 Memba SA. All rights reserved.
 * 
 * This file is part of Velodoc XP Edition.
 * 
 * Velodoc XP Edition is open-source software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 2 of the License, or (at your option) any later version.
 * 
 * Velodoc XP Edition is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with Velodoc XP Edition.
 * If not, see <http://www.gnu.org/licenses/>.
 * 
 * For more information, please contact Memba at <http://www.memba.com>.
 * You can find more information about Velodoc at <http://www.velodoc.com> and <http://www.velodoc.com/help>.
 * You can try Velodoc online at <http://www.velodoc.net>.
 *
*/
using System;
using System.Web;
using System.Globalization; //CultureInfo
using System.Text.RegularExpressions; //Regex

namespace Memba.Files.Business
{
    /// <summary>
    /// Summary description for XPBODisplay
    /// </summary>
    public static class BODisplay
    {
        /// <summary>
        /// H encoding
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Encode(object o)
        {
            if (o == null)
                return String.Empty;

            //think about filtering some characters
            return HttpUtility.HtmlEncode(o.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Encode(string s)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            return HttpUtility.HtmlEncode(s);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Encode(bool b, string trueValue, string falseValue)
        {
            return b ? Encode(trueValue) : Encode(falseValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string TextEllipsis(string text, int size)
        {
            const string ELLIPSIS = "...";
            char[] PUNCTUATION = { ' ', ',', ';', '.', ':', '?', '!', '(', ')' };

            string sRet = null;

            if (text != null)
            {
                sRet = text.Trim();
                if (!String.IsNullOrEmpty(sRet))
                {
                    if (sRet.Length > size)
                    {
                        sRet = sRet.Substring(0, size).Trim(PUNCTUATION);
                        int iPos = sRet.LastIndexOfAny(PUNCTUATION);
                        if ((iPos > -1) && (iPos > size - 3))
                            sRet = sRet.Substring(0, iPos).Trim(PUNCTUATION);
                        sRet += ELLIPSIS;
                    }
                }
            }

            return sRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// See: http://regexlib.com/CheatSheet.aspx
        /// See: http://www.4guysfromrolla.com/webtech/073000-1.shtml
        /// See: http://www.regular-expressions.info/reference.html
        /// </remarks>
        /// <param name="?"></param>
        /// <returns></returns>
        public static string RemoveAllHtmlTags(string htmlText)
        {
            //First remove dangerouns tags with inner content
            string sRet = RemoveDangerousHtmlTags(htmlText, false);

            //Embedded event handlers will be removed by the following anyway

            //Remove all other tags, but leave inner content
            Regex rx = new Regex(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            sRet = rx.Replace(sRet, String.Empty); //remove html tags 

            rx = null;
            return sRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="htmlText"></param>
        /// <returns></returns>
        public static string RemoveDangerousHtmlTags(string htmlText, bool removeEmbeddedEventHandlers)
        {
            //remove <script .../> tags
            Regex rx = new Regex(@"<script[^>]*/[\s]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string sRet = rx.Replace(htmlText, String.Empty);

            //remove <script ...>....</script> tags
            rx = new Regex(@"<script[^>]*>[^\u0000]*?</script[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            sRet = rx.Replace(sRet, String.Empty);

            //remove <embed ...>....</embed> tags
            rx = new Regex(@"<embed[^>]*>[^\u0000]*?</embed[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            sRet = rx.Replace(sRet, String.Empty);

            //remove <object ...>....</object> tags
            rx = new Regex(@"<object[^>]*>[^\u0000]*?</object[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            sRet = rx.Replace(sRet, String.Empty);

            //remove <applet ...>....</applet> tags
            rx = new Regex(@"<applet[^>]*>[^\u0000]*?</applet[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            sRet = rx.Replace(sRet, String.Empty);

            if (removeEmbeddedEventHandlers)
            {
                //remove any attributes in the form on*="..."
                //Non escaped regular expression is (<[^>]*)(\bon\w+\s*=\s*(\"|\')[^\3]*?\3)([^>]*>)
                rx = new Regex(@"(<[^>]*)(\bon\w+\s*=\s*(\""|\')[^\3]*?\3)([^>]*>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                sRet = rx.Replace(sRet, "$1$4");
            }

            rx = null;
            return sRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Replaces multiple CRLF with single &lt;br/&gt;</remarks>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string ConvertToHtml(string plainText)
        {
            Regex rx = new Regex("\\r\\n");
            string sRet = rx.Replace(plainText, "<br />");

            rx = null;
            //return "<p>" + sRet + "</p>"; //You may want to display in a div or td
            return sRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeSpan"></param>
        public static string TimeSpanFormat(TimeSpan timeSpan)
        {
            //see: http://msdn2.microsoft.com/en-us/library/97x6twsz(VS.80).aspx

            if (timeSpan.Days > 0) //if timeSpan is too large dtDummyDate.Add will throw an exception
                return Properties.Resources.FormatTimeSpanDays;

            DateTime dtDummyDate = new DateTime(2006, 2, 14);
            dtDummyDate = dtDummyDate.Add(timeSpan);
            if (timeSpan.Hours > 0)
                return String.Format(CultureInfo.CurrentUICulture, "{0:H:mm:ss}", dtDummyDate);
            else
                return String.Format(CultureInfo.CurrentUICulture, "{0:mm:ss}", dtDummyDate);
        }
        /// <summary>
        /// Formats a number between 0 and 1 into a percentage
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static string PercentFormat(double ratio)
        {
            if ((ratio < 0) || (ratio > 1))
                throw new ArgumentOutOfRangeException("ratio");

            //see: http://msdn2.microsoft.com/en-us/library/427bttx3(VS.80).aspx
            return String.Format(CultureInfo.CurrentUICulture, "{0:P0}", ratio);
        }
        /// <summary>
        /// Formats file sizes in Kb, Mb or Gb as required. Note that formatting is localized.
        /// </summary>
        /// <param name="arrBytes"></param>
        /// <param name="byteFormat"></param>
        /// <returns></returns>
        public static string SizeFormat(long bytes, ByteFormat byteFormat)
        {
            //see: http://msdn2.microsoft.com/en-us/library/427bttx3(VS.80).aspx

            double dBytes = (double)bytes;

            dBytes = dBytes / 1024;
            if ((byteFormat == ByteFormat.Kb) || ((byteFormat == ByteFormat.Adapt) && (dBytes < 1024)))
                return String.Format(Properties.Resources.Culture, Properties.Resources.FormatKB, dBytes);

            dBytes = dBytes / 1024;
            if ((byteFormat == ByteFormat.Mb) || ((byteFormat == ByteFormat.Adapt) && (dBytes < 1024)))
                return String.Format(Properties.Resources.Culture, Properties.Resources.FormatMB, dBytes);

            dBytes = dBytes / 1024;
            return String.Format(Properties.Resources.Culture, Properties.Resources.FormatGB, dBytes);
        }
        /// <summary>
        /// Formats transfer bandwidth in Kb/s, Mb/s or Gb/s as required. Note that formatting is localized.
        /// </summary>
        /// <param name="bytesPerSecond"></param>
        /// <param name="byteFormat"></param>
        /// <returns></returns>
        public static string BandwidthFormat(double bytesPerSecond, ByteFormat byteFormat)
        {
            //see: http://msdn2.microsoft.com/en-us/library/427bttx3(VS.80).aspx

            bytesPerSecond = bytesPerSecond / 1024;
            if ((byteFormat == ByteFormat.Kb) || ((byteFormat == ByteFormat.Adapt) && (bytesPerSecond < 1024)))
                return String.Format(Properties.Resources.Culture, Properties.Resources.FormatKBPerSecond, bytesPerSecond);

            bytesPerSecond = bytesPerSecond / 1024;
            if ((byteFormat == ByteFormat.Mb) || ((byteFormat == ByteFormat.Adapt) && (bytesPerSecond < 1024)))
                return String.Format(Properties.Resources.Culture, Properties.Resources.FormatMBPerSecond, bytesPerSecond);

            bytesPerSecond = bytesPerSecond / 1024;
            return String.Format(Properties.Resources.Culture, Properties.Resources.FormatGBPerSecond, bytesPerSecond);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public enum ByteFormat
    {
        Kb,
        Mb,
        Gb,
        Adapt
    }
}