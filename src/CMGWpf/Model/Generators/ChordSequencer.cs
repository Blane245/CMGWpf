using CMGWpf.Helpers;
using CMGWpf.Model.Database;
using CMGWpf.SoundFont_2;
using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using System.Xml;

namespace CMGWpf.Model.Generators
{
    public class ChordSequencer(int uid, Track parent) : Generator(uid, parent)
    {
        private string chordSequenceName = "";
        public string ChordSequenceName { get => chordSequenceName; 
            set
            {
                chordSequenceName = value;
                ChordSequence = ChordSequenceHelpers.Get(value).Result;
                if (ChordSequence == null) chordSequenceName = "";
            } }
        public ChordSequence? ChordSequence { get; set; } = null;
        public SoundFont? SoundFont { get; set; } = null;
        public string SoundFontFileName { get; set; } = ";";
        private string presetName = "";
        public string PresetName { get { return presetName; } set { if (value != "") { presetName = value; } } }
        public Preset? Preset { get; set; } = null;
        public string Key { get; set; } = "C";
        public int Octave { get; set; } = 4;
        public int Repeat { get; set; } = 0; // 0=repeat until generator stop time, > 0, repeat based on count
        public string NoiseSeed { get; set; } = "";
        public FastRandom Random { get; set; } = MathUtilities.StartFastRandom(null);
        public double NoiseFrequency { get; set; } = 0;
        public double NoiseAmplitude { get; set; } = 0;
        public bool NoiseEnabled { get; set; } = false;
        public bool AttackEnabled { get; set; } = true;
        public double ReverbDelay { get; set; } = 0;
        public int ReverbDecay { get; set; } = 1;
        public Tremolo Tremolo { get; set; } = new Tremolo();
        public Tremolo Vibrato { get; set; } = new Tremolo();
        public Algorithm SpeedAlgorithm { get; set; } = new Constant(60);
        public Algorithm VolumeAlgorithm { get; set; } = new Constant(0);
        public Algorithm PanAlgorithm { get; set; } = new Constant(0);
        public override bool Equals(Generator value)
        {
            if (value is ChordSequencer g)
            {
                if (base.Equals(value) &&
                    ChordSequence == g.ChordSequence &&
                    SoundFontFileName == g.SoundFontFileName &&
                    PresetName == g.PresetName &&
                    Repeat == g.Repeat &&
                    NoiseSeed == g.NoiseSeed &&
                    NoiseFrequency == g.NoiseFrequency &&
                    NoiseAmplitude == g.NoiseAmplitude &&
                    SpeedAlgorithm.Equals(g.SpeedAlgorithm) &&
                    VolumeAlgorithm.Equals(g.VolumeAlgorithm) &&
                    PanAlgorithm.Equals(g.PanAlgorithm) &&
                    ReverbDecay.Equals(g.ReverbDecay) &&
                    ReverbDelay.Equals(g.ReverbDelay) &&
                    Tremolo.Equals(g.Tremolo) &&
                    Vibrato.Equals(g.Vibrato) &&
                    Key.Equals(g.Key) &&
                    Octave.Equals(g.Octave)
                    )
                    return true;
                else
                    return false;
            }
            else return false;
        }
        public override ChordSequencer Clone(Track parent)
        {
            ChordSequencer n = (ChordSequencer)MemberwiseClone();
            n.Parent = parent;
            n.ChordSequence = ChordSequence?.Clone();
            n.SpeedAlgorithm = SpeedAlgorithm.Clone();
            n.VolumeAlgorithm = VolumeAlgorithm.Clone();
            n.PanAlgorithm = PanAlgorithm.Clone();
            n.Tremolo = Tremolo.Clone();
            n.Vibrato = Vibrato.Clone();
            n.Random = MathUtilities.StartFastRandom(NoiseSeed);
            return n;
        }
        public void InitialSequence()
        {
            Random = MathUtilities.StartFastRandom(NoiseSeed);
            // Initialize all of the attribute algorithms
            SpeedAlgorithm.Initialize();
            VolumeAlgorithm.Initialize();
            PanAlgorithm.Initialize();
        }
        public override double GetEndTime()
        {
            return StopTime;
        }

        // ChordSequencer uses a different method to get current values than other generators, so this method is not implemented.
        public override CurrentValues GetCurrentValues(double time, double beats)
        {
            throw new NotImplementedException();
        }

