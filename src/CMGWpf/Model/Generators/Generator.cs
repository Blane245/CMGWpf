using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Xml;

namespace CMGWpf.Model.Generators
{
    public enum GENERATORTYPE
    {
        NoGenertor,
        Algorithmic,
        Stochastic,
    }
    public class CurrentValues
    {
        public bool Beat { get; set; } = false;
        public double Note { get; set; } = 60;
        public double Attack { get; set; } = 63;
        public double Speed { get; set; } = 60;
        public double Duration { get; set; } = 60;
        public double Volume { get; set; } = 0;
        public double Pan { get; set; } = 0;
    }

    /// <summary>
    /// The parent class for all generators. A generator is an object that generates note, speed, volume, and pan values over time. Each generator is associated with a track and has a start time and stop time and a unique name. The generator's values are only active between its start time and stop time. Each generator has a method for getting the current values at a given time, which will be used by the track to determine what values to use for the track at that time. Each generator also has methods for cloning itself, appending itself to an XML document, loading itself from an XML document, and validating itself. The NoGenerator class is a special type of generator that does not generate any values and is not used. It is here only to establish a framework for create generators.
    /// </summary>
    public abstract class Generator(int uid, Track parent)
    {
        public string Name { get; set; } = "G" + uid;
        public Track Parent { get; set; } = parent;
        public double StartTime { get; set; }
        public double StopTime { get; set; }
        public bool Mute { get; set; } = false;
        public int Position { get; set; } = 0;

        public static class GeneratorFactory
        {
            public static Generator Create(GENERATORTYPE type, Track parent)
            {
                int uid = Uid.Get("generator", FileViewModel.Instance.File.Tracks);
                return type switch
                {
                    GENERATORTYPE.Algorithmic => new Algorithmic(uid, parent),
                    GENERATORTYPE.Stochastic => new Stochastic(uid, parent),
                    _ => throw new ArgumentException("Unknown generator type", nameof(type)),
                };
            }
        }
        public abstract Generator Clone(Track parent);
        public virtual bool Equals(Generator value) => base.Equals(value);
        public abstract double GetEndTime();
        public abstract CurrentValues GetCurrentValues(double time, double beats);
        public abstract void AppendXML(XmlDocument doc, XmlElement elem);
        public abstract Task LoadXML(XmlElement generatorElem, Track parent);
        public virtual ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = [];
            if (string.IsNullOrEmpty(Name)) errors.Add(new Message() { Text = "Name cannot be empty.", Error = true });
            if (StopTime <= StartTime) errors.Add(new Message() { Text = "Stop time must be greater than start time.", Error = true });
            return errors;
        }
        public override string ToString() => "None";
    }
}
