using System;
using System.Collections.Generic;
using System.Text;
using FactoorSharp.FacturXDocumentationParser;

namespace FactoorSharp.FacturXDocumentationRenderer
{
    public class ElementDetailDTO
    {
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public string Id { get; set; } = String.Empty;
        public string Xpath { get; set; } = String.Empty;
        public List<string> BusinessRules { get; set; } = new List<string>();
        public string BusinessTerm { get; set; } = String.Empty;
        public Cardinality Cardinality { get; internal set; }
        public string ProfileSupport { get; internal set; } = String.Empty;
        public List<ChildElementDTO> Children { get; set; } = new List<ChildElementDTO>();
    }

    /// <summary>
    /// Represents a child element reference for display in the detail view.
    /// </summary>
    public class ChildElementDTO
    {
        public string Name { get; set; } = String.Empty;
        public string ElementId { get; set; } = String.Empty;
    } // !ChildElementDTO
}