        // this routine is called for each chord in the sequence. If it gets to the end of the sequence, the sequence is restarted depending on the repeat count
        public ChordSequencerValues GetChordSequencerValues(double time)
        {

            double speed = SpeedAlgorithm.GetCurrentValue(time, 0);
            double volume = VolumeAlgorithm.GetCurrentValue(time, 0);
            double pan = PanAlgorithm.GetCurrentValue(time, 0);
            return new ChordSequencerValues { Speed = speed, Volume = volume, Pan = pan };
        }
        private struct AlgorithmDesignator(string name, Algorithm type)
        {
            public string Name = name;
            public Algorithm Type = type;
        }
        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("type", this.ToString());
            elem.SetAttribute("name", Name);
            elem.SetAttribute("startTime", StartTime.ToString());
            elem.SetAttribute("stopTime", StopTime.ToString());
            elem.SetAttribute("mute", Mute.ToString());
            elem.SetAttribute("position", Position.ToString());
            string[] nameParts = SoundFontFileName.Split(['\\', '/']);
            if (nameParts.Length > 0)
                elem.SetAttribute("soundFontFile", nameParts[^1]);
            else
                elem.SetAttribute("soundFontFile", SoundFontFileName);
            elem.SetAttribute("presetName", PresetName);
            elem.SetAttribute("attackEnabled", AttackEnabled.ToString());
            elem.SetAttribute("key", Key);
            elem.SetAttribute("octave", Octave.ToString());
            elem.SetAttribute("repeat", Repeat.ToString());
            elem.SetAttribute("noiseSeed", NoiseSeed);
            elem.SetAttribute("noiseFrequency", NoiseFrequency.ToString());
            elem.SetAttribute("noiseAmplitude", NoiseAmplitude.ToString());
            elem.SetAttribute("chordSequenceName", ChordSequence?.Name ?? "");
            AlgorithmDesignator[] designators = [
                new AlgorithmDesignator("speedP", SpeedAlgorithm),
                new AlgorithmDesignator("volumeP", VolumeAlgorithm),
                new AlgorithmDesignator("panP", PanAlgorithm),
                ];
            foreach (AlgorithmDesignator d in designators)
            {
                Algorithm a = d.Type;
                XmlElement aElem = doc.CreateElement(d.Name);
                aElem.SetAttribute("algorithmType", a.ToString());
                a.AppendXML(doc, aElem);
                elem.AppendChild(aElem);
            }

