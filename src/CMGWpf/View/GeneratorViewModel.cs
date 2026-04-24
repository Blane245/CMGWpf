using CMGWpf.Dialogs;
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.MVVM;
using CMGWpf.Services;
using CMGWpf.SoundFont_2;
using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using static CMGWpf.Model.Generators.StochasticTypes;
using System.Diagnostics;

namespace CMGWpf.View
{
    public class GeneratorViewModel(Model.Generators.Generator generator, TrackViewModel parentViewModel) : ViewModelBase
    {
        private Model.Generators.Generator generator = generator;
        private readonly TrackViewModel parentViewModel = parentViewModel;
        private Model.Generators.Generator UIgenerator = generator.Clone(parentViewModel.Track); // This is the generator instance that the UI is currently editing. It is a copy of the original generator for editing purposes, and will be used to update the original generator on submit.

        // Call this after construction to initialize subscriptions
        public void InitializeSubscriptions()
        {
            if (UIgenerator is Stochastic)
            {
                SubscribeToVoices();
            }
            if (UIgenerator is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += OnModelPropertyChanged;
            }
        }
        #region Common View Properties
        // when a generator change affects its parent track, a notification is sent to the parent
        public void NotifyTrackChanged(TrackViewModel? trackVm = null)
        {
            if (trackVm == null)
                parentViewModel.NotifyTrackChanged(Generator.Parent);
            else
                trackVm.NotifyTrackChanged(trackVm.Track);
        }
        public Model.Generators.Generator Generator => generator;

        // Method to update the generator reference after modifications
        public void UpdateGenerator(Model.Generators.Generator updatedGenerator)
        {
            generator = updatedGenerator.Clone(generator.Parent);
            OnPropertyChanged(nameof(Generator));
        }
        public Model.Generators.Generator UIGenerator
        {
            get => UIgenerator;
            set
            {
                // Unsubscribe from old generator's PropertyChanged
                if (UIgenerator is INotifyPropertyChanged oldNotify)
                {
                    oldNotify.PropertyChanged -= OnModelPropertyChanged;
                }
                UIgenerator = value;
                // Subscribe to new generator's PropertyChanged
                if (UIgenerator is INotifyPropertyChanged newNotify)
                {
                    newNotify.PropertyChanged += OnModelPropertyChanged;
                }
                OnPropertyChanged();
                // Notify all dependent properties
                if (value.GetType() == typeof(Algorithmic))
                {
                    OnPropertyChanged(nameof(AlgorithmicGenerator));
                    OnPropertyChanged(nameof(SequencerName));
                    // Reset cached Algorithmic attribute descriptors so they get recreated with new generator
                    _noteAttribute = null;
                    _attackAttribute = null;
                    _speedAttribute = null;
                    _durationAttribute = null;
                    _volumeAttribute = null;
                    _panAttribute = null;
                    OnPropertyChanged(nameof(NoteAttribute));
                    OnPropertyChanged(nameof(AttackAttribute));
                    OnPropertyChanged(nameof(SpeedAttribute));
                    OnPropertyChanged(nameof(DurationAttribute));
                    OnPropertyChanged(nameof(VolumeAttribute));
                    OnPropertyChanged(nameof(PanAttribute));
                }
                if (value.GetType() == typeof(Stochastic))
                {
                    // Subscribe to voice property changes
                    SubscribeToVoices();
                    OnPropertyChanged(nameof(StochasticGenerator));
                    OnPropertyChanged(nameof(StochasticComposition));
                }
            }
        }
        // Handler for when the model (Stochastic) raises PropertyChanged
        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When the Stochastic model's Composition changes, notify the view that StochasticComposition changed
            if (e.PropertyName == nameof(Stochastic.Composition))
            {
                OnPropertyChanged(nameof(StochasticComposition));
            }
        }
        private Window? activeGeneratorDialog = null; // only one generator dialog per generator can be opened
        public Window? ActiveGeneratorDialog { get => activeGeneratorDialog; set { activeGeneratorDialog = value; OnPropertyChanged(); } }
        public string GeneratorDialogTitle { get => $"{Mode} Generator {UIGenerator.Name}"; }
        public string MoveCopyTitle { get => $"{MoveCopyMode} Generator '{UIGenerator.Name}'"; }

