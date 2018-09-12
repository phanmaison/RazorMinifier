using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RazorMinifier
{
    public static class RazorMinification
    {
        #region MinifyHtml

        /// <summary>
        /// Minifies the given HTML string.
        /// </summary>
        /// <param name="htmlContents">The html to minify.</param>
        /// <param name="option">The features</param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string MinifyHtml(string htmlContents, MinifyOptions option)
        {
            // to be removed later
            //// First, remove all JavaScript comments
            //if (!features.IgnoreJsComments)
            //{
            //    htmlContents = RemoveJavaScriptComments(htmlContents);
            //}

            // remove block comment /* */
            htmlContents = Regex.Replace(htmlContents, @"/\*(.|\n)*?\*/", "", RegexOptions.Multiline);

            // remove block comment @* *@ to save some more space
            htmlContents = Regex.Replace(htmlContents, @"@\*(.|\n)*?\*@", "", RegexOptions.Multiline);

            // Minify the string
            htmlContents = Regex.Replace(htmlContents, @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/", "");

            // ReplaceTextLine (@: => <text></text>)
            htmlContents = ReplaceTextLine(htmlContents);

            // double slash (//) not start with semi colon (:)
            htmlContents = Regex.Replace(htmlContents, @"[^:]//(.*?)\r?\n", "", RegexOptions.Singleline);

            // Replace #region and #endregion
            htmlContents = Regex.Replace(htmlContents, @"#region(.*?)\r?\n", "", RegexOptions.Singleline);
            htmlContents = Regex.Replace(htmlContents, @"#endregion(.*?)\r?\n", "", RegexOptions.Singleline);

            // Replace spaces between quotes
            htmlContents = Regex.Replace(htmlContents, @"\s+", " ");

            // Replace line breaks
            htmlContents = Regex.Replace(htmlContents, @"\s*\n\s*", "\n");

            // Replace spaces between brackets
            htmlContents = Regex.Replace(htmlContents, @"\s*\>\s*\<\s*", "><");

            // never replace html comment
            //// Replace comments
            //if (!features.IgnoreHtmlComments)
            //{
            //    if (features.IgnoreKnockoutComments)
            //    {
            //        htmlContents = Regex.Replace(htmlContents, @"<!--(?!(\[|\s*#include))(?!ko .*)(?!\/ko)(.*?)-->", "");
            //    }
            //    else
            //    {
            //        htmlContents = Regex.Replace(htmlContents, @"<!--(?!(\[|\s*#include))(.*?)-->", "");
            //    }
            //}

            // single-line doctype must be preserved
            var firstEndBracketPosition = htmlContents.IndexOf(">", StringComparison.Ordinal);
            if (firstEndBracketPosition >= 0)
            {
                htmlContents = htmlContents.Remove(firstEndBracketPosition, 1);
                htmlContents = htmlContents.Insert(firstEndBracketPosition, ">");
            }

            // Put back special keys
            //htmlContents = htmlContents.Replace("{{{SLASH_STAR}}}", "/*");

            return htmlContents.Trim();
        }

        #endregion MinifyHtml

        #region ReplaceTextLine

        /// <summary>
        /// Replaces new comment lines (@:) in Razor with HTML text tag
        /// </summary>
        /// <param name="htmlContents">The html to minify</param>
        /// <returns>A string with all comment lines replaced with text tags</returns>
        private static string ReplaceTextLine(string htmlContents)
        {
            var sb = new StringBuilder();
            foreach (var line in Regex.Split(htmlContents, "\r\n"))
            {
                if (line.Contains("@:"))
                    sb.AppendLine(line.Replace("@:", "<text>") + "</text>");
                else
                    sb.AppendLine(line);
            }
            return sb.ToString();
        }

        #endregion ReplaceTextLine
    }
}
