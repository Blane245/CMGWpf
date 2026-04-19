using CMGWpf.SoundFont_2;
using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using System.Xml;

namespace CMGWpf.Model.Generators
{
    public class Algorithmic(int uid, Track parent) : Generator(uid, parent)
    {
        public SoundFont? SoundFont { get; set; } = null;
        public string SoundFontFileName { get; set; } = ";";
        private string presetName = "";
        public string PresetName { get { return presetName; } set { if (value != "") { presetName = value; } } }
        public Preset? Preset { get; set; } = null;
        public bool Microtones { get; set; } = true;
        public bool IsLooping { get; set; } = true;
        public int MeasureLength { get; set; } = 4;
        public int BeatCount { get; set; } = 4;
        public int OffsetSequence { get; set; } = 0;
        public int NoteCount { get; set; } = 12;
        public int NoteShift { get; set; } = 0;
        public int OffsetNotes { get; set; }
        private int[] activeNotes = EuclideanRhythm.Get(12, 12, 0);
        public string NoiseSeed { get; set; } = "";
        public FastRandom Random { get; set; } = MathUtilities.StartFastRandom(null);
        public double NoiseFrequency { get; set; } = 0;
        public double NoiseAmplitude { get; set; } = 0;
        public bool NoiseEnabled { get; set; } = false;
        public bool AttackEnabled { get; set; } = true;
        public bool MicrotonesEnabled { get; set; } = true;
        public double ReverbDelay { get; set; } = 0;
        public int ReverbDecay { get; set; } = 1;
        public Tremolo Tremolo { get; set; } = new Tremolo();
        public Tremolo Vibrato { get; set; } = new Tremolo();
        public Algorithm NoteAlgorithm { get; set; } = new Constant(60);
        public Algorithm AttackAlgorithm { get; set; } = new Constant(63);
        public Algorithm SpeedAlgorithm { get; set; } = new Constant(60);
        public Algorithm DurationAlgorithm { get; set; } = new Constant(100);
        public Algorithm VolumeAlgorithm { get; set; } = new Constant(0);
        public Algorithm PanAlgorithm { get; set; } = new Constant(0);

        public override bool Equals(Generator value)
        {
            if (value is Algorithmic g)
            {
                if (base.Equals(value) &&
                    SoundFontFileName == g.SoundFontFileName &&
                    PresetName == g.PresetName &&
                    Microtones == g.Microtones &&
                    IsLooping == g.IsLooping &&
                    MeasureLength == g.MeasureLength &&
                    BeatCount == g.BeatCount &&
                    OffsetSequence == g.OffsetSequence &&
                    NoteCount == g.NoteCount &&
                    OffsetNotes == g.OffsetNotes &&
                    NoiseSeed == g.NoiseSeed &&
                    NoiseFrequency == g.NoiseFrequency &&
                    NoiseAmplitude == g.NoiseAmplitude &&
                    NoteAlgorithm.Equals(g.NoteAlgorithm) &&
                    AttackAlgorithm.Equals(g.AttackAlgorithm) &&
                    DurationAlgorithm.Equals(g.DurationAlgorithm) &&
                    SpeedAlgorithm.Equals(g.SpeedAlgorithm) &&
                    VolumeAlgorithm.Equals(g.VolumeAlgorithm) &&
                    PanAlgorithm.Equals(g.PanAlgorithm) &&
                    Tremolo.Equals(g.Tremolo) &&
                    Vibrato.Equals(g.Vibrato)
                    )
                    return true;
                else
                    return false;
            }
            else return false;
        }
        public override Algorithmic Clone(Track parent)
        {
            Algorithmic n = (Algorithmic)MemberwiseClone();
            n.Parent = parent;  // FIX: Set the correct parent track
            n.NoteAlgorithm = NoteAlgorithm.Clone();
            n.AttackAlgorithm = AttackAlgorithm.Clone();
            n.DurationAlgorithm = DurationAlgorithm.Clone();
            n.SpeedAlgorithm = SpeedAlgorithm.Clone();
            n.VolumeAlgorithm = VolumeAlgorithm.Clone();
            n.PanAlgorithm = PanAlgorithm.Clone();
            n.Tremolo = Tremolo.Clone();  // Clone Tremolo
            n.Vibrato = Vibrato.Clone();  // Clone Vibrato
            n.Random = MathUtilities.StartFastRandom(NoiseSeed);  // Create new FastRandom instance
            n.activeNotes = (int[])activeNotes.Clone();  // Clone array
            n.beatSequence = (int[])beatSequence.Clone();  // Clone array
            return n;
        }

