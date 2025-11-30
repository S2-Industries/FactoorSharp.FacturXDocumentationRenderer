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
        internal async Task RunAsync(string[] args)
        {
            string excelPath = @"E:\develop\ZUGFeRD-csharp-private\documentation\zugferd233de\Dokumentation\3. FACTUR-X 1.07.3 - 2025 05 15 - EN FR VF.xlsx";
            string xsdPath = @"E:\develop\ZUGFeRD-csharp-private\documentation\zugferd233de\Schema\4. Factur-X_1.07.3_EXTENDED\Factur-X_1.07.3_EXTENDED.xsd";

            List<Element> rootElements = await Parser.ParseAsync(xsdPath, excelPath);
            int id = 1;
            foreach (var rootElement in rootElements)
            {
                rootElement.Traverse(element => element.Children, element =>
                {
                    element.AdditionalData.Add("Id", $"elem-{id}")  ;
                    id++;
                });
            }

            // render treeview
            string treeviewData = TreeviewCreator.CreateTreeview(rootElements);


            // render element information
            Dictionary<string, ElementDetailDTO> dtoMap = ElementDTOConverter.Convert(rootElements);
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(
                    dtoMap,
                    options);

            // store data
            string htmlData = System.IO.File.ReadAllText("template.html");
            htmlData = htmlData.Replace("<ul class=\"list-unstyled mb-0\" />", treeviewData);
            htmlData = htmlData.Replace("const elementData = {};", $"const elementData = {json};");
            System.IO.File.WriteAllText("output.html", htmlData);
        } // !RunAsync()
    }
}
