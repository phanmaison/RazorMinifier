using System;
using System.Collections.Generic;
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
            // remove block comment @* *@ to save some more space
            htmlContents = Regex.Replace(htmlContents, @"@\*(.|\n)*?\*@", "", RegexOptions.Multiline);

            // ReplaceTextLine (@: => <text></text>)
            htmlContents = ReplaceTextLine(htmlContents);

            if (!option.IgnoreJsComments)
            {
                // remove block comment /* */
                htmlContents = Regex.Replace(htmlContents, @"/\*(.|\n)*?\*/", "", RegexOptions.Multiline);

                // double slash (//) not start with semi colon (:)
                htmlContents = Regex.Replace(htmlContents, @"[^:]//(.*?)\r?\n", "", RegexOptions.Singleline);
            }

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

            // Ensure that the max length is less than 65K characters
            htmlContents = EnsureMaxLength(htmlContents, option.MaxLength);

            // Re-add the @model declaration
            htmlContents = RearrangeDeclarations(htmlContents);


            return htmlContents.Trim();
        }

        #endregion Minify

        #region RearrangeDeclarations

        /// <summary>
        /// Find any occurences of the particular Razor keywords
        /// and add a new line or move to the top of the view.
        /// </summary>
        /// <param name="fileContents">The contents of the file</param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string RearrangeDeclarations(string fileContents)
        {
            // A list of all the declarations
            Dictionary<string, bool> declarations = new Dictionary<string, bool>
            {
                {"@model ", true},
                {"@using ", false},
                {"@inherits ", false},
                {"@helper ", false} // robin: add helper block code
            };

            // Loop through the declarations
            foreach (var declaration in declarations)
            {
                fileContents = RearrangeDeclarations(fileContents, declaration.Key, declaration.Value);
            }

            return fileContents;
        }

        /// <summary>
        /// Re-arranges the razor syntax on its own line.
        /// It seems to break the razor engine if this isnt on
        /// it's own line in certain cases.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="declaration">The declaration keywords that will cause a new line split.</param>
        /// <param name="bringToTop">if set to <c>true</c> [bring to top]</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        private static string RearrangeDeclarations(string fileContents, string declaration, bool bringToTop)
        {
            // Find possible multiple occurences in the file contents
            MatchCollection matches = Regex.Matches(fileContents, declaration);

            // Loop through the matches
            int alreadyMatched = 0;
            foreach (Match match in matches)
            {
                int position = declaration.Length;
                int declarationPosition = match.Index;

                // If we have more than one match, we need to keep the counter moving everytime we add a new line
                if (matches.Count > 1 && alreadyMatched > 0)
                {
                    // Cos we added one or more new line break \n\r
                    declarationPosition += (2 * alreadyMatched);
                }

                while (true)
                {
                    // Move one forward
                    position += 1;
                    if (position > fileContents.Length) break;
                    string substring = fileContents.Substring(declarationPosition, position);

                    // Check if it contains a whitespace at the end
                    if (!substring.EndsWith(", ") && (substring.EndsWith(" ")
                        || substring.EndsWith(">") && fileContents.Substring(declarationPosition + position - 1, 2) != ">>"))
                    {
                        if (bringToTop)
                        {
                            // First replace the occurence
                            fileContents = fileContents.Replace(substring, "");

                            // Next move it to the top on its own line
                            fileContents = substring + Environment.NewLine + fileContents;
                            break;
                        }
                        else
                        {
                            // Add a line break afterwards
                            fileContents = fileContents.Replace(substring, substring + Environment.NewLine);
                            alreadyMatched++;
                            break;
                        }
                    }
                }
            }

            return fileContents;
        }

        #endregion RearrangeDeclarations

        #region EnsureMaxLength

        /// <summary>
        /// Ensure that the max character count is less than 65K.
        /// If so, break onto the next line.
        /// </summary>
        /// <param name="htmlContents">The minified HTML</param>
        /// <param name="maxLength">The maximum length</param>
        public static string EnsureMaxLength(string htmlContents, int maxLength)
        {
            if (maxLength > 0)
            {
                int htmlLength = htmlContents.Length;
                int currentMaxLength = maxLength;

                while (htmlLength > currentMaxLength)
                {
                    var position = htmlContents.LastIndexOf("><", currentMaxLength, StringComparison.Ordinal);
                    htmlContents = htmlContents.Substring(0, position + 1) + "\r\n" + htmlContents.Substring(position + 1);
                    currentMaxLength += maxLength;
                }
            }
            return htmlContents;
        }

        #endregion EnsureMaxLength

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
        public MinifyOptions()
        {
        }

        /// <summary>
        /// Check the arguments passed in to determine if we should enable or disable any features.
        /// </summary>
        /// <param name="args">The arguments passed in.</param>
        public MinifyOptions(string[] args)
        {
            // should always ignore html comments
            //if (args.Contains("ignorehtmlcomments"))
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
