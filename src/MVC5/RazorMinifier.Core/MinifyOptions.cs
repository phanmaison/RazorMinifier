using System.Linq;

namespace RazorMinifier
{
    public class MinifyOptions
    {
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
        public bool IgnoreJsComments { get; set; } = false;

        /// <summary>
        /// Should we ignore the html comments and not minify?
        /// </summary>
        public bool IgnoreHtmlComments { get; set; } = false;

        public bool IgnoreRegion { get; set; } = false;

        /// <summary>
        /// Should we ignore knockout comments?
        /// </summary>
        public bool IgnoreKnockoutComments { get; set; } = false;

        /// <summary>
        /// Property for the max character count
        /// </summary>
        public int MaxLength { get; set; } = 0;
    }
}
