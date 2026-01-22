using System;
using System.Collections.Generic;
using System.Text;

namespace FactoorSharp.FacturXDocumentationRenderer
{
    public class ElementDetailDTO
    {
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public string Id { get; set; } = String.Empty;
        public string Xpath { get; set; } = String.Empty;
        public string BusinessRule { get; set; } = String.Empty;
        public string BusinessTerm { get; set; } = String.Empty;
        public string Cardinality { get; internal set; } = String.Empty;
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
