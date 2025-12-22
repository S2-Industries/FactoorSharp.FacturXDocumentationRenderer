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
            string excelPath = @"E:\develop\ZUGFeRD-csharp\documentation\zugferd233de\Dokumentation\3. FACTUR-X 1.07.3 - 2025 05 15 - EN FR VF.xlsx";
            string xsdPath = @"E:\develop\ZUGFeRD-csharp\documentation\zugferd233de\Schema\4. Factur-X_1.07.3_EXTENDED\Factur-X_1.07.3_EXTENDED.xsd";

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

            // store data
            string htmlData = System.IO.File.ReadAllText("template.html");
            htmlData = htmlData.Replace("<ul class=\"list-unstyled mb-0\" />", treeviewData);
            htmlData = htmlData.Replace("const elementData = {};", $"const elementData = {json};");
            System.IO.File.WriteAllText("output.html", htmlData);
        } // !RunAsync()
    }
}
