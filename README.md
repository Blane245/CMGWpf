# CMGWpf - Computer Music Generator

![CMG Logo](src/CMGWpf/Assets/CMG-Logo.png)

A powerful WPF-based desktop application for algorithmic and stochastic music composition, audio synthesis, and multimedia generation.

## 🎵 Overview

CMGWpf is a sophisticated computer music generation tool that enables composers and sound designers to create complex musical compositions using both algorithmic and stochastic (chance-based) methods. The application provides a comprehensive environment for designing, editing, and rendering musical generators with advanced DSP capabilities.

## ✨ Key Features

### Music Generation
- **Algorithmic Generators**: Create deterministic musical sequences using mathematical algorithms
- **Stochastic Generators**: Generate music using probability-based cloud compositions
- **Multi-Track Support**: Organize generators across multiple tracks for complex arrangements
- **Timeline Interface**: Visual timeline for managing generators and their temporal relationships

### Audio & Video Recording
- **Audio Export**: WAV and MP3 format support (using NAudio and NAudio.Lame)
- **Video Recording**: MP4 generation with synchronized audio and soundroll visualization (using FFMpegCore)
- **Virtual Frame Rendering**: 30fps video generation without real-time constraints
- **Report Generation**: Comprehensive composition reports

### DSP & Effects
- **Reverb**: Feedback-delay loop with configurable delay and decay
- **Tremolo**: Amplitude modulation effects
- **Vibrato**: Pitch modulation effects  
- **Noise Generation**: Configurable noise synthesis
- **SoundFont Support**: SF2 soundfont integration for instrument synthesis

### Advanced Features
- **Multi-Instance Support**: Run multiple CMG instances simultaneously
- **File Locking**: Prevents concurrent access to the same composition file
- **Jump List Integration**: Quick access to recent files from Windows taskbar
- **Window State Persistence**: Remembers window positions and sizes
- **Taskbar-Aware Maximize**: Smart window maximization that respects taskbar placement
- **Custom Chrome**: Modern, consistent window styling across all dialogs

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- .NET 10.0 Runtime
- FFmpeg (automatically downloaded by FFMpegCore on first use)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/Blane245/CMGWpf.git
   ```

2. Open the solution in Visual Studio 2026 or later:
   ```
   CMGWpf.sln
   ```

3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

4. Build the solution:
   ```bash
   dotnet build
   ```

5. Run the application:
   ```bash
   dotnet run --project src/CMGWpf/CMGWpf.csproj
   ```

### Quick Start
1. Launch CMGWpf
2. Create a new composition or open an existing .cmg file
3. Add tracks to organize your composition
4. Create generators (algorithmic or stochastic) on each track
5. Configure generator parameters using the edit dialog
6. Preview your composition using the Play feature
7. Export to audio (WAV/MP3) or video (MP4) formats

## 📚 Documentation

- **[User's Guide](docs/UsersGuide.md)**: Comprehensive guide for using the application
- **[Programmer's Guide](docs/ProgrammersGuide.md)**: Architecture and development documentation

## 🏗️ Project Structure

```
CMGWpf/
├── src/
│   └── CMGWpf/
│       ├── Assets/              # Application icons and images
│       ├── Dialogs/             # Dialog windows and user controls
│       ├── Layout/              # Main layout components (Menu, etc.)
│       ├── Model/               # Business logic and data models
│       │   └── Generators/      # Algorithmic and Stochastic generators
│       ├── MVVM/                # ViewModels and Commands
│       ├── PlayFunctions/       # Audio/Video playback and recording
│       │   └── DSP/             # Digital Signal Processing
│       ├── Services/            # Application services (file locking, etc.)
│       ├── SoundFont_2/         # SoundFont parsing and management
│       ├── Styles/              # XAML styling resources
│       ├── Types/               # Custom type definitions
│       ├── Utilities/           # Helper classes and utilities
│       └── View/                # ViewModels for main UI components
├── tests/
│   ├── CMGWpf.UITests/          # UI automation tests
│   └── CMGWpf.UnitTests/        # Unit tests
└── docs/                        # Documentation
```

## 🔧 Technologies Used

- **.NET 10.0**: Modern .NET framework
- **WPF**: Windows Presentation Foundation for rich UI
- **NAudio**: Audio playback and WAV export
- **NAudio.Lame**: MP3 encoding
- **FFMpegCore**: Video generation and encoding
- **Extended.Wpf.Toolkit**: Enhanced WPF controls
- **Material.Icons.WPF**: Material Design icons
- **Microsoft.Xaml.Behaviors.Wpf**: MVVM support

## 🧪 Testing

Run unit tests:
```bash
dotnet test tests/CMGWpf.UnitTests
```

Run UI automation tests:
```bash
dotnet test tests/CMGWpf.UITests
```

## 🐛 Known Issues

See [notes.md](src/CMGWpf/notes.md) for current bug tracking and implementation status.

## 📝 License

Copyright © 2025. All rights reserved.

## 👤 Author

**Blane**
- GitHub: [@Blane245](https://github.com/Blane245)

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the issues page.

## 📧 Support

For support and questions, please open an issue on the GitHub repository.

---

**Note**: This project targets .NET 10.0 and requires Visual Studio 2026 or later for development.
