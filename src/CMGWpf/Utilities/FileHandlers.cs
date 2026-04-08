using CMGWpf.Model;
using CMGWpf.Services;
using CMGWpf.View;
using System.Xml;

namespace CMGWpf.Utilities
{
    public static class FileHandlers
    {
        public static async Task<(string error, CMGFile file)> Read(string fileName)
        {
            System.Diagnostics.Debug.WriteLine($"FileHandlers.Read: {fileName}");
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
                return ($"Exception ocurred while reading {fileName}: {e.Message}.", new CMGFile());
            }
        }

        public static string Write(CMGFile file, string fileName)
        {
            System.Diagnostics.Debug.WriteLine($"FileHandlers.Write: {fileName}");
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
                return $"Exception ocurred while writing {fileName}: {e.Message}";
            }
        }
    }
}