        private int[] beatSequence = [];
        private int currentRhythmEntry = 0;
        public void InitialSequence()
        {
            beatSequence = EuclideanRhythm.Get(BeatCount, MeasureLength, OffsetSequence);
            currentRhythmEntry = 0;
        }

        private double GetSelectedNote(double note)
        {
            //int noteIndex = (int)((note + OffsetNotes) % NoteCount);
            //return activeNotes[noteIndex] + 12 * Math.Floor((note + OffsetNotes) / NoteCount);
            // get the pitch integer and fraction parts
            double pitch = this.Microtones ? note : Math.Round(note);
            double midiFraction = note - pitch;

            // get the octave and offset values
            double midiOffset = pitch % 12;
            int normalizedMidiOffset = (int)(midiOffset + 12) % 12;
            double octave = Math.Floor(pitch / 12);

            // if the note is on, return the original note
            if (activeNotes[normalizedMidiOffset] == 1) return note;

            // find the two selected notes surrounding this nonselected note
            // this assumes that the first note in the sequence is selected
            int first = normalizedMidiOffset;
            int last = normalizedMidiOffset;
            while (first >= 0 && activeNotes[first] == 0) first--;
            while (last < 12 && activeNotes[last] == 0) last++;
            int firstOffset = normalizedMidiOffset - first;
            int lastOffset = last - normalizedMidiOffset;

            // set the pitch to the closest active note, favoring the lower one
            if (firstOffset <= lastOffset) pitch = octave * 12 + first;
            else pitch = octave * 12 + last;

            // return with the fractional note applied
            return pitch + midiFraction;
        }
        public override double GetEndTime()
        {
            return StopTime;
        }
        public override CurrentValues GetCurrentValues(double time, double beats)
        {
            int entry = currentRhythmEntry;
            currentRhythmEntry = (currentRhythmEntry + 1) % MeasureLength;
            bool beat = beatSequence[entry] != 0;
            double note = NoteAlgorithm.GetCurrentValue(time, beats);
            note = Math.Clamp(note, 0, 127);
            note = GetSelectedNote(note);
            if (!Microtones) note = Math.Round(note);
            double attack = AttackAlgorithm.GetCurrentValue(time, beats);
            attack = Math.Clamp(attack, 0, 127);
            double speed = SpeedAlgorithm.GetCurrentValue(time, beats);
            speed = Math.Clamp(speed, 0.001, 10_000);
            double duration = DurationAlgorithm.GetCurrentValue(time, beats);
            duration = Math.Clamp(duration, 0.01, 100);
            double volume = VolumeAlgorithm.GetCurrentValue(time, beats);
            volume = Math.Clamp(volume, -10, 10);
            double pan = PanAlgorithm.GetCurrentValue(time, beats);
            pan = Math.Clamp(pan, -1, 1);
            return new CurrentValues()
            {
                Beat = beat,
                Note = note,
                Attack = attack,
                Speed = speed,
                Duration = duration,
                Volume = volume,
                Pan = pan
            };


        }
        public override void AppendXML(XmlDocument doc, XmlElement generatorElem)
        {
            generatorElem.SetAttribute("type", this.ToString());
            generatorElem.SetAttribute("name", Name);
            generatorElem.SetAttribute("startTime", StartTime.ToString());
            generatorElem.SetAttribute("stopTime", StopTime.ToString());
            generatorElem.SetAttribute("mute", Mute.ToString());
            generatorElem.SetAttribute("position", Position.ToString());
            // strip the path from the soundfont file name
            string[] nameParts = SoundFontFileName.Split(['\\', '/']);
            if (nameParts.Length > 0)
                generatorElem.SetAttribute("soundFontFile", nameParts[^1]);
            else
                generatorElem.SetAttribute("soundFontFile", SoundFontFileName);
            generatorElem.SetAttribute("presetName", PresetName);
            generatorElem.SetAttribute("microtones", Microtones ? "true" : "false");
            generatorElem.SetAttribute("isLooping", IsLooping ? "true" : "false");
            generatorElem.SetAttribute("measureLength", MeasureLength.ToString());
            generatorElem.SetAttribute("beatCount", BeatCount.ToString());
            generatorElem.SetAttribute("offsetSequence", OffsetSequence.ToString());
            generatorElem.SetAttribute("noteCount", NoteCount.ToString());
            generatorElem.SetAttribute("offsetNotes", OffsetNotes.ToString());
            generatorElem.SetAttribute("noiseSeed", NoiseSeed);
            generatorElem.SetAttribute("noiseFrequency", NoiseFrequency.ToString());
            generatorElem.SetAttribute("noiseAmplitude", NoiseAmplitude.ToString());
            generatorElem.SetAttribute("attackEnabled", AttackEnabled.ToString());
            AlgorithmDesignator[] designators = [
                new AlgorithmDesignator("noteP", NoteAlgorithm),
                new AlgorithmDesignator("attackP", AttackAlgorithm),
                new AlgorithmDesignator("speedP", SpeedAlgorithm),
                new AlgorithmDesignator("durationP", DurationAlgorithm),
                new AlgorithmDesignator("volumeP", VolumeAlgorithm),
                new AlgorithmDesignator("panP", PanAlgorithm),
                ];
            foreach (AlgorithmDesignator d in designators)
            {
                Algorithm a = d.Type;
                XmlElement elem = doc.CreateElement(d.Name);
                elem.SetAttribute("algorithmType", a.ToString());
                a.AppendXML(doc, elem);
                generatorElem.AppendChild(elem);
            }

            XmlElement tremeloAlgorithmElem = doc.CreateElement("tremolo");
            XmlElement vibratoAlgorithmElem = doc.CreateElement("vibrato");
            Tremolo.AppendXML(doc, tremeloAlgorithmElem);
            Vibrato.AppendXML(doc, vibratoAlgorithmElem);
            generatorElem.AppendChild(tremeloAlgorithmElem);
            generatorElem.AppendChild(vibratoAlgorithmElem);
        }
        private struct AlgorithmDesignator(string name, Algorithm type)
        {
            public string Name = name;
            public Algorithm Type = type;
        }
        public override async Task LoadXML(XmlElement elem, Track parent)
        {
            Name = XMLFunctions.GetAttributeString(elem, "name", "");
            Parent = parent;
            double readStopTime = XMLFunctions.GetAttributeDouble(elem, "stopTime", 0);
            StartTime = XMLFunctions.GetAttributeDouble(elem, "startTime", 0);
            StopTime = readStopTime; // override the calculation doen when starttime is read
            Position = XMLFunctions.GetAttributeInt(elem, "position", 0);
            Mute = XMLFunctions.GetAttributeBool(elem, "mute", false);
            SoundFontFileName = XMLFunctions.GetAttributeString(elem, "soundFontFile", "");
            PresetName = XMLFunctions.GetAttributeString(elem, "presetName", "");

            // load the soundfont file and preset. 
            SoundFont = SoundFontUtilities.GetSoundFont(SoundFontFileName);
            if (PresetName != "" && SoundFont != null)
            {
                Preset = SoundFont.Presets.FirstOrDefault(p => SoundFontUtilities.BankPresetToName(p) == PresetName);
            }
            Microtones = XMLFunctions.GetAttributeBool(elem, "microtones", true);
            IsLooping = XMLFunctions.GetAttributeBool(elem, "isLooping", false);
            MeasureLength = XMLFunctions.GetAttributeInt(elem, "measureLength", 4);
            BeatCount = XMLFunctions.GetAttributeInt(elem, "beatCount", 4);
            OffsetSequence = XMLFunctions.GetAttributeInt(elem, "offsetSequence", 0);
            NoteCount = XMLFunctions.GetAttributeInt(elem, "noteCount", 12);
            OffsetNotes = XMLFunctions.GetAttributeInt(elem, "offsetNotes", 0);
            activeNotes = EuclideanRhythm.Get(XMLFunctions.GetAttributeInt(elem, "noteCount", 12), 12, XMLFunctions.GetAttributeInt(elem, "offsetNotes", 0));
            NoiseSeed = XMLFunctions.GetAttributeString(elem, "noiseSeed", "");
            NoiseFrequency = XMLFunctions.GetAttributeDouble(elem, "noiseFrequency", 0);
            NoiseAmplitude = XMLFunctions.GetAttributeDouble(elem, "noiseAmplitude", 0);
            AttackEnabled = XMLFunctions.GetAttributeBool(elem, "attackEnabled", true);
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
                new AlgorithmDesignator("noteP", NoteAlgorithm),
                new AlgorithmDesignator("attackP", AttackAlgorithm),
                new AlgorithmDesignator("speedP", SpeedAlgorithm),
                new AlgorithmDesignator("durationP", DurationAlgorithm),
                new AlgorithmDesignator("volumeP", VolumeAlgorithm),
                new AlgorithmDesignator("panP", PanAlgorithm)
                ];
            foreach (AlgorithmDesignator d in designators)
            {
                XmlElement? aElem = elem.GetElementsByTagName(d.Name).Cast<XmlElement?>().FirstOrDefault();
                if (aElem != null)
                {
                    string typeString = XMLFunctions.GetAttributeString(aElem, "algorithmType", "None");
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
                            case "noteP":
                                NoteAlgorithm = a;
                                break;
                            case "attackP":
                                AttackAlgorithm = a;
                                break;
                            case "speedP":
                                SpeedAlgorithm = a;
                                break;
                            case "durationP":
                                DurationAlgorithm = a;
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
            }
        }

