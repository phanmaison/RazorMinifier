using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RazorMinifier;

namespace RazorMinifierConsole
{
    class Program
    {
        /**
         * Note: the RazorMinifier.Core source code is embedded so that there will be
         * only 1 exe for deployment.
         * There is no need of RazorMinifier.Core.dll             
         * */

        #region Main

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide folder path or file(s) to process");
            }
            else
            {
                // Determine which features to enable or disable
                var features = new MinifyOptions(args);
                foreach (var arg in args)
                {
                    if (Directory.Exists(arg))
                    {
                        ProcessDirectory(features, arg);
                    }
                    else if (File.Exists(arg))
                    {
                        ProcessFile(features, arg);
                    }
                }
                Console.WriteLine("Razor Minification Complete");
            }

            //Console.ReadLine();
        }

        #endregion Main

        #region ProcessDirectory

        /// <summary>
        /// Minify all files in a given file
        /// </summary>
        /// <param name="features">Features object</param>
        /// <param name="folderPath">The path to the folder</param>
        public static void ProcessDirectory(MinifyOptions features, string folderPath)
        {
            // Loop through the files in the folder and look for any of the following extensions
            var subFolders = GetDirectories(folderPath);

            foreach (string folder in subFolders)
            {
                var filePaths = Directory.GetFiles(folder);
                foreach (var filePath in filePaths)
                {
                    if (IsHtmlFile(filePath))
                        ProcessFile(features, filePath);
                }
            }
        }

        #endregion ProcessDirectory

        #region ProcessFile

        /// <summary>
        /// Minify a given file
        /// </summary>
        /// <param name="features">Features object</param>
        /// <param name="filePath">The path to the file</param>
        public static void ProcessFile(MinifyOptions features, string filePath)
        {
            // Minify contents
            string minifiedContents = MinifyHtml(filePath, features);
            // Write to the same file
            File.WriteAllText(filePath, minifiedContents, Encoding.UTF8);
            Console.WriteLine("Minified file : " + filePath);
        }

        #endregion ProcessFile

        #region GetDirectories

        /// <summary>
        /// Gets the directories and subdirectories for a given path.
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>A list of the directories.</returns>
        public static IEnumerable<string> GetDirectories(string path)
        {
            // Get all subdirectories
            IEnumerable<string> directories = from subdirectory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories) select subdirectory;

            // Add the subdirectories
            IList<string> allDirectories = directories as IList<string> ?? directories.ToList();

            // Add the root folder
            allDirectories.Add(path);

            return allDirectories;
        }

        #endregion GetDirectories

        #region MinifyHtml

        /// <summary>
        /// Minifies the contents of the given view.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="features">The features</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public static string MinifyHtml(string filePath, MinifyOptions features)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return RazorMinification.MinifyHtml(reader.ReadToEnd(), features);
            }
        }

        #endregion MinifyHtml


        /// <summary>
        /// Checks if a file extension matches 
        /// any given types for an ASP.net application
        /// </summary>
        /// <param name="value">The html file name.</param>
        /// <returns>A boolean if the file is an html file.</returns>
        private static bool IsHtmlFile(string value)
        {
            var filePath = value.ToLower();

            return filePath.EndsWith(".cshtml") ||
                    filePath.EndsWith(".vbhtml") ||
                    filePath.EndsWith(".aspx") ||
                    filePath.EndsWith(".html") ||
                    filePath.EndsWith(".htm") ||
                    filePath.EndsWith(".ascx") ||
                    filePath.EndsWith(".master") ||
                    filePath.EndsWith(".inc");
        }
    }
}
