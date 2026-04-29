
using CMGDatabaseServer;
using CMGDBEditor.Types;
using CMGWpf.Model.Generators;
using CMGWpf.Services;
using System.Collections.ObjectModel;
namespace CMGDBEditor.Model
{
    public class Voice
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public StochasticTypes.TIMBRE Timbre { get; set; } = StochasticTypes.TIMBRE.sustained;
        public float RegisterLo { get; set; } = 0;
        public float RegisterHi { get; set; } = 0;
        public float Duration { get; set; } = 0;
        public string SoundFontFile { get; set; } = "";
        public string PresetName { get; set; } = "";

        // EF Core navigation property for many-to-many relationship
        public virtual ICollection<Ensemble> Ensembles { get; set; } = new List<Ensemble>();

        public Voice Clone()
        {
            Voice n = (Voice)this.MemberwiseClone();
            return n;
        }
        public static ObservableCollection<Error> Validate(Voice voice, string newName, ObservableCollection<Voice> allVoices)
        {
            ObservableCollection<Error> errors = [];
            if (newName.Trim(' ', '\t') == "")
            {
                errors.Add(new Error()
                {
                    IsError = true,
                    Message = "Voice name must not be blank"
                });
            }
            // find the voice object so it can be skipped
            int voiceIndex = -1;
            for (int i = 0; i < allVoices.Count; i++)
            {
                if (allVoices[i].Name == voice.Name) { voiceIndex = i; break; }
            }
            // see if there is another voice with the same name
            for (int i = 0; i < allVoices.Count; i++)
            {
                if (i == voiceIndex) continue;
                if (newName == allVoices[i].Name)
                {
                    errors.Add(new Error()
                    {
                        IsError = true,
                        Message = $"Voice name '{newName}' must be unique"
                    });
                    break;
                }
            }

            // check register range
            if (voice.RegisterLo < 0 || voice.RegisterHi > 127 || voice.RegisterLo > voice.RegisterHi)
            {
                errors.Add(new Error()
                {
                    IsError = true,
                    Message = "Voice register lo must be less than or equal to register hi and register hi must be within the range lo-127"
                });
            }

            // validate soundfont file and preset
            if (voice.SoundFontFile.Trim(' ', '\t') == "")
            {
                errors.Add(new Error()
                {
                    IsError = true,
                    Message = "No soundfont file specified for voice"
                });
            }
            if (voice.PresetName.Trim(' ', '\t') == "")
            {
                errors.Add(new Error()
                {
                    IsError = true,
                    Message = "No preset name specified for voice"
                });
            }
            return errors;
        }
        //public async Task<DatabaseResponse?> Add()
        //{
        //    try
        //    {
        //        // add the new voice to the DB
        //        var client = new DatabaseClient();
        //        var query = "INSERT INTO voice (Name, Description) VALUES (@Name, @Description);";
        //        var parameters = new Dictionary<string, object?>
        //        {
        //            { "@Name", Name },
        //            { "@Description", Description }
        //        };
        //        var response = await client.ExecuteQueryAsync(query, parameters);
        //        return response;
        //    }
        //    catch
        //    {
        //        return new DatabaseResponse() { Success = false, ErrorMessage = "An error occurred while adding the ensemble to the database." };
        //    }
        //}
        //public async Task<DatabaseResponse?> Modify(string newName)
        //{
        //    // modify the ensemble record changing the name is necessary
        //    try
        //    {
        //        var client = new DatabaseClient();
        //        var query = "UPDATE voice SET (Name, Description) = (@NewName, @Description) WHERE Name = @Name;";
        //        var parameters = new Dictionary<string, object?>
        //        {
        //            { "@Name", Name },
        //            { "@NewName", newName },
        //            { "@Description", Description }
        //        };
        //        var response = await client.ExecuteQueryAsync(query, parameters);
        //        return response;
        //    }
        //    catch
        //    {
        //        return new DatabaseResponse() { Success = false, ErrorMessage = $"An error occurred while modifying the ensemble '{Name}' in the database." };
        //    }
        //}
        //public async Task<DatabaseResponse?> Delete()
        //{
        //    try
        //    {
        //        var client = new DatabaseClient();
        //        var query = "DELETE FROM voice WHERE Name = @Name;";
        //        var parameters = new Dictionary<string, object?>
        //        {
        //            { "@Name", Name }
        //        };
        //        var response = await client.ExecuteQueryAsync(query, parameters);
        //        return response;
        //    }
        //    catch
        //    {
        //        return new DatabaseResponse() { Success = false, ErrorMessage = $"An error occurred while deleting the ensemble '{Name}' from the database." };

        //    }
        //}
        //public static async Task<List<Dictionary<string, object?>>> List()
        //{
        //    try
        //    {
        //        var client = new DatabaseClient();
        //        var query = "SELECT name, description from voice;";
        //        var response = await client.ExecuteQueryAsync(query, null);
        //        return (response != null && response.Success && response.Data != null) ? response.Data : [];
        //    }
        //    catch
        //    {
        //        return [];
        //    }
        //}
        //public static async Task<DatabaseResponse?> Get(string name)
        //{
        //    try
        //    {
        //        var client = new DatabaseClient();
        //        var query = "SELECT v.name as name, v.description as description, (SELECT GROUP_CONCAT(ev.ensemble_name SEPARATOR ',') AS ensembles FROM ensemble_voice ev WHERE v.name = ev.voice_name GROUP BY v.name) ensemble FROM voice v WHERE v.name = @Name;";
        //        var parameters = new Dictionary<string, object?>
        //        {
        //            { "@Name", name }
        //        };
        //        var response = await client.ExecuteQueryAsync(query, parameters);
        //        return response;
        //    }
        //    catch
        //    {
        //        return new DatabaseResponse() { Success = false, ErrorMessage = $"An error occurred while retrieving the ensemble '{name}' from the database." };
        //    }
        //}
    }
}