        public override ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = base.Validate();
            if (SoundFontFileName == null || SoundFontFileName.Length == 0) errors.Add(new Message() { Text = "SoundFont file must not be empty.", Error = true });
            if (PresetName == null || PresetName.Length == 0) errors.Add(new Message() { Text = "Preset name must not be empty.", Error = true });
            if (MeasureLength < 0) errors.Add(new Message() { Text = "Measure length must be greater than 0.", Error = true });
            if (BeatCount < 0 || BeatCount > MeasureLength) errors.Add(new Message() { Text = "Beat count must be greater than 0 and less than or equal to measure length.", Error = true });
            if (OffsetSequence < 0 || OffsetSequence >= MeasureLength) errors.Add(new Message() { Text = "Offset sequence must be greater than or equal to 0 and less than measure length.", Error = true });
            if (NoteCount < 0 || NoteCount > 12) errors.Add(new Message() { Text = "Note count must be greater than 0 and less than or equal to 12.", Error = true });
            if (NoiseFrequency < 0) errors.Add(new Message() { Text = "Noise frequency must be greater than or equal to 0.", Error = true });
            if (NoiseAmplitude < 0) errors.Add(new Message() { Text = "Noise amplitude must be greater than or equal to 0.", Error = true });
            // validate the parameters of the algorithms and add any errors to the errors list
            ObservableCollection<Message> noteE = NoteAlgorithm.Validate();
            ObservableCollection<Message> attackE = AttackAlgorithm.Validate();
            ObservableCollection<Message> speedE = SpeedAlgorithm.Validate();
            ObservableCollection<Message> volumeE = VolumeAlgorithm.Validate();
            ObservableCollection<Message> panE = PanAlgorithm.Validate();
            foreach (Message error in noteE) errors.Add(error);
            foreach (Message error in attackE) errors.Add(error);
            foreach (Message error in speedE) errors.Add(error);
            foreach (Message error in volumeE) errors.Add(error);
            foreach (Message error in panE) errors.Add(error);
            return errors;
        }
        public override string ToString() => "Algorithmic";
    }
}
