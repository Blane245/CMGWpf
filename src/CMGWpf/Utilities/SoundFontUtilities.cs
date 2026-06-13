using CMGWpf.Properties;
using CMGWpf.SoundFont_2;
using System.Collections.ObjectModel;
using System.IO;

namespace CMGWpf.Utilities
{
    public static class SoundFontUtilities
    {
        public static string SoundFontLocation { get; set; } = Settings.Default.CMGSoundFontLocation;
        /// <summary>
        /// Returns a list of soundfont files in the specified location. The location is specified in the settings file and can be changed by the user. The soundfont files must have the .sf2 extension. If the location is not specified or does not exist, an empty list is returned and a message is logged to the debug output.
        /// </summary>
        /// <param name="location"></param>
        /// <returns>ObservableCollection of soundfont file names</returns>
        public static ObservableCollection<string> List(string location)
        {
            ObservableCollection<string> files = [];
            if (location == "")
            {
                DebugLog.Write("Local folder not specified");
                return files;
            }
            if (!Directory.Exists(location))
            {
                DebugLog.Write("Local folder does not exist");
                return files;
            }
            files = new ObservableCollection<string>(Directory.GetFiles(location, "*.sf2"));
            // strip the file location from the name
            for (int i = 0; i < files.Count; i++)
            {
                files[i] = files[i].Replace(location + "\\", "");
            }
            return files;

        }
        /// <summary>
        /// Returns a string representation of a preset in the format "BankNumber:PatchNumber:Name". The bank and patch numbers are formatted as three-digit numbers with leading zeros. The name is the name of the preset. This method is used to display the preset information in the UI and to identify the preset when loading it from a soundfont file.
        /// </summary>
        /// <param name="preset">The preset to convert to a string representation.</param>
        /// <returns>A string representation of the preset in the format "BankNumber:PatchNumber:Name".</returns>
        public static string BankPresetToName(Preset preset)
        {
            return
                preset.BankNumber.ToString("000") + ":" +
                preset.PatchNumber.ToString("000") + ":" +
                preset.Name.ToString();
        }
        /// <summary>
        /// Converts a MIDI note number to a string representation of the note. The MIDI note number is a decimal value where the integer part represents the base MIDI note and the fractional part represents the cents (microtonal deviation). The method calculates the octave, note name, and cents, and returns a string in the format "NoteNameOctave(+Cents)" if there are cents, or "NoteNameOctave" if there are no cents. If the MIDI note number is negative, it returns "Rest". This method is useful for displaying MIDI notes in a human-readable format in the UI.
        /// </summary>
        /// <param name="midi">The MIDI note number to convert to a string representation.</param>
        /// <returns>A string representation of the MIDI note in the format "NoteNameOctave(+Cents)" or "NoteNameOctave" if there are no cents. Returns "Rest" if the MIDI note number is negative.</returns>
        public static string MidiToNote(double midi)
        {
            if (midi < 0) return "Rest";
            int baseMidi = (int)Math.Round(midi);
            int cents = (int)Math.Round((midi - (double)baseMidi) * 100);
            int octave = baseMidi / 12 - 1;
            int note = baseMidi % 12;
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            string noteName = noteNames[note];
            string sign = (cents > 0) ? "+" : "";
            string extra = (cents == 0) ? "" : $"({sign}{cents})";
            return $"{noteName}{octave}{extra}";
        }

        private static readonly Dictionary<string, SoundFont> SFPool = [];
        /// <summary>
        /// Returns a SoundFont object for the specified soundfont file name. The method first checks if the soundfont is already loaded in the SFPool dictionary. If it is, it returns the cached SoundFont object. If it is not, it attempts to load the soundfont from the file system using the SoundFontLocation and the provided file name. If the file is found and successfully loaded, it adds the SoundFont object to the SFPool dictionary and returns it. If the file is not found or an error occurs during loading, it logs an appropriate message to the debug output and returns null. This method helps to optimize performance by caching loaded soundfonts and avoiding redundant file reads.
        /// </summary>
        /// <param name="SFFileName">The name of the soundfont file to load.</param>
        /// <returns>A SoundFont object if the file is successfully loaded; otherwise, null.</returns>
        public static SoundFont? GetSoundFont(string SFFileName)
        {
            if (SFPool.TryGetValue(SFFileName, out SoundFont? value))
            { return value; }
            else
            {
                try
                {
                    // either read the soundfont from the pool or from the file
                    // if the soundfont is not in the pool, read it from the local file or server
                    Stream stream = new FileStream(SoundFontLocation + "\\" + SFFileName, FileMode.Open, FileAccess.Read);
                    // create a new soundfont from the stream
                    SoundFont soundFont = new(stream);
                    SFPool.Add(SFFileName, soundFont);
                    return soundFont;
                }
                catch (FileNotFoundException e)
                {
                    DebugLog.Write($"SoundFont file not found: {e.Message}");
                    return null;
                }
                catch (Exception e)
                {
                    DebugLog.Write($"Error loading SoundFont: {e.Message}");
                    return null;
                }
            }
        }
    }
}