            XmlElement tremeloAlgorithmElem = doc.CreateElement("tremolo");
            XmlElement vibratoAlgorithmElem = doc.CreateElement("vibrato");
            Tremolo.AppendXML(doc, tremeloAlgorithmElem);
            Vibrato.AppendXML(doc, vibratoAlgorithmElem);
            elem.AppendChild(tremeloAlgorithmElem);
            elem.AppendChild(vibratoAlgorithmElem);
            elem.SetAttribute("reverbDelay", ReverbDelay.ToString());
            elem.SetAttribute("reverbDecay", ReverbDecay.ToString());

        }
        public override async Task LoadXML(XmlElement elem, Track parent)
        {
            Name = XMLFunctions.GetAttributeString(elem, "name", "");
            Parent = parent;
            StartTime = XMLFunctions.GetAttributeDouble(elem, "startTime", 0);
            StopTime = XMLFunctions.GetAttributeDouble(elem, "stopTime", 0);
            Position = XMLFunctions.GetAttributeInt(elem, "position", 0);
            Mute = XMLFunctions.GetAttributeBool(elem, "mute", false);
            SoundFontFileName = XMLFunctions.GetAttributeString(elem, "soundFontFile", "");
            PresetName = XMLFunctions.GetAttributeString(elem, "presetName", "");
            Key = XMLFunctions.GetAttributeString(elem, "key", "C");
            Octave = XMLFunctions.GetAttributeInt(elem, "octave", 4);
            Repeat = XMLFunctions.GetAttributeInt(elem, "repeat", 0);
            ChordSequenceName = XMLFunctions.GetAttributeString(elem, "chordSequenceName", "");
            NoiseSeed = XMLFunctions.GetAttributeString(elem, "noiseSeed", "");
            NoiseFrequency = XMLFunctions.GetAttributeDouble(elem, "noiseFrequency", 0);
            NoiseAmplitude = XMLFunctions.GetAttributeDouble(elem, "noiseAmplitude", 0);
            AttackEnabled = XMLFunctions.GetAttributeBool(elem, "attackEnabled", true);
            ReverbDecay = XMLFunctions.GetAttributeInt(elem, "reverbDecay", 1);
            ReverbDelay = XMLFunctions.GetAttributeDouble(elem, "reverbDelay", 0);
            XmlElement? tremoloElem = elem.GetElementsByTagName("tremolo").Cast<XmlElement?>().FirstOrDefault();
            if (tremoloElem == null) Tremolo = new Tremolo();
            else Tremolo.LoadXML(tremoloElem);
            XmlElement? vibratoElem = elem.GetElementsByTagName("vibrato").Cast<XmlElement?>().FirstOrDefault();
            if (vibratoElem == null) Vibrato = new Tremolo();
            else Vibrato.LoadXML(vibratoElem);
            XmlElement? tremoloAlgorithmElem = elem.GetElementsByTagName("tremolo").Cast<XmlElement?>().FirstOrDefault();
            if (tremoloAlgorithmElem != null)
            {
                Tremolo.LoadXML(tremoloAlgorithmElem);
            }
            else Tremolo = new Tremolo();

            XmlElement? vibratoAlgorithmElem = elem.GetElementsByTagName("vibrato").Cast<XmlElement?>().FirstOrDefault();
            if (vibratoAlgorithmElem != null)
            {
                Vibrato.LoadXML(vibratoAlgorithmElem);
            }
            else Vibrato = new Tremolo();

            AlgorithmDesignator[] designators = [
                new AlgorithmDesignator("speedP", SpeedAlgorithm),
                new AlgorithmDesignator("volumeP", VolumeAlgorithm),
                new AlgorithmDesignator("panP", PanAlgorithm)
    ];
            foreach (AlgorithmDesignator d in designators)
            {
                XmlElement? aElem = elem.GetElementsByTagName(d.Name).Cast<XmlElement?>().FirstOrDefault();
                if (aElem != null)
                {
                    string typeString = XMLFunctions.GetAttributeString(aElem, "algorithmType", "Constant");
                    Algorithm a = d.Type;
                    try
                    {
                        ALGORITHMTYPE type = Enum.Parse<ALGORITHMTYPE>(typeString);
                        a = type switch
                        {
                            ALGORITHMTYPE.Constant => new Constant(),
                            ALGORITHMTYPE.Markovian => new Markovian(),
                            ALGORITHMTYPE.Wiener => new Wiener(),
                            ALGORITHMTYPE.Oscillator => new Oscillator(),
                            ALGORITHMTYPE.Sequencer => new Sequencer(),
                            ALGORITHMTYPE.Poisson => new Poisson(),
                            ALGORITHMTYPE.Autoregressive => new Autoregressive(),
                            _ => new Constant(),
                        };
                        a.LoadXML(aElem);

                        // If it's a Sequencer, initialize it asynchronously to load items from database
                        if (a is Sequencer sequencer)
                        {
                            await sequencer.InitializeAsync().ConfigureAwait(false);
                        }

                        // Assign the loaded algorithm back to the appropriate property
                        switch (d.Name)
                        {
                            case "speedP":
                                SpeedAlgorithm = a;
                                break;
                            case "volumeP":
                                VolumeAlgorithm = a;
                                break;
                            case "panP":
                                PanAlgorithm = a;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        a = new Constant();
                    }
                }


                SoundFont = SoundFontUtilities.GetSoundFont(SoundFontFileName);
                if (PresetName != "" && SoundFont != null)
                {
                    Preset = SoundFont.Presets.FirstOrDefault(p => SoundFontUtilities.BankPresetToName(p) == PresetName);
                }
                ChordSequence = ChordSequenceHelpers.Get(ChordSequenceName).Result;
            }
        }
        public override ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = base.Validate();
            if (SoundFontFileName == null || SoundFontFileName.Length == 0) errors.Add(new Message() { Text = "SoundFont file must not be empty.", Error = true });
            if (PresetName == null || PresetName.Length == 0) errors.Add(new Message() { Text = "Preset name must not be empty.", Error = true });
            if (NoiseFrequency < 0) errors.Add(new Message() { Text = "Noise frequency must be greater than or equal to 0.", Error = true });
            if (NoiseAmplitude < 0) errors.Add(new Message() { Text = "Noise amplitude must be greater than or equal to 0.", Error = true });
            if (ChordSequenceName == "") errors.Add(new Message() { Text = "Chord Sequence must be given", Error = true });
            if (Key == null || Key.Length == 0) errors.Add(new Message() { Text = "Key must not be empty.", Error = true });
            if (Octave < -1 || Octave > 9) errors.Add(new Message() { Text = "Octave must be in the interval [-1, 9].", Error = true });
            // validate the parameters of the algorithms and add any errors to the errors list
            ObservableCollection<Message> speedE = SpeedAlgorithm.Validate();
            ObservableCollection<Message> volumeE = VolumeAlgorithm.Validate();
            ObservableCollection<Message> panE = PanAlgorithm.Validate();
            foreach (Message error in speedE) errors.Add(error);
            foreach (Message error in volumeE) errors.Add(error);
            foreach (Message error in panE) errors.Add(error);
            return errors;
        }
        public override string ToString() => "ChordSequencer";
    }

}
