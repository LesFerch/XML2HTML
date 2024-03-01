using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Convert XML files with XSL stylesheet to HTML");
            Console.WriteLine("Usage: XML2HTML <file1.xml> <file2.xml> ...");
            return;
        }

        foreach (var xmlFile in args)
        {
            try
            {
                string xslFileName = GetXslFileNameFromXml(xmlFile);
                if (xslFileName == null)
                {
                    Console.WriteLine($"XSL filename not found in {xmlFile}");
                    continue;
                }

                string xmlContents = File.ReadAllText(xmlFile);
                string xslContents = File.ReadAllText(xslFileName);

                string outputFileName = Path.GetFileNameWithoutExtension(xmlFile) + ".html";
                TransformXml(xmlContents, xslContents, outputFileName);

                Console.WriteLine($"Transformed [{xmlFile}] with [{xslFileName}] to make [{outputFileName}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {xmlFile}: {ex.Message}");
            }
        }
    }

    static string GetXslFileNameFromXml(string xmlFile)
    {
        string[] lines = File.ReadAllLines(xmlFile);
        foreach (string line in lines)
        {
            if (line.Contains("<?xml-stylesheet"))
            {
                int startIndex = line.IndexOf("href=\"") + "href=\"".Length;
                int endIndex = line.IndexOf("\"", startIndex);
                if (startIndex != -1 && endIndex != -1)
                {
                    return line.Substring(startIndex, endIndex - startIndex);
                }
            }
        }
        return null;
    }

    static void TransformXml(string xmlContent, string xslContent, string outputFileName)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlContent);

        var xsltDocument = new XslCompiledTransform();
        xsltDocument.Load(new XmlTextReader(new StringReader(xslContent)));

        using (var memoryStream = new MemoryStream())
        {
            using (var xmlTextWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = new UTF8Encoding(false), Indent = true }))
            {
                xsltDocument.Transform(xmlDocument, null, xmlTextWriter);
            }

            string htmlContent = Encoding.UTF8.GetString(memoryStream.ToArray());

            htmlContent = InsertContentTypeMetaTag(htmlContent);

            File.WriteAllText(outputFileName, htmlContent);
        }
    }
    static string InsertContentTypeMetaTag(string htmlContent)
    {
        int headIndex = htmlContent.IndexOf("<head>");
        if (headIndex != -1)
        {
            string metaTag = "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">";
            return htmlContent.Insert(headIndex + "<head>".Length, metaTag);
        }
        return htmlContent;
    }
}
