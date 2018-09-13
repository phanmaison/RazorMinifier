using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RazorMinifier
{
    #region RazorMinification

    /// <summary>
    /// Utility to minify the content
    /// </summary>
    public static class RazorMinify
    {
        #region Minify

        /// <summary>
        /// Minifies the given HTML string.
        /// </summary>
        /// <param name="htmlContents">The html to minify.</param>
        /// <param name="option">The features</param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string Minify(string htmlContents, MinifyOptions option)
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

            // ReplaceTextLine (@: => <text></text>)
            htmlContents = ReplaceTextLine(htmlContents);

            if (!option.IgnoreJsComments)
                // double slash (//) not start with semi colon (:)
                htmlContents = Regex.Replace(htmlContents, @"[^:]//(.*?)\r?\n", "", RegexOptions.Singleline);

            // Replace #region and #endregion
            if (!option.IgnoreRegion)
            {
                htmlContents = Regex.Replace(htmlContents, @"#region(.*?)\r?\n", "", RegexOptions.Singleline);
                htmlContents = Regex.Replace(htmlContents, @"#endregion(.*?)\r?\n", "", RegexOptions.Singleline);
            }

            // warning: should never replace html comment
            // Replace comments
            if (!option.IgnoreHtmlComments)
            {
                htmlContents = Regex.Replace(htmlContents,
                    option.IgnoreKnockoutComments ?
                        @"<!--(?!(\[|\s*#include))(?!ko .*)(?!\/ko)(.*?)-->" :
                        @"<!--(?!(\[|\s*#include))(.*?)-->", "");
            }

            // Minify the string
            htmlContents = Regex.Replace(htmlContents, @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/", "");

            // Replace spaces between quotes
            htmlContents = Regex.Replace(htmlContents, @"\s+", " ");

            // Replace line breaks
            htmlContents = Regex.Replace(htmlContents, @"\s*\n\s*", "\n");

            // Replace spaces between brackets
            htmlContents = Regex.Replace(htmlContents, @"\s*\>\s*\<\s*", "><");

            // single-line doctype must be preserved
            //var firstEndBracketPosition = htmlContents.IndexOf(">", StringComparison.Ordinal);
            //if (firstEndBracketPosition >= 0)
            //{
            //    htmlContents = htmlContents.Remove(firstEndBracketPosition, 1);
            //    htmlContents = htmlContents.Insert(firstEndBracketPosition, ">");
            //}

            // Put back special keys
            //htmlContents = htmlContents.Replace("{{{SLASH_STAR}}}", "/*");

            return htmlContents.Trim();
        }

        #endregion Minify

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

    #endregion RazorMinification

    #region MinifyOptions

    /// <summary>
    /// Option for minifying
    /// </summary>
    public class MinifyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinifyOptions"/> class
        /// </summary>
        public MinifyOptions() { }

        /// <summary>
        /// Check the arguments passed in to determine if we should enable or disable any features.
        /// </summary>
        /// <param name="args">The arguments passed in.</param>
        public MinifyOptions(string[] args)
        {
            if (args.Contains("ignorehtmlcomments"))
                IgnoreHtmlComments = true;

            if (args.Contains("ignorejscomments"))
                IgnoreJsComments = true;

            if (args.Contains("ignoreregion"))
                this.IgnoreRegion = true;

            if (args.Contains("ignoreknockoutcomments"))
                IgnoreKnockoutComments = true;


            // maxlength=XXXXX
            int maxLength = 0;

            foreach (string arg in args)
            {
                if (arg.Contains("maxlength="))
                {
                    int.TryParse(arg.Split("=".ToCharArray())[1], out maxLength);
                }
            }

            MaxLength = maxLength;
        }

        /// <summary>
        /// Should we ignore the JavaScript comments and not minify?
        /// </summary>
        public bool IgnoreJsComments { get; set; }

        /// <summary>
        /// Should we ignore the html comments and not minify?
        /// </summary>
        public bool IgnoreHtmlComments { get; set; }

        /// <summary>
        /// If the #region should be ignored
        /// </summary>
        /// <value>
        ///   <c>true</c> if [ignore region]; otherwise, <c>false</c>
        /// </value>
        public bool IgnoreRegion { get; set; }

        /// <summary>
        /// Should we ignore knockout comments?
        /// </summary>
        public bool IgnoreKnockoutComments { get; set; }

        /// <summary>
        /// Property for the max character count
        /// </summary>
        public int MaxLength { get; set; }
    }

    #endregion MinifyOptions
}
