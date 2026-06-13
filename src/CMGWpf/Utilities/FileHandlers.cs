using CMGWpf.Model;
using CMGWpf.Services;
using CMGWpf.View;
using System.Xml;

namespace CMGWpf.Utilities;
/// <summary>
/// Provides methods for reading and writing CMG files in XML format. The Read method loads a CMG file and returns any error messages along with the loaded CMGFile object. The Write method saves a CMGFile object to disk and returns any error messages that occur during the process.
/// </summary>
public static class FileHandlers
{
    public static async Task<(string error, CMGFile file)> Read(string fileName)
    {
        DebugLog.Write($"FileHandlers.Read: {fileName}");
        try
        {
            XmlDocument xml = new();
            xml.Load(fileName);
            CMGFile file = new();
            XmlElement? doc = (XmlElement?)xml.DocumentElement;
            if (doc == null || doc.Name != "CMG") return ($"{fileName} is missing the CMG tag.", file);
            string result = await file.LoadXML(doc, fileName).ConfigureAwait(false);
            return (result, file);
        }
        catch (Exception e)
        {
            return ($"Exception occurred while reading {fileName}: {e.Message}.", new CMGFile());
        }
    }

    public static string Write(CMGFile file, string fileName)
    {
        DebugLog.Write($"FileHandlers.Write: {fileName}");
        try
        {
            XmlDocument doc = new();
            XmlElement cmgElem = doc.CreateElement("CMG");
            doc.AppendChild(cmgElem);
            file.AppendXML(doc, cmgElem);
            //timeLine.AppendXml(doc, timeLineElem);
            doc.Save(fileName);
            return string.Empty;
        }
        catch (Exception e)
        {
            return $"Exception occurred while writing {fileName}: {e.Message}";
        }
    }
}
