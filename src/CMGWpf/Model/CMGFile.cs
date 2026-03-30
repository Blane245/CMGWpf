using CMGWpf.Services;
using CMGWpf.Utilities;
using System.Xml;

namespace CMGWpf.Model
{
    public class CMGFile
    {
        public string Comment { get; set; } = "";
        public TimeLine TimeLine { get; set; } = new(0, 0);
        public List<Track> Tracks { get; set; } = [];

        public CMGFile Clone()
        {
            CMGFile n = new()
            {
                Comment = Comment,
                TimeLine = TimeLine.Clone(),
                Tracks = []
            };
            foreach (Track t in this.Tracks)
            {
                n.Tracks.Add(t.Clone());
            }
            return n;
        }

        public void Reset()
        {
            Comment = "";
            Tracks = [];
            TimeLine = new(0, 0);
        }
        public bool Equals(CMGFile? value)
        {
            if (value == null)
                return false;
            if (value == this) return true;
            if (value.TimeLine.Equals(TimeLine)) return true;
            if (Tracks.Count != value.Tracks.Count) return false;
            for (int i = 0; i < Tracks.Count; i++)
            {
                if (!Tracks[i].Equals(value.Tracks[i])) return false;
            }
            return (Comment == value.Comment);
        }

        public void AppendXML(XmlDocument doc, XmlElement elem)
        {
            // Note: the timeLine and fileContents tags are at the same level,
            // despite the internal structure of the CMGFile class
            XmlElement fileContentsElem = doc.CreateElement("fileContents");
            elem.AppendChild(fileContentsElem);
            TimeLine.AppendXml(doc, elem);
            fileContentsElem.SetAttribute("comment", Comment);
            XmlElement tracksElem = doc.CreateElement("tracks");
            fileContentsElem.AppendChild(tracksElem);
            foreach (Track track in Tracks)
            {
                track.AppendXml(doc, tracksElem);
            }
        }

        public string LoadXML(XmlElement doc, string fileName)
        {
            // load the timeline tag, which is optional, if it is not present, create a new timeline with default values
            XmlElement? timeLineElem = doc.GetElementsByTagName("timeLine").Cast<XmlElement?>().FirstOrDefault();
            TimeLine = new(SizeService.Instance.DisplayWidth.Value, SizeService.Instance.TimeLineHeight.Value);
            if (timeLineElem != null) TimeLine.LoadXml(timeLineElem);

            // load the filecontents tag
            XmlElement? fileElem = doc.GetElementsByTagName("fileContents").Cast<XmlElement?>().FirstOrDefault();
            if (fileElem == null) 
                return $"The file {fileName} is missing from the fileContents tag.";
            Comment = XMLFunctions.GetAttributeString(fileElem, "comment", "");

            XmlElement? TracksElem = fileElem.GetElementsByTagName("tracks").Cast<XmlElement?>().FirstOrDefault();
            if (TracksElem != null)
            {
                foreach (XmlElement trackElem in TracksElem)
                {
                    if (trackElem.Name == "track")
                    {
                        Track t = new(0);
                        t.LoadXML(trackElem);
                        Tracks.Add(t);
                    } // skip any non-track elements in the tracks tag
                }
            }
            return "";

        }
    }
}
