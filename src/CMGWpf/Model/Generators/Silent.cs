using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using System.Xml;

namespace CMGWpf.Model.Generators
{
    public class Silent(int uid, Track parent) : Generator(uid, parent)
    {
        public override Silent Clone(Track parent)
        {
            Silent clone = (Silent)this.MemberwiseClone();
            clone.Parent = parent;
            return clone;
        }
        public override bool Equals(Generator value)
        {
            return (
                Name == value.Name &&
                StartTime == value.StartTime &&
                StopTime == value.StopTime &&
                Mute == value.Mute &&
                Position == value.Position);
        }

        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("type", this.ToString());
            elem.SetAttribute("name", Name);
            elem.SetAttribute("start", StartTime.ToString());
            elem.SetAttribute("stop", StopTime.ToString());
            elem.SetAttribute("mute", Mute.ToString());
            elem.SetAttribute("position", Position.ToString());

        }
        public override void LoadXML(XmlElement elem, Track parent)
        {
            Name = XMLFunctions.GetAttributeString(elem, "name", "");
            Parent = parent;
            StartTime = XMLFunctions.GetAttributeDouble(elem, "start", 0);
            StopTime = XMLFunctions.GetAttributeDouble(elem, "stop", 0);
            Position = XMLFunctions.GetAttributeInt(elem, "position", 0);
            Mute = XMLFunctions.GetAttributeBool(elem, "mute", false);
        }
        public new ObservableCollection<Message> Validate()
        {
            return base.Validate();
        }
        public override CurrentValues GetCurrentValues(double time, int beat)
        {
            return new CurrentValues()
            {
                Note = 0,
                Attack = 0,
                Speed = 0,
                Volume = 0,
                Pan = 0
            };
        }
        public override string ToString() => "Silent";
    }
}
