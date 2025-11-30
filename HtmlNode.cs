using System;
using System.Collections.Generic;
using System.Text;

namespace FactoorSharp.FacturXDocumentationRenderer
{
    internal class HtmlNode
    {
        internal string Title { get; set; } = String.Empty;
        internal List<HtmlNode> Children { get; set; } = [];
    }
}
