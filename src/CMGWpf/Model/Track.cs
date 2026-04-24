using CMGWpf.Layout;
using CMGWpf.Model.Generators;
using CMGWpf.Utilities;
using System.Xml;

namespace CMGWpf.Model
{
    public class Track(int uid)
    {
        public string Name { get; set; } = "T" + uid;
        public bool Mute { get; set; } = false;
        public bool Solo { get; set; } = false;
        public int Volume { get; set; } = 0; //dB
        public List<Generator> Generators { get; set; } = [];
        public Track Clone()
        {
            Track n = new(0)
            {
                Name = Name,
                Mute = Mute,
                Solo = Solo,
                Volume = Volume
            };
            n.Generators = [.. Generators.Select(g => g.Clone(n))];
            return n;
        }
        public override string ToString() { return Name; }
        public bool Equals(Track? value)
        {
            if (value == null)
                return false;
            if (value == this) return true;
            if (value.Solo == Solo && value.Mute == Mute && value.Volume == Volume && value.Generators.Count == Generators.Count)
            {
                for (int i = 0; i < Generators.Count; i++)
                {
                    if (!Generators[i].Equals(value.Generators[i]))
                        return false;
                }
            }
            else
                return false;
            return Name == value.Name;
        }
        public void AppendXml(XmlDocument doc, XmlElement elem)
        {
            XmlElement trackElem = doc.CreateElement("track");
            elem.AppendChild(trackElem);
            trackElem.SetAttribute("name", Name);
            trackElem.SetAttribute("mute", Mute.ToString());
            trackElem.SetAttribute("solo", Solo.ToString());
            trackElem.SetAttribute("volume", Volume.ToString());
            XmlElement gElem = doc.CreateElement("generators");
            trackElem.AppendChild(gElem);

            foreach (Generator generator in Generators)
            {
                XmlElement generatorElem = doc.CreateElement("generator");
                gElem.AppendChild(generatorElem);
                generator.AppendXML(doc, generatorElem);
            }
        }
        public async Task LoadXML(XmlElement elem)
        {
            Name = XMLFunctions.GetAttributeString(elem, "name", "");
            Mute = XMLFunctions.GetAttributeBool(elem, "mute", false);
            Solo = XMLFunctions.GetAttributeBool(elem, "solo", false);
            Volume = XMLFunctions.GetAttributeInt(elem, "volume", 0);
            Generators = [];
            XmlElement? GeneratorsElem = elem.GetElementsByTagName("generators").Cast<XmlElement?>().FirstOrDefault();
            if (GeneratorsElem != null)
            {
                foreach (XmlElement generatorElem in GeneratorsElem)
                {
                    if (generatorElem.Name == "generator")
                    {
                        string type = XMLFunctions.GetAttributeString(generatorElem, "type", "");
                        switch (type)
                        {
                            case "Algorithmic":
                                {
                                    Algorithmic g = new(0, this);
                                    await g.LoadXML(generatorElem, this).ConfigureAwait(false);
                                    Generators.Add(g);
                                    break;
                                }
                            case "Stochastic":
                                {
                                    Stochastic g = new(0, this);
                                    await g.LoadXML(generatorElem, this).ConfigureAwait(false);
                                    Generators.Add(g);
                                    break;
                                }
                            default: break;
                        }
                    }
                }
            }

        }
    }
}
