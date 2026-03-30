using CMGWpf.Model;
using System.Collections.ObjectModel;
using static CMGWpf.Model.Generators.StochasticTypes;
using static CMGWpf.Types.DBTypes;

namespace CMGWpf.Utilities
{
    public static class EnsembleUtilities
    {
        public static async Task<ObservableCollection<Ensemble>> GetEnsembleListAsync()
        {
            List<Ensemble> ensembleList = [];
            DbResult<DbEnsembleListType> dbResponse = await CMGDB.FetchAsync<DbEnsembleListType>($"ensembles", "GET");

            if (dbResponse.IsSuccess && dbResponse.Value.value != null)
            {
                return [..dbResponse.Value.value.Select(dbEnsemble => new Ensemble
                {
                    Name = dbEnsemble.name,
                    Description = dbEnsemble.description,
                    Voices = dbEnsemble.voices,
                }).ToList()];
            }
            else
            {
                return [];
            }
        }
        public static async Task<(Ensemble, List<Voice>)> GetEnsembleAsync(string name)
        {
            DbResult<DbEnsembleType> dbResponse = await CMGDB.FetchAsync<DbEnsembleType>($"ensemble/{name}", "GET");
            if (dbResponse.IsSuccess)
            {
                EnsembleType dbEnsemble = dbResponse.Value.value;
                Ensemble ensemble = new()
                {
                    Name = dbEnsemble.name,
                    Description = dbEnsemble.description,
                    Voices = dbEnsemble.voices,
                };
                    List<Voice> voiceList = await GetEnsembleVoicesAsync(ensemble);
                return (ensemble, voiceList);
            }
            else
            {
                return (new Ensemble { Name = name, Description = "Error fetching ensemble", Voices = "" }, []);
            }
        }
        public static async Task<List<Voice>> GetEnsembleVoicesAsync(Ensemble ensemble)
        {
            List<Voice> voiceList = [];
            string[] voiceNames = [.. ensemble.Voices.Split(',').Select(v => v.Trim())];
            foreach (string voiceName in voiceNames)
            {
                DbResult<DbVoiceType> dbResponse = await CMGDB.FetchAsync<DbVoiceType>($"voice/{voiceName}", "GET");
                if (dbResponse.IsSuccess)
                {
                    DbVoiceType dbVoice = dbResponse.Value;
                    Voice voice = new()
                    {
                        Name = dbVoice.value.name,
                        Description = dbVoice.value.description,
                        SoundFontFileName = dbVoice.value.soundFontFile,
                        PresetName = dbVoice.value.presetName,
                        Timbre = Enum.Parse<TIMBRE>(dbVoice.value.timbre),
                        RegisterLo = dbVoice.value.registerLo,
                        RegisterHi = dbVoice.value.registerHi,
                        Duration = dbVoice.value.duration,
                        Muted = false, // default value for UI parameter
                        Volume = 0, // default value for UI parameter
                        Velocity = 63 // default value for UI parameter
                    };

                    // Read the soundfont from the identified file, and find the preset in the soundfont. If the soundfont or preset cannot be found, then these fields should be cleared. This is because the soundfont and preset are used to generate the sound for the voice, and if they cannot be found, then the voice cannot be generated, so these fields should be cleared to reflect that.
                    SoundFont_2.SoundFont? soundFont = SoundFontUtilities.GetSoundFont(voice.SoundFontFileName);
                    if (voice.PresetName != "" && soundFont != null)
                    {
                        voice.Preset = soundFont.Presets.FirstOrDefault(p => SoundFontUtilities.BankPresetToName(p) == voice.PresetName);
                    }
                    else
                    {
                        voice.Preset = null;
                        voice.PresetName = "";
                    }
                    voiceList.Add(voice);

                }
                else
                {
                    // if the voice cannot be found, add a placeholder voice with the name and an error description, and clear the soundfont and preset fields
                    Voice voice = new()
                    {
                        Name = voiceName,
                        Description = "Error fetching voice",
                        SoundFontFileName = "",
                        PresetName = "",
                        Timbre = TIMBRE.sustained,
                        RegisterLo = 0,
                        RegisterHi = 0,
                        Duration = 0,
                        Muted = false,
                        Volume = 0,
                        Velocity = 63,
                        Preset = null
                    };
                    voiceList.Add(voice);
                }
            }
                ;
            return voiceList;
        }
    }
}