        private string newGeneratorName = "";
        public string NewGeneratorName
        {
            get { return newGeneratorName; }
            set { if (newGeneratorName != value) { newGeneratorName = value; OnPropertyChanged(); } }
        }
        public void InitializeNewTimes(double start, double stop)
        {
            newStartTime = start;
            newStopTime = stop;
        }
        private double newStartTime = 0;
        public double NewStartTime
        {
            get => newStartTime;
            set
            {
                if (newStartTime == value) return;
                double shift = value - UIGenerator.StartTime;
                newStartTime = value;
                newStopTime = UIgenerator.StopTime += shift;
                UIGenerator.StartTime = value;
                UIGenerator.StopTime = newStopTime;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NewStopTime));
                OnPropertyChanged(nameof(UIgenerator));
            }
        }
        private double newStopTime = 0;
        public double NewStopTime
        {
            get => newStopTime;
            set
            {
                if (newStopTime == value) return;
                newStopTime = value;
                UIgenerator.StopTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UIgenerator));
            }
        }

        private GeneratorEditMode mode = GeneratorEditMode.Modify;
        public GeneratorEditMode Mode { get => mode; set { mode = value; OnPropertyChanged(); } }
        private MoveCopyMode moveCopyMode = MoveCopyMode.Move;
        public MoveCopyMode MoveCopyMode
        {
            get { return moveCopyMode; }
            set { moveCopyMode = value; OnPropertyChanged(); }
        }
        public ObservableCollection<TrackViewModel> TrackViews => TracksViewModel.Instance.Tracks;
        public ObservableCollection<Track> Tracks
        {
            get
            {
                ObservableCollection<Track> theTracks = [];
                foreach (TrackViewModel trackVm in TrackViews)
                {
                    theTracks.Add(trackVm.Track);
                }
                return theTracks;

            }
        }

        private Track? selectedTrack;
        public Track? SelectedTrack
        {
            get => selectedTrack;
            set { selectedTrack = value; OnPropertyChanged(); }
        }
        public static TimeLine TimeLine => TimeLineViewModel.Instance.TimeLine;
        private static double TimeToOffset(double time)
        {
            double result = SizeService.Instance.DisplayWidth * (time - TimeLine.StartTime) / TimeLineTypes.TimeLineScales[TimeLine.CurrentZoomLevel].Extent;
            return result;
        }

        // Position in pixels - calculated from start time
        public double StartOffset => TimeToOffset(Generator.StartTime);

        // Width in pixels - calculated from duration of generator
        public double Width
        {
            get
            {
                double duration = SizeService.Instance.DisplayWidth * (generator.StopTime - generator.StartTime) / TimeLineTypes.TimeLineScales[TimeLine.CurrentZoomLevel].Extent;
                return duration;
            }
        }

        // Vertical position - can be moved up/down to avoid overlap
        private double verticalOffset = generator.Position;
        public double VerticalOffset
        {
            get => verticalOffset;
            set
            {
                verticalOffset = value;
                UIgenerator.Position = Convert.ToInt32(value);
                OnPropertyChanged();
            }
        }

        // Height is 1/3 of track height as per requirements
        public static double Height => SizeService.Instance.TrackHeight / 3.0;
        private bool IsSelected()
        {
            bool selected = (Generator.StartTime >= TimeLine.TimeInterval.StartTime && Generator.StopTime <= TimeLine.TimeInterval.EndTime);
            return selected;
        }

        // Background color based on generator type and selection state
        public void UpdateColor()
        {
            String type = Generator.ToString();
            bool selected = IsSelected();
            Brush brush = type switch
            {
                "Algorithmic" => selected ? Brushes.Cyan : Brushes.LightCyan,
                "Stochastic" => selected ? Brushes.Coral : Brushes.LightCoral,
                _ => Brushes.White
            };
            _backgroundColor = brush;
            OnPropertyChanged(nameof(BackgroundColor));
        }
        private Brush _backgroundColor = Brushes.White;
        public Brush BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; OnPropertyChanged(); } }
        private ObservableCollection<Message> messages = [];
        public ObservableCollection<Message> Messages
        {
            get { return messages; }
            set
            {
                if (messages != value)
                {
                    foreach (var item in value)
                    {
                        if (item.Error) item.Brush = new SolidColorBrush(Colors.Red);
                        else item.Brush = new SolidColorBrush(Colors.Black);
                    }
                    messages = value; OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<Message> Status { set { FileViewModel.Instance.Messages = value; } }

        private object? generatorPanel;
        public object? GeneratorPanel
        {
            get => generatorPanel;
            set
            {
                generatorPanel = value;
                OnPropertyChanged();
            }
        }
        private UserControl? algorithmPanel;
        public UserControl? AlgorithmPanel
        {
            get => algorithmPanel;
            set
            {
                algorithmPanel = value;
                OnPropertyChanged();
            }
        }
        private UserControl? stochasticPanel;

        public UserControl? StochasticPanel
        {
            get { return stochasticPanel; }
            set { stochasticPanel = value; }
        }
        public bool IsDirty { get => GlobalService.Instance.IsDirty; set { GlobalService.Instance.IsDirty = value; OnPropertyChanged(); } }
        public void NotifyGeneratorChanged(string? name = null)
        {
            if (name == null)
            {
                OnPropertyChanged(nameof(Generator));
                OnPropertyChanged(nameof(StartOffset));
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(VerticalOffset));
                OnPropertyChanged(nameof(BackgroundColor));
            }
            else
            {
                OnPropertyChanged(name);
            }
        }
        #endregion
        #region Generator General Commands
        private RelayCommand<Model.Generators.Generator>? _editCommand;
        public RelayCommand<Model.Generators.Generator> EditCommand =>
            _editCommand ??= new RelayCommand<Model.Generators.Generator>(execute => new GeneratorCommands(this, UIGenerator).Edit());
        private RelayCommand<Model.Generators.Generator>? _copyCommand;
        public RelayCommand<Model.Generators.Generator> CopyCommand =>
            _copyCommand ??= new RelayCommand<Model.Generators.Generator>(execute => new GeneratorCommands(this, UIGenerator).Copy());

        private RelayCommand<Model.Generators.Generator>? _moveCommand;
        public RelayCommand<Model.Generators.Generator> MoveCommand =>
            _moveCommand ??= new RelayCommand<Model.Generators.Generator>(execute => new GeneratorCommands(this, UIGenerator).Move());
        private RelayCommand<Model.Generators.Generator>? _muteCommand;
        public RelayCommand<Model.Generators.Generator> MuteCommand =>
            _muteCommand ??= new RelayCommand<Model.Generators.Generator>(execute => new GeneratorCommands(this, UIGenerator).Mute());

        private RelayCommand<Model.Generators.Generator>? _playCommand;
        public RelayCommand<Model.Generators.Generator> PlayCommand =>
            _playCommand ??= new RelayCommand<Model.Generators.Generator>(execute => new GeneratorCommands(this, UIGenerator).Play());

        private RelayCommand<Model.Generators.Generator>? _deleteCommand;
        public RelayCommand<Model.Generators.Generator> DeleteCommand =>
            _deleteCommand ??= new RelayCommand<Model.Generators.Generator>(execute => new GeneratorCommands(this, UIGenerator).Delete());
        private RelayCommand<Model.Generators.Generator>? _generatorPlayCommand;
        public RelayCommand<Model.Generators.Generator> GeneratorPlayCommand =>
            _generatorPlayCommand ??= new RelayCommand<Model.Generators.Generator>(generator =>
            {
                PlayFunctions.PlayEngine.StartUp(generator, true, true);
            });
        private RelayCommand<Model.Generators.Generator>? _generatorSubmitCommand;
        public RelayCommand<Model.Generators.Generator> GeneratorSubmitCommand =>
            _generatorSubmitCommand ??= new RelayCommand<Model.Generators.Generator>(execute => new GeneratorCommands(this, UIGenerator).Submit());
        private RelayCommand<Model.Generators.Generator>? _moveCopyCommand;
        public RelayCommand<Model.Generators.Generator> MoveCopyCommand =>
            _moveCopyCommand ??= new RelayCommand<Model.Generators.Generator>(execute => new GeneratorCommands(this, UIGenerator).MoveCopyAction());

        #endregion
        #region Left Mouse Generator Movement Commands
        // Handle mouse drag actions using native WPF mouse events (like TimeLineViewModel does)
        private FrameworkElement? _generatorBorder;

        private bool _isDragging;
        public bool IsDragging
        {
            get => _isDragging;
            set
            {
                _isDragging = value;
                OnPropertyChanged();
            }
        }

        // Called from TrackDisplay.xaml.cs when Border is loaded
        public void AttachMouseHandlers(FrameworkElement border)
        {
            _generatorBorder = border;

            // Attach native mouse event handlers (same pattern as TimeLineViewModel)
            border.MouseLeftButtonDown += GeneratorBorder_MouseLeftButtonDown;
            border.MouseMove += GeneratorBorder_MouseMove;
            border.MouseLeftButtonUp += GeneratorBorder_MouseLeftButtonUp;
            border.MouseLeave += GeneratorBorder_MouseLeave;
        }

        private void GeneratorBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_generatorBorder == null) return;

            Debug.WriteLine($"[{Generator.Name}] Mouse down - starting drag");
            IsDragging = true;

            // Capture mouse (same as TimeLineViewModel)
            _generatorBorder.CaptureMouse();
            _generatorBorder.Cursor = Cursors.SizeNS;

            e.Handled = true;
        }

        private void GeneratorBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (_generatorBorder == null || !_generatorBorder.IsMouseCaptured) return;

            Debug.WriteLine($"[{Generator.Name}] Mouse move with capture");

            // Get the Canvas parent to calculate position relative to track
            if (FindParentCanvas(_generatorBorder) is Canvas canvas)
            {
                Point position = e.GetPosition(canvas);
                DragGenerator(position.Y);
            }

            e.Handled = true;
        }

        private void GeneratorBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_generatorBorder == null) return;

            // Only continue dragging if mouse is captured and leaves the border
            if (_generatorBorder.IsMouseCaptured)
            {
                if (FindParentCanvas(_generatorBorder) is Canvas canvas)
                {
                    Point position = e.GetPosition(canvas);
                    DragGenerator(position.Y);
                }
            }
            // If not captured, don't change cursor - let XAML default (Hand) remain

            e.Handled = true;
        }

        private void GeneratorBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_generatorBorder == null) return;

            Debug.WriteLine($"[{Generator.Name}] Mouse up - ending drag");

            _generatorBorder.ReleaseMouseCapture();
            _generatorBorder.Cursor = Cursors.Hand;
            IsDragging = false;

            // NOW notify track to refresh (after drag is complete)
            NotifyTrackChanged();

            e.Handled = true;
        }

        private static Canvas? FindParentCanvas(DependencyObject child)
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                Debug.WriteLine($"Searching for parent: {parent.GetType().Name}");
                if (parent is Canvas canvas)
                    return canvas;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private void DragGenerator(double y)
        {
            // y is already absolute Canvas position, clamp it to track height
            double newPosition = Math.Clamp(y, 0, SizeService.Instance.TrackHeight - SizeService.Instance.TrackHeight / 3);
            Debug.WriteLine($"move generator to position {newPosition}");

            // Update VerticalOffset which is bound to Canvas.Top in XAML
            // This moves the visual element without causing a track refresh
            VerticalOffset = newPosition;

            // Also update the underlying generator position for when drag completes
            Generator.Position = (int)Math.Round(newPosition);

            // DO NOT call NotifyTrackChanged() here - it destroys and recreates the Border!
            // NotifyTrackChanged() is called in MouseLeftButtonUp when drag is complete
        }
        #endregion
        #region Algorithmic Generator Specific Properties and Commands
        // the following is specific to the algorithmic generator as it has one soundfont and one list of presets. The stochastic generator will have multiple soundfonts and preset lists 
        public Algorithmic? AlgorithmicGenerator => UIgenerator as Algorithmic;

        /// <summary>
        /// Gets or sets the name of the sequence when NoteAlgorithm is a Sequencer
        /// </summary>
        public string SequencerName
        {
            get
            {
                if (AlgorithmicGenerator?.NoteAlgorithm is Sequencer sequencer)
                {
                    return sequencer.Name;
                }
                return string.Empty;
            }
            set
            {
                if (AlgorithmicGenerator?.NoteAlgorithm is Sequencer sequencer && sequencer.Name != value)
                {
                    // Fire and forget the async load
                    _ = LoadSequenceAsync(value);
                }
            }
        }

        public ObservableCollection<string> SoundFontFileNames { get => GlobalService.Instance.SoundFontFileNames; set { GlobalService.Instance.SoundFontFileNames = value; OnPropertyChanged(); } }

        private string algorithmicSoundFontFileName = string.Empty;
        public string AlgorithmicSoundFontFileName
        {
            // this 'get' checks to see of the algorithmic generator has a value for the soundfont file name, and if it does, it updates the local variable and the preset list for the dropdown. This is to ensure that if the user changes the soundfont file name in the algorithmic generator dialog, the changes are reflected in real time in the preset dropdown.
            get
            {
                if (AlgorithmicGenerator != null && AlgorithmicGenerator.SoundFontFileName != string.Empty)
                {
                    algorithmicSoundFontFileName = AlgorithmicGenerator.SoundFontFileName;
                    if (AlgorithmicGenerator.SoundFont != null)
                    {
                        AlgorithmicPresetNames = new ObservableCollection<string>(AlgorithmicGenerator.SoundFont.Presets.Select(preset => SoundFontUtilities.BankPresetToName(preset)).OrderBy(name => name));
                    }
                }
                return algorithmicSoundFontFileName;
            }
            set
            {
                SoundFont? SoundFont = SoundFontUtilities.GetSoundFont(value);
                if (SoundFont == null)
                {
                    Messages.Clear(); Messages.Add(new Message { Text = $"Error loading SoundFont: {value}", Error = true });
                    return;
                }
                algorithmicSoundFontFileName = value;
                OnPropertyChanged();

                // update the algorithmic generator with the new soundFont
                if (AlgorithmicGenerator == null)
                {
                    Messages.Clear(); Messages.Add(new Message { Text = $"Error: AlgorithmicGenerator is null.", Error = true });
                    return;
                }
                AlgorithmicGenerator.SoundFont = SoundFont;
                AlgorithmicGenerator.SoundFontFileName = value;
                OnPropertyChanged(nameof(AlgorithmicGenerator));

                // get the presets for this soundFont and sort them in name order for the preset dropdown
                algorithmicPresets = new(SoundFont.Presets);
                algorithmicPresetNames = new ObservableCollection<string>(AlgorithmicPresets.Select(preset => SoundFontUtilities.BankPresetToName(preset)).OrderBy(name => name));
                OnPropertyChanged(nameof(AlgorithmicPresets));
                OnPropertyChanged(nameof(AlgorithmicPresetNames));
            }
        }
        private ObservableCollection<Preset> algorithmicPresets = [];
        public ObservableCollection<Preset> AlgorithmicPresets
        {
            get { return algorithmicPresets; }
            set { algorithmicPresets = value; OnPropertyChanged(); }
        }
        private ObservableCollection<string> algorithmicPresetNames = [];
        public ObservableCollection<string> AlgorithmicPresetNames
        {
            // This get checks to see if the algorithmic generator has a soundfont and if it does, it updates the preset names list for the dropdown. This is to ensure that if the user changes the soundfont file name in the algorithmic generator dialog, the changes are reflected in real time in the preset dropdown.
            // this may never happen since the soundfont file name get/set should update the preset list, but this is just to be safe and ensure that the preset list is always up to date with the soundfont file name.
            get
            {
                if (AlgorithmicGenerator != null && AlgorithmicGenerator.SoundFont != null)
                {
                    algorithmicPresetNames = new ObservableCollection<string>(AlgorithmicGenerator.SoundFont.Presets.Select(preset => SoundFontUtilities.BankPresetToName(preset)).OrderBy(name => name));
                }
                return algorithmicPresetNames;
            }
            set { algorithmicPresetNames = value; OnPropertyChanged(); }
        }
        private string algorithmicPresetName = string.Empty;
        public string AlgorithmicPresetName
        {
            // this get checks to see if the algorithmic generator has a preset and if it does, it updates the local variable for the preset name. This is to ensure that if the user changes the preset in the algorithmic generator dialog, the changes are reflected in real time in the preset name variable which is used to set the dropdown selection.
            get
            {
                if (AlgorithmicGenerator != null && AlgorithmicGenerator.Preset != null)
                {
                    algorithmicPresetName = SoundFontUtilities.BankPresetToName(AlgorithmicGenerator.Preset);
                }
                return algorithmicPresetName;
            }
            set
            {
                algorithmicPresetName = value;
                OnPropertyChanged();
                // update the algorithmic generator with the new preset
                Preset? preset = AlgorithmicPresets.FirstOrDefault(p => SoundFontUtilities.BankPresetToName(p) == value);
                if (preset != null && AlgorithmicGenerator != null)
                {
                    AlgorithmicGenerator.Preset = preset;
                    AlgorithmicGenerator.PresetName = value;
                    OnPropertyChanged(nameof(AlgorithmicGenerator));
                }
            }
        }
        private RelayCommand<Algorithmic>? _noiseSeedCommand;
        public RelayCommand<Algorithmic> NoiseSeedCommand =>
            _noiseSeedCommand ??= new RelayCommand<Algorithmic>(generator =>
            {
                if (generator is Algorithmic g)
                {
                    string newSeed = StringUtils.GenerateRandomString(10);
                    g.NoiseSeed = newSeed;
                    // Notify that the algorithm changed so UI updates
                    OnPropertyChanged(nameof(AlgorithmicGenerator));
                    Messages.Clear(); Messages.Add(new Message { Text = "New Noise seed assigned.", Error = false });
                }
            });

        private RelayCommand<Algorithm>? _markovianSeedCommand;
        public RelayCommand<Algorithm> MarkovianSeedCommand =>
            _markovianSeedCommand ??= new RelayCommand<Algorithm>(algorithm =>
            {
                if (algorithm is Markovian markovian)
                {
                    string newSeed = StringUtils.GenerateRandomString(10);
                    markovian.Seed = newSeed;
                    // Notify that the algorithm changed so UI updates
                    OnPropertyChanged(nameof(AlgorithmicGenerator));
                    Messages.Clear(); Messages.Add(new Message { Text = "New Markovian seed assigned.", Error = false });
                }
            });

        private RelayCommand<Algorithm>? _wienerSeedCommand;
        public RelayCommand<Algorithm> WienerSeedCommand =>
            _wienerSeedCommand ??= new RelayCommand<Algorithm>(algorithm =>
            {
                if (algorithm is Wiener wiener)
                {
                    string newSeed = StringUtils.GenerateRandomString(10);
                    wiener.Seed = newSeed;
                    // Notify that the algorithm changed so UI updates
                    OnPropertyChanged(nameof(AlgorithmicGenerator));
                    Messages.Clear(); Messages.Add(new Message { Text = "New Wiener seed assigned.", Error = false });
                }
            });
        private RelayCommand<Algorithm>? _autoregressiveSeedCommand;
        public RelayCommand<Algorithm> AutoregressiveSeedCommand =>
            _autoregressiveSeedCommand ??= new RelayCommand<Algorithm>(algorithm =>
            {
                if (algorithm is Autoregressive autoregressive)
                {
                    string newSeed = StringUtils.GenerateRandomString(10);
                    autoregressive.Seed = newSeed;
                    // Notify that the algorithm changed so UI updates
                    OnPropertyChanged(nameof(AlgorithmicGenerator));
                    Messages.Clear(); Messages.Add(new Message { Text = "New Autoregressive seed assigned.", Error = false });
                }
            });
        // reload the list of all ensembles from the database and update the dropdown in the stochastic generator dialog. This is to ensure that if the user adds or removes ensembles from the database while the stochastic generator dialog is open, the dropdown for selecting ensembles is updated in real time to reflect those changes.
        private RelayCommand<Algorithmic>? _reloadEnsembles;
        public RelayCommand<Algorithmic> ReloadEnsembles =>
            _reloadEnsembles ??= new RelayCommand<Algorithmic>(generator =>
            {
                if (generator is Algorithmic g)
                {
                    _ = ReloadEnsemblesAsync();
                }
            });

        private async Task ReloadEnsemblesAsync()
        {
            try
            {
                ObservableCollection<Ensemble> ensembles = await EnsembleUtilities.GetEnsembleListAsync();
                ObservableCollection<string> names = [.. ensembles.Select(ensemble => ensemble.Name)];
                GlobalService.Instance.EnsembleNames = names;
            }
            catch (Exception ex)
            {
                Messages.Clear(); Messages.Add(new Message { Text = $"Error reloading ensembles: {ex.Message}", Error = true });
            }
        }


        public static ObservableCollection<MODULATORTYPE> ModulatorTypes => new(Enum.GetValues<MODULATORTYPE>());
        public static ObservableCollection<ALGORITHMTYPE> AlgorithmTypes => new(Enum.GetValues<ALGORITHMTYPE>());

        // Attribute Descriptors for Algorithmic Generator
        private AttributeDescriptor? _noteAttribute;
        public AttributeDescriptor? NoteAttribute
        {
            get
            {
                if (_noteAttribute == null && AlgorithmicGenerator != null)
                {
                    _noteAttribute = new AttributeDescriptor
                    {
                        Name = "Note",
                        Synonym = "(pitch)",
                        ValueUnits = (value) => SoundFontUtilities.MidiToNote(value),
                        AmplitudeUnits = (value) => "pitch",
                        ValueFormat = "F2",
                        AmplitudeFormat = "F2",
                        Minimum = 0,
                        Maximum = 127,
                        Increment = 0.01,
                        Algorithm = AlgorithmicGenerator.NoteAlgorithm,
                    };
                    _noteAttribute.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(AttributeDescriptor.Algorithm) && AlgorithmicGenerator != null)
                        {
                            AlgorithmicGenerator.NoteAlgorithm = _noteAttribute.Algorithm;
                            OnPropertyChanged(nameof(AlgorithmicGenerator));
                            OnPropertyChanged(nameof(SequencerName));
                        }
                    };
                }
                return _noteAttribute;
            }
        }

        private AttributeDescriptor? _attackAttribute;
        public AttributeDescriptor? AttackAttribute
        {
            get
            {
                if (_attackAttribute == null && AlgorithmicGenerator != null)
                {
                    _attackAttribute = new AttributeDescriptor
                    {
                        Name = "Attack",
                        Synonym = "(velocity)",
                        ValueUnits = (value) => "[0-127]",
                        AmplitudeUnits = (value) => "[0-127]",
                        ValueFormat = "F0",
                        AmplitudeFormat = "F0",
                        Minimum = 0,
                        Maximum = 127,
                        Increment = 1,
                        Algorithm = AlgorithmicGenerator.AttackAlgorithm,
                    };
                    _attackAttribute.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(AttributeDescriptor.Algorithm) && AlgorithmicGenerator != null)
                        {
                            AlgorithmicGenerator.AttackAlgorithm = _attackAttribute.Algorithm;
                            OnPropertyChanged(nameof(AlgorithmicGenerator));
                        }
                    };
                }
                return _attackAttribute;
            }
        }

        private AttributeDescriptor? _speedAttribute;
        public AttributeDescriptor? SpeedAttribute
        {
            get
            {
                if (_speedAttribute == null && AlgorithmicGenerator != null)
                {
                    _speedAttribute = new AttributeDescriptor
                    {
                        Name = "Speed",
                        Synonym = "(tempo)",
                        ValueUnits = (value) => "BPM",
                        AmplitudeUnits = (value) => "BPM",
                        ValueFormat = "F0",
                        AmplitudeFormat = "F0",
                        Minimum = 1,
                        Maximum = 10000,
                        Increment = 1,
                        Algorithm = AlgorithmicGenerator.SpeedAlgorithm,
                    };
                    _speedAttribute.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(AttributeDescriptor.Algorithm) && AlgorithmicGenerator != null)
                        {
                            AlgorithmicGenerator.SpeedAlgorithm = _speedAttribute.Algorithm;
                            OnPropertyChanged(nameof(AlgorithmicGenerator));
                        }
                    };
                }
                return _speedAttribute;
            }
        }

        private AttributeDescriptor? _durationAttribute;
        public AttributeDescriptor? DurationAttribute
        {
            get
            {
                if (_durationAttribute == null && AlgorithmicGenerator != null)
                {
                    _durationAttribute = new AttributeDescriptor
                    {
                        Name = "Duration",
                        Synonym = "(note value)",
                        ValueUnits = (value) => "%",
                        AmplitudeUnits = (value) => "%",
                        ValueFormat = "F0",
                        AmplitudeFormat = "F0",
                        Minimum = 1,
                        Maximum = 100,
                        Increment = 1,
                        Algorithm = AlgorithmicGenerator.DurationAlgorithm,
                    };
                    _durationAttribute.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(AttributeDescriptor.Algorithm) && AlgorithmicGenerator != null)
                        {
                            AlgorithmicGenerator.DurationAlgorithm = _durationAttribute.Algorithm;
                            OnPropertyChanged(nameof(AlgorithmicGenerator));
                        }
                    };
                }
                return _durationAttribute;
            }
        }

        private AttributeDescriptor? _volumeAttribute;
        public AttributeDescriptor? VolumeAttribute
        {
            get
            {
                if (_volumeAttribute == null && AlgorithmicGenerator != null)
                {
                    _volumeAttribute = new AttributeDescriptor
                    {
                        Synonym = "(intensity)",
                        Name = "Volume",
                        ValueUnits = (value) => "dB",
                        AmplitudeUnits = (value) => "dB",
                        ValueFormat = "F0",
                        AmplitudeFormat = "F0",
                        Minimum = -10,
                        Maximum = 10,
                        Increment = 1,
                        Algorithm = AlgorithmicGenerator.VolumeAlgorithm,
                    };
                    _volumeAttribute.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(AttributeDescriptor.Algorithm) && AlgorithmicGenerator != null)
                        {
                            AlgorithmicGenerator.VolumeAlgorithm = _volumeAttribute.Algorithm;
                            OnPropertyChanged(nameof(AlgorithmicGenerator));
                        }
                    };
                }
                return _volumeAttribute;
            }
        }

        private AttributeDescriptor? _panAttribute;
        public AttributeDescriptor? PanAttribute
        {
            get
            {
                if (_panAttribute == null && AlgorithmicGenerator != null)
                {
                    _panAttribute = new AttributeDescriptor
                    {
                        Name = "Pan",
                        Synonym = "(channel)",
                        ValueUnits = (value) => "[-1,+1]",
                        AmplitudeUnits = (value) => "[-1,+1]",
                        ValueFormat = "F1",
                        AmplitudeFormat = "F1",
                        Minimum = -1,
                        Maximum = 1,
                        Increment = 0.1,
                        Algorithm = AlgorithmicGenerator.PanAlgorithm,
                    };
                    _panAttribute.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(AttributeDescriptor.Algorithm) && AlgorithmicGenerator != null)
                        {
                            AlgorithmicGenerator.PanAlgorithm = _panAttribute.Algorithm;
                            OnPropertyChanged(nameof(AlgorithmicGenerator));
                        }
                    };
                }
                return _panAttribute;
            }
        }
        #endregion
        #region Stochastic Generator Specific Properties and Commands
        public ObservableCollection<string> EnsembleNames { get => GlobalService.Instance.EnsembleNames; set { GlobalService.Instance.EnsembleNames = value; OnPropertyChanged(); } }
        public ObservableCollection<string> NoteSequenceNames { get => GlobalService.Instance.NoteSequenceNames; set { GlobalService.Instance.NoteSequenceNames = value; OnPropertyChanged(); } }
        //private string _newSequenceName = "";
        //public string NewSequenceName 
        //{ 
        //    get => _newSequenceName; 
        //    set 
        //    {
        //        if (AlgorithmicGenerator == null || AlgorithmicGenerator.NoteAlgorithm == null) return;
        //        if (_newSequenceName == value) return; // Avoid redundant calls

        //        _newSequenceName = value;
        //        OnPropertyChanged();

        //        // Fire and forget the async load - use discard to indicate intentional non-await
        //        _ = LoadSequenceAsync(value);
        //    }
        //}

        private async Task LoadSequenceAsync(string sequenceName)
        {
            if (AlgorithmicGenerator?.NoteAlgorithm is not Sequencer sequencer) return;

            try
            {
                // Set the name first
                sequencer.Name = sequenceName;

                // Load the sequence from the database using NoteSequenceUtilities
                var sequence = await NoteSequenceUtilities.GetNoteSequenceAsync(sequenceName);
                if (sequence != null)
                {
                    sequencer.Items = sequence.Items;

                    // Notify that the sequencer has been updated
                    OnPropertyChanged(nameof(AlgorithmicGenerator));
                    OnPropertyChanged(nameof(SequencerName));
                    Messages.Clear(); 
                    Messages.Add(new Message { Text = $"Sequence '{sequenceName}' loaded with {sequence.Items.Count} items.", Error = false });
                }
                else
                {
                    Messages.Clear(); 
                    Messages.Add(new Message { Text = $"Sequence '{sequenceName}' not found.", Error = true });
                }
            }
            catch (Exception ex)
            {
                Messages.Clear(); 
                Messages.Add(new Message { Text = $"Error loading sequence: {ex.Message}", Error = true });
            }
        }

        public Stochastic? StochasticGenerator => UIgenerator as Stochastic;
        private string ensembleName = string.Empty;
        public string StochasticEnsembleName
        {
            get
            {
                if (StochasticGenerator != null && StochasticGenerator.Ensemble.Name != ensembleName)
                {
                    ensembleName = StochasticGenerator.Ensemble.Name;
                }
                return ensembleName;
            }
            // when the ensemble name changes reload it along with the voices
            set
            {
                if (value != ensembleName && StochasticGenerator != null)
                {
                    ensembleName = value;
                    OnPropertyChanged();
                    _ = LoadEnsembleAsync(value);
                }
            }
        }

        private async Task LoadEnsembleAsync(string name)
        {
            try
            {
                var (ensemble, voices) = await EnsembleUtilities.GetEnsembleAsync(name);
                if (StochasticGenerator != null)
                {
                    // Unsubscribe from old voices
                    UnsubscribeFromVoices();

                    StochasticGenerator.Ensemble = ensemble;
                    StochasticGenerator.Voices = [.. voices];
                    StochasticGenerator.Composition = [];

                    // Subscribe to new voices
                    SubscribeToVoices();

                    OnPropertyChanged(nameof(StochasticGenerator));
                    OnPropertyChanged(nameof(stochasticComposition));
                    Messages.Clear(); Messages.Add(new Message { Text = $"Ensemble {ensemble.Name} read with {voices.Count} voices.", Error = false });
                }
            }
            catch (Exception ex)
            {
                Messages.Clear(); Messages.Add(new Message { Text = $"Error loading ensemble: {ex.Message}", Error = true });
            }
        }


        private void SubscribeToVoices()
        {
            if (StochasticGenerator?.Voices != null)
            {
                foreach (var voice in StochasticGenerator.Voices)
                {
                    voice.PropertyChanged += Voice_PropertyChanged;
                }
            }
        }

        private void UnsubscribeFromVoices()
        {
            if (StochasticGenerator?.Voices != null)
            {
                foreach (var voice in StochasticGenerator.Voices)
                {
                    voice.PropertyChanged -= Voice_PropertyChanged;
                }
            }
        }

        private void Voice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When a voice's Muted property changes, refresh the composition display
            if (e.PropertyName == nameof(Voice.Muted))
            {
                OnPropertyChanged(nameof(StochasticComposition));
            }
        }
        private RelayCommand<Stochastic>? _compositionSeedCommand;
        public RelayCommand<Stochastic> CompositionSeedCommand =>
            _compositionSeedCommand ??= new RelayCommand<Stochastic>(generator =>
            {
                if (generator is Stochastic g)
                {
                    string newSeed = StringUtils.GenerateRandomString(10);
                    g.CompositionSeed = newSeed;
                    OnPropertyChanged(nameof(StochasticGenerator));
                    Messages.Clear(); Messages.Add(new Message { Text = "New composition seed assigned.", Error = false });
                }
            });
        private RelayCommand<Stochastic>? _dynamicsSeedCommand;
        public RelayCommand<Stochastic> DynamicsSeedCommand =>
            _dynamicsSeedCommand ??= new RelayCommand<Stochastic>(generator =>
            {
                if (generator is Stochastic g)
                {
                    string newSeed = StringUtils.GenerateRandomString(10);
                    g.DynamicsSeed = newSeed;
                    OnPropertyChanged(nameof(StochasticGenerator));
                    Messages.Clear(); Messages.Add(new Message { Text = "New dynamics seed assigned.", Error = false });
                }
            });
        private RelayCommand<object>? _reloadSequencesCommand;
        public RelayCommand<object> ReloadSequencesCommand =>
            _reloadSequencesCommand ??= new RelayCommand<object>(generator =>
            {
                _ = ReloadSequencesAsync();
            });
        private async Task ReloadSequencesAsync()
        {
            try
            {
                ObservableCollection<string> names = await NoteSequenceUtilities.GetNoteSequenceNamesAsync();
                GlobalService.Instance.NoteSequenceNames = names;
            }
            catch (Exception ex)
            {
                Messages.Clear(); Messages.Add(new Message { Text = $"Error reloading note sequences: {ex.Message}", Error = true });
            }
        }
        private RelayCommand<Sequencer>? _viewSequenceCommand;
        public RelayCommand<Sequencer> ViewSequenceCommand =>
            _viewSequenceCommand ??= new RelayCommand<Sequencer>(sequence =>
            {
                Window dialog = new ViewSequence()
                {
                    DataContext = sequence,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    SizeToContent = SizeToContent.WidthAndHeight,
                };
                //FileViewModel.Instance.ActiveDialog = activeDialog;
                dialog.ShowDialog();
            });
        public static ObservableCollection<INTENSITYOPTION> IntensityOptionTypes => new(Enum.GetValues<INTENSITYOPTION>());
        public static ObservableCollection<INTENSITYTRANSITIONOPTION> IntensityTransitionOptionTypes => new(Enum.GetValues<INTENSITYTRANSITIONOPTION>());
        public static ObservableCollection<PANOPTION> PanOptionTypes => new(Enum.GetValues<PANOPTION>());
        public static ObservableCollection<PANALGORITHM> PanAlgorithmTypes => new(Enum.GetValues<PANALGORITHM>());
        public static ObservableCollection<REVERBOPTION> ReverbOptionTypes => new(Enum.GetValues<REVERBOPTION>());
        public record struct StochasticCompositionRow
        {
            public StochasticCompositionRow() { }
            public double Time { get; set; } = 0;
            public ObservableCollection<int> Values { get; set; } = [];
            public int Sum { get; set; } = 0;
        }
        private ObservableCollection<StochasticCompositionRow> stochasticComposition = [];
        public ObservableCollection<StochasticCompositionRow> StochasticComposition
        {
            // This property is used to display the stochastic composition in the stochastic generator dialog. It converts the 2D array of values in the stochastic generator composition into a collection of rows, where each row represents a time step and contains the values for each unmuted voice at that time step, as well as the sum of those values for that time step. This allows for easy data binding to a grid in the UI to display the composition in a readable format.
            get
            {
                if (StochasticGenerator == null) return [];

                // now construct the display portion of the composition from the active noise list
                stochasticComposition = [];
                double deltaTime = StochasticGenerator.GetDeltaT();
                for (int i = 0; i < StochasticGenerator.Composition.Length; i++)
                {
                    ObservableCollection<int> values = [];
                    int sum = 0;
                    for (int j = 0; j < StochasticGenerator.Composition[i].Length; j++)
                    {
                        if (StochasticGenerator.Voices[j].Muted) continue;
                        values.Add(StochasticGenerator.Composition[i][j]);
                        sum += StochasticGenerator.Composition[i][j];
                    }
                    stochasticComposition.Add(new StochasticCompositionRow
                    {
                        Time = (i * deltaTime),
                        Values = values,
                        Sum = sum
                    });
                }
                return stochasticComposition;
            }
            set
            {
                stochasticComposition = value;
                OnPropertyChanged();
            }
        }
        // reload the voices for the currently selected ensemble from the database and update the stochastic generator with the new voices. This is to ensure that if the user changes the voices in an ensemble in the database while the stochastic generator dialog is open, the stochastic generator is updated in real time with the new voices for the currently selected ensemble. If a signaificant change has been made to the voices (like the number of them, then invalidate the composition since it may no longer be compatible with the new voices.
        private RelayCommand<Stochastic>? _reloadVoices;
        public RelayCommand<Stochastic> ReloadVoices =>
            _reloadVoices ??= new RelayCommand<Stochastic>(generator =>
            {
                if (generator is Stochastic g)
                {
                    if (g.Ensemble.Name == string.Empty)
                    {
                        Messages.Clear(); Messages.Add(new Message { Text = "Error: No ensemble selected.", Error = true });
                        return;
                    }
                    _ = ReloadVoicesAsync();
                }
            });

        private async Task ReloadVoicesAsync()
        {
            try
            {
                if (StochasticGenerator == null) return;
                Stochastic g = StochasticGenerator;

                // Unsubscribe from old voices
                UnsubscribeFromVoices();

                ObservableCollection<Voice> voices = [.. await EnsembleUtilities.GetEnsembleVoicesAsync(StochasticGenerator.Ensemble)];
                if (g.Voices.Count != voices.Count)
                {
                    // if the number of voices has changed, invalidate the composition since it may no longer be compatible with the new voices.
                    g.Composition = [];
                    stochasticComposition = [];
                    OnPropertyChanged(nameof(StochasticComposition));
                    Messages.Clear(); Messages.Add(new Message { Text = "Composition has been invalidated due to the change in the number of voices in the ensemble.", Error = false });
                }
                StochasticGenerator.Voices = voices;

                // Subscribe to new voices
                SubscribeToVoices();

                OnPropertyChanged(nameof(StochasticGenerator));
            }
            catch (Exception ex)
            {
                Messages.Clear(); Messages.Add(new Message { Text = $"Error reloading voices: {ex.Message}", Error = true });
            }
        }

        // this is the business end of the stocahstic generator construction of the composition. When the user clicks the button to build the composition in the stochastic generator dialog, this command is executed to build the composition based on the currently selected ensemble and voices, and the composition parameters of the stochastic generator.
        private RelayCommand<Stochastic>? _buildComposition;
        public RelayCommand<Stochastic> BuildComposition =>
            _buildComposition ??= new RelayCommand<Stochastic>(generator =>
            {
                if (generator == null) return;
                if (generator is Stochastic g)
                {
                    // confirm that everything needed to create a composition is provided
                    messages.Clear();
                    if (g.Ensemble.Name == string.Empty)
                    {
                        messages.Add(new Message { Text = "Error: No ensemble selected.", Error = true });
                    }
                    if (g.Voices == null || g.Voices.Count == 0)
                    {
                        messages.Add(new Message { Text = "Error: No voices in ensemble.", Error = true });
                    }
                    if (g.CompositionDuration <= 0)
                    {
                        messages.Add(new Message { Text = "Error: The composition length must be positive.", Error = true });
                    }
                    if (g.NumberOfTimeCells <= 0)
                    {
                        messages.Add(new Message { Text = "Error: The number of time cells must be positive.", Error = true });
                    }
                    if (g.Lambda <= 0)
                    {
                        messages.Add(new Message { Text = "Error: The number of events/row must be positive.", Error = true });
                    }
                    OnPropertyChanged(nameof(Messages));
                    if (messages.Count > 0) return;

                    Composition composition = StochasticUtilities.BuildComposition(g);
                    g.Composition = composition;
                    OnPropertyChanged(nameof(StochasticGenerator));

                    // build the observable list of the composition for the stochastic panel
                    double deltaT = g.GetDeltaT();

                    stochasticComposition = []; // setting the private field of the StochasticComposition property
                    for (int i = 0; i < composition.Length; i++)
                    {
                        int sum = 0;
                        ObservableCollection<int> values = [];
                        for (int j = 0; j < composition[i].Length; j++)
                        {
                            sum += composition[i][j];
                            values.Add(composition[i][j]);
                        }
                        StochasticCompositionRow row = new StochasticCompositionRow()
                        {
                            Time = i * deltaT,
                            Sum = sum,
                            Values = values,

                        };
                        stochasticComposition.Add(row);
                    }
                    OnPropertyChanged(nameof(StochasticComposition));
                }
            });
        #endregion
        #region Nested Classes
        /// <summary>
        /// Descriptor that wraps an Algorithm with its metadata (name, units) for data binding
        /// </summary>
        public class AttributeDescriptor : INotifyPropertyChanged
        {
            public required string Name { get; set; } = string.Empty;
            public required Func<double, string> ValueUnits { get; set; } = (value) => value.ToString();
            public required Func<double, string> AmplitudeUnits { get; set; } = (value) => value.ToString();
            public required string Synonym { get; set; } = string.Empty;
            public required double? Minimum { get; set; } = 0;
            public required double? Maximum { get; set; } = 0;
            public required double? Increment { get; set; } = 0;
            public string ValueFormat { get; set; } = "F3";
            public string AmplitudeFormat { get; set; } = "F3";

            /// <summary>
            /// Gets the list of algorithm types available for this attribute.
            /// Sequence is only available for the Note attribute.
            /// </summary>
            public ObservableCollection<ALGORITHMTYPE> AvailableAlgorithmTypes
            {
                get
                {
                    // Sequence algorithm only makes sense for Note (pitch) attribute
                    if (Name == "Note")
                    {
                        return AlgorithmTypes; // All algorithm types including Sequencer
                    }
                    else
                    {
                        // For other attributes, exclude Sequence
                        return new ObservableCollection<ALGORITHMTYPE>(
                            AlgorithmTypes.Where(t => t != ALGORITHMTYPE.Sequencer));
                    }
                }
            }

            private Algorithm _algorithm = new Constant();
            public required Algorithm Algorithm
            {
                get => _algorithm;
                set
                {
                    if (_algorithm != value)
                    {
                        _algorithm = value;
                        OnPropertyChanged();
                        // Update the selected type to match the algorithm
                        _selectedType = GetAlgorithmType(_algorithm);
                    }
                }
            }

            private ALGORITHMTYPE _selectedType = ALGORITHMTYPE.Constant;
            public ALGORITHMTYPE SelectedType
            {
                get => _selectedType;
                set
                {
                    _selectedType = value;
                    // Create new algorithm instance based on type
                    Algorithm = AlgorithmFactory.CreateAlgorithm(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Algorithm));
                }
            }

            private static ALGORITHMTYPE GetAlgorithmType(Algorithm algorithm)
            {
                return algorithm switch
                {
                    Constant => ALGORITHMTYPE.Constant,
                    Oscillator => ALGORITHMTYPE.Oscillator,
                    Wiener => ALGORITHMTYPE.Wiener,
                    Markovian => ALGORITHMTYPE.Markovian,
                    Autoregressive => ALGORITHMTYPE.Autoregressive,
                    Sequencer => ALGORITHMTYPE.Sequencer,
                    _ => ALGORITHMTYPE.Constant
                };
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
