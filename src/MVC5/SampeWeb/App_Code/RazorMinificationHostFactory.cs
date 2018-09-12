using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Razor;
using System.Web.Razor.Parser;
using System.Web.Razor.Text;
using System.Web.WebPages.Razor;
using RazorMinifier;

namespace SampeWeb
{
    /**
     * Replace the default MVC factory with new host factory
     * Which contains minifying parser
     * 
     * */


    public class RazorMinificationHostFactory : MvcWebRazorHostFactory
    {
        private static MinifyOptions _option = null;

        private static MinifyOptions Option
        {
            get
            {
                if (_option == null)
                {
                    _option = new MinifyOptions();
                }

                return _option;
            }
        }


        /// <summary>
        /// Creates the host.
        /// </summary>
        /// <param name="virtualPath">The virtual path</param>
        /// <param name="physicalPath">The physical path</param>
        /// <returns></returns>
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            WebPageRazorHost host = base.CreateHost(virtualPath, physicalPath);

            return !host.IsSpecialPage ? new RazorMinificationHost(virtualPath, physicalPath) : host;
        }

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

        private class MinifyHtmlMarkupParser : HtmlMarkupParser
        {
            public override void ParseDocument()
            {
                if (Context == null)
                {
                    throw new NullReferenceException("Context");
                }

                // read the current source
                // optimize it and return back to the context
                string content = this.Context.Source.ReadToEnd();

                content = RazorMinification.MinifyHtml(content, new MinifyOptions());

                //content = content.Replace("abc", "<b>Hahaha</b>");

                this.Context.Source = new TextDocumentReader(new SeekableTextReader(content));

                base.ParseDocument();
            }
        }



    }



}
