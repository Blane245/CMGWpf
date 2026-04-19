# CMGWpf Programmer's Guide

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Project Structure](#project-structure)
3. [Design Patterns](#design-patterns)
4. [Core Components](#core-components)
5. [Data Models](#data-models)
6. [MVVM Implementation](#mvvm-implementation)
7. [Services](#services)
8. [DSP and Audio](#dsp-and-audio)
9. [File I/O](#file-io)
10. [UI Components](#ui-components)
11. [Testing](#testing)
12. [Build and Deployment](#build-and-deployment)
13. [Contributing Guidelines](#contributing-guidelines)
14. [API Reference](#api-reference)

---

## Architecture Overview

### Technology Stack

- **.NET 10.0**: Target framework
- **WPF (Windows Presentation Foundation)**: UI framework
- **MVVM (Model-View-ViewModel)**: Architectural pattern
- **NAudio**: Audio I/O and processing
- **FFMpegCore**: Video encoding
- **Extended.Wpf.Toolkit**: Enhanced controls
- **Material.Icons.WPF**: Icon library

### High-Level Architecture

```
┌─────────────────────────────────────────────┐
│                    View                     │
│  (XAML + Code-behind for UI events)         │
└──────────────────┬──────────────────────────┘
                   │ Data Binding
                   │ Commands
┌──────────────────▼──────────────────────────┐
│                ViewModel                    │
│  (View/, Commands, Presentation Logic)      │
└──────────────────┬──────────────────────────┘
                   │ Business Logic
                   │ Calls
┌──────────────────▼──────────────────────────┐
│              Model / MVVM                   │
│  (Track, Generator, Composition, etc.)      │
└──────────────────┬──────────────────────────┘
                   │
    ┌──────────────┼──────────────┐
    │              │              │
┌───▼────┐   ┌────▼─────┐   ┌───▼────┐
│Services│   │   DSP    │   │  I/O   │
│        │   │ (Audio)  │   │ (File) │
└────────┘   └──────────┘   └────────┘
```

### Key Design Principles

1. **Separation of Concerns**: Clear boundaries between UI, business logic, and data
2. **MVVM Pattern**: Testable, maintainable UI code
3. **Service-Oriented**: Reusable services for cross-cutting concerns
4. **Non-Modal Architecture**: Multiple concurrent editing sessions
5. **File Locking**: Prevent concurrent access to same file across instances
6. **Event-Driven**: Loose coupling through events and commands

---

## Project Structure

### Directory Organization

```
src/CMGWpf/
├── Assets/                    # Images, icons, resources
├── Converters/               # Value converters for XAML binding
├── Dialogs/                  # Dialog windows and user controls
│   ├── GeneratorDialog.xaml  # Generator editor
│   ├── TrackDialog.xaml      # Track management
│   └── ToolsDialogs/         # Align, stagger, etc.
├── Layout/                   # Main layout components
│   ├── Menu.xaml             # Application menu
│   └── CustomChrome.xaml     # Window chrome UserControl
├── Model/                    # Business logic layer
│   ├── Generators/           # Generator implementations
│   │   ├── Generator.cs      # Abstract base class
│   │   ├── Algorithmic.cs    # Algorithmic generator
│   │   └── Stochastic.cs     # Stochastic generator
│   ├── Algorithm.cs          # Algorithm base class
│   └── Track.cs              # Track model
├── MVVM/                     # MVVM infrastructure
│   ├── Composition.cs        # Main composition model
│   ├── Track.cs              # Track ViewModel wrapper
│   ├── Generator.cs          # Generator ViewModel wrapper
│   ├── FileCommands.cs       # File menu commands
│   ├── PlayCommands.cs       # Playback commands
│   └── ToolsCommands.cs      # Tools commands
├── PlayFunctions/            # Audio/Video subsystem
│   ├── PlayDialog.xaml       # Playback UI
│   ├── PlayEngine.cs         # Core playback logic
│   ├── DSP/                  # Digital Signal Processing
│   │   ├── InstrumentSample.cs
│   │   ├── SourcesFromAlgorithmic.cs
│   │   └── SourcesFromStochastic.cs
│   └── Utilities/
│       ├── ReadyPlay.cs      # Playback preparation
│       ├── ReportWriter.cs   # Report generation
│       └── VideoRecorder.cs  # MP4 generation
├── Services/                 # Application services
│   ├── FileLockService.cs    # Multi-instance file locking
│   ├── SizeService.cs        # Window size management
│   └── MessageService.cs     # UI messaging
├── SoundFont_2/              # SoundFont parsing
│   ├── SoundFont.cs          # SF2 file parser
│   └── Preset.cs             # Instrument preset
├── Styles/                   # XAML styling
│   ├── ApplicationStyles.xaml
│   └── PlayDialogStyles.xaml
├── Types/                    # Custom types
│   ├── Tremolo.cs
│   └── FastRandom.cs
├── Utilities/                # Helper classes
│   ├── CMGDB.cs              # File I/O
│   ├── MathUtilities.cs
│   ├── EuclideanRhythm.cs
│   └── StochasticUtilities.cs
└── View/                     # ViewModels
    ├── FileViewModel.cs      # Main application ViewModel
    ├── TracksViewModel.cs    # Track management
    ├── TimeLineViewModel.cs  # Timeline display
    ├── GeneratorViewModel.cs # Generator editing
    └── ToolsViewModel.cs     # Tools operations
```

### Project Files

**CMGWpf.csproj**: Main project file
- Target: .NET 10.0 Windows
- NuGet packages: NAudio, NAudio.Lame, FFMpegCore, Extended.Wpf.Toolkit, Material.Icons.WPF

**App.xaml/App.xaml.cs**: Application entry point
- Global exception handling
- Resource dictionaries
- File lock cleanup on exit

**MainWindow.xaml/MainWindow.xaml.cs**: Main application window
- Window state persistence
- Taskbar-aware maximize
- Data context setup

---

## Design Patterns

### MVVM (Model-View-ViewModel)

**Model**: Business logic and data structures
- `Model/Generators/Generator.cs`
- `Model/Track.cs`
- `Model/Algorithm.cs`

**ViewModel**: Presentation logic and commands
- `View/FileViewModel.cs`
- `View/TracksViewModel.cs`
- `MVVM/` command classes

**View**: XAML UI definitions
- `MainWindow.xaml`
- `Dialogs/*.xaml`

### Command Pattern

All user actions implemented as ICommand:

```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
```

**Usage**:
```csharp
public ICommand SaveCommand { get; }

public FileViewModel()
{
    SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
}
```

### Singleton Pattern

Used for application services:

```csharp
public class FileLockService
{
    private static FileLockService? _instance;
    private static readonly object _lock = new object();

    public static FileLockService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new FileLockService();
                }
            }
            return _instance;
        }
    }

    private FileLockService() { }
}
```

### Factory Pattern

Used for creating generators:

```csharp
public static Generator CreateGenerator(string type, int uid, Track parent)
{
    return type.ToLower() switch
    {
        "algorithmic" => new Algorithmic(uid, parent),
        "stochastic" => new Stochastic(uid, parent),
        _ => throw new ArgumentException($"Unknown generator type: {type}")
    };
}
```

### Observer Pattern

Implemented via `INotifyPropertyChanged`:

```csharp
public class Generator : INotifyPropertyChanged
{
    private string _name = "";
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

---

## Core Components

### Composition

**File**: `MVVM/Composition.cs`

Main data structure containing all tracks and generators.

```csharp
public class Composition
{
    public string FileName { get; set; } = "";
    public ObservableCollection<Track> Tracks { get; set; } = new();
    public string Comments { get; set; } = "";
    public DateTime LastModified { get; set; }
    
    // Preferences/settings
    public int SampleRate { get; set; } = 44100;
    public double ReverbDelay { get; set; } = 0.0;
    public int ReverbDecay { get; set; } = 1;
}
```

### Track

**Files**: 
- `Model/Track.cs` (Model)
- `MVVM/Track.cs` (ViewModel wrapper)

Container for generators with track-level settings.

```csharp
public class Track : INotifyPropertyChanged
{
    public string Name { get; set; } = "";
    public double Volume { get; set; } = 0.0; // dB
    public ObservableCollection<Generator> Generators { get; set; } = new();
    
    public void AddGenerator(Generator generator) { ... }
    public void RemoveGenerator(Generator generator) { ... }
    public void ShiftGenerators(double seconds) { ... }
}
```

### Generator (Abstract Base)

**File**: `Model/Generators/Generator.cs`

Base class for all generator types.

```csharp
public abstract class Generator : INotifyPropertyChanged
{
    public int UID { get; set; }
    public Track Parent { get; set; }
    public string Name { get; set; } = "";
    public double StartTime { get; set; } // seconds
    public double StopTime { get; set; }  // seconds
    public double Duration => StopTime - StartTime;
    
    public abstract bool Validate(out List<string> errors);
    public abstract bool Equals(Generator other);
    public abstract Generator Clone();
}
```

### Algorithmic Generator

**File**: `Model/Generators/Algorithmic.cs`

Deterministic music generation using algorithms.

```csharp
public class Algorithmic : Generator
{
    // Soundfont
    public SoundFont? SoundFont { get; set; }
    public string PresetName { get; set; } = "";
    public Preset? Preset { get; set; }
    
    // Algorithms
    public Algorithm NoteAlgorithm { get; set; } = new Constant(60);
    public Algorithm SpeedAlgorithm { get; set; } = new Constant(60);
    public Algorithm DurationAlgorithm { get; set; } = new Constant(100);
    public Algorithm AttackAlgorithm { get; set; } = new Constant(63);
    public Algorithm VolumeAlgorithm { get; set; } = new Constant(0);
    public Algorithm PanAlgorithm { get; set; } = new Constant(0);
    
    // Settings
    public bool Microtones { get; set; } = true;
    public bool IsLooping { get; set; } = true;
    public int MeasureLength { get; set; } = 4;
    public int BeatCount { get; set; } = 4;
    public int NoteShift { get; set; } = 0;
    
    // Effects
    public Tremolo Tremolo { get; set; } = new();
    public Tremolo Vibrato { get; set; } = new();
    public double NoiseFrequency { get; set; } = 0;
    public double NoiseAmplitude { get; set; } = 0;
}
```

### Stochastic Generator

**File**: `Model/Generators/Stochastic.cs`

Probability-based music generation using clouds.

```csharp
public class Stochastic : Generator
{
    public int TimeCells { get; set; }
    public int Voices { get; set; }
    public double[,] Composition { get; set; } // [timeCells][voices]
    
    // Per-voice settings
    public List<VoiceSettings> VoiceSettings { get; set; } = new();
    
    // Cloud definitions
    public List<Cloud> Clouds { get; set; } = new();
}

public class Cloud
{
    public string Name { get; set; } = "";
    public int Grains { get; set; } // number of notes
    public double StartTime { get; set; }
    public double StopTime { get; set; }
    
    // Pitch distribution
    public int PitchLow { get; set; }
    public int PitchHigh { get; set; }
    public string PitchDistribution { get; set; } = "Uniform";
    
    // Duration distribution
    public double DurationMin { get; set; }
    public double DurationMax { get; set; }
    
    // Volume envelope
    public double Attack { get; set; }
    public double Sustain { get; set; }
    public double Release { get; set; }
}
```

### Algorithm

**File**: `Model/Algorithm.cs`

Base class for parameter algorithms.

```csharp
public abstract class Algorithm
{
    public abstract double GetValue(double time, int index);
    public abstract string GetDescription();
}

public class Constant : Algorithm
{
    public double Value { get; set; }
    
    public override double GetValue(double time, int index) => Value;
}

public class Linear : Algorithm
{
    public double StartValue { get; set; }
    public double EndValue { get; set; }
    public double Duration { get; set; }
    
    public override double GetValue(double time, int index)
    {
        if (time >= Duration) return EndValue;
        return StartValue + (EndValue - StartValue) * (time / Duration);
    }
}

// Additional: Exponential, Random, Sequence, EuclideanRhythm, etc.
```

---

## MVVM Implementation

### ViewModels

#### FileViewModel

**File**: `View/FileViewModel.cs`

Main application ViewModel, manages file operations and composition state.

```csharp
public class FileViewModel : INotifyPropertyChanged
{
    private Composition _composition = new();
    public Composition Composition
    {
        get => _composition;
        set { _composition = value; OnPropertyChanged(nameof(Composition)); }
    }

    private bool _isDirty = false;
    public bool IsDirty
    {
        get => _isDirty;
        set { _isDirty = value; OnPropertyChanged(nameof(IsDirty)); }
    }

    public ICommand NewCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }

    public FileViewModel()
    {
        NewCommand = new RelayCommand(_ => NewComposition(), _ => CanNewComposition());
        OpenCommand = new RelayCommand(_ => OpenComposition(), _ => CanOpenComposition());
        SaveCommand = new RelayCommand(_ => SaveComposition(), _ => CanSaveComposition());
        SaveAsCommand = new RelayCommand(_ => SaveAsComposition(), _ => CanSaveAsComposition());
    }

    private void NewComposition() { /* Implementation */ }
    private void OpenComposition() { /* Implementation */ }
    private void SaveComposition() { /* Implementation */ }
    private void SaveAsComposition() { /* Implementation */ }
}
```

#### TracksViewModel

**File**: `View/TracksViewModel.cs`

Manages track operations.

```csharp
public class TracksViewModel
{
    public ObservableCollection<Track> Tracks => Composition.Tracks;
    
    public ICommand AddTrackCommand { get; }
    public ICommand DeleteTrackCommand { get; }
    public ICommand RenameTrackCommand { get; }
    public ICommand ShiftTrackCommand { get; }
    public ICommand TrackVolumeCommand { get; }

    public void AddTrack(string name) { /* Implementation */ }
    public void DeleteTrack(Track track) { /* Implementation */ }
    public void RenameTrack(Track track, string newName) { /* Implementation */ }
}
```

#### GeneratorViewModel

**File**: `View/GeneratorViewModel.cs`

Manages generator editing.

```csharp
public class GeneratorViewModel : INotifyPropertyChanged
{
    private Generator _originalGenerator;
    private Generator _uiGenerator; // Working copy
    
    public Generator UIGenerator
    {
        get => _uiGenerator;
        set { _uiGenerator = value; OnPropertyChanged(nameof(UIGenerator)); }
    }

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand PlayCommand { get; }

    public GeneratorViewModel(Generator generator)
    {
        _originalGenerator = generator;
        _uiGenerator = generator.Clone();
        
        SubmitCommand = new RelayCommand(_ => Submit(), _ => CanSubmit());
        CancelCommand = new RelayCommand(_ => Cancel());
        PlayCommand = new RelayCommand(_ => Play(), _ => CanPlay());
    }

    private void Submit()
    {
        // Copy UIGenerator back to original
        _originalGenerator.CopyFrom(_uiGenerator);
        CloseDialog();
    }

    private void Cancel()
    {
        // Discard changes
        CloseDialog();
    }

    private bool CanSubmit()
    {
        return _uiGenerator.Validate(out _);
    }
}
```

### Commands

#### RelayCommand

**File**: `MVVM/RelayCommand.cs`

```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    
    public void Execute(object? parameter) => _execute(parameter);
}
```

#### Command Organization

**FileCommands.cs**: New, Open, Save, SaveAs, Recent  
**PlayCommands.cs**: Play, Stop, Record (Audio/Video), Report  
**ToolsCommands.cs**: Align, Stagger, Equal, Calculators  
**TrackCommands.cs**: AddTrack, DeleteTrack, Rename, Shift, Volume  
**GeneratorCommands.cs**: Add, Edit, Delete, Move, Copy

---

## Services

### FileLockService

**File**: `Services/FileLockService.cs`

Prevents multiple instances from opening the same file.

```csharp
public class FileLockService
{
    public static FileLockService Instance { get; } = new();
    
    private string? _currentLockFile;
    private const int STALE_LOCK_HOURS = 1;

    public bool TryAcquireLock(string filePath)
    {
        string lockFile = filePath + ".lock";
        
        // Check for existing lock
        if (File.Exists(lockFile))
        {
            if (IsLockStale(lockFile))
            {
                // Remove stale lock
                File.Delete(lockFile);
            }
            else
            {
                return false; // File is locked
            }
        }

        // Create lock file
        WriteLockFile(lockFile);
        _currentLockFile = lockFile;
        return true;
    }

    public void ReleaseLock()
    {
        if (_currentLockFile != null && File.Exists(_currentLockFile))
        {
            File.Delete(_currentLockFile);
            _currentLockFile = null;
        }
    }

    private bool IsLockStale(string lockFile)
    {
        var lockTime = File.GetCreationTime(lockFile);
        return (DateTime.Now - lockTime).TotalHours > STALE_LOCK_HOURS;
    }

    private void WriteLockFile(string lockFile)
    {
        var lockData = new
        {
            ProcessId = Environment.ProcessId,
            Timestamp = DateTime.Now,
            MachineName = Environment.MachineName,
            UserName = Environment.UserName
        };

        File.WriteAllText(lockFile, System.Text.Json.JsonSerializer.Serialize(lockData));
    }
}
```

### SizeService

**File**: `Services/SizeService.cs`

Manages window sizes for responsive layouts.

```csharp
public class SizeService
{
    public static SizeService Instance { get; } = new();
    
    public double WindowWidth { get; private set; }
    public double WindowHeight { get; private set; }

    public event EventHandler<SizeChangedEventArgs>? SizeChanged;

    public void UpdateSize(double width, double height)
    {
        WindowWidth = width;
        WindowHeight = height;
        SizeChanged?.Invoke(this, new SizeChangedEventArgs(width, height));
    }
}
```

### MessageService

**File**: `Services/MessageService.cs`

Centralized UI messaging.

```csharp
public class MessageService
{
    public static MessageService Instance { get; } = new();
    
    public event EventHandler<MessageEventArgs>? MessageReceived;

    public void ShowInfo(string message)
    {
        MessageReceived?.Invoke(this, new MessageEventArgs(message, MessageType.Info));
    }

    public void ShowWarning(string message)
    {
        MessageReceived?.Invoke(this, new MessageEventArgs(message, MessageType.Warning));
    }

    public void ShowError(string message)
    {
        MessageReceived?.Invoke(this, new MessageEventArgs(message, MessageType.Error));
    }
}

public enum MessageType { Info, Warning, Error }

public class MessageEventArgs : EventArgs
{
    public string Message { get; }
    public MessageType Type { get; }
    public DateTime Timestamp { get; }

    public MessageEventArgs(string message, MessageType type)
    {
        Message = message;
        Type = type;
        Timestamp = DateTime.Now;
    }
}
```

---

## DSP and Audio

### Audio Pipeline

```
Generator → InstrumentSample → DSP Effects → Mixer → Output
```

### InstrumentSample

**File**: `PlayFunctions/DSP/InstrumentSample.cs`

Represents a single note instance.

```csharp
public class InstrumentSample
{
    public Preset Preset { get; set; }
    public int Note { get; set; }
    public int Velocity { get; set; }
    public double StartTime { get; set; }
    public double Duration { get; set; }
    public double Pan { get; set; } // -1.0 to 1.0
    public double Volume { get; set; } // dB
    
    // Sample data
    public short[] LeftChannel { get; set; }
    public short[] RightChannel { get; set; }
    
    public void ApplyVolume(double db) { /* Implementation */ }
    public void ApplyPan(double pan) { /* Implementation */ }
    public void ApplyTremolo(Tremolo tremolo, int sampleRate) { /* Implementation */ }
    public void ApplyVibrato(Tremolo vibrato, int sampleRate) { /* Implementation */ }
}
```

### SourcesFromAlgorithmic

**File**: `PlayFunctions/DSP/SourcesFromAlgorithmic.cs`

Generates InstrumentSamples from algorithmic generator.

```csharp
public static class SourcesFromAlgorithmic
{
    public static List<InstrumentSample> Generate(Algorithmic generator, int sampleRate)
    {
        var samples = new List<InstrumentSample>();
        double currentTime = 0;
        int index = 0;

        while (currentTime < generator.Duration)
        {
            // Get parameters from algorithms
            double note = generator.NoteAlgorithm.GetValue(currentTime, index);
            double speed = generator.SpeedAlgorithm.GetValue(currentTime, index);
            double duration = generator.DurationAlgorithm.GetValue(currentTime, index);
            double attack = generator.AttackAlgorithm.GetValue(currentTime, index);
            double volume = generator.VolumeAlgorithm.GetValue(currentTime, index);
            double pan = generator.PanAlgorithm.GetValue(currentTime, index);

            // Create sample
            var sample = new InstrumentSample
            {
                Preset = generator.Preset,
                Note = (int)Math.Round(note + generator.NoteShift),
                Velocity = (int)Math.Round(attack),
                StartTime = generator.StartTime + currentTime,
                Duration = duration / 1000.0, // ms to seconds
                Pan = pan,
                Volume = volume
            };

            samples.Add(sample);

            // Advance time
            currentTime += 60.0 / speed; // BPM to seconds
            index++;
        }

        return samples;
    }
}
```

### SourcesFromStochastic

**File**: `PlayFunctions/DSP/SourcesFromStochastic.cs`

Generates InstrumentSamples from stochastic generator.

```csharp
public static class SourcesFromStochastic
{
    public static List<InstrumentSample> Generate(Stochastic generator, int sampleRate)
    {
        var samples = new List<InstrumentSample>();

        foreach (var cloud in generator.Clouds)
        {
            var cloudSamples = GenerateCloud(cloud, generator, sampleRate);
            samples.AddRange(cloudSamples);
        }

        return samples;
    }

    private static List<InstrumentSample> GenerateCloud(Cloud cloud, Stochastic generator, int sampleRate)
    {
        var samples = new List<InstrumentSample>();
        var random = new FastRandom();

        for (int i = 0; i < cloud.Grains; i++)
        {
            // Random start time within cloud duration
            double startTime = cloud.StartTime + random.NextDouble() * (cloud.StopTime - cloud.StartTime);
            
            // Random pitch from distribution
            int note = GeneratePitch(cloud, random);
            
            // Random duration
            double duration = cloud.DurationMin + random.NextDouble() * (cloud.DurationMax - cloud.DurationMin);
            
            // Volume envelope
            double volume = CalculateVolumeEnvelope(startTime, cloud);

            var sample = new InstrumentSample
            {
                // Set properties...
            };

            samples.Add(sample);
        }

        return samples;
    }
}
```

### Reverb

**File**: `PlayFunctions/DSP/Reverb.cs`

Simple feedback-delay reverb.

```csharp
public class Reverb
{
    private double _delaySeconds;
    private double _decay; // 0.0 to 1.0
    private int _delaySamples;
    private Queue<double> _delayBuffer;

    public Reverb(double delaySeconds, double decay, int sampleRate)
    {
        _delaySeconds = delaySeconds;
        _decay = decay;
        _delaySamples = (int)(delaySeconds * sampleRate);
        _delayBuffer = new Queue<double>(_delaySamples);
        
        // Initialize with zeros
        for (int i = 0; i < _delaySamples; i++)
            _delayBuffer.Enqueue(0);
    }

    public double Process(double input)
    {
        // Get delayed signal
        double delayed = _delayBuffer.Dequeue();
        
        // Mix input with feedback
        double output = input + (delayed * _decay);
        
        // Add to delay buffer
        _delayBuffer.Enqueue(output);
        
        return output;
    }
}
```

### PlayEngine

**File**: `PlayFunctions/PlayEngine.cs`

Core audio playback engine.

```csharp
public class PlayEngine
{
    private WaveOutEvent? _waveOut;
    private BufferedWaveProvider? _waveProvider;
    private List<InstrumentSample> _allSamples = new();
    private int _sampleRate = 44100;

    public async Task PlayComposition(Composition composition, CancellationToken cancellationToken)
    {
        // Generate all samples
        _allSamples = GenerateAllSamples(composition);
        
        // Sort by start time
        _allSamples = _allSamples.OrderBy(s => s.StartTime).ToList();

        // Initialize audio output
        _waveProvider = new BufferedWaveProvider(new WaveFormat(_sampleRate, 16, 2));
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_waveProvider);
        _waveOut.Play();

        // Stream samples
        await StreamSamples(cancellationToken);
    }

    private List<InstrumentSample> GenerateAllSamples(Composition composition)
    {
        var allSamples = new List<InstrumentSample>();

        foreach (var track in composition.Tracks)
        {
            foreach (var generator in track.Generators)
            {
                List<InstrumentSample> samples;
                
                if (generator is Algorithmic alg)
                    samples = SourcesFromAlgorithmic.Generate(alg, _sampleRate);
                else if (generator is Stochastic sto)
                    samples = SourcesFromStochastic.Generate(sto, _sampleRate);
                else
                    continue;

                // Apply track volume
                foreach (var sample in samples)
                    sample.ApplyVolume(track.Volume);

                allSamples.AddRange(samples);
            }
        }

        return allSamples;
    }

    private async Task StreamSamples(CancellationToken cancellationToken)
    {
        // Implementation: Mix and stream samples to audio output
    }
}
```

---

## File I/O

### CMGDB

**File**: `Utilities/CMGDB.cs`

Handles reading and writing .cmg files.

```csharp
public static class CMGDB
{
    public static Composition Read(string filePath)
    {
        var composition = new Composition();
        var doc = new XmlDocument();
        doc.Load(filePath);

        // Parse XML structure
        var root = doc.DocumentElement;
        
        // Read metadata
        composition.FileName = filePath;
        composition.Comments = ReadComments(root);
        
        // Read tracks
        var tracksNode = root.SelectSingleNode("Tracks");
        foreach (XmlNode trackNode in tracksNode.ChildNodes)
        {
            var track = ReadTrack(trackNode);
            composition.Tracks.Add(track);
        }

        return composition;
    }

    public static void Write(string filePath, Composition composition)
    {
        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("Composition");
        doc.AppendChild(root);

        // Write metadata
        WriteMetadata(root, composition);

        // Write tracks
        var tracksNode = doc.CreateElement("Tracks");
        root.AppendChild(tracksNode);

        foreach (var track in composition.Tracks)
        {
            var trackNode = WriteTrack(doc, track);
            tracksNode.AppendChild(trackNode);
        }

        // Atomic write: write to temp file, then replace
        string tempFile = filePath + ".tmp";
        doc.Save(tempFile);
        
        if (File.Exists(filePath))
            File.Delete(filePath);
            
        File.Move(tempFile, filePath);
    }

    private static Track ReadTrack(XmlNode trackNode)
    {
        var track = new Track
        {
            Name = trackNode.Attributes["name"]?.Value ?? "",
            Volume = double.Parse(trackNode.SelectSingleNode("Volume")?.InnerText ?? "0")
        };

        var generatorsNode = trackNode.SelectSingleNode("Generators");
        foreach (XmlNode genNode in generatorsNode.ChildNodes)
        {
            Generator generator;
            
            if (genNode.Name == "AlgorithmicGenerator")
                generator = ReadAlgorithmicGenerator(genNode, track);
            else if (genNode.Name == "StochasticGenerator")
                generator = ReadStochasticGenerator(genNode, track);
            else
                continue;

            track.Generators.Add(generator);
        }

        return track;
    }

    // Additional helper methods...
}
```

### Atomic File Writing

To prevent corruption, files are written atomically:

1. Write to temporary file (`.tmp`)
2. Delete old file
3. Rename temporary to final name

This ensures the file is never in a partially-written state.

---

## UI Components

### Custom Chrome

**File**: `Layout/CustomChrome.xaml`

Reusable window chrome UserControl.

```xml
<UserControl x:Class="CMGWpf.Layout.CustomChrome"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="40"/> <!-- Title bar -->
            <RowDefinition Height="Auto"/> <!-- Menu (optional) -->
            <RowDefinition Height="*"/> <!-- Content -->
        </Grid.RowDefinitions>

        <!-- Title Bar -->
        <Border Grid.Row="0" Background="{StaticResource TitleBarBrush}">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="40"/> <!-- Logo -->
                    <ColumnDefinition Width="*"/> <!-- Title -->
                    <ColumnDefinition Width="Auto"/> <!-- Buttons -->
                </Grid.ColumnDefinitions>

                <!-- Logo -->
                <Image Grid.Column="0" Source="/Assets/CMG-Logo.png" />

                <!-- Title -->
                <TextBlock Grid.Column="1" Text="{Binding Title}" 
                           HorizontalAlignment="Center" VerticalAlignment="Center"/>

                <!-- Window Buttons -->
                <StackPanel Grid.Column="2" Orientation="Horizontal">
                    <Button Content="—" Command="{Binding MinimizeCommand}"/>
                    <Button Content="□" Command="{Binding MaximizeCommand}"/>
                    <Button Content="✕" Command="{Binding CloseCommand}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Menu -->
        <ContentControl Grid.Row="1" Content="{Binding Menu}"/>

        <!-- Content -->
        <ContentControl Grid.Row="2" Content="{Binding Content}"/>
    </Grid>
</UserControl>
```

### Timeline

**File**: `View/TimeLineViewModel.cs`

Timeline visualization and interaction.

**Features**:
- Horizontal scrolling
- Zoom in/out
- Generator dragging (move)
- Generator resizing (start/stop time)
- Grid snapping
- Time ruler

**Implementation**: Uses `Canvas` with data-bound generator rectangles.

### Generator Dialog

**File**: `Dialogs/GeneratorDialog.xaml`

Complex dialog with tabbed interface:
- Basic settings tab
- Algorithms tab (for Algorithmic)
- Composition grid tab (for Stochastic)
- Effects tab
- Validation display

**Key Features**:
- Real-time validation
- Error highlighting
- Soundfont browser
- Algorithm editor sub-dialogs
- Non-modal operation

---

## Testing

### Unit Tests

**Project**: `tests/CMGWpf.UnitTests`

**Framework**: xUnit

**Test Categories**:
- Model tests (Generator, Track, Composition)
- Algorithm tests (Constant, Linear, Exponential, etc.)
- Utility tests (Math, Euclidean, Stochastic)
- Service tests (FileLock, Message)

**Example**:
```csharp
public class AlgorithmTests
{
    [Fact]
    public void Constant_ReturnsFixedValue()
    {
        var alg = new Constant(42);
        Assert.Equal(42, alg.GetValue(0, 0));
        Assert.Equal(42, alg.GetValue(100, 50));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0.5, 5, 5)]
    [InlineData(1.0, 10, 10)]
    public void Linear_InterpolatesCorrectly(double time, double expected, double duration)
    {
        var alg = new Linear { StartValue = 0, EndValue = 10, Duration = duration };
        Assert.Equal(expected, alg.GetValue(time, 0));
    }
}
```

### UI Tests

**Project**: `tests/CMGWpf.UITests`

**Framework**: FlaUI (UI Automation)

**Test Categories**:
- Window state tests
- File operation tests
- Generator CRUD tests
- Playback tests

**Example**:
```csharp
[Test]
public void CanOpenAndCloseGeneratorDialog()
{
    using var app = Application.Launch("CMGWpf.exe");
    using var automation = new UIA3Automation();
    var window = app.GetMainWindow(automation);

    // Add track
    var trackMenu = window.FindFirstDescendant(cf => cf.ByText("Track")).AsMenuItem();
    trackMenu.Click();
    var addTrackItem = window.FindFirstDescendant(cf => cf.ByText("Add Track")).AsMenuItem();
    addTrackItem.Click();

    // Add generator
    var addGenButton = window.FindFirstDescendant(cf => cf.ByText("Add Generator")).AsButton();
    addGenButton.Click();

    // Verify dialog opened
    var genDialog = window.FindFirstChild(cf => cf.ByName("Generator Editor"));
    Assert.IsNotNull(genDialog);

    // Close dialog
    var cancelButton = genDialog.FindFirstDescendant(cf => cf.ByText("Cancel")).AsButton();
    cancelButton.Click();

    // Verify dialog closed
    genDialog = window.FindFirstChild(cf => cf.ByName("Generator Editor"));
    Assert.IsNull(genDialog);
}
```

---

## Build and Deployment

### Build Configuration

**Debug**: 
- Symbols included
- No optimization
- Debugger-friendly

**Release**:
- Optimized
- No symbols (separate .pdb file)
- Trimmed dependencies

### Building from Command Line

```bash
# Restore dependencies
dotnet restore

# Build Debug
dotnet build --configuration Debug

# Build Release
dotnet build --configuration Release

# Run
dotnet run --project src/CMGWpf/CMGWpf.csproj

# Publish (self-contained)
dotnet publish -c Release -r win-x64 --self-contained true
```

### Deployment

**Requirements**:
- .NET 10.0 Runtime (or self-contained publish)
- Windows 10/11
- FFmpeg (auto-downloaded by FFMpegCore)

**Distribution**:
1. Publish as self-contained
2. Include:
   - CMGWpf.exe
   - All DLLs
   - Assets folder
   - Optional: Sample .cmg files, soundfonts
3. Create installer (e.g., WiX, InnoSetup)

### Jump List Setup

**File**: `src/CMGWpf/JumpList-Setup.md`

Configure Windows taskbar Jump List for recent files.

**Implementation**: Use `System.Windows.Shell.JumpList`

---

## Contributing Guidelines

### Code Style

- **C#**: Follow Microsoft C# conventions
  - PascalCase for public members
  - camelCase for private fields (with `_` prefix)
  - Meaningful names
  - XML documentation comments for public APIs

- **XAML**: 
  - Consistent indentation (4 spaces)
  - Alphabetize attributes
  - Use bindings over code-behind where possible

### Git Workflow

1. **Fork** the repository
2. **Create feature branch** from `master`
   ```bash
   git checkout -b feature/my-new-feature
   ```
3. **Make changes** with clear, atomic commits
4. **Test** thoroughly (unit tests + manual testing)
5. **Push** to your fork
6. **Create Pull Request** to `master`

### Pull Request Guidelines

- Describe what the PR does
- Reference any related issues
- Include screenshots for UI changes
- Ensure all tests pass
- Update documentation if needed

### Issue Reporting

**Bug Reports**:
- Steps to reproduce
- Expected vs. actual behavior
- Screenshots/videos if applicable
- OS version, .NET version

**Feature Requests**:
- Use case description
- Mockups/wireframes if applicable
- Rationale for the feature

---

## API Reference

### Key Classes

#### Composition
```csharp
public class Composition
{
    public string FileName { get; set; }
    public ObservableCollection<Track> Tracks { get; set; }
    public string Comments { get; set; }
    public int SampleRate { get; set; }
}
```

#### Track
```csharp
public class Track : INotifyPropertyChanged
{
    public string Name { get; set; }
    public double Volume { get; set; }
    public ObservableCollection<Generator> Generators { get; set; }
    
    public void AddGenerator(Generator generator);
    public void RemoveGenerator(Generator generator);
    public void ShiftGenerators(double seconds);
}
```

#### Generator (Abstract)
```csharp
public abstract class Generator : INotifyPropertyChanged
{
    public int UID { get; set; }
    public string Name { get; set; }
    public double StartTime { get; set; }
    public double StopTime { get; set; }
    public double Duration { get; }
    
    public abstract bool Validate(out List<string> errors);
    public abstract Generator Clone();
}
```

#### Algorithmic : Generator
```csharp
public class Algorithmic : Generator
{
    public SoundFont? SoundFont { get; set; }
    public Preset? Preset { get; set; }
    public Algorithm NoteAlgorithm { get; set; }
    public Algorithm SpeedAlgorithm { get; set; }
    public Algorithm DurationAlgorithm { get; set; }
    public Algorithm AttackAlgorithm { get; set; }
    public Algorithm VolumeAlgorithm { get; set; }
    public Algorithm PanAlgorithm { get; set; }
    public Tremolo Tremolo { get; set; }
    public Tremolo Vibrato { get; set; }
}
```

#### Algorithm (Abstract)
```csharp
public abstract class Algorithm
{
    public abstract double GetValue(double time, int index);
    public abstract string GetDescription();
}
```

#### FileLockService
```csharp
public class FileLockService
{
    public static FileLockService Instance { get; }
    
    public bool TryAcquireLock(string filePath);
    public void ReleaseLock();
}
```

---

## Appendix: Architecture Diagrams

### Component Diagram

```
┌────────────────────────────────────────────────┐
│                 Presentation                    │
│  ┌─────────────┐  ┌──────────────┐            │
│  │MainWindow   │  │  Dialogs     │            │
│  │  (XAML)     │  │  (XAML)      │            │
│  └──────┬──────┘  └──────┬───────┘            │
└─────────┼─────────────────┼────────────────────┘
          │                 │
┌─────────▼─────────────────▼────────────────────┐
│              View Models                        │
│  ┌─────────────┐  ┌──────────────┐            │
│  │FileViewModel│  │TracksViewModel│           │
│  └──────┬──────┘  └──────┬───────┘            │
└─────────┼─────────────────┼────────────────────┘
          │                 │
┌─────────▼─────────────────▼────────────────────┐
│           Model / Business Logic                │
│  ┌─────────┐  ┌──────────┐  ┌─────────┐       │
│  │Track    │  │Generator │  │Algorithm│       │
│  └─────────┘  └──────────┘  └─────────┘       │
└─────────┬──────────┬──────────┬────────────────┘
          │          │          │
    ┌─────▼────┐┌───▼─────┐┌──▼──────┐
    │Services  ││   DSP   ││  I/O    │
    │          ││         ││         │
    └──────────┘└─────────┘└─────────┘
```

### Playback Flow

```
User clicks Play
       │
       ▼
PlayEngine.PlayComposition()
       │
       ├─► Generate samples (Algorithmic)
       │   └─► SourcesFromAlgorithmic.Generate()
       │
       ├─► Generate samples (Stochastic)
       │   └─► SourcesFromStochastic.Generate()
       │
       ├─► Sort by start time
       │
       ├─► Apply effects
       │   ├─► Tremolo
       │   ├─► Vibrato
       │   ├─► Reverb
       │   └─► Volume/Pan
       │
       ├─► Mix to stereo
       │
       └─► Stream to WaveOut
```

---

*CMGWpf Programmer's Guide - Version 1.0*  
*Last Updated: 2025*
