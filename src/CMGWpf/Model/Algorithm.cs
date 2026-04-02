using CMGWpf.Model.Generators;
using CMGWpf.Types;
using CMGWpf.Utilities;
using Microsoft.Xaml.Behaviors.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using static CMGWpf.Types.DBTypes;

namespace CMGWpf.Model
{
    public enum ALGORITHMTYPE
    {
        Constant,
        Oscillator,
        Markovian,
        Wiener,
        Autoregressive,
        Sequence
    }
    public enum MARKOVSTATE
    {
        SAME,
        UP,
        DOWN,
    }
    public enum SEQUENCEATTRIBUTE
    {
        attack,
        duration,
        volue
    }
    public static class AlgorithmFactory
    {
        public static Algorithm CreateAlgorithm(ALGORITHMTYPE type)
        {
            return type switch
            {
                ALGORITHMTYPE.Constant => new Constant(),
                ALGORITHMTYPE.Oscillator => new Oscillator(),
                ALGORITHMTYPE.Markovian => new Markovian(),
                ALGORITHMTYPE.Wiener => new Wiener(),
                ALGORITHMTYPE.Autoregressive => new Autoregressive(),
                ALGORITHMTYPE.Sequence => new Sequence(),
                _ => throw new ArgumentException("Unknown generator type", nameof(type)),
            };
        }
    }
    public abstract class Algorithm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public abstract Algorithm Clone();
        public virtual bool Equals(Generator value) => base.Equals(value);
        public abstract double GetCurrentValue(double time);
        public abstract void AppendXML(XmlDocument doc, XmlElement elem);
        public abstract void LoadXML(XmlElement algorithmElem);
        public virtual ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = [];
            return errors;
        }
        public override string ToString() => "No Algorithm";

    }
    public class Constant() : Algorithm
    {
        private double _value = 0;
        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }
        public Constant(double? value) : this()
        {
            Value = value ?? 0;
        }
        public override Constant Clone()
        {
            return new Constant()
            {
                Value = this.Value
            };
        }
        public bool Equals(Algorithm value)
        {
            return value is not null && value is Constant a && this.Value == a.Value;
        }
        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("value", Value.ToString());
        }
        public override void LoadXML(XmlElement elem)
        {
            Value = XMLFunctions.GetAttributeDouble(elem, "value", 0);
        }
        public override ObservableCollection<Message> Validate() => [];
        public override double GetCurrentValue(double _time) => Value;
        public override string ToString() => "Constant";
    }
    public class Oscillator : Algorithm
    {
        private MODULATORTYPE _modulator = MODULATORTYPE.NoModulator;
        public MODULATORTYPE Modulator
        {
            get => _modulator;
            set
            {
                if (_modulator != value)
                {
                    _modulator = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _center = 0;
        public double Center
        {
            get => _center;
            set
            {
                if (_center != value)
                {
                    _center = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _frequency = 0;
        public double Frequency
        {
            get => _frequency;
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _amplitude = 0;
        public double Amplitude
        {
            get => _amplitude;
            set
            {
                if (_amplitude != value)
                {
                    _amplitude = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _phase = 0;
        public double Phase
        {
            get => _phase;
            set
            {
                if (_phase != value)
                {
                    _phase = value;
                    OnPropertyChanged();
                }
            }
        }

        public Oscillator()
        {
            Modulator = MODULATORTYPE.NoModulator;
            Center = 0;
            Frequency = 0;
            Amplitude = 0;
            Phase = 0;
        }
        override public Oscillator Clone()
        {
            return (Oscillator)MemberwiseClone();
        }
        public bool Equals(Algorithm value)
        {
            if (value is null)
                return false;
            if (this.GetType() != value.GetType()) return false;
            if (value is Oscillator a)
            {
                return
                    Modulator == a.Modulator &&
                    Center == a.Center &&
                    Frequency == a.Frequency &&
                    Amplitude == a.Amplitude &&
                    Phase == a.Phase;
            }
            else return false;
        }

        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("type", Modulator.ToString());
            elem.SetAttribute("center", Center.ToString());
            elem.SetAttribute("frequency", Frequency.ToString());
            elem.SetAttribute("amplitude", Amplitude.ToString());
            elem.SetAttribute("phase", Phase.ToString());
        }
        public override void LoadXML(XmlElement elem)
        {
            string modulatorString = XMLFunctions.GetAttributeString(elem, "type", MODULATORTYPE.NoModulator.ToString());
            if (Enum.TryParse<MODULATORTYPE>(modulatorString, out MODULATORTYPE modulator)) Modulator = modulator;
            Center = XMLFunctions.GetAttributeDouble(elem, "center", 0);
            Frequency = XMLFunctions.GetAttributeDouble(elem, "frequency", 0);
            Amplitude = XMLFunctions.GetAttributeDouble(elem, "amplitude", 0);
            Phase = XMLFunctions.GetAttributeDouble(elem, "phase", 0);
        }

        public override ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = [];
            if (Frequency < 0 || Frequency > 10000) errors.Add(new Message() { Text = "Frequency must be between 0 and 10000.", Error = true });
            if (Amplitude < 0 || Amplitude > 1000) errors.Add(new Message() { Text = "Amplitude must be between 0 and 1000.", Error = true });
            if (Phase < 0 || Phase > 360) errors.Add(new Message() { Text = "Phase must be between 0 and 360.", Error = true });
            return errors;
        }
        public override double GetCurrentValue(double time)
        {
            return Modulator switch
            {
                MODULATORTYPE.NoModulator => ModulatorFunctions.NoModulator(time, Center, Frequency / 1000, Amplitude, Phase),
                MODULATORTYPE.Sine => ModulatorFunctions.Sine(time, Center, Frequency / 1000, Amplitude, Phase),
                MODULATORTYPE.Square => ModulatorFunctions.Square(time, Center, Frequency / 1000, Amplitude, Phase),
                MODULATORTYPE.Triangle => ModulatorFunctions.Triangle(time, Center, Frequency / 1000, Amplitude, Phase),
                MODULATORTYPE.AscendingSawTooth => ModulatorFunctions.AscendingSawTooth(time, Center, Frequency / 1000, Amplitude, Phase),
                MODULATORTYPE.DescendingSawTooth => ModulatorFunctions.DescendingSawTooth(time, Center, Frequency / 1000, Amplitude, Phase),
                _ => 0,
            };
        }
        public override string ToString() => "Oscillator";
    }
    // This class is used to represent a row in the Markovian transition probabilities table in the UI. 
    // It is an unfortunate necessity to have this class because the DataGrid in WPF does not support ObservableCollection of arrays directly.
    public class TransitionRow(string label, ObservableCollection<string> values)
    {
        public string Label { get; set; } = label;
        public ObservableCollection<string> Values { get; set; } = values;
    }
    public class Markovian : Algorithm
    {
        private string _seed = "";
        public string Seed
        {
            get => _seed;
            set
            {
                if (_seed != value)
                {
                    _seed = value;
                    OnPropertyChanged();
                }
            }
        }
        public Random Random { get; set; } = new();

        public void InitializeRandom()
        {
            if (!string.IsNullOrEmpty(Seed))
            {
                Random = new Random(Seed.GetHashCode());
            }
        }
        private MARKOVSTATE CurrentState { get; set; } = MARKOVSTATE.SAME;
        private double CurrentValue { get; set; } = 0;
        private double _start = 0;
        public double Start { get => _start; set { if (_start != value) { _start = value; OnPropertyChanged(); } } }
        private double lo = 0;
        public double Lo { get { return lo; } set { if (lo != value) { lo = value; OnPropertyChanged(); } } }
        private double hi = 0;
        public double Hi { get { return hi; } set { if (hi != value) { hi = value; OnPropertyChanged(); } } }
        private double step = 0;
        public double Step { get { return step; } set { if (step != value) { step = value; OnPropertyChanged(); } } }
        // TransitionProbabilities is a 3x3 matrix representing the transition probabilities between the states: SAME, UP, and DOWN.
        private double[,] TransitionProbabilities = new double[3, 3]
        {
            {1, 0, 0 }, // SAME
            { 1, 0, 0 }, // UP
            { 1, 0, 0}  // DOWN
        };
        // TransitionRows is an ObservableCollection of TransitionRow objects that represent the transition probabilities in a format suitable for data binding in WPF.
        private ObservableCollection<TransitionRow> transitionRows = [
                new("Same", ["1", "0", "0"]),
                new("Up", [ "1", "0", "0" ]),
                new("Down",[ "1", "0", "0" ])
            ];
        public ObservableCollection<TransitionRow> TransitionRows { get { return transitionRows; } set { transitionRows = value; } }
        public Markovian() { }
        public override Markovian Clone()
        {
            Markovian n = new()
            {
                Seed = this.Seed,
                Random = this.Seed != "" ? new Random(Seed.GetHashCode()) : new Random(),
                CurrentState = this.CurrentState,
                CurrentValue = this.CurrentValue,
                Start = this.Start,
                Lo = this.Lo,
                Hi = this.Hi,
                Step = this.Step,
                TransitionProbabilities = (double[,])this.TransitionProbabilities.Clone(),
                TransitionRows =
            [
                new TransitionRow("Same", [TransitionProbabilities[0, 0].ToString(), TransitionProbabilities[0, 1].ToString(), TransitionProbabilities[0, 2].ToString()]),
                new TransitionRow("Up", [TransitionProbabilities[1, 0].ToString(), TransitionProbabilities[1, 1].ToString(), TransitionProbabilities[1, 2].ToString()]),
                new TransitionRow("Down", [TransitionProbabilities[2, 0].ToString(), TransitionProbabilities[2, 1].ToString(), TransitionProbabilities[2, 2].ToString()])
            ]
            };
            return n;
        }

        public bool Equals(Algorithm value)
        {
            return value.GetType() == GetType() &&
                value is Markovian g &&
                Seed == g.Seed &&
                Start == g.Start &&
                Lo == g.Lo &&
                Hi == g.Hi &&
                Step == g.Step &&
                TransitionProbabilities == g.TransitionProbabilities;
        }

        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("seed", Seed);
            elem.SetAttribute("start", Start.ToString());
            elem.SetAttribute("range.lo", Lo.ToString());
            elem.SetAttribute("range.hi", Hi.ToString());
            elem.SetAttribute("range.step", Step.ToString());
            for (int i = 0; i < 3; i++)
            {
                string prefix = i switch
                {
                    0 => "same",
                    1 => "up",
                    2 => "down",
                    _ => throw new InvalidOperationException("Invalid index")
                };

                for (int j = 0; j < 3; j++)
                {
                    string suffix = j switch
                    {
                        0 => "same",
                        1 => "up",
                        2 => "down",
                        _ => throw new InvalidOperationException("Invalid index")
                    };
                    elem.SetAttribute($"{prefix}.{suffix}", TransitionProbabilities[i, j].ToString());
                }
            }
        }
        public override void LoadXML(XmlElement elem)
        {
            Seed = XMLFunctions.GetAttributeString(elem, "seed", string.Empty);
            Random = (Seed == string.Empty) ? new Random() : new Random(Seed.GetHashCode());
            Start = XMLFunctions.GetAttributeDouble(elem, "start", 0);
            CurrentValue = Start;
            CurrentState = MARKOVSTATE.SAME;
            Lo = XMLFunctions.GetAttributeDouble(elem, "range.lo", 0);
            Hi = XMLFunctions.GetAttributeDouble(elem, "range.hi", 0);
            Step = XMLFunctions.GetAttributeDouble(elem, "range.step", 0);
            TransitionProbabilities[0, 0] = XMLFunctions.GetAttributeDouble(elem, "same.same", 1);
            TransitionProbabilities[0, 1] = XMLFunctions.GetAttributeDouble(elem, "same.up", 0);
            TransitionProbabilities[0, 2] = XMLFunctions.GetAttributeDouble(elem, "same.down", 0);
            TransitionProbabilities[1, 0] = XMLFunctions.GetAttributeDouble(elem, "up.same", 1);
            TransitionProbabilities[1, 1] = XMLFunctions.GetAttributeDouble(elem, "up.up", 0);
            TransitionProbabilities[1, 2] = XMLFunctions.GetAttributeDouble(elem, "up.down", 0);
            TransitionProbabilities[2, 0] = XMLFunctions.GetAttributeDouble(elem, "down.same", 1);
            TransitionProbabilities[2, 1] = XMLFunctions.GetAttributeDouble(elem, "down.up", 0);
            TransitionProbabilities[2, 2] = XMLFunctions.GetAttributeDouble(elem, "down.down", 0);
            TransitionRows =
            [
                new TransitionRow("Same", [TransitionProbabilities[0, 0].ToString(), TransitionProbabilities[0, 1].ToString(), TransitionProbabilities[0, 2].ToString()]),
                new TransitionRow("Up", [TransitionProbabilities[1, 0].ToString(), TransitionProbabilities[1, 1].ToString(), TransitionProbabilities[1, 2].ToString()]),
                new TransitionRow("Down", [TransitionProbabilities[2, 0].ToString(), TransitionProbabilities[2, 1].ToString(), TransitionProbabilities[2, 2].ToString()])
            ];
        }
        public override ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = [];
            if (Lo > Hi)
            {
                errors.Add(new Message() { Text = "Markovian Lo must be less than or equal to Hi.", Error = true });
            }
            if (Step < 0)
            {
                errors.Add(new Message() { Text = "Markovian Step must be nonnegative.", Error = true });
            }
            if (Start < Lo || Start > Hi)
            {
                errors.Add(new Message() { Text = "Start must be between Lo and Hi, inclusive.", Error = true });
            }
            // loop through all of the transition rows to check if they can be parsed as double, update the transitionprobabilities if so, otherwise add an error message
            for (int i = 0; i < 3; i++)
            {
                ObservableCollection<string> row = TransitionRows[i].Values;
                for (int j = 0; j < 3; j++)
                {
                    string cell = row[j];
                    if (!double.TryParse(cell, out double value) || value < 0 || value > 1)
                    {
                        errors.Add(new Message() { Text = $"Markovian Transition probabilities for {((MARKOVSTATE)i).ToString()} to {((MARKOVSTATE)j).ToString()} must be a number and be between 0 and 1.", Error = true });
                    }
                    else
                    {
                        TransitionProbabilities[i, j] = value;
                    }
                }
            }
            if (Math.Abs(TransitionProbabilities[0, 0] + TransitionProbabilities[0, 1] + TransitionProbabilities[0, 2] - 1) > 0.001)
            {
                errors.Add(new Message() { Text = "Markovian Transition probabilities for SAME must sum to 1.", Error = true });
            }
            if (Math.Abs(TransitionProbabilities[1, 0] + TransitionProbabilities[1, 1] + TransitionProbabilities[1, 2] - 1) > 0.001)
            {
                errors.Add(new Message() { Text = "Markovian Transition probabilities for UP must sum to 1.", Error = true });
            }
            if (Math.Abs(TransitionProbabilities[2, 0] + TransitionProbabilities[2, 1] + TransitionProbabilities[2, 2] - 1) > 0.001)
            {
                errors.Add(new Message() { Text = "Markovian Transition probabilities for DOWN must sum to 1.", Error = true });
            }
            return errors;
        }

        public override double GetCurrentValue(double time)
        {
            // transition the current state to the next state based on the transition probabilities
            double randomValue = Random.NextDouble();
            MARKOVSTATE nextState;
            if (randomValue < TransitionProbabilities[(int)CurrentState, 0])
            {
                nextState = MARKOVSTATE.SAME;
            }
            else if (randomValue < TransitionProbabilities[(int)CurrentState, 0] + TransitionProbabilities[(int)CurrentState, 1])
            {
                nextState = MARKOVSTATE.UP;
            }
            else
            {
                nextState = MARKOVSTATE.DOWN;
            }
            CurrentState = nextState;
            double value = CurrentValue;
            switch (CurrentState)
            {
                case MARKOVSTATE.SAME:
                    break;
                case MARKOVSTATE.UP:
                    if (value + Step < Hi) value += Step; else value -= Step;
                    break;
                case MARKOVSTATE.DOWN:
                    if (value - Step > Lo) value -= Step; else value += Step;
                    break;
            }
            CurrentValue = value;
            return value;
        }
        public override string ToString() => "Markovian";
    }
    public class Wiener() : Algorithm
    {
        private string _seed = "";
        public string Seed
        {
            get => _seed;
            set
            {
                if (_seed != value)
                {
                    _seed = value;
                    OnPropertyChanged();
                }
            }
        }
        public Random Random { get; set; } = new();

        public void InitializeRandom()
        {
            if (!string.IsNullOrEmpty(Seed))
            {
                Random = new Random(Seed.GetHashCode());
            }
        }

        private double _initial = 0;
        public double Initial
        {
            get => _initial;
            set
            {
                if (_initial != value)
                {
                    _initial = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _trend = 0;
        public double Trend
        {
            get => _trend;
            set
            {
                if (_trend != value)
                {
                    _trend = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _dispersion = 0;
        public double Dispersion
        {
            get => _dispersion;
            set
            {
                if (_dispersion != value)
                {
                    _dispersion = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _lo = 0;
        public double Lo
        {
            get => _lo;
            set
            {
                if (_lo != value)
                {
                    _lo = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _hi = 0;
        public double Hi
        {
            get => _hi;
            set
            {
                if (_hi != value)
                {
                    _hi = value;
                    OnPropertyChanged();
                }
            }
        }

        public override Wiener Clone()
        {
            Wiener n = new()
            {
                Seed = this.Seed,
                Random = (Seed == string.Empty) ? new Random() : new Random(Seed.GetHashCode()),
                Initial = this.Initial,
                Trend = this.Trend,
                Dispersion = this.Dispersion,
                Lo = Lo,
                Hi = Hi,
            };
            return n;
        }
        public bool Equals(Algorithm value)
        {
            return value.GetType() == GetType() &&
                value is Wiener g &&
                Seed == g.Seed &&
                Initial == g.Initial &&
                Trend == g.Trend &&
                Dispersion == g.Dispersion &&
                Lo == g.Lo &&
                Hi == g.Hi;
        }
        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("seed", Seed);
            elem.SetAttribute("initial", Initial.ToString());
            elem.SetAttribute("trend", Trend.ToString());
            elem.SetAttribute("dispersion", Dispersion.ToString());
            elem.SetAttribute("lo", Lo.ToString());
            elem.SetAttribute("hi", Hi.ToString());
        }
        public override void LoadXML(XmlElement elem)
        {
            Seed = XMLFunctions.GetAttributeString(elem, "seed", string.Empty);
            Random = (Seed == string.Empty) ? new Random() : new Random(Seed.GetHashCode());
            Initial = XMLFunctions.GetAttributeDouble(elem, "initial", 0);
            Trend = XMLFunctions.GetAttributeDouble(elem, "trend", 0);
            Dispersion = XMLFunctions.GetAttributeDouble(elem, "dispersion", 0);
            Lo = XMLFunctions.GetAttributeDouble(elem, "lo", 0);
            Hi = XMLFunctions.GetAttributeDouble(elem, "hi", 0);
        }
        public override ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = [];
            if (Lo > Hi) errors.Add(new Message() { Text = "Wiener Lo must be less than or equal to Hi.", Error = true });
            if (Initial < Lo || Initial > Hi) errors.Add(new Message() { Text = "Wiener Initial must be between Lo and Hi.", Error = true });
            if (Trend < 0) errors.Add(new Message() { Text = "Wiener Trend must be nonnegative.", Error = true });
            if (Dispersion < 0) errors.Add(new Message() { Text = "Wiener Dispersion must be nonnegative.", Error = true });
            return errors;
        }

        public override double GetCurrentValue(double time)
        {
            double x = GaussianNoise.Get(Random, 0, Dispersion * Math.Sqrt(time));
            double value = Math.Max(Math.Min(Initial + Trend * time + Dispersion * (time) * x, Hi), Lo);
            return value;
        }
        public override string ToString() => "Wiener";
    }
    public class Tremolo
    {
        public Tremolo() { }
        public double Speed { get; set; } = 0;
        public double Depth { get; set; } = 0;
        public MODULATORTYPE WaveForm { get; set; } = MODULATORTYPE.NoModulator;
        public Tremolo Clone()
        {
            return (Tremolo)MemberwiseClone();
        }
        public bool Equals(Tremolo value)
        {
            return value.GetType() == GetType() &&
                value is Tremolo g &&
                Speed == g.Speed &&
                Depth == g.Depth &&
                WaveForm == g.WaveForm;
        }
        public void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("speed", Speed.ToString());
            elem.SetAttribute("depth", Depth.ToString());
            elem.SetAttribute("waveform", WaveForm.ToString());
        }
        public void LoadXML(XmlElement elem)
        {
            Speed = XMLFunctions.GetAttributeDouble(elem, "speed", 0);
            Depth = XMLFunctions.GetAttributeDouble(elem, "depth", 0);
            string waveFormString = XMLFunctions.GetAttributeString(elem, "waveform", MODULATORTYPE.NoModulator.ToString());
            if (Enum.TryParse<MODULATORTYPE>(waveFormString, out var waveForm)) WaveForm = waveForm;
        }
        public double GetCurrentValue(double time)
        {
            double value = WaveForm switch
            {
                MODULATORTYPE.NoModulator => ModulatorFunctions.NoModulator(time, 0, Speed, Depth, 0),
                MODULATORTYPE.Sine => ModulatorFunctions.Sine(time, 0, Speed, Depth, 0),
                MODULATORTYPE.Square => ModulatorFunctions.Square(time, 0, Speed, Depth, 0),
                MODULATORTYPE.Triangle => ModulatorFunctions.Triangle(time, 0, Speed, Depth, 0),
                MODULATORTYPE.AscendingSawTooth => ModulatorFunctions.AscendingSawTooth(time, 0, Speed, Depth, 0),
                MODULATORTYPE.DescendingSawTooth => ModulatorFunctions.DescendingSawTooth(time, 0, Speed, Depth, 0),
                _ => 0,
            };
            return value;
        }
        public static ObservableCollection<string> Validate() { return []; }

        public override string ToString() => "Tremolo";
    }
    public class Autoregressive() : Algorithm
    {
        private string _seed = "";
        public string Seed { get => _seed; set { if (_seed != value) { _seed = value; OnPropertyChanged(); Random = (_seed == string.Empty) ? new Random() : new Random(_seed.GetHashCode()); OnPropertyChanged(nameof(Random)); } } }
        public Random Random { get; set; } = new();
        private double _initial = 0;
        public double Initial { get => _initial; set { if (_initial != value) { _initial = value; OnPropertyChanged(); } } }
        private double _alpha = 0;
        public double Alpha { get => _alpha; set { if (_alpha != value) { _alpha = value; OnPropertyChanged(); } } }
        private double _sigma = 0;
        public double Sigma { get => _sigma; set { if (_sigma != value) { _sigma = value; OnPropertyChanged(); } } }
        private double _lo = 0;
        public double Lo { get => _lo; set { if (_lo != value) { _lo = value; OnPropertyChanged(); } } }
        private double _hi = 0;
        public double Hi { get => _hi; set { if (_hi != value) { _hi = value; OnPropertyChanged(); } } }
        private double _currentValue = 0;
        public override Autoregressive Clone()
        {
            Autoregressive n = (Autoregressive)this.MemberwiseClone();
            n.Random = (Seed == string.Empty) ? new Random() : new Random(Seed.GetHashCode());
            return n;
        }
        public bool Equals(Algorithm value)
        {
            return value.GetType() == GetType() &&
                value is Autoregressive g &&
                Initial == g.Initial &&
                Seed == g.Seed &&
                Alpha == g.Alpha &&
                Sigma == g.Sigma &&
                Lo == g.Lo &&
                Hi == g.Hi;
        }
        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("seed", Seed);
            elem.SetAttribute("alpha", Alpha.ToString(CultureInfo.InvariantCulture));
            elem.SetAttribute("sigma", Sigma.ToString(CultureInfo.InvariantCulture));
            elem.SetAttribute("lo", Lo.ToString(CultureInfo.InvariantCulture));
            elem.SetAttribute("hi", Hi.ToString(CultureInfo.InvariantCulture));
        }
        public override void LoadXML(XmlElement elem)
        {
            Seed = XMLFunctions.GetAttributeString(elem, "seed", string.Empty);
            Random = (Seed == string.Empty) ? new Random() : new Random(Seed.GetHashCode());
            Alpha = XMLFunctions.GetAttributeDouble(elem, "alpha", 0);
            Sigma = XMLFunctions.GetAttributeDouble(elem, "sigma", 0);
            Lo = XMLFunctions.GetAttributeDouble(elem, "lo", 0);
            Hi = XMLFunctions.GetAttributeDouble(elem, "hi", 0);
        }
        public override ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = [];
            if (Lo > Hi) errors.Add(new Message() { Text = "Autoregressive Lo must be less than or equal to Hi.", Error = true });
            if (Initial < Lo || Initial > Hi) errors.Add(new Message() { Text = "Autoregressive Initial must be between Lo and Hi.", Error = true });
            if (Alpha < 0) errors.Add(new Message() { Text = "Autoregressive Alpha must be positive.", Error = true });
            if (Sigma < 0) errors.Add(new Message() { Text = "Autoregressive Sigma must be positive.", Error = true });
            return errors;
        }
        public override double GetCurrentValue(double time)
        {
            if (time == 0) { _currentValue = Initial; return _currentValue; }
            double epsilon = Sigma * (Random.NextDouble() - 0.5);
            double newValue = Math.Clamp((_currentValue - Initial) * Alpha + epsilon + Initial, Lo, Hi);
            _currentValue = newValue;
            return newValue;
        }
        public override string ToString() => "Autoregressive";
    }
    public class Sequence : Algorithm
    {
        private string _name = "";
        // when the sequence name changes , we need to load the sequence items from the CMG DB based on the new name. This is done in the setter of the Name property.
        private async void LoadSequenceItems(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Sequence sequence = await NoteSequenceUtilities.GetNoteSequenceAsync(name);
                _items = [.. sequence.Items];
            }
            else
            {
                _items = [];
            }
            OnPropertyChanged(nameof(Items));
        }
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value; LoadSequenceItems(_name); OnPropertyChanged();
                }
            }
        }
        private double _transpose = 0;
        public double Transpose { get => _transpose; set { if (_transpose != value) { _transpose = value; OnPropertyChanged(); } } }
        private ObservableCollection<SequenceItem> _items = [];
        public ObservableCollection<SequenceItem> Items { get => _items; set { if (_items != value) { _items = value; OnPropertyChanged(); } } }
        private bool _reverseSequence = false;
        public bool ReverseSequence { get => _reverseSequence; set { if (_reverseSequence != value) { _reverseSequence = value; OnPropertyChanged(); } } }
        private bool _reflectSequence = false;
        public bool ReflectSequence { get => _reflectSequence; set { if (_reflectSequence != value) { _reflectSequence = value; OnPropertyChanged(); } } }
        private double _reflectPitch = 0;
        public double ReflectPitch { get => _reflectPitch; set { if (_reflectPitch != value) { _reflectPitch = value; OnPropertyChanged(); } } }

        public Sequence() { }
        public override Sequence Clone()
        {
            Sequence n = (Sequence)this.MemberwiseClone();
            n.Items = [.. this.Items];
            return n;
        }
        public bool Equals(Algorithm value)
        {
            return value.GetType() == GetType() &&
                value is Sequence g &&
                Name == g.Name &&
                Transpose == g.Transpose &&
                ReverseSequence == g.ReverseSequence &&
                ReflectSequence == g.ReflectSequence &&
                ReflectPitch == g.ReflectPitch &&
                Items.SequenceEqual(g.Items);
        }
        public override void AppendXML(XmlDocument doc, XmlElement elem)
        {
            elem.SetAttribute("name", Name);
            elem.SetAttribute("transpose", Transpose.ToString());
            elem.SetAttribute("reverseSequence", ReverseSequence.ToString());
            elem.SetAttribute("reflectSequence", ReflectSequence.ToString());
            elem.SetAttribute("reflectPitch", ReflectPitch.ToString());
        }
        public override void LoadXML(XmlElement elem)
        {
            Name = XMLFunctions.GetAttributeString(elem, "name", "");
            Transpose = XMLFunctions.GetAttributeDouble(elem, "transpose", 0);
            ReverseSequence = XMLFunctions.GetAttributeBool(elem, "reverseSequence", false);
            ReflectSequence = XMLFunctions.GetAttributeBool(elem, "reflectSequence", false);
            ReflectPitch = XMLFunctions.GetAttributeDouble(elem, "reflectPitch", 0);
            // load the sequence from the CMG DB based on the name
            Items = NoteSequenceUtilities.GetNoteSequenceAsync(Name).Result.Items;
        }
        public override ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = [];
            if (string.IsNullOrEmpty(Name)) errors.Add(new Message() { Text = "Sequence Name cannot be empty.", Error = true });
            if (Items == null || Items.Count == 0) errors.Add(new Message() { Text = "Sequence must have at least one item.", Error = true });
            return errors;
        }
        public void SetReverse()
        {
            if (this.ReflectSequence)
            {
                Items = [.. Items.Reverse()];
            }
        }
        public void SetReflect()
        {
            if (this.ReflectSequence)
            {
                Items = [.. Items.Select(item => new SequenceItem {
                    value = 2 * ReflectPitch - item.value,
                beats = item.beats,
                id = item.id})];
            }
        }
        private int beatsToIndex(double beat, SequenceItem[] items)
        {
            if (items.Length == 0) return 0;
            double beatSum = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].beats + beatSum >= beat - 1) return i;
                beatSum += items[i].beats;
                if (i == items.Length - 1) return i;
            }
            return -1;
        }
        public override double GetCurrentValue(double time)
        {
            int itemIndex = beatsToIndex(time, [.. Items]);
            double value = itemIndex < 0 ? 0 : Items[itemIndex].value;
            return value + Transpose;
        }
        public override string ToString() => "Sequence";

    }
}
