using CMGWpf.Properties;
using CMGWpf.SoundFont_2;
using System.Collections.ObjectModel;
using System.IO;

namespace CMGWpf.Utilities
{
    public static class SoundFontUtilities
    {
        public static string SoundFontLocation { get; set; } = Settings.Default.CMGSoundFontLocation;
        public static ObservableCollection<string> List(string location)
        {
            ObservableCollection<string> files = [];
            if (location == "")
            {
                System.Diagnostics.Debug.WriteLine("Local folder not specified");
                return files;
            }
            if (!Directory.Exists(location))
            {
                System.Diagnostics.Debug.WriteLine("Local folder does not exist");
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

        public static string BankPresetToName(Preset preset)
        {
            return
                preset.BankNumber.ToString("000") + ":" +
                preset.PatchNumber.ToString("000") + ":" +
                preset.Name.ToString();
        }

        public static string MidiToNote(double midi)
        {
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
                    System.Diagnostics.Debug.WriteLine($"SoundFont file not found: {e.Message}");
                    return null;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading SoundFont: {e.Message}");
                    return null;
                }
            }
        }
        public static void ClearSoundFontPool()
        {
            SFPool.Clear();
        }

    }
}