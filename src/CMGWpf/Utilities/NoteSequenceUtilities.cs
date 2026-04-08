
using CMGWpf.Model;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using static CMGWpf.Model.Generators.StochasticTypes;
using static CMGWpf.Types.DBTypes;

namespace CMGWpf.Utilities
{
    public static class NoteSequenceUtilities
    {
        public static async Task<ObservableCollection<string>> GetNoteSequenceNamesAsync()
        {
            DbResult<DbNoteSequenceValidNamesType> dbResponse = await CMGDB.FetchAsync<DbNoteSequenceValidNamesType>($"note/valid", "GET").ConfigureAwait(false);

            if (dbResponse.IsSuccess && dbResponse.Value.value != null)
            {
                return [.. dbResponse.Value.value.Select(dbNoteSequence => dbNoteSequence.name)];
            }
            else
            {
                return [];
            }
        }
        public static async Task<Sequencer> GetNoteSequenceAsync(string name)
        {
            DbResult<DbNoteSequenceValueType> dbResponse = await CMGDB.FetchAsync<DbNoteSequenceValueType>($"note/{name}", "GET").ConfigureAwait(false);
            if (dbResponse.IsSuccess)
            {
                Sequencer sequencer = new()
                {
                    Name = dbResponse.Value.value.name,
                    Items = [..dbResponse.Value.value.items],
                };
                return sequencer;
            }
            else
            {
                return new Sequencer { Name = name, Items = [] };
            }

        }
    }
}
