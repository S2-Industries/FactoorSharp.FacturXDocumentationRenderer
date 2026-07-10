using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FactoorSharp.FacturXDocumentationParser.Common;

namespace FactoorSharp.FacturXDocumentationRenderer
{
    /// <summary>
    /// Generates a Markdown document listing every BT/BG element together with a
    /// matrix that shows in which Factur-X profiles the element is available.
    /// The output is meant to be consumed as a Claude Code skill so that BT
    /// elements written in C# code can be validated against the profile in use.
    /// </summary>
    internal static class BTMarkdownGenerator
    {
        /// <summary>
        /// Profiles in ascending order of richness. Each profile is a superset of the previous one.
        /// </summary>
        private static readonly string[] _Profiles =
        {
            "MINIMUM",
            "BASIC WL",
            "BASIC",
            "EN16931",
            "EXTENDED"
        };

        /// <summary>
        /// Creates the Markdown document for the given root elements.
        /// </summary>
        /// <param name="rootElements">The hierarchical list of root elements to parse.</param>
        /// <param name="title">Title (e.g. the Factur-X version) shown in the document header.</param>
        /// <returns>The complete Markdown document.</returns>
        internal static string CreateBTMarkdown(List<Element> rootElements, string title)
        {
            if (rootElements == null)
            {
                throw new ArgumentNullException(nameof(rootElements));
            }

            // Collect every element that carries a BT/BG identifier.
            var btElements = new List<Element>();
            foreach (var rootElement in rootElements)
            {
                rootElement.Traverse(
                    element => element.Children,
                    element =>
                    {
                        if (_IsBusinessTermId(element.Id))
                        {
                            btElements.Add(element);
                        }
                    });
            }

            // Deduplicate by id (the same BT can appear in multiple branches) and sort.
            var sortedElements = btElements
                .GroupBy(e => e.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(e => e, Comparer<Element>.Create(_CompareIds))
                .ToList();

            var sb = new StringBuilder();

            // Header / legend
            sb.AppendLine($"# Factur-X BT/BG elements per profile — {title}");
            sb.AppendLine();
            sb.AppendLine("This file lists every business term (BT) and business group (BG) of the Factur-X / ZUGFeRD model");
            sb.AppendLine("and shows in which profile each element is available. The profiles are cumulative supersets in this order:");
            sb.AppendLine();
            sb.AppendLine("`MINIMUM` ⊂ `BASIC WL` ⊂ `BASIC` ⊂ `EN16931` ⊂ `EXTENDED`");
            sb.AppendLine();
            sb.AppendLine("`✓` means the element is available in that profile; an empty cell means it is **not** allowed there.");
            sb.AppendLine("When writing C# code for a given profile, only use BT/BG elements that have a `✓` in that profile's column.");
            sb.AppendLine();
            sb.AppendLine($"Total elements: {sortedElements.Count}");
            sb.AppendLine();

            // Table header
            sb.Append("| BT/BG | Business Term | ");
            sb.Append(string.Join(" | ", _Profiles));
            sb.AppendLine(" |");

            sb.Append("|---|---|");
            sb.Append(string.Concat(Enumerable.Repeat(":-:|", _Profiles.Length)));
            sb.AppendLine();

            // Rows
            foreach (var element in sortedElements)
            {
                var support = element.ProfileSupport ?? new List<string>();
                var supportSet = new HashSet<string>(
                    support.Select(p => p.Replace(" ", string.Empty)),
                    StringComparer.OrdinalIgnoreCase);

                sb.Append($"| {_EscapeCell(element.Id)} | {_EscapeCell(element.BusinessTerm)} |");
                foreach (var profile in _Profiles)
                {
                    bool isSupported = supportSet.Contains(profile.Replace(" ", string.Empty));
                    sb.Append(isSupported ? " ✓ |" : " |");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        } // !CreateBTMarkdown()


        /// <summary>
        /// Determines whether an id is a BT/BG identifier (covers BT-, BG-, BT-X-, BG-X-).
        /// </summary>
        private static bool _IsBusinessTermId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return Regex.IsMatch(id, @"^(BT|BG)(-X)?-\d", RegexOptions.IgnoreCase);
        } // !_IsBusinessTermId()


        /// <summary>
        /// Sort order: core (BT/BG) before extended (BT-X/BG-X), then by numeric value, then by full id.
        /// </summary>
        private static int _CompareIds(Element a, Element b)
        {
            var (aExtended, aNumber, aSuffix) = _ParseId(a.Id);
            var (bExtended, bNumber, bSuffix) = _ParseId(b.Id);

            int byExtended = aExtended.CompareTo(bExtended);
            if (byExtended != 0)
            {
                return byExtended;
            }

            int byNumber = aNumber.CompareTo(bNumber);
            if (byNumber != 0)
            {
                return byNumber;
            }

            return string.Compare(aSuffix, bSuffix, StringComparison.OrdinalIgnoreCase);
        } // !_CompareIds()


        /// <summary>
        /// Parses an id into (isExtended, mainNumber, remainingSuffix) for sorting.
        /// </summary>
        private static (bool Extended, int Number, string Suffix) _ParseId(string id)
        {
            id ??= string.Empty;
            var match = Regex.Match(id, @"^(BT|BG)(-X)?-(\d+)(.*)$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return (false, int.MaxValue, id);
            }

            bool extended = match.Groups[2].Success;
            int number = int.TryParse(match.Groups[3].Value, out int n) ? n : int.MaxValue;
            return (extended, number, match.Groups[4].Value);
        } // !_ParseId()


        /// <summary>
        /// Escapes characters that would break a Markdown table cell.
        /// </summary>
        private static string _EscapeCell(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("|", "\\|");
        } // !_EscapeCell()
    }
}
