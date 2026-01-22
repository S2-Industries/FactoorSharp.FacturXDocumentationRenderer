using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FactoorSharp.FacturXDocumentationParser;

namespace FactoorSharp.FacturXDocumentationRenderer
{
    /// <summary>
    /// Generates an HTML list of all elements with BT numbers, sorted by their BT number.
    /// </summary>
    internal class BTListGenerator
    {
        /// <summary>
        /// Creates a complete HTML document with a ul/li list of all elements with BT numbers.
        /// </summary>
        /// <param name="rootElements">The hierarchical list of root elements to parse.</param>
        /// <param name="baseTemplate">The base HTML template to use.</param>
        /// <returns>Complete HTML code containing the sorted BT list.</returns>
        internal static string CreateBTList(List<Element> rootElements, string baseTemplate)
        {
            if (rootElements == null)
            {
                throw new ArgumentNullException(nameof(rootElements));
            }

            if (string.IsNullOrWhiteSpace(baseTemplate))
            {
                throw new ArgumentException("Base template must not be empty.", nameof(baseTemplate));
            }

            // Collect all elements with BT numbers
            var btElements = new List<(Element Element, int BtNumber)>();

            foreach (var rootElement in rootElements)
            {
                rootElement.Traverse(
                    element => element.Children,
                    element =>
                    {
                        if (!string.IsNullOrWhiteSpace(element.Id))
                        {
                            var btNumber = _ExtractBTNumber(element.Id);
                            if (btNumber.HasValue)
                            {
                                btElements.Add((element, btNumber.Value));
                            }
                        }
                    });
            }

            // Sort by BT number
            var sortedElements = btElements.OrderBy(x => x.BtNumber).ToList();

            // Load BT list body template
            string btListBodyTemplate = System.IO.File.ReadAllText("template-bt-list.html", Encoding.UTF8);

            // Generate list items HTML
            var listItemsHtml = new StringBuilder();

            foreach (var (element, btNumber) in sortedElements)
            {
                // Get the element ID for linking to treeview
                string elementId = element.AdditionalData.ContainsKey("Id") ? element.AdditionalData["Id"] : string.Empty;
                
                listItemsHtml.Append($"            <li data-element-id=\"{elementId}\" onclick=\"window.location.href='index.html#{elementId}';\">");
                listItemsHtml.Append("<div>");
                listItemsHtml.Append($"<span class=\"bt-number\">{element.Id}</span>");
                listItemsHtml.Append($"<span class=\"element-name\">{_EscapeHtml(element.Name)}</span>");

                if (!string.IsNullOrWhiteSpace(element.BusinessTerm))
                {
                    listItemsHtml.Append($"<span class=\"business-term\">({_EscapeHtml(element.BusinessTerm)})</span>");
                }

                listItemsHtml.Append("</div>");

                // Add description on new line if available
                if (!string.IsNullOrWhiteSpace(element.Description))
                {
                    listItemsHtml.Append($"<div class=\"description\">{_EscapeHtml(element.Description)}</div>");
                }

                listItemsHtml.AppendLine("</li>");
            }

            // Build BT list body content
            string btListBody = btListBodyTemplate
                .Replace("{{ELEMENT_COUNT}}", $"Total elements: {sortedElements.Count}")
                .Replace("{{BT_LIST_ITEMS}}", listItemsHtml.ToString());

            // Build final BT list HTML
            string btListHtml = baseTemplate
                .Replace("{{TITLE}}", "BT Elements List")
                .Replace("{{BODY_CONTENT}}", btListBody);

            return btListHtml;
        } // !CreateBTList()


        /// <summary>
        /// Extracts the numeric part of a BT identifier (e.g., "BT-1" -> 1, "BT-123" -> 123).
        /// </summary>
        /// <param name="btId">The BT identifier string.</param>
        /// <returns>The numeric BT number, or null if not found.</returns>
        private static int? _ExtractBTNumber(string btId)
        {
            if (string.IsNullOrWhiteSpace(btId))
            {
                return null;
            }

            // Match patterns like "BT-1", "BT-123", "bt-1", etc.
            var match = Regex.Match(btId, @"^BT-?(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
            {
                return number;
            }

            return null;
        } // !_ExtractBTNumber()


        /// <summary>
        /// Escapes HTML special characters to prevent XSS and display issues.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>HTML-escaped text.</returns>
        private static string _EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        } // !_EscapeHtml()
    }
}
