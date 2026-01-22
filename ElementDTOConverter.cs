using System;
using System.Collections.Generic;
using System.Text;
using FactoorSharp.FacturXDocumentationParser;

namespace FactoorSharp.FacturXDocumentationRenderer
{
    internal class ElementDTOConverter
    {
        internal static Dictionary<string, ElementDetailDTO> Convert(List<Element> rootElements)
        {
            if (rootElements == null)
            {
                return [];
            }

            var result = new Dictionary<string, ElementDetailDTO>();
            foreach (var rootElement in rootElements)
            {
                rootElement.Traverse(element => element.Children, element =>
                {
                    var childrenList = new List<ChildElementDTO>();
                    foreach (var child in element.Children)
                    {
                        childrenList.Add(new ChildElementDTO
                        {
                            Name = child.Name ?? string.Empty,
                            ElementId = child.AdditionalData.ContainsKey("Id") ? child.AdditionalData["Id"] : string.Empty
                        });
                    }

                    var dto = new ElementDetailDTO
                    {
                        Name = element.Name ?? string.Empty,
                        Description = element.Description ?? string.Empty,
                        BusinessRule = element.BusinessRule ?? string.Empty,
                        BusinessTerm = element.BusinessTerm ?? string.Empty,
                        Cardinality = element.CiiCardinality ?? string.Empty,
                        Id = element.Id,
                        Xpath = element.XPath ?? string.Empty,
                        ProfileSupport = element.ProfileSupport != null ? ("|" + string.Join("|", element.ProfileSupport) + "|").Replace(" ", "") : string.Empty,
                        Children = childrenList
                    };

                    result.Add(element.AdditionalData["Id"], dto);
                });
            }

            return result;
        } // !Convert()
    }
}
