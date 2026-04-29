using CMGDatabaseServer;
using CMGDBEditor.Types;
using CMGWpf.Services;
using System.Collections.ObjectModel;
namespace CMGDBEditor.Model
{
    public class Ensemble
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        // EF Core navigation property for many-to-many relationship
        public virtual ICollection<Voice> Voices { get; set; } = new List<Voice>();

        public Ensemble Clone() { return new Ensemble { Name = Name, Description = Description, Voices = new List<Voice>(Voices) }; }
        public static ObservableCollection<Error> Validate(Ensemble ensemble, string newName, ObservableCollection<Ensemble> allEnsembles)
        {
            ObservableCollection<Error> errors = new ObservableCollection<Error>();
            // check that the ensemble name is not blank and is unique
            if (newName.Trim(' ', '\t') == "")
            {
                errors.Add(new Error() { IsError = true, Message = "Ensemble name must not be blank" });
            }
            // find the ensemble object so it can be skipped
            int ensembleIndex = -1;
            for (int i = 0; i < allEnsembles.Count; i++)
            {
                if (allEnsembles[i].Name == ensemble.Name) { ensembleIndex = i; break; }
            }
            // see if there is another ensemble with the same name
            for (int i = 0; i < allEnsembles.Count; i++)
            {
                if (i == ensembleIndex) continue;
                if (newName == allEnsembles[i].Name)
                {
                    errors.Add(new Error()
                    {
                        IsError = true,
                        Message = $"Ensemble name '{newName}' must be unique"
                    });
                    break;
                }
            }
            return errors;
        }
        //public async Task<DatabaseResponse?> Add()
        //{
        //    try
        //    {
        //        // add the new ensemble to the DB
        //        var client = new DatabaseClient();
        //        var query = "INSERT INTO ensemble (Name, Description, Voices) VALUES (@Name, @Description);";
        //        var parameters = new Dictionary<string, object?>
        //        {
        //            { "@Name", Name },
        //            { "@Description", Description },
        //        };
        //        var response = await client.ExecuteQueryAsync(query, parameters);
        //        if (response == null || !response.Success) return response;

        //        // add the voices to the ensemble_voice table
        //        foreach (var voice in Voices)
        //        {
        //            var voiceQuery = "INSERT INTO ensemble_voice (ensemble_name, voice_name) VALUES (@EnsembleName, @VoiceName);";
        //            var voiceParameters = new Dictionary<string, object?>
        //            {
        //                { "@EnsembleName", Name },
        //                { "@VoiceName", voice.Name },
        //            };
        //            var voiceResponse = await client.ExecuteQueryAsync(voiceQuery, voiceParameters);
        //            if (voiceResponse == null || !voiceResponse.Success) return voiceResponse;
        //        }

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
        //        var query = "UPDATE ensemble SET (Name, Description, Voices) = (@NewName, @Description) WHERE Name = @Name;";
        //        var parameters = new Dictionary<string, object?>
        //        {
        //            { "@Name", Name },
        //            { "@NewName", newName },
        //            { "@Description", Description },
        //        };
        //        var response = await client.ExecuteQueryAsync(query, parameters);
        //        if (response == null || !response.Success) return response;

        //        // clear the existing voices from the ensemble_voice table
        //        var clearVoicesQuery = "DELETE FROM ensemble_voice WHERE ensemble_name = @EnsembleName;";
        //        var clearVoicesParameters = new Dictionary<string, object?>
        //        {
        //            { "@EnsembleName", Name }
        //        };
        //        var clearVoicesResponse = await client.ExecuteQueryAsync(clearVoicesQuery, clearVoicesParameters);
        //        if (clearVoicesResponse == null || !clearVoicesResponse.Success) return clearVoicesResponse;

        //        // add the voices to the ensemble_voice table
        //        foreach (var voice in Voices)
        //        {
        //            var voiceQuery = "INSERT INTO ensemble_voice (ensemble_name, voice_name) VALUES (@EnsembleName, @VoiceName);";
        //            var voiceParameters = new Dictionary<string, object?> {
        //                { "@EnsembleName", Name },
        //                { "@VoiceName", voice.Name },
        //            };
        //            var voiceResponse = await client.ExecuteQueryAsync(voiceQuery, voiceParameters);
        //            if (voiceResponse == null || !voiceResponse.Success) return voiceResponse;
        //        }

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
        //        var query = "DELETE FROM Ensembles WHERE Name = @Name;";
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
        //        var query = "SELECT name, description from ensemble;";
        //        var response = await client.ExecuteQueryAsync(query, null);
        //        return (response != null && response.Data != null) ? response.Data : new List<Dictionary<string, object?>>();
        //    }
        //    catch
        //    {
        //        return new List<Dictionary<string, object?>>();
        //    }
        //}
        //public static async Task<DatabaseResponse?> Get(string name)
        //{
        //    try
        //    {
        //        var client = new DatabaseClient();
        //        var query = "SELECT e.name as name, e.description as description, (SELECT GROUP_CONCAT(ev.voice_name SEPARATOR ',') AS voices FROM ensemble_voice ev WHERE e.name = ev.ensemble_name GROUP BY e.name) voices FROM ensemble e WHERE e.name = @Name;";
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
