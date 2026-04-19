# CMGWpf User's Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [User Interface Overview](#user-interface-overview)
4. [Working with Files](#working-with-files)
5. [Tracks and Composition](#tracks-and-composition)
6. [Generators](#generators)
7. [Playback and Recording](#playback-and-recording)
8. [Tools and Utilities](#tools-and-utilities)
9. [Preferences and Settings](#preferences-and-settings)
10. [Keyboard Shortcuts](#keyboard-shortcuts)
11. [Tips and Best Practices](#tips-and-best-practices)
12. [Troubleshooting](#troubleshooting)

---

## Introduction

CMGWpf (Computer Music Generator for WPF) is a desktop application for creating music through algorithmic and stochastic (chance-based) composition techniques. Whether you're an experimental composer, sound designer, or music educator, CMGWpf provides powerful tools for generating, editing, and rendering complex musical structures.

### What is Computer Music Generation?

Computer music generation uses mathematical algorithms and probability models to create musical sequences. This approach enables:
- Exploration of musical patterns that would be difficult to compose manually
- Creation of long-form compositions with evolving structures
- Generation of variation and unpredictability in musical performance
- Integration of precise timing and control with creative randomness

### Key Concepts

- **Track**: A container for organizing related generators
- **Generator**: A musical pattern creator (either algorithmic or stochastic)
- **Timeline**: Visual representation of generators across time
- **Soundfont**: A collection of sampled instrument sounds (SF2 format)
- **Algorithm**: A mathematical formula that produces musical parameters
- **Cloud**: In stochastic generators, a probabilistic distribution of notes

---

## Getting Started

### Installation

1. Download and install the latest version of CMGWpf
2. Ensure you have .NET 10.0 Runtime installed
3. Launch CMGWpf from the Start Menu or desktop shortcut

### First Launch

On first launch, CMGWpf will:
- Display a splash screen with the CMG logo
- Open with an empty composition
- Position the window on your primary or secondary monitor
- Create necessary application data folders

### Creating Your First Composition

1. **Create a Track**
   - Go to `Track → Add Track`
   - Give your track a descriptive name (e.g., "Melody", "Bass", "Percussion")

2. **Add a Generator**
   - Right-click on the track in the timeline
   - Select `Add Generator`
   - Choose between Algorithmic or Stochastic

3. **Configure the Generator**
   - Set the start and stop times
   - Select a soundfont and preset (instrument)
   - Configure musical parameters

4. **Preview Your Work**
   - Click `Play` from the menu or generator dialog
   - Listen to the generated audio

5. **Save Your Composition**
   - Go to `File → Save` or `File → Save As`
   - Choose a location and filename (.cmg extension)

---

## User Interface Overview

### Main Window

The main window consists of:

**Title Bar**
- CMG logo (left)
- Composition filename (center)
- Window controls: Minimize, Maximize, Close (right)

**Menu Bar**
- File: New, Open, Save, Recent files
- Edit: Preferences, Comments
- Track: Manage tracks and track-level operations
- Play: Playback and recording functions
- Tools: Utilities for generator manipulation

**Timeline View**
- Horizontal timeline showing time progression
- Vertical track lanes
- Generator blocks showing their temporal extent
- Zoom and pan controls

**Status/Message Area**
- Displays informational messages, warnings, and errors
- Automatically scrolls to show recent messages
- Cleared when starting a new operation

### Window Behavior

CMGWpf supports modern window management:

- **Multiple Instances**: Run multiple CMG instances simultaneously (different files only)
- **Non-Modal Dialogs**: Edit multiple tracks and generators concurrently
- **Window Memory**: Remembers size and position across sessions
- **Taskbar-Aware Maximize**: Maximizes without overlapping the taskbar

---

## Working with Files

### File Types

**CMG File (.cmg)**
- Native CMGWpf composition format
- XML-based structure
- Contains all tracks, generators, and settings

### File Operations

#### New Composition
`File → New` or `Ctrl+N`
- Prompts to save current file if modified
- Creates empty composition with default settings

#### Open
`File → Open` or `Ctrl+O`
- Opens file browser to select .cmg file
- Cannot open files already opened in another CMGWpf instance
- Displays error if file is locked

#### Save
`File → Save` or `Ctrl+S`
- Saves current composition to existing filename
- Prompts for filename if composition is new

#### Save As
`File → Save As` or `Ctrl+Shift+S`
- Saves composition with new filename
- Useful for creating variations

#### Recent Files
`File → Recent`
- Shows recently opened files
- Click to open (subject to file locking)
- Also available from Windows taskbar Jump List

### File Locking

CMGWpf prevents multiple instances from opening the same file:
- Lock files (`.lock`) created when opening a composition
- Automatically cleaned up on normal exit
- Stale locks (older than 1 hour) are automatically removed
- Manual unlock required if application crashes

---

## Tracks and Composition

### Understanding Tracks

Tracks organize your composition into logical layers, similar to tracks in a DAW (Digital Audio Workstation). Each track can contain multiple generators that play sequentially or simultaneously.

### Track Operations

#### Adding a Track
1. `Track → Add Track`
2. Enter a unique track name
3. Track appears in the timeline

#### Renaming a Track
1. Select track in timeline
2. `Track → Rename Track`
3. Enter new name (must be unique)
4. Only one rename dialog per track allowed

#### Deleting a Track
1. Select track
2. `Track → Delete Track`
3. Confirm deletion
4. All generators on track are removed

### Track-Level Tools

#### Track Volume
`Track → Tools → Volume`
- Adjusts overall volume for all generators on track
- Range: -100 dB to +100 dB (0 dB = no change)
- Applied during playback and rendering

#### Track Shift
`Track → Tools → Shift`
- Moves all generators on track forward or backward in time
- Useful for adjusting timing relationships
- **Note**: Automatically updates start/stop times of open generator editors

---

## Generators

Generators are the core of CMGWpf, producing the actual musical content. There are two types: **Algorithmic** and **Stochastic**.

### Generator Types

#### Algorithmic Generators

Algorithmic generators use mathematical formulas to create deterministic musical sequences.

**Key Parameters:**
- **Note Algorithm**: Determines which notes to play
- **Speed Algorithm**: Controls tempo/rhythm
- **Duration Algorithm**: Sets note lengths
- **Attack Algorithm**: Controls note velocity/volume
- **Volume Algorithm**: Overall level adjustment
- **Pan Algorithm**: Stereo positioning

**Algorithm Types:**
- **Constant**: Fixed value
- **Linear**: Value changes linearly over time
- **Exponential**: Value changes exponentially
- **Random**: Randomly selected values within range
- **Euclidean Rhythm**: Distributes notes evenly across beats
- **Sequence**: Repeating or non-repeating list of values

**Additional Features:**
- **Microtones**: Enable pitch values between semitones
- **Looping**: Repeat sequence patterns
- **Note Shift**: Transpose entire sequence
- **Euclidean Note Distribution**: Algorithmic note selection

#### Stochastic Generators

Stochastic generators create music through probability distributions called "clouds."

**Structure:**
- **Composition**: Grid of voices across time cells
- **Voices**: Individual layers with independent settings
- **Clouds**: Probabilistic note distributions within time ranges
- **Grains**: Individual note events within clouds

**Key Parameters:**
- **Time Cells**: Temporal divisions of the generator
- **Voices**: Number of independent musical lines
- **Cloud Density**: Number of grains (notes) per cloud
- **Pitch Distribution**: Range and probability of pitches
- **Duration Distribution**: Range and probability of note lengths
- **Volume Envelope**: Attack, sustain, release characteristics

### Generator Operations

#### Adding a Generator
1. Right-click on track (or `Track → Add Generator`)
2. Choose Algorithmic or Stochastic
3. Generator editor opens in Add mode (modal)
4. Configure parameters
5. Click `Submit` to add generator

#### Editing a Generator
1. Double-click generator in timeline
2. Generator editor opens (non-modal)
3. Modify parameters
4. Click `Submit` to save changes or `Cancel` to discard
5. Multiple generators can be edited simultaneously

#### Moving/Copying a Generator
1. Right-click generator
2. Select `Move` or `Copy`
3. Choose destination track
4. Generator is moved/copied
5. **Note**: Cannot move/copy if generator is being edited

#### Deleting a Generator
1. Right-click generator
2. Select `Delete`
3. Confirm deletion
4. **Note**: Cannot delete if generator is being edited

#### Playing a Generator
- Click `Play` button in generator editor
- Plays current state (even if not yet submitted)
- Useful for auditioning changes before saving

### Generator Dialog Features

#### Error Display
- Bottom section shows validation errors
- Scrollable list of issues
- Prevents submission until all errors resolved
- Updates in real-time as parameters change

#### Soundfont Selection
1. Click "Select SoundFont" button
2. Browse to .sf2 file
3. Preset list populates automatically
4. Choose instrument preset

#### Time Controls
- **Start Time**: When generator begins (format: mm:ss.ms)
- **Stop Time**: When generator ends
- **Duration**: Calculated automatically
- **Auto-adjust**: Changing start time maintains duration

#### Validation
- Real-time validation as you type
- Red highlighting for invalid fields
- Descriptive error messages
- `Submit` disabled until valid

---

## Playback and Recording

### Audio Playback

#### Play Dialog
`Play → Play` or click Play in generator editor

The Play dialog provides:
- **Progress Bar**: Visual playback position
- **Time Display**: Current time and total duration
- **Pause/Resume**: Pause and continue playback
- **Stop**: Terminate playback
- **Minimize/Maximize**: Window controls

**Important**: While Play dialog is active, main window is blocked. Minimize the play window if you need to access other applications.

#### Playback from Generator Editor
- Click `Play` in an open generator editor
- Plays the generator in its current state (pre-submission)
- Useful for testing parameter changes

### Audio Recording

#### Recording Formats
- **WAV**: Uncompressed, high quality
- **MP3**: Compressed, smaller file size (requires NAudio.Lame)

#### Recording Process
1. `Play → Record → Audio`
2. Select output format (WAV or MP3)
3. Choose file location and name
4. Click `Record`
5. Playback begins with simultaneous recording
6. File saved automatically when complete

#### Recording Settings
- Sample Rate: 44.1 kHz
- Bit Depth: 16-bit (WAV), Variable (MP3)
- Channels: Stereo

### Video Recording

#### Video with Soundroll
`Play → Record → Video`

Creates MP4 video with:
- Synchronized audio track
- Animated soundroll visualization
- 30 frames per second
- Programmatic scrolling (no real-time requirement)

#### Video Recording Process
1. Select `Play → Record → Video`
2. Choose file location and name
3. Click `Record`
4. Virtual rendering begins
   - Generates frames programmatically
   - Synchronizes audio
   - Shows progress dialog
5. MP4 file saved when complete

#### Video Requirements
- FFmpeg (automatically downloaded by FFMpegCore)
- Sufficient disk space
- Processing time varies with composition length

### Report Generation

`Play → Report`

Creates detailed text report with:
- Composition metadata
- Track listings
- Generator parameters
- Timeline visualization
- Algorithm specifications

**Uses**: Documentation, debugging, sharing composition details

---

## Tools and Utilities

### Generator Alignment Tools

#### Align Generators
`Tools → Align Generators`

Aligns the start or end times of multiple generators.

**Usage:**
1. Select `Tools → Align Generators`
2. Choose "Primary" generator (reference)
3. Select "Secondary" generators to align
4. Choose alignment type:
   - Align Start Times
   - Align End Times
5. Click `OK`

**Effect**: Secondary generators shift to match primary generator's alignment point.

#### Stagger Generators
`Tools → Stagger Generators`

Creates temporal offset between generators.

**Usage:**
1. Select primary generator (first in sequence)
2. Select secondary generators (to be staggered)
3. Enter stagger interval (seconds)
4. Click `OK`

**Effect**: Each secondary generator starts at an offset from the previous one.

#### Set Generators Equal Duration
`Tools → Set Generators Equal`

Makes multiple generators the same duration.

**Usage:**
1. Select primary generator (reference duration)
2. Select secondary generators
3. Click `OK`

**Effect**: Secondary generators' end times adjusted to match primary duration.

**Note**: These tools cannot run if any affected generators are being edited. Close generator editors first.

### Calculation Tools

#### Measure Duration Calculator
`Tools → Measure Duration Calculator`

Calculates time duration of musical measures.

**Inputs:**
- Tempo (BPM)
- Time Signature
- Number of Measures

**Output**: Duration in seconds

**Use Case**: Setting generator durations to exact measure lengths

#### Oscillator Frequency Calculator
`Tools → Oscillator Frequency Calculator`

Converts between frequency and musical pitch.

**Inputs:**
- Frequency (Hz) or MIDI note number
- Reference pitch (A4 = 440 Hz default)

**Outputs:**
- Equivalent MIDI note
- Frequency

**Use Case**: Tremolo/Vibrato frequency settings, noise generation

---

## Preferences and Settings

### Accessing Preferences
`Edit → Preferences`

### Preference Categories

#### Audio Settings
- **Sample Rate**: 44.1 kHz or 48 kHz
- **Buffer Size**: Affects latency and performance
- **Output Device**: Select audio interface

#### Display Settings
- **Timeline Zoom**: Default zoom level
- **Grid Snap**: Enable/disable grid snapping
- **Color Scheme**: Timeline and generator colors

#### File Settings
- **Auto-save Interval**: Minutes between auto-saves
- **Recent Files Count**: Number of files in recent list
- **Default Directory**: Starting location for file dialogs

#### Playback Settings
- **Pre-roll**: Silence before playback starts
- **Post-roll**: Silence after composition ends
- **Fade Out**: Automatic fade at end

### Comments
`Edit → Comment`

Add text notes to your composition:
- Project information
- Compositional intentions
- Performance notes
- Revision history

---

## Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| New Composition | `Ctrl+N` |
| Open Composition | `Ctrl+O` |
| Save | `Ctrl+S` |
| Save As | `Ctrl+Shift+S` |
| Play | `Ctrl+P` or `Space` |
| Stop | `Esc` |
| Add Track | `Ctrl+T` |
| Add Generator | `Ctrl+G` |
| Delete | `Delete` |
| Undo | `Ctrl+Z` |
| Redo | `Ctrl+Y` |
| Zoom In | `Ctrl++` |
| Zoom Out | `Ctrl+-` |
| Zoom to Fit | `Ctrl+0` |
| Preferences | `Ctrl+,` |
| Close Window | `Alt+F4` |

---

## Tips and Best Practices

### Composition Workflow

1. **Plan Your Structure**
   - Sketch out tracks and their roles
   - Consider which parts are algorithmic vs. stochastic
   - Think about temporal relationships

2. **Start Simple**
   - Begin with one track and one generator
   - Test playback early and often
   - Add complexity gradually

3. **Use Meaningful Names**
   - Name tracks and generators descriptively
   - Makes navigation easier in complex projects
   - Helps when returning to old projects

4. **Save Regularly**
   - Use `Ctrl+S` frequently
   - Create backup copies with `Save As`
   - Consider version numbering (e.g., MyPiece_v1, MyPiece_v2)

### Generator Design

#### Algorithmic Generators
- **Test Algorithms Independently**: Use constant values for other parameters while testing one algorithm
- **Use Euclidean Rhythms**: Great for creating polyrhythms and interesting patterns
- **Combine Algorithm Types**: Use random note selection with constant rhythm for variation
- **Layer Generators**: Multiple simple generators can create complex results

#### Stochastic Generators
- **Start with Few Voices**: 1-3 voices are easier to manage
- **Use Fewer Time Cells**: Start with 2-4 cells, add more as needed
- **Control Density**: Too many grains can muddy the texture
- **Experiment with Envelopes**: Volume envelopes greatly affect character
- **Listen to Each Voice**: Solo voices to ensure each is contributing

### Performance Optimization

- **Soundfont Size**: Smaller soundfonts load faster
- **Generator Count**: Many overlapping generators increase CPU load
- **Reverb Usage**: Reverb is CPU-intensive; use sparingly
- **Timeline Zoom**: Zoom in when editing; zoom out for overview
- **Close Unused Dialogs**: Non-modal dialogs consume memory

### Troubleshooting Tips

- **Generator Won't Play**: Check error messages in generator dialog
- **File Won't Open**: Another instance may have it open; check for .lock file
- **No Sound**: Verify soundfont loaded and preset selected
- **Distorted Audio**: Reduce volume or track count to prevent clipping
- **Slow Playback**: Reduce number of active generators or disable reverb

---

## Troubleshooting

### Common Issues

#### "File is already open in another instance"
**Cause**: Another CMGWpf instance has the file open, or stale lock file exists.

**Solution**:
1. Check for other CMGWpf windows
2. Close the other instance
3. If no other instance is running, delete the `.lock` file in the composition directory
4. Retry opening the file

#### "Invalid Generator - Cannot Play"
**Cause**: Generator has validation errors.

**Solution**:
1. Open generator editor
2. Check error display at bottom
3. Fix highlighted issues
4. Revalidate by clicking `Submit`

#### "Soundfont not found"
**Cause**: Soundfont file moved or deleted since last use.

**Solution**:
1. Open generator editor
2. Click "Select SoundFont"
3. Browse to correct .sf2 file location
4. Reselect preset

#### No Audio Output
**Possible Causes**:
- Audio device not selected
- Volume set to 0
- Soundfont not loaded
- Generator outside playback range

**Solutions**:
1. Check `Edit → Preferences → Audio Settings`
2. Verify soundfont loaded in generator
3. Check track and generator volumes
4. Ensure generator times overlap playback range

#### Application Crashes
**If CMGWpf crashes**:
1. Restart the application
2. Delete any `.lock` files manually if needed
3. Check for corrupted .cmg files (open in text editor to verify XML structure)
4. Report issue with reproduction steps

#### Video Recording Fails
**Possible Causes**:
- FFmpeg not installed
- Insufficient disk space
- Invalid output path

**Solutions**:
1. Ensure FFmpeg available (FFMpegCore auto-downloads)
2. Check available disk space
3. Verify output directory exists and is writable
4. Try shorter composition or lower quality settings

### Getting Help

- **Error Messages**: Read carefully; they often indicate the specific problem
- **Status Area**: Check message area for warnings and information
- **Recent Files**: If a file won't open, try recent backup
- **GitHub Issues**: Report bugs at https://github.com/Blane245/CMGWpf/issues

---

## Appendix A: Algorithm Reference

### Constant
Returns fixed value.

**Parameters**: Value

**Use**: Stable parameters like fixed tempo or constant pitch

### Linear
Value changes linearly from start to end.

**Parameters**: Start Value, End Value, Duration

**Use**: Gradual changes like accelerando, crescendo

### Exponential
Value changes exponentially.

**Parameters**: Start Value, End Value, Duration, Exponent

**Use**: Natural-sounding curves, faster/slower at ends

### Random
Random value within range.

**Parameters**: Min Value, Max Value, Seed (optional)

**Use**: Variation, unpredictability, aleatoric effects

### Euclidean Rhythm
Distributes events evenly.

**Parameters**: Steps (total), Pulses (events), Rotation (offset)

**Use**: Polyrhythms, interesting patterns, world music rhythms

### Sequence
Repeating or non-repeating list of values.

**Parameters**: Value List, Loop (true/false)

**Use**: Melodic patterns, rhythmic cycles, ostinatos

---

## Appendix B: File Format Specification

### CMG File Structure

CMG files are XML-based with the following structure:

```xml
<Composition>
  <Metadata>
    <Title>...</Title>
    <Author>...</Author>
    <Comment>...</Comment>
  </Metadata>
  <Tracks>
    <Track name="...">
      <Generators>
        <AlgorithmicGenerator>
          ...parameters...
        </AlgorithmicGenerator>
        <StochasticGenerator>
          ...parameters...
        </StochasticGenerator>
      </Generators>
    </Track>
  </Tracks>
  <Settings>
    ...preferences...
  </Settings>
</Composition>
```

### Lock File Format

`.lock` files contain:
- Process ID
- Lock timestamp
- Machine name
- User name

---

## Appendix C: Glossary

**Algorithm**: Mathematical formula producing parameter values  
**Algorithmic Generator**: Deterministic musical sequence  
**Attack**: Initial volume/velocity of note  
**BPM**: Beats Per Minute (tempo)  
**Cloud**: Probabilistic note distribution in stochastic generator  
**Composition**: Complete musical work in CMG  
**Duration**: Length of note or generator  
**Euclidean Rhythm**: Evenly-distributed rhythm pattern  
**Generator**: Musical pattern creator (algorithmic or stochastic)  
**Grain**: Individual note event in stochastic cloud  
**Jump List**: Windows taskbar recent file list  
**Lock File**: Prevents concurrent file access  
**Microtone**: Pitch between standard semitones  
**Pan**: Stereo positioning (left/right)  
**Preset**: Instrument sound from soundfont  
**Reverb**: Echo/ambience effect  
**Soundfont**: Collection of sampled instruments (.sf2)  
**Soundroll**: Visual representation of playback  
**Stochastic**: Probability-based composition  
**Time Cell**: Temporal division in stochastic generator  
**Timeline**: Visual representation of composition structure  
**Track**: Container for organizing generators  
**Tremolo**: Amplitude (volume) modulation  
**Vibrato**: Pitch modulation  
**Voice**: Independent layer in stochastic generator  

---

*CMGWpf User's Guide - Version 1.0*  
*Last Updated: 2025*
