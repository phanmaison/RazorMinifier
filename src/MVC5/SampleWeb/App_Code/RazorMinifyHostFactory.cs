using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Razor;
using System.Web.Razor.Parser;
using System.Web.Razor.Text;
using System.Web.WebPages.Razor;

namespace RazorMinifier
{
    /**
     * Replace the default MVC factory with new host factory
     * Which contains minifying parser
     * 
     * */

    public class RazorMinifyHostFactory : MvcWebRazorHostFactory
    {
        #region Option

        private static MinifyOptions _option;

        /// <summary>
        /// Gets the option
        /// </summary>
        /// <value>
        /// The option
        /// </value>
        private static MinifyOptions Option
        {
            get
            {
                if (_option == null)
                {
                    _option = new MinifyOptions();
                    // customize the default option
                }

                return _option;
            }
        }
        #endregion Option

        #region DisableMinify

        /// <summary>
        /// If the minifying is disabled
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable minify]; otherwise, <c>false</c>
        /// </value>
        private static bool DisableMinify
        {
            get { return ConfigurationManager.AppSettings["RazorMinifier:Disabled"] == "true"; }
        }

        #endregion DisableMinify

        #region CreateHost

        /// <summary>
        /// Creates the host.
        /// </summary>
        /// <param name="virtualPath">The virtual path</param>
        /// <param name="physicalPath">The physical path</param>
        /// <returns></returns>
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            if (DisableMinify) return base.CreateHost(virtualPath, physicalPath);

            WebPageRazorHost host = base.CreateHost(virtualPath, physicalPath);

            return !host.IsSpecialPage ? new RazorMinificationHost(virtualPath, physicalPath) : host;
        }

        #endregion CreateHost

        #region RazorMinificationHost

        /// <summary>
        /// Container for razor parser
        /// </summary>
        private class RazorMinificationHost : MvcWebPageRazorHost
        {
            public RazorMinificationHost(string virtualPath, string physicalPath)
                : base(virtualPath, physicalPath)
            {
            }

            public override ParserBase DecorateMarkupParser(ParserBase incomingMarkupParser)
            {
                return new MinifyHtmlMarkupParser();
            }


            public override ParserBase CreateMarkupParser()
            {
                return new MinifyHtmlMarkupParser();
            }
        }

        #endregion RazorMinificationHost

        #region MinifyHtmlMarkupParser

        /// <summary>
        /// Overrided html markup parser
        /// </summary>
        /// <seealso cref="System.Web.Razor.Parser.HtmlMarkupParser" />
        private class MinifyHtmlMarkupParser : HtmlMarkupParser
        {
            public override void ParseDocument()
            {
                if (Context == null)
                    throw new NullReferenceException("Context");

                if (!DisableMinify)
                {
                    // read the current source
                    // minify it and return back to the context
                    string content = this.Context.Source.ReadToEnd();

                    content = RazorMinify.Minify(content, RazorMinifyHostFactory.Option);

                    this.Context.Source = new TextDocumentReader(new SeekableTextReader(content));
                }

                // continue the normal flow
                base.ParseDocument();
            }
        }

        #endregion MinifyHtmlMarkupParser

    }



}
