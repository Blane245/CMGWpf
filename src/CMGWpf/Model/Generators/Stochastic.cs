global using Composition = int[][];
using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using static CMGWpf.Model.Generators.StochasticTypes;

namespace CMGWpf.Model.Generators
{
    #region Stochastic Types
    public class StochasticTypes
    {
        public enum TIMBRE
        {
            sustained,
            glissando,
        }
        public enum INTENSITYOPTION
        {
            none,
            composition,
            voice,
            cloud
        }
        public enum INTENSITYTRANSITIONOPTION
        {
            none,
            random,
            persistent
        }
        public class IntensityParameters
        {
            public double CycleTime { get; set; }
        }

        public enum PANOPTION
        {
            none,
            composition,
            voice,
            cloud
        }
        public enum PANALGORITHM
        {
            none,
            glide,
            walk
        }
        public class PanParameters
        {
            public double CycleTime { get; set; }
        }
        public enum REVERBOPTION
        {
            none,
            composition,
            voice,
            cloud
        }
        public class ReverbParameters
        {
            public double Delay { get; set; } // milliseconds of delay
            public double Decay { get; set; } // decay level (dB)
        }
        public class Ensemble
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Voices { get; set; } = "";
        }
        public enum INTENSITY
        {
            pppp,
            ppp,
            pp,
            p,
            mp,
            mf,
            f,
            ff,
            fff,
            ffff
        }
        public class IntensityValue
        {
            public double DB { get; set; } = 0;
            public double Velocity { get; set; } = 0;
        };
        public static readonly Dictionary<INTENSITY, IntensityValue> IntensityProfiles = new Dictionary<INTENSITY, IntensityValue>
        {
            { INTENSITY.pppp, new IntensityValue { DB = -25, Velocity = 30 } },
            { INTENSITY.ppp, new IntensityValue { DB = -20, Velocity = 40 } },
            { INTENSITY.pp, new IntensityValue { DB = -10, Velocity = 50 } },
            { INTENSITY.p, new IntensityValue { DB = -5, Velocity = 60 } },
            { INTENSITY.mp, new IntensityValue { DB = -0, Velocity = 70 } },
            { INTENSITY.mf, new IntensityValue { DB = 5, Velocity = 80 } },
            { INTENSITY.f, new IntensityValue { DB = 10, Velocity = 90 } },
            { INTENSITY.ff, new IntensityValue { DB = 15, Velocity = 100 } },
            { INTENSITY.fff, new IntensityValue { DB = 20, Velocity = 110 } },
            { INTENSITY.ffff, new IntensityValue { DB = 25, Velocity = 120 } }
        };
        public class IntensityTransition
        {
            public INTENSITY Start { get; set; } = INTENSITY.pppp;
            public INTENSITY? Middle { get; set; } = null;
            public INTENSITY End { get; set; } = INTENSITY.pppp;
        };
        public static readonly IntensityTransition[] IntensityTransitions =
        [
            new IntensityTransition { Start = INTENSITY.ppp, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.ppp, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.ppp, Middle = INTENSITY.p, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.ppp, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.ppp, Middle = INTENSITY.f, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.ppp, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.ppp, Middle = INTENSITY.ff, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.ppp, Middle = INTENSITY.f, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.ppp, Middle = INTENSITY.ff, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.p, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.p, Middle = INTENSITY.f, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.p, Middle = INTENSITY.ppp, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.p, Middle = INTENSITY.ppp, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.p, Middle = INTENSITY.ff, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.p, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.p, Middle = INTENSITY.ppp, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.p, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.p, Middle = INTENSITY.f, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.p, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.p, Middle = INTENSITY.ff, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.p, Middle = INTENSITY.ff, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.f, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.f, Middle = INTENSITY.ppp, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.f, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.f, Middle = INTENSITY.ppp, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.f, Middle = INTENSITY.ff, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.f, Middle = INTENSITY.p, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.f, Middle = INTENSITY.ff, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.f, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.f, Middle = INTENSITY.ppp, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.f, Middle = INTENSITY.p, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.f, Middle = INTENSITY.ff, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.f, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.ff, End = INTENSITY.ppp },
            new IntensityTransition { Start = INTENSITY.ff, Middle = INTENSITY.ppp, End = INTENSITY.p },
            new IntensityTransition { Start = INTENSITY.ff, Middle = INTENSITY.ppp, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.ff, Middle = INTENSITY.p, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.ff, End = INTENSITY.f },
            new IntensityTransition { Start = INTENSITY.ff, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.ff, Middle = INTENSITY.ppp, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.ff, Middle = INTENSITY.p, End = INTENSITY.ff },
            new IntensityTransition { Start = INTENSITY.ff, Middle = INTENSITY.f, End = INTENSITY.ff },
        ];
    }
    #endregion

    #region Stochastic Generator
    public class Stochastic(int uid, Track parent) : Generator(uid, parent), INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Composition parameters. Changes to these property causes the composition to be cleared. Note use of OnPropertyChanged
        private Ensemble ensemble = new() { Name = "", Description = "", Voices = "" };
        public Ensemble Ensemble
        {
            get => ensemble;
            set
            {
                if (value.Name != ensemble.Name || value.Voices != ensemble.Voices) { Composition = []; }
                ensemble = value;
            }
        } // the ensemble name, description, and voiceList. If name or voice list changes, clear the composition
        private double compositionDuration = 0;
        public double CompositionDuration
        {
            get => compositionDuration;
            set { if (value != compositionDuration) { Composition = []; compositionDuration = value; }; }
        } // the length of the composition in seconds. If it changes, the composition is cleared
        private int numberOfTimeCells = 0;
        public int NumberOfTimeCells
        {
            get => numberOfTimeCells;
            set { if (value != numberOfTimeCells) { Composition = []; numberOfTimeCells = value; }; }
        } // the number of time cells in the composition. The time cells are used to determine the timing of events in the composition. The number of time cells is determined by the user and can be changed during the composition process. If it changes, clear the composition
        private double lambda = 0;
        public double Lambda { get => lambda; set { if (value != lambda) { Composition = []; lambda = value; }; } } // The average number of events per time cell. If it changes, clear the composition
        private string compositionSeed = string.Empty;
        public string CompositionSeed { get => compositionSeed; set { if (value != compositionSeed) { Composition = []; compositionSeed = value; }; } } // the seed for the random number generator used to determine the timing of events in the composition. The seed is provided by the user and can be changed during the composition process. If it changes, clear the composition
        public FastRandom CompositionRn = MathUtilities.StartFastRandom(null); // the random number generator used to determine the timing of events in the composition. The random number generator is initialized with the composition seed provided by the user. If no seed is provided, the random number generator is initialized with a seed determined from the current time.
        private ObservableCollection<Voice> voices = [];
        public ObservableCollection<Voice> Voices
        {
            get => voices;
            set { if (value.Count != voices.Count) { Composition = []; voices = value; } else voices = [.. value]; }
        } // the voices in the ensemble. The voices are read from the database and augmented with generator specific data. The voices are used to determine the sound of the events in the composition. user can change the settings for each voice, such as whether it is muted or not, and the volume and velocity. If the number of voices changes, clear the composition.
        // Signal Processing parameters
        public bool Microtones { get; set; } = false; // whether microtones are allowed in the composition. 
        public string DynamicsSeed { get; set; } = String.Empty; // the seed for the random number generator used to determine the dynamics of events in the composition. 
        public FastRandom DynamicsRn = MathUtilities.StartFastRandom(null); // the random number generator used to determine the dynamics of events in the composition. 
        public INTENSITYOPTION IntensityOption { get; set; } = INTENSITYOPTION.none; // the option for determining the intensity of events in the composition. The intensity of events can be determined by the composition, by the voice, or by a cloud of events. 
        public INTENSITYTRANSITIONOPTION IntensityTransitionOption { get; set; } = INTENSITYTRANSITIONOPTION.none; // the option for determining the transition of intensity of events in the composition. The transition of intensity can be random, persistent, or none. 
        public IntensityParameters IntensityParameters { get; set; } = new() { CycleTime = 0 }; // the parameters for determining the intensity of events in the composition. The parameters are used to determine the cycle time for changing the intensity of events in the composition. 
        public PANOPTION PanOption { get; set; } = PANOPTION.none; // the option for determining the pan of events in the composition. The pan of events can be determined by the composition, by the voice, or by a cloud of events. 
        public PANALGORITHM PanAlgorithm { get; set; } = PANALGORITHM.none; // the option for determining the algorithm for changing the pan of events in the composition. The algorithm for changing the pan can be glide, walk, or none. 
        public PanParameters PanParameters { get; set; } = new() { CycleTime = 0 }; // the parameters for determining the pan of events in the composition. The parameters are used to determine the cycle time for changing the pan of events in the composition. 
        public ReverbParameters ReverbParameters { get; set; } = new() { Delay = 0, Decay = 1 }; // the parameters for determining the reverb of events in the composition. The parameters are used to determine the delay and decay for the reverb effect. 

        // The Composition part
        private Composition composition = [];
        public Composition Composition { get => composition; set { composition = value; OnPropertyChanged(); } } // the composition is a two dimensional array that has the number of clouds in each cell. There is one cell for each voice for each time. If any of the composition parameters are changed, except voice UI parameters, a new composition is generated. The composition is generated by the GenerateComposition method, which is called whenever any of the composition parameters are changed. The composition is generated by iterating through each time cell and each voice, and determining whether an event occurs based on the lambda parameter and the random number generator. If an event occurs, the intensity and pan of the event are determined based on the intensity and pan options and parameters.

        public void InitializeComposition()
        {
            if (!string.IsNullOrEmpty(CompositionSeed))
            {
                CompositionRn = MathUtilities.StartFastRandom(CompositionSeed);
            } else CompositionRn = MathUtilities.StartFastRandom(null);
        }
        public void InitializeDynamics()
        {
            if (!string.IsNullOrEmpty(DynamicsSeed))
            {
                DynamicsRn = MathUtilities.StartFastRandom(DynamicsSeed);
            } else DynamicsRn = MathUtilities.StartFastRandom(null);
        }

        public double GetDeltaT() { return NumberOfTimeCells == 0 ? Double.PositiveInfinity : CompositionDuration / NumberOfTimeCells; }
        public override bool Equals(Generator value)
        {
            if (value is Stochastic g)
            {
                if (base.Equals(value) &&
                    Ensemble == g.Ensemble &&
                    CompositionDuration == g.CompositionDuration &&
                    NumberOfTimeCells == g.NumberOfTimeCells &&
                    Lambda == g.Lambda &&
                    CompositionSeed == g.CompositionSeed &&
                    Voices == g.Voices &&
                    Microtones == g.Microtones &&
                    DynamicsSeed == g.DynamicsSeed &&
                    IntensityOption == g.IntensityOption &&
                    IntensityTransitionOption == g.IntensityTransitionOption &&
                    IntensityParameters.CycleTime == g.IntensityParameters.CycleTime &&
                    PanOption == g.PanOption &&
                    PanAlgorithm == g.PanAlgorithm &&
                    PanParameters.CycleTime == g.PanParameters.CycleTime &&
                    Composition == g.Composition
                    )
                    return true;
                else
                    return false;
            }
            else return false;
        }
        public override Stochastic Clone(Track parent)
        {
            var clone = (Stochastic)this.MemberwiseClone();
            clone.Parent = parent;

            // Deep copy the Ensemble
            clone.Ensemble = new Ensemble
            {
                Name = this.Ensemble.Name,
                Description = this.Ensemble.Description,
            };

            // Deep copy the voices so changes don't affect the original
            clone.Voices = [];
            foreach (var voice in this.Voices)
            {
                var voiceCopy = new Voice
                {
                    Name = voice.Name,
                    Description = voice.Description,
                    SoundFontFileName = voice.SoundFontFileName,
                    SoundFont = voice.SoundFont,
                    PresetName = voice.PresetName,
                    Preset = voice.Preset,
                    Timbre = voice.Timbre,
                    RegisterLo = voice.RegisterLo,
                    RegisterHi = voice.RegisterHi,
                    Duration = voice.Duration,
                    Muted = voice.Muted,
                    Volume = voice.Volume,
                    Velocity = voice.Velocity,
                    Delta = voice.Delta
                };
                clone.Voices.Add(voiceCopy);
            }

            // Deep copy the composition array
            if (this.Composition != null && this.Composition.Length > 0)
            {
                clone.Composition = new int[this.Composition.Length][];
                for (int i = 0; i < this.Composition.Length; i++)
                {
                    clone.Composition[i] = (int[])this.Composition[i].Clone();
                }
            }

            // Create new Random instances
            clone.CompositionRn = CompositionRn;
            clone.DynamicsRn = DynamicsRn;

            // Deep copy IntensityParameters
            clone.IntensityParameters = new IntensityParameters
            {
                CycleTime = this.IntensityParameters.CycleTime
            };

            // Deep copy PanParameters
            clone.PanParameters = new PanParameters
            {
                CycleTime = this.PanParameters.CycleTime
            };

            // Deep copy ReverbParameters
            clone.ReverbParameters = new ReverbParameters
            {
                Delay = this.ReverbParameters.Delay,
                Decay = this.ReverbParameters.Decay
            };

            return clone;
        }
        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("type", this.ToString());
            elem.SetAttribute("name", Name);
            elem.SetAttribute("startTime", StartTime.ToString());
            elem.SetAttribute("stopTime", StopTime.ToString());
            elem.SetAttribute("mute", Mute.ToString());
            elem.SetAttribute("position", Position.ToString());
            XmlElement ensembleElem = doc.CreateElement("ensemble");
            elem.AppendChild(ensembleElem);
            ensembleElem.SetAttribute("name", Ensemble.Name);
            ensembleElem.SetAttribute("description", Ensemble.Description);
            ensembleElem.SetAttribute("voices", Ensemble.Voices);
            XmlElement voicesElem = doc.CreateElement("voices");
            elem.AppendChild(voicesElem);
            foreach (Voice voice in Voices)
            {
                XmlElement voiceElem = doc.CreateElement("voice");
                voicesElem.AppendChild(voiceElem);
                voiceElem.SetAttribute("name", voice.Name);
                voiceElem.SetAttribute("description", voice.Description);
                voiceElem.SetAttribute("soundFontFile", voice.SoundFontFileName);
                voiceElem.SetAttribute("presetName", voice.PresetName);
                voiceElem.SetAttribute("timbre", voice.Timbre.ToString());
                voiceElem.SetAttribute("registerLo", voice.RegisterLo.ToString());
                voiceElem.SetAttribute("registerHi", voice.RegisterHi.ToString());
                voiceElem.SetAttribute("duration", voice.Duration.ToString());
                voiceElem.SetAttribute("muted", voice.Muted.ToString());
                voiceElem.SetAttribute("volume", voice.Volume.ToString());
                voiceElem.SetAttribute("velocity", voice.Velocity.ToString());
                voiceElem.SetAttribute("delta", voice.Delta.ToString());
            }
            elem.SetAttribute("Tc", CompositionDuration.ToString());
            elem.SetAttribute("Nt", NumberOfTimeCells.ToString());
            elem.SetAttribute("lambda", Lambda.ToString());
            elem.SetAttribute("compositionSeed", CompositionSeed);
            elem.SetAttribute("microtones", Microtones.ToString());
            elem.SetAttribute("dynamicsSeed", DynamicsSeed);
            XmlElement intensityElem = doc.CreateElement("intensity");
            elem.AppendChild(intensityElem);
            intensityElem.SetAttribute("intensityOption", IntensityOption.ToString());
            intensityElem.SetAttribute("intensityTransitionOption", IntensityTransitionOption.ToString());
            intensityElem.SetAttribute("cycleTime", IntensityParameters.CycleTime.ToString());
            XmlElement panElem = doc.CreateElement("pan");
            elem.AppendChild(panElem);
            panElem.SetAttribute("panOption", PanOption.ToString());
            panElem.SetAttribute("panAlgorithm", PanAlgorithm.ToString());
            panElem.SetAttribute("cycleTime", PanParameters.CycleTime.ToString());
            XmlElement reverbElem = doc.CreateElement("reverb");
            elem.AppendChild(reverbElem);
            reverbElem.SetAttribute("delay", ReverbParameters.Delay.ToString());
            reverbElem.SetAttribute("decay", ReverbParameters.Decay.ToString());
            // add the composition as a string in row/column order
            string compositionString = String.Empty;
            for (int i = 0; i < Composition.Length; i++)
            {
                for (int j = 0; j < Composition[i].Length; j++)
                {
                    compositionString += Composition[i][j].ToString() + ",";
                }
                compositionString += ";";
            }
            elem.SetAttribute("composition", compositionString);
        }
        public override Task LoadXML(XmlElement generatorElem, Track parent)
        {
            Name = XMLFunctions.GetAttributeString(generatorElem, "name", "");
            Parent = parent;
            StartTime = XMLFunctions.GetAttributeDouble(generatorElem, "startTime", 0);
            StopTime = XMLFunctions.GetAttributeDouble(generatorElem, "stopTime", 0);
            Position = XMLFunctions.GetAttributeInt(generatorElem, "position", 0);
            Mute = XMLFunctions.GetAttributeBool(generatorElem, "mute", false);

            // Load ensemble
            XmlElement? ensembleElem = generatorElem.GetElementsByTagName("ensemble").Cast<XmlElement?>().FirstOrDefault();
            if (ensembleElem != null) // load the ensemble and voices from XML, but then check the ensemble against the database to see if it has changed. If the ensemble has changed, then we will need to regenerate the composition, and we will load the voices from the database instead of the XML, because the voices in the XML may not match the voices in the database for the new ensemble. If the ensemble has not changed, then we can load the voices from the XML without needing to check them against the database, because they will match the voices in the database for the same ensemble.
            {
                Ensemble = new Ensemble
                {
                    Name = XMLFunctions.GetAttributeString(ensembleElem, "name", ""),
                    Description = XMLFunctions.GetAttributeString(ensembleElem, "description", ""),
                    Voices = XMLFunctions.GetAttributeString(ensembleElem, "voices", "")
                };
                var delta = XMLFunctions.GetAttributeDouble(generatorElem, "delta", 0); // get as default based on old format
                XmlElement? voicesElem = generatorElem.GetElementsByTagName("voices").Cast<XmlElement?>().FirstOrDefault();
                if (voicesElem != null)
                {
                    var voiceList = new List<Voice>();
                    foreach (XmlElement voiceElem in voicesElem.GetElementsByTagName("voice"))
                    {
                        Voice voice = new()
                        {
                            Name = XMLFunctions.GetAttributeString(voiceElem, "name", ""),
                            Description = XMLFunctions.GetAttributeString(voiceElem, "description", ""),
                            SoundFontFileName = XMLFunctions.GetAttributeString(voiceElem, "soundFontFile", ""),
                            PresetName = XMLFunctions.GetAttributeString(voiceElem, "presetName", ""),
                            Timbre = Enum.Parse<TIMBRE>(XMLFunctions.GetAttributeString(voiceElem, "timbre", "Sustained")),
                            RegisterLo = XMLFunctions.GetAttributeDouble(voiceElem, "registerLo", 0),
                            RegisterHi = XMLFunctions.GetAttributeDouble(voiceElem, "registerHi", 127),
                            Duration = XMLFunctions.GetAttributeDouble(voiceElem, "duration", 1),
                            Muted = XMLFunctions.GetAttributeBool(voiceElem, "muted", false),
                            Volume = XMLFunctions.GetAttributeDouble(voiceElem, "volume", 0),
                            Velocity = XMLFunctions.GetAttributeDouble(voiceElem, "velocity", 64),
                            Delta = XMLFunctions.GetAttributeDouble(voiceElem, "delta", delta), // use old format default is missing
                        };
                        // Read the soundfont from the identified file, and find the preset in the soundfont. If the soundfont or preset cannot be found, then these fields should be cleared. This is because the soundfont and preset are used to generate the sound for the voice, and if they cannot be found, then the voice cannot be generated, so these fields should be cleared to reflect that.
                        voice.SoundFont = SoundFontUtilities.GetSoundFont(voice.SoundFontFileName);
                        if (voice.PresetName != "" && voice.SoundFont != null)
                        {
                            voice.Preset = voice.SoundFont.Presets.FirstOrDefault(p => SoundFontUtilities.BankPresetToName(p) == voice.PresetName);
                        }
                        else
                        {
                            voice.Preset = null;
                            voice.PresetName = "";
                        }

                        voiceList.Add(voice);
                    }
                    Voices = [.. voiceList];
                }
                else
                {
                    // Ensemble not found in DB or error occurred - clear ensemble
                    Ensemble = new();
                    Voices = [];
                    Composition = [];
                }

                // Load composition parameters
                CompositionDuration = XMLFunctions.GetAttributeDouble(generatorElem, "Tc", 0);
                NumberOfTimeCells = XMLFunctions.GetAttributeInt(generatorElem, "Nt", 0);
                Lambda = XMLFunctions.GetAttributeDouble(generatorElem, "lambda", 0);
                CompositionSeed = XMLFunctions.GetAttributeString(generatorElem, "compositionSeed", "");
                CompositionRn = MathUtilities.StartFastRandom(CompositionSeed);

                // Load signal processing parameters
                Microtones = XMLFunctions.GetAttributeBool(generatorElem, "microtones", false);
                DynamicsSeed = XMLFunctions.GetAttributeString(generatorElem, "dynamicsSeed", "");
                DynamicsRn = MathUtilities.StartFastRandom(DynamicsSeed);

                // Load intensity parameters
                XmlElement? intensityElem = generatorElem.GetElementsByTagName("intensity").Cast<XmlElement?>().FirstOrDefault();
                if (intensityElem != null)
                {
                    IntensityOption = Enum.Parse<INTENSITYOPTION>(XMLFunctions.GetAttributeString(intensityElem, "intensityOption", "None"));
                    IntensityTransitionOption = Enum.Parse<INTENSITYTRANSITIONOPTION>(XMLFunctions.GetAttributeString(intensityElem, "intensityTransitionOption", "None"));
                    IntensityParameters = new IntensityParameters
                    {
                        CycleTime = XMLFunctions.GetAttributeDouble(intensityElem, "cycleTime", 0)
                    };
                }

                // Load pan parameters
                XmlElement? panElem = generatorElem.GetElementsByTagName("pan").Cast<XmlElement?>().FirstOrDefault();
                if (panElem != null)
                {
                    PanOption = Enum.Parse<PANOPTION>(XMLFunctions.GetAttributeString(panElem, "panOption", "None"));
                    PanAlgorithm = Enum.Parse<PANALGORITHM>(XMLFunctions.GetAttributeString(panElem, "panAlgorithm", "None"));
                    PanParameters = new PanParameters
                    {
                        CycleTime = XMLFunctions.GetAttributeDouble(panElem, "cycleTime", 0)
                    };
                }
                // Load reverb parameters
                XmlElement? reverbElem = generatorElem.GetElementsByTagName("reverb").Cast<XmlElement?>().FirstOrDefault();
                if (reverbElem != null)
                {
                    ReverbParameters = new ReverbParameters
                    {
                        Delay = XMLFunctions.GetAttributeDouble(reverbElem, "delay", 0),
                        Decay = XMLFunctions.GetAttributeDouble(reverbElem, "decay", 0)
                    };
                }
                Composition = [];
                string compositionString = XMLFunctions.GetAttributeString(generatorElem, "composition", "");
                if (!string.IsNullOrEmpty(compositionString))
                {
                    string[] rows = compositionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    if (rows.Length == NumberOfTimeCells)
                    {
                        // New format with semicolons separating rows
                        Composition = new int[rows.Length][];
                        for (int i = 0; i < rows.Length; i++)
                        {
                            string[] values = rows[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                            Composition[i] = new int[values.Length];
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (int.TryParse(values[j], out int val))
                                    Composition[i][j] = val;
                            }
                        }
                    }
                    else
                    {
                        // Old format: flat comma-separated list that needs reshaping
                        // Expected format: [time0_voice0, time0_voice1, ..., time1_voice0, time1_voice1, ...]
                        string[] values = compositionString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        int numVoices = Voices.Count;
                        if (numVoices > 0 && values.Length > 0)
                        {
                            int numTimeCells = values.Length / numVoices;
                            Composition = new int[numTimeCells][];
                            for (int i = 0; i < numTimeCells; i++)
                            {
                                Composition[i] = new int[numVoices];
                                for (int j = 0; j < numVoices; j++)
                                {
                                    int index = i * numVoices + j;
                                    if (index >= values.Length)
                                    {
                                        // incompatibility between number of voices, number of times, or both and the data in the file. Clear the composition and quit.
                                        Composition = [];
                                        return Task.CompletedTask;
                                    }
                                    if (index < values.Length && int.TryParse(values[index], out int val))
                                        Composition[i][j] = val;
                                }
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
        public override double GetEndTime()
        {
            return StopTime + CompositionDuration / numberOfTimeCells; // add one time cells to provide for cloud overflows
        }
        // CMG does not use CurrentValues from the Stochastic class. Rather, values calculations are done in the GetSourcesFromStochastic routine.
        public override CurrentValues GetCurrentValues(double time, double beats)
        {
            return new CurrentValues
            {
                Note = 0,
                Attack = 0,
                Speed = 0,
                Duration = 0,
                Volume = 0,
                Pan = 0
            };
        }
        public override ObservableCollection<Message> Validate()
        {

            ObservableCollection<Message> errors = base.Validate();
            if (CompositionDuration <= 0)
                errors.Add(new Message() { Text = "Composition duration must be positive.", Error = true });
            if (NumberOfTimeCells <= 0)
                errors.Add(new Message() { Text = "Number of time cells must be positive.", Error = true });
            if (Lambda <= 0)
                errors.Add(new Message() { Text = "Events/Row must be positive.", Error = true });
            if (Voices.Count == 0)
                errors.Add(new Message() { Text = "At least one voice is required.", Error = true });
            if (PanOption != PANOPTION.none && PanAlgorithm != PANALGORITHM.none && PanParameters.CycleTime <= 0)
                errors.Add(new Message() { Text = "Pan cycle time must be positive.", Error = true });
            if (IntensityOption != INTENSITYOPTION.none && IntensityTransitionOption != INTENSITYTRANSITIONOPTION.none && IntensityParameters.CycleTime <= 0)
                errors.Add(new Message() { Text = "Intensity cycle time must be positive.", Error = true });
            if (Composition.Length == 0)
                errors.Add(new Message() { Text = "Composition must be generated.", Error = true });
            foreach (var voice in Voices)
            {
                if (voice.Delta <= 0)
                    errors.Add(new Message() { Text = $"Voice '{voice.Name}': Delta must be positive.", Error = true });
            }
            return errors;
        }
        public override string ToString() => "Stochastic";
    }
    #endregion
}
