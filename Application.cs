using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FactoorSharp.FacturXDocumentationParser;


namespace FactoorSharp.FacturXDocumentationRenderer
{
    internal class Application
    {
        private readonly JsonSerializerOptions _Options = new ()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        internal async Task RunAsync()
        {
            /*
            string excelPath = @"E:\develop\ZUGFeRD-csharp\documentation\zugferd233de\Dokumentation\3. FACTUR-X 1.07.3 - 2025 05 15 - EN FR VF.xlsx";
            string xsdPath = @"E:\develop\ZUGFeRD-csharp\documentation\zugferd233de\Schema\4. Factur-X_1.07.3_EXTENDED\Factur-X_1.07.3_EXTENDED.xsd";
            */  

            string excelPath = @"E:\develop\ZUGFeRD-csharp\documentation\zugferd240en\Documentation\1_FACTUR-X 1.08 - 2025 12 04 - EN FR - VF.xlsx";
            string xsdPath = @"E:\develop\ZUGFeRD-csharp\documentation\zugferd240en\Schema\4_Factur-X_1.08_EXTENDED\FACTUR-X_EXTENDED.xsd";


            List<Element> rootElements = await Parser.ParseAsync(xsdPath, excelPath);
            int id = 1;
            foreach (var rootElement in rootElements)
            {
                rootElement.Traverse(element => element.Children, element =>
                {
                    if (!String.IsNullOrWhiteSpace(element.Id))
                    {
                        element.AdditionalData.Add("Id", element.Id.ToLower());
                    }
                    else
                    {
                        element.AdditionalData.Add("Id", $"elem-{id}");
                    }
                    id++;
                });
            }

            // render treeview
            string treeviewData = TreeviewCreator.CreateTreeview(rootElements);

            // render element information
            Dictionary<string, ElementDetailDTO> dtoMap = ElementDTOConverter.Convert(rootElements);            
            var json = JsonSerializer.Serialize(
                    dtoMap,
                    _Options);

            // Load templates
            string baseTemplate = System.IO.File.ReadAllText("template.html", Encoding.UTF8);
            string treeviewBodyTemplate = System.IO.File.ReadAllText("template-treeview-body.html", Encoding.UTF8);

            // Build treeview body content
            string treeviewBody = treeviewBodyTemplate
                .Replace("{{TREEVIEW_DATA}}", treeviewData)
                .Replace("{{ELEMENT_DATA}}", $"const elementData = {json};");

            // Build final treeview HTML
            string treeviewHtml = baseTemplate
                .Replace("{{TITLE}}", "FacturX 1.0.8")
                .Replace("{{BODY_CONTENT}}", treeviewBody);

            System.IO.File.WriteAllText("index.html", treeviewHtml, Encoding.UTF8);

            // render BT list
            string btListHtml = BTListGenerator.CreateBTList(rootElements, baseTemplate);
            System.IO.File.WriteAllText("bt-elements.html", btListHtml, Encoding.UTF8);
        } // !RunAsync()
    }
}
