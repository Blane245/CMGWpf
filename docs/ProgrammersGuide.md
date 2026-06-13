# CMGWpf Programmer's Guide
<!-- TOC-->
  - [Architecture Overview](#architecture-overview)
    - [Technology Stack](#technology-stack)
    - [High-Level Architecture](#high-level-architecture)
    - [Key Design Principles](#key-design-principles)
  - [Project Structure](#project-structure)
    - [Project Files](#project-files)
  - [Design Patterns](#design-patterns)
    - [MVVM (Model-View-ViewModel)](#mvvm-model-view-viewmodel)
    - [Command Pattern](#command-pattern)
    - [Singleton Pattern](#singleton-pattern)
    - [Factory Pattern](#factory-pattern)
    - [Observer Pattern](#observer-pattern)
<!-- TOC -->

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
│  (File, Track, Generator, etc.)             │
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