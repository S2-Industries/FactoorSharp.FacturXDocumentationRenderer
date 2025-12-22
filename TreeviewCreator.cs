using System;
using System.Collections.Generic;
using System.Text;
using FactoorSharp.FacturXDocumentationParser;

namespace FactoorSharp.FacturXDocumentationRenderer
{
    internal class TreeviewCreator
    {
        internal static string CreateTreeview(List<Element> rootElements)
        {
            var sb = new StringBuilder();

            sb.Append("<ul class=\"list-unstyled mb-0\">");

            foreach (var element in rootElements)
            {
                _BuildNode(element, sb, 0);
            }

            sb.Append("</ul>");
            return sb.ToString();
        } // !CreateTreeview()


        private static void _BuildNode(Element element, StringBuilder sb, int depth)
        {                        
            string marginClass = $"ms-3";

            bool hasChildren = element.Children.Count > 0;
            string collapseId = $"collapse-children-of-{element.Id}";

            sb.Append("<li class=\"tree-node mb-0\">");

            // Header-Zeile
            sb.Append($"<div class=\"node-header d-flex align-items-center {marginClass} fs-7 lh-1\">");

            if (hasChildren)
            {
                sb.Append(
                    $"<i class=\"bi bi-chevron-right text-secondary toggle-icon me-1\" " +
                    $"data-bs-toggle=\"collapse\" data-bs-target=\"#{collapseId}\" " +
                    $"aria-expanded=\"false\" aria-controls=\"{collapseId}\"></i>"
                );
            }
            else
            {
                sb.Append("<i class=\"bi bi-chevron-right toggle-icon invisible me-1\" style=\"pointer-events:none;\"></i>");
            }

            sb.Append(
                $"<a class=\"element-link\" href=\"#{element.AdditionalData["Id"]}\" " +
                $"class=\"flex-grow-1 text-body text-decoration-none rounded-1 text-truncate fs-7 lh-1\">" +
                $"{element.Name}</a>"
            );

            sb.Append("</div>"); // header

            // Child-UL
            if (hasChildren)
            {
                sb.Append(
                    $"<ul class=\"node-children list-unstyled collapse {marginClass}\" id=\"{collapseId}\">"
                );

                foreach (var child in element.Children)
                {
                    _BuildNode(child, sb, depth + 1);
                }

                sb.Append("</ul>");
            }

            sb.Append("</li>");
        } // !_BuildNode()
    }
}
