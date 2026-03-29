using CMGWpf.Model;
using CMGWpf.Services;
using CMGWpf.View;
using System.Xml;

namespace CMGWpf.Utilities
{
    public static class FileHandlers
    {
        public static string Read(out CMGFile file, string fileName)
        {
            System.Diagnostics.Debug.WriteLine($"FileHandlers.Read: {fileName}");
            try
            {
                XmlDocument xml = new();
                xml.Load(fileName);
                file = new();
                XmlElement? doc = (XmlElement?)xml.DocumentElement;
                if (doc == null || doc.Name != "CMG") return $"{fileName} is missing the CMG tag.";
                string result = file.LoadXML(doc, fileName);
                return result;
            }
            catch (Exception e)
            {
                file = new();
                return $"Exception ocurred while reading {fileName}: {e.Message}.";
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